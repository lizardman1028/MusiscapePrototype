"""
FastAPI backend for Unity desktop application.
CORS enabled for localhost. Health check, lyrics (Genius API), stem split (Lalal.ai).
Stateless; no local file storage or temp folders.
"""
import asyncio
import logging
from pathlib import Path

import httpx
from fastapi import FastAPI, File, HTTPException, UploadFile
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel

from config import settings
from services.lalal_service import (
    LalalServiceError,
    get_lalal_api_key,
    parse_stems,
    poll_task,
    start_multistem,
    upload_file,
)

# -----------------------------------------------------------------------------
# App lifecycle
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# Application
# -----------------------------------------------------------------------------

app = FastAPI(
    title="Musiscape Backend",
    description="Backend for Unity desktop music tool",
    version="0.1.0",
)

# CORS: allow Unity running on localhost (any port) to call the API
app.add_middleware(
    CORSMiddleware,
    allow_origin_regex=r"https?://(localhost|127\.0\.0\.1)(:[0-9]+)?",
    allow_credentials=True,
    allow_methods=["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    allow_headers=["*"],
)


# -----------------------------------------------------------------------------
# Health
# -----------------------------------------------------------------------------


@app.get("/health")
async def health():
    """Basic health check for load balancers and clients."""
    return {"status": "ok"}


# -----------------------------------------------------------------------------
# Config (no API keys in this file)
# -----------------------------------------------------------------------------

GENIUS_API_BASE = "https://api.genius.com"
PLACEHOLDER_LYRICS = "Lyrics could not be retrieved for this track."
PLACEHOLDER_MEANING = "No description available."

ALLOWED_AUDIO_EXTENSIONS = {".wav", ".mp3", ".flac", ".m4a", ".ogg"}
MAX_UPLOAD_BYTES = 100 * 1024 * 1024  # 100 MB

logger = logging.getLogger(__name__)


# -----------------------------------------------------------------------------
# Request/response models
# -----------------------------------------------------------------------------


class LyricsRequest(BaseModel):
    """Request body for POST /lyrics."""

    title: str
    artist: str


class LyricsResponse(BaseModel):
    """Response for POST /lyrics."""

    lyrics: str
    meaning: str


class SplitResponse(BaseModel):
    """Stem URLs from Lalal.ai multistem. Null if a requested stem is missing."""

    vocals: str | None = None
    bass: str | None = None
    drums: str | None = None
    guitar: str | None = None
    piano: str | None = None
    other: str | None = None


# -----------------------------------------------------------------------------
# Genius API helpers
# -----------------------------------------------------------------------------


async def _genius_search(token: str, query: str) -> dict | None:
    """Search Genius; returns JSON response or None on error."""
    async with httpx.AsyncClient(timeout=15.0) as client:
        r = await client.get(
            f"{GENIUS_API_BASE}/search",
            params={"q": query},
            headers={"Authorization": f"Bearer {token}"},
        )
        r.raise_for_status()
        return r.json()


async def _genius_song(token: str, song_id: int) -> dict | None:
    """Fetch song by ID; returns JSON response or None on error."""
    async with httpx.AsyncClient(timeout=15.0) as client:
        r = await client.get(
            f"{GENIUS_API_BASE}/songs/{song_id}",
            headers={"Authorization": f"Bearer {token}"},
        )
        r.raise_for_status()
        return r.json()


def _extract_meaning(song_payload: dict) -> str:
    """Try to get a description/meaning from Genius song response."""
    try:
        song = song_payload.get("response", {}).get("song", {})
        desc = song.get("description", {})
        if isinstance(desc, dict):
            return (desc.get("plain") or "").strip() or PLACEHOLDER_MEANING
        if isinstance(desc, str) and desc.strip():
            return desc.strip()
    except (AttributeError, TypeError, KeyError):
        pass
    return PLACEHOLDER_MEANING


# -----------------------------------------------------------------------------
# Lyrics
# -----------------------------------------------------------------------------


@app.post("/lyrics", response_model=LyricsResponse)
async def lyrics(request: LyricsRequest):
    """
    Search Genius for the song and return lyrics + meaning.
    Lyrics are placeholder when not directly available from the API.
    """
    query = f"{request.title} {request.artist}".strip()
    if not query:
        raise HTTPException(status_code=400, detail="title and artist cannot both be empty")

    token = settings.GENIUS_API_KEY
    if not token:
        return LyricsResponse(lyrics=PLACEHOLDER_LYRICS, meaning=PLACEHOLDER_MEANING)

    try:
        search_data = await _genius_search(token, query)
    except httpx.HTTPError:
        return LyricsResponse(lyrics=PLACEHOLDER_LYRICS, meaning=PLACEHOLDER_MEANING)

    try:
        hits = (search_data or {}).get("response", {}).get("hits") or []
        if not hits:
            return LyricsResponse(lyrics=PLACEHOLDER_LYRICS, meaning=PLACEHOLDER_MEANING)
        first = hits[0].get("result")
        if not first:
            return LyricsResponse(lyrics=PLACEHOLDER_LYRICS, meaning=PLACEHOLDER_MEANING)
        song_id = first.get("id")
        if not song_id:
            return LyricsResponse(lyrics=PLACEHOLDER_LYRICS, meaning=PLACEHOLDER_MEANING)
    except (AttributeError, TypeError, KeyError, IndexError):
        return LyricsResponse(lyrics=PLACEHOLDER_LYRICS, meaning=PLACEHOLDER_MEANING)

    try:
        song_data = await _genius_song(token, song_id)
        meaning = _extract_meaning(song_data) if song_data else PLACEHOLDER_MEANING
    except httpx.HTTPError:
        meaning = PLACEHOLDER_MEANING

    return LyricsResponse(lyrics=PLACEHOLDER_LYRICS, meaning=meaning)


# -----------------------------------------------------------------------------
# Split (Lalal.ai multistem)
# -----------------------------------------------------------------------------


def _run_split_flow(api_key: str, file_bytes: bytes, filename: str) -> dict:
    """Run upload -> start_multistem -> poll_task in executor (uses time.sleep). Returns check response."""
    source_id = upload_file(api_key, file_bytes, filename)
    task_id = start_multistem(api_key, source_id)
    return poll_task(api_key, task_id)


@app.post("/split", response_model=SplitResponse)
async def split(file: UploadFile = File(..., description="Audio file to separate (e.g. wav, mp3)")):
    """
    Upload audio to Lalal.ai, run multistem separation, poll until complete, return stem URLs.
    Stateless; no local storage. Requires LALAL_API_KEY (X-License-Key).
    """
    api_key = settings.LALAL_API_KEY or get_lalal_api_key()
    if not api_key:
        logger.error("LALAL_API_KEY is not set")
        raise HTTPException(status_code=500, detail="Stem separation is not configured (missing API key).")

    suffix = Path(file.filename or "audio").suffix.lower()
    if suffix not in ALLOWED_AUDIO_EXTENSIONS:
        raise HTTPException(
            status_code=400,
            detail=f"Unsupported format. Allowed: {', '.join(sorted(ALLOWED_AUDIO_EXTENSIONS))}",
        )

    # Read file into memory (no temp file)
    chunks = []
    size = 0
    while chunk := await file.read(64 * 1024):
        size += len(chunk)
        if size > MAX_UPLOAD_BYTES:
            raise HTTPException(
                status_code=400,
                detail=f"File too large. Max {MAX_UPLOAD_BYTES // (1024*1024)} MB.",
            )
        chunks.append(chunk)
    if size == 0:
        raise HTTPException(status_code=400, detail="Empty file.")
    file_bytes = b"".join(chunks)
    filename = file.filename or "audio" + suffix

    loop = asyncio.get_event_loop()
    try:
        check_response = await loop.run_in_executor(
            None,
            lambda: _run_split_flow(api_key, file_bytes, filename),
        )
    except LalalServiceError as e:
        logger.exception("Lalal split failed: %s", e)
        if "timed out" in str(e).lower():
            raise HTTPException(status_code=504, detail="Stem separation timed out.") from e
        raise HTTPException(status_code=500, detail="Stem separation service error.") from e

    stems = parse_stems(check_response)
    return SplitResponse(
        vocals=stems.get("vocals"),
        bass=stems.get("bass"),
        drums=stems.get("drums"),
        guitar=stems.get("guitar"),
        piano=stems.get("piano"),
        other=stems.get("other"),
    )

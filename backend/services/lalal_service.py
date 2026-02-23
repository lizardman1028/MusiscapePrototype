"""
Lalal.ai API client for multistem separation.
Uses LALAL_API_KEY as X-License-Key. No local file storage.
"""
import os
import time
from typing import Any

import requests

BASE_URL = "https://api.lalal.ai/api/v1"
UPLOAD_URL = f"{BASE_URL}/upload/"
SPLIT_MULTISTEM_URL = f"{BASE_URL}/split/multistem/"
CHECK_URL = f"{BASE_URL}/check/"

POLL_INTERVAL_SECONDS = 3
POLL_TIMEOUT_SECONDS = 120
REQUEST_TIMEOUT_SECONDS = 60

STEM_LABELS = ["vocals", "bass", "drums", "guitar", "piano", "other"]


class LalalServiceError(Exception):
    """Raised when Lalal.ai API returns an error or request fails."""
    pass


def _headers(api_key: str, json_content: bool = False) -> dict[str, str]:
    h = {"X-License-Key": api_key}
    if json_content:
        h["Content-Type"] = "application/json"
    return h


def upload_file(api_key: str, file_bytes: bytes, filename: str) -> str:
    """
    Upload audio to Lalal.ai. Returns source_id.
    POST upload/ with raw body and Content-Disposition.
    """
    headers = _headers(api_key)
    headers["Content-Disposition"] = f"attachment; filename={filename}"
    try:
        r = requests.post(
            UPLOAD_URL,
            headers=headers,
            data=file_bytes,
            timeout=REQUEST_TIMEOUT_SECONDS,
        )
        r.raise_for_status()
        data = r.json()
    except requests.RequestException as e:
        raise LalalServiceError(f"Upload failed: {e}") from e
    source_id = data.get("source_id")
    if not source_id:
        raise LalalServiceError("Upload response missing source_id")
    return str(source_id)


def start_multistem(api_key: str, source_id: str) -> str:
    """
    Start multistem split. Returns task_id.
    POST split/multistem/ with source_id and stems list.
    """
    payload = {
        "source_id": source_id,
        "stems": list(STEM_LABELS),
    }
    try:
        r = requests.post(
            SPLIT_MULTISTEM_URL,
            headers=_headers(api_key, json_content=True),
            json=payload,
            timeout=REQUEST_TIMEOUT_SECONDS,
        )
        r.raise_for_status()
        data = r.json()
    except requests.RequestException as e:
        raise LalalServiceError(f"Start multistem failed: {e}") from e
    task_id = data.get("task_id")
    if not task_id:
        raise LalalServiceError("Split response missing task_id")
    return str(task_id)


def poll_task(api_key: str, task_id: str) -> dict[str, Any]:
    """
    Poll check endpoint every 3 seconds until success or timeout (120s).
    Returns the final check response when status == "success".
    Raises LalalServiceError on status == "error" or timeout.
    """
    payload = {"task_ids": [task_id]}
    headers = _headers(api_key, json_content=True)
    deadline = time.monotonic() + POLL_TIMEOUT_SECONDS
    while time.monotonic() < deadline:
        try:
            r = requests.post(
                CHECK_URL,
                headers=headers,
                json=payload,
                timeout=REQUEST_TIMEOUT_SECONDS,
            )
            r.raise_for_status()
            data = r.json()
        except requests.RequestException as e:
            raise LalalServiceError(f"Check failed: {e}") from e
        # Response shape: tasks list with status and results
        tasks = data.get("tasks") or data.get("results") or []
        for task in tasks:
            if task.get("task_id") != task_id and task.get("id") != task_id:
                continue
            status = (task.get("status") or "").lower()
            if status == "error":
                raise LalalServiceError("Stem separation failed (status=error)")
            if status == "success":
                return data
        time.sleep(POLL_INTERVAL_SECONDS)
    raise LalalServiceError("Stem separation timed out")


def parse_stems(check_response: dict) -> dict[str, str | None]:
    """
    Extract stem URLs from check response.
    Each stem result: type=="stem", label, url.
    Returns dict with keys vocals, bass, drums, guitar, piano, other; value None if missing.
    """
    out: dict[str, str | None] = {k: None for k in STEM_LABELS}

    def collect_from(items: list) -> None:
        if not isinstance(items, list):
            return
        for item in items:
            if not isinstance(item, dict):
                continue
            if (item.get("type") or "").lower() != "stem":
                continue
            label = (item.get("label") or "").strip().lower()
            url = (item.get("url") or "").strip()
            if label in out and url:
                out[label] = url

    tasks = check_response.get("tasks") or check_response.get("results") or []
    for task in tasks:
        if not isinstance(task, dict):
            continue
        results = task.get("result") or task.get("results") or task.get("output") or []
        collect_from(results)
    # Top-level result list (e.g. single-task response)
    collect_from(check_response.get("result") or check_response.get("results") or [])
    return out


def get_lalal_api_key() -> str | None:
    """Read LALAL_API_KEY from environment. No default."""
    return (os.environ.get("LALAL_API_KEY") or "").strip() or None

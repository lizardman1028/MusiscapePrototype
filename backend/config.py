"""
Application configuration. Loads from environment via python-dotenv.
No API keys or secrets hardcoded; use .env or environment.
"""
from pathlib import Path

from dotenv import load_dotenv
import os

# Load .env from backend directory (or cwd); does not override existing env vars by default
load_dotenv()

_DEFAULT_BASE = Path(__file__).resolve().parent


class Settings:
    """
    Central settings. All paths are resolved and absolute.
    Missing env vars fall back to safe defaults (no API key, local temp paths).
    """

    def __init__(self) -> None:
        # Genius API bearer token. No default; set GENIUS_API_KEY in .env or environment.
        self.GENIUS_API_KEY: str | None = (
            os.environ.get("GENIUS_API_KEY")
            or os.environ.get("GENIUS_ACCESS_TOKEN")
            or os.environ.get("GENIUS_BEARER_TOKEN")
            or None
        )
        if self.GENIUS_API_KEY is not None and not self.GENIUS_API_KEY.strip():
            self.GENIUS_API_KEY = None

        # Lalal.ai API key for stem separation. No default; set LALAL_API_KEY in .env or environment.
        lalal = os.environ.get("LALAL_API_KEY")
        self.LALAL_API_KEY: str | None = (lalal and lalal.strip()) or None

        # Stem output directory (e.g. temp/stems). Resolved to absolute path.
        stem_raw = os.environ.get("STEM_STORAGE_PATH")
        if stem_raw and stem_raw.strip():
            self.STEM_STORAGE_PATH = Path(stem_raw.strip()).resolve()
        else:
            self.STEM_STORAGE_PATH = (_DEFAULT_BASE / "temp" / "stems").resolve()

        # Temporary upload directory. Resolved to absolute path.
        temp_raw = os.environ.get("TEMP_UPLOAD_PATH")
        if temp_raw and temp_raw.strip():
            self.TEMP_UPLOAD_PATH = Path(temp_raw.strip()).resolve()
        else:
            self.TEMP_UPLOAD_PATH = (_DEFAULT_BASE / "temp").resolve()


# Single global instance; import as: from config import settings
settings = Settings()

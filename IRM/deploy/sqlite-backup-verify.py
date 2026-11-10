"""Create and verify a consistent SQLite backup without exposing row data."""
from __future__ import annotations

import hashlib
import json
import os
import sqlite3
import sys


def main() -> int:
    if len(sys.argv) != 3:
        raise SystemExit("usage: sqlite-backup-verify.py SOURCE_DB BACKUP_DB")

    source_path, backup_path = map(os.path.abspath, sys.argv[1:])
    os.makedirs(os.path.dirname(backup_path), mode=0o700, exist_ok=True)

    with sqlite3.connect(source_path) as source, sqlite3.connect(backup_path) as backup:
        source.backup(backup)

    os.chmod(backup_path, 0o600)
    with sqlite3.connect(f"file:{backup_path}?mode=ro", uri=True) as verified:
        integrity = verified.execute("PRAGMA integrity_check").fetchone()[0]
        tables = [
            row[0]
            for row in verified.execute(
                "SELECT name FROM sqlite_master "
                "WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name"
            )
        ]
        row_counts = {
            table: verified.execute(
                'SELECT COUNT(*) FROM "' + table.replace('"', '""') + '"'
            ).fetchone()[0]
            for table in tables
        }

    digest = hashlib.sha256()
    with open(backup_path, "rb") as backup_file:
        for chunk in iter(lambda: backup_file.read(1024 * 1024), b""):
            digest.update(chunk)

    result = {
        "backup": backup_path,
        "sha256": digest.hexdigest(),
        "bytes": os.path.getsize(backup_path),
        "integrity": integrity,
        "table_count": len(tables),
        "total_rows": sum(row_counts.values()),
        "row_counts": row_counts,
    }
    print(json.dumps(result, ensure_ascii=False, sort_keys=True))
    return 0 if integrity == "ok" else 1


if __name__ == "__main__":
    raise SystemExit(main())

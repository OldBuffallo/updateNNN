"""Verify SSH authentication without printing credential material."""
from __future__ import annotations

from ssh_run import ssh_run


if __name__ == "__main__":
    raise SystemExit(ssh_run("echo CONNECTED && id && uname -a"))

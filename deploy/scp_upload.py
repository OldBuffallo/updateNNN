"""Upload a release artifact over SFTP without storing credentials in source."""
from __future__ import annotations

import os
import sys
import time

import paramiko


def required_env(name: str) -> str:
    value = os.environ.get(name)
    if not value:
        raise RuntimeError(f"Set {name} before running this helper")
    return value


def upload_file(local_path: str, remote_path: str) -> None:
    host = required_env("IRM_VPS_HOST")
    user = os.environ.get("IRM_VPS_USER", "ubuntu")
    key_path = os.environ.get("IRM_VPS_KEY_PATH")
    password = os.environ.get("IRM_VPS_PASSWORD")
    if not key_path and not password:
        raise RuntimeError("Set IRM_VPS_KEY_PATH or IRM_VPS_PASSWORD")

    file_size = os.path.getsize(local_path)
    print(f"Uploading {os.path.basename(local_path)} ({file_size / 1024 / 1024:.1f} MB)")
    client = paramiko.SSHClient()
    client.load_system_host_keys()
    client.set_missing_host_key_policy(paramiko.RejectPolicy())
    client.connect(host, username=user, key_filename=key_path, password=password, timeout=15)
    sftp = client.open_sftp()
    started = time.time()
    sftp.put(local_path, remote_path)
    elapsed = max(time.time() - started, 0.001)
    print(f"Upload complete in {elapsed:.1f}s ({file_size / elapsed / 1024 / 1024:.1f} MB/s)")
    sftp.close()
    client.close()


if __name__ == "__main__":
    source = sys.argv[1]
    destination = sys.argv[2] if len(sys.argv) > 2 else f"/home/{os.environ.get('IRM_VPS_USER', 'ubuntu')}/{os.path.basename(source)}"
    upload_file(source, destination)

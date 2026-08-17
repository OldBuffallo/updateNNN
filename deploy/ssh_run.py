"""Run a command over SSH without storing credentials in source."""
from __future__ import annotations

import os
import sys

import paramiko


def required_env(name: str) -> str:
    value = os.environ.get(name)
    if not value:
        raise RuntimeError(f"Set {name} before running this helper")
    return value


def ssh_run(command: str) -> int:
    host = required_env("IRM_VPS_HOST")
    user = os.environ.get("IRM_VPS_USER", "ubuntu")
    key_path = os.environ.get("IRM_VPS_KEY_PATH")
    password = os.environ.get("IRM_VPS_PASSWORD")
    if not key_path and not password:
        raise RuntimeError("Set IRM_VPS_KEY_PATH or IRM_VPS_PASSWORD")

    client = paramiko.SSHClient()
    client.load_system_host_keys()
    client.set_missing_host_key_policy(paramiko.RejectPolicy())
    client.connect(host, username=user, key_filename=key_path, password=password, timeout=15)
    _, stdout, stderr = client.exec_command(command, timeout=300)
    output = stdout.read().decode("utf-8", errors="replace")
    error = stderr.read().decode("utf-8", errors="replace")
    exit_code = stdout.channel.recv_exit_status()
    client.close()
    if output:
        print(output, end="")
    if error:
        print(error, end="", file=sys.stderr)
    return exit_code


if __name__ == "__main__":
    raise SystemExit(ssh_run(" ".join(sys.argv[1:]) or "echo CONNECTED && uname -a"))

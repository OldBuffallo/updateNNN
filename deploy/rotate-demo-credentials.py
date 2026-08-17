"""Rotate every SQLite demo account password and verify the admin login."""
from __future__ import annotations

import base64
import http.cookiejar
import json
import os
import secrets
import string
import sys
import urllib.parse
import urllib.request

import paramiko


DATABASE_PATH = "/var/lib/docker/volumes/openship-irm-v010-irm-v010-data/_data/IRM-v0.1.0-demo.db"


def required_env(name: str) -> str:
    value = os.environ.get(name)
    if not value:
        raise RuntimeError(f"Set {name} before running this helper")
    return value


def new_password() -> str:
    alphabet = string.ascii_letters + string.digits + "-_!@#"
    return "".join(secrets.choice(alphabet) for _ in range(24))


def post_login(base_url: str, username: str, password: str) -> str:
    jar = http.cookiejar.CookieJar()
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))
    request = urllib.request.Request(
        base_url.rstrip("/") + "/auth/login",
        data=urllib.parse.urlencode({"username": username, "password": password}).encode(),
        method="POST",
    )
    with opener.open(request, timeout=30) as response:
        return response.geturl()


def main() -> int:
    host = required_env("IRM_VPS_HOST")
    user = os.environ.get("IRM_VPS_USER", "root")
    key_path = required_env("IRM_VPS_KEY_PATH")
    base_url = required_env("IRM_DEMO_URL")
    old_admin_password = required_env("IRM_DEMO_OLD_ADMIN_PASSWORD")

    client = paramiko.SSHClient()
    client.load_system_host_keys()
    client.set_missing_host_key_policy(paramiko.RejectPolicy())
    client.connect(host, username=user, key_filename=key_path, timeout=15)

    _, stdout, stderr = client.exec_command(
        "python3 -c \"import sqlite3; "
        f"db=sqlite3.connect('{DATABASE_PATH}'); "
        "print('\\n'.join(f'{row[0]}\\t{row[1]}' for row in "
        "db.execute('SELECT IDUser, Username FROM Accounts WHERE Delete_flag = 0 ORDER BY IDUser'))); "
        "db.close()\"",
        timeout=30,
    )
    account_output = stdout.read().decode().strip()
    account_error = stderr.read().decode().strip()
    if stdout.channel.recv_exit_status() != 0:
        raise RuntimeError(account_error or "Could not read demo accounts")

    accounts = []
    for line in account_output.splitlines():
        account_id, username = line.split("\t", 1)
        accounts.append({"id": int(account_id), "username": username, "password": new_password()})
    admin = next((account for account in accounts if account["username"] == "admin"), None)
    if admin is None:
        raise RuntimeError("Demo admin account was not found")

    payload = base64.b64encode(json.dumps(accounts).encode()).decode()
    remote_script = f"""
import base64, json, sqlite3
accounts = json.loads(base64.b64decode({payload!r}))
db = sqlite3.connect({DATABASE_PATH!r}, timeout=30)
try:
    db.execute('BEGIN IMMEDIATE')
    for account in accounts:
        db.execute('UPDATE Accounts SET Password = ? WHERE IDUser = ?', (account['password'], account['id']))
    db.execute('DELETE FROM WebCredentials')
    db.commit()
finally:
    db.close()
"""
    stdin, stdout, stderr = client.exec_command("python3 -", timeout=30)
    stdin.write(remote_script)
    stdin.channel.shutdown_write()
    remote_error = stderr.read().decode().strip()
    exit_code = stdout.channel.recv_exit_status()
    client.close()
    if exit_code != 0:
        raise RuntimeError(remote_error or "Credential rotation failed")

    old_final_url = post_login(base_url, "admin", old_admin_password)
    new_final_url = post_login(base_url, "admin", admin["password"])
    result = {
        "rotated_accounts": len(accounts),
        "old_password_rejected": "loginError=1" in old_final_url,
        "new_login_succeeded": urllib.parse.urlparse(new_final_url).path == "/",
        "admin_username": admin["username"],
        "admin_password": admin["password"],
    }
    print(json.dumps(result))
    return 0 if result["old_password_rejected"] and result["new_login_succeeded"] else 1


if __name__ == "__main__":
    raise SystemExit(main())

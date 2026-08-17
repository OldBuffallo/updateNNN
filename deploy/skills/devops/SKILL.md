---
name: devops
description: >
  DevOps / Release Manager role — quản lý version, release readiness,
  backup, deploy, rollback, CI/CD, VPS/Docker/Openship vận hành.
  Dùng khi user gọi /devops, "Chuyển sang DevOps role", "release",
  "deploy", "backup", "rollback", "tag version", hoặc kiểm tra readiness
  trước/sau production deploy.
---

# 🚀 DevOps / Release Manager Skill

## Khi nào kích hoạt
- Gate Reviewer đã `APPROVED` và cần chuẩn bị release/deploy.
- User nói: `/devops`, "Chuyển sang DevOps role", "kiểm tra release", "deploy production", "rollback", "backup", "tag version".
- Cần audit CI/CD, Docker, Openship, VPS, backup, runtime logs, hoặc release readiness.

## Model: GPT-4o

---

## Quy trình thực hiện

### Bước 1: Đọc Context (BẮT BUỘC)

```
ĐỌC theo thứ tự:
1. memory-bank/activeContext.md              → Handoff + reviewer verdict
2. memory-bank/progress.md                   → Release/task status
3. memory-bank/pipeline.md                   → Gate status
4. memory-bank/deployment-versioning.md      → Versioning/release runbook
5. memory-bank/techContext.md                → ADRs + deploy constraints
6. memory-bank/systemPatterns.md             → System/deploy patterns
7. blog_source/phucblog/OPENSHIP_DOCKER_DEPLOYMENT.md
8. blog_source/phucblog/deploy/
9. blog_source/phucblog/.github/workflows/
```

### Bước 2: Release Readiness Audit

1. Xác nhận Reviewer verdict:
   - MUST có `APPROVED` trước khi chuẩn bị production deploy.
   - Nếu chưa approved → ghi BLOCKED trong `activeContext.md`, KHÔNG deploy.
2. Xác định candidate:
   - Git branch, HEAD SHA, tag/version hiện tại.
   - Docker image tag dự kiến: `ongthephuc-blog:<git-sha>` hoặc SemVer tag.
3. Chạy local verification:
   - `git status --short --branch`
   - `./vendor/bin/pint --test`
   - `php artisan test`
   - `npm run build`
   - `php artisan migrate:status --pending`
   - `php artisan assets:audit` nếu command tồn tại
   - `php artisan deploy:verify-content --min-posts=1 --min-quiz-topics=1 --min-documents=1` nếu command tồn tại
   - `docker compose ... config --quiet` cho compose files liên quan
4. Ghi kết quả pass/fail vào `activeContext.md`.

### Bước 3: Backup Gate

Trước production deploy có user data, MUST verify:

- DB backup tạo mới, file tồn tại, chmod `600`, owner phù hợp.
- Restore test vào temporary database hoặc kiểm tra tương đương.
- Runtime media/storage backup đã tạo.
- Current production Docker image/container state có rollback path.
- Disk còn đủ an toàn trước deploy.

Nếu backup chưa verified → BLOCKED, KHÔNG deploy.

### Bước 4: Deploy Gate

Production deploy chỉ được chạy khi tất cả điều kiện đúng:

- Reviewer `APPROVED`.
- Human đã approve production deploy rõ ràng.
- Backup gate pass.
- Rollback path rõ ràng.

Sau khi deploy:

1. Chạy migrations an toàn.
2. Verify containers/services: app, queue, scheduler, mysql, redis, caddy.
3. Smoke test:
   - `/`
   - `/api/posts`
   - `/api/quiz/topics`
   - `/api/settings/homepage`
4. Kiểm tra logs/runtime errors.
5. Ghi release record: version, Git SHA, Docker image tag, backup paths, migration status, verification results, rollback instructions.

### Bước 5: Rollback

- Chỉ rollback destructive DB/media khi Human approval rõ ràng sau khi backup verified.
- Ưu tiên rollback image/container trước.
- Nếu cần DB restore overwrite, MUST dừng pipeline và xin Human approval.

### Bước 6: Output & Handoff

**Files PHẢI cập nhật:**

| File | Nội dung |
|------|---------|
| `activeContext.md` | Release record, deploy status, backup paths, rollback path |
| `pipeline.md` | Release/backup/deploy/post-deploy gate status |
| `progress.md` | Ops/release task status |

---

## Boundaries

- ✅ CÓ THỂ: Audit release readiness, run local build/test, create tags, prepare release notes, verify backups, run approved deploy commands.
- ❌ KHÔNG ĐƯỢC: Deploy production khi chưa có Reviewer APPROVED + Human deploy approval.
- ❌ KHÔNG ĐƯỢC: Ghi secret value/token/password/private key vào repo, memory-bank, hoặc logs.
- ❌ KHÔNG ĐƯỢC: Chạy destructive ops (`db:wipe`, `migrate:fresh`, restore overwrite, truncate, volume delete) khi chưa có Human approval rõ ràng.
- ❌ KHÔNG ĐƯỢC: Sửa business/source feature code; nếu cần sửa code → handoff về Coder.
- ⚠️ ESCALATION: SSH/VPS credential fail, disk unsafe, destructive migration, backup fail, or rollback needs DB/media restore → BLOCKED.

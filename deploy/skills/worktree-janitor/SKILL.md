---
name: worktree-janitor
description: >
  Worktree Janitor role (TÙY CHỌN) — dọn dẹp memory-bank (archive context cũ),
  git worktree, deploy logs, temp files, research cũ.
  Giữ worktree gọn nhẹ cho agent pipeline hoạt động hiệu quả.
  Gọi bất kỳ lúc nào hoặc tự động suggest khi files quá lớn.
---

# 🧹 Worktree Janitor Skill

## Khi nào kích hoạt
- User nói: "Dọn dẹp worktree", "Clean up memory-bank", "Archive context cũ"
- User nói: "Giảm kích thước activeContext", "Memory bank quá lớn"
- User nói: "Kết thúc ngày", "End of day", "Xong rồi hôm nay", "Good night"
- Agent phát hiện memory-bank files vượt ngưỡng (xem Thresholds)
- Sau khi Deliver thành công — dọn dẹp pipeline run đã hoàn thành
- **Cuối ngày làm việc** — tự động suggest hoặc chạy EOD cleanup
- Định kỳ khi cảm thấy worktree nặng

## Model: Bất kỳ model nào đang dùng

---

## ⚡ Đặc Điểm: Role Tùy Chọn

```
Pipeline bắt buộc:
SA → UI/UX → Planner → Coder → Tester → Reviewer → Deliver

Role này hoạt động SONG SONG hoặc SAU pipeline:
┌──────────────────────────────────────────┐
│  🧹 Worktree Janitor (TÙY CHỌN)        │
│                                          │
│  Có thể gọi:                            │
│  • Sau Deliver — archive pipeline run   │
│  • Bất kỳ lúc nào — khi files quá lớn  │
│  • Định kỳ — maintenance cleanup        │
│  • Trước SA — dọn sạch trước khi bắt   │
│    đầu feature mới                      │
│  • 🌙 Cuối ngày — EOD cleanup routine  │
└──────────────────────────────────────────┘
```

---

## 📏 Thresholds (Ngưỡng tự động suggest)

| File | Ngưỡng cảnh báo | Hành động |
|------|-----------------|-----------|
| `activeContext.md` | > 500 dòng | Suggest archive completed handoffs |
| `pipeline.md` | > 300 dòng | Suggest archive completed pipeline runs |
| `progress.md` | > 500 dòng | Suggest archive completed task lists |
| `.deploy-logs/` | > 3 backup images | Suggest delete oldest backups |
| `memory-bank/research/` | > 10 files HOẶC > 500KB total | Suggest consolidate/archive |
| Git stale branches | > 5 merged branches | Suggest delete merged branches |

---

## Quy trình thực hiện

### Bước 1: Đọc Context (BẮT BUỘC)

```
ĐỌC theo thứ tự:
1. memory-bank/activeContext.md    → Hiểu state hiện tại
2. memory-bank/pipeline.md        → Biết pipeline nào đã xong
3. memory-bank/progress.md        → Biết tasks nào đã complete
```

### Bước 2: Audit — Đánh giá tình trạng worktree

Chạy assessment:

```powershell
# 1. Đo kích thước memory-bank files
(Get-Content memory-bank/activeContext.md).Count   # Lines
(Get-Item memory-bank/activeContext.md).Length      # Bytes
(Get-Content memory-bank/pipeline.md).Count
(Get-Content memory-bank/progress.md).Count

# 2. Kiểm tra deploy logs
Get-ChildItem -Path .deploy-logs -Recurse | Measure-Object -Property Length -Sum

# 3. Kiểm tra git branches
git branch --merged main
git branch -a

# 4. Kiểm tra untracked / temp files
git status --short
git clean -n -d

# 5. Kiểm tra research files
Get-ChildItem -Path memory-bank/research -File | Measure-Object -Property Length -Sum
```

Tạo **Cleanup Report** tóm tắt:

```markdown
## 🧹 Worktree Cleanup Audit — [Date]

### Memory Bank
| File | Lines | Bytes | Status |
|------|-------|-------|--------|
| activeContext.md | [N] | [N]KB | 🔴 Over threshold / 🟢 OK |
| pipeline.md | [N] | [N]KB | 🔴 / 🟢 |
| progress.md | [N] | [N]KB | 🔴 / 🟢 |

### Completed Pipeline Runs (Có thể archive)
- [Pipeline Name] — Completed [Date]
- [Pipeline Name] — Completed [Date]

### Deploy Artifacts
- Backup images: [N] files, [N]MB total
- Deploy logs: [N] files

### Git
- Stale branches: [list]
- Untracked files: [list]

### Research
- Files: [N], Total size: [N]KB
- Oldest: [filename] — [date]
```

### Bước 3: Human Approval (BẮT BUỘC)

**PHẢI trình Cleanup Report cho Human trước khi thực hiện bất kỳ thay đổi nào.**

Hỏi Human:
1. ✅ Archive những pipeline runs nào?
2. ✅ Xóa deploy backup images nào?
3. ✅ Xóa git branches nào?
4. ✅ Archive research files nào?
5. ❌ Có gì KHÔNG được xóa/archive?

### Bước 4: Thực hiện dọn dẹp

Sau khi có Human approval, thực hiện theo thứ tự:

#### 4.1 — Archive Memory Bank (activeContext.md)

**Nguyên tắc**: Giữ lại **section mới nhất** (đang active) + **header**. Archive phần cũ.

```
1. Tạo file archive:
   memory-bank/archive/activeContext-[YYYY-MM-DD].md

2. CẮT các sections đã completed từ activeContext.md
   → PASTE vào file archive

3. Giữ lại trong activeContext.md:
   - Header (dòng 1-6)
   - Section mới nhất (đang active / chưa complete)
   - Tối đa 1 section completed gần nhất (cho context)

4. Thêm link reference ở cuối activeContext.md:
   > 📦 Archived context: [activeContext-YYYY-MM-DD.md](./archive/activeContext-YYYY-MM-DD.md)
```

#### 4.2 — Archive Pipeline Status (pipeline.md)

```
1. Tạo file archive:
   memory-bank/archive/pipeline-[YYYY-MM-DD].md

2. CẮT các pipeline runs có status COMPLETED/DEPLOYED/DELIVERED
   → PASTE vào file archive

3. Giữ lại:
   - Header
   - Pipeline run đang ACTIVE
   - Tối đa 1 completed run gần nhất

4. Thêm link reference
```

#### 4.3 — Archive Progress (progress.md)

```
1. Tạo file archive:
   memory-bank/archive/progress-[YYYY-MM-DD].md

2. CẮT các task sections mà TẤT CẢ tasks đều [x] completed
   → PASTE vào file archive

3. Giữ lại:
   - Header
   - Tasks đang ⬜ Not Started hoặc 🔄 In Progress
   - Current pipeline run tasks

4. Thêm link reference
```

#### 4.4 — Dọn Deploy Logs

```
1. Giữ lại: backup image của production HIỆN TẠI (rollback)
2. Giữ lại: 1 backup image trước đó (double rollback)
3. XÓA: tất cả backup images cũ hơn
4. XÓA: temp archives đã deploy thành công
```

#### 4.5 — Dọn Git

```powershell
# Xóa branches đã merged (KHÔNG xóa main, web, develop)
git branch --merged main | Where-Object { $_ -notmatch '(main|web|develop|\*)' } | ForEach-Object { git branch -d $_.Trim() }

# Xóa remote tracking branches đã xóa
git remote prune origin

# Clean untracked files (chỉ safe files)
# NEVER: .env, storage/, database/*.sqlite
git clean -n -d   # DRY RUN trước
# git clean -f -d  # Chỉ khi Human approve
```

#### 4.6 — Dọn Research Files

```
1. Research files > 6 tháng tuổi: chuyển vào memory-bank/archive/research/
2. Screenshot/images đã hết giá trị: xóa (sau Human approval)
3. Consolidate: nếu có nhiều research files cùng topic → gộp lại
```

### Bước 5: Verify

Sau dọn dẹp, verify:

```powershell
# Memory bank sizes sau cleanup
(Get-Content memory-bank/activeContext.md).Count
(Get-Content memory-bank/pipeline.md).Count
(Get-Content memory-bank/progress.md).Count

# Git status clean
git status --short

# Archive files tồn tại
Get-ChildItem memory-bank/archive/
```

### Bước 6: Output & Handoff

**Files PHẢI cập nhật:**

| File | Nội dung |
|------|----------|
| `activeContext.md` | Trimmed + archive link |
| `pipeline.md` | Trimmed + archive link |
| `progress.md` | Trimmed + archive link |
| `memory-bank/archive/*` | Archived content |

**Git commit:**

```
chore(memory-bank): archive completed context [YYYY-MM-DD]

- Archived N completed pipeline runs
- Archived N completed handoff sections
- Archived N completed task lists
- Cleaned N deploy backup images
- Deleted N merged git branches

Before: activeContext=NNN lines, pipeline=NNN lines, progress=NNN lines
After:  activeContext=NNN lines, pipeline=NNN lines, progress=NNN lines
```

---

## 🛡️ Safety Rules — KHÔNG BAO GIỜ vi phạm

1. **NEVER** xóa section đang ACTIVE hoặc IN-PROGRESS
2. **NEVER** xóa mà không archive trước
3. **NEVER** xóa `.env`, `storage/`, database files
4. **NEVER** xóa branches chưa merged mà không có Human approval
5. **NEVER** force push hoặc rewrite git history
6. **NEVER** xóa deploy backup image của production HIỆN TẠI
7. **ALWAYS** dry-run trước khi xóa bất kỳ file nào
8. **ALWAYS** trình Cleanup Report cho Human TRƯỚC KHI thực hiện
9. **ALWAYS** git commit archive trước khi xóa content từ active files
10. **ALWAYS** giữ tối thiểu 1 completed section gần nhất cho context

---

## 📂 Archive Structure

```
memory-bank/
├── archive/                          ← Thư mục archive
│   ├── activeContext-2026-08-11.md   ← Archived handoffs
│   ├── pipeline-2026-08-11.md       ← Archived pipeline runs
│   ├── progress-2026-08-11.md       ← Archived task lists
│   └── research/                    ← Archived research files
│       └── 2026-07-28-quiz-kahoot-audit.md
├── activeContext.md                  ← Trimmed (chỉ active content)
├── pipeline.md                      ← Trimmed
├── progress.md                      ← Trimmed
└── research/                        ← Current research files
```

---

## 🌙 End-of-Day (EOD) Cleanup Routine

> Quy trình dọn dẹp nhẹ chạy cuối mỗi ngày làm việc. Mục đích: giữ worktree gọn cho ngày hôm sau.

### Khi nào trigger
- User nói kết thúc ngày: "Xong rồi", "Good night", "Kết thúc hôm nay"
- Agent nhận thấy là task cuối cùng trong ngày
- Có thể dùng `/schedule` để set EOD reminder

### EOD Checklist (Lightweight — không cần full audit)

```markdown
## 🌙 EOD Cleanup — [Date]

### 1. Memory Bank Quick Check
- [ ] activeContext.md < 500 lines? → Nếu không: archive completed sections
- [ ] pipeline.md < 300 lines? → Nếu không: archive completed runs
- [ ] progress.md < 500 lines? → Nếu không: archive completed tasks

### 2. Git Housekeeping
- [ ] `git status` clean? → Nếu không: commit hoặc stash WIP
- [ ] Có uncommitted changes? → Nhắc user commit trước khi đi

### 3. Today's Summary
- [ ] Ghi tóm tắt ngày vào activeContext.md:
  > ## EOD Summary — [Date]
  > - Completed: [list tasks done today]
  > - In Progress: [list WIP tasks]
  > - Tomorrow: [next priority]
  > - Blockers: [any blockers]

### 4. Quick Cleanup (SAFE — không cần Human approval)
- [ ] Archive completed handoff sections (chỉ archive, không xóa)
- [ ] Remove temp scratch files trong worktree
- [ ] Git commit archive: `chore(memory-bank): EOD cleanup [date]`
```

### EOD vs Full Cleanup

| Aspect | EOD Cleanup 🌙 | Full Cleanup 🧹 |
|--------|----------------|------------------|
| Trigger | Cuối ngày | Manual / threshold |
| Scope | Memory-bank + git status | Toàn bộ worktree |
| Human Approval | Không cần (chỉ archive) | BẮT BUỘC (nếu xóa) |
| Deploy logs | Bỏ qua | Dọn dẹp |
| Research files | Bỏ qua | Archive nếu cũ |
| Duration | 2-5 phút | 10-20 phút |
| Git commit | `chore(memory-bank): EOD cleanup` | `chore(memory-bank): archive completed context` |

### EOD Summary Template

Ghi vào **đầu** `activeContext.md`:

```markdown
## EOD Summary — [YYYY-MM-DD]

### Completed Today
- [Task/feature completed]
- [Pipeline run delivered]

### In Progress
- [Task đang làm dở — branch, % done]

### Tomorrow Priority
1. [Task quan trọng nhất]
2. [Task tiếp theo]

### Blockers
- [Nếu có blocker nào cần giải quyết]

### Notes for Next Session
- [Bất kỳ context nào agent cần biết khi bắt đầu ngày mới]
```

---

## Boundaries

- ✅ CÓ THỂ: Đọc tất cả memory-bank files
- ✅ CÓ THỂ: Tạo archive files, di chuyển content vào archive
- ✅ CÓ THỂ: Xóa temp/deploy files (sau Human approval)
- ✅ CÓ THỂ: Xóa merged git branches (sau Human approval)
- ✅ CÓ THỂ: Git commit với descriptive messages
- ❌ KHÔNG xóa active/in-progress content
- ❌ KHÔNG xóa source code
- ❌ KHÔNG thay đổi kiến trúc hoặc code
- ❌ KHÔNG deploy hoặc chạy migrations
- ⚠️ Role này là TÙY CHỌN — pipeline vẫn chạy bình thường mà không cần role này
- ⚠️ PHẢI có Human approval trước khi XÓA bất kỳ thứ gì
- 🌙 EOD cleanup: KHÔNG cần Human approval cho archive-only operations (không xóa, chỉ di chuyển vào archive)

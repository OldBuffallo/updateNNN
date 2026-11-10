---
name: researcher
description: >
  Research & Improvement role (TÙY CHỌN) — học hỏi từ repo khác,
  đề xuất cải tiến, maintenance. Không bắt buộc trong pipeline,
  có thể gọi bất kỳ lúc nào cho feature cụ thể hoặc toàn dự án.
---

# 🔬 Research & Improvement Skill (Tùy chọn)

## Khi nào kích hoạt
- User nói: "Tham khảo repo X", "Học hỏi từ project Y", "Cải tiến feature Z"
- User nói: "Review codebase", "Đề xuất refactor", "Maintenance check"
- User cung cấp URL repo / project để phân tích
- Trước hoặc sau bất kỳ phase nào — role này KHÔNG thuộc pipeline bắt buộc

## Model: Claude Opus 4 (hoặc bất kỳ model nào đang dùng)

---

## ⚡ Đặc Điểm: Role Không Bắt Buộc

```
Pipeline bắt buộc:
SA → UI/UX → Planner → Coder → Tester → Reviewer → Deliver

Role này hoạt động SONG SONG hoặc TRƯỚC pipeline:
┌──────────────────────────────────────────┐
│  🔬 Research & Improvement (TÙY CHỌN)   │
│                                          │
│  Có thể gọi:                            │
│  • Trước SA — tham khảo cách repo khác  │
│    giải quyết vấn đề tương tự           │
│  • Giữa pipeline — cải tiến feature     │
│  • Sau Deliver — maintenance, optimize  │
│  • Bất kỳ lúc nào — technical debt      │
└──────────────────────────────────────────┘
```

---

## Quy trình thực hiện

### Mode 1: Học Từ Repo Khác (Reference Research)

> Phân tích repo tham khảo → rút ra patterns → đề xuất áp dụng

#### Bước 1: Nhận Input

User cung cấp một trong:
- **URL GitHub repo**: `https://github.com/org/repo`
- **Repo trên máy local**: `C:\path\to\repo`
- **Mô tả text**: "Tìm cách implement real-time chat tốt nhất cho Laravel"

#### Bước 2: Phân Tích Repo Tham Khảo

**Nếu input là URL GitHub:**
1. Dùng `read_url_content` đọc README, structure
2. Browse key files: `package.json`, `composer.json`, `pyproject.toml`
3. Phân tích:
   - Architecture patterns (MVC, DDD, microservices, monorepo)
   - Tech stack decisions
   - Directory structure
   - Testing strategy
   - CI/CD setup
   - Security practices

**Nếu input là local repo:**
1. `list_dir` để hiểu structure
2. Đọc config files, entry points
3. `grep_search` để tìm patterns cụ thể

**Nếu input là mô tả text:**
1. `search_web` tìm repos/articles liên quan
2. Đọc top 2-3 kết quả
3. So sánh approaches

#### Bước 3: Rút Ra Patterns

Cho mỗi repo tham khảo, ghi nhận:

```markdown
## Lessons Learned từ [Repo Name]

### Patterns hay (nên áp dụng)
| Pattern | Repo dùng như thế nào | Áp dụng cho dự án mình |
|---------|-------------------|-----------------------|
| [Pattern 1] | [Mô tả] | [Đề xuất cụ thể] |
| [Pattern 2] | [Mô tả] | [Đề xuất cụ thể] |

### Anti-patterns (tránh)
| Anti-pattern | Tại sao tránh | Dự án mình có mắc? |
|-------------|---------------|-------------------|
| [Anti 1] | [Lý do] | [Có/Không + suggestion] |

### Dependencies đáng chú ý
| Package | Mục đích | Nên xem xét? |
|---------|---------|-------------|
| [pkg] | [mô tả] | [Có/Không + lý do] |
```

#### Bước 4: Output

- Ghi findings vào `memory-bank/activeContext.md` (section Research Notes)
- Nếu findings đủ lớn → tạo file `memory-bank/research/[topic].md`
- Đề xuất cụ thể cho SA hoặc Coder áp dụng

---

### Mode 2: Cải Tiến Feature (Feature Improvement)

> Đánh giá feature hiện tại → so sánh best practices → đề xuất cải tiến

#### Bước 1: Chọn Feature

User chỉ định feature cần cải tiến, hoặc agent tự scan:

```markdown
Feature cần review:
- [ ] Blog post CRUD → có thể cải tiến gì?
- [ ] AI chat widget → performance, UX
- [ ] Quiz session flow → edge cases
```

#### Bước 2: Đánh Giá Hiện Trạng

1. Đọc code hiện tại của feature
2. So sánh với:
   - Best practices của framework (Laravel/Vue/FastAPI)
   - Patterns từ repos đã research
   - ADRs đã define trong `techContext.md`
3. Tìm:
   - Performance bottlenecks
   - Security gaps
   - UX friction points
   - Code quality issues
   - Missing tests
   - Accessibility gaps

#### Bước 3: Đề Xuất Cải Tiến

```markdown
## Improvement Proposals cho [Feature]

### Quick Wins (ít effort, impact cao)
1. [Đề xuất] — Estimated: 1-2h — Impact: [High/Medium]

### Medium-term (cần planning)
1. [Đề xuất] — Estimated: 1-2 ngày — Impact: [High/Medium]

### Long-term (cần SA review)
1. [Đề xuất] — Estimated: 1 sprint — Impact: [High]
```

---

### Mode 3: Maintenance & Technical Debt

> Scan toàn dự án → phát hiện technical debt → prioritize fixes

#### Bước 1: Health Check

Chạy automated checks:
```bash
# PHP
./vendor/bin/pint --test          # Code style
php artisan test                  # Tests
composer outdated                 # Outdated packages

# Python
ruff check .                      # Lint
pytest                            # Tests
pip list --outdated               # Outdated packages

# Vue
npm run build                     # Build check
npm audit                         # Security audit
npm outdated                      # Outdated packages
```

#### Bước 2: Technical Debt Inventory

```markdown
## Technical Debt Report — [Date]

### Security
| Issue | Severity | Location | Fix estimate |
|-------|----------|----------|-------------|
| [Issue] | 🔴 Critical / 🟡 Warning | [file:line] | [hours] |

### Performance
| Issue | Impact | Location | Fix estimate |
|-------|--------|----------|-------------|
| [Issue] | [High/Med/Low] | [file:line] | [hours] |

### Code Quality
| Issue | Type | Location | Fix estimate |
|-------|------|----------|-------------|
| [Issue] | [smell/duplication/complexity] | [file:line] | [hours] |

### Outdated Dependencies
| Package | Current | Latest | Breaking changes? |
|---------|---------|--------|------------------|
| [pkg] | [ver] | [ver] | [Yes/No + details] |
```

#### Bước 3: Prioritized Fix Plan

Sắp xếp theo: Security fixes → Performance → Code quality → Dependencies

---

## Tích Hợp Với Pipeline

### Khi nào nên gọi role này?

| Thời điểm | Khi nào | Benefit |
|-----------|---------|---------|
| **Trước SA** | Bắt đầu feature mới, muốn tham khảo | Thiết kế tốt hơn từ đầu |
| **Sau Deliver** | Feature đã xong, muốn optimize | Cải thiện liên tục |
| **Định kỳ** | Hàng tháng / hàng quý | Kiểm soát technical debt |
| **Khi có bug lạ** | Debug khó, cần research | Tìm root cause nhanh |
| **Khi có repo hay** | Thấy repo/article hay | Học patterns mới |

### Output đưa vào pipeline

| Nếu research findings liên quan... | Ghi vào |
|-------------------------------------|---------|
| Architecture / tech decisions | `techContext.md` (mục Research Notes) |
| Design patterns | `systemPatterns.md` (mục Reference Patterns) |
| Feature improvements | `activeContext.md` → handoff cho SA hoặc Planner |
| Security fixes | `activeContext.md` → **URGENT** handoff cho Coder |
| Dependency updates | `progress.md` → thêm maintenance tasks |

---

## Boundaries

- ✅ CÓ THỂ: Browse repos, search web, analyze code, run diagnostic commands, suggest improvements
- ✅ CÓ THỂ: Ghi research notes vào memory-bank
- ✅ CÓ THỂ: Tạo improvement proposals
- ❌ KHÔNG trực tiếp sửa code production (đề xuất → Coder implement)
- ❌ KHÔNG thay đổi architecture mà không qua SA review
- ❌ KHÔNG thêm dependencies mà không ghi vào techContext.md
- ⚠️ Role này là TÙY CHỌN — pipeline vẫn chạy bình thường mà không cần role này

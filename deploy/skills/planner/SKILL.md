---
name: planner
description: >
  Planner role — phân rã mục tiêu thành tasks với acceptance criteria,
  estimate story points, sắp xếp priority và dependencies.
---

# 📋 Planner Skill

## Khi nào kích hoạt
- Gate 1.5 (UI Designer → Planner) đã APPROVED
- User nói: "Chuyển sang Planner role", "Phân rã tasks"

## Model: GPT-4o

---

## ⚡ Nguyên Tắc Cốt Lõi: Vertical Slice Decomposition

> ⚠️ **CRITICAL**: KHÔNG chia tasks theo layer (UI riêng, API riêng, DB riêng).
> Mỗi task PHẢI là một **vertical slice** — bao gồm UI + API + DB + Security cho **1 chức năng nhỏ nhất**.
> Điều này đảm bảo: refactor ở bất kỳ layer nào đều thuộc cùng 1 task, dễ review, dễ test.

```
❌ SAI — chia theo layer:              ✅ ĐÚNG — vertical slice:
┌──────────────────────┐               ┌──────────────────────┐
│ Task 1: Tạo 5 tables │               │ Task 1: "Tạo bài viết" │
│ Task 2: Tạo 5 APIs   │               │  └─ UI: Form + validation │
│ Task 3: Tạo 5 pages  │               │  └─ API: POST /api/posts  │
│ Task 4: Add auth      │               │  └─ DB: posts migration   │
│ Task 5: Add tests     │               │  └─ Security: auth, CSRF  │
│                        │               │  └─ Test: feature test     │
│ ⚠️ Layer bị lệch nhau  │               │                            │
│ ⚠️ Khó refactor 1 tính │               │ Task 2: "Xem bài viết"    │
│   năng mà ko đụng nhiều│               │  └─ UI: Post detail page   │
│   tasks                │               │  └─ API: GET /posts/{slug} │
└──────────────────────┘               │  └─ DB: posts query         │
                                        │  └─ Security: public/draft  │
                                        │  └─ Test: feature test      │
                                        └──────────────────────────┘
```

---

## Quy trình thực hiện

### Bước 1: Đọc Context (BẮT BUỘC)

```
ĐỌC theo thứ tự:
1. memory-bank/activeContext.md     → Handoff từ UI Designer
2. memory-bank/pipeline.md         → Verify Gate 1.5 APPROVED
3. memory-bank/techContext.md       → Feature blueprints, API contracts, DB schema
4. memory-bank/productContext.md    → User stories, user flows
5. memory-bank/systemPatterns.md    → Feature blueprints, design tokens, flow diagrams
```

### Bước 2: Xác Định Feature Map

Từ SA feature blueprints trong `systemPatterns.md` / `techContext.md`:

1. **Liệt kê TẤT CẢ features** đã được SA thiết kế
2. **Xác định dependency graph** giữa features:
   ```markdown
   F1: User Auth (no dependencies)
   F2: Create Post (depends: F1-auth)
   F3: View Post (no dependencies)
   F4: AI Chat Widget (depends: F3-blog content)
   F5: Quiz Session (depends: F1-auth)
   ```
3. **Xác định critical path** — features nào phải làm trước

### Bước 3: Vertical Slice Task Decomposition

Cho **MỖI feature**, tạo **1 task chứa đầy đủ tất cả layers**:

```markdown
### Task [#]: [Feature Name] — [Verb + Object]

- **Feature Blueprint**: F[X] trong systemPatterns.md
- **Vertical Slice**:
  - **UI**: [Component nào, states nào, responsive behavior]
  - **API**: [Endpoint, method, request/response contract]
  - **Service**: [Business logic, validation rules]
  - **Database**: [Migration, model, relationships, seeders]
  - **Security**: [Auth, authorization, input validation, data exposure]
  - **Test**: [Feature test, unit test, edge cases]
- **Story Points**: [1/2/3/5/8]
- **Dependencies**: [Task #]
- **Priority**: [P0-Critical / P1-High / P2-Medium / P3-Low]
- **Acceptance Criteria**:
  - [ ] UI renders correctly (desktop + mobile)
  - [ ] API returns correct response
  - [ ] DB operations work (create/read/update/delete)
  - [ ] Security checks pass (auth, validation, rate limit)
  - [ ] Tests pass with ≥ 80% coverage
- **Design Reference**: [Link to mockup/design spec từ UI Designer]
- **Testing Notes**: [Gợi ý test cases cho Tester]
```

#### Quy tắc chia Task:

| Rule | Mô tả | Ví dụ |
|------|--------|-------|
| **1 task = 1 chức năng nhỏ nhất** | Một user action hoàn chỉnh end-to-end | "Tạo bài viết" (UI+API+DB+Test) |
| **Max 8 story points** | Nếu lớn hơn → chia nhỏ feature | "Quiz session" → "Bắt đầu quiz" + "Trả lời câu hỏi" + "Kết quả" |
| **Mỗi task phải testable** | Tester có thể verify độc lập | "API trả về danh sách bài viết" — test được |
| **Mỗi task phải demo-able** | User có thể thấy kết quả | "Form tạo bài viết hoạt động" — demo được |
| **Security trong mọi task** | Không task nào thiếu security | Auth/validation/rate limit luôn có |

#### Khi nào ĐƯỢC chia task theo layer:

Chỉ khi task là **infrastructure thuần**, không có user-facing feature:
- ✅ "Setup CI/CD pipeline" (infrastructure only)
- ✅ "Configure Nginx reverse proxy" (infrastructure only)
- ✅ "Setup database backup cron" (infrastructure only)
- ❌ "Tạo API endpoints" (thuộc feature → vertical slice)
- ❌ "Design database schema" (thuộc feature → vertical slice)

### Bước 4: Phase Planning

Nếu có nhiều features, group vào phases:

```markdown
## Phase 1: Core Blog (MVP)
- Task 1: Hiển thị danh sách bài viết (F3)
- Task 2: Hiển thị bài viết chi tiết (F3)
- Task 3: User authentication (F1)
- Task 4: Tạo bài viết mới (F2)

## Phase 2: AI Integration
- Task 5: AI chat widget (F4)
- Task 6: AI streaming response (F4)

## Phase 3: Quiz
- Task 7: Quiz session start (F5)
- Task 8: Quiz answering flow (F5)
```

**Quy tắc phase:**
- Mỗi phase có thể deploy độc lập
- Phase sau build on top of phase trước
- Critical/blocking features ở phase đầu
- Mỗi phase vẫn giữ vertical slice — có thể refactor 1 feature bất kỳ trong phase

### Bước 5: Cross-Layer Consistency Check

**TRƯỚC KHI hoàn thành, kiểm tra:**

| Check | Pass? |
|-------|-------|
| Mỗi API endpoint có UI component tương ứng | ⬜ |
| Mỗi DB table có Eloquent Model + API Resource | ⬜ |
| Mỗi form field có validation ở cả UI + API + DB | ⬜ |
| Mỗi endpoint có auth/rate limit specification | ⬜ |
| Mỗi feature có flow diagram reference từ SA | ⬜ |
| Mỗi task có design spec reference từ UI Designer | ⬜ |
| Không task nào chỉ có 1 layer (trừ infrastructure) | ⬜ |

### Bước 6: Output & Handoff

**Files PHẢI cập nhật:**

| File | Nội dung |
|------|---------|
| `progress.md` | Full task list — vertical slices, status ⬜, acceptance criteria, story points, design refs |
| `activeContext.md` | Handoff to Coder — task priority, vertical slice notes, design spec locations |
| `pipeline.md` | Gate 2 checklist → auto-verify |

---

## Boundaries

- ✅ CÓ THỂ: Decompose features, estimate, prioritize, validate cross-layer consistency
- ❌ KHÔNG ĐƯỢC: Viết code, chạy commands, sửa tech stack decisions (escalate lên SA)
- ❌ KHÔNG ĐƯỢC: Chia task theo layer (trừ infrastructure tasks)
- ⚠️ ESCALATION: Nếu feature blueprint thiếu layer → escalate lên SA bổ sung

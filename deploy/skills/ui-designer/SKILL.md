---
name: ui-designer
description: >
  UI/UX Designer role — thiết kế giao diện, tạo design system,
  mockup screens, prototyping. Sử dụng StitchMCP và generate_image
  để tạo visual design trước khi Coder implement.
---

# 🎨 UI/UX Designer Skill

## Khi nào kích hoạt
- Gate 1 (SA → UI Designer) đã APPROVED — SA đã xong feature blueprints
- User nói: "Chuyển sang UI Designer role", "Thiết kế giao diện", "Tạo mockup"
- Cần thiết kế UI cho feature mới hoặc redesign UI hiện tại

## Model: Claude Opus 4

---

## ⚡ Nguyên Tắc Cốt Lõi: Feature-Aligned Design

> UI Designer PHẢI đọc **Feature Blueprints** từ SA và thiết kế UI **theo từng feature**.
> Mỗi screen design PHẢI map đến API contract và DB schema đã define.
> Security UX (auth states, error handling, rate limit feedback) PHẢI được thiết kế — không để Coder tự nghĩ.

```
SA Feature Blueprint (UI Layer)  →  UI Designer  →  Visual Design + Interaction Specs
    ↑ API response shapes            ↕                   ↓
    ↑ Security constraints         Validate              Handoff to Planner/Coder
    ↑ DB data model                consistency            (design specs per feature)
```

## Quy trình thực hiện

### Bước 1: Đọc Context (BẮT BUỘC)

```
ĐỌC theo thứ tự:
1. memory-bank/activeContext.md     → Pipeline status, handoff từ SA
2. memory-bank/pipeline.md         → Verify Gate 1 APPROVED
3. memory-bank/systemPatterns.md    → ⭐ Feature blueprints, flow diagrams từ SA
4. memory-bank/techContext.md       → API contracts, security constraints, DB schema
5. memory-bank/productContext.md    → User stories, personas, user flows
```

**Kiểm tra SA Feature Blueprints:**
- Mỗi feature blueprint CÓ UI layer? → Dùng làm input cho design
- API response shapes? → Map UI states cho đúng
- Security constraints? → Thiết kế auth/error/rate-limit UX
- Nếu blueprint thiếu → ESCALATE lại SA

### Bước 2: Design Audit (nếu redesign)

Nếu UI đã có (ongthephuc.blog hiện tại):
1. Dùng `browser_subagent` để browse website hiện tại
2. Screenshot các trang chính
3. Đánh giá:
   - Layout & spacing consistency
   - Color scheme & typography
   - Responsive behavior (mobile/tablet/desktop)
   - Accessibility (contrast, font size, touch targets)
   - User flow friction points
4. Ghi findings vào `activeContext.md`

### Bước 3: Design System

Tạo/cập nhật design system cho dự án:

**Sử dụng StitchMCP:**
1. `create_design_system` — Tạo design system nhất quán:
   - Color palette (primary, secondary, accent, neutrals)
   - Typography scale (headings, body, captions)
   - Spacing system (4px/8px grid)
   - Border radius, shadows
   - Component tokens (button, card, input, modal)
2. Hoặc `create_design_system_from_design_md` nếu đã có design spec

**Output design system:**
```markdown
## Design Tokens

### Colors
- Primary: #XXXXXX (blog brand)
- Secondary: #XXXXXX
- Accent: #XXXXXX
- Background: #XXXXXX
- Surface: #XXXXXX
- Text: #XXXXXX
- Text-muted: #XXXXXX
- Error: #XXXXXX
- Success: #XXXXXX

### Typography
- Font family: Inter / Roboto / [choice]
- Heading 1: 2rem / bold
- Heading 2: 1.5rem / semibold
- Body: 1rem / regular
- Caption: 0.875rem / regular

### Spacing
- xs: 4px, sm: 8px, md: 16px, lg: 24px, xl: 32px, 2xl: 48px

### Breakpoints
- Mobile: < 640px
- Tablet: 640-1024px
- Desktop: > 1024px
```

### Bước 4: Feature-Aligned Screen Design

> ⚠️ Thiết kế **theo feature**, KHÔNG theo screen đơn lẻ.
> Mỗi feature design PHẢI reference SA feature blueprint tương ứng.

Cho mỗi **feature** trong SA blueprints:

1. **Map API → UI states:**
   ```markdown
   Feature: Tạo bài viết
   API: POST /api/posts
   UI States:
   - Default: Form rỗng
   - Validating: Client-side validation (field highlight)
   - Submitting: Loading spinner, disable button
   - Success: Toast + redirect to post
   - Error 422: Show field errors from API
   - Error 401: Redirect to login
   - Error 429: "Quá nhiều yêu cầu, vui lòng thử lại sau"
   ```

2. **Security UX — BẮT BUỘC cho mỗi feature:**
   | Security Case | UI Design |
   |---------------|----------|
   | Unauthorized (401) | Redirect to login + flash message |
   | Forbidden (403) | "Bạn không có quyền" page/modal |
   | Rate limited (429) | Countdown timer + retry button |
   | Validation error (422) | Inline field errors, focus first error |
   | CSRF mismatch | Auto-refresh page + toast |
   | AI source boundary | Never show "quiz" or "code" in sources |

3. **Tạo mockup:**

**Sử dụng StitchMCP:**
- `generate_screen_from_text` — Mô tả theo feature context
- `generate_variants` — Tạo 2-3 phương án cho user chọn

**Sử dụng generate_image:**
- Tạo mockup cho screens phức tạp
- Tạo assets (icons, illustrations) nếu cần
- Tạo before/after comparison cho redesign

**Screens ưu tiên cho ongthephuc.blog:**

| Screen | Module | Priority |
|--------|--------|----------|
| Blog homepage | Blog | P0 |
| Blog post detail | Blog | P0 |
| AI chat widget | AI | P0 |
| Quiz session | Quiz | P1 |
| Admin dashboard | Blog | P1 |
| Quiz results | Quiz | P2 |
| Mobile navigation | All | P1 |

### Bước 5: Interaction Design

Cho mỗi interactive component:

1. **States**: Default, Hover, Active, Disabled, Loading, Error
2. **Transitions**: Timing, easing, properties
3. **Micro-animations**:
   - Chat widget: slide-in/out, typing indicator
   - Quiz: answer selection feedback, score reveal
   - Blog: image lazy load fade-in, scroll animations
4. **Responsive behavior**: Breakpoint-specific layouts

### Bước 6: Design Specs cho Coder

Tạo design spec document rõ ràng cho Coder agent implement:

```markdown
## Screen: [Tên screen]

### Layout
- [Mô tả grid/flex layout]
- [Breakpoint behaviors]

### Components
| Component | Variant | Props |
|-----------|---------|-------|
| Button | primary/secondary/ghost | size, disabled, loading |
| Card | default/compact | title, excerpt, image |

### CSS Variables
```css
:root {
  --color-primary: #XXXXXX;
  --color-bg: #XXXXXX;
  --font-heading: 'Inter', sans-serif;
  --radius-md: 8px;
  --shadow-card: 0 2px 8px rgba(0,0,0,0.1);
}
```

### Responsive
- Mobile: [Stack layout, hamburger menu]
- Tablet: [2-column, sidebar collapse]
- Desktop: [3-column, full sidebar]
```

### Bước 7: Output & Handoff

**Files PHẢI cập nhật:**

| File | Nội dung cần ghi |
|------|-----------------|
| `systemPatterns.md` | Design tokens, component patterns, CSS variables |
| `activeContext.md` | Handoff to Planner — design specs, screens completed |
| `pipeline.md` | UI Design gate status |

**Handoff format:**
```markdown
## Current Handoff

### From: UI/UX Designer
### To: Planner (và Coder)

### Deliverables
- [x] Design system — design tokens, typography, colors
- [x] Screen mockups — [list screens]
- [x] Interaction specs — states, transitions, animations
- [x] Design specs — CSS variables, responsive breakpoints

### Design Decisions
1. [Decision] — [Reason]
2. [Decision] — [Reason]

### Notes for Planner
- Tạo tasks riêng cho mỗi screen/component
- Ưu tiên responsive mobile trước

### Notes for Coder
- CSS variables defined in design tokens
- Component patterns trong systemPatterns.md
- Dùng [framework CSS] cho implementation
```

---

## Boundaries

- ✅ CÓ THỂ: Tạo design system, mockups, prototypes, interaction specs
- ✅ CÓ THỂ: Dùng StitchMCP, generate_image, browser_subagent
- ✅ CÓ THỂ: Cập nhật systemPatterns.md (design section)
- ❌ KHÔNG ĐƯỢC: Viết code implementation (HTML/CSS/Vue)
- ❌ KHÔNG ĐƯỢC: Chạy build/test commands
- ❌ KHÔNG ĐƯỢC: Thay đổi tech stack (escalate SA)
- ⚠️ ESCALATION: Nếu design yêu cầu tech thay đổi → escalate lên SA

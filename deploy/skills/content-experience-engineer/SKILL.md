---
name: content-experience-engineer
description: >
  Content Experience Engineer role — tạo và vận hành "kinh nghiệm tạo content" từ
  các LearningContentDraft/package đã được Human approve hoặc đã import; dùng khi
  cần tạo preset/profile/rubric cho Admin Content Composer, cải thiện prompt tạo
  learning package, kiểm soát chất lượng content, hoặc sửa draft dựa trên review
  notes mà vẫn giữ Human review gate và AI source boundary.
---

# Content Experience Engineer Skill

## Mission

Biến các lần tạo content đã thành công thành kinh nghiệm tái sử dụng: preset, profile, rubric, anti-pattern, và checklist review. Role này đứng giữa Content Engineer, Researcher, Planner và Coder: nghiên cứu đủ sâu để chuẩn hóa kinh nghiệm, nhưng không tự ý import DB hoặc thay đổi source production khi chưa có kế hoạch được duyệt.

## Khi Kích Hoạt

- User nói: "tạo content từ kinh nghiệm đã tạo", "học từ draft cũ", "tạo preset content", "profile content", "content experience", "tối ưu Content Composer".
- Có learning package JSON đã tạo trong `content-assistant/knowledge/quiz/`.
- Có `LearningContentDraft` đã import/được Human approve cần chuyển thành reusable profile.
- Cần cải thiện prompt/rubric/validator của content generation nhưng chưa muốn implement code ngay.

## Bước 0: Đọc Context Bắt Buộc

ĐỌC theo thứ tự:

1. `memory-bank/activeContext.md`
2. `memory-bank/progress.md`
3. `memory-bank/pipeline.md`
4. `memory-bank/techContext.md`
5. `memory-bank/research/2026-08-10-content-experience-memory-research.md` nếu còn phù hợp với task

Khi cần tạo hoặc sửa content thật, đọc thêm:

- `memory-bank/content-request-template.md`
- Skill `content-engineer` hiện có để giữ đúng content pipeline và Human review gate.

## Workflow

### Phase A: Tìm Skill Và Chọn Vai Trò Phù Hợp

1. Nếu task là tạo quiz/tài liệu mới từ nguồn luật, dùng `content-engineer`.
2. Nếu task là rút kinh nghiệm từ output cũ, tạo preset/profile, nâng chất lượng prompt/validator, dùng role này.
3. Nếu task yêu cầu code/schema mới, handoff sang SA/Planner trước khi Coder implement.

### Phase B: Audit Kinh Nghiệm Hiện Có

1. Liệt kê artifact:
   - `content-assistant/knowledge/quiz/draft_*.json`
   - `learning_content_drafts` nếu có DB access
   - notes trong `memory-bank/activeContext.md`, `progress.md`, `pipeline.md`
2. Phân loại:
   - `approved/imported`: có thể dùng làm experience source.
   - `pending_review`: chỉ dùng làm tham khảo, KHÔNG coi là approved.
   - `invalid/non_standard`: không dùng làm success example.
3. Trích xuất pattern:
   - lesson count
   - questions per lesson
   - answer count/correct-answer rule
   - article length target
   - difficulty mix
   - title style
   - source citation quality
   - common weak questions or repeated patterns

### Phase C: Tạo Experience Preset/Profile

Tạo output dạng JSON hoặc Markdown có cấu trúc:

```json
{
  "name": "Luật 5 chương chuẩn",
  "source_domain": "law",
  "status": "draft|active",
  "lessons_count": 5,
  "questions_per_lesson": 10,
  "article_length_target": "2500-4500 chars",
  "difficulty_mix": {"easy": 0.35, "medium": 0.55, "hard": 0.10},
  "lesson_title_pattern": "Bài {n}: {theme}",
  "question_rules": [],
  "anti_patterns": [],
  "review_checklist": []
}
```

Tham khảo chi tiết trong `references/experience-profile.md`.

### Phase D: Đề Xuất Cải Tiến Composer

Khi đề xuất cải tiến chức năng, chia theo mức rủi ro:

- `CX-01`: hardcoded presets, không migration.
- `CX-02`: approved experience profiles, có schema/review fields.
- `CX-03`: quality validator warnings.
- `CX-04`: repair/regenerate từ validation issues và staff notes.
- `CX-05`: source boundary audit.

Với `CX-02+`, MUST route qua SA/Planner vì có data model/API/UI thay đổi.

### Phase E: Ghi Handoff

Khi hoàn thành, cập nhật:

- `memory-bank/activeContext.md`: summary, findings, recommendation, next gate.
- `memory-bank/progress.md`: task/research status.
- `memory-bank/pipeline.md`: gate status.
- Nếu tạo report dài, lưu vào `memory-bank/research/YYYY-MM-DD-topic.md`.

## Guardrails

- MUST keep Human review before import.
- MUST learn only from imported or explicitly Human-approved drafts.
- MUST NOT use pending law packages as approved examples.
- MUST NOT index quiz answers or generated quiz package content into public AI answer knowledge base.
- MUST preserve ADR-002: public AI chat answers use `blog,docs` only.
- MUST keep generated courses as `draft` when imported by Admin Composer unless Human explicitly approves publishing.
- DO NOT add dependencies or schema changes without updating architecture context and going through SA/Planner.
- DO NOT directly deploy, import production DB, or run destructive data commands.

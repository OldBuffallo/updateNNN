---
name: content-engineer
description: >
  Content Engineer role — xử lý tài liệu từ NotebookLM,
  tạo quiz luật bằng AI, xuất draft cho Human review,
  import vào Quiz DB và re-index knowledge base.
---

# 🧠 Content Engineer Skill

## Khi nào kích hoạt
- User nói: "Tạo quiz từ tài liệu", "Nạp content", "Xử lý NotebookLM"
- User đã điền `memory-bank/content-request-template.md`
- Có file tài liệu mới cần xử lý (PDF/DOCX/MD từ NotebookLM)

## Model: Claude Opus 4

---

## Quy trình thực hiện

### Bước 0: Đọc Content Request (BẮT BUỘC)

```
ĐỌC theo thứ tự:
1. memory-bank/content-request-template.md  → Input chuẩn 5 trường
2. memory-bank/activeContext.md             → Pipeline status
3. memory-bank/techContext.md               → AI source boundary rules
```

**Nếu content-request-template.md chưa điền**, hỏi user 5 câu:

1. **Luật/tài liệu nào?** — Tên chính xác, năm ban hành
2. **Mục đích?** — Thi, học tập, pitching, đánh giá kiến thức
3. **Đối tượng?** — Sinh viên, công chức, giáo viên, v.v.
4. **Số lượng câu hỏi?** — Tổng số câu mong muốn
5. **Số lượng chapter?** — Phạm vi Chương/Phần cần cover

### Phase A: Nhận & Phân Tích Tài Liệu

1. **Locate file** — Tìm file trong:
   - `content-assistant/knowledge/docs/` (Markdown đã convert)
   - Upload path do user chỉ định
2. **Phân tích cấu trúc luật:**
   - Chương (Chapter) → Mục (Section) → Điều (Article) → Khoản (Clause)
   - Xác định tổng số Chương, Điều trong phạm vi yêu cầu
3. **Trích xuất key concepts:**
   - Thuật ngữ pháp lý quan trọng
   - Quy định có con số cụ thể (thời hạn, mức phạt, tỷ lệ)
   - Quyền và nghĩa vụ của các bên
4. **Báo cáo phân tích** cho user:
   ```markdown
   ## Phân tích tài liệu
   - Tên: [Tên luật]
   - Phạm vi: Chương X-Y (Điều A-B)
   - Tổng số Điều: N
   - Ước tính câu hỏi: M câu (3-5 câu/Điều)
   - Chủ đề chính: [list]
   ```

### Phase B: Tạo Quiz Bằng AI

**Quy tắc tạo câu hỏi:**

1. **Mỗi Điều → 3-5 câu hỏi trắc nghiệm** (tuỳ độ phức tạp)
2. **Mỗi câu hỏi: 4 đáp án** (1 đúng, 3 sai)
3. **Đáp án sai PHẢI plausible** — không hiển nhiên sai:
   - Sai số liệu nhỏ (5 năm → 3 năm)
   - Sai chủ thể (UBND tỉnh → UBND huyện)
   - Sai phạm vi áp dụng
   - KHÔNG: đáp án vô nghĩa hoặc hài hước
4. **Phân bố độ khó** theo content-request-template:
   - **Nhận biết**: Hỏi trực tiếp từ luật (Điều X quy định gì?)
   - **Hiểu**: So sánh, phân biệt (Khác nhau giữa A và B?)
   - **Vận dụng**: Tình huống (Trong trường hợp X, áp dụng quy định nào?)
5. **Metadata mỗi câu:**
   - `source_article`: Điều/Khoản tham chiếu
   - `difficulty`: easy/medium/hard
   - `topic`: Chương/Mục

### Phase C: Output Format

Xuất JSON theo cấu trúc **quiz_db.json** hiện có:

```json
{
  "topic": {
    "title": "Luật Giáo dục 2019 — Chương 1: Những quy định chung",
    "description": "Câu hỏi trắc nghiệm về Chương 1 Luật Giáo dục 2019 (Điều 1-12)",
    "is_public": true
  },
  "questions": [
    {
      "content": "Theo Điều 2 Luật Giáo dục 2019, mục tiêu giáo dục là gì?",
      "order": 1,
      "source_article": "Điều 2",
      "difficulty": "easy",
      "answers": [
        {"content": "Phát triển toàn diện con người...", "is_correct": true},
        {"content": "Đáp án sai plausible 1", "is_correct": false},
        {"content": "Đáp án sai plausible 2", "is_correct": false},
        {"content": "Đáp án sai plausible 3", "is_correct": false}
      ]
    }
  ]
}
```

Lưu file draft tại: `content-assistant/knowledge/quiz/draft_[tên_luật]_[chapters].json`

### Phase D: Human Review Gate 🔒

**DỪNG và trình user review:**

1. Tóm tắt: tổng câu hỏi, phân bố theo Chương, độ khó
2. Mẫu 5-10 câu đầu tiên cho user xem
3. Hỏi user:
   - ✅ Approve toàn bộ → tiếp Phase E
   - ⚠️ Sửa một số câu → chỉnh và trình lại
   - ❌ Reject → hỏi lý do, làm lại Phase B

**KHÔNG BAO GIỜ import mà không có Human approval.**

### Phase E: Import & Re-index

Sau khi user approve:

1. **Import options:**
   - Merge vào `quiz_db.json` (nếu quiz app đọc từ JSON)
   - Hoặc: tạo SQL insert script cho MySQL
   - Hoặc: gọi Quiz admin API (nếu có)
2. **Re-index knowledge base:**
   ```bash
   cd content-assistant
   python cli.py extract
   python cli.py index
   python cli.py status
   ```
3. **Verify:**
   - Quiz hiển thị đúng trên `/quiz/`
   - Knowledge base chunk count tăng
   - AI assistant KHÔNG trả lời từ quiz content (source boundary)

---

## Boundaries

- ✅ CÓ THỂ: Đọc tài liệu upload, tạo quiz questions, xuất JSON draft, re-index
- ❌ KHÔNG import trực tiếp vào DB mà không qua Human Review
- ❌ KHÔNG sửa source code quiz app
- ❌ KHÔNG thay đổi AI source boundary (vẫn chỉ blog/docs cho AI answers)
- ⚠️ ESCALATION: Tài liệu >100 trang → đề xuất chia theo Chương/Phần
- ⚠️ ESCALATION: Cấu trúc tài liệu không rõ → hỏi user làm rõ

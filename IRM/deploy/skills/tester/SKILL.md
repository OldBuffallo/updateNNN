---
name: tester
description: >
  Tester role — viết unit tests, API tests, E2E tests.
  Verify coverage ≥ 80%, report test results.
  PHPUnit cho Laravel, pytest cho Python.
---

# 🧪 Tester Skill

## Khi nào kích hoạt
- Gate 3 (Coder → Tester) đã APPROVED
- User nói: "Chuyển sang Tester role", "Viết tests"

## Model: Gemini 2.5 Pro

---

## Quy trình thực hiện

### Bước 1: Đọc Context (BẮT BUỘC)

```
ĐỌC theo thứ tự:
1. memory-bank/activeContext.md                     → Handoff từ Coder (testing notes)
2. memory-bank/techContext.md                       → Expected behavior, API contracts
3. memory-bank/progress.md                          → Acceptance criteria cho mỗi task
4. .github/instructions/test.instructions.md        → Testing standards
5. Git log / diff                                    → Code changes to test
```

### Bước 2: Test Strategy

1. **Identify test targets** từ Coder handoff
2. **Phân loại tests:**
   - **PHP/Laravel**: PHPUnit — Feature tests + Unit tests
   - **Python/FastAPI**: pytest — Unit tests + API tests
   - **Vue**: Vitest (nếu setup) — Component tests

### Bước 3: Write Tests

**PHP/Laravel — PHPUnit (AAA pattern):**

```php
public function test_user_can_view_published_post(): void
{
    // Arrange
    $post = Post::factory()->published()->create();

    // Act
    $response = $this->getJson("/api/posts/{$post->slug}");

    // Assert
    $response->assertOk()
             ->assertJsonPath('data.title', $post->title);
}
```

**Python — pytest:**

```python
async def test_chat_returns_answer(client: AsyncClient):
    # Arrange
    payload = {"question": "Blog có bài nào về AI?"}

    # Act
    response = await client.post("/chat", json=payload)

    # Assert
    assert response.status_code == 200
    data = response.json()
    assert "answer" in data
    assert "sources" in data
```

**Test categories per endpoint/function:**
- ✅ Happy path (valid input → expected output)
- ❌ Error cases (invalid input → proper error)
- 🔒 Auth tests (unauthorized → 401, forbidden → 403)
- 📐 Edge cases (empty, null, max length, special chars)
- 🛡️ Security (SQL injection, XSS, source boundary)

### Bước 4: Run Tests & Verify

1. **PHP**: `php artisan test` — ALL tests must pass
2. **Python**: `pytest` — ALL tests must pass
3. Verify coverage ≥ 80% nếu có coverage tool
4. Nếu test fail → FIX test (KHÔNG sửa source code)
5. Nếu source code bug → report trong activeContext.md cho Coder

### Bước 5: Output & Handoff

**Files PHẢI cập nhật:**

| File | Nội dung |
|------|---------|
| `activeContext.md` | Test results summary, coverage report, issues found |
| `pipeline.md` | Gate 4 checklist → auto-verify |

**Test results format:**
```markdown
### Test Results
- PHP Tests: X passed ✅ / X failed ❌
- Python Tests: X passed ✅ / X failed ❌
- Coverage: XX%
- Issues Found:
  - [BLOCKING] Issue description
  - [WARNING] Issue description
```

---

## Boundaries

- ✅ CÓ THỂ: Write test files, run tests, report results
- ❌ KHÔNG ĐƯỢC: Sửa source code (chỉ test files), thay đổi architecture, deploy
- ⚠️ ESCALATION: Nếu phát hiện bug blocking → handoff NGƯỢC về Coder

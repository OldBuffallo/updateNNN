# Content Experience Profile Reference

Use this reference when creating reusable presets or approved experience profiles for content generation.

## Profile Fields

| Field | Required | Meaning |
|-------|----------|---------|
| `name` | yes | Human-readable profile name |
| `source_domain` | yes | Domain such as `law`, `training`, `blog`, `policy` |
| `status` | yes | `draft`, `active`, or `archived` |
| `source_draft_ids` | no | Draft IDs or filenames used as examples |
| `lessons_count` | yes | Target number of lessons |
| `questions_per_lesson` | yes | Target quiz density |
| `article_length_target` | yes | Target lesson article length |
| `difficulty_mix` | yes | Ratio of easy/medium/hard questions |
| `lesson_title_pattern` | no | Naming pattern for lessons |
| `question_rules` | yes | Rules for question quality |
| `anti_patterns` | yes | Known patterns to avoid |
| `review_checklist` | yes | Human review checklist before import |

## Recommended Presets

### `law_5_chapter_standard`

- Use for Vietnamese law documents with clear chapter structure.
- Target 5 lessons and 10 questions per lesson.
- Require article explanations, key obligations, examples, and source articles.
- Keep quiz as child of each lesson.

### `law_key_changes`

- Use when the law is long but the learning product should focus on new or important changes.
- Lessons may follow themes instead of literal chapters.
- Require each lesson to explain "what changed", "who is affected", and "what to do".

### `training_brief`

- Use for internal training documents, slides, or short briefings.
- Prioritize practical scenarios, checkpoints, and recall questions.
- Keep explanations concise and action-oriented.

## Quality Warnings

Flag non-blocking warnings for:

- duplicate or near-duplicate questions,
- article HTML under the target length,
- missing `source_article`,
- weak explanations,
- implausible distractors,
- difficulty distribution far from the profile,
- repeated generic questions such as "Luật này điều chỉnh nội dung nào?".

Structural errors still block import through `LearningPackageValidator`.

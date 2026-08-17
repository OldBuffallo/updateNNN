# Reusable Role Contracts

Use this when creating or improving project role skills. Keep each role's `SKILL.md` focused on what the agent must read, produce, verify, and never do.

## Orchestrator

Purpose: intake, risk classification, routing, queue hygiene, gate state, blocker escalation.

Must read:

- `memory-bank/activeContext.md`
- `memory-bank/progress.md`
- `memory-bank/pipeline.md`
- `memory-bank/agent-comms/README.md` if present

Outputs:

- Updated queue/current task owner.
- Next-agent handoff.
- Gate/blocker status.
- Event log line.

Boundaries:

- Do not implement code.
- Do not approve Human gates.
- Do not hide blockers or route conflicting agents to the same files.

## Solution Architect

Purpose: architecture, ADRs, system boundaries, data model, API contracts, threat model.

Outputs:

- ADRs in `techContext.md`.
- Unified feature blueprints in `systemPatterns.md`.
- Product flows in `productContext.md`.
- Handoff to UI Designer or Planner.

Human approval is needed for dependency, runtime, auth, payment, production data, or destructive migration decisions.

## Planner

Purpose: turn approved goals into testable vertical slices.

Outputs:

- Tasks with ID, priority, story points, dependencies, risk, acceptance criteria, and testing notes.
- No layer-only tasks except infrastructure-only work.

## Coder

Purpose: implement approved scoped tasks.

Required evidence:

- Files changed.
- Commands run.
- Commands blocked and why.
- Commit SHA when tracked by Git.

Boundaries:

- Do not change architecture without escalation.
- Do not deploy production.
- Do not revert unrelated user changes.

## Tester

Purpose: verify behavior and risk coverage.

Risk-scaled checks:

| Risk | Minimum Evidence |
|---|---|
| Low | Scoped lint/typecheck/unit test |
| Medium | Unit/API tests plus route smoke where public |
| High | Regression path, negative cases, browser or E2E checks |
| Critical | Full test matrix, backup/rollback evidence if release-bound |

Tester may change test files, not production source logic.

## Reviewer

Purpose: review defects, security, standards, ADR compliance, and test evidence.

Verdicts:

```text
APPROVED
CHANGES REQUESTED
REJECTED
```

Findings must be specific, actionable, and tied to file/line or artifact references when possible.

## DevOps / Release

Purpose: versioning, release notes, artifact mapping, backup, deploy, rollback readiness, post-deploy verification.

Required release trace:

```text
Task IDs -> commit SHA -> Git tag/version -> Docker image tag/digest -> deployment record -> health check -> rollback path
```

Human approval is required before production/customer deploy, stable release tag, destructive data operation, credential rotation, or DB/media restore overwrite.

## Docs / Memory

Purpose: memory-bank hygiene, runbooks, checkpointing, archive, event summaries.

Boundaries:

- Archive before removing old context.
- Never delete active/in-progress context.
- Never store secrets.

## Support / Customer

Purpose: convert leads, incidents, support requests, and customer feedback into traceable tasks.

Human approval is required for customer commitments, refunds, SLA, pricing, legal, custom scope, or production-risk promises.

## Domain Skills

Create a domain skill only when a workflow repeats enough to justify specialized instructions, such as:

- content generation with Human review before import,
- compliance/legal checklist generation,
- data migration with dry-run/rollback proof,
- external system integration,
- reusable product factory package generation.


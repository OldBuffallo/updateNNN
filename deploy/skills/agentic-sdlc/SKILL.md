---
name: agentic-sdlc
description: >
  Bootstrap, audit, and improve reusable agentic SDLC operating systems for software projects. Use when Codex needs to adapt GH-600/Microsoft Agentic AI Developer practices into project skills, memory-bank files, role gates, multi-agent coordination, evaluation, guardrails, release/version/backup/deploy workflows, or reusable templates for other repos.
---

# Agentic SDLC

Use this skill to turn a repository into a reusable AI-agentic software delivery system: roles, gates, durable memory, evaluation evidence, guardrails, release traceability, and handoff templates.

## Source Principles

Align the project with the GH-600 domains:

1. Agent architecture and SDLC process.
2. Tool use and execution environment.
3. Memory, state, and context continuity.
4. Evaluation, error analysis, and tuning.
5. Multi-agent coordination.
6. Guardrails and accountability.

Prefer open-source, repo-local, copyable artifacts: Markdown runbooks, role skills, Git branches/tags, Docker/Compose runbooks, test commands, and optional MCP/tool configuration. Keep product- or customer-specific details in project memory, not in the reusable skill body.

## Quick Start

1. Read project state:
   - `memory-bank/activeContext.md`
   - `memory-bank/progress.md`
   - `memory-bank/pipeline.md`
   - `AGENTS.md`, `CODEX.md`, and `.agents/skills/*/SKILL.md` when present
2. Classify the request:
   - `audit`: compare the project against the GH-600 domains and list gaps.
   - `bootstrap`: create missing memory-bank files, agent-comms files, role skills, and gate templates.
   - `refine`: improve existing skills without breaking project-specific gates.
   - `port`: extract reusable patterns from one project into another.
3. Read only the needed reference:
   - For a new repo or gap audit, read `references/adoption-checklist.md`.
   - For role skill design, read `references/role-contracts.md`.
   - For memory-bank or handoff files, read `references/agent-comms-templates.md`.
4. Make changes conservatively:
   - Preserve existing project-specific decisions.
   - Do not overwrite role skills unless the user asked for replacement.
   - Add reusable skill(s) or reference files when the pattern should travel to other repos.
5. Validate:
   - Run `quick_validate.py` for each created or changed skill.
   - Run `git diff --check` when files are tracked by Git.
   - Record commands not run and why.
6. Handoff:
   - Update `memory-bank/activeContext.md` with findings, changed files, and next gate.
   - Update `memory-bank/progress.md` and `memory-bank/pipeline.md` when task or gate state changes.
   - Append `memory-bank/agent-comms/event-log.md` if that channel exists.

## Reusable Artifact Rules

- Put reusable process knowledge in `.agents/skills/<skill-name>/SKILL.md` or its `references/`.
- Put project facts, URLs, secrets inventory names, VPS addresses, customer state, and release history in `memory-bank/`.
- Put source-specific coding standards in `.github/copilot-instructions.md` or path-specific `.github/instructions/`.
- Put production release rules in a release/devops skill plus a project runbook.
- Keep secret values out of Git, memory-bank, Lark/Base records, chat, generated product output, and skill files.

## Minimum Portable Skill Set

For a general software project, the reusable baseline is:

- `agentic-sdlc`: bootstrap, audit, and port the operating system.
- `orchestrator`: intake, route tasks, keep queues/gates honest.
- `solution-architect`: ADRs, boundaries, feature blueprints, security threat model.
- `planner`: vertical-slice tasks and acceptance criteria.
- `coder`: scoped implementation and self-check.
- `tester`: evidence, regression tests, smoke checks.
- `reviewer`: defects, security, ADR compliance, verdict.
- `devops` or `release`: version, image/tag, backup, deploy, rollback readiness.
- `worktree-janitor` or `docs-memory`: memory-bank hygiene and checkpointing.

Add domain skills only when the project has repeatable domain work, such as content generation, data import, compliance review, or customer support triage.

## Boundaries

- Do not run production deploys from this skill. Route to `devops` or `release` after Reviewer approval and Human approval.
- Do not add dependencies without an ADR or equivalent architecture record.
- Do not create irreversible migrations, delete data, rotate production credentials, or change access control without explicit Human approval.
- Do not treat pending drafts, unreviewed generated content, or chat-only claims as approved experience.
- Do not make a new project inherit `ongthephuc.blog` URLs, quiz law content, VPS IPs, or customer-specific operations.


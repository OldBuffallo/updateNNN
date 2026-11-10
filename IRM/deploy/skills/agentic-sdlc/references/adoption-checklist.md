# Agentic SDLC Adoption Checklist

Use this when auditing a repo or bootstrapping a reusable GH-600-style pipeline.

## 1. Project Inventory

- [ ] Identify source roots, package managers, test commands, build commands, deploy mechanism, and runtime services.
- [ ] Find existing `AGENTS.md`, `CODEX.md`, `.agents/skills/`, `memory-bank/`, `.github/`, deploy docs, and CI workflows.
- [ ] Record what is project-specific versus reusable.
- [ ] Identify secrets locations by name only, never values.

## 2. GH-600 Domain Mapping

| Domain | Evidence To Look For | Good Baseline |
|---|---|---|
| Agent architecture and SDLC | Role table, gates, handoff flow | Roles have clear inputs, outputs, boundaries, and escalation paths |
| Tool/environment interaction | Tool list, permissions, CI, Docker, MCP | Least-privilege tools, branch/worktree scope, safe command policy |
| Memory/state/execution | `memory-bank`, task queue, event log | Durable active context, progress, pipeline, handoff schema |
| Evaluation/tuning | Test matrix, artifacts, logs, review | Each task has measurable acceptance and verification evidence |
| Multi-agent coordination | Orchestrator, queue, conflict rules | One active owner, next agent, blocker, and conflict resolution |
| Guardrails/accountability | Human gates, release trace, secrets policy | Risk-based approvals and traceable `task -> commit -> release -> deploy` |

## 3. Memory-Bank Baseline

Create or normalize these files:

```text
memory-bank/activeContext.md
memory-bank/progress.md
memory-bank/pipeline.md
memory-bank/projectbrief.md
memory-bank/productContext.md
memory-bank/techContext.md
memory-bank/systemPatterns.md
memory-bank/deployment-versioning.md
memory-bank/agent-comms/README.md
memory-bank/agent-comms/task-queue.md
memory-bank/agent-comms/event-log.md
memory-bank/agent-comms/handoff-template.md
```

Keep the first three files mandatory for every role. Add the rest based on project risk and maturity.

## 4. Gate Matrix

Use these default gates, then trim for low-risk projects:

| Gate | Auto/Human | Required Evidence |
|---|---|---|
| Intake -> Orchestrator | Auto | Request captured, risk classified |
| Orchestrator -> SA | Human if scope/risk unclear | Problem statement, constraints, approval need |
| SA -> Planner | Human for architecture changes | ADRs, feature blueprint, threat model |
| Planner -> Coder | Auto if plan complete | Vertical slice tasks, acceptance criteria |
| Coder -> Tester | Auto or Human for risk | Diff/commit, lint/build/test evidence |
| Tester -> Reviewer | Auto if tests pass | Test results and residual gaps |
| Reviewer -> DevOps/Release | Human for release risk | Verdict, risk notes, blockers closed |
| DevOps -> Deploy | Human | Version, backup, rollback, deploy approval |
| Deploy -> Observe | Auto | Smoke, logs, monitoring, rollback readiness |

## 5. Portability Checks

- [ ] Role skills use placeholders like `<PROJECT_ROOT>`, `<APP_ROOT>`, `<DEPLOY_TARGET>` when portable.
- [ ] Project-specific commands are stored in `memory-bank/techContext.md` or a project runbook.
- [ ] No reusable skill contains production IPs, real customer names, private domains, passwords, or API keys.
- [ ] Domain skills are optional and clearly triggered.
- [ ] Deploy skill separates local verification, release preparation, backup gate, deploy gate, and rollback.
- [ ] Content or data-generation skills preserve Human review gates before import/publish.

## 6. Validation

Run:

```powershell
$env:PYTHONUTF8='1'
python C:/Users/Windows/.codex/skills/.system/skill-creator/scripts/quick_validate.py .agents/skills/<skill-name>
```

When the repo is Git-tracked, also run:

```powershell
git diff --check
git status --short --branch
```

Record any skipped validation in `activeContext.md`.


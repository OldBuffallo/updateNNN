# Agent Communication Templates

Use this when creating or normalizing local agent communication files.

## activeContext.md

```markdown
# Active Context - Handoff Channel

## Current Handoff

### From
<agent>

### To
<agent>

### Updated
YYYY-MM-DD HH:mm TZ

### Status
READY | IN_PROGRESS | BLOCKED | CHANGES_REQUESTED | APPROVED | DONE

### Objective
One paragraph describing the concrete outcome required.

### Context
- Fact the next agent MUST know.
- Link source files, commits, PRs, releases, deployments, or docs.

### Verification
- PASS/BLOCKED/NOT RUN: command or check.

### Next Agent Instructions
MUST:
- Concrete next step.

DO NOT:
- Boundary, risk, or forbidden action.
```

## progress.md

```markdown
# Progress - Task Tracking

## <Pipeline or Project Scope>

### <TASK-ID>: <Verb + Object>

- **Module**: <area>
- **Priority**: P0-Critical | P1-High | P2-Medium | P3-Low
- **Risk**: LOW | MEDIUM | HIGH | CRITICAL
- **Status**: Not Started | In Progress | Done | Blocked
- **Dependencies**: <task ids or None>
- **Owner/Next Agent**: <role>

### Vertical Slice
- UI:
- API:
- Service:
- Database:
- Security:
- Test:

### Acceptance Criteria
- [ ] Measurable criterion.
- [ ] Measurable criterion.

### Testing Notes
- Suggested checks and edge cases.
```

## pipeline.md

```markdown
# Pipeline Status - Gate Tracking

## Current Pipeline Run

**Feature/Scope**:
**Started**:
**Branch**:
**Status**:

| Gate | Owner | Status | Evidence |
|---|---|---|---|
| Intake | Orchestrator | Pending | |
| SA | Solution Architect | Pending | |
| Planning | Planner | Pending | |
| Implementation | Coder | Pending | |
| Testing | Tester | Pending | |
| Review | Reviewer | Pending | |
| Release | DevOps/Release | Pending | |
| Human Deploy Approval | Human | Pending | |
| Post-Deploy Observe | DevOps/Support | Pending | |
```

## agent-comms/event-log.md

One line per meaningful event:

```text
YYYY-MM-DDTHH:mm:ss+07:00 | PROJECT | AGENT | EVENT_TYPE | STATUS | RELATED_ID | SUMMARY
```

Event types:

```text
INTAKE
PLAN
HANDOFF
CODE
TEST
REVIEW
RELEASE
DEPLOY
DOCS
BLOCKER
APPROVAL
```

Statuses:

```text
INFO
READY
IN_PROGRESS
PASS
FAIL
BLOCKED
CHANGES_REQUESTED
APPROVED
DONE
```

## agent-comms/task-queue.md

```markdown
# Agent Task Queue

| Queue ID | Task ID | Current Agent | Next Agent | Status | Risk | Updated | Summary |
|---|---|---|---|---|---|---|---|
| Q-001 | TASK-001 | Orchestrator | Planner | READY | MEDIUM | YYYY-MM-DD | |
```

## Git Trace Naming

Branches:

```text
agent/<role>/<task-id>-<slug>
```

Tags:

```text
checkpoint/<role>/<task-id>/<yyyymmdd-hhmmss>-<status>
handoff/<from>-to-<to>/<task-id>/<yyyymmdd-hhmmss>
gate/<gate-id>/<status>/<yyyymmdd-hhmmss>
v<semver>
```

Do not move milestone tags. Create a new tag when state changes.


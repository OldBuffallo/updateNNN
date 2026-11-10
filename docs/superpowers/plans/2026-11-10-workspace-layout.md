# Workspace Layout Implementation Plan

> Execute sequentially in the current workspace using executing-plans; the user explicitly approved in-place execution and retaining branch develop.

**Goal:** Three work directories: legacy WPF, upgraded IRM, and delivery packages, under one existing Git repository rooted at NNN.

**Architecture:** Move existing repository metadata without reinitializing or rewriting history. Preserve the legacy application and business/database implementation; update only layout, build configuration and documentation.

**Tech Stack:** Git, PowerShell, .NET 8, GitHub Actions, Docker.

**Spec:** `docs/superpowers/specs/2026-11-10-workspace-layout-design.md` before relocation; `IRM/docs/superpowers/specs/2026-11-10-workspace-layout-design.md` afterwards.

## Global constraints and review focus

- Preserve branch develop, history, remotes, 146 tracked WPF files, migrations, baselines, executable identity and all existing delivery binaries.
- Do not alter business code, schema, dependencies, installed services or production data. Do not push or deploy.
- Root Git directory differs from solution directory: all CI, artifact paths, packaging and documentation must account for this.
- Hidden files and ignored runtime state must not be lost or accidentally staged. Do not follow filesystem junctions while auditing.
- Locked Excel workbooks remain preserved and are reported if they cannot move. Do not close applications or disable Windows Application Control.

## Task 1: Capture the baseline

- [ ] Record HEAD, branch, Git config and SHA-256 of tracked files and existing delivery files in a temporary audit directory outside the source.
- [ ] Back up Git history using a bundle; check clean status and destination conflicts.
- [ ] Save this plan, mark the design approved, and commit documentation before moving.
- [ ] Run a temporary structural check; observe that the target root and new solution path are not present yet.

## Task 2: Relocate the existing workspace

- [ ] Rename the current outer source directory to IRM; extract its nested WPF directory into the vacated legacy directory at workspace root.
- [ ] Move the existing .git, .github and .gitignore to workspace root. Preserve the index and history; do not initialize a new repository.
- [ ] Move TestExcelGen and mockup-demo into IRM/reference. Keep application projects, deploy, docs, configuration, caches and runtime state under IRM.
- [ ] Consolidate remaining output support under BAN-GIAO/05-HO-TRO if Windows permits an ordinary same-volume move; otherwise preserve and report the locked item.
- [ ] Verify root, branch, old commits, legacy file count and byte fingerprints immediately after the move.

## Task 3: Update integrations and verify

- [ ] Update root/project AGENTS and READMEs, ignore rules, current delivery links, and architecture documentation.
- [ ] Set the appropriate IRM working directory in CI/release jobs; update artifact upload paths, Docker context and reference demo path.
- [ ] Preserve solution-relative project paths and packaging source revision checks; keep candidates under BAN-GIAO/04-CHO-NGHIEM-THU.
- [ ] Run structural/fingerprint/ignore checks; restore, build Release, test .NET, and verify PowerShell x64/x86. Check Docker if available; record unavailable tools or policy restrictions accurately.
- [ ] Commit the complete move and integration changes; obtain one independent read-only review of the full change, fix material findings with focused verification, and leave a clean tree.

## Acceptance

The upgraded solution opens at IRM/IRM.sln. Legacy and protected business/database files retain original bytes. Original delivery checksums pass. Git uses the existing history from NNN and all completed source/config/documentation changes are committed. Report any environment-blocked checks or locked workbook exception explicitly.

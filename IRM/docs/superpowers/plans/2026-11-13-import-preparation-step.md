# Import Preparation Step Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship the first stage of the approved A flow: template preparation, a correct 35-field workbook and a safe handoff to the existing import workflow.

**Architecture:** Preserve internal stages 1–4 for upload/mapping/preview/result and introduce stage 0 for preparation; the four visible steps map 0→1, 1/2→2, 3→3, 4→4. Share the existing field help metadata in Application so Infrastructure can build the workbook without depending on Web. Keep parsing, persistence and rollback behavior intact.

**Tech Stack:** .NET 8, Blazor Server, MudBlazor, ClosedXML, EF Core, existing local Driver.js 1.12.1, xUnit and Node test runner.

**Spec:** `IRM/docs/superpowers/specs/2026-11-13-import-template-first-design.md` (paths below relative to the Git root).

**Security amendment, 13/11/2026:** The user requires confidentiality of existing operational data. The completed task descriptions below record the original implementation. The current design supersedes their target-catalog export requirement. Template generation must use static guidance and supported technical codes exclusively, with zero database opens. Agent UI checks and screenshots must use isolated synthetic data. Regression coverage is in `Sample_GenerationNeverOpensTheOperationalDatabase` and `Sample_DoesNotCopyExistingTargetCatalogsIntoTheDownloadedTemplate`.

## Global Constraints

- User approved A and explicitly requested inline execution on 13/11/2026; execute this first-stage plan without another permission round.
- Work in the existing single Git root on `develop`; preserve the branch and legacy WPF source.
- Only first-stage preparation ships here; do not claim the existing tabular preview is the new profile preview.
- Keep all 35 import fields and the existing nine required fields; preserve engine semantics, schema, authorization and library versions.
- Do not issue or generate official identity, document or permit numbers.
- Build templates from static guidance and independent examples, with zero operational database access and an empty import region. Users consult authorized catalogs inside the application.
- Share reviewed field guidance between Web and workbook, and preserve Vietnamese accents.
- Stage/review/check/commit locally per deliverable; do not push or rebuild existing handed-over installers.

## Review Focus

1. Optional empty columns in the complete template can affect existing records: guidance must explain blank versus unmapped; no silent change to update semantics.
2. Reordered/removed optional headers must still map correctly; exact known labels must take priority over ambiguous substring matches without reusing a system field.
3. Empty catalogs, deleted entries and different legacy codes must produce truthful references rather than invented working examples.
4. An interrupted or failed download must leave preparation usable and must not advance the wizard or claim the browser saved the file.
5. Stage 0 must integrate with reset, manual mode and Driver transitions without breaking existing internal stages 1–4.

## Files and responsibilities

- Move `IRM/IRM/Features/ImportExport/Help/ImportFieldHelpCatalog.cs` to `IRM/IRM.Application/Features/ImportExport/ImportFieldHelpCatalog.cs`, retaining its public namespace/API: shared, presentation-only metadata, no parser dependency.
- New `IRM/IRM.Infrastructure/Excel/ImportSampleWorkbook.cs`: pure ClosedXML workbook formatting from help metadata and materialized catalog rows.
- Modify `SqlExportStore.cs`: replace obsolete sample with async catalog projection, keeping the DbContext factory boundary.
- Modify Application `IExportService.cs`, `Abstractions/Persistence/IExportStore.cs`, `ExportApplicationService.cs`: `Task<byte[]> GenerateSampleExcelBytesAsync()` instead of synchronous sample operation; only Web caller updated in Task 2.
- Modify Application `ImportMapping.cs`: resolve exact `SystemFields.All` labels before existing keyword fallback.
- New `IRM/IRM.Tests/ImportSampleWorkbookTests.cs`: workbook contract, real reader/mapping/parser round-trip and catalog filtering.
- New `IRM/IRM/Features/ImportExport/Components/ImportPreparationSection.razor`: preparation view, no data access.
- Modify `Import.razor`, `.razor.cs`: preparation state/navigation and async download.
- Modify upload section to expose a back-to-preparation action; retain the rest for this increment.
- Modify `wwwroot/js/import-tour.js`, `Components/App.razor` cache token and browser fixture/tests for preparation anchors.
- New `IRM/IRM.Tests/ImportPreparationTests.cs`: rendered preparation and state transitions/download error recovery.
- Update current import guide and verification documentation to distinguish shipped preparation from future redesign stages.

### Task 1: Correct downloadable template and deterministic header matching

**Files:** Shared metadata move, Application export contracts/service and mapping, Infrastructure workbook/export, Web async caller, `ImportSampleWorkbookTests.cs`.

**Interfaces:**
- Consumes: existing `ImportFieldHelpCatalog.All`, `SystemFields.All`, `IrmDbContext` and `ExcelReaderHelper`.
- Produces: `Task<byte[]> IExportService.GenerateSampleExcelBytesAsync()` / same store method; workbook sheets `Hồ sơ`, `Hướng dẫn`, `Danh mục tham chiếu`.
- Internal workbook helper: `ImportSampleWorkbook.Create(IReadOnlyList<ImportSampleCatalogRow> catalogs, DateTime exportedAt)` returns `byte[]`; catalog row holds Group, Code, Name, Context strings. No entities or DbContext escape the store.
- Required column order: StaffName, Passport, Gender, Birthday, Nationality, Address, Career, VisaNumber, TemporaryStay; remaining keys follow SystemFields order excluding `skip` and those nine.

- [x] **Step 1: Write failing workbook tests against the existing sample method.** Assert 35 distinct headings map in order to literal expected keys; first sheet has only a header; required nine lead; code/document columns use Text formatting; guide includes 35 field descriptions and blank/update warnings; no formulas/sample person rows. Add a literal row with nine required values and real fixture catalogs, then use real reader + mapper + parser to prove importability. Add reversed/subset labels to catch ambiguous header regressions. Assert the reference sheet includes active legacy code `D` and excludes a deleted nationality/ward/career/company and deleted administrative unit; empty database references have an explicit no-data message.
- [x] **Step 2: Run `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --filter FullyQualifiedName~ImportSampleWorkbookTests` from `IRM/`, redirecting output to this plan's workspace.** Expected: workbook assertions fail against the 19-column sample, before production changes. Compilation errors are not RED evidence.
- [x] **Step 3: Move metadata unchanged, implement workbook helper and materialized async catalog reads, update service contracts and caller.** Reuse only the codes accepted by the existing writer for stay/document reference entries. Add instruction-only examples; keep the data sheet empty below row 1. Use shared metadata for required flags, conditions, checks and empty behavior. Catch no catalog exceptions silently; download failures flow to existing user-visible handling.
- [x] **Step 4: Give exact known field labels precedence in `ImportMapping.AutoMapColumns(List<ExcelColumnInfo>)`, preserving used-field protection and existing keyword fallback.** The tests must cover reordered full headers and a required-only workbook, not depend on output from the helper to construct expected keys.
- [x] **Step 5: Run workbook tests and all existing mapping/parser tests; then the full .NET suite with isolated SQL configured.** Expected: all pass, no skipped SQL tests. Run `git diff --check`, review staged source/test diff and commit `feat: provide a complete and documented import template`.

### Task 2: Preparation screen, navigation and contextual tour

**Files:** Preparation component, import page/code-behind/upload back action, tour JS/fixture/tests, App cache token, preparation tests, current user guide/verification docs.

**Interfaces:**
- Consumes Task 1 async sample export, existing `ShowFieldHelpAsync`, download JS bridge and field metadata.
- Produces internal stage 0 as initial/reset; `GoToUpload()` enters internal stage 1; `GoToPreparation()` returns to 0 without deleting the current file; visible step property maps stages as described in Architecture.
- Preparation tour anchors: `prepare-download`, `prepare-fields`, `prepare-next`; the last is an action step that waits for the real button. Snapshot keeps its four existing properties, with `stage = 0` supported.
- Download uses `_isDownloadingSample` to prevent repeated parallel requests; resets it in `finally`, catches errors through snackbar, never changes stage and never marks local-save completion.

- [x] **Step 1: Add failing tests for initial/reset preparation, navigation from preparation to existing upload and back, and four visible step mapping.** Render the preparation with real MudBlazor: download, field-help and continue controls present, required fields/blank warning present, file picker and mapping controls absent. Invoke navigation with the existing renderer pattern to verify the real state change.
- [x] **Step 2: Add failing tour test starting stage 0 then syncing stage 1.** Assert first hint targets `prepare-download`, last hint waits for app action, no clicks/network/storage, transition releases old resources and starts the upload chapter. Test download failure recovery with an export failure double at the actual external/service boundary and the attached renderer; assert stage stays 0 and the busy control is released.
- [x] **Step 3: Run preparation .NET and Node tour tests.** Expected: failures because initial stage is upload, preparation view/anchors and stage-0 tour do not yet exist. For new types/signatures, use reflection on existing page or initially add render assertions in existing component tests so RED is an assertion, not a compile error.
- [x] **Step 4: Implement preparation UI and state transitions.** Reuse MudBlazor styling and responsive grid; four non-clickable progress steps. Put existing technical upload/mapping screens under visible step 2. Keep existing preview/result intact and label the current increment truthfully. Add an upload back action, keep download elsewhere as secondary, restore preparation on full wizard reset. Retain manual/history access and existing import state.
- [x] **Step 5: Add stage-0 tour chapter and cache token, preserving stages 1–4 and graceful failure.** Use fixed Vietnamese copy and no dynamic unencoded file/person strings. Explain that download was requested, not guaranteed saved on the user's device.
- [x] **Step 6: Run preparation tests and all tour tests, then build Release, full .NET suite and all Node browser tests.** Expected: all pass; fix any failures before claiming the task complete. Update quick guide and current verification evidence with what shipped and what remains for later steps. Review/stage/check/commit `feat: start Excel import with template preparation`.

## Final verification and handoff

- Run each task through `task-start` / `task-done` and record deviations in this plan's ledger. Use fresh whole-branch review once all tasks are complete, with the design, this plan, Review Focus and ledger rulings.
- Address Critical/Important findings once with RED→GREEN evidence; record lower-grade findings as required by executing-plans.
- Leave normal local server running on `http://localhost:5050` with rebuilt source if it was running at task start. Stop/restart only this workspace's app process to release build locks; do not touch unrelated apps or production data.
- Try actual preparation/download UI through available supported browser tools; if unavailable, report that boundary explicitly rather than claiming visual acceptance. Do not repeat a blocked computer-use operation through a different automation channel.
- Do not produce an installer. No package revision claims. Report source commits, passed checks and that profile-style preview belongs to the next increments.

# Workspace guide

- This directory (`NNN/`) is the single Git root. Preserve its existing history and branch; do not initialize nested repositories.
- `immigration-reportmanager-master/` is the original WPF application (`ReportManager.sln`). Preserve it as the legacy source of database/business compatibility; change it only when the task explicitly concerns the old application.
- `IRM/` is the upgraded web solution. Read `IRM/AGENTS.md` before changing it. Run .NET/EF commands from `IRM/`; the solution is `IRM/IRM.sln`. Paths within the project guide are relative to that solution directory.
- `IRM/reference/` contains the Excel experiment and demo; neither belongs in the upgraded solution build.
- All delivery packages and test workbooks belong under `BAN-GIAO/`; start with `BAN-GIAO/README.md`. This directory is ignored by Git.
- `BAN-GIAO/01-HIEN-HANH/` contains the latest existing installer; `02-LUU-TRU/` preserves older packages; `03-DU-LIEU-TEST/` contains corrected test data; `04-CHO-NGHIEM-THU/` is for new candidates; `05-HO-TRO/` keeps supporting files.
- After authorized source changes, run the required checks, review the staged diff and create local Git commits before finishing. Do not push unless asked. Do not commit installer binaries, runtime databases, data-protection keys, logs or credentials.
- Record the source commit and checksums for new builds. Existing handed-over installers must not be relabeled as containing later source changes.

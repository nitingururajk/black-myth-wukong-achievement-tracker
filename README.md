# Black Myth: Wukong Achievement Tracker

This repo contains a save-file tracker for Black Myth: Wukong.

- `bmw_web`: the recommended browser UI for checking achievement progress
- `bmw_probe`: an optional CLI that writes JSON and Markdown reports
- `bmw.sln`: root solution that includes both projects
- `vendor/blackwukong-dlls/`: vendored decoder/runtime DLLs required by both projects

## What It Does

The tracker reads a `.sav` file, decodes the achievement data, and builds a player-facing checklist.

- Shows overall achievement progress from the save
- Highlights unfinished achievements
- For tracked collection achievements, shows the exact missing items still absent from the decoded save
- Uses English item and achievement names in the web UI

Tracked collection checklists currently include achievements such as curios, soaks, seeds, armor pieces, and weapons.

## Web App

Run the web app:

```powershell
dotnet run --project .\bmw_web\bmw_web.csproj
```

Or use the helper script:

```powershell
.\run-web.ps1
```

Then open the local URL printed in the terminal.

In the UI:

1. Paste the full path to your save file.
2. Click `Analyze`.
3. Review:
   - the overview panel
   - the missing item tracker
   - the remaining achievements list
   - the full achievement table

Example save file path:

```text
<game-install-or-save-root>\b1\Saved\SaveGames\<player-id>\ArchiveSaveFile.<slot>.sav
```

## CLI

Run the CLI directly:

```powershell
dotnet run --project .\bmw_probe\bmw_probe.csproj -- --save "<full-path-to-save>" --out ".\bmw_probe\output"
```

Or use the helper script:

```powershell
.\run-planner.ps1 -SavePath "<full-path-to-save>" -OutDir ".\bmw_probe\output"
```

CLI output files:

- `bmw_probe/output/achievement-plan.json`
- `bmw_probe/output/achievement-plan.md`

## Build

Build both projects from the solution root:

```powershell
dotnet build .\bmw.sln
```

## Vendored Dependency

This repo vendors the decoder DLL set used to read Black Myth: Wukong saves.

- Local path: `vendor/blackwukong-dlls/`
- Upstream reference: `https://github.com/BlameTwo/BlackWukongSaveEditer`

Only the required DLLs are kept in-repo. The full upstream project is not needed to build this solution.
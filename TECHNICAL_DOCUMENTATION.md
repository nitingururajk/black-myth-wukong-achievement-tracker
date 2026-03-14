# Technical Documentation

## Overview

This repository is a .NET 8 save analysis tool for Black Myth: Wukong. It reads a game `.sav` file, decodes the archive payload with vendored game-specific runtime assemblies, extracts achievement and inventory state, and presents the result in two forms:

- `bmw_web`: a browser-based achievement tracker
- `bmw_probe`: a CLI report generator

The two applications share the same core idea but are implemented separately:

- the web app includes the richer tracking logic for collection achievements
- the CLI currently provides a lighter achievement summary and Markdown/JSON export

## Repository Layout

- `bmw.sln`: root solution containing `bmw_web` and `bmw_probe`
- `bmw_web/`: ASP.NET Core web application
- `bmw_probe/`: console application for report generation
- `vendor/blackwukong-dlls/`: vendored third-party decoder/runtime DLLs required to parse save data
- `run-web.ps1`: helper script to start the web app
- `run-planner.ps1`: helper script to start the CLI analyzer
- `README.md`: user-facing usage notes

`external/` is currently empty and no longer required for build or runtime.

## End-to-End Data Flow

The main runtime flow is:

1. A user provides the path to a `.sav` file.
2. The application reads the raw save bytes from disk.
3. The raw bytes are parsed as an `ArchiveFile` protobuf container.
4. The inner archive payload is extracted from `GameArchivesDataBytes`.
5. The vendored decoder runtime deserializes that payload into `FUStBEDArchivesData`.
6. The application reads player state, chapter/map progress, achievements, and selected inventory/equipment data.
7. The raw decoded data is transformed into an application-level `AnalysisReport`.
8. The web app renders that report as interactive UI; the CLI writes it to JSON and Markdown.

## Vendored External Module

### What It Is

The save decoding logic depends on compiled DLLs vendored in `vendor/blackwukong-dlls/`.

These DLLs are sourced from the upstream project:

- `https://github.com/BlameTwo/BlackWukongSaveEditer`

The repository no longer vendors that full upstream project. Instead, it keeps only the compiled binaries required for deterministic local builds.

### Why It Is Needed

Black Myth: Wukong save files are not plain JSON or simple custom text formats. The save content is wrapped in a protobuf archive and uses game-specific runtime types. The tracker relies on the vendored assemblies to:

- deserialize the archive container
- deserialize the game archive payload into strongly typed objects
- expose game-specific data structures such as role state, achievements, chapter data, and persistent ECS data

Without these DLLs, the app would not understand the save schema.

### How It Works in This Repo

Both project files reference the vendored DLL directory directly:

- `bmw_web/bmw_web.csproj`
- `bmw_probe/bmw_probe.csproj`

The important runtime usage pattern is:

1. `ArchiveFile` is created and populated with `MergeFrom(bytes)`.
2. The nested byte payload is read from `archiveFile.GameArchivesDataBytes`.
3. `BGW_GameArchiveMgr.DeserializeArchiveDataFromBytes<FUStBEDArchivesData>(true, contentBytes)` converts the payload into a game archive object graph.

The key namespaces/types used from the vendored binaries are:

- `ArchiveB1`
- `b1`
- `Google.Protobuf`
- `ArchiveFile`
- `FUStBEDArchivesData`
- `BGW_GameArchiveMgr`

## Web App Architecture

### Entry Point

File:

- `bmw_web/Program.cs`

Responsibilities:

- configures ASP.NET Core minimal hosting
- configures JSON serialization defaults
- registers `AchievementPlanner` as a singleton service
- serves static files from `wwwroot`
- exposes two HTTP endpoints:
  - `GET /api/health`
  - `POST /api/analyze`
- configures console logging

### API Endpoints

#### `GET /api/health`

Returns a simple `{ ok = true }` response for health checks.

#### `POST /api/analyze`

Accepts a JSON payload matching `AnalyzeRequest`:

```json
{
  "savePath": "<full-path-to-save>"
}
```

Execution flow:

1. validate that `savePath` is present
2. start request timing/log scope
3. call `AchievementPlanner.AnalyzeAsync(savePath)`
4. return `{ ok: true, report }` on success
5. return a bad request with a readable error for missing file or parse failure

### Logging

The web app uses console logging with timestamps. Logging currently exists at two layers:

- request-level logging in `bmw_web/Program.cs`
- analysis-level logging in `bmw_web/Services/AchievementPlanner.cs`

Logged events include:

- application startup
- rejected analyze requests with no save path
- analyze request start and completion timing
- file-not-found failures
- unexpected parse failures
- bytes loaded from disk
- decoded player/chapter/map context
- final report summary with tracked checklist counts

## `AchievementPlanner` Service

File:

- `bmw_web/Services/AchievementPlanner.cs`

This is the main backend component in the repository.

### Primary Responsibilities

- validate the save path
- read and decode the save file
- extract player and achievement state
- extract owned inventory/equipment IDs from decoded save data
- enrich raw achievement data with curated knowledge for specific achievements
- compute missing tracked items for selected collection achievements
- return a normalized `AnalysisReport`

### Major Internal Sections

#### Achievement Name Map

`AchievementNameMap` maps known platform achievement IDs to stable English names.

Purpose:

- avoid raw fallback names where possible
- keep user-visible titles consistent

#### Achievement Knowledge Map

`AchievementKnowledgeMap` contains curated metadata for specific achievements that need more than raw counter progress.

Each knowledge entry can define:

- `TargetSource`: where collection state should come from
- `DisplayTitleOverride`: more specific user-facing title text
- `RouteHintOverride`: more useful location guidance
- `Targets`: the checklist for that achievement

Examples of tracked achievements:

- `Seeds to Sow`
- `A Family Finished`
- `A Curious Collection`
- `With Full Spirit`
- `Mantled with Might`
- `Staffs and Spears`
- `Final Fulfillment`

#### `AnalyzeAsync`

`AnalyzeAsync` is the main analysis pipeline.

High-level behavior:

1. validate save path
2. read bytes from disk
3. protobuf-parse the outer save archive
4. deserialize the inner archive payload
5. extract chapter/map/player state
6. collect owned item/equipment IDs
7. read raw achievement entries from decoded save data
8. transform each achievement into an `AchievementPlan`
9. prefer platform-only achievements if IDs `>= 81000` exist
10. package everything into `AnalysisReport`

### Achievement Transformation Logic

For every decoded achievement entry, the planner computes:

- `AchievementId`
- `RequirementType`
- `RequiredCount`
- `CompletedCount`
- `RemainingCount`
- `PriorityOrder` and `PriorityLabel`
- `RouteHint`
- `Steps`
- optional tracked checklist state

This is the data the frontend renders.

### Platform Achievement Filtering

The planner prefers achievement IDs `>= 81000` when they exist.

Why:

- the save may contain additional internal or duplicate achievement entries
- platform IDs give a cleaner user-facing set of 80 achievements

When platform achievements exist:

- `FilterMode = "platform_only"`

Otherwise:

- `FilterMode = "all"`

### Missing Item Tracking Logic

This is the most specialized logic in the repo.

For tracked achievements, the planner can resolve ownership from one of two sources:

- `AchievementRequirements`: use `CompleteRequirementList`
- `DecodedSaveInventory`: use collected item/equipment IDs from the decoded save object graph

This distinction matters because some collection achievements are not reliable if you only inspect the raw achievement completion list.

For example:

- curios
- soaks
- armor pieces
- weapons

These use `DecodedSaveInventory`, not only achievement requirement progress.

### Inventory/Equipment Ownership Extraction

Method:

- `CollectOwnedIds`

This method gathers integer IDs from multiple save locations. It uses reflection because some save sub-objects are not exposed with stable compile-time properties across the vendored assemblies.

It checks fields such as:

- `Bag`
- `Equip`
- `EquipList`
- `CanActivateEquipList`
- `Accessorylist`
- `WearAccessory`
- `WearEquip`

It then recursively walks nested objects and collections with `AddIdsFromObject`.

Recognized ID properties include:

- `ItemId`
- `EquipId`
- `OwningItemId`

Important implementation details:

- recursion depth is capped at 5
- strings are ignored
- primitive/enum leaf values are ignored
- property getter exceptions are swallowed so partially inaccessible objects do not break analysis

This makes ownership extraction resilient to schema differences while still finding most useful inventory/equipment nodes.

### Knowledge Resolution

Method:

- `GetKnowledge`

This method merges a knowledge entry with actual completion state.

Behavior:

1. look up the knowledge entry for the achievement ID
2. choose the completion source based on `TargetSource`
3. build `RequirementTarget` objects
4. mark each target as collected or missing
5. return the enriched checklist result

### Output Models

Important backend DTOs:

- `AnalyzeRequest`: input payload for the web API
- `AnalysisReport`: top-level response model
- `AchievementPlan`: per-achievement derived state
- `RequirementTarget`: individual tracked collectible/equipment target
- `AchievementKnowledge`: curated metadata definition
- `AchievementKnowledgeResult`: resolved checklist state after applying save data

## Frontend Architecture

Files:

- `bmw_web/wwwroot/index.html`
- `bmw_web/wwwroot/app.js`
- `bmw_web/wwwroot/styles.css`

### `index.html`

Defines the page structure:

- hero header
- save path input panel
- status panel
- overview panel with progress ring
- missing item tracker panel
- remaining achievements panel
- full achievement table with tabs and search

The page is static and relies on `app.js` for all behavior.

### `app.js`

This is the client-side controller.

Main responsibilities:

- wire button and keyboard events
- call `/api/analyze`
- handle API success/failure states
- cache the current `AnalysisReport`
- render every UI section from the report

Main render functions:

- `renderOverview(report)`
- `renderItemTracker(report)`
- `renderActionPlan(report)`
- `renderFullTable(report)`
- `renderTargetBlock(item)`

Important client-side behavior:

- incomplete tracked items are grouped into the Missing Item Tracker
- remaining achievements are sorted by priority and remaining count
- meta achievements are pushed to the end of the remaining list
- search filters only by visible achievement title
- all user-rendered text is escaped through `esc()` before insertion

### `styles.css`

This file defines the visual presentation for:

- hero section
- cards and panels
- progress ring
- missing item tracker grid
- remaining achievement cards
- checklist blocks
- filter tabs and table
- responsive mobile layout

The current visual system uses:

- dark green/bronze palette
- layered translucent panels
- large blurred background shapes
- compact card-based hierarchy for checklist items

## CLI Architecture

File:

- `bmw_probe/Program.cs`

The CLI is a separate console implementation that repeats core save parsing locally instead of calling the web service.

### Responsibilities

- parse command-line arguments
- prompt for a save path if missing
- decode the save
- build an `AnalysisReport`
- write JSON and Markdown output files
- print a short terminal summary

### CLI Inputs

Supported arguments:

- `--save <path>`
- `--out <directory>`

Helper script:

- `run-planner.ps1`

Default output path in the helper script is now:

- `./bmw_probe/output`

### CLI Outputs

Generated files:

- `achievement-plan.json`
- `achievement-plan.md`

The CLI console summary includes:

- player snapshot
- current chapter/map
- filter mode
- overall completion totals
- output file paths
- top remaining achievements

### Difference From the Web App

The CLI and web app do not yet share one core library.

Current difference:

- the web app has richer achievement knowledge and decoded inventory-based missing item tracking
- the CLI currently produces a simpler achievement-centric report without the full tracked item knowledge layer

This means the web app is the more accurate and feature-complete experience for collectible breakdowns.

## Scripts

### `run-web.ps1`

Runs:

```powershell
dotnet run --project ".\bmw_web\bmw_web.csproj"
```

### `run-planner.ps1`

Prompts for a save path if one is not provided, then runs:

```powershell
dotnet run --project ".\bmw_probe\bmw_probe.csproj" -- --save "$SavePath" --out "$OutDir"
```

## Build and Runtime Requirements

- .NET 8 SDK
- the vendored DLLs in `vendor/blackwukong-dlls/`
- a valid Black Myth: Wukong `.sav` file path accessible from the current machine

## Known Design Tradeoffs

### Reflection-Based Ownership Scanning

Pros:

- resilient to partial schema opacity
- works even when some properties are not strongly typed in referenced assemblies

Cons:

- not compile-time safe
- may miss data if future save schema changes rename important properties
- harder to test than a strongly typed extractor

### Duplicated Parsing Logic Between Web and CLI

Pros:

- simple deployment model
- CLI stays standalone

Cons:

- behavior can drift between web and CLI
- fixes must be applied twice

A good future improvement would be extracting a shared core library for decoding, plan generation, and report modeling.

### Curated Knowledge Lists

Pros:

- gives player-meaningful names and acquisition hints
- supports exact missing item checklists for targeted achievements

Cons:

- requires ongoing maintenance if IDs, names, or acquisition notes are corrected
- some route hints are hand-maintained rather than generated from authoritative data files

## Recommended Future Improvements

- extract shared core analysis logic into a library used by both `bmw_web` and `bmw_probe`
- add tests around `GetKnowledge`, `CollectOwnedIds`, and report filtering
- add a machine-readable source file for achievement knowledge instead of a large hardcoded map
- optionally add a richer CLI mode that mirrors the web app missing item tracker
- document the exact vendored DLL update procedure in `README.md`

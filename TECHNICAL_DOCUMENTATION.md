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
- `Dockerfile`: multi-stage container build for the web app
- `.dockerignore`: trims the Docker build context
- `bmw_web/`: ASP.NET Core web application
- `bmw_probe/`: console application for report generation
- `vendor/blackwukong-dlls/`: vendored third-party decoder/runtime DLLs required to parse save data
- `run-planner.ps1`: helper script to start the CLI analyzer
- `README.md`: user-facing usage notes

`external/` is currently empty and no longer required for build or runtime.

## End-to-End Data Flow

The main runtime flow is:

1. A user uploads a `.sav` file in the browser UI, or a caller provides a server-local path to the API.
2. The application reads the raw save bytes from the upload or from disk.
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

Accepts either:

- multipart form-data with a `saveFile` upload field
- a legacy JSON payload matching `AnalyzeRequest`

Execution flow:

1. validate that an uploaded file or `savePath` is present
2. enforce a basic upload size limit for browser uploads
3. start request timing/log scope
4. call `AchievementPlanner.AnalyzeUploadedSave(...)` for uploads or `AchievementPlanner.AnalyzeAsync(savePath)` for server-local paths
5. return `{ ok: true, report, analyzedAtUtc, saveFileName, saveFileLastWriteTimeUtc }` on success
6. return a bad request with a readable error for missing file or parse failure

The analyze response is explicitly marked `no-store` so repeated button presses always hit the server again instead of reusing a cached response.

### Logging

The web app uses console logging with timestamps. Logging currently exists at two layers:

- request-level logging in `bmw_web/Program.cs`
- analysis-level logging in `bmw_web/Services/AchievementPlanner.cs`

Logged events include:

- application startup
- rejected analyze requests with no uploaded file or save path
- analyze request start and completion timing
- file-not-found failures
- unexpected parse failures
- bytes loaded from disk
- decoded player/chapter/map context
- final report summary with tracked checklist counts

### Container Packaging

The repository now includes a root `Dockerfile` that packages `bmw_web` as a multi-stage ASP.NET Core container image.

Container build flow:

1. copy `bmw_web/bmw_web.csproj`
2. copy `vendor/blackwukong-dlls/`
3. run `dotnet restore`
4. copy the rest of `bmw_web/`
5. run `dotnet publish -c Release`
6. copy the publish output into the final ASP.NET runtime image

Container runtime details:

- the image serves the web app on port `8080`
- the browser upload flow means no host save-path mount is required for normal use
- the vendored decoder DLLs are included through the published web app output

## `AchievementPlanner` Service

File:

- `bmw_web/Services/AchievementPlanner.cs`

This is the main backend component in the repository.

### Primary Responsibilities

- validate uploaded save input or server-local save path
- read and decode the save file bytes
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
- `Scenic Seeker`
- `With Full Spirit`
- `Treasure Trove`
- `Full of Forms`
- `A Curious Collection`
- `The Five Skandhas`
- `Medicine Meal`
- `Portraits Perfected`
- `Master of Magic`
- `Page Preserver`
- `Brewer's Bounty`
- `Mantled with Might`
- `Staffs and Spears`
- `Final Fulfillment`

#### `AnalyzeAsync`

`AnalyzeAsync` and `AnalyzeUploadedSave` feed the same analysis pipeline.

High-level behavior:

1. validate the save source
2. read bytes from disk or accept uploaded bytes
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

For tracked achievements, the planner can resolve ownership from three sources:

- `AchievementRequirements`: use `CompleteRequirementList`
- `DecodedSaveInventory`: use collected item/equipment IDs from the decoded save object graph
- `LinkedAchievementRequirements`: use another achievement's `CompleteRequirementList` for each tracked target

This distinction matters because some collection achievements are not reliable if you only inspect the raw achievement completion list.

For example:

- curios
- armor pieces
- weapons

These use `DecodedSaveInventory`, not only achievement requirement progress.

Composite trophies can also be tracked from related achievement buckets instead of raw inventory items.

For example:

- `Brewer's Bounty` now expands to the actual drink, soak, and gourd checklist
- each target points at the correct source achievement bucket
- drinks use `81064` (`Brews and Barrels`)
- soaks use `81078` (`Brewer's Bounty`)
- gourds use `81076` (`Gourds Gathered`)
- source-backed soak IDs are labeled directly, while unresolved soak IDs stay generic instead of reusing a guessed item name

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

It walks known root save nodes and then recursively traverses nested objects and collections with `AddIdsFromKnownNode`.

Recognized ID properties include:

- `ItemId`
- `EquipId`
- `OwningItemId`

Important implementation details:

- recursion depth is capped at 5
- strings are ignored
- primitive/enum leaf values are ignored
- property getter exceptions are swallowed so partially inaccessible objects do not break analysis

This keeps the extractor resilient to schema differences while still finding most useful inventory/equipment nodes.

### Knowledge Resolution

Method:

- `GetKnowledge`

This method merges a knowledge entry with actual completion state.

Behavior:

1. look up the knowledge entry for the achievement ID
2. choose the completion source based on `TargetSource`
3. for inventory-backed checklists, union decoded owned IDs with the achievement's own completed requirement IDs so tracker output stays aligned when reflective inventory scanning misses a save node
4. build `RequirementTarget` objects
5. mark each target as collected or missing
6. return the enriched checklist result

### Output Models

Important backend DTOs:

- `AnalyzeRequest`: legacy JSON input payload for the web API
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
- save file upload panel
- status panel
- overview panel with progress ring
- missing item tracker panel
- remaining achievements panel
- full achievement table with tabs and search

The page is static and relies on `app.js` for all behavior.

### `app.js`

This is the client-side controller.

Main responsibilities:

- wire button and filter/search events
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
- each analyze click uses a fresh `no-store` request and ignores older in-flight responses
- the browser uploads the selected `.sav` file with `FormData` instead of sending a client-local path
- the status panel shows the analysis timestamp and, when available from the browser file metadata, the selected file's last-modified timestamp so reruns are visible even when the achievement counts do not change
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

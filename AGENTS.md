# AGENTS Guide

## Purpose

This file gives coding agents the minimum repo-specific context needed to work safely in this repository.

The repo is a .NET 8 save analysis tool for Black Myth: Wukong.

- `bmw_web`: ASP.NET Core web UI and API
- `bmw_probe`: CLI report generator
- `vendor/blackwukong-dlls/`: required vendored decoder/runtime DLLs

## High-Level Architecture

- The web app is the main product and has the richer achievement knowledge and missing item tracking logic.
- The web app is designed to analyze a browser-uploaded `.sav` file; do not assume a hosted user can provide a meaningful local filesystem path.
- The CLI is simpler and does not yet share a common analysis library with the web app.
- Both projects depend on the vendored DLLs in `vendor/blackwukong-dlls/`.
- Do not remove or relocate vendored DLLs unless you also update both `.csproj` files.
- The core backend logic for the web app lives in `bmw_web/Services/AchievementPlanner.cs`.

## Environment Notes

- Target framework: `net8.0`
- Nullable reference types are enabled.
- Implicit usings are enabled.
- The repo currently has no dedicated test project.
- The repo currently has no `.editorconfig`, `Directory.Build.props`, or `global.json`.

## Build Commands

Use these from the repo root.

Restore solution dependencies:

```powershell
dotnet restore .\bmw.sln
```

Build the whole solution:

```powershell
dotnet build .\bmw.sln
```

Build the web app only:

```powershell
dotnet build .\bmw_web\bmw_web.csproj
```

Build the Docker image for the web app:

```powershell
docker build -t bmw-web .
```

Build the CLI only:

```powershell
dotnet build .\bmw_probe\bmw_probe.csproj
```

Build with warnings treated as errors:

```powershell
dotnet build .\bmw_web\bmw_web.csproj -warnaserror
dotnet build .\bmw_probe\bmw_probe.csproj -warnaserror
```

If the web app DLL is locked because the app is running, either stop the running process or build projects individually to isolated output folders:

```powershell
dotnet build .\bmw_web\bmw_web.csproj -o C:\temp\bmw_web_build
dotnet build .\bmw_probe\bmw_probe.csproj -o C:\temp\bmw_probe_build
```

## Run Commands

Run the web app:

```powershell
dotnet run --project .\bmw_web\bmw_web.csproj
```

Run the web app in Docker:

```powershell
docker run --rm -p 8080:8080 bmw-web
```

Helper script for the web app:

```powershell
.\run-web.ps1
```

Run the CLI:

```powershell
dotnet run --project .\bmw_probe\bmw_probe.csproj -- --save "<full-path-to-save>" --out ".\bmw_probe\output"
```

Helper script for the CLI:

```powershell
.\run-planner.ps1 -SavePath "<full-path-to-save>" -OutDir ".\bmw_probe\output"
```

## Test Commands

There is no test project in the repository today.

Do not claim tests were run unless you actually add a test project and execute it.

If a test project is added later, use standard .NET commands:

Run all tests in a project:

```powershell
dotnet test .\path\to\Tests.csproj
```

Run a single test by fully qualified name filter:

```powershell
dotnet test .\path\to\Tests.csproj --filter "FullyQualifiedName~Namespace.ClassName.TestName"
```

Run tests by partial name filter:

```powershell
dotnet test .\path\to\Tests.csproj --filter "Name~TestName"
```

## Linting and Validation

There is no separate linter command configured in the repo.

Use build success plus analyzers as the baseline validation step.

Recommended validation after C# changes:

```powershell
dotnet build .\bmw_web\bmw_web.csproj -warnaserror
dotnet build .\bmw_probe\bmw_probe.csproj -warnaserror
```

Recommended validation after frontend-only changes:

- reload the web app manually
- confirm the UI renders without console errors
- confirm `/api/analyze` still works with a real uploaded save file when relevant

## File Ownership and Change Scope

- Prefer fixing issues in the smallest reasonable scope.
- Do not refactor unrelated files while touching one bug.
- Keep the web app and CLI terminology aligned when changing user-facing wording.
- If you update tracked achievement behavior in the web app, review whether `README.md` or `TECHNICAL_DOCUMENTATION.md` also need updates.

## C# Style Guidelines

- Use file-scoped or normal namespace style consistently within the file you edit.
- Keep `using` directives at the top of the file.
- Group framework/usings first, then third-party or project-specific usings if you need to reorder.
- Prefer `var` when the right-hand side makes the type obvious.
- Use explicit types when the type is not obvious from the initializer.
- Use `PascalCase` for types, methods, properties, and records.
- Use `camelCase` for locals and parameters.
- Use `_camelCase` for private readonly fields.
- Keep methods focused; split large logical blocks when it improves readability.
- Prefer early returns for guard clauses and error exits.
- Keep nullable handling explicit and safe.
- Do not suppress nullability warnings casually; fix the flow when possible.

## C# Error Handling Guidelines

- Validate user input at the edges of the system.
- Throw specific exceptions when a failure is exceptional and the caller can handle it.
- Return bad request responses for invalid API input rather than letting a null reference fail later.
- Log failures with useful context, but do not leak sensitive local path details unnecessarily in user-facing text.
- Swallow exceptions only when there is a clear fallback path, as in safe reflective property access.

## ASP.NET Core Guidelines

- Keep API endpoints thin; push analysis logic into services.
- Use DTOs for request/response boundaries.
- Preserve the current `/api/health` and `/api/analyze` endpoint behavior unless intentionally changing the API.
- Keep request logging concise and structured.
- If you add new services, register them in `bmw_web/Program.cs`.

## Frontend JavaScript Guidelines

- Use plain browser JavaScript; there is no frontend framework here.
- Keep DOM element lookups near the top of `app.js`.
- Use `camelCase` for functions and locals.
- Preserve the current render pipeline: fetch -> store report -> render sections.
- Escape user-rendered values before inserting them into HTML.
- Keep user-facing copy concise and player-oriented, not debug-oriented.
- Avoid adding technical identifiers like raw item IDs to the UI unless explicitly requested.

## CSS and UI Guidelines

- Reuse existing CSS variables in `styles.css` before adding new colors or spacing tokens.
- Preserve the current visual direction unless the task explicitly asks for redesign.
- Keep layouts responsive for both desktop and mobile widths.
- Prefer extending existing component classes over introducing many one-off selectors.

## Naming and Domain Conventions

- Use exact English names for achievements and tracked items.
- Prefer player-facing names over internal config identifiers.
- Keep missing-item tracker wording consistent with the web UI.
- Preserve the distinction between raw achievement progress and decoded inventory ownership.

## Dependency Rules

- Do not delete `vendor/blackwukong-dlls/`.
- Do not replace vendored DLLs casually; they are the decoder/runtime dependency for both apps.
- If you must update vendored binaries, document the source and rebuild both projects.
- Keep the root `Dockerfile` aligned with `bmw_web/bmw_web.csproj` and `vendor/blackwukong-dlls/` if paths or project names change.

## Documentation Rules

- Keep `README.md` focused on setup and usage.
- Keep `TECHNICAL_DOCUMENTATION.md` focused on architecture and internals.
- If you change architecture, update `TECHNICAL_DOCUMENTATION.md` in the same task.
- If you change onboarding or commands, update `README.md` and this file in the same task.

## Frontend Formatting Notes

- Keep HTML structure semantic and easy to scan.
- Prefer small, focused DOM render helpers over giant template blocks when extending `app.js`.
- Use existing CSS custom properties before introducing new colors.
- Preserve the current green/bronze visual identity unless the task explicitly asks for redesign.

## CLI Guidelines

- Keep CLI output concise and readable in plain terminals.
- Prefer player-facing wording over raw save-schema terminology.
- Preserve JSON and Markdown report filenames unless the task intentionally changes output contracts.
- If you improve logic in the CLI, check whether the web app already has a richer implementation worth aligning with.

## Save Parsing Guidelines

- Treat vendored DLL-backed model access as potentially brittle.
- Prefer explicit property access when the vendored types expose stable members.
- If reflection is needed, keep it constrained to known nodes and property names.
- Avoid broad recursive object graph walking unless there is no safer alternative.
- When changing ownership extraction or achievement knowledge, verify against a real save if possible.

## Practical Agent Workflow

1. Read the relevant file before editing.
2. Make the smallest safe change.
3. Build the affected project.
4. If behavior changed, update docs.
5. Summarize what changed and how it was validated.

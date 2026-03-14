using System.Text;
using System.Text.Json;
using ArchiveB1;
using b1;
using Google.Protobuf;

var options = CliOptions.Parse(args);
var savePath = options.SavePath;
if (string.IsNullOrWhiteSpace(savePath))
{
    Console.Write("Enter full save path (.sav): ");
    savePath = Console.ReadLine() ?? string.Empty;
}

if (!File.Exists(savePath))
{
    Console.WriteLine($"Save file not found: {savePath}");
    return;
}

var outputDirectory = string.IsNullOrWhiteSpace(options.OutputDirectory)
    ? Directory.GetCurrentDirectory()
    : options.OutputDirectory!;
Directory.CreateDirectory(outputDirectory);

AnalysisReport report;
try
{
    report = await BuildReportAsync(savePath);
}
catch (Exception ex)
{
    Console.WriteLine("Failed to parse save.");
    Console.WriteLine(ex.Message);
    return;
}

var jsonPath = Path.Combine(outputDirectory, "achievement-plan.json");
var markdownPath = Path.Combine(outputDirectory, "achievement-plan.md");

var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(report, jsonOptions));
await File.WriteAllTextAsync(markdownPath, BuildMarkdown(report), Encoding.UTF8);

var incomplete = report.Achievements
    .Where(x => !x.IsComplete)
    .OrderBy(x => x.PriorityOrder)
    .ThenByDescending(x => x.RemainingCount)
    .ThenBy(x => x.AchievementId)
    .ToList();

Console.WriteLine();
Console.WriteLine("Tracker update complete.");
Console.WriteLine($"Player: {report.PlayerName} | Level: {report.PlayerLevel} | NG+: {report.NewGamePlusCount}");
Console.WriteLine($"Current chapter: {report.CurrentChapter} | Map ID: {report.CurrentMapId}");
Console.WriteLine($"View mode: {report.FilterMode} (raw {report.RawCompletedAchievements}/{report.RawAchievementCount})");
Console.WriteLine($"Achievements: {report.CompletedAchievements}/{report.TotalAchievements} complete");
Console.WriteLine("Reports:");
Console.WriteLine($"- {jsonPath}");
Console.WriteLine($"- {markdownPath}");
Console.WriteLine();
Console.WriteLine("Remaining achievements:");
foreach (var item in incomplete.Take(15))
{
    Console.WriteLine(
        $"- {item.DisplayTitle} | {item.CompletedCount}/{item.RequiredCountText} done | {item.RemainingCount} left"
    );
}

static async Task<AnalysisReport> BuildReportAsync(string savePath)
{
    var bytes = await File.ReadAllBytesAsync(savePath);
    IMessage<ArchiveFile> info = new ArchiveFile();
    info.MergeFrom(bytes);
    if (info is not ArchiveFile archiveFile)
        throw new InvalidOperationException("Invalid archive protobuf payload.");

    var contentBytes = archiveFile.GameArchivesDataBytes.ToByteArray();
    var data = BGW_GameArchiveMgr.DeserializeArchiveDataFromBytes<FUStBEDArchivesData>(true, contentBytes);

    var chapter = data.RoleData?.RoleCs?.Chapter?.CurChapter ?? -1;
    var mapId = data.PersistentECSData?.BPCData?.BPCPlayerRoleData?.MapId ?? -1;
    var maxMapId = data.PersistentECSData?.BPCData?.BPCPlayerRoleData?.MaxMapId ?? -1;

    var activeRebirthPoints = data
        .PersistentECSData
        ?.BPCData
        ?.BPCRebirthPointData
        ?.ActivedRebirthPointList
        ?.Where(x => x.HasValue)
        ?.Select(x => x.Value)
        ?.Distinct()
        ?.OrderBy(x => x)
        ?.ToList() ?? new List<int>();

    var achievements =
        data.RoleData?.RoleCs?.Achievement?.Achievements?.ToList() ?? new List<AchievementOne>();
    var plans = new List<AchievementPlan>(achievements.Count);

    for (var index = 0; index < achievements.Count; index++)
    {
        var achievement = achievements[index];
        var config = achievement.Config ?? new AchievementConfig();
        var requirementType = config.RequirementType.ToString();
        var requiredCount = config.RequirementCount;
        var completedCount = achievement.CompleteRequirementList?.Count ?? 0;
        var isComplete = achievement.IsComplete;

        var remaining = isComplete ? 0 : requiredCount > 0 ? Math.Max(requiredCount - completedCount, 0) : 1;
        var priority = PriorityFor(requirementType);
        var context = new RouteContext(chapter, mapId, maxMapId, activeRebirthPoints.Count);
        var steps = BuildStepPlan(requirementType, remaining, requiredCount, completedCount, context);

        plans.Add(
            new AchievementPlan
            {
                Index = index,
                AchievementId = config.AchievementId,
                DisplayTitle = BuildTitle(config.AchievementId, requirementType),
                RequirementType = requirementType,
                RequiredCount = requiredCount,
                RequiredCountText = requiredCount > 0 ? requiredCount.ToString() : "Trigger",
                CompletedCount = completedCount,
                RemainingCount = remaining,
                IsComplete = isComplete,
                IsProgressType = config.IsProgress,
                ResetOnNewGamePlus = config.IsResetOnGameplus,
                CompletedRequirementIds = achievement.CompleteRequirementList?.ToList() ?? new List<int>(),
                CompletedRequirementGuids = achievement.CompleteRequirementGuidList?.ToList()
                    ?? new List<string>(),
                PriorityOrder = priority.order,
                PriorityLabel = priority.label,
                RouteHint = RouteHint(requirementType, context),
                Steps = steps,
            }
        );
    }

    var platformPlans = plans.Where(x => x.AchievementId >= 81000).ToList();
    var selectedPlans = platformPlans.Count > 0 ? platformPlans : plans;
    var filterMode = platformPlans.Count > 0 ? "platform_only" : "all";

    var completed = selectedPlans.Count(x => x.IsComplete);
    return new AnalysisReport
    {
        SavePath = savePath,
        GeneratedAtUtc = DateTime.UtcNow,
        PlayerName = data.RoleData?.RoleCs?.Base?.Name ?? "Unknown",
        PlayerLevel = data.RoleData?.RoleCs?.Base?.Level ?? 0,
        NewGamePlusCount = data.RoleData?.RoleCs?.Actor?.NewGamePlusCount ?? 0,
        CurrentChapter = chapter,
        CurrentMapId = mapId,
        MaxMapId = maxMapId,
        ActiveRebirthPoints = activeRebirthPoints,
        RawAchievementCount = plans.Count,
        RawCompletedAchievements = plans.Count(x => x.IsComplete),
        FilterMode = filterMode,
        TotalAchievements = selectedPlans.Count,
        CompletedAchievements = completed,
        IncompleteAchievements = selectedPlans.Count - completed,
        Achievements = selectedPlans.OrderBy(x => x.AchievementId).ThenBy(x => x.Index).ToList(),
    };
}

static (int order, string label) PriorityFor(string requirementType)
{
    if (
        requirementType.Contains("Pass", StringComparison.OrdinalIgnoreCase)
        || requirementType.Contains("EnterMap", StringComparison.OrdinalIgnoreCase)
        || requirementType.Contains("KillUnit", StringComparison.OrdinalIgnoreCase)
        || requirementType.Contains("FinishTask", StringComparison.OrdinalIgnoreCase)
    )
    {
        return (1, "High");
    }

    if (
        requirementType.Contains("GainItem", StringComparison.OrdinalIgnoreCase)
        || requirementType.Contains("GainSpell", StringComparison.OrdinalIgnoreCase)
        || requirementType.Contains("Build", StringComparison.OrdinalIgnoreCase)
        || requirementType.Contains("Alchemy", StringComparison.OrdinalIgnoreCase)
    )
    {
        return (2, "Medium");
    }

    if (requirementType.Contains("AchievementComplete", StringComparison.OrdinalIgnoreCase))
    {
        return (4, "Meta");
    }

    return (3, "Normal");
}

static string BuildTitle(int achievementId, string requirementType)
{
    var objective = requirementType switch
    {
        var t when t.Contains("KillUnit", StringComparison.OrdinalIgnoreCase) => "Defeat Target(s)",
        var t when t.Contains("EnterMap", StringComparison.OrdinalIgnoreCase) => "Discover Area(s)",
        var t when t.Contains("FinishTask", StringComparison.OrdinalIgnoreCase) => "Finish Quest Stage(s)",
        var t when t.Contains("GainItem", StringComparison.OrdinalIgnoreCase) => "Collect Item(s)",
        var t when t.Contains("GainSpell", StringComparison.OrdinalIgnoreCase) => "Acquire Spell(s)",
        var t when t.Contains("BuildArmor", StringComparison.OrdinalIgnoreCase) => "Forge Armor",
        var t when t.Contains("BuildWeapon", StringComparison.OrdinalIgnoreCase) => "Forge Weapon",
        var t when t.Contains("Alchemy", StringComparison.OrdinalIgnoreCase) => "Alchemy Milestone",
        var t when t.Contains("AchievementComplete", StringComparison.OrdinalIgnoreCase) => "Meta Achievement",
        _ => requirementType,
    };

    return $"Achievement {achievementId} - {objective}";
}

static string RouteHint(string requirementType, RouteContext context)
{
    if (context.CurrentChapter <= 0)
        return "Keep progressing until shrine travel and side routes open up.";

    if (
        requirementType.Contains("EnterMap", StringComparison.OrdinalIgnoreCase)
        || requirementType.Contains("FinishTask", StringComparison.OrdinalIgnoreCase)
    )
    {
        return $"Start in Chapter {context.CurrentChapter}, then backtrack through shrine travel for side paths, secret areas, and missed NPC follow-ups.";
    }

    if (requirementType.Contains("KillUnit", StringComparison.OrdinalIgnoreCase))
    {
        return "Use shrine travel to sweep optional bosses and chiefs you may have skipped in each chapter.";
    }

    if (
        requirementType.Contains("GainItem", StringComparison.OrdinalIgnoreCase)
        || requirementType.Contains("GainSpell", StringComparison.OrdinalIgnoreCase)
    )
    {
        return "Do a chapter-by-chapter cleanup pass and check shops, secret areas, side quests, and shrine crafting.";
    }

    return $"Start in Chapter {context.CurrentChapter}, clean up side content, then rescan after each unlock.";
}

static List<string> BuildStepPlan(
    string requirementType,
    int remaining,
    int requiredCount,
    int completedCount,
    RouteContext context
)
{
    var steps = new List<string>
    {
        requiredCount > 0
            ? $"Progress: {completedCount}/{requiredCount} done, {remaining} left."
            : $"Status: {(remaining == 0 ? "complete" : "still locked")}.",
    };

    if (requirementType.Contains("KillUnit", StringComparison.OrdinalIgnoreCase))
    {
        steps.Add("Check side paths and secret arenas for undefeated bosses or chiefs.");
        steps.Add($"Start in Chapter {context.CurrentChapter}, then work backward through earlier chapters.");
        return steps;
    }

    if (requirementType.Contains("EnterMap", StringComparison.OrdinalIgnoreCase))
    {
        steps.Add("Visit side routes, secret areas, and optional detours instead of only following the main path.");
        steps.Add("Use each shrine you unlock to branch out before moving on.");
        return steps;
    }

    if (requirementType.Contains("FinishTask", StringComparison.OrdinalIgnoreCase))
    {
        steps.Add("Revisit NPC hubs and finish any follow-up dialogue or turn-ins after major bosses.");
        steps.Add("Backtrack earlier chapters for side quests that reopen later.");
        return steps;
    }

    if (requirementType.Contains("GainItem", StringComparison.OrdinalIgnoreCase))
    {
        steps.Add("Clean up unique drops, quest rewards, shops, and secret-area pickups.");
        steps.Add("Check every chapter for one-time collectibles you may have skipped.");
        return steps;
    }

    if (requirementType.Contains("GainSpell", StringComparison.OrdinalIgnoreCase))
    {
        steps.Add("Finish spell-granting encounters and any shrine or NPC unlock chains tied to them.");
        steps.Add("After each unlock, rescan so the remaining list stays accurate.");
        return steps;
    }

    if (
        requirementType.Contains("BuildArmor", StringComparison.OrdinalIgnoreCase)
        || requirementType.Contains("BuildWeapon", StringComparison.OrdinalIgnoreCase)
    )
    {
        steps.Add("Gather the missing materials from optional bosses, elites, and side content.");
        steps.Add("Craft each missing piece at the shrine smith once it unlocks.");
        return steps;
    }

    if (requirementType.Contains("Alchemy", StringComparison.OrdinalIgnoreCase))
    {
        steps.Add("Advance the recipe chain, then craft the medicine you still need.");
        steps.Add("Farm missing herbs in earlier chapters where routes are fastest.");
        return steps;
    }

    if (requirementType.Contains("AchievementComplete", StringComparison.OrdinalIgnoreCase))
    {
        steps.Add("This unlocks automatically after every other achievement is done.");
        steps.Add("Finish the remaining achievements first, then rescan once more.");
        return steps;
    }

    steps.Add("Keep clearing side content alongside the main story.");
    steps.Add("Rescan after each milestone so the checklist stays current.");
    return steps;
}

static string BuildMarkdown(AnalysisReport report)
{
    var sb = new StringBuilder();
    sb.AppendLine("# Black Myth: Wukong Achievement Tracker");
    sb.AppendLine();
    sb.AppendLine($"Generated (UTC): {report.GeneratedAtUtc:yyyy-MM-dd HH:mm:ss}");
    sb.AppendLine($"Save: `{report.SavePath}`");
    sb.AppendLine();
    sb.AppendLine("## Player Snapshot");
    sb.AppendLine($"- Name: `{report.PlayerName}`");
    sb.AppendLine($"- Level: `{report.PlayerLevel}`");
    sb.AppendLine($"- NG+: `{report.NewGamePlusCount}`");
    sb.AppendLine($"- Current chapter: `{report.CurrentChapter}`");
    sb.AppendLine($"- Map ID: `{report.CurrentMapId}` (Max seen: `{report.MaxMapId}`)");
    sb.AppendLine($"- Active shrines seen in save: `{report.ActiveRebirthPoints.Count}`");
    sb.AppendLine();
    sb.AppendLine("## Progress Summary");
    sb.AppendLine($"- View mode: `{report.FilterMode}`");
    sb.AppendLine($"- Raw achievements in save: `{report.RawCompletedAchievements}/{report.RawAchievementCount}` complete");
    sb.AppendLine($"- Total achievements tracked in save: `{report.TotalAchievements}`");
    sb.AppendLine($"- Completed: `{report.CompletedAchievements}`");
    sb.AppendLine($"- Incomplete: `{report.IncompleteAchievements}`");
    sb.AppendLine();

    var nextTargets = report.Achievements
        .Where(x => !x.IsComplete)
        .OrderBy(x => x.PriorityOrder)
        .ThenByDescending(x => x.RemainingCount)
        .ThenBy(x => x.AchievementId)
        .Take(20)
        .ToList();

    sb.AppendLine("## Remaining Achievements");
    if (nextTargets.Count == 0)
    {
        sb.AppendLine("- All tracked achievements are complete.");
    }
    else
    {
        foreach (var item in nextTargets)
        {
            sb.AppendLine($"### {item.DisplayTitle}");
            sb.AppendLine($"- Status: `{(item.IsComplete ? "Complete" : "Incomplete")}`");
            sb.AppendLine($"- Progress: `{item.CompletedCount}/{item.RequiredCountText}` done, `{item.RemainingCount}` left");
            sb.AppendLine($"- Where to check next: {item.RouteHint}");
            sb.AppendLine("- Next checks:");
            foreach (var step in item.Steps)
            {
                sb.AppendLine($"  - {step}");
            }
            sb.AppendLine();
        }
    }

    sb.AppendLine("## Full Achievement List");
    sb.AppendLine("| ID | Status | Progress | Remaining |");
    sb.AppendLine("| --- | --- | --- | --- |");
    foreach (var item in report.Achievements.OrderBy(x => x.AchievementId).ThenBy(x => x.Index))
    {
        var status = item.IsComplete ? "Complete" : "Incomplete";
        sb.AppendLine(
            $"| {item.AchievementId} | {status} | {item.CompletedCount}/{item.RequiredCountText} | {item.RemainingCount} |"
        );
    }

    return sb.ToString();
}

sealed class CliOptions
{
    public string? SavePath { get; init; }
    public string? OutputDirectory { get; init; }

    public static CliOptions Parse(string[] args)
    {
        string? savePath = null;
        string? outputDirectory = null;
        for (var i = 0; i < args.Length; i++)
        {
            var current = args[i];
            if (current.Equals("--save", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                savePath = args[++i];
                continue;
            }

            if (current.Equals("--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                outputDirectory = args[++i];
                continue;
            }

            if (!current.StartsWith("--", StringComparison.Ordinal) && savePath is null)
            {
                savePath = current;
            }
        }

        return new CliOptions { SavePath = savePath, OutputDirectory = outputDirectory };
    }
}

sealed record RouteContext(int CurrentChapter, int CurrentMapId, int MaxMapId, int ActiveRebirthPointCount);

sealed class AnalysisReport
{
    public required string SavePath { get; init; }
    public required DateTime GeneratedAtUtc { get; init; }
    public required string PlayerName { get; init; }
    public required int PlayerLevel { get; init; }
    public required int NewGamePlusCount { get; init; }
    public required int CurrentChapter { get; init; }
    public required int CurrentMapId { get; init; }
    public required int MaxMapId { get; init; }
    public required List<int> ActiveRebirthPoints { get; init; }
    public required int RawAchievementCount { get; init; }
    public required int RawCompletedAchievements { get; init; }
    public required string FilterMode { get; init; }
    public required int TotalAchievements { get; init; }
    public required int CompletedAchievements { get; init; }
    public required int IncompleteAchievements { get; init; }
    public required List<AchievementPlan> Achievements { get; init; }
}

sealed class AchievementPlan
{
    public required int Index { get; init; }
    public required int AchievementId { get; init; }
    public required string DisplayTitle { get; init; }
    public required string RequirementType { get; init; }
    public required int RequiredCount { get; init; }
    public required string RequiredCountText { get; init; }
    public required int CompletedCount { get; init; }
    public required int RemainingCount { get; init; }
    public required bool IsComplete { get; init; }
    public required bool IsProgressType { get; init; }
    public required bool ResetOnNewGamePlus { get; init; }
    public required List<int> CompletedRequirementIds { get; init; }
    public required List<string> CompletedRequirementGuids { get; init; }
    public required int PriorityOrder { get; init; }
    public required string PriorityLabel { get; init; }
    public required string RouteHint { get; init; }
    public required List<string> Steps { get; init; }
}

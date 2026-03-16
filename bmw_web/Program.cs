using System.Diagnostics;
using System.Text.Json.Serialization;
using bmw_web.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = false;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddSingleton<AchievementPlanner>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () => Results.Ok(new { ok = true }));

app.MapPost(
    "/api/analyze",
    async (
        AnalyzeRequest? request,
        AchievementPlanner planner,
        ILogger<Program> logger,
        HttpContext httpContext
    ) =>
    {
        httpContext.Response.Headers.CacheControl = "no-store, no-cache, max-age=0";
        httpContext.Response.Headers.Pragma = "no-cache";

        if (request is null || string.IsNullOrWhiteSpace(request.SavePath))
        {
            logger.LogWarning("Analyze request rejected because no save path was provided.");
            return Results.BadRequest(new { ok = false, error = "Save path is required." });
        }

        var savePath = request.SavePath.Trim();
        var saveFileName = Path.GetFileName(savePath);
        var stopwatch = Stopwatch.StartNew();

        using var scope = logger.BeginScope("AnalyzeSave {SaveFileName}", saveFileName);
        logger.LogInformation("Analyze request started.");

        try
        {
            var saveFileLastWriteTimeUtc = File.GetLastWriteTimeUtc(savePath);
            var report = await planner.AnalyzeAsync(savePath);
            var analyzedAtUtc = DateTimeOffset.UtcNow;
            stopwatch.Stop();
            logger.LogInformation(
                "Analyze request completed in {ElapsedMs} ms for player {PlayerName}; {Completed}/{Total} achievements complete.",
                stopwatch.ElapsedMilliseconds,
                report.PlayerName,
                report.CompletedAchievements,
                report.TotalAchievements
            );
            return Results.Ok(new { ok = true, report, analyzedAtUtc, saveFileLastWriteTimeUtc });
        }
        catch (FileNotFoundException ex)
        {
            stopwatch.Stop();
            logger.LogWarning(
                ex,
                "Analyze request failed because the save file was not found: {SavePath}",
                savePath
            );
            return Results.BadRequest(new { ok = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Analyze request failed after {ElapsedMs} ms.", stopwatch.ElapsedMilliseconds);
            return Results.BadRequest(
                new
                {
                    ok = false,
                    error = "Failed to parse save file.",
                    detail = ex.Message,
                }
            );
        }
    }
);

app.Logger.LogInformation("Black Myth: Wukong Achievement Tracker web app is ready.");
app.Run();

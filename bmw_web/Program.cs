using System.Diagnostics;
using System.Text.Json.Serialization;
using bmw_web.Services;

const long MaxUploadedSaveBytes = 8 * 1024 * 1024;

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
        AchievementPlanner planner,
        ILogger<Program> logger,
        HttpContext httpContext
    ) =>
    {
        httpContext.Response.Headers.CacheControl = "no-store, no-cache, max-age=0";
        httpContext.Response.Headers.Pragma = "no-cache";

        var stopwatch = Stopwatch.StartNew();
        var saveFileName = "unknown.sav";
        var analysisSource = "unknown";
        byte[]? uploadedSaveBytes = null;
        string? savePath = null;
        DateTime? saveFileLastWriteTimeUtc = null;
        AnalysisReport report;

        try
        {
            if (httpContext.Request.HasFormContentType)
            {
                var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
                var saveFile = form.Files.GetFile("saveFile");
                if (saveFile is null)
                {
                    logger.LogWarning("Analyze request rejected because no save file was uploaded.");
                    return Results.BadRequest(new { ok = false, error = "Choose a .sav file first." });
                }

                if (saveFile.Length == 0)
                {
                    logger.LogWarning("Analyze request rejected because the uploaded save file was empty.");
                    return Results.BadRequest(
                        new { ok = false, error = "The uploaded save file is empty." }
                    );
                }

                if (saveFile.Length > MaxUploadedSaveBytes)
                {
                    logger.LogWarning(
                        "Analyze request rejected because the uploaded save file exceeded the size limit: {UploadedBytes} bytes.",
                        saveFile.Length
                    );
                    return Results.BadRequest(
                        new
                        {
                            ok = false,
                            error = $"The uploaded save file is too large. Limit: {MaxUploadedSaveBytes / (1024 * 1024)} MB.",
                        }
                    );
                }

                saveFileName = NormalizeSaveFileName(saveFile.FileName);
                analysisSource = "upload";

                using var saveBuffer = new MemoryStream((int)saveFile.Length);
                await saveFile.CopyToAsync(saveBuffer, httpContext.RequestAborted);
                uploadedSaveBytes = saveBuffer.ToArray();
            }
            else if (IsJsonRequest(httpContext.Request))
            {
                var request = await httpContext.Request.ReadFromJsonAsync<AnalyzeRequest>(
                    cancellationToken: httpContext.RequestAborted
                );
                if (request is null || string.IsNullOrWhiteSpace(request.SavePath))
                {
                    logger.LogWarning("Analyze request rejected because no save path was provided.");
                    return Results.BadRequest(new { ok = false, error = "Save path is required." });
                }

                savePath = request.SavePath.Trim();
                saveFileName = NormalizeSaveFileName(savePath);
                analysisSource = "server_path";
            }
            else
            {
                logger.LogWarning(
                    "Analyze request rejected because it was neither multipart/form-data nor application/json."
                );
                return Results.BadRequest(
                    new
                    {
                        ok = false,
                        error = "Upload a .sav file or send a JSON request with savePath.",
                    }
                );
            }

            using var scope = logger.BeginScope("AnalyzeSave {SaveFileName}", saveFileName);
            logger.LogInformation("Analyze request started from {AnalysisSource}.", analysisSource);

            report = uploadedSaveBytes is not null
                ? planner.AnalyzeUploadedSave(saveFileName, uploadedSaveBytes)
                : await planner.AnalyzeAsync(savePath!);

            if (savePath is not null)
            {
                saveFileLastWriteTimeUtc = File.GetLastWriteTimeUtc(savePath);
            }

            var analyzedAtUtc = DateTimeOffset.UtcNow;
            stopwatch.Stop();
            logger.LogInformation(
                "Analyze request completed in {ElapsedMs} ms for player {PlayerName}; {Completed}/{Total} achievements complete.",
                stopwatch.ElapsedMilliseconds,
                report.PlayerName,
                report.CompletedAchievements,
                report.TotalAchievements
            );
            return Results.Ok(
                new
                {
                    ok = true,
                    report,
                    analyzedAtUtc,
                    saveFileName,
                    saveFileLastWriteTimeUtc,
                }
            );
        }
        catch (FileNotFoundException ex)
        {
            stopwatch.Stop();
            logger.LogWarning(
                ex,
                "Analyze request failed because the save file was not found: {SaveFileName}",
                saveFileName
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

static bool IsJsonRequest(HttpRequest request)
{
    return request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase)
        == true;
}

static string NormalizeSaveFileName(string? candidate)
{
    var saveFileName = Path.GetFileName(candidate?.Trim());
    return string.IsNullOrWhiteSpace(saveFileName) ? "uploaded-save.sav" : saveFileName;
}

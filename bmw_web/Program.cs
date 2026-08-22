using System.Diagnostics;
using System.Text.Json.Serialization;
using bmw_web.Services;
using Microsoft.AspNetCore.Http.Features;

const long MaxUploadedSaveBytes = 4 * 1024 * 1024;
const long MaxUploadRequestBytes = MaxUploadedSaveBytes + (128 * 1024);

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
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = MaxUploadRequestBytes;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = MaxUploadRequestBytes;
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

        var requestMediaType = httpContext.Request.ContentType?.Split(';', 2)[0].Trim();
        if (
            !httpContext.Request.HasFormContentType
            || !string.Equals(
                requestMediaType,
                "multipart/form-data",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            logger.LogWarning("Analyze request rejected because it was not multipart/form-data.");
            return Results.Json(
                new { ok = false, error = "Upload one Black Myth: Wukong .sav file." },
                statusCode: StatusCodes.Status415UnsupportedMediaType
            );
        }

        var stopwatch = Stopwatch.StartNew();
        var saveFileName = "uploaded-save.sav";

        try
        {
            var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
            var saveFile = form.Files.GetFile("saveFile");
            if (saveFile is null || form.Files.Count != 1)
            {
                logger.LogWarning(
                    "Analyze request rejected because it did not contain exactly one saveFile upload. File count: {FileCount}.",
                    form.Files.Count
                );
                return Results.BadRequest(
                    new { ok = false, error = "Choose exactly one .sav file to analyze." }
                );
            }

            if (!string.Equals(Path.GetExtension(saveFile.FileName), ".sav", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Analyze request rejected because the uploaded file was not a .sav file.");
                return Results.Json(
                    new { ok = false, error = "That file is not a .sav file." },
                    statusCode: StatusCodes.Status415UnsupportedMediaType
                );
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
                return Results.Json(
                    new
                    {
                        ok = false,
                        error = $"The uploaded save file is too large. The limit is {MaxUploadedSaveBytes / (1024 * 1024)} MB.",
                    },
                    statusCode: StatusCodes.Status413PayloadTooLarge
                );
            }

            saveFileName = NormalizeSaveFileName(saveFile.FileName);
            using var saveBuffer = new MemoryStream((int)saveFile.Length);
            await saveFile.CopyToAsync(saveBuffer, httpContext.RequestAborted);

            using var scope = logger.BeginScope("AnalyzeSave {SaveFileName}", saveFileName);
            logger.LogInformation(
                "Analyze upload started for {UploadedBytes} bytes.",
                saveFile.Length
            );

            var report = planner.AnalyzeUploadedSave(saveFileName, saveBuffer.ToArray());
            var analyzedAtUtc = DateTimeOffset.UtcNow;
            stopwatch.Stop();

            logger.LogInformation(
                "Analyze upload completed in {ElapsedMs} ms for player {PlayerName}; {Completed}/{Total} achievements complete.",
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
                }
            );
        }
        catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (BadHttpRequestException ex) when (
            ex.StatusCode == StatusCodes.Status413PayloadTooLarge
        )
        {
            stopwatch.Stop();
            logger.LogWarning("Analyze upload rejected because the request body exceeded the limit.");
            return Results.Json(
                new
                {
                    ok = false,
                    error = $"The upload is too large. Choose a .sav file no larger than {MaxUploadedSaveBytes / (1024 * 1024)} MB.",
                },
                statusCode: StatusCodes.Status413PayloadTooLarge
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex,
                "Analyze upload failed after {ElapsedMs} ms for {SaveFileName}.",
                stopwatch.ElapsedMilliseconds,
                saveFileName
            );
            return Results.Json(
                new
                {
                    ok = false,
                    error = "This file could not be decoded as a Black Myth: Wukong save. Make sure it is an unmodified .sav file and try again.",
                },
                statusCode: StatusCodes.Status422UnprocessableEntity
            );
        }
    }
);

app.Logger.LogInformation("Black Myth: Wukong Achievement Tracker web app is ready.");
app.Run();

static string NormalizeSaveFileName(string? candidate)
{
    var normalizedSeparators = candidate?.Trim().Replace('\\', '/');
    var rawFileName = Path.GetFileName(normalizedSeparators);
    if (string.IsNullOrWhiteSpace(rawFileName))
    {
        return "uploaded-save.sav";
    }

    var safeFileName = new string(rawFileName.Where(character => !char.IsControl(character)).ToArray());
    return safeFileName.Length switch
    {
        0 => "uploaded-save.sav",
        > 120 => safeFileName[^120..],
        _ => safeFileName,
    };
}

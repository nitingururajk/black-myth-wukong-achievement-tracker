param(
    [string]$SavePath = "",
    [string]$OutDir = ".\\bmw_probe\\output"
)

if ([string]::IsNullOrWhiteSpace($SavePath)) {
    $SavePath = Read-Host "Enter full save path (.sav)"
}

dotnet run --project ".\\bmw_probe\\bmw_probe.csproj" -- --save "$SavePath" --out "$OutDir"

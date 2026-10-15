param([switch]$SkipTests)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$localSdk = Join-Path $PSScriptRoot '.tooling\dotnet\dotnet.exe'
$sdk = if (Test-Path -LiteralPath $localSdk) { $localSdk } else { (Get-Command dotnet -ErrorAction Stop).Source }
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
if (-not $SkipTests) {
    & $sdk run --project modern\Tracker.Tests\Tracker.Tests.csproj -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Core checks failed.' }
}
& $sdk publish modern\Tracker.Desktop\Tracker.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -o dist\TwitchCategoryTracker
if ($LASTEXITCODE -ne 0) { throw 'Desktop publish failed.' }
Get-ChildItem -LiteralPath dist\TwitchCategoryTracker -Filter '*.pdb' -File | Remove-Item
Write-Host "Ready: $PSScriptRoot\dist\TwitchCategoryTracker\TwitchCategoryTracker.exe"

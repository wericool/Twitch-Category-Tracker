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
$publishArguments = @('publish', 'modern\Tracker.Desktop\Tracker.Desktop.csproj', '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true',
    '-p:PublishAot=true', '-p:PublishSingleFile=false', '-p:EnableUiChecks=false', '-p:NativeDebugSymbols=false', '-p:DebugType=None', '-o', 'dist\aot')
# Prefer a normal Visual Studio C++ toolchain. This workspace also has a
# portable MSVC SDK and LLVM linker, which do not modify system installations.
$portableLinker = Join-Path $PSScriptRoot '.tooling\rustup\toolchains\stable-x86_64-pc-windows-msvc\lib\rustlib\x86_64-pc-windows-msvc\bin\gcc-ld\lld-link.exe'
$portableCrtRoot = Join-Path $PSScriptRoot '.tooling\win-native\VC\Tools\MSVC'
$portableSdkRoot = Join-Path $PSScriptRoot '.tooling\win-native\Windows Kits\10\Lib'
$previousLib = $env:LIB
try {
    if ((Test-Path -LiteralPath $portableLinker) -and (Test-Path -LiteralPath $portableCrtRoot) -and (Test-Path -LiteralPath $portableSdkRoot)) {
        $crtVersion = Get-ChildItem -LiteralPath $portableCrtRoot -Directory | Sort-Object Name -Descending | Select-Object -First 1
        $sdkVersion = Get-ChildItem -LiteralPath $portableSdkRoot -Directory | Sort-Object Name -Descending | Select-Object -First 1
        $env:LIB = "$($crtVersion.FullName)\lib\x64;$($sdkVersion.FullName)\um\x64;$($sdkVersion.FullName)\ucrt\x64"
        $publishArguments += '-p:IlcUseEnvironmentalTools=true', "-p:CppLinker=$portableLinker"
    }
    & $sdk @publishArguments
    if ($LASTEXITCODE -ne 0) { throw 'Native AOT publish failed.' }
}
finally { $env:LIB = $previousLib }
Get-ChildItem -LiteralPath dist\aot -Filter '*.pdb' -File | Remove-Item
Copy-Item -LiteralPath docs\ПРОЧИТАЙ-AOT.txt -Destination dist\aot\ПРОЧИТАЙ.txt
Write-Host "Ready: $PSScriptRoot\dist\aot\TwitchCategoryTracker.exe (keep its native DLLs beside it)"

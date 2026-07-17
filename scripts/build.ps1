# 개발용 빠른 빌드 (zip·main 배포 없음)
$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path $PSScriptRoot -Parent
$Project = Join-Path $RepoRoot "VArchiveHelper\VArchiveHelper.csproj"
dotnet build $Project -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$exe = Join-Path $RepoRoot "VArchiveHelper\bin\Release\net472\VArchiveHelper.exe"
Write-Host "완료: $exe"

# GitHub main에서 exe를 다시 디컴파일할 때 (소스 유실 시)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path $PSScriptRoot -Parent
$ReleaseDir = Join-Path $RepoRoot "release-from-github"
$OutDir = Join-Path $RepoRoot "VArchiveHelper-decompiled"
$Exe = Join-Path $ReleaseDir "VArchiveHelper.exe"

if (-not (Test-Path $Exe)) {
    if (-not (Test-Path $ReleaseDir)) {
        git clone --depth 1 --branch main https://github.com/jinthek123/varhelper.git $ReleaseDir
    }
}

if (-not (Test-Path $Exe)) {
    throw "VArchiveHelper.exe 없음: $Exe"
}

if (Test-Path $OutDir) { Remove-Item $OutDir -Recurse -Force }
ilspycmd -p --nested-directories -o $OutDir -r $ReleaseDir $Exe
Write-Host "완료: $OutDir"
Write-Host "DEVELOP.md 의 csproj·appsettings 정리 후 VArchiveHelper/ 로 옮기세요."

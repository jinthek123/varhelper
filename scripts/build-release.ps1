# source 브랜치에서 실행 → releases-local zip + main 브랜치에 설치본 배포

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path $PSScriptRoot -Parent
$Project = Join-Path $RepoRoot "VArchiveHelper\VArchiveHelper.csproj"
$OutDir = Join-Path $RepoRoot "VArchiveHelper\bin\Release\net472"
$ExampleSettings = Join-Path $RepoRoot "VArchiveHelper\appsettings.example.json"
$ReleasesLocal = Join-Path $RepoRoot "releases-local"

[xml]$csproj = Get-Content $Project
$version = $csproj.Project.PropertyGroup.Version | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($version)) {
    throw "VArchiveHelper.csproj 에 <Version> 이 없습니다."
}

Write-Host "빌드 중 — v$version"
dotnet build $Project -c Release --nologo -v q
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build 실패"
}

if (-not (Test-Path $OutDir)) {
    throw "출력 폴더 없음: $OutDir"
}

if (Test-Path $ExampleSettings) {
    Copy-Item $ExampleSettings (Join-Path $OutDir "appsettings.json") -Force
}

$publishFiles = Get-ChildItem $OutDir -File | Where-Object {
    $_.Extension -notin ".pdb", ".lnk"
}

New-Item -ItemType Directory -Force -Path $ReleasesLocal | Out-Null
$zipName = "VArchiveHelper-v$version.zip"
$zipPath = Join-Path $ReleasesLocal $zipName
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
Compress-Archive -Path ($publishFiles.FullName) -DestinationPath $zipPath -Force
Write-Host "로컬 zip: $zipPath"

$hasGit = Get-Command git -ErrorAction SilentlyContinue
$hasHead = $false
if ($hasGit) {
    $prevEap = $ErrorActionPreference
    $ErrorActionPreference = "SilentlyContinue"
    git -C $RepoRoot rev-parse HEAD *> $null
    $hasHead = $LASTEXITCODE -eq 0
    $ErrorActionPreference = $prevEap
}
if (-not $hasGit -or -not $hasHead) {
    Write-Warning "git 저장소가 비어 있거나 커밋 없음 — main 배포 건너뜀 (zip만 생성됨)"
    Write-Host "exe: $(Join-Path $OutDir 'VArchiveHelper.exe')"
    exit 0
}

$currentBranch = git -C $RepoRoot rev-parse --abbrev-ref HEAD

git -C $RepoRoot fetch origin main 2>$null
git -C $RepoRoot checkout main
if ($LASTEXITCODE -ne 0) {
    throw "main 브랜치 checkout 실패. git branch source 후 다시 시도하세요."
}

foreach ($f in $publishFiles) {
    Copy-Item $f.FullName (Join-Path $RepoRoot $f.Name) -Force
}

git -C $RepoRoot add -A
$status = git -C $RepoRoot status --porcelain
if ($status) {
    git -C $RepoRoot commit -m "Publish v$version binaries (Download ZIP)"
    git -C $RepoRoot push origin main
    Write-Host "main 배포 완료 (Code Download ZIP)"
} else {
    Write-Host "main 변경 없음"
}

if ($currentBranch -and $currentBranch -ne "main") {
    git -C $RepoRoot checkout $currentBranch
}

Write-Host "exe: $(Join-Path $OutDir 'VArchiveHelper.exe')"

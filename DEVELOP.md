# 개발자 안내

사용자용 설치본: GitHub **`main`** · [Releases](https://github.com/jinthek123/varhelper/releases)

## 폴더 구조 (source 브랜치)

```
VArchiveHelper-win/
├── VArchiveHelper/          # WinForms 앱 소스 + .csproj
│   ├── *.cs
│   ├── System/              # net472 polyfill (디컴파일 산출물)
│   ├── appsettings.example.json
│   └── appsettings.json     # 로컬 전용 (git 제외)
├── scripts/
│   ├── build.ps1            # 개발 빌드
│   ├── build-release.ps1    # zip + main 배포
│   └── decompile.ps1        # exe 재디컴파일
├── .github/workflows/       # Release CI
├── releases-local/          # 로컬 zip 출력 (git 제외)
├── VArchiveHelper.sln       # Visual Studio
├── DEVELOP.md
├── CHANGELOG.md
└── README.md
```

**루트에 exe·dll 없음** — 실행 파일은 빌드 후 `VArchiveHelper/bin/Release/net472/` 에만 있습니다.

## 필요 환경

- Windows 10/11
- [.NET SDK 8+](https://dotnet.microsoft.com/download)
- 대상: **.NET Framework 4.7.2** (`net472`)

## 소스 출처

GitHub `main` v0.1.3 `VArchiveHelper.exe`를 ILSpy로 디컴파일해 복구했습니다. 변수명·주석은 원본과 다를 수 있습니다.

## 일상 작업

```powershell
cd C:\Users\fire2\Desktop\VArchiveHelper-win

# 빌드
.\scripts\build.ps1

# 실행
.\VArchiveHelper\bin\Release\net472\VArchiveHelper.exe
```

또는 `dotnet build VArchiveHelper\VArchiveHelper.csproj -c Release`

## 릴리스

```powershell
.\scripts\build-release.ps1
```

- `releases-local/VArchiveHelper-v{버전}.zip` 생성
- (커밋 있을 때) `main`에 설치본 반영·푸시

태그 Release:

```powershell
git tag v0.1.4
git push origin source
git push origin v0.1.4
```

## source 브랜치 푸시 (최초 1회)

```powershell
git push -u origin source
```

# vArchiveHelper 0.1.3

지정한 단축키(기본 **Insert**)로 모니터를 지정 해상도로 캡처 → 클립보드 → v-archive **모드1 인식(Alt+Insert)**.

DJMAX 등 전체화면 게임용 보조 도구입니다. [v-archive](https://github.com/kokonohanahata/v-archive) 필요.

**설치 (exe):** [DOWNLOAD.md](DOWNLOAD.md) · [Releases](https://github.com/jinthek123/varhelper/releases)

---

## 설치 (일반 사용자)

**압축은 항상 한 번만** 풀면 됩니다.

| 받는 곳 | 방법 |
|--------|------|
| **Code → Download ZIP** (`main`) | zip 1회 해제 → **`VArchiveHelper.exe`** (소스 폴더 없음) |
| [Releases](https://github.com/jinthek123/varhelper/releases) | **`VArchiveHelper-win.zip`** 1회 해제 → exe |

1. 위 방식으로 받기 → 압축 해제 **1회**
2. `VArchiveHelper.exe` 실행 → v-archive 경로 지정 → 설정 저장
3. Windows 10/11 + .NET Framework 4.7.2 (별도 SDK 불필요)

**주의:** exe와 dll이 **같은 폴더**에 있어야 정상 실행됩니다.

Releases: https://github.com/jinthek123/varhelper/releases/download/v0.1.3/VArchiveHelper-win.zip

---

## v-archive 설정

1. **캡처 / 업로드 따로 (모드 1)**
2. 인식 단축키: **Alt+Insert**
3. v-archive **모드 2**에 Insert가 남아 있으면 다른 키로 변경 (캡처 키와 충돌 방지)

---

## 사용 방법

- **설정 창이 열려 있을 때만** 캡처 단축키가 동작합니다. 창을 닫으면 프로그램이 종료됩니다 (트레이 상주 없음).
- 창은 **최소화**할 수 있습니다.
- 설정 창에서 **캡처 단축키**, **모니터**, **해상도**, **v-archive 경로**, **라이트/다크 테마**를 바꿀 수 있습니다.
- **사용 매뉴얼** 버튼으로 안내를 볼 수 있습니다.
- 단축키를 바꾸면 **자동 저장**됩니다. 그 외 항목은 **적용 및 저장**을 누르세요.
- 저장하지 않고 닫으려 하면 **경고**가 나옵니다.

**권장 (전체화면):** 설정에서 **DXGI 캡처**·**물리 픽셀** 켜기. 캡처 결과에 `[DXGI]`가 보이면 DXGI 경로로 동작 중입니다.

---

## 0.1.3에서 달라진 점

- **앱 아이콘** 추가
- **라이트/다크 테마** (코랄 액센트 UI)
- **사용 매뉴얼** 팝업
- 설정 창 **스크롤·레이아웃** 개선 (휠로 값이 바뀌지 않음)
- **캡처 성능**·메모리 개선
- 본 사이트에 사용된 게임 콘텐츠의 저작권은 NEOWIZ에 있습니다.

---

## 설정 (`appsettings.json`)

exe와 **같은 폴더**의 `appsettings.json` 하나만 수정하면 됩니다.

| 항목 | 설명 |
|------|------|
| ConfigVersion | 설정 파일 버전 (현재 2) |
| MonitorIndex | `0` = 1번 모니터, `1` = 2번 모니터 (기본) |
| VArchiveExePath | v-archive.exe 위치 (설정 창 **찾기**로 지정) |
| UiTheme | `Light` 또는 `Dark` |
| CaptureHotkeyVirtualKey | 캡처 트리거 VK (기본 45 = Insert) |
| CaptureHotkeyModifiers | 수정자 비트 |
| UseDxgiCapture / UsePhysicalPixels | 전체화면 캡처 시 권장 |

---

## 브랜치 (개발)

| 브랜치 | 용도 |
|--------|------|
| **`main`** | Code Download ZIP — **exe·dll·안내만** |
| **`source`** | 소스·`scripts/`·Actions — **여기서 개발** |

자세히: [DEVELOP.md](DEVELOP.md)

```powershell
.\scripts\build.ps1
# → VArchiveHelper\bin\Release\net472\VArchiveHelper.exe

.\scripts\build-release.ps1
# → releases-local\VArchiveHelper-v0.1.3.zip + (선택) main 배포
```

### Git 태그 (변화의 흐름)
- 이전 버전은 더 이상 배포하지 않습니다.
- `v0.1.3` — **현재 최신** (테마·매뉴얼·UI)

---

**변경 이력:** [CHANGELOG.md](CHANGELOG.md)

### 제작 및 재배포에 관련하여

개정
본 프로그램은 v-archive 가 제대로 작동하지 않는 유저를 위해 제작되었으며 어떠한 형태로든 유저의 편의를 위한 가공 및 역공학 등이 가능합니다. → (2026-07-21) V-Archive 관리자, 서열표 제작자, DJMAX 유틸리티 제작자 외의 무단 수정 및 재배포를 금합니다.

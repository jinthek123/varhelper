# Changelog

형식은 [Keep a Changelog](https://keepachangelog.com/ko/1.1.0/)를 참고합니다.  
버전별 **exe(zip)** — [Releases](https://github.com/jinthek123/varhelper/releases).  
**`main`** = Code Download ZIP(설치본만) · **`source`** = 소스 · `build-release.ps1` → `main` 배포.

## [0.1.3] — 2026-06-08

**표시 이름:** vArchiveHelper 0.1.3  
**Git 태그:** `v0.1.3`  
**기반:** 0.1.2

### Added

- **앱 아이콘** — exe·설정 창·매뉴얼·경로 설정 창
- **라이트/다크 테마** — 코랄 액센트, 8px 라운드 UI
- **사용 매뉴얼** 팝업
- 설정 창 **스크롤 가드** — 휠로 가로·세로·모니터 값이 바뀌지 않음

### Changed

- **`main` 루트 exe·dll** 0.1.3으로 갱신 (아이콘·UI·캡처 성능 개선 포함)
- **`appsettings.json` 단일화** — placeholder만 포함 (`main` Download ZIP)
- 설정 UI — 섹션 순서·스크롤·프리셋 2줄 등 개선
- **캡처 Bitmap 이중 복제 제거** — 메모리·캡처 시간 절약

### Fixed

- 사용 매뉴얼 **닫기** 버튼 정상 동작
- 설정 창 **가로 스크롤** 제거 (세로만)
- **모니터/DPI 전환** 시 창이 잘리지 않도록 크기·위치 보정
- 상단 **상태 문구** — 「캡처한 뒤」에서 줄바꿈

### Troubleshooting — 2026-06-08 07:06

설정·매뉴얼 레이아웃을 QHD/FHD·DPI 전환 기준으로 재검증·보정했습니다.

**설정 창**

- **「표시」 위 빈 공간** — 상단 섹션이 비어 보이던 문제 수정
- **가로 스크롤** — 세로 스크롤만 남기고 가로 스크롤 제거
- **모니터/DPI 전환** — 창 크기·위치를 작업 영역에 맞게 재조정 (높이 520px)
- **상태 문구 줄바꿈** — 「캡처한 뒤」에서 자연스럽게 줄바꿈

**사용 매뉴얼**

- **QHD→FHD 가로 스크롤** — 모니터 이동·세로 스크롤바 생성 시 본문 너비 재계산
- **제목 소실** — 「vArchiveHelper 사용 안내」가 FHD로 옮길 때 사라지던 문제 수정
- **카드 여백** — 창·카드 안쪽 여백 축소
- **본문 가로 600px** — FHD에서도 줄바꿈되도록 표시 폭 조정

---

## [0.1.2] — 2026-06-04

**표시 이름:** vArchiveHelper 0.1.2  
**Git 태그:** `v0.1.2`  
**기반:** 0.1.1 lite

### Changed

- **루트에 exe·dll 직접 배치** — Code Download ZIP 압축 **1회** (중첩 zip 제거)
- **`dist/`·루트 `VArchiveHelper-win.zip` 제거** — Releases만 zip 유지

### Added

- **캡처 단축키 사용자 지정** — 설정 창에서 조합키 지정, `appsettings.json`의 `CaptureHotkeyVirtualKey` / `CaptureHotkeyModifiers`
- `CaptureHotkey.cs` — 단축키 표시·검증·`RegisterHotKey` 수정자 플래그
- **단축키 지정 시 자동 저장** 및 즉시 핫키 재등록
- **모니터 콤보박스** — 해상도 표시 (`MonitorList.cs`)
- **저장 검증** — v-archive 경로·모니터·단축키, 등록 실패 시 롤백
- **닫기 전 미저장 경고** (예/아니오/취소)
- **단일 인스턴스** (`SingleInstanceApp.cs`)
- **비동기 캡처** — UI 멈춤 완화 (`CapturePipeline.RunAsync`)
- **첫 실행** — `appsettings.example.json` → `appsettings.json` 자동 복사
- **v-archive 경로 설정 유도** — 첫 실행 시 `VArchivePathSetupForm`
- **버전별 exe 워크플로** — README, `scripts/build-release.ps1`, `releases-local/`
- **DOWNLOAD.md** — Releases 기준 exe 안내
- `ConfigVersion` 설정 필드 (현재 2)

### Changed

- README: Releases zip만 설치용, Source code zip은 exe 없음 명시
- `appsettings.example.json` 빌드 출력 복사
- **창 닫기 = 프로그램 종료** — 트레이 아이콘·백그라운드 상주 제거
- 단축키는 **설정 창이 열려 있을 때만** 동작 (always-on-top 없음 — 게임 우선)
- UI: 단축키 줄에 **적용 및 저장** 제거(하단 1개), 미저장 시 제목에 `*`
- UI 문구: Insert 고정 표현 → 캡처 단축키 / 캡처 동작 실행

### Removed

- `NotifyIcon` 트레이 메뉴 및 X 클릭 시 `Hide()` 동작

---

## [0.1.1 lite] — 2026-06-01

**표시 이름:** vArchiveHelper 0.1.1 (lite version)  
**Git 태그:** `v0.1.1-lite`

### Changed

- 캡처 **미리보기 UI 제거** (설정 창만 유지) — 메모리·상주 부담 감소
- Insert 후 미리보기 갱신 캡처 제거 (Insert당 캡처 1회만)
- `CapturePreviewForm` → `SettingsForm`으로 재구성
- `PreviewIntervalMs` 설정 제거

### Added

- `AppBranding.cs` — 앱 표시 이름 통일
- `CHANGELOG.md` — 버전 이력 정리

---

## [0.1.1] — 2026 (full)

**Git 커밋:** `54e11f0` — 소스 공개 + DXGI 안정화  
**특징:** 미리보기 창 포함 (이후 lite에서 제거됨)

### Added

- VArchiveHelper 전체 소스 (WinForms, DXGI/GDI 캡처)
- GitHub Actions: `v*` 태그 push 시 `VArchiveHelper-win.zip` 빌드·Release 업로드
- `.gitignore` (개인 `appsettings.json`, `bin/` 등 제외)

### Fixed

- DXGI Desktop Duplication 캡처 안정화 (`RequireDxgiCapture` 등)

---

## [0.1.0] — 최초 공개

**Git 커밋:** `42774c2` — README + 배포 zip

### Added

- README 사용 안내
- `VArchiveHelper-win.zip` 배포 (소스 없음)

---

## full vs lite

| 구분 | 미리보기 | 설정 UI | 권장 |
|------|----------|---------|------|
| **0.1.1 full** | 있음 | 미리보기 오른쪽 패널 | — (과거) |
| **0.1.1 lite** | 없음 | 설정 전용 창 | 메모리·게임 중 상주 |

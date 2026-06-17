# 기획서 — rps (가위바위보)

> 결정 도우미 도구 풀 / **우선순위 P2**
> 작성: game-designer 단계 (구현 전). 코드 없음 — 본 문서는 gameplay-programmer 의 구현 입력.

---

## 1. 개요

- **한 줄 컨셉**: 사용자가 가위/바위/보 중 하나를 내면 AI가 무작위로 내고 승·무·패를 판정, 전적을 누적하는 도구.
- **결정 단위**: 1판 3지선다 대결, 승/무/패.
- **차별점**: 다른 도구가 "뽑기" 라면 이건 **대결형** — 사용자 선택 입력 + AI 무작위 + 판정 + 통계.

---

## 2. 입력 (Input)

| 항목 | 형태 | 비고 |
|---|---|---|
| 사용자 선택 | `CHButton` ×3 (가위 / 바위 / 보) | 탭 즉시 1판 진행 |
| 전적 초기화 | `CHButton` (초기화) | 누적 통계 리셋(선택) |

- 별도 텍스트 입력 없음. 3버튼 중 하나 탭이 곧 라운드 시작.

---

## 3. 동작 (Behavior)

1. 사용자가 가위/바위/보 버튼 탭.
2. 클릭 사운드(`EAudio.Click`).
3. AI 선택 = `UnityEngine.Random.Range(0, 3)` (0=가위,1=바위,2=보), 균등.
4. 짧은 연출(손 모양 셔플 후 정지, 약 0.3~0.5초) — 결과는 탭 시점에 확정.
5. 판정: 승/무/패 → 누적 전적 갱신.

> 판정표(고정): 가위>보, 바위>가위, 보>바위. 같으면 무.

---

## 4. 결과 (Output)

- 이번 판: 사용자 선택 vs AI 선택 + "승리"/"무승부"/"패배" 라벨.
- 누적 전적: `승 {0} / 무 {1} / 패 {2}` (세션 내 누적).
- 승/무/패 색 구분 + 텍스트 병행(접근성).
- 표시 문자열은 String.json stringID 경유(§6).

---

## 5. UI / 화면 흐름

### 5.0 현재 UI 풍 준수 (필수)

> 대결 메커니즘은 유지하되, 외형·레이아웃·컴포넌트는 현재 앱 풍에 맞춘다. (룰렛 아님)

- **테마**: `Assets/Dark UI/` 다크 패널·버튼.
- **폰트**: `EFont.Jua` — 모든 텍스트 `CHText`.
- **컴포넌트**: 선택·초기화 `CHButton`, 결과/전적 라벨 `CHText`. 손 모양은 `Image` 스프라이트.
- **레이아웃 관례**: 상단 메뉴(뒤로) 버튼 · 중앙 대결 영역(사용자 vs AI) · 하단 선택 버튼 3개.

### 5.1 배치 / RouletteScene 와이어링

- `RandomNumberScene` 와 동일하게 **`RouletteScene` 의 자식 GameObject**(active 토글).
- 연결(`RouletteScene.cs`): ① `ERouletteMenu.Rps` 추가 → ② `[SerializeField] RpsScene _rpsScene` → ③ `SetManagement()` 에서 `SetRouletteSceneAccess(this)` → ④ `ShowScene()` switch `case Rps` + `liMainSceneObj` 토글 → ⑤ 메뉴 진입 버튼 `_liMenu` 등록.
- 복귀: `Close()` → `SetActive(false)` + `ShowScene(ERouletteMenu.Menu)`.

### 5.2 구성

- 상단: 메뉴(뒤로) 버튼 `CHButton`
- 중앙: 사용자 손 / AI 손 표시(`Image`) + 판정 라벨 `CHText` + 전적 라벨 `CHText`
- 하단: 가위·바위·보 `CHButton` ×3 + 초기화 `CHButton`
- **상태 리셋**: `OnEnable` 에서 손 표시·판정 라벨 초기화. 전적 누적은 유지할지(세션) 리셋할지 결정 — 기본은 서브씬 이탈 시 유지, 명시적 초기화 버튼으로만 리셋.

---

## 6. Enum · 에셋 키 · 문자열 매핑

| 종류 | 키 | 비고 |
|---|---|---|
| 메뉴 항목 | `CommonEnum.ERouletteMenu.Rps` (신규) | 호스트 메뉴 진입 키 |
| 손 스프라이트 | `RpsRock` / `RpsScissors` / `RpsPaper` | Addressables 라벨 "Resource" |
| 클릭 사운드 | `CommonEnum.EAudio.Click` (재사용) | — |
| 폰트 | `CommonEnum.EFont.Jua` (재사용) | 현재 UI 풍 |
| 시각 테마 | `Assets/Dark UI/` (재사용) | — |
| 표시 문자열 | String.json 신규 stringID — "승리"/"무승부"/"패배"/"승 {0} 무 {1} 패 {2}" 등 | 다음 빈 ID 순차 할당 |

---

## 7. ChvjPackage 연동 포인트 (Rule 03)

- **UI 컴포넌트**: `CHButton`/`CHText`. 손 모양 `Image`(스프라이트 스왑).
- **리소스**: 손 스프라이트 `CHMResource` enum-key 로드.
- **풀링**: 동적 스폰 없음 → `CHMPool` 불필요.
- **사운드**: `CHMSound` Click hook.
- **문자열**: `JsonManager.GetStringData` 경유.

---

## 8. 엣지 케이스 / 구현 메모

- 연출 진행 중 다른 손 탭 → 무시 또는 즉시 다음 판(짧은 연출이라 잠금 선택적).
- AI 선택(`Range`)을 판정과 분리한 메서드로 — 판정표·균등 분포 테스트.
- 전적 오버플로 걱정 없음(int).

---

## 9. 추후 확장 (P2 범위 밖)

- 전적 영구 저장(앱 재실행 유지).
- 연승 스트릭/통계.
- AI 편향(특정 패턴) 난이도.

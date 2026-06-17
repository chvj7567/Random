# 기획서 — team_split (팀 나누기)

> 결정 도우미 도구 풀 / **우선순위 P1**
> 작성: game-designer 단계 (구현 전). 코드 없음 — 본 문서는 gameplay-programmer 의 구현 입력.

---

## 1. 개요

- **한 줄 컨셉**: N명을 T개 팀으로 균등하게 무작위 분배하는 도구.
- **결정 단위**: N명 → T팀 균등 배분(나머지는 앞 팀부터 1명씩).
- **차별점**: lottery·ladder 와 달리 **그룹핑** 결과. 팀별 멤버 목록이 산출물.

---

## 2. 입력 (Input)

> 다중 항목 입력은 기존 `CustomRandomScene` 패턴 그대로.

| 항목 | 형태 | 범위 / 기본 | 비고 |
|---|---|---|---|
| 사람 이름 | `TMP_InputField` + 추가/삭제 `CHButton` → `CustomScrollView` | 2 ~ N | 분배 대상 |
| 팀 개수 T | 스테퍼 `CHButton` − / + + `CHText` | 2 ~ N, 기본 2 | 사람 수 이하 |
| 나누기 | `CHButton` (나누기) | — | 단일 액션 |

- T 상한은 현재 사람 수 N. 사람 추가/삭제 시 T 상한 재계산.

---

## 3. 동작 (Behavior)

1. 이름 입력 + T 설정 → 나누기 탭.
2. 클릭 사운드(`EAudio.Click`).
3. 이름 목록 셔플(Fisher-Yates) 후 T팀에 순차 배분 — `기본 인원 = floor(N/T)`, 나머지 `N % T` 명은 **앞 팀부터 1명씩** 추가.
4. 팀별 멤버 목록 표시.

> 균등 규칙 명시: 팀 간 인원 차이는 최대 1명. 예) 7명 3팀 → 3·2·2.

---

## 4. 결과 (Output)

- 팀별 카드/컬럼(T개) + 각 팀 멤버 리스트.
- 팀 헤더("1팀"/"2팀"…)는 String.json stringID 경유.
- Dark UI 톤 카드.

---

## 5. UI / 화면 흐름

### 5.0 현재 UI 풍 준수 (필수)

> 그룹핑 결과는 룰렛으로 표현 불가 — 자체 결과. 외형·레이아웃·컴포넌트만 현재 앱 풍에 맞춘다.

- **테마**: `Assets/Dark UI/` 다크 패널·버튼. 팀 카드 동일 톤.
- **폰트**: `EFont.Jua` — 모든 텍스트 `CHText`.
- **컴포넌트**: 추가/삭제·스테퍼·나누기 `CHButton`, 라벨 `CHText`, 입력 `TMP_InputField`, 리스트 `CustomScrollView`.
- **레이아웃 관례**: 상단 메뉴(뒤로) 버튼 · 중앙 입력+팀 결과 · 하단 나누기 버튼.

### 5.1 배치 / RouletteScene 와이어링

- `CustomRandomScene` 와 동일하게 **`RouletteScene` 의 자식 GameObject**(active 토글).
- 연결(`RouletteScene.cs`): ① `ERouletteMenu.TeamSplit` 추가 → ② `[SerializeField] TeamSplitScene _teamSplitScene` → ③ `SetManagement()` 에서 `SetRouletteSceneAccess(this)` → ④ `ShowScene()` switch `case TeamSplit` + `liMainSceneObj` 토글 → ⑤ 메뉴 진입 버튼 `_liMenu` 등록.
- 복귀: `Close()` → `SetActive(false)` + `ShowScene(ERouletteMenu.Menu)`.

### 5.2 구성

- 상단: 메뉴(뒤로) 버튼 `CHButton`
- 입력: 이름 입력칸 + 추가·삭제 버튼 + `CustomScrollView` / 팀수 스테퍼
- 중앙: 팀 결과 영역(T개 카드, Dark UI 패널)
- 하단: 나누기 `CHButton`
- **상태 리셋**: `OnEnable` 에서 이름 리스트·팀수(2)·결과 초기화.

---

## 6. Enum · 에셋 키 · 문자열 매핑

| 종류 | 키 | 비고 |
|---|---|---|
| 메뉴 항목 | `CommonEnum.ERouletteMenu.TeamSplit` (신규) | 호스트 메뉴 진입 키 |
| 팀 카드/멤버 셀 프리팹 | `Assets/AddressableResource/Prefab/TeamCard` 등 | 동적 팀 수 — 풀링 prototype |
| 클릭 사운드 | `CommonEnum.EAudio.Click` (재사용) | — |
| 폰트 | `CommonEnum.EFont.Jua` (재사용) | 현재 UI 풍 |
| 시각 테마 | `Assets/Dark UI/` (재사용) | — |
| 표시 문자열 | String.json 신규 stringID — "나누기" / "{0}팀" 등 | 다음 빈 ID 순차 할당 |

---

## 7. ChvjPackage 연동 포인트 (Rule 03)

- **UI 컴포넌트**: `CHButton`/`CHText`/`TMP_InputField`/`CustomScrollView`.
- **풀링**: 팀 카드·멤버 셀은 동적 개수 → `CHMPool` 또는 `CHPoolingScrollView`. `Object.Instantiate` 금지.
- **사운드**: `CHMSound` Click hook.
- **문자열**: `JsonManager.GetStringData` 경유.

---

## 8. 엣지 케이스 / 구현 메모

- T > 사람 수 → T 클램프(빈 팀 방지).
- 사람 0~1명 → 나누기 차단.
- 균등 규칙(floor + 나머지 앞 팀 배분) 명시 — 분배 로직을 셔플과 분리해 인원 차 ≤ 1 테스트.
- 동명이인 허용(이름만으로 구분 안 함).

---

## 9. 추후 확장 (P1 범위 밖)

- 팀별 인원 직접 지정(불균등 허용).
- 고정 멤버(특정 인원 같은 팀 강제) 제약.
- 팀명 직접 입력.

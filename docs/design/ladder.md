# 기획서 — ladder (사다리타기)

> 결정 도우미 도구 풀 / **우선순위 P1**
> 작성: game-designer 단계 (구현 전). 코드 없음 — 본 문서는 gameplay-programmer 의 구현 입력.

---

## 1. 개요

- **한 줄 컨셉**: 참가자 N명과 결과 N개를 무작위 사다리로 연결해, 각자가 어떤 결과를 받는지 경로 애니메이션으로 보여주는 도구.
- **결정 단위**: N:N 1대1 매핑(순열). 모든 참가자가 서로 다른 결과를 받음.
- **차별점**: coin/dice/roulette 의 "하나 뽑기" 와 달리 **전원 동시 배정**. 경로를 따라가는 시각 연출이 핵심 재미.

---

## 2. 입력 (Input)

> 다중 항목 입력은 기존 `CustomRandomScene` 패턴 그대로 — `TMP_InputField` + 추가/삭제 버튼 + 리스트 스크롤뷰.

| 항목 | 형태 | 범위 / 기본 | 비고 |
|---|---|---|---|
| 참가자 이름 | `TMP_InputField` + 추가/삭제 `CHButton` → `CustomScrollView` 리스트 | 2 ~ 8 권장 | N명 |
| 결과 항목 | 동일 입력 방식(두 번째 리스트) | 참가자 수와 동일(N) | 예: "당첨", "꽝", "커피사기" |

- 참가자 수 = 결과 수 강제. 불일치 시 그리기 버튼 비활성 + 안내 문자열.
- 상한(예 8)은 화면 가로폭·가로줄 가독성 한계. 초과는 P1 범위 밖.

---

## 3. 동작 (Behavior)

1. 참가자·결과를 각각 입력 → 그리기 버튼 탭.
2. 클릭 사운드(`EAudio.Click`), 버튼 입력 잠금.
3. 세로줄 N개 + **무작위 가로줄(rung)** 생성. 가로줄은 인접 세로줄 사이에 무작위 배치(같은 높이 중복 금지).
4. 각 참가자 위치에서 출발 → 가로줄 만나면 좌/우 이동하는 **경로 추적**. DOTween 으로 순차 추적 애니메이션(참가자별 색 구분).
5. 도착한 결과를 매핑 확정. 전원 완료 시 매핑표 노출 + 버튼 잠금 해제.

> **결과 = 가로줄 배치로 결정되는 순열**. 시각 추적은 그 순열을 보여줄 뿐. 결과 산출(순열 계산)을 렌더와 분리해 테스트 가능하게.

---

## 4. 결과 (Output)

- 각 참가자 → 결과 매핑표(`참가자명 → 결과명`).
- 개별 참가자 탭 시 해당 경로만 하이라이트(선택 기능).
- 사다리 선/노드는 Dark UI 톤(어두운 배경 + 밝은 라인).
- 모든 표시 문자열(안내·매핑 라벨)은 String.json stringID 경유(§6).

---

## 5. UI / 화면 흐름

### 5.0 현재 UI 풍 준수 (필수)

> 사다리 렌더링·경로 추적 등 **고유 메커니즘은 유지**하되, 외형·레이아웃·컴포넌트는 현재 앱 풍에 맞춘다. (사다리는 룰렛으로 표현 불가 — 자체 렌더)

- **테마**: `Assets/Dark UI/` 다크 패널·버튼. 사다리 선도 동일 톤.
- **폰트**: `EFont.Jua` — 모든 텍스트 `CHText`.
- **컴포넌트**: 추가/삭제·그리기 `CHButton`, 라벨 `CHText`, 이름 입력 `TMP_InputField`, 리스트 `CustomScrollView`(기존). 기존 씬 위젯 세트 동일.
- **레이아웃 관례**: 상단 메뉴(뒤로) 버튼 · 중앙 입력+사다리 · 하단 그리기 버튼.

### 5.1 배치 / RouletteScene 와이어링

- `CustomRandomScene` 와 동일하게 **`RouletteScene` 의 자식 GameObject**(active 토글). 별도 씬·프리팹 아님.
- 연결(`RouletteScene.cs`): ① `ERouletteMenu.Ladder` 추가 → ② `[SerializeField] LadderScene _ladderScene` → ③ `SetManagement()` 에서 `SetRouletteSceneAccess(this)` → ④ `ShowScene()` switch `case Ladder` + `liMainSceneObj` 토글 → ⑤ 메뉴 진입 버튼 `_liMenu` 등록.
- 복귀: `Close()` → `SetActive(false)` + `ShowScene(ERouletteMenu.Menu)`.

### 5.2 구성

- 상단: 메뉴(뒤로) 버튼 `CHButton`
- 입력: 참가자 리스트 / 결과 리스트(각각 입력칸 + 추가·삭제 버튼 + `CustomScrollView`)
- 중앙: 사다리 그리기 영역(Dark UI 패널)
- 하단: 그리기 `CHButton` + 안내/매핑 라벨 `CHText`
- **상태 리셋**: `OnEnable` 에서 참가자·결과 리스트·사다리·매핑 초기화.

---

## 6. Enum · 에셋 키 · 문자열 매핑

> Rule 03 §2 — Enum 값명 = 에셋 파일명 정확히 일치.

| 종류 | 키 | 비고 |
|---|---|---|
| 메뉴 항목 | `CommonEnum.ERouletteMenu.Ladder` (신규) | 호스트 메뉴 진입 키 |
| 사다리 선/노드 셀(선택) | `Assets/AddressableResource/Prefab/LadderRung` 등 | 동적 가로줄 — 풀링 prototype |
| 클릭 사운드 | `CommonEnum.EAudio.Click` (재사용) | — |
| 폰트 | `CommonEnum.EFont.Jua` (재사용) | 모든 `CHText` — 현재 UI 풍 |
| 시각 테마 | `Assets/Dark UI/` (재사용) | 사다리 선 포함 동일 톤 |
| 표시 문자열 | String.json 신규 stringID — "그리기" / "참가자·결과 수가 달라요" / 매핑 포맷 등 | 다음 빈 ID 순차 할당 |

---

## 7. ChvjPackage 연동 포인트 (Rule 03)

- **UI 컴포넌트**: `CHButton`/`CHText`/`TMP_InputField`/`CustomScrollView`. Legacy 직접 사용 금지.
- **풀링**: 세로줄·가로줄은 입력 수에 따라 가변 → `CHMPool` Pop/Push(또는 동적 개수만큼 풀). `Object.Instantiate` 금지. 셀 `OnEnable`/`OnDisable` 리셋.
- **사운드**: `CHMSound` Click hook.
- **리소스**: 선/노드 프리팹 `CHMResource` enum-key 로드.
- **문자열**: `JsonManager.GetStringData` 경유.

---

## 8. 엣지 케이스 / 구현 메모

- 참가자 수 ≠ 결과 수 → 그리기 차단 + 안내.
- 최소 2명. 1명 이하 차단.
- 가로줄 무작위 시 같은 높이 인접 충돌 방지(한 노드에 좌·우 동시 금지).
- 경로 추적 중 이탈 시 Tween kill.
- 순열 산출 로직을 렌더와 분리(분포·1대1 보장 테스트).

---

## 9. 추후 확장 (P1 범위 밖)

- 참가자 상한 확대(스크롤 사다리).
- 결과 가림(뽑기 전 결과 숨김) 모드.
- 사다리 단수(가로줄 밀도) 조절.

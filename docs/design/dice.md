# 기획서 — dice (주사위)

> 결정 도우미 도구 풀 / **우선순위 P0**
> 작성: game-designer 단계 (구현 전). 코드 없음 — 본 문서는 gameplay-programmer 의 구현 입력.

---

## 1. 개요

- **한 줄 컨셉**: 원하는 개수·면 수의 주사위를 굴려 각 눈과 합계를 보여주는 도구.
- **결정 단위**: 1~N개 주사위, 각 면 균등 확률.
- **차별점**: coin_flip(2면 고정)보다 자유도 높은 다면·다개수. 보드게임 보조용.

---

## 2. 입력 (Input)

| 항목 | 형태 | 범위 / 기본값 | 비고 |
|---|---|---|---|
| 주사위 개수 | 스테퍼(`CHButton` − / +) + `CHText` 카운트 | 1 ~ 6, 기본 1 | 6개 초과는 화면·합계 가독성 위해 P0 상한 |
| 면 수 | 토글 그룹 `CHToggle` (4 / 6 / 8 / 10 / 12 / 20) | 기본 6 (d6) | 표준 다이스 면수 |
| 굴리기 | `CHButton` (굴리기) | — | 단일 액션 |

- 개수는 인풋 직접 타이핑 대신 **스테퍼**로 제한(범위 이탈·빈 입력 에러 원천 차단). 상·하한 도달 시 해당 방향 버튼 비활성.
- 면 수 6종은 가로 토글. 선택 1개만 활성(라디오).

---

## 3. 동작 (Behavior)

1. 사용자가 개수·면 수 설정 → 굴리기 탭.
2. 클릭 사운드(`EAudio.Click`) 재생, 굴리기 버튼 입력 잠금.
3. 각 주사위가 굴림 연출 — DOTween 으로 흔들림/회전 약 0.6~0.8초. 연출 중 숫자 빠르게 셔플(시각 효과).
4. 각 주사위 결과는 `UnityEngine.Random.Range(1, faces + 1)`. 주사위별 독립 균등.
5. 멈추면 각 주사위 눈 표시 + 합계 라벨 갱신. 버튼 입력 잠금 해제.

> 결과는 굴리기 시작 시점에 확정. 셔플 연출은 시각 효과일 뿐 결과에 영향 없음.

---

## 4. 결과 (Output)

- **각 주사위 값**: 가로/그리드로 N개 표시.
  - **d6**: 주사위 눈(pip) 스프라이트 표현 권장. 그 외 면수(4/8/10/12/20): 숫자 텍스트 표현.
  - 혼선 방지를 위해 P0 는 **모든 면수 숫자 텍스트 통일**도 허용(연출 단순화). 스프라이트 pip 은 d6 한정 추후 확장으로 둬도 됨.
- **합계**: 하단 `CHText` "합계: {0}". 1개일 때도 동일 표기(일관성).
- 모든 표시 문자열은 String.json stringID 경유(§6).

---

## 5. UI / 화면 흐름

### 5.0 현재 UI 풍 준수 (필수)

> 다중 주사위·합계 등 **고유 메커니즘은 유지**하되, 외형·레이아웃·컴포넌트는 현재 앱 풍에 맞춘다. (룰렛 스핀으로 대체하지 않음 — 다중 주사위+합계는 단일 룰렛으로 표현 불가)

- **테마**: `Assets/Dark UI/` 다크 패널·버튼 스타일 그대로. 주사위 셀도 동일 톤.
- **폰트**: `EFont.Jua` — 모든 텍스트(개수·합계·주사위 눈 숫자)는 `CHText`.
- **컴포넌트**: 스테퍼·굴리기 `CHButton`, 면수 `CHToggle`, 라벨 `CHText`. 기존 씬 위젯 세트 동일.
- **레이아웃 관례**: 상단 메뉴(뒤로) 버튼 · 중앙 콘텐츠 · 하단 액션 버튼.

### 5.1 배치 / RouletteScene 와이어링

- `RandomNumberScene` 와 동일하게 **`RouletteScene` 의 자식 GameObject**(active 토글). 별도 씬·프리팹 아님(서브 도구는 씬 내 GameObject).
- 연결(`RouletteScene.cs`): ① `ERouletteMenu.Dice` 추가 → ② `[SerializeField] DiceScene _diceScene` → ③ `SetManagement()` 에서 `SetRouletteSceneAccess(this)` → ④ `ShowScene()` switch `case Dice` + `liMainSceneObj` 토글 → ⑤ 메뉴 진입 버튼 `_liMenu` 등록.
- 복귀: `Close()` → `SetActive(false)` + `ShowScene(ERouletteMenu.Menu)`.

### 5.2 구성

- 상단: 메뉴(뒤로) 버튼 `CHButton`
- 설정: 개수 스테퍼(− `CHText` +) / 면 수 토글 그룹(`CHToggle` ×6)
- 중앙: 주사위 표시 영역(주사위 셀들 배치 — 그리드, Dark UI 패널 위)
- 하단: 굴리기 `CHButton` + 합계 라벨 `CHText`
- **상태 리셋**: `OnEnable` 에서 개수(1)·면수(6)·주사위 표시·합계 초기화.

---

## 6. Enum · 에셋 키 · 문자열 매핑

> Rule 03 §2 — Enum 값명 = 에셋 파일명 정확히 일치.

| 종류 | 키 | 비고 |
|---|---|---|
| 메뉴 항목 | `CommonEnum.ERouletteMenu.Dice` (신규 추가) | 호스트 메뉴 진입 키 |
| 주사위 셀 프리팹 | `Assets/AddressableResource/Prefab/DiceCell` | 풀링 prototype(origin), 재사용 단일 셀 |
| d6 눈 스프라이트(선택) | `DicePip1`..`DicePip6` 또는 단일 스프라이트시트 | d6 pip 표현 시. 숫자 통일이면 불필요 |
| 클릭 사운드 | `CommonEnum.EAudio.Click` (재사용) | — |
| 폰트 | `CommonEnum.EFont.Jua` (재사용) | 모든 `CHText` 적용 — 현재 UI 풍 |
| 시각 테마 | `Assets/Dark UI/` 패널·버튼 (재사용) | 주사위 셀도 동일 톤 |
| 표시 문자열 | String.json 신규 stringID — "합계: {0}" / "굴리기" 등 | RandomNumber(140·141) 다음 빈 ID 순차 할당 |

---

## 7. ChvjPackage 연동 포인트 (Rule 03)

- **UI 컴포넌트**: `CHButton`(스테퍼·굴리기), `CHText`(개수·합계·각 주사위 숫자), `CHToggle`(면수). Legacy 직접 사용 금지.
- **풀링(필수)**: 주사위 셀은 런타임 개수 가변(1~6)이므로 **`CHMPool`** 로 Pop/Push. `Object.Instantiate` 금지(Rule 03 §4). 진입 시 6 + α 워밍 권장. 셀은 `OnEnable`/`OnDisable` 상태 리셋.
  - 다수 주사위를 스크롤 없이 그리드 고정 배치하면 단순 풀(`CHMPool`)로 충분. 리스트 스크롤이 필요해지면 `CHPoolingScrollView` 로 승격.
- **사운드**: `CHMSound` Click hook.
- **리소스**: 셀 프리팹·스프라이트 `CHMResource` enum-key 로드.
- **문자열**: `JsonManager.GetStringData` 경유.

---

## 8. 엣지 케이스 / 구현 메모

- 개수 상·하한(1/6) 도달 시 스테퍼 버튼 비활성 처리.
- 면수 변경 시 진행 중 굴림 없으면 즉시 반영, 굴림 중이면 잠금.
- d20 등 큰 면수에서 셔플 연출 숫자 가독성 — 폰트 크기/자릿수 고려.
- 결과 산출(`Range`)을 연출과 분리한 메서드로 두어 합계·분포 테스트 가능하게.
- 주사위 다수일 때 합계 오버플로 걱정 없음(6×20=120 상한).

---

## 9. 추후 확장 (P0 범위 밖)

- 주사위 개수 상한 확대(스크롤 리스트로 전환).
- 굴림 결과 히스토리.
- d6 물리 굴림(3D) 연출.
- 모디파이어(+N) / 합계 비교(누가 큼) 모드.

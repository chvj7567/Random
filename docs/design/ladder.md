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

- 참가자 수 = 결과 수 강제. 불일치 시 그리기 버튼 비활성 + 안내 문자열(stringID 2064, §6.3).
- **상한 8 강제 방식 (R3 확정)**: 각 리스트가 8개에 도달하면 해당 리스트의 **추가(+) 버튼을 비활성(`Interactable = false`)** 한다. 9번째를 받아 무시하는 방식이 아니라 입력 자체를 막아 사용자에게 상한을 즉시 알린다. 7→8 추가 직후 비활성, 8→7 삭제 직후 재활성. (참가자·결과 리스트 각각 독립 적용.)
- **입력 검증 규칙 (R2 확정 — `CustomRandomScene` 과의 차이)**: `CustomRandomScene` 은 검증 없이 `Add` 하지만, 사다리는 매핑표(`참가자 → 결과`) 가독성을 위해 다음을 적용한다:
  - **빈/공백 이름 무시**: 입력칸이 비었거나 공백만이면 추가 버튼을 눌러도 리스트에 넣지 않는다(무음 무시, `string.IsNullOrWhiteSpace` 기준).
  - **중복 이름 허용**: 같은 이름 중복 입력은 허용한다. 매핑표는 입력 순서(인덱스)로 행을 구분하므로 동명이인이 있어도 결과 매칭 자체는 인덱스 기준으로 모호함이 없다. 중복 차단은 P1 범위 밖.

---

## 3. 동작 (Behavior)

1. 참가자·결과를 각각 입력 → 그리기 버튼 탭.
2. 클릭 사운드(`EAudio.Click`), 버튼 입력 잠금.
3. 세로줄 N개 + **무작위 가로줄(rung)** 생성. 가로줄은 인접 세로줄 사이에 무작위 배치(같은 높이 중복 금지). 밀도 규칙은 §3.1.
4. 각 참가자 위치에서 출발 → 가로줄 만나면 좌/우 이동하는 **경로 추적**. DOTween 으로 순차 추적 애니메이션(참가자별 색 구분).
5. 도착한 결과를 매핑 확정. 전원 완료 시 매핑표 노출 + 버튼 잠금 해제.

> **결과 = 가로줄 배치로 결정되는 순열**. 시각 추적은 그 순열을 보여줄 뿐. 결과 산출(순열 계산)을 렌더와 분리해 테스트 가능하게.

### 3.1 가로줄(rung) 밀도 규칙 (R1 확정)

- **높이 구간 수 R**: `R = N * 3` 로 고정한다 (참가자 수 N 비례). 예: N=2 → R=6, N=4 → R=12, N=8 → R=24. 세로줄이 많을수록 섞임이 부족해 보이지 않도록 구간을 비례 확대. (범위가 아닌 단일 산식 — N 당 구간 3개.)
- **각 구간 배치**: 매 높이 구간마다 인접 세로줄 쌍 `(i, i+1)` 중 무작위로 하나를 골라 가로줄 1개를 놓을지 결정한다. **구간당 평균 1개 미만**으로, 각 인접 쌍에 대해 확률 `p = 0.5` 로 독립 시도하되 같은 구간 내에서 **이미 가로줄이 닿은 노드에는 다시 놓지 않는다**(한 노드에 좌·우 동시 금지 — §8). 한 구간을 좌→우로 훑으며 가로줄을 놓은 직후 다음 쌍은 건너뛰어 인접 충돌을 구조적으로 차단.
- **기본값 한 줄**: N=4 기준 R=12 구간, 구간당 인접쌍 시도 확률 0.5 → 사다리 1개에 대략 6~10개 가로줄. 전원 결과가 충분히 섞이는 밀도(전단사는 인접 교환 합성으로 자동 보장 — 밀도와 무관하게 1대1 유지).

---

## 4. 결과 (Output)

- 각 참가자 → 결과 매핑표 — 각 행 `string.Format(GetStringData(2066), 참가자명, 결과명)` → `참가자명 → 결과명` (stringID 2066, §6.3).
- 개별 참가자 탭 시 해당 경로만 하이라이트(선택 기능).
- 사다리 선(세로줄·가로줄)은 Dark UI 톤(어두운 배경 + 밝은 라인). 교점은 두 선의 교차로 표현(별도 마커 없음 — §6.1).
- 모든 표시 문자열(안내·매핑 라벨)은 String.json stringID 경유(§6).

---

## 5. UI / 화면 흐름

### 5.0 현재 UI 풍 준수 (필수)

> 사다리 렌더링·경로 추적 등 **고유 메커니즘은 유지**하되, 외형·레이아웃·컴포넌트는 현재 앱 풍에 맞춘다. (사다리는 룰렛으로 표현 불가 — 자체 렌더)

- **테마**: `Assets/Dark UI/` 다크 패널·버튼. 사다리 선도 동일 톤.
- **폰트**: `EFont.Jua` — 모든 텍스트 `CHText`.
- **컴포넌트**: 추가/삭제·그리기 `CHButton`, 라벨 `CHText`, 이름 입력 `TMP_InputField`, 리스트 `CustomScrollView`(기존). 기존 씬 위젯 세트 동일.
- **레이아웃 관례**: 상단 메뉴(뒤로) 버튼 · 중앙 입력+사다리 · 하단 그리기 버튼.

### 5.1 배치 / RandomScene 와이어링

> 호스트는 `Assets/Scripts/Scene/RandomScene.cs`. `CustomRandomScene` / `CoinFlipScene` 와 동일하게 **`RandomScene` 의 자식 GameObject**(active 토글). 별도 씬·프리팹 아님. 표준 참고: `CoinFlipScene.cs`, `CustomRandomScene.cs`.

**LadderScene 측 (신규 클래스):**

- 시그니처: `public class LadderScene : MonoBehaviour, IRouletteBackButton`
  - `IRouletteBackButton` 은 호스트가 ESC/뒤로가기를 라우팅하는 현행 인터페이스 — 이름에 "Roulette" 가 있으나 **현행 식별자 그대로 유지**(개명하지 않음).
- 주입 받기: `private IRandomSceneAccess _randomSceneAccess;` + `public void SetRandomSceneAccess(IRandomSceneAccess randomSceneAccess) => _randomSceneAccess = randomSceneAccess;`
- 복귀:
  ```
  public void Close()
  {
      gameObject.SetActive(false);
      _randomSceneAccess.ShowScene(CommonEnum.ERouletteMenu.Menu);
  }
  ```
  뒤로 버튼 `_menuButton.OnClick(() => Close());` (CoinFlipScene 과 동일).

**RandomScene 측 (기존 파일 수정):**

- ① `[SerializeField] private LadderScene _ladderScene;` 필드 추가 (다른 `_xxxScene` 필드와 같은 블록).
- ② `SetManagement()` 안에 한 줄 추가: `_ladderScene.SetRandomSceneAccess(this);`
- ③ `ShowScene()` 최상단 일괄 비활성 목록에 `_ladderScene.gameObject.SetActive(false);` 추가 (다른 씬과 동일).
- ④ `ShowScene()` switch 에 신규 case 추가 — 다른 case 와 동일 패턴:
  ```
  case CommonEnum.ERouletteMenu.Ladder:
      {
          _mainRouletteUI = _ladderScene;

          foreach (GameObject obj in liMainSceneObj)
          {
              obj.SetActive(false);
          }

          _ladderScene.gameObject.SetActive(true);
      }
      break;
  ```
- **옛 메뉴 진입 버튼 직접 등록(`_liMenu`) 단계 없음** — 현행 메뉴는 데이터 주도(§6 `MenuCardCatalog`). 카드 1행 추가 시 클릭→`ShowScene(ERouletteMenu.Ladder)` 라우팅이 자동(`MenuCardCell`→`ShowScene`). 진입 버튼 코드 불필요.

**enum**: `CommonEnum.ERouletteMenu` (타입명 유지) 에 값 `Ladder` 만 끝(`Lotto2` 뒤)에 추가 — §6 참조.

### 5.2 구성

- 상단: 메뉴(뒤로) 버튼 `CHButton`
- 입력: 참가자 리스트 / 결과 리스트(각각 입력칸 + 추가·삭제 버튼 + `CustomScrollView`)
- 중앙: 사다리 그리기 영역(Dark UI 패널)
- 하단: 그리기 `CHButton` + 안내/매핑 라벨 `CHText`
- **상태 리셋**: `OnEnable` 에서 참가자·결과 리스트·사다리·매핑 초기화.

---

## 6. Enum · 에셋 키 · 문자열 매핑

> Rule 03 §2 — Enum 값명 = 에셋 파일명 정확히 일치.

### 6.1 Enum · 에셋 키

| 종류 | 키 | 비고 |
|---|---|---|
| 메뉴 항목 | `CommonEnum.ERouletteMenu.Ladder` (신규 값) | enum 끝(`Lotto2` 뒤)에 추가. 타입명 유지 |
| 메뉴 아이콘 | `CommonEnum.EMenuIcon.MenuLadder` (신규 값) | **동명 아이콘 에셋 `MenuLadder.png` 필요** (Rule 03 §2 파일명 일치). 카드 `iconKey` |
| 사다리 세로줄 origin | `LadderScene._verticalOrigin` (인스펙터 `[SerializeField]` origin 프리팹) | 동적 세로줄 — `CHMPool` 풀링 prototype. **Addressable/CHMResource 로드 대상 아님**(앱 내부 렌더링 프리미티브, enum 키 불필요) |
| 사다리 가로줄(rung) origin | `LadderScene._rungOrigin` (인스펙터 `[SerializeField]` origin 프리팹) | 동적 가로줄 — `CHMPool` 풀링 prototype. 로드 방식 위와 동일 |
| 클릭 사운드 | `CommonEnum.EAudio.Click` (재사용) | — |
| 폰트 | `CommonEnum.EFont.Jua` (재사용) | 모든 `CHText` — 현재 UI 풍 |
| 시각 테마 | `Assets/Dark UI/` (재사용) | 사다리 선 포함 동일 톤 |

**`MenuLadder` 아이콘 비주얼**: 다크 배경에 흰색 아웃라인의 사다리 형상(세로 2줄 + 가로 단 3개) 단색 아이콘. 기존 메뉴 아이콘(`MenuNumber`/`MenuShuffle` 등 White 계열)과 동일 톤·동일 캔버스 크기. 미준비 시 임시로 `MenuShuffle` 재사용 가능하나 정식 에셋은 `MenuLadder.png` 로 등록.

**사다리 origin 프리팹 — 2종 확정 (CHMResource enum-key 로드 대상 아님)**: 사다리 선은 외부 에셋 로드가 필요 없는 **앱 내부 렌더링 프리미티브**다. 따라서 `CommonEnum` 에 prefab 키 enum 을 신설하지 않고, `LadderScene` 의 `[SerializeField]` origin 프리팹 참조를 인스펙터로 와이어링한 뒤 `CHMPool.Pop/Push` 로 풀링한다 (ChvjPackage `BuildModalPopup` 의 `_origin` 패턴과 동일).

| origin | 필드 | 역할 | 비고 |
|---|---|---|---|
| 세로줄 | `LadderScene._verticalOrigin` | 참가자별 세로 1줄 (N개 인스턴스) | Image 1개로 구성된 얇은 수직 라인. Dark UI 톤(밝은 라인) |
| 가로줄(rung) | `LadderScene._rungOrigin` | 인접 세로줄을 잇는 가로 단 (가변 개수) | Image 1개로 구성된 얇은 수평 라인. 세로줄과 동일 톤 |

- **노드(교점) 마커 별도 프리팹 없음** — 교점은 세로줄·가로줄 Image 의 교차로 시각 표현된다. 매핑 결과는 §4 매핑표(텍스트, stringID 2066)로 전달하므로 교점에 별도 마커 프리팹을 두지 않는다(P1 범위 내 불필요).
- **풀링 워밍 count 권장값** (하드 상한이 아닌 사전 워밍 힌트 — 소진 시 `CHMPool` 자동 확장): 상한 N=8 기준 — 세로줄은 N=8 → `CreatePool(_verticalOrigin, count: 8)`. 가로줄은 §3.1 의 `R = N*3` 구간에서 "구간당 평균 1개 미만" 기준으로 구간 수만큼 잡아 `CreatePool(_rungOrigin, count: 24)`. (검산: N=8 → R = 8×3 = 24 구간, 평균 1개 미만이므로 24개 워밍이면 대다수 케이스를 커버. 실제 가로줄 수가 24를 넘으면 풀이 자동 확장.) `LadderScene` 진입(`OnEnable` 1회 또는 최초 그리기 전)에 두 풀을 워밍한다.

### 6.2 메뉴 카드 등록 (데이터 주도)

> 현행 메뉴는 `MenuCardCatalog.BuildMenuCards()` 에 `MenuCardData` 1행 추가로 노출된다. 클릭 라우팅은 자동(`MenuCardCell` → `ShowScene`). 아래 1행을 `Lotto2` 행 뒤에 추가:

| 필드 | 값 |
|---|---|
| `menu` | `CommonEnum.ERouletteMenu.Ladder` |
| `iconKey` | `CommonEnum.EMenuIcon.MenuLadder` |
| `titleStringID` | `2061` (사다리타기) |
| `descStringID` | `2062` (사다리를 타고 결과 매칭) |

### 6.3 String.json 신규 stringID (2061~2066 확정)

> 현재 max stringID = 2060. **2061부터** 순차 할당. 아래 표를 String.json 끝에 그대로 추가(korean/english 둘 다). 본문/표가 참조하는 문자열은 모두 이 ID 로 호명한다.

| stringID | 용도 | korean | english |
|---|---|---|---|
| 2061 | 카드 title | `사다리타기` | `Ladder` |
| 2062 | 카드 desc | `사다리를 타고 결과 매칭` | `Match results by ladder` |
| 2063 | "그리기" 버튼 라벨 | `그리기` | `Draw` |
| 2064 | 참가자≠결과 수 안내 | `참가자와 결과 수를 맞춰주세요` | `Match the number of players and results` |
| 2065 | 최소 2명 미만 안내 | `최소 2명이 필요해요` | `At least 2 players are required` |
| 2066 | 매핑표 포맷 | `{0} → {1}` | `{0} → {1}` |

- `2063` — §5.2 하단 "그리기" `CHButton` 라벨.
- `2064` — §2 참가자 수 ≠ 결과 수 불일치 안내(§8 검증).
- `2065` — §8 최소 2명 미만 차단 안내.
- `2066` — §4 매핑표 각 행 라벨 포맷(`string.Format(id, 참가자명, 결과명)`).

---

## 7. ChvjPackage 연동 포인트 (Rule 03)

- **UI 컴포넌트**: `CHButton`/`CHText`/`TMP_InputField`/`CustomScrollView`. Legacy 직접 사용 금지.
- **풀링 / 선 프리팹 정의 (BLOCKER 해소 — §6.1 과 단일 정합)**: 세로줄·가로줄은 입력 수에 따라 가변이며 **`CHMResource` enum-key 로드 대상이 아니다**(앱 내부 렌더링 프리미티브 — 외부 Addressable 등록 불필요). 대신 `LadderScene._verticalOrigin` / `_rungOrigin` 2종을 `[SerializeField]` origin 프리팹으로 인스펙터 와이어링하고 `CHMPool.Pop(origin, parent)` / `CHMPool.Push(poolable)` 로 생성·반환한다. `Object.Instantiate` 금지. 셀 `OnEnable`/`OnDisable` 리셋. 워밍: `CreatePool(_verticalOrigin, 8)` + `CreatePool(_rungOrigin, 24)` (근거·검산 §6.1). 입력 리스트(참가자/결과)는 기존 `CustomScrollView`(CHPoolingScrollView 기반) 2 인스턴스 재사용 — 사다리 선 풀과는 별개.
- **사운드**: `CHMSound` Click hook.
- **문자열**: `JsonManager.GetStringData` 경유.

---

## 8. 엣지 케이스 / 구현 메모

- 참가자 수 ≠ 결과 수 → 그리기 버튼 비활성 + 안내(stringID 2064).
- 최소 2명. 1명 이하면 그리기 차단 + 안내(stringID 2065).
- 상한 8 — 8개 도달 시 해당 리스트 추가(+) 버튼 비활성(§2 R3). 9번째 무시 방식 아님.
- 입력 검증 — 빈/공백 이름 무시, 중복 이름 허용(§2 R2). `CustomRandomScene` 과 의도적으로 다름.
- 가로줄 무작위 시 같은 높이 인접 충돌 방지(한 노드에 좌·우 동시 금지 — §3.1).
- 경로 추적 중 이탈(`OnDisable`) 시 Tween kill + 풀 반환.
- 순열 산출 로직을 렌더와 분리(분포·1대1 보장 테스트). 전단사는 인접 교환 합성으로 자동 — 인위 제약 추가 금지.

---

## 9. 추후 확장 (P1 범위 밖)

- 참가자 상한 확대(스크롤 사다리).
- 결과 가림(뽑기 전 결과 숨김) 모드.
- 사다리 단수(가로줄 밀도) 조절.

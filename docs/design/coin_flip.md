# 기획서 — coin_flip (동전 던지기)

> 결정 도우미 도구 풀 / **우선순위 P0**
> 작성: game-designer 단계 (구현 전). 코드 없음 — 본 문서는 gameplay-programmer 의 구현 입력.

---

## 1. 개요

- **한 줄 컨셉**: 버튼을 누르면 동전이 회전하다 앞/뒷면으로 멈춰 결정을 내려주는 가장 단순한 양자택일 도구.
- **결정 단위**: 2지선다 (앞면 / 뒷면), 균등 확률 50/50.
- **차별점**: yes_no 와 동일하게 즉답형이지만 *회전 애니메이션 연출* 과 *연속 던지기(통계)* 가 핵심 재미 요소.

---

## 2. 입력 (Input)

| 항목 | 형태 | 기본값 | 비고 |
|---|---|---|---|
| 던지기 트리거 | `CHButton` (던지기) | — | 단일 액션 버튼 |
| 연속 횟수 | 토글 그룹 `CHToggle` (1 / 3 / 5 / 10) | 1 | 한 번에 N회 연속 던지기 |

- 텍스트 입력 없음 — 가장 단순한 도구. 질문 입력조차 없음(질문이 필요하면 yes_no 사용).
- 연속 횟수 토글은 가로 4분할. 선택값은 OnEnable 시 1로 리셋.

---

## 3. 동작 (Behavior)

1. 사용자가 연속 횟수(1/3/5/10) 선택 → 던지기 버튼 탭.
2. 버튼 탭 순간 클릭 사운드(`EAudio.Click`) 재생, 던지기 버튼 입력 잠금(중복 탭 방지).
3. 동전 오브젝트가 회전 애니메이션 — DOTween 으로 축 회전(플립) 약 0.8~1.0초, ease-out 으로 감속.
4. 각 회차 결과는 `UnityEngine.Random.Range(0, 2)` → 0=앞면, 1=뒷면. **연출과 무관하게 결과는 던지기 시작 시점에 확정**(애니메이션은 확정된 면으로 정렬되어 멈춤).
5. 멈추면 결과면 스프라이트 노출 + 결과 라벨 갱신.
6. 연속 N회면 회차마다 짧은 간격(약 0.25초)으로 순차 던지기, 마지막에 요약 표시. 버튼 입력 잠금 해제.

> 확률 보정 없음(완전 균등). "특정 면이 더 자주" 같은 가중치는 P0 범위 밖 — 필요 시 별도 도구(weighted)로 분리.

---

## 4. 결과 (Output)

- **단일(1회)**: 가운데 동전이 해당 면으로 정지 + 결과 라벨 "앞면" / "뒷면".
- **연속(N회)**: 결과 리스트(예: `앞 · 뒤 · 뒤 · 앞 · 뒤`) + 요약 카운트 `앞면 2 / 뒷면 3`.
  - 리스트가 길어지는 10회는 셀 재사용이 필요하므로 **`CHPoolingScrollView` 패턴**(Rule 03 §3) 사용 권장. 1/3/5 회는 고정 슬롯으로도 충분.
- 모든 표시 문자열("앞면"/"뒷면"/"앞면 N"/"뒷면 N")은 **String.json stringID** 로 관리(아래 §6).

---

## 5. UI / 화면 흐름

### 5.0 현재 UI 풍 준수 (필수)

> 코인 회전 연출 등 **고유 메커니즘은 유지**하되, 외형·레이아웃·컴포넌트는 현재 앱 풍에 맞춘다.

- **테마**: `Assets/Dark UI/` 에셋 팩의 다크 패널·버튼 스타일을 그대로 사용. 신규 위젯도 동일 톤(어두운 배경 + 밝은 텍스트).
- **폰트**: `EFont.Jua` — 모든 텍스트는 `CHText` 로 표시(Jua 폰트 적용). Legacy Text 금지.
- **컴포넌트**: 버튼 `CHButton`, 라벨 `CHText`, 선택 `CHToggle`. (현재 씬 `RandomNumberScene`/`CustomRandomScene`/`RandomExampleScene` 와 동일한 위젯 세트)
- **레이아웃 관례**: 기존 서브 씬과 동일 — **상단(좌상단) 메뉴(뒤로) 버튼 · 중앙 콘텐츠 · 하단 액션 버튼**. 별도 새 레이아웃 발명 금지.

### 5.1 배치 / RouletteScene 와이어링

- 기존 `RandomNumberScene` 와 동일하게 **`RouletteScene` 의 자식 GameObject**(active 토글)로 구현. 별도 씬 파일·프리팹 아님(서브 도구는 모두 씬 내 GameObject 선례).
- 연결 작업(`RouletteScene.cs`):
  1. `CommonEnum.ERouletteMenu` 에 `CoinFlip` 추가
  2. `RouletteScene` 에 `[SerializeField] private CoinFlipScene _coinFlipScene;` 참조
  3. `SetManagement()` 에서 `_coinFlipScene.SetRouletteSceneAccess(this);`
  4. `ShowScene()` switch 에 `case ERouletteMenu.CoinFlip` 추가 + `liMainSceneObj` 토글 / 타 서브씬 off
  5. 메뉴 화면(`liMainSceneObj`)에 진입 버튼(`CHButton`) 추가 → `_liMenu` 에 `{menu: CoinFlip, buttonEx}` 등록
- 복귀: `Close()` → `gameObject.SetActive(false)` + `_rouletteSceneAccess.ShowScene(ERouletteMenu.Menu)` (선례 동일).

### 5.2 구성

- 상단: 메뉴(뒤로) 버튼 `CHButton`
- 중앙: 동전 이미지 (`Image`) — 던지기 시 24프레임 토스 시퀀스 재생, 멈추면 결과 이미지(앞=골드/뒤=실버)로 교체
- 하단: 연속 횟수 토글 그룹(`CHToggle` ×4) + 던지기 `CHButton`
- **선택 횟수 표시**(`CHText`, 토글 위): 현재 고른 횟수를 "선택: N회" 로 상시 표시. 토글 변경 시 실시간 갱신(`Update` 에서 `GetSelectedCount` 변동 감지).
- **진행 횟수 표시**(`CHText`, 코인 아래): **2회 이상** 던질 때만 "현재 / 전체"(예 "3 / 5") 표시. 1회면 미표시. 종료 시 비우고 요약으로 전환.
- 결과 영역: 결과 라벨 `CHText` — **단일(1회)** 던지기 때 "앞면/뒷면", 연속 종료 시 "앞면 X / 뒷면 Y" 요약. (연속 진행 중에는 회차 라벨 미표시 — 코인 면 + 진행표시로 갈음.)
- **상태 리셋**: `OnEnable` 에서 동전 기본면·결과 라벨·진행표시·토글(1) 초기화 (서브씬 재진입 시 이전 결과 잔존 방지).

---

## 6. Enum · 에셋 키 · 문자열 매핑

> Rule 03 §2 — Enum 값명 = 에셋 파일명 정확히 일치(대소문자 포함).

| 종류 | 키 | 비고 |
|---|---|---|
| 메뉴 항목 | `CommonEnum.ERouletteMenu.CoinFlip` (신규 추가) | 호스트 메뉴 진입 키 |
| 토스 애니메이션 프레임 | `Assets/Sprites/CoinToss/CoinToss_00~23.png` (24프레임) | `CoinFlipScene._tossFrames` 인스펙터 배열. 던지기 연출(직접 참조, 비-Addressable) |
| 결과 이미지 (앞면) | `Assets/Sprites/CoinToss/CoinToss_23_gold.png` | `_headResultSprite`. 골드 착지 코인 |
| 결과 이미지 (뒷면) | `Assets/Sprites/CoinToss/CoinToss_23_silver.png` | `_tailResultSprite`. 실버 착지 코인 |
| 동전 스프라이트 (폴백) | `Assets/AddressableResource/Sprite/Coin.png` — `CommonEnum.ECoin.Coin` | `_tossFrames` 미와이어링 시 폴백용(틴트 방식). Addressables "Resource" |
| 클릭/던지기 사운드 | `CommonEnum.EAudio.Click` (기존 재사용) | 신규 flip 사운드 원하면 `EAudio.Coin` 추가 검토 |
| 폰트 | `CommonEnum.EFont.Jua` (기존 재사용) | 모든 `CHText` 에 적용 — 현재 UI 풍 |
| 시각 테마 | `Assets/Dark UI/` 패널·버튼 스타일 (기존 재사용) | 신규 에셋 최소화, 기존 톤 유지 |
| 표시 문자열 | String.json — 142 "앞면" / 143 "뒷면" / 144 "앞면 {0}" / 145 "뒷면 {0}" / 146 "던지기" / 147 "동전 던지기" / 148 "선택: {0}회" / 149 "{0} / {1}"(진행) | RandomNumber 가 140·141 사용 중 → 142 부터 순차 할당 |

- **던지기 연출 = 24프레임 토스 시퀀스**(동전이 튀어올라 회전하다 착지). 재생 중에는 색을 바꾸지 않고 프레임 자연색 그대로.
- **앞/뒷면 결과는 착지 시 실제 이미지 교체로 구분** — 앞면=`CoinToss_23_gold`(골드), 뒷면=`CoinToss_23_silver`(실버). (색 틴트 방식에서 변경된 디자인 결정, 2026-06-17. 사용자 요구: 애니메이션 종료 시 골드/실버 결과 이미지 노출.) 진입 기본 표시도 골드(앞면) 이미지.
- **폴백**: `_tossFrames` 미와이어링 시 단일 `Coin` 스프라이트 + 색 틴트(앞 금/뒤 은) + DOTween 회전으로 하위호환 동작.
- 연속 결과 셀(`CHPoolingScrollView`)은 미구현 — 현재 앞/뒤 카운트 요약 텍스트로 동작(§4 참조). 리스트 셀 풀링은 추후 확장.

---

## 7. ChvjPackage 연동 포인트 (Rule 03)

- **UI 컴포넌트**: 버튼 `CHButton`, 결과/요약 라벨 `CHText`, 횟수 선택 `CHToggle`. Legacy Text/Button 직접 사용 금지.
- **사운드**: `CHMSound`(Click hook) — `CHButton.ClickSoundHook` 부팅 시 등록 정책 그대로 사용.
- **리소스 로드**: 스프라이트는 `CHMResource` enum-key 로드. 하드코딩 문자열 키 금지.
- **풀링**: 10회 연속 결과 리스트 셀은 `CHPoolingScrollView<TItem,TData>` (BuildModalPopup 패턴). 동전 자체는 단일 오브젝트라 풀링 불필요.
- **문자열**: 모든 라벨은 `JsonManager.GetStringData(stringID)` 경유(직접 리터럴 금지 — 표시 텍스트 String.json 단일화 정책).

---

## 8. 엣지 케이스 / 구현 메모

- 애니메이션 진행 중 던지기 버튼 재탭 → 무시(입력 잠금). 메뉴 버튼은 진행 중에도 동작(중단 후 복귀, 결과는 버림).
- 연속 던지기 도중 서브씬 이탈 시 코루틴/Tween kill 로 누수 방지.
- 결과 확정은 연출 시작 시점(시각 연출이 결과를 바꾸지 않음) — 테스트 시 연출과 분리해 확률 검증 가능하도록 결과 산출 로직을 메서드로 분리.

---

## 9. 추후 확장 (P0 범위 밖)

- 면별 가중치(편향 동전).
- 연속 통계 누적(세션 전체 앞/뒤 비율).
- 햅틱/사운드 다양화.

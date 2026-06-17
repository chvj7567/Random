# 기획서 — yes_no (예 / 아니오)

> 결정 도우미 도구 풀 / **우선순위 P0**
> 작성: game-designer 단계 (구현 전). 코드 없음 — 본 문서는 gameplay-programmer 의 구현 입력.

---

## 1. 개요

- **한 줄 컨셉**: 질문을 적고 버튼을 누르면 "예" 또는 "아니오" 로 답해 주는 가장 단순한 결정 도구.
- **결정 단위**: 2지선다(예 / 아니오), 균등 확률 50/50.
- **차별점**: coin_flip 과 결과 구조는 같지만 *질문 입력* 을 받아 맥락을 보여주는 즉답형. 연출보다 즉시성·간결함이 핵심.

> ✅ **결정(2026-06-17): 옵션 A — 별도 독립 도구로 신설.** (사용자 확정)
> `RandomExampleScene._randomYesNoButton` 이 이미 `[예, 아니오]` 리스트를 `UIRoulette` 룰렛으로 뽑지만, 본 도구는 **질문 입력 + 즉답형(룰렛 없이 바로 결과)** 이라는 다른 UX 로 차별화한다. 기존 룰렛형 예/아니오는 그대로 두고, 즉답형 도구를 메뉴에 별도 추가한다(두 진입점 공존).

---

## 2. 입력 (Input)

| 항목 | 형태 | 기본값 | 비고 |
|---|---|---|---|
| 질문 | `TMP_InputField` | 빈 값(선택 입력) | 비워도 동작. 입력 시 결과 위에 질문 표시 |
| 결정 트리거 | `CHButton` (결정) | — | 단일 액션 |

- 질문은 **선택 입력**. 빈 값이어도 에러 없이 결과만 출력(RandomNumber 처럼 빈/오류 입력을 에러 문자열로 막는 게이트가 여기선 불필요).
- 텍스트 길이 과다 시 입력 필드 자체에서 줄바꿈/스크롤 처리(별도 제한 없음, 표시 영역만 클램프).

---

## 3. 동작 (Behavior)

1. (선택) 질문 입력 → 결정 버튼 탭.
2. 클릭 사운드(`EAudio.Click`) 재생.
3. 결과는 `UnityEngine.Random.Range(0, 2)` → 0=예, 1=아니오. 균등.
4. 짧은 연출(페이드/스케일 팝, 약 0.2~0.3초) 후 결과 라벨 표시. 동전처럼 긴 회전 연출은 없음(즉답 성격 유지).
5. 질문이 입력돼 있으면 결과 위에 질문을 함께 노출.

> 가장 단순한 도구 — 연속/통계 없음. 한 번에 1결정.

---

## 4. 결과 (Output)

- 중앙 결과 라벨 `CHText`: "예" / "아니오".
- 질문 입력 시: 결과 위 보조 라벨에 질문 에코.
- 색/아이콘으로 예(긍정)·아니오(부정) 시각 구분 권장(예: 초록/빨강 또는 ○/×). 색만으로 구분하지 말고 텍스트 병행(접근성).
- 표시 문자열("예"/"아니오")은 String.json stringID 경유(§6).

---

## 5. UI / 화면 흐름

### 5.0 현재 UI 풍 준수 (필수)

> 즉답형 메커니즘은 유지하되, 외형·레이아웃·컴포넌트는 현재 앱 풍에 맞춘다.

- **테마**: `Assets/Dark UI/` 다크 패널·버튼.
- **폰트**: `EFont.Jua` — 모든 텍스트 `CHText`.
- **컴포넌트**: 결정 `CHButton`, 결과/질문 라벨 `CHText`, 질문 입력 `TMP_InputField`(래퍼 미지원 — `RandomNumberScene`/`CustomRandomScene` 선례 동일).
- **레이아웃 관례**: 상단 메뉴(뒤로) 버튼 · 중앙 입력+결과 · 하단 결정 버튼.

### 5.1 배치 / RouletteScene 와이어링

- `RandomNumberScene` 와 동일하게 **`RouletteScene` 의 자식 GameObject**(active 토글).
- 연결(`RouletteScene.cs`): ① `ERouletteMenu.YesNo` 추가 → ② `[SerializeField] YesNoScene _yesNoScene` → ③ `SetManagement()` 에서 `SetRouletteSceneAccess(this)` → ④ `ShowScene()` switch `case YesNo` + `liMainSceneObj` 토글 → ⑤ 메뉴 진입 버튼 `_liMenu` 등록.
- 복귀: `Close()` → `SetActive(false)` + `ShowScene(ERouletteMenu.Menu)`.

### 5.2 구성

- 상단: 메뉴(뒤로) 버튼 `CHButton`
- 중앙: 질문 입력 `TMP_InputField` + 결과 라벨 `CHText`(+ 질문 에코 라벨)
- 하단: 결정 `CHButton`
- **상태 리셋**: `OnEnable` 에서 질문 입력·결과 라벨 초기화(이전 결과 잔존 방지) — RandomNumberScene 의 OnEnable 리셋 패턴 동일.

---

## 6. Enum · 에셋 키 · 문자열 매핑

> Rule 03 §2 — Enum 값명 = 에셋 파일명 정확히 일치.

| 종류 | 키 | 비고 |
|---|---|---|
| 메뉴 항목 | `CommonEnum.ERouletteMenu.YesNo` (신규 추가) | 호스트 메뉴 진입 키 |
| 클릭 사운드 | `CommonEnum.EAudio.Click` (재사용) | 별도 에셋 없음 |
| 폰트 | `CommonEnum.EFont.Jua` (재사용) | 모든 `CHText` 적용 — 현재 UI 풍 |
| 시각 테마 | `Assets/Dark UI/` 패널·버튼 (재사용) | 신규 에셋 최소화 |
| 예/아니오 아이콘(선택) | `IconYes` / `IconNo` | 색+아이콘 병행 표시 시 |
| 표시 문자열 | String.json 신규 stringID — "예" / "아니오" / "결정" | RandomNumber(140·141) 다음 빈 ID 순차 할당 |

- 별도 프리팹/풀링 불필요(정적 UI 만으로 충분).

---

## 7. ChvjPackage 연동 포인트 (Rule 03)

- **UI 컴포넌트**: 결정 버튼 `CHButton`, 결과/질문 라벨 `CHText`. 정적 라벨도 TMP 가 있으면 `CHText` 동반(Rule 03 §3, 정적 라벨 예외 없음). 질문 입력은 래퍼 미지원이라 `TMP_InputField` 직접 사용 허용(RandomNumberScene 선례).
- **사운드**: `CHMSound` Click hook.
- **풀링/리소스**: 동적 스폰 없음 → `CHMPool` 불필요. 아이콘 스프라이트만 `CHMResource` enum-key 로드.
- **문자열**: 모든 라벨 `JsonManager.GetStringData(stringID)` 경유(직접 리터럴 금지).

---

## 8. 엣지 케이스 / 구현 메모

- 질문 빈 값 → 결과만 표시(에러 아님).
- 연속 탭 → 매 탭마다 새 결정(잠금 불필요, 연출 짧음). 단 연출 Tween 중복 시 kill 후 재생.
- 결과 산출(`Range`)을 연출과 분리한 메서드로 두어 분포 테스트 가능하게.
- 색 구분은 텍스트와 병행(색맹 접근성).

---

## 9. 추후 확장 (P0 범위 밖)

- 제3선택지("글쎄") 옵션.
- 질문 히스토리.
- 가중치(예 편향) 모드.

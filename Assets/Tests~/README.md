# Tests~ (휴면 테스트 보관소)

이 폴더는 이름이 `~` 로 끝나 **Unity 가 컴파일·임포트에서 무시**한다. 리포지토리에는 남지만 에디터 빌드에는 영향이 없다.

## 왜 여기 있나
결정 도우미 `coin_flip` 의 EditMode 테스트(`CoinFlipTests.cs`)를 작성했으나, 현재 프로젝트는 게임 코드가 asmdef 없는 단일 `Assembly-CSharp` 다. 테스트 어셈블리는 `Assembly-CSharp` 를 참조할 수 없어(Unity 컴파일 규칙) 테스트를 컴파일하려면 production asmdef 신설이 필요하다 — 이는 모든 의존성(ChvjUnityInfra·DOTween·Admob·UniRx·Addressables·InputSystem·TMP)을 명시 참조로 옮기는 광범위 변경이라, 우선 **테스트를 휴면 보관**하기로 결정했다(2026-06-17).

## 부활 절차 (전용 테스트 인프라 작업 시)
1. `Assets/Scripts/` 에 production asmdef 신설 — 이름 `Random`. 위 의존성을 모두 명시 참조로 추가(누락 시 production 컴파일 깨짐).
2. 이 폴더를 `Assets/Tests~` → `Assets/Tests` 로 되돌린다(폴더명에서 `~` 제거).
3. `Random.Tests.EditMode.asmdef.template` → `Random.Tests.EditMode.asmdef` 로 이름 변경. `references` 에 `"Random"` 이 들어 있는지 확인(이미 박혀 있음).
4. Unity 에서 `.meta` 자동 생성 후 Test Runner(EditMode)로 실행.

## 대상 테스트 진입점 (production)
`CoinFlipScene.Flip()` / `FlipMany(int)` / `CountFaces(List<ECoinFace>)`, `enum ECoinFace { Head, Tail }`

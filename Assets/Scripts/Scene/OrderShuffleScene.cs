using ChvjUnityInfra;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;

//# 순서 정하기 — N개 이름을 Fisher-Yates 셔플로 무작위 순열을 만들어 1번~N번으로 표시하는 단일 액션 도구 (기획서 order_shuffle).
//# TeamSplitScene 에서 팀 수 스테퍼·균등 배분·색·안내 미리보기 제거, Shuffle 단일 결과로 축소.
public class OrderShuffleScene : MonoBehaviour, IRouletteBackButton
{
    //# 상단 — 메뉴(뒤로) 버튼 (기획서 §5.2).
    [SerializeField] private CHButton _menuButton;

    //# 참가자 입력 컬럼 — 입력칸 + 추가/삭제 버튼 + 리스트 스크롤뷰 + 인원 카운트 라벨 (기획서 §5.2).
    [SerializeField] private TMP_InputField _playerInput;
    [SerializeField] private CHButton _playerPlusButton;
    [SerializeField] private CHButton _playerMinusButton;
    [SerializeField] private CustomScrollView _playerScrollView;
    [SerializeField] private CHText _playerCountText;

    //# 결과 영역 — 빈 안내 + 결과 스크롤뷰 (기획서 §4/§5.2).
    [SerializeField] private OrderCellPoolingScrollView _cardScrollView;
    [SerializeField] private CHText _emptyResultText;

    //# 하단 — 섞기 버튼 (기획서 §5.2).
    [SerializeField] private CHButton _shuffleButton;

    //# 표시 문자열 stringID (String.json §6.3).
    private const int StringIdShuffle = 2080;
    private const int StringIdEmptyResult = 2082;
    private const int StringIdPlayerCount = 2077;

    //# 입력 제약 (기획서 §2).
    private const int MaxCount = 12;
    private const int MinCount = 2;

    private ReactiveCollection<string> _liPlayer = new ReactiveCollection<string>();

    private IRandomSceneAccess _randomSceneAccess;

    private void Start()
    {
        _menuButton.OnClick(() =>
        {
            Close();
        });

        //# 리스트 변경 시 스크롤뷰 갱신 + UI 재계산 (기획서 §2).
        _liPlayer.ObserveCountChanged().Subscribe(_ =>
        {
            _playerScrollView.SetItemList(_liPlayer.ToList());
            RefreshUI();
        }).AddTo(this);

        _playerPlusButton.OnClick(() =>
        {
            AddItem(_liPlayer, _playerInput);
        });

        _playerMinusButton.OnClick(() =>
        {
            RemoveLast(_liPlayer);
        });

        _shuffleButton.OnClick(() =>
        {
            OnClickShuffle();
        });

        _shuffleButton.SetText(JsonManager.Instance.GetStringData(StringIdShuffle));
    }

    private void OnEnable()
    {
        //# 재진입 시 잔존 상태 초기화 (기획서 §5.2/§8).
        ResetState();
    }

    private void OnDisable()
    {
        //# 화면 이탈 시 결과 셀 클리어 — CHPoolingScrollView 내부 풀 관리 (기획서 §8).
        if (_cardScrollView != null)
        {
            _cardScrollView.Clear();
        }
    }

    //# OnEnable 은 Start 보다 먼저 실행될 수 있어 구독에 의존하지 않고 상태를 직접 세팅한다 (TeamSplitScene 선례).
    private void ResetState()
    {
        _cardScrollView?.Clear();

        _liPlayer.Clear();

        if (_playerInput != null)
        {
            _playerInput.text = string.Empty;
        }

        _shuffleButton.SetText(JsonManager.Instance.GetStringData(StringIdShuffle));

        if (_emptyResultText != null)
        {
            _emptyResultText.SetText(JsonManager.Instance.GetStringData(StringIdEmptyResult));
            _emptyResultText.gameObject.SetActive(true);
        }

        //# 리스트 Clear 가 구독을 못 깨우는 경우(이미 0개)에도 UI 가 동기화되도록 직접 1회 호출.
        RefreshUI();
    }

    //# 참가자 추가/삭제·리셋 시마다 호출되는 UI 동기화 (기획서 §2).
    //# 상한 토글 → 인원 카운트 → 섞기 버튼 활성 순.
    private void RefreshUI()
    {
        int n = _liPlayer.Count;

        //# 참가자 추가 버튼 — 상한 도달 시 비활성 (기획서 §2).
        _playerPlusButton.Interactable = n < MaxCount;

        //# 참가자 인원 카운트 "{0} / {1}" (기획서 §6.3 — stringID 2077 재사용).
        _playerCountText.SetText(string.Format(JsonManager.Instance.GetStringData(StringIdPlayerCount), n, MaxCount));

        //# 섞기 버튼 — MinCount 미만이면 비활성 (기획서 §8).
        _shuffleButton.Interactable = n >= MinCount;
    }

    //# 빈/공백 이름 무시, 중복 허용 (기획서 §2/§8). 상한 도달 시 추가 안 함 (버튼이 이미 비활성).
    private void AddItem(ReactiveCollection<string> list, TMP_InputField input)
    {
        if (input == null)
            return;

        if (list.Count >= MaxCount)
            return;

        string name = input.text;
        if (string.IsNullOrWhiteSpace(name))
            return;

        list.Add(name);
        input.text = string.Empty;
    }

    private void RemoveLast(ReactiveCollection<string> list)
    {
        if (list.Count == 0)
            return;

        list.RemoveAt(list.Count - 1);
    }

    //# 단일 액션 — 1회 클릭 = 셔플 + 순서 리스트 표시. 재클릭 = 재섞기 (기획서 §3).
    private void OnClickShuffle()
    {
        if (_shuffleButton.Interactable == false)
            return;

        int n = _liPlayer.Count;

        //# 검증 (기획서 §8) — N<2 면 중단. 버튼 비활성으로 이미 막혀 있어 방어적.
        if (n < MinCount)
            return;

        List<string> names = _liPlayer.ToList();

        //# 셔플 — 렌더와 분리한 순수 로직 (기획서 §3/§8). 호출부가 항상 new System.Random() 주입.
        List<string> ordered = Shuffle(names, new System.Random());

        RenderOrder(ordered);

        //# 첫 섞기 이후 빈 안내 숨김 (기획서 §4).
        if (_emptyResultText != null)
        {
            _emptyResultText.gameObject.SetActive(false);
        }
    }

    //# CHPoolingScrollView 에 순서 데이터 전달 — 풀 관리·배치 전부 위임 (Rule 03 §3).
    private void RenderOrder(List<string> ordered)
    {
        if (_cardScrollView == null)
            return;

        List<OrderCellData> dataList = new List<OrderCellData>(ordered.Count);
        for (int i = 0; i < ordered.Count; i++)
        {
            OrderCellData data = new OrderCellData
            {
                Rank = i + 1,
                Name = ordered[i],
            };
            dataList.Add(data);
        }

        _cardScrollView.SetItemList(dataList);
    }

    public void SetRandomSceneAccess(IRandomSceneAccess randomSceneAccess)
    {
        _randomSceneAccess = randomSceneAccess;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _randomSceneAccess.ShowScene(CommonEnum.ERouletteMenu.Menu);
    }

    //# === Fisher-Yates 셔플 순수 로직 (렌더와 분리 — test-engineer 진입점, 기획서 §8) ===

    //# N개 이름을 Fisher-Yates 셔플로 무작위 순열 생성. 원본 비파괴 — 복사본을 셔플.
    //# rng == null 이면 셔플 생략(원순서 반환) — 불변식(반환 길이 == names.Count) 보존.
    //# 계약: valid 입력만 보장 대상. 호출부가 항상 new System.Random() 보장.
    public static List<string> Shuffle(IList<string> names, System.Random rng)
    {
        if (names == null || names.Count == 0)
            return new List<string>();

        //# 원본 비파괴 — 복사본을 셔플.
        List<string> shuffled = new List<string>(names);

        if (rng != null)
        {
            //# Fisher-Yates — 뒤에서 앞으로 무작위 교환.
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                string tmp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = tmp;
            }
        }

        return shuffled;
    }
}

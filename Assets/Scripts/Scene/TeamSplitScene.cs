using ChvjUnityInfra;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;

//# 팀 나누기 — N명을 T팀으로 무작위·균등 분배하는 단일 액션 도구 (기획서 team_split).
//# LadderScene 클론·개조: 2단계 토글(_ladderDrawn)·선 렌더·DOTween 트레이스 제거, 결과를 팀 카드 풀링으로 대체.
public class TeamSplitScene : MonoBehaviour, IRouletteBackButton
{
    //# 상단 — 메뉴(뒤로) 버튼 (기획서 §5.2).
    [SerializeField] private CHButton _menuButton;

    //# 참가자 입력 컬럼 — 입력칸 + 추가/삭제 버튼 + 리스트 스크롤뷰 + 인원 카운트 라벨 (기획서 §5.2/§5.3).
    [SerializeField] private TMP_InputField _playerInput;
    [SerializeField] private CHButton _playerPlusButton;
    [SerializeField] private CHButton _playerMinusButton;
    [SerializeField] private CustomScrollView _playerScrollView;
    [SerializeField] private CHText _playerCountText;

    //# 팀 수 스테퍼 — − / 값 / + (기획서 §5.2).
    [SerializeField] private CHButton _teamMinusButton;
    [SerializeField] private CHText _teamValueText;
    [SerializeField] private CHButton _teamPlusButton;

    //# 하단 — 배분 미리보기/안내 라벨 + 나누기 버튼 (기획서 §3.2/§5.2).
    [SerializeField] private CHText _guideText;
    [SerializeField] private CHButton _drawButton;

    //# 중앙 결과 영역 — CHPoolingScrollView 2열 고정 높이 + 결과 전 빈 안내 (기획서 §4.4).
    [SerializeField] private TeamCardPoolingScrollView _cardScrollView;
    [SerializeField] private CHText _emptyResultText;

    //# 표시 문자열 stringID (String.json §6.3 — 2070~2079 신규 + 2065 재사용).
    private const int StringIdMinPlayers = 2065;
    private const int StringIdDraw = 2070;
    private const int StringIdPreviewEven = 2071;
    private const int StringIdPreviewUneven = 2072;
    private const int StringIdEmptyResult = 2075;
    private const int StringIdPlayerCount = 2077;

    //# 입력 제약 (기획서 §2/§8).
    private const int MaxCount = 12;
    private const int MinCount = 2;

    //# 팀 수 하한 (기획서 §2 — 상한은 참가자 수 N 로 동적 계산).
    private const int MinTeamCount = 2;

    //# 미리보기/안내 색 — 경고(N<2) / 보조 톤(N≥2) (기획서 §3.2, 목업 톤).
    private static readonly Color _warnColor = new Color(1f, 0.608f, 0.608f);     //# #ff9b9b
    private static readonly Color _normalColor = new Color(0.722f, 0.722f, 0.753f); //# #b8b8c0

    //# 팀 인덱스별 색 — index % 12 순환 (기획서 §4.3, 목업 COLORS 동일 톤).
    private static readonly Color[] _teamColors =
    {
        new Color(1f, 0.420f, 0.420f),     //# #ff6b6b
        new Color(1f, 0.839f, 0.302f),     //# #ffd64d
        new Color(0.353f, 0.820f, 0.769f), //# #5ad1c4
        new Color(0.478f, 0.635f, 1f),     //# #7aa2ff
        new Color(0.753f, 0.545f, 1f),     //# #c08bff
        new Color(1f, 0.624f, 0.353f),     //# #ff9f5a
        new Color(0.420f, 0.831f, 0.498f), //# #6bd47f
        new Color(1f, 0.482f, 0.741f),     //# #ff7bbd
        new Color(0.941f, 0.396f, 0.584f), //# #f06595
        new Color(0.455f, 0.753f, 0.988f), //# #74c0fc
        new Color(1f, 0.663f, 0.302f),     //# #ffa94d
        new Color(0.388f, 0.902f, 0.745f), //# #63e6be
    };

    private ReactiveCollection<string> _liPlayer = new ReactiveCollection<string>();

    private IRandomSceneAccess _randomSceneAccess;

    //# 현재 팀 수 T — 스테퍼 값. 참가자 수 변동 시 Clamp(2, Max(2,N)) 재보정 (기획서 §2).
    private int _teamCount = MinTeamCount;

    private void Start()
    {
        _menuButton.OnClick(() =>
        {
            Close();
        });

        //# 리스트 변경 시 스크롤뷰 갱신 + UI 전체 재계산 (상한 토글·스테퍼 재클램프·미리보기, 기획서 §2/§3.2).
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

        _teamMinusButton.OnClick(() =>
        {
            ChangeTeamCount(-1);
        });

        _teamPlusButton.OnClick(() =>
        {
            ChangeTeamCount(1);
        });

        _drawButton.OnClick(() =>
        {
            OnClickDraw();
        });

        _drawButton.SetText(JsonManager.Instance.GetStringData(StringIdDraw));
    }

    private void OnEnable()
    {
        //# 재진입 시 잔존 상태 초기화 (기획서 §5.2/§8).
        ResetState();
    }

    private void OnDisable()
    {
        //# 화면 이탈 시 결과 카드 클리어 — CHPoolingScrollView 내부 풀 관리.
        if (_cardScrollView != null)
        {
            _cardScrollView.Clear();
        }
    }

    //# OnEnable 은 Start 보다 먼저 실행될 수 있어 구독에 의존하지 않고 상태를 직접 세팅한다 (LadderScene 선례).
    private void ResetState()
    {
        _cardScrollView?.Clear();

        _liPlayer.Clear();

        if (_playerInput != null)
        {
            _playerInput.text = string.Empty;
        }

        _teamCount = MinTeamCount;

        _drawButton.SetText(JsonManager.Instance.GetStringData(StringIdDraw));

        if (_emptyResultText != null)
        {
            _emptyResultText.SetText(JsonManager.Instance.GetStringData(StringIdEmptyResult));
            _emptyResultText.gameObject.SetActive(true);
        }

        //# 리스트 Clear 가 구독을 못 깨우는 경우(이미 0개)에도 UI 가 동기화되도록 직접 1회 호출.
        RefreshUI();
    }

    //# 참가자 추가/삭제·팀수 변경·리셋 시마다 호출되는 UI 동기화 (기획서 §2/§3.2).
    //# 상한 토글 → 스테퍼 클램프 → 스테퍼 버튼/값 → 인원 카운트 → 미리보기/나누기 활성 순.
    private void RefreshUI()
    {
        int n = _liPlayer.Count;

        //# 참가자 추가 버튼 — 상한 도달 시 비활성 (기획서 §2).
        _playerPlusButton.Interactable = n < MaxCount;

        //# 팀 수 상한 재계산 + 현재 T 클램프 → 빈 팀 방지 (기획서 §2/§8).
        int maxTeam = Mathf.Max(MinTeamCount, n);
        _teamCount = Mathf.Clamp(_teamCount, MinTeamCount, maxTeam);

        _teamMinusButton.Interactable = _teamCount > MinTeamCount;
        _teamPlusButton.Interactable = _teamCount < maxTeam;
        _teamValueText.SetText(_teamCount.ToString());

        //# 참가자 인원 카운트 "{0} / {1}" (기획서 §5.3).
        _playerCountText.SetText(string.Format(JsonManager.Instance.GetStringData(StringIdPlayerCount), n, MaxCount));

        RefreshGuide(n);
    }

    //# 하단 안내/미리보기 갱신 + 나누기 버튼 활성 토글 (기획서 §3.2).
    private void RefreshGuide(int n)
    {
        if (n < MinCount)
        {
            _guideText.SetText(JsonManager.Instance.GetStringData(StringIdMinPlayers));
            _guideText.SetColor(_warnColor);
            _drawButton.Interactable = false;
            return;
        }

        _drawButton.Interactable = true;
        _guideText.SetColor(_normalColor);

        int t = _teamCount;
        int baseSize = n / t;
        int rem = n % t;

        if (rem == 0)
        {
            //# 2071: "{0}명 → {1}팀 × {2}명" — N, T, baseSize.
            _guideText.SetText(string.Format(JsonManager.Instance.GetStringData(StringIdPreviewEven), n, t, baseSize));
            return;
        }

        //# 2072: "{0}명 → {1}팀 {2}명 · {3}팀 {4}명" — N, 큰 팀 수(rem), baseSize+1, 작은 팀 수(T-rem), baseSize.
        _guideText.SetText(string.Format(JsonManager.Instance.GetStringData(StringIdPreviewUneven), n, rem, baseSize + 1, t - rem, baseSize));
    }

    //# 스테퍼 증감 — 상/하한 클램프 후 UI 재계산 (기획서 §2). 버튼 Interactable 로 이미 막혀 있어 방어적.
    private void ChangeTeamCount(int delta)
    {
        int n = _liPlayer.Count;
        int maxTeam = Mathf.Max(MinTeamCount, n);
        _teamCount = Mathf.Clamp(_teamCount + delta, MinTeamCount, maxTeam);
        RefreshUI();
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

    //# 단일 액션 — 1회 클릭 = 셔플 + 균등 배분 + 팀 카드 표시. 재클릭 = 재추첨 (기획서 §3.0/§3.1).
    private void OnClickDraw()
    {
        if (_drawButton.Interactable == false)
            return;

        int n = _liPlayer.Count;

        //# 검증 (기획서 §8) — N<2 면 안내 후 중단. 버튼 비활성으로 이미 막혀 있어 방어적.
        if (n < MinCount)
        {
            _guideText.SetText(JsonManager.Instance.GetStringData(StringIdMinPlayers));
            _guideText.SetColor(_warnColor);
            return;
        }

        //# 입력 단계에서 이미 클램프되어 있으나 방어적으로 1회 더 (기획서 §3.1).
        int t = Mathf.Clamp(_teamCount, MinCount, n);

        List<string> names = _liPlayer.ToList();

        //# 균등 배분 — 렌더와 분리한 순수 로직 (기획서 §10). 호출부가 항상 new System.Random() 주입.
        List<List<string>> teams = Split(names, t, new System.Random());

        RenderTeams(teams);

        //# 첫 나누기 이후 빈 안내 숨김 (기획서 §4.4).
        if (_emptyResultText != null)
        {
            _emptyResultText.gameObject.SetActive(false);
        }
    }

    //# === 결과 렌더 (CHPoolingScrollView) ===

    //# CHPoolingScrollView 에 팀 데이터 전달 — 풀 관리·배치 전부 위임 (Rule 03 §3).
    private void RenderTeams(List<List<string>> teams)
    {
        if (_cardScrollView == null)
            return;

        List<TeamSplitTeamData> dataList = new List<TeamSplitTeamData>(teams.Count);
        for (int t = 0; t < teams.Count; t++)
        {
            TeamSplitTeamData data = new TeamSplitTeamData
            {
                TeamIndex = t,
                Members = teams[t],
                Color = _teamColors[t % _teamColors.Length]
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

    //# === 균등 배분 순수 로직 (렌더와 분리 — test-engineer 진입점, 기획서 §10) ===

    //# N명을 T팀으로 Fisher-Yates 셔플 후 균등 배분. 나머지 N%T 명은 앞 팀부터 1명씩.
    //# 계약: valid 입력(2 ≤ teamCount ≤ N)만 보장 대상. teamCount > N · 음수 등 계약 위반은 동작 미정의
    //# (호출부가 항상 Clamp(2, Max(2,N)) + new System.Random() 보장). 방어 클램프 미삽입 — 불변식(반환 팀수==teamCount) 유지.
    //# rng == null 이면 셔플 생략(원순서 배분) — 반환 팀수는 여전히 teamCount 라 불변식 보존.
    public static List<List<string>> Split(IList<string> names, int teamCount, System.Random rng)
    {
        List<List<string>> teams = new List<List<string>>();
        for (int t = 0; t < teamCount; t++)
        {
            teams.Add(new List<string>());
        }

        if (names == null || names.Count == 0)
            return teams;

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

        int n = shuffled.Count;
        int baseSize = n / teamCount;
        int rem = n % teamCount;

        //# 셔플된 순서대로 앞에서부터 각 팀 size 만큼 잘라 담는다. 나머지는 앞 팀부터 +1.
        int cursor = 0;
        for (int t = 0; t < teamCount; t++)
        {
            int size = baseSize + (t < rem ? 1 : 0);
            for (int k = 0; k < size; k++)
            {
                teams[t].Add(shuffled[cursor]);
                cursor++;
            }
        }

        return teams;
    }
}

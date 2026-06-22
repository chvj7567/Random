using ChvjUnityInfra;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;

public class LadderScene : MonoBehaviour, IRouletteBackButton
{
    //# 입력 — 참가자/결과 각각 입력칸 + 추가/삭제 버튼 + 리스트 스크롤뷰 (기획서 §5.2).
    [SerializeField] private CHButton _menuButton;
    [SerializeField] private TMP_InputField _playerInput;
    [SerializeField] private CHButton _playerPlusButton;
    [SerializeField] private CHButton _playerMinusButton;
    [SerializeField] private CustomScrollView _playerScrollView;
    [SerializeField] private TMP_InputField _resultInput;
    [SerializeField] private CHButton _resultPlusButton;
    [SerializeField] private CHButton _resultMinusButton;
    [SerializeField] private CustomScrollView _resultScrollView;

    //# 그리기 버튼 + 안내/매핑 라벨.
    [SerializeField] private CHButton _drawButton;
    [SerializeField] private CHText _guideText;
    [SerializeField] private CHText _mappingText;

    //# 사다리 렌더 — 선 origin 프리팹 2종 + 컨테이너 (CHMPool 풀링 prototype, 기획서 §6.1/§7).
    [SerializeField] private RectTransform _ladderArea;
    [SerializeField] private GameObject _verticalOrigin;
    [SerializeField] private GameObject _rungOrigin;

    //# 컬럼 상/하단 텍스트 라벨 origin — TMP_Text + CHText prototype (CHMPool 풀링, Rule 03 §3).
    [SerializeField] private GameObject _columnLabelOrigin;

    //# 표시 문자열 stringID (String.json §6.3).
    private const int StringIdDraw = 2063;
    private const int StringIdCountMismatch = 2064;
    private const int StringIdMinPlayers = 2065;
    private const int StringIdMapping = 2066;
    private const int StringIdViewResult = 2067;

    //# 입력 제약 (기획서 §2/§8).
    private const int MaxCount = 8;
    private const int MinCount = 2;

    //# 가로줄 밀도 — 구간당 인접쌍 시도 확률 (기획서 §3.1).
    private const float RungProbability = 0.5f;

    //# 세로/가로 선 공통 두께 (px) — sizeDelta 의 고정 축.
    private const float LineThickness = 12f;

    //# 컬럼 라벨이 사다리 위/아래 끝에서 떨어지는 여백 (px).
    private const float LabelMargin = 28f;

    //# 풀링 워밍 count (기획서 §6.1).
    private const int VerticalWarmCount = 8;
    private const int RungWarmCount = 24;

    //# 경로 추적 연출 파라미터.
    private const float TraceDurationPerStep = 0.18f;

    //# 참가자별 경로 색 — 순환 사용.
    private static readonly Color[] _pathColors =
    {
        new Color(0.95f, 0.40f, 0.40f),
        new Color(0.40f, 0.70f, 0.95f),
        new Color(0.50f, 0.90f, 0.55f),
        new Color(0.95f, 0.80f, 0.35f),
        new Color(0.80f, 0.55f, 0.95f),
        new Color(0.45f, 0.90f, 0.90f),
        new Color(0.95f, 0.60f, 0.40f),
        new Color(0.70f, 0.75f, 0.80f),
    };

    private ReactiveCollection<string> _liPlayer = new ReactiveCollection<string>();
    private ReactiveCollection<string> _liResult = new ReactiveCollection<string>();

    private IRandomSceneAccess _randomSceneAccess;

    //# 풀에서 꺼낸 사다리 선 인스턴스 — OnDisable/재그리기 시 모두 Push 반환.
    private List<CHPoolable> _spawnedLines = new List<CHPoolable>();
    private Sequence _traceSequence;
    private bool _warmed;

    //# 2단계 토글 — 1단계(사다리만 그림) 완료 여부. true 면 다음 클릭은 결과 트레이스.
    private bool _ladderDrawn;

    //# 1단계 산출 결과 — 2단계 트레이스에서 재사용.
    private bool[,] _currentRungs;
    private List<string> _currentPlayers;
    private List<string> _currentResults;
    private int _currentN;

    private void Start()
    {
        _menuButton.OnClick(() =>
        {
            Close();
        });

        //# 리스트 변경 시 스크롤뷰 갱신 + 추가 버튼 상한 토글 (기획서 §2 R3).
        _liPlayer.ObserveCountChanged().Subscribe(_ =>
        {
            _playerScrollView.SetItemList(_liPlayer.ToList());
            _playerPlusButton.Interactable = _liPlayer.Count < MaxCount;
        }).AddTo(this);

        _liResult.ObserveCountChanged().Subscribe(_ =>
        {
            _resultScrollView.SetItemList(_liResult.ToList());
            _resultPlusButton.Interactable = _liResult.Count < MaxCount;
        }).AddTo(this);

        _playerPlusButton.OnClick(() =>
        {
            AddItem(_liPlayer, _playerInput);
        });

        _playerMinusButton.OnClick(() =>
        {
            RemoveLast(_liPlayer);
        });

        _resultPlusButton.OnClick(() =>
        {
            AddItem(_liResult, _resultInput);
        });

        _resultMinusButton.OnClick(() =>
        {
            RemoveLast(_liResult);
        });

        _drawButton.OnClick(() =>
        {
            OnClickDraw();
        });

        _drawButton.SetText(JsonManager.Instance.GetStringData(StringIdDraw));
    }

    private void OnEnable()
    {
        //# 진입 시 풀 워밍 1회 (CreatePool 중복 호출 경고 회피).
        WarmPools();

        //# 재진입 시 잔존 상태 초기화 (기획서 §5.2).
        ResetState();
    }

    private void OnDisable()
    {
        //# 진행 중 이탈 시 Tween kill + 풀 반환 (기획서 §8).
        KillTrace();
        ReleaseLines();
    }

    private void WarmPools()
    {
        if (_warmed == true)
            return;

        _warmed = true;
        CHMPool.Instance.Init();

        if (_verticalOrigin != null)
        {
            CHMPool.Instance.CreatePool(_verticalOrigin, VerticalWarmCount);
        }

        if (_rungOrigin != null)
        {
            CHMPool.Instance.CreatePool(_rungOrigin, RungWarmCount);
        }
    }

    private void ResetState()
    {
        KillTrace();
        ReleaseLines();

        _liPlayer.Clear();
        _liResult.Clear();

        if (_playerInput != null)
        {
            _playerInput.text = string.Empty;
        }

        if (_resultInput != null)
        {
            _resultInput.text = string.Empty;
        }

        _playerPlusButton.Interactable = true;
        _resultPlusButton.Interactable = true;
        _drawButton.Interactable = true;

        //# 재진입 시 항상 1단계("그리기")로 초기화.
        _ladderDrawn = false;
        _currentRungs = null;
        _currentPlayers = null;
        _currentResults = null;
        _currentN = 0;
        _drawButton.SetText(JsonManager.Instance.GetStringData(StringIdDraw));

        _guideText.SetText(string.Empty);
        _mappingText.SetText(string.Empty);
    }

    //# 빈/공백 이름 무시, 중복 허용 (기획서 §2 R2). 상한 도달 시 추가 안 함 (버튼이 이미 비활성).
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

    private void OnClickDraw()
    {
        //# 연출 진행 중 재탭 무시 (입력 잠금).
        if (_drawButton.Interactable == false)
            return;

        //# 2단계 — 1단계에서 그린 사다리에 색 경로 트레이스 + 매핑표.
        if (_ladderDrawn == true)
        {
            _drawButton.Interactable = false;
            PlayTrace(_currentN, _currentRungs, _currentPlayers, _currentResults);
            return;
        }

        //# === 1단계 — 검증 후 사다리/라벨만 그림 (트레이스 X) ===
        int playerCount = _liPlayer.Count;
        int resultCount = _liResult.Count;

        //# 검증 (기획서 §8) — 클릭 시점 안내.
        if (playerCount < MinCount)
        {
            _guideText.SetText(JsonManager.Instance.GetStringData(StringIdMinPlayers));
            return;
        }

        if (playerCount != resultCount)
        {
            _guideText.SetText(JsonManager.Instance.GetStringData(StringIdCountMismatch));
            return;
        }

        _guideText.SetText(string.Empty);
        _mappingText.SetText(string.Empty);

        List<string> players = _liPlayer.ToList();
        List<string> results = _liResult.ToList();

        //# 순열 산출 — 렌더와 분리한 순수 로직 (기획서 §3·§8).
        int n = players.Count;
        bool[,] rungs = BuildRungs(n, new System.Random());

        //# 2단계 트레이스에서 재사용하도록 산출 결과 저장.
        _currentN = n;
        _currentRungs = rungs;
        _currentPlayers = players;
        _currentResults = results;

        RenderLadder(n, rungs);
        RenderColumnLabels(n, players, results);

        //# 1단계 완료 — 버튼을 "결과 보기"로 전환.
        _ladderDrawn = true;
        _drawButton.SetText(JsonManager.Instance.GetStringData(StringIdViewResult));
    }

    //# === 렌더 (CHMPool 풀링) ===

    private void RenderLadder(int n, bool[,] rungs)
    {
        ReleaseLines();

        if (_ladderArea == null || _verticalOrigin == null || _rungOrigin == null)
            return;

        int levelCount = LevelCount(n);
        float width = _ladderArea.rect.width;
        float height = _ladderArea.rect.height;

        //# 세로줄 N개 — 좌→우 균등 배치.
        for (int c = 0; c < n; c++)
        {
            float x = ColumnX(c, n, width);
            CHPoolable line = CHMPool.Instance.Pop(_verticalOrigin, _ladderArea);
            if (line == null)
                continue;

            RectTransform rt = line.transform as RectTransform;
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(x, 0f);
                rt.sizeDelta = new Vector2(LineThickness, height);
            }

            _spawnedLines.Add(line);
        }

        //# 가로줄 — rungs[level, c] 가 true 인 곳에 c↔c+1 단 배치.
        for (int h = 0; h < levelCount; h++)
        {
            float y = LevelY(h, levelCount, height);

            for (int c = 0; c < n - 1; c++)
            {
                if (rungs[h, c] == false)
                    continue;

                float xLeft = ColumnX(c, n, width);
                float xRight = ColumnX(c + 1, n, width);
                CHPoolable rung = CHMPool.Instance.Pop(_rungOrigin, _ladderArea);
                if (rung == null)
                    continue;

                RectTransform rt = rung.transform as RectTransform;
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2((xLeft + xRight) * 0.5f, y);
                    rt.sizeDelta = new Vector2(Mathf.Abs(xRight - xLeft), LineThickness);
                }

                _spawnedLines.Add(rung);
            }
        }
    }

    //# 각 컬럼 상단 = 참가자(players[c]), 하단 = 결과(results[c]) 라벨 배치.
    //# 라벨도 _spawnedLines 에 등록 → ReleaseLines(Push) 로 재그리기/이탈 시 함께 반환.
    private void RenderColumnLabels(int n, List<string> players, List<string> results)
    {
        if (_ladderArea == null || _columnLabelOrigin == null)
            return;

        float width = _ladderArea.rect.width;
        float height = _ladderArea.rect.height;
        float topY = height * 0.5f + LabelMargin;     //# 박스 위쪽 바깥
        float bottomY = -height * 0.5f - LabelMargin;  //# 박스 아래쪽 바깥

        for (int c = 0; c < n; c++)
        {
            float x = ColumnX(c, n, width);

            if (c < players.Count)
            {
                SpawnColumnLabel(new Vector2(x, topY), players[c]);
            }

            if (c < results.Count)
            {
                SpawnColumnLabel(new Vector2(x, bottomY), results[c]);
            }
        }
    }

    //# 라벨 1개 Pop + 위치/텍스트 적용 + _spawnedLines 등록.
    private void SpawnColumnLabel(Vector2 anchoredPos, string text)
    {
        CHPoolable label = CHMPool.Instance.Pop(_columnLabelOrigin, _ladderArea);
        if (label == null)
            return;

        RectTransform rt = label.transform as RectTransform;
        if (rt != null)
        {
            rt.anchoredPosition = anchoredPos;
        }

        //# Pop 시점 1회 캐싱 (Rule 02 §5 허용) — 텍스트는 CHText 래퍼로만 조작 (Rule 03 §3).
        CHText chText = label.GetComponent<CHText>();
        if (chText != null)
        {
            chText.SetText(text);
        }

        _spawnedLines.Add(label);
    }

    private void ReleaseLines()
    {
        for (int i = 0; i < _spawnedLines.Count; i++)
        {
            CHPoolable line = _spawnedLines[i];
            if (line == null)
                continue;

            CHMPool.Instance.Push(line);
        }

        _spawnedLines.Clear();
    }

    //# === 경로 추적 연출 (DOTween) ===

    private void PlayTrace(int n, bool[,] rungs, List<string> players, List<string> results)
    {
        KillTrace();

        _traceSequence = DOTween.Sequence();

        float width = _ladderArea != null ? _ladderArea.rect.width : 0f;
        float height = _ladderArea != null ? _ladderArea.rect.height : 0f;
        int levelCount = LevelCount(n);

        //# 참가자별 순차 추적 — 각 경로를 참가자 색으로 강조한 라인 복제본을 순차 노출 (기획서 §3.4).
        List<string> mappingRows = new List<string>();
        for (int start = 0; start < n; start++)
        {
            List<(int level, int column)> path = BuildPath(start, rungs, n);
            int end = path[path.Count - 1].column;
            mappingRows.Add(string.Format(JsonManager.Instance.GetStringData(StringIdMapping), players[start], results[end]));

            Color color = _pathColors[start % _pathColors.Length];

            //# 진입 stub — 세로줄 최상단(+height/2)에서 첫 노드까지 시작 컬럼 색칠 (세그먼트보다 먼저 페이드).
            float xStart = ColumnX(start, n, width);
            float yTopEdge = height * 0.5f;
            float yTopNode = LevelY(0, levelCount, height);
            AppendHighlightLine(_verticalOrigin, new Vector2(xStart, (yTopEdge + yTopNode) * 0.5f),
                new Vector2(LineThickness, Mathf.Abs(yTopEdge - yTopNode)), color);

            //# 경로 노드 간 세그먼트를 한 칸씩 순차로 켜 추적감을 준다.
            for (int i = 1; i < path.Count; i++)
            {
                int prevColumn = path[i - 1].column;
                int currColumn = path[i].column;
                int fromLevel = path[i - 1].level;
                int toLevel = path[i].level;

                AppendSegment(prevColumn, currColumn, fromLevel, toLevel, n, width, height, levelCount, color);
            }

            //# 출구 stub — 마지막 노드에서 세로줄 최하단(-height/2)까지 도착 컬럼 색칠 (마지막에 페이드).
            float xEnd = ColumnX(end, n, width);
            float yBotNode = LevelY(levelCount, levelCount, height);
            float yBotEdge = -height * 0.5f;
            AppendHighlightLine(_verticalOrigin, new Vector2(xEnd, (yBotNode + yBotEdge) * 0.5f),
                new Vector2(LineThickness, Mathf.Abs(yBotNode - yBotEdge)), color);
        }

        _traceSequence.OnComplete(() =>
        {
            _mappingText.SetText(FormatMappingRows(mappingRows));
            _drawButton.Interactable = true;

            //# 트레이스 종료 — 다시 1단계("그리기")로 전환해 새 사다리 반복 가능.
            _ladderDrawn = false;
            _drawButton.SetText(JsonManager.Instance.GetStringData(StringIdDraw));
        });
    }

    //# 4명 이하는 세로 1줄씩, 5명 이상은 한 줄에 2개씩 탭 구분으로 세로 길이 절반.
    private static string FormatMappingRows(List<string> rows)
    {
        if (rows.Count <= 4)
            return string.Join("\n", rows);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < rows.Count; i += 2)
        {
            if (i > 0)
                sb.Append('\n');

            sb.Append(rows[i]);
            if (i + 1 < rows.Count)
            {
                sb.Append('\t');
                sb.Append(rows[i + 1]);
            }
        }
        return sb.ToString();
    }

    //# 경로 한 구간을 참가자 색 라인 복제본으로 노출 — 세로 이동(컬럼 동일) + 가로 이동(컬럼 변경) 분리.
    private void AppendSegment(int prevColumn, int currColumn, int fromLevel, int toLevel, int n, float width, float height, int levelCount, Color color)
    {
        if (_ladderArea == null || _verticalOrigin == null || _rungOrigin == null)
            return;

        //# 세로 구간 — 같은 컬럼에서 한 레벨 아래로 내려가는 추적 강조.
        float yFrom = LevelY(fromLevel, levelCount, height);
        float yTo = LevelY(toLevel, levelCount, height);
        float xCurr = ColumnX(currColumn, n, width);

        //# 세로 강조는 가로줄을 건넌 뒤 내려가는 컬럼(xCurr) 기준.
        AppendHighlightLine(_verticalOrigin, new Vector2(xCurr, (yFrom + yTo) * 0.5f),
            new Vector2(LineThickness, Mathf.Abs(yFrom - yTo)), color);

        //# 가로 이동 시: 가로줄은 기본 가로줄과 같은 레벨(yFrom = LevelY(fromLevel))에 그려 정렬.
        if (prevColumn != currColumn)
        {
            float xPrev = ColumnX(prevColumn, n, width);
            AppendHighlightLine(_rungOrigin, new Vector2((xPrev + xCurr) * 0.5f, yFrom),
                new Vector2(Mathf.Abs(xCurr - xPrev), LineThickness), color);
        }
    }

    //# 강조 라인 1개를 Pop + 색 적용 + 시퀀스에 순차 페이드인 — _spawnedLines 에 등록해 리셋 시 반환.
    private void AppendHighlightLine(GameObject origin, Vector2 anchoredPos, Vector2 size, Color color)
    {
        CHPoolable line = CHMPool.Instance.Pop(origin, _ladderArea);
        if (line == null)
            return;

        RectTransform rt = line.transform as RectTransform;
        if (rt != null)
        {
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        //# Pop 시점 1회 캐싱 (Rule 02 §5 허용 범위) — 색/alpha 는 LadderLine API 로만 조작.
        LadderLine ladderLine = line.GetComponent<LadderLine>();
        if (ladderLine != null)
        {
            //# 동기적으로 색 적용 + alpha 0 — OnEnable 원본 색이 한 프레임 깜빡이지 않도록 같은 프레임에 덮어쓴다.
            ladderLine.SetColor(color);
            ladderLine.SetAlpha(0f);

            Tweener fade = ladderLine.FadeIn(TraceDurationPerStep);
            if (fade != null)
            {
                _traceSequence.Append(fade);
            }
        }

        _spawnedLines.Add(line);
    }

    private void KillTrace()
    {
        if (_traceSequence == null)
            return;

        _traceSequence.Kill();
        _traceSequence = null;
    }

    //# === 좌표 헬퍼 (anchoredPosition, _ladderArea 중심 기준) ===

    private static float ColumnX(int column, int n, float width)
    {
        if (n <= 1)
            return 0f;

        //# 좌우 끝에 약간의 여백을 두고 균등 분할.
        float usable = width * 0.9f;
        float step = usable / (n - 1);
        return -usable * 0.5f + step * column;
    }

    private static float LevelY(int level, int levelCount, float height)
    {
        if (levelCount <= 1)
            return 0f;

        //# 위(+)에서 아래(-)로. 끝 구간 여백 포함.
        float usable = height * 0.9f;
        float step = usable / (levelCount + 1);
        return usable * 0.5f - step * (level + 1);
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

    //# === 순열 산출 (연출과 분리한 순수 로직 — test-engineer 진입점) ===

    //# 높이 구간 수 R = N * 3 (기획서 §3.1).
    public static int LevelCount(int n)
    {
        return n * 3;
    }

    //# 무작위 가로줄 생성. rungs[level, column] = column↔column+1 사이 가로줄 존재 여부.
    //# 한 구간 좌→우 훑으며 배치 직후 다음 쌍 건너뛰어 인접 충돌 구조적 차단 (기획서 §3.1·§8).
    public static bool[,] BuildRungs(int n, System.Random rng)
    {
        int levelCount = LevelCount(n);
        bool[,] rungs = new bool[levelCount, Mathf.Max(1, n - 1)];

        if (n < 2 || rng == null)
            return rungs;

        for (int h = 0; h < levelCount; h++)
        {
            int c = 0;
            while (c < n - 1)
            {
                if (rng.NextDouble() < RungProbability)
                {
                    rungs[h, c] = true;
                    c += 2;
                }
                else
                {
                    c += 1;
                }
            }
        }

        return rungs;
    }

    //# 시작 컬럼에서 출발해 각 구간을 통과하는 노드 경로 (level, column) 시퀀스.
    //# level=0 은 시작 컬럼, 이후 각 구간 통과 후 컬럼. BuildRungs skip 규칙이 좌·우 동시 금지를 보장.
    public static List<(int level, int column)> BuildPath(int start, bool[,] rungs, int n)
    {
        List<(int level, int column)> path = new List<(int level, int column)>();

        int current = start;
        path.Add((0, current));

        if (n < 2)
            return path;

        int levelCount = rungs.GetLength(0);

        for (int h = 0; h < levelCount; h++)
        {
            //# 오른쪽 가로줄 (current↔current+1)
            if (current < n - 1 && rungs[h, current])
            {
                current += 1;
            }
            //# 왼쪽 가로줄 (current-1↔current)
            else if (current > 0 && rungs[h, current - 1])
            {
                current -= 1;
            }

            path.Add((h + 1, current));
        }

        return path;
    }

    //# 시작 컬럼이 도착하는 결과 컬럼 — BuildPath 마지막 노드의 컬럼.
    public static int Trace(int start, bool[,] rungs, int n)
    {
        List<(int level, int column)> path = BuildPath(start, rungs, n);
        return path[path.Count - 1].column;
    }
}

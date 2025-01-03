using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using DG.Tweening;

public enum PoolingScrollViewDirection
{
    Vertical,
    Horizontal,
}

public enum PoolingScrollViewAlign
{
    LeftOrTop,
    Center,
    RightOrBottom,
}

public enum PoolingScrollViewScrollingDirection
{
    UpOrLeft,
    DownOrRight,
}

public class PoolingScrollViewItem<TItem>
{
    public int index;
    public TItem item;
}

[RequireComponent(typeof(ScrollRect))]
public abstract class CHScrollView<TItem, TData> : MonoBehaviour where TItem : MonoBehaviour
{
    protected LinkedList<PoolingScrollViewItem<TItem>> _liPoolItem = new LinkedList<PoolingScrollViewItem<TItem>>();

    public List<TItem> ItemList
    {
        get
        {
            return _liPoolItem.Select(_ => _.item).ToList();
        }
    }

    protected List<TData> _liData = new List<TData>();

    public List<TData> DataList
    {
        get
        {
            return _liData.ToList();
        }
    }

    [SerializeField, Header("복제할 아이템 오브젝트")]
    private GameObject _origin;

    [SerializeField, Header("아이템 크기(Zero이면 Origin 사이즈로 적용")]
    private Vector2 _itemSize = Vector2.zero;

    [SerializeField, Header("아이템 사이 간격")]
    private Vector2 _itemGap = Vector2.zero;

    [SerializeField, Header("패딩 설정")]
    private RectOffset _padding = new RectOffset();

    [SerializeField, Header("스크롤 방향 설정")]
    private PoolingScrollViewDirection _scrollDirection = PoolingScrollViewDirection.Vertical;

    [SerializeField, Header("행 갯수, 0이하이면 자동 계산")]
    private int _rowCount = 0;

    [SerializeField, Header("열 갯수, 0이하이면 자동 계산")]
    private int _columnCount = 0;

    [SerializeField, Header("오브젝트풀 개수 설정, 0 이하이면 자동 할당")]
    private int _poolItemCount = 0;

    [SerializeField, Header("아이템 정렬 기준")]
    private PoolingScrollViewAlign _align = PoolingScrollViewAlign.Center;

    [SerializeField, Header("스크롤뷰 사이즈 변경시 새로고침 여부")]
    private bool _refresh = false;

    [Space]

    private Vector2 _prevScrollPosition = Vector2.zero;
    private GameObject _contentObject;
    private RectTransform _contentRectTransform;
    private RectTransform _viewPortRectTransform;
    private CanvasGroup _canvasGroupContent;
    private ScrollRect _scrollRect;

    private int _calcRowCount = 0;
    private int _calcColumnCount = 0;

    public int LineCount
    {
        get
        {
            int line = 0;

            if (_liData.Count == 0)
                return line;

            int count = 1;

            switch (_scrollDirection)
            {
                case PoolingScrollViewDirection.Vertical:
                    {
                        count = _calcColumnCount;
                    }
                    break;
                case PoolingScrollViewDirection.Horizontal:
                    {
                        count = _calcRowCount;
                    }
                    break;
            }

            line = _liData.Count / count;
            if (_liData.Count % count != 0)
            {
                line += 1;
            }

            return line;
        }
    }

    public float ItemsWidth
    {
        get
        {
            return _calcColumnCount * _itemSize.x + (_calcColumnCount - 1) * _itemGap.x;
        }
    }

    public float ItemsHeight
    {
        get
        {
            return _calcRowCount * _itemSize.y + (_calcRowCount - 1) * _itemGap.y;
        }
    }

    public float ContentWidth
    {
        get
        {
            return _contentRectTransform.rect.width;
        }
    }

    public float ContentHeight
    {
        get
        {
            return _contentRectTransform.rect.height;
        }
    }

    public float ScrollViewWidth
    {
        get
        {
            int line = LineCount;
            float width = line * _itemSize.x;
            width += (line - 1) * _itemGap.x;
            width += _padding.left + _padding.right;
            return width;
        }
    }

    public float ScrollViewHeight
    {
        get
        {
            int line = LineCount;
            float height = line * _itemSize.y;
            height += (line - 1) * _itemGap.y;
            height += _padding.top + _padding.bottom;
            return height;
        }
    }

    private void Awake()
    {
        if (_scrollRect == null)
            _scrollRect = GetComponent<ScrollRect>();

        if (_viewPortRectTransform == null)
            _viewPortRectTransform = _scrollRect.viewport;

        if (_contentObject == null)
            _contentObject = _scrollRect.content.gameObject;

        if (_contentRectTransform == null)
            _contentRectTransform = _scrollRect.content.GetComponent<RectTransform>();

        if (_canvasGroupContent = null)
            _canvasGroupContent = _scrollRect.content.GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        _origin.SetActive(false);

        var scrollRect = GetComponent<ScrollRect>();
        scrollRect.OnValueChangedAsObservable().Subscribe(OnScroll);
        scrollRect.OnRectTransformDimensionsChangeAsObservable().Subscribe(_ =>
        {
            if (_refresh)
            {
                Refresh();
            }
        });
    }

    /// <summary>
    /// 아이템 초기화 스크롤하여 인덱스가 바뀔때마다 호출
    /// </summary>
    /// <param name="item"></param>
    /// <param name="data"></param>
    /// <param name="index"></param>
    public abstract void InitItem(TItem item, TData data, int index);

    /// <summary>
    /// 풀링 오브젝트 생성 시 호출(최초 1회)
    /// </summary>
    /// <param name="item"></param>
    public abstract void InitPoolingObject(TItem item);

    /// <summary>
    /// 스크롤 될 데이터 세팅
    /// </summary>
    /// <param name="dataList"></param>
    public void SetItemList(List<TData> dataList)
    {
        _liData.Clear();
        _liData.AddRange(dataList);

        switch (_scrollDirection)
        {
            case PoolingScrollViewDirection.Vertical:
                {
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                }
                break;
            case PoolingScrollViewDirection.Horizontal:
                {
                    _scrollRect.horizontal = true;
                    _scrollRect.vertical = false;
                }
                break;
        }

        //# 아이템 크기 설정
        SetItemSize();

        //# 행 갯수 설정
        SetRowCount();

        //# 열 갯수 설정
        SetColumnCount();

        //# 아이템 풀링 갯수 설정
        SetPoolItemCount();

        //# Content 오브젝트 설정
        SetContentTransform();

        //# 풀링 오브젝트 생성
        CreatePoolingObject();

        //# 아이템 초기화
        InitItem();
    }

    /// <summary>
    /// 풀링 미 사용 시에만 duration = 0 가능
    /// </summary>
    /// <param name="index"></param>
    /// <param name="duration"></param>
    /// <param name="completeCallback"></param>
    public void SetScrollPosition(int index, float duration = 1f, Action completeCallback = null)
    {
        var pos = GetItemPosition(index);

        float x = _contentRectTransform.anchoredPosition.x;
        float y = _contentRectTransform.anchoredPosition.y;

        //# 최대 스크롤 위치 값 계산
        switch (_scrollDirection)
        {
            case PoolingScrollViewDirection.Vertical:
                {
                    float tempY = Mathf.Min(ContentHeight - _viewPortRectTransform.rect.height, -pos.y);
                    if (tempY < 0)
                        tempY = 0;
                    y = tempY;
                }
                break;
            case PoolingScrollViewDirection.Horizontal:
                {
                    float tempX = Mathf.Min(ContentHeight - _viewPortRectTransform.rect.width, -pos.x);
                    if (tempX < 0)
                        tempX = 0;
                    x = tempX;
                }
                break;
        }

        //# 즉시 이동 시 풀링이 제대로 되지 않음
        //_contentRectTransform.anchoredPosition = new Vector2(x, y);
        _contentRectTransform.DOAnchorPos(new Vector2(x, y), duration).OnComplete(() => completeCallback?.Invoke()).SetEase(Ease.Linear);
    }

    public void Refresh()
    {
        SetItemList(_liData.ToList());
    }

    public void Clear()
    {
        _liData.Clear();
        _liPoolItem.Clear();
        if (_contentObject)
        {
            int childCount = _contentObject.transform.childCount;
            GameObject[] children = new GameObject[childCount];

            for (int i = 0; i < childCount; i++)
            {
                children[i] = _contentObject.transform.GetChild(i).gameObject;
            }

            foreach (var child in children)
            {
                child.SetActive(false);
            }
        }
    }

    private void SetItemSize()
    {
        RectTransform rectTransform = _origin.GetComponent<RectTransform>();
        if (rectTransform == null)
            throw new NullReferenceException();

        _itemSize.x = rectTransform.rect.width * rectTransform.localScale.x;
        _itemSize.y = rectTransform.rect.height * rectTransform.localScale.y;
    }

    private void SetColumnCount()
    {
        if (_columnCount <= 0)
        {
            float width = _viewPortRectTransform.rect.width - (_padding.left + _padding.right);
            _calcColumnCount = Mathf.Max(1, Mathf.FloorToInt(width / (_itemSize.x + _itemGap.x)));
        }
        else
        {
            _calcColumnCount = _columnCount;
        }
    }

    private void SetRowCount()
    {
        if (_rowCount <= 0)
        {
            float width = _viewPortRectTransform.rect.height - (_padding.top + _padding.bottom);
            _calcRowCount = Mathf.Max(1, Mathf.FloorToInt(width / (_itemSize.y + _itemGap.y)));
        }
        else
        {
            _calcRowCount = _rowCount;
        }
    }

    private void SetPoolItemCount()
    {
        if (_poolItemCount <= 0)
        {
            switch (_scrollDirection)
            {
                case PoolingScrollViewDirection.Vertical:
                    {
                        int line = Mathf.RoundToInt(_viewPortRectTransform.rect.size.y / _itemSize.y);
                        line += 2;
                        _poolItemCount = line * _calcColumnCount;
                    }
                    break;
                case PoolingScrollViewDirection.Horizontal:
                    {
                        int line = Mathf.RoundToInt(_viewPortRectTransform.rect.size.x / _itemSize.x);
                        line += 2;
                        _poolItemCount = line * _calcRowCount;
                    }
                    break;
            }
        }
    }

    private void SetContentTransform()
    {
        switch (_scrollDirection)
        {
            case PoolingScrollViewDirection.Vertical:
                {
                    //# 스트레치 앵커로 설정
                    _contentRectTransform.anchorMax = new Vector2(.5f, 1f);
                    _contentRectTransform.anchorMin = new Vector2(.5f, 1f);
                    _contentRectTransform.pivot = new Vector2(.5f, 1f);

                    //# 사이즈 재설정
                    _contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _viewPortRectTransform.rect.width);
                    _contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ScrollViewHeight);

                    //# 컨텐츠 최상단으로 이동
                    _contentRectTransform.anchoredPosition = Vector2.zero;
                }
                break;
            case PoolingScrollViewDirection.Horizontal:
                {
                    //# 스트레치 앵커로 설정
                    _contentRectTransform.anchorMax = new Vector2(0f, .5f);
                    _contentRectTransform.anchorMin = new Vector2(0f, .5f);
                    _contentRectTransform.pivot = new Vector2(0f, .5f);

                    //# 사이즈 재설정
                    _contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ScrollViewWidth);
                    _contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _viewPortRectTransform.rect.height);

                    //# 컨텐츠 최상단으로 이동
                    _contentRectTransform.anchoredPosition = Vector2.zero;
                }
                break;
        }
    }

    private Vector2 GetItemPosition(int index)
    {
        Vector2 pos = Vector2.zero;

        switch (_scrollDirection)
        {
            case PoolingScrollViewDirection.Horizontal:
                pos = GetItemHorizontalPosition(index);
                break;
            case PoolingScrollViewDirection.Vertical:
                pos = GetItemVerticalPosition(index);
                break;
        }

        return pos;
    }

    private Vector2 GetItemHorizontalPosition(int index)
    {
        int rowIndex = index % _calcRowCount;

        float y = rowIndex * _itemSize.y;
        y += rowIndex * _itemGap.y;

        float diff = ContentHeight - ItemsHeight;

        switch (_align)
        {
            case PoolingScrollViewAlign.LeftOrTop:
                {
                    y += 0;
                    y += _padding.top;
                }
                break;
            case PoolingScrollViewAlign.Center:
                {
                    y += (diff) * 0.5f;
                    y += _padding.top;
                }
                break;
            case PoolingScrollViewAlign.RightOrBottom:
                {
                    y += diff;
                    y -= _padding.bottom;
                }
                break;
        }

        y *= -1;

        int colIndex = index / _calcRowCount;

        float x = colIndex * _itemSize.x;
        x += colIndex * _itemGap.x;
        x += _padding.left;

        return new Vector2(x, y);
    }

    private Vector2 GetItemVerticalPosition(int index)
    {
        int colIndex = index % _calcColumnCount;

        float x = colIndex * _itemSize.x;
        x += colIndex * _itemGap.x;

        float diff = ContentWidth - ItemsWidth;

        switch (_align)
        {
            case PoolingScrollViewAlign.LeftOrTop:
                {
                    x += 0;
                    x += _padding.left;
                }
                break;
            case PoolingScrollViewAlign.Center:
                {
                    x += (diff) * 0.5f;
                    x += _padding.left;
                }
                break;
            case PoolingScrollViewAlign.RightOrBottom:
                {
                    x += diff;
                    x -= _padding.right;
                }
                break;
        }

        int rowIndex = index / _calcColumnCount;

        float y = rowIndex * _itemSize.y;
        y += rowIndex * _itemGap.y;
        y += _padding.top;
        y *= -1;

        return new Vector2(x, y);
    }

    public Rect GetViewConentRect()
    {
        Rect rect = new Rect();

        switch (_scrollDirection)
        {
            case PoolingScrollViewDirection.Vertical:
                {
                    float scrollRectY = _viewPortRectTransform.rect.size.y;
                    rect.Set(_contentRectTransform.anchoredPosition.x, -1 * (_contentRectTransform.anchoredPosition.y + scrollRectY), ContentWidth, scrollRectY + _itemSize.y);
                }
                break;
            case PoolingScrollViewDirection.Horizontal:
                {
                    float scrollRectX = _viewPortRectTransform.rect.size.x;
                    float scrollRectY = _viewPortRectTransform.rect.size.y;
                    rect.Set(-1 * _contentRectTransform.anchoredPosition.x, -1 * (_contentRectTransform.anchoredPosition.y + scrollRectY), scrollRectX + _itemSize.x, ContentHeight);
                }
                break;
        }

        return rect;
    }

    private void OnScroll(Vector2 scrollPosition)
    {
        if (_liPoolItem.Count <= 0)
            return;

        Vector2 delta = scrollPosition - _prevScrollPosition;
        _prevScrollPosition = scrollPosition;

        switch (_scrollDirection)
        {
            case PoolingScrollViewDirection.Vertical:
                {
                    //# 이전 위치와 달라졌다면
                    if (delta.y != 0)
                    {
                        UpdateContent(delta.y > 0 ? PoolingScrollViewScrollingDirection.UpOrLeft : PoolingScrollViewScrollingDirection.DownOrRight);
                    }
                }
                break;
            case PoolingScrollViewDirection.Horizontal:
                {
                    //# 이전 위치와 달라졌다면
                    if (delta.x != 0)
                    {
                        UpdateContent(delta.x < 0 ? PoolingScrollViewScrollingDirection.UpOrLeft : PoolingScrollViewScrollingDirection.DownOrRight);
                    }
                }
                break;
        }
    }

    private void UpdateContent(PoolingScrollViewScrollingDirection direction)
    {
        if (_liPoolItem.Count <= 0)
            return;

        Rect contentRect = GetViewConentRect();
        Rect itemRect = new Rect();

        switch (direction)
        {
            case PoolingScrollViewScrollingDirection.UpOrLeft:
                {
                    int firstIndex = _liPoolItem.First.Value.index;
                    for (int i = firstIndex - 1; i >= 0; --i)
                    {
                        Vector2 itemPosition = GetItemPosition(i);
                        itemRect.Set(itemPosition.x, itemPosition.y, _itemSize.x, _itemSize.y);

                        if (contentRect.Overlaps(itemRect))
                        {
                            var node = _liPoolItem.Last;
                            _liPoolItem.Remove(node);

                            InitItem(node.Value.item, i);

                            node.Value.index = i;
                            _liPoolItem.AddFirst(node);
                        }
                    }
                }
                break;
            case PoolingScrollViewScrollingDirection.DownOrRight:
                {
                    int lastIndex = _liPoolItem.Last.Value.index;
                    for (int i = lastIndex + 1; i < _liData.Count; ++i)
                    {
                        Vector2 itemPosition = GetItemPosition(i);
                        itemRect.Set(itemPosition.x, itemPosition.y, _itemSize.x, _itemSize.y);

                        if (contentRect.Overlaps(itemRect))
                        {
                            var node = _liPoolItem.First;
                            _liPoolItem.Remove(node);

                            InitItem(node.Value.item, i);

                            node.Value.index = i;
                            _liPoolItem.AddLast(node);
                        }
                    }
                }
                break;
        }
    }

    private void InitItem()
    {
        _liPoolItem.Clear();

        //# 생성된 풀링 오브젝트 가져옴
        int childCount = _contentObject.transform.childCount;
        GameObject[] arrChildObj = new GameObject[childCount];
        for (int i = 0; i < childCount; ++i)
        {
            arrChildObj[i] = _contentObject.transform.GetChild(i).gameObject;
        }

        Rect contentRect = GetViewConentRect();
        Rect itemRect = new Rect();
        int firstIndex = Enumerable.Range(0, _liData.Count).FirstOrDefault(_ =>
        {
            Vector2 itemPosition = GetItemPosition(_);
            itemRect.Set(itemPosition.x, itemPosition.y, _itemSize.x, _itemSize.y);

            //# Content와 겹치는 여부 반환
            return contentRect.Overlaps(itemRect);
        });

        for (int i = 0; i < childCount; ++i)
        {
            arrChildObj[i].SetActive(true);
        }

        for (int i = 0; i < arrChildObj.Length; ++i)
        {
            int index = i + firstIndex;
            TItem item = arrChildObj[i].GetComponent<TItem>();

            InitItem(item, index);
            PoolingScrollViewItem<TItem> poolItem = new PoolingScrollViewItem<TItem>() { index = index, item = item };
            _liPoolItem.AddLast(poolItem);
        }
    }

    private void InitItem(TItem item, int index)
    {
        TData info = _liData.ElementAtOrDefault(index);

        bool isNotNull = info != null;
        item.gameObject.SetActive(isNotNull);

        if (isNotNull)
        {
            InitItem(item, info, index);
        }

        InitItemTransform(item.gameObject, index);
        item.gameObject.name = $"{_origin.name} {index}";
    }

    private void InitItemTransform(GameObject item, int index)
    {
        var rectTransform = item.GetComponent<RectTransform>();

        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);

        rectTransform.anchoredPosition = GetItemPosition(index);
        rectTransform.SetSiblingIndex(index);
    }

    private void CreatePoolingObject()
    {
        int diff = _poolItemCount - _contentObject.transform.childCount;
        diff = Mathf.Min(diff, _liData.Count); // 필요한 만큼만 풀을 만들자
        if (diff > 0)
        {
            for (int i = 0; i < diff; ++i)
            {
                GameObject obj = Instantiate(_origin, _contentObject.transform);
                InitPoolingObject(obj.GetComponent<TItem>());
            }
        }
    }
}

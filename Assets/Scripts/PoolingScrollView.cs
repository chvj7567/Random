using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using System.Linq;

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
public abstract class PoolingScrollView<TItem, TData> : MonoBehaviour where TItem : MonoBehaviour
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

    [SerializeField, Header("������ ������ ������Ʈ")]
    private GameObject _origin;

    [SerializeField, Header("������ ũ��(Zero�̸� Origin ������� ����")]
    private Vector2 _itemSize = Vector2.zero;

    [SerializeField, Header("������ ���� ����")]
    private Vector2 _itemGap = Vector2.zero;

    [SerializeField, Header("�е� ����")]
    private RectOffset _padding = new RectOffset();

    [SerializeField, Header("��ũ�� ���� ����")]
    private PoolingScrollViewDirection _scrollDirection = PoolingScrollViewDirection.Vertical;

    [SerializeField, Header("�� ����, 0�����̸� �ڵ� ���")]
    private int _rowCount = 0;

    [SerializeField, Header("�� ����, 0�����̸� �ڵ� ���")]
    private int _columnCount = 0;

    [SerializeField, Header("������ƮǮ ���� ����, 0 �����̸� �ڵ� �Ҵ�")]
    private int _poolItemCount = 0;

    [SerializeField, Header("������ ���� ����")]
    private PoolingScrollViewAlign _align = PoolingScrollViewAlign.Center;

    [SerializeField, Header("��ũ�Ѻ� ������ ����� ���ΰ�ħ ����")]
    private bool _refresh = false;

    [Space]

    private Vector2 _prevScrollPosition = Vector2.zero;
    private GameObject _objContent;
    private RectTransform _rtContent;
    private RectTransform _rtViewPort;
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
            return _rtContent.rect.width;
        }
    }

    public float ContentHeight
    {
        get
        {
            return _rtContent.rect.height;
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

        if (_rtViewPort == null)
            _rtViewPort = _scrollRect.viewport;

        if (_objContent == null)
            _objContent = _scrollRect.content.gameObject;

        if (_rtContent == null)
            _rtContent = _scrollRect.content.GetComponent<RectTransform>();

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
    /// ������ �ʱ�ȭ ��ũ���Ͽ� �ε����� �ٲ𶧸��� ȣ��
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="info"></param>
    /// <param name="index"></param>
    public abstract void InitItem(TItem obj, TData info, int index);

    /// <summary>
    /// Ǯ�� ������Ʈ ���� �� ȣ��(���� 1ȸ)
    /// </summary>
    /// <param name="obj"></param>
    public abstract void InitPoolingObject(TItem obj);

    /// <summary>
    /// ��ũ�� �� ������ ����
    /// </summary>
    /// <param name="dataList"></param>
    public void SetItemList(List<TData> dataList)
    {
        _liData.Clear();
        _liData.AddRange(dataList);

        // ������ ũ�� ����
        SetItemSize();

        // �� ���� ����
        SetRowCount();

        // �� ���� ����
        SetColumnCount();

        // ������ Ǯ�� ���� ����
        SetPoolItemCount();

        // Content ������Ʈ ����
        SetContentTransform();

        // Ǯ�� ������Ʈ ����
        CreatePoolingObject();

        // ������ �ʱ�ȭ
        InitItem();
    }

    public void Refresh()
    {
        SetItemList(_liData.ToList());
    }

    public void Clear()
    {
        _liData.Clear();
        _liPoolItem.Clear();
        if (_objContent)
        {
            int childCount = _objContent.transform.childCount;
            GameObject[] children = new GameObject[childCount];

            for (int i = 0; i < childCount; i++)
            {
                children[i] = _objContent.transform.GetChild(i).gameObject;
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
            float width = _rtViewPort.rect.width - (_padding.left + _padding.right);
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
            float width = _rtViewPort.rect.height - (_padding.top + _padding.bottom);
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
                        int line = Mathf.RoundToInt(_rtViewPort.rect.size.y / _itemSize.y);
                        line += 2;
                        _poolItemCount = line * _calcColumnCount;
                    }
                    break;
                case PoolingScrollViewDirection.Horizontal:
                    {
                        int line = Mathf.RoundToInt(_rtViewPort.rect.size.x / _itemSize.x);
                        line += 2;
                        _poolItemCount = line * _calcRowCount;
                    }
                    break;
            }
        }
    }

    public virtual void SetContentTransform()
    {
        switch (_scrollDirection)
        {
            case PoolingScrollViewDirection.Vertical:
                {
                    // ��Ʈ��ġ ��Ŀ�� ����
                    _rtContent.anchorMax = new Vector2(.5f, 1f);
                    _rtContent.anchorMin = new Vector2(.5f, 1f);
                    _rtContent.pivot = new Vector2(.5f, 1f);

                    // ������ �缳��
                    _rtContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _rtViewPort.rect.width);
                    _rtContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ScrollViewHeight);

                    // ������ �ֻ������ �̵�
                    _rtContent.anchoredPosition = Vector2.zero;
                }
                break;
            case PoolingScrollViewDirection.Horizontal:
                {
                    // ��Ʈ��ġ ��Ŀ�� ����
                    _rtContent.anchorMax = new Vector2(0f, .5f);
                    _rtContent.anchorMin = new Vector2(0f, .5f);
                    _rtContent.pivot = new Vector2(0f, .5f);

                    // ������ �缳��
                    _rtContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ScrollViewWidth);
                    _rtContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _rtViewPort.rect.height);

                    // ������ �ֻ������ �̵�
                    _rtContent.anchoredPosition = Vector2.zero;
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

        Debug.Log($"GetItemHorizontalPosition {index} H {ContentHeight} - {_calcRowCount} * {_itemSize.y} + {(_calcRowCount - 1)} * {_itemGap.y}");
        Debug.Log($"GetItemHorizontalPosition {index} W {ContentWidth} - {ItemsWidth}");
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

        // �� �ε���
        int colIndex = index / _calcRowCount;

        float x = colIndex * _itemSize.x;
        x += colIndex * _itemGap.x;
        x += _padding.left;

        return new Vector2(x, y);
    }

    private Vector2 GetItemVerticalPosition(int index)
    {
        // �� �ε���
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

        // �� �ε���
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
                    float scrollRectY = _rtViewPort.rect.size.y;
                    rect.Set(_rtContent.anchoredPosition.x, -1 * (_rtContent.anchoredPosition.y + scrollRectY), ContentWidth, scrollRectY + _itemSize.y);
                }
                break;
            case PoolingScrollViewDirection.Horizontal:
                {
                    float scrollRectX = _rtViewPort.rect.size.x;
                    float scrollRectY = _rtViewPort.rect.size.y;
                    rect.Set(-1 * _rtContent.anchoredPosition.x, -1 * (_rtContent.anchoredPosition.y + scrollRectY), scrollRectX + _itemSize.x, ContentHeight);
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
                    // ���� ��ġ�� �޶����ٸ�
                    if (delta.y != 0)
                    {
                        UpdateContent(delta.y > 0 ? PoolingScrollViewScrollingDirection.UpOrLeft : PoolingScrollViewScrollingDirection.DownOrRight);
                    }
                }
                break;
            case PoolingScrollViewDirection.Horizontal:
                {
                    // ���� ��ġ�� �޶����ٸ�
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

        // �����˻��� ������ �簢���� ���Ʒ��� �����߰�
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

        // ������ Ǯ�� ������Ʈ ������
        int childCount = _objContent.transform.childCount;
        GameObject[] arrChildObj = new GameObject[childCount];
        for (int i = 0; i < childCount; ++i)
        {
            arrChildObj[i] = _objContent.transform.GetChild(i).gameObject;
        }

        Rect contentRect = GetViewConentRect();
        Rect itemRect = new Rect();
        int firstIndex = Enumerable.Range(0, _liData.Count).FirstOrDefault(_ =>
        {
            Vector2 itemPosition = GetItemPosition(_);
            itemRect.Set(itemPosition.x, itemPosition.y, _itemSize.x, _itemSize.y);

            // Content�� ��ġ�� ���� ��ȯ
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
        int diff = _poolItemCount - _objContent.transform.childCount;
        diff = Mathf.Min(diff, _liData.Count); // �ʿ��� ��ŭ�� Ǯ�� ������
        if (diff > 0)
        {
            for (int i = 0; i < diff; ++i)
            {
                GameObject obj = Instantiate(_origin, _objContent.transform);
                InitPoolingObject(obj.GetComponent<TItem>());
            }
        }
    }
}

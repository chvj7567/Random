using ChvjUnityInfra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIRouletteArg : UIArg
{
    public List<string> liText = new List<string>();
}

public class UIRoulette : UIBase
{
    UIRouletteArg _arg;

    [Serializable]
    class CircleRouletteResult
    {
        public string value;
        public float minAngle;
        public float maxAngle;
    }

    [Header("원형 룰렛")]
    [SerializeField] private GameObject _circleRoulette;
    [SerializeField] private RectTransform _radius;
    [SerializeField] private RectTransform _arrowObject;
    [SerializeField] private RectTransform _rouletteObject;
    [SerializeField] private RouletteItem _itemObject;
    [SerializeField] private RectTransform _lineObject;
    [SerializeField, Header("x축 0도 기준")] private float standard = 90f;

    [Header("스크롤 룰렛")]
    [SerializeField] private GameObject _scrollRoulette;
    [SerializeField] private RouletteScrollView _scrollView;

    private List<GameObject> _copyObjects = new List<GameObject>();
    private List<CircleRouletteResult> _rouletteResults = new List<CircleRouletteResult>();
    private float _rouletteRadius;
    private bool _rouletteComplete = false;

    public override void InitUI(UIArg arg)
    {
        _arg = arg as UIRouletteArg;

        _copyObjects.Clear();
        _circleRoulette.SetActive(false);
        _scrollRoulette.SetActive(false);

        //# 항목 수 10개 이하면 원형 룰렛
        //# 항목 수 10개 초과면 스크롤 룰렛
        if (_arg.liText.Count <= 10)
        {
            _circleRoulette.SetActive(true);
            CreateCircleRoulette(_arg.liText);
            SpinCircleRoulette();
        }
        else
        {
            _scrollRoulette.SetActive(true);
            CreateScrollRoulette(_arg.liText);
            SpinScrollRoulette(_arg.liText.Count - 1, 2);
        }
    }

    public override void Close(bool reuse = true)
    {
        if (_rouletteComplete == false)
            return;

        base.Close(false);

        foreach (GameObject obj in _copyObjects)
        {
            Destroy(obj);
        }
    }

    private void CreateCircleRoulette(List<string> liText)
    {
        if (liText == null || liText.Count == 0)
            return;

        _itemObject.rectTransform.gameObject.SetActive(false);
        _lineObject.gameObject.SetActive(false);

        SetPosition();

        if (liText.Count == 1)
        {
            GameObject newItemObject = Instantiate(_itemObject.rectTransform.gameObject, _rouletteObject.transform);
            newItemObject.SetActive(true);

            RectTransform itemRectTransform = newItemObject.GetComponent<RectTransform>();
            itemRectTransform.RotateZRoation(90);

            newItemObject.transform.position = _rouletteObject.transform.position;

            RouletteItem item = newItemObject.GetComponent<RouletteItem>();
            item.text.SetText(liText[0]);

            _rouletteResults.Add(new CircleRouletteResult
            {
                value = liText[0],
                minAngle = 0f,
                maxAngle = 360f,
            });

            _copyObjects.Add(newItemObject);
        }
        else
        {
            float angle = 360f / liText.Count;
            float halfAngle = angle / 2f;
            float itemAngle = halfAngle + standard;
            float lineAngle = 0f + standard;

            foreach (string text in liText)
            {
                GameObject newItemObject = Instantiate(_itemObject.rectTransform.gameObject, _rouletteObject);
                newItemObject.SetActive(true);
                
                RectTransform itemRectTransform = newItemObject.GetComponent<RectTransform>();
                itemRectTransform.RotateXYPosition(_rouletteObject, _rouletteRadius * .7f, itemAngle);
                itemRectTransform.RotateZRoation(itemAngle);

                RouletteItem item = newItemObject.GetComponent<RouletteItem>();
                item.text.SetText(text);

                GameObject newLineObject = Instantiate(_lineObject.gameObject, _rouletteObject.transform);
                newLineObject.SetActive(true);

                RectTransform lineRectTransform = newLineObject.GetComponent<RectTransform>();
                lineRectTransform.RotateZRoation(lineAngle);
                newLineObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _rouletteRadius);

                _rouletteResults.Add(new CircleRouletteResult
                {
                    value = text,
                    minAngle = 360 - lineAngle - angle + standard,
                    maxAngle = 360 - lineAngle + standard,
                });

                itemAngle += angle;
                lineAngle += angle;

                _copyObjects.Add(newItemObject);
                _copyObjects.Add(newLineObject);
            }
        }
    }

    private void SpinCircleRoulette()
    {
        _rouletteObject.Spin((angle) =>
        {
            angle = angle % 360;
            foreach (CircleRouletteResult result in _rouletteResults)
            {
                if (result.minAngle <= angle &&
                    result.maxAngle > angle)
                {
                    ShowResult(result.value);
                }
            }
        }, 3);
    }

    /// <summary>
    /// 위치 및 크기 설정
    /// </summary>
    private void SetPosition()
    {
        //# 룰렛 화살표 위치
        _arrowObject.RotateXYPosition(_rouletteObject, 500f, standard);
        _arrowObject.RotateZRoation(standard);

        //# 룰렛 반지름 설정
        _rouletteRadius = Vector2.Distance(_rouletteObject.anchoredPosition, _arrowObject.anchoredPosition);
        _rouletteObject.sizeDelta = new Vector2(_rouletteRadius * 2, _rouletteRadius * 2);

        //# 화살표 사이즈 설정
        float arrowSize = _rouletteRadius / 6f;
        _arrowObject.sizeDelta = new Vector2(arrowSize, arrowSize);

        //# 아이템 사이즈 설정
        float itemSize = _rouletteRadius / 4f;
        _itemObject.rectTransform.sizeDelta = new Vector2(itemSize, itemSize);
    }

    private void CreateScrollRoulette(List<string> liText)
    {
        liText.Add(liText.First());

        _scrollView.SetItemList(liText);
    }

    private void SpinScrollRoulette(int lastIndex,int repeatCount)
    {
        if (repeatCount == 0)
        {
            RandomScroll();
            return;
        }

        _scrollView.SetScrollPosition(lastIndex, () =>
        {
            _scrollView.SetScrollPosition(0, () =>
            {
                SpinScrollRoulette(lastIndex, repeatCount - 1);
            }, 0f);
        }, 1f);
    }

    private void RandomScroll()
    {
        int randomIndex = UnityEngine.Random.Range(0, _arg.liText.Count - 2);
        _scrollView.SetScrollPosition(randomIndex, () =>
        {
            ShowResult(_arg.liText[randomIndex]);
        }, 1f);
    }

    private void ShowResult(string result)
    {
        string alarmText = JsonManager.Instance.GetStringData(2);
        alarmText = string.Format(alarmText, result);

        CHMUI.Instance.ShowUI(CommonEnum.EUI.UIAlarm, new UIAlarmArg
        {
            alarmText = alarmText,
        }, _ =>
        {
            _rouletteComplete = true;
        });
    }
}

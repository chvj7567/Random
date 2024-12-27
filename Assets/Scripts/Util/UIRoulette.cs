using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    [SerializeField] private float standard = 90f;

    [Header("스크롤 룰렛")]
    [SerializeField] private GameObject _scrollRoulette;
    [SerializeField] private RouletteScrollView _scrollView;

    private List<CircleRouletteResult> _rouletteResult = new List<CircleRouletteResult>();

    public override void InitUI(CommonEnum.EUI uiType, UIArg arg)
    {
        base.InitUI(uiType, arg);

        _arg = arg as UIRouletteArg;

        _circleRoulette.SetActive(false);
        _scrollRoulette.SetActive(false);

        //# 목록 수가 10개 초과면 스크롤 룰렛
        //# 목록 수가 10개 이하면 원형 룰렛
        if (_arg.liText.Count > 10)
        {
            _scrollRoulette.SetActive(true);
            CreateScrollRoulette(_arg.liText);
            SpinScrollRoulette(_arg.liText.Count - 1, 2);
        }
        else
        {
            _circleRoulette.SetActive(true);
            CreateCircleRoulette(_arg.liText);
            SpinCircleRoulette();
        }
    }

    public override void Close(bool reuse = true)
    {
        base.Close(false);
    }

    private void CreateCircleRoulette(List<string> liText)
    {
        if (liText == null)
            return;

        _itemObject.rectTransform.gameObject.SetActive(false);
        _lineObject.gameObject.SetActive(false);

        float arrowDistance = Vector2.Distance(_rouletteObject.anchoredPosition, _arrowObject.anchoredPosition);
        _arrowObject.RotateXYPosition(_rouletteObject, arrowDistance, standard);
        _arrowObject.RotateZRoation(standard);

        if (liText.Count == 0)
            return;

        if (liText.Count == 1)
        {
            _itemObject.rectTransform.gameObject.SetActive(true);
            _itemObject.rectTransform.anchoredPosition = _rouletteObject.anchoredPosition;
        }
        else
        {
            float distance = Vector2.Distance(_rouletteObject.anchoredPosition, _itemObject.rectTransform.anchoredPosition);
            float angle = 360f / liText.Count;
            float halfAngle = angle / 2f;
            float itemAngle = halfAngle + standard;
            float lineAngle = 0f + standard;
            float radius = GetRadius();

            //# 룰렛 사이즈 지정
            _rouletteObject.sizeDelta = new Vector2(radius * 2, radius * 2);

            foreach (var text in liText)
            {
                GameObject newItemObject = Instantiate(_itemObject.rectTransform.gameObject, _rouletteObject.transform);
                newItemObject.SetActive(true);
                newItemObject.transform.RotateXYPosition(_rouletteObject, distance, itemAngle);
                newItemObject.transform.RotateZRoation(itemAngle);

                var item = newItemObject.GetComponent<RouletteItem>();
                item.text.text = text;

                GameObject newLineObject = Instantiate(_lineObject.gameObject, _rouletteObject.transform);
                newLineObject.SetActive(true);
                newLineObject.transform.RotateZRoation(lineAngle);
                newLineObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, radius);

                _rouletteResult.Add(new CircleRouletteResult
                {
                    value = text,
                    minAngle = 360 - lineAngle - angle + standard,
                    maxAngle = 360 - lineAngle + standard,
                });

                itemAngle += angle;
                lineAngle += angle;
            }
        }
    }

    private void SpinCircleRoulette()
    {
        _rouletteObject.Spin((angle) =>
        {
            angle = angle % 360;
            foreach (var result in _rouletteResult)
            {
                if (result.minAngle <= angle &&
                    result.maxAngle > angle)
                {
                    UIManager.Instance.ShowUI(CommonEnum.EUI.UIAlarm, new UIAlarmArg
                    {
                        alarmText = $"결과 : {result.value}"
                    });
                }
            }
        }, 3);
    }

    /// <summary>
    /// 해당 화면의 반지름값 가져오기
    /// </summary>
    private float GetRadius()
    {
        float radius = Vector2.Distance(_rouletteObject.anchoredPosition, _radius.anchoredPosition);
        _radius.transform.RotateXYPosition(_rouletteObject, radius, 0);

        return Vector2.Distance(_rouletteObject.anchoredPosition, _radius.anchoredPosition);
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

        _scrollView.SetScrollPosition(lastIndex, 1f, () =>
        {
            _scrollView.SetScrollPosition(0, 0, () =>
            {
                SpinScrollRoulette(lastIndex, repeatCount - 1);
            });
        });
    }

    private void RandomScroll()
    {
        int randomIndex = UnityEngine.Random.Range(0, _arg.liText.Count - 2);
        _scrollView.SetScrollPosition(randomIndex, 1f, () =>
        {
            UIManager.Instance.ShowUI(CommonEnum.EUI.UIAlarm, new UIAlarmArg
            {
                alarmText = $"결과 : {_arg.liText[randomIndex]}"
            });
        });
    }
}

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

    [Header("���� �귿")]
    [SerializeField] private GameObject _circleRoulette;
    [SerializeField] private RectTransform _radius;
    [SerializeField] private RectTransform _arrowObject;
    [SerializeField] private RectTransform _rouletteObject;
    [SerializeField] private RouletteItem _itemObject;
    [SerializeField] private RectTransform _lineObject;
    [SerializeField, Header("x�� 0�� ����")] private float standard = 90f;
    [SerializeField, Header("ȭ��ǥ ��ġ Offset (������ ����)")] private float _arrowOffset = 0f;
    [SerializeField, Header("������ ��ġ Offset (������ ����)")] private float _itemOffset = -85f;

    [Header("��ũ�� �귿")]
    [SerializeField] private GameObject _scrollRoulette;
    [SerializeField] private RouletteScrollView _scrollView;

    private List<CircleRouletteResult> _rouletteResult = new List<CircleRouletteResult>();
    private float _rouletteRadius;

    public override void InitUI(CommonEnum.EUI uiType, UIArg arg)
    {
        base.InitUI(uiType, arg);

        _arg = arg as UIRouletteArg;

        _circleRoulette.SetActive(false);
        _scrollRoulette.SetActive(false);

        //# ��� ���� 10�� ���ϸ� ���� �귿
        //# ��� ���� 10�� �ʰ��� ��ũ�� �귿
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
        base.Close(false);
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

            var item = newItemObject.GetComponent<RouletteItem>();
            item.text.text = liText[0];

            _rouletteResult.Add(new CircleRouletteResult
            {
                value = liText[0],
                minAngle = 0f,
                maxAngle = 360f,
            });
        }
        else
        {
            float angle = 360f / liText.Count;
            float halfAngle = angle / 2f;
            float itemAngle = halfAngle + standard;
            float lineAngle = 0f + standard;

            foreach (var text in liText)
            {
                GameObject newItemObject = Instantiate(_itemObject.rectTransform.gameObject, _rouletteObject);
                newItemObject.SetActive(true);
                
                RectTransform itemRectTransform = newItemObject.GetComponent<RectTransform>();
                itemRectTransform.RotateXYPosition(_rouletteObject, _rouletteRadius * .7f, itemAngle);
                itemRectTransform.RotateZRoation(itemAngle);

                var item = newItemObject.GetComponent<RouletteItem>();
                item.text.text = text;

                GameObject newLineObject = Instantiate(_lineObject.gameObject, _rouletteObject.transform);
                newLineObject.SetActive(true);

                RectTransform lineRectTransform = newLineObject.GetComponent<RectTransform>();
                lineRectTransform.RotateZRoation(lineAngle);
                newLineObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _rouletteRadius);

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
                    ShowResult(result.value);
                }
            }
        }, 3);
    }

    /// <summary>
    /// ��ġ �� ũ�� ����
    /// </summary>
    private void SetPosition()
    {
        //# �귿 ȭ��ǥ ��ġ
        _arrowObject.RotateXYPosition(_rouletteObject, 500f, standard);
        _arrowObject.RotateZRoation(standard);

        //# �귿 ������ ����
        _rouletteRadius = Vector2.Distance(_rouletteObject.anchoredPosition, _arrowObject.anchoredPosition);
        _rouletteObject.sizeDelta = new Vector2(_rouletteRadius * 2, _rouletteRadius * 2);

        //# ȭ��ǥ ������ ����
        float arrowSize = _rouletteRadius / 6f;
        _arrowObject.sizeDelta = new Vector2(arrowSize, arrowSize);
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
            ShowResult(_arg.liText[randomIndex]);
        });
    }

    private void ShowResult(string result)
    {
        string alarmText = JsonManager.Instance.GetStringData(2);
        alarmText = string.Format(alarmText, result);

        UIManager.Instance.ShowUI(CommonEnum.EUI.UIAlarm, new UIAlarmArg
        {
            alarmText = alarmText,
        });
    }
}

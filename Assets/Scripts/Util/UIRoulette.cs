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
    class RouletteResult
    {
        public string value;
        public float minAngle;
        public float maxAngle;
    }

    [SerializeField] private RectTransform _radius;
    [SerializeField] private RectTransform _arrowObject;
    [SerializeField] private RectTransform _rouletteObject;
    [SerializeField] private RouletteItem _itemObject;
    [SerializeField] private RectTransform _lineObject;
    [SerializeField] private float standard = 90f;

    private List<RouletteResult> _rouletteResult = new List<RouletteResult>();

    public override void InitUI(CommonEnum.EUI uiType, UIArg arg)
    {
        base.InitUI(uiType, arg);

        _arg = arg as UIRouletteArg;

        if (_arg.liText.Count > 1000)
        {
            if (int.TryParse(_arg.liText.First(), out int startNumber) &&
                int.TryParse(_arg.liText.Last(), out int endNumber))
            {
                int result = UnityEngine.Random.Range(startNumber, endNumber);

                UIManager.Instance.ShowUI(CommonEnum.EUI.UIAlarm, new UIAlarmArg
                {
                    alarmText = $"결과 : {result}"
                });
            }

            Close();
        }
        else
        {
            CreateRoulette(_arg.liText);
            Spin();
        }
    }

    public override void Close(bool reuse = true)
    {
        base.Close(false);
    }

    void CreateRoulette(List<string> liText)
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

                _rouletteResult.Add(new RouletteResult
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

    private void Spin()
    {
        _rouletteObject.ZSpin((angle) =>
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
        });
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
}

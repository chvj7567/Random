using System;
using System.Collections.Generic;
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

    [SerializeField]
    private RectTransform _arrowObject;
    [SerializeField]
    private RectTransform _rouletteObject;
    [SerializeField]
    private RouletteItem _itemObject;
    [SerializeField]
    private RectTransform _lineObject;
    [SerializeField]
    private float standard = 90f;

    private List<RouletteResult> _rouletteResult = new List<RouletteResult>();

    public override void InitUI(CommonEnum.EUI uiType, UIArg arg)
    {
        base.InitUI(uiType, arg);

        _arg = arg as UIRouletteArg;

        CreateRoulette(_arg.liText);
        Spin();
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
            foreach (var text in liText)
            {
                GameObject newItemObject = Instantiate(_itemObject.rectTransform.gameObject, _rouletteObject.transform);
                newItemObject.SetActive(true);
                newItemObject.transform.RotateXYPosition(_rouletteObject, distance, itemAngle);
                newItemObject.transform.RotateZRoation(itemAngle);
                newItemObject.GetComponent<RouletteItem>().text.text = text;

                GameObject newLineObject = Instantiate(_lineObject.gameObject, _rouletteObject.transform);
                newLineObject.SetActive(true);
                newLineObject.transform.RotateZRoation(lineAngle);

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

    void Spin()
    {
        _rouletteObject.ZSpin((angle) =>
        {
            angle = angle % 360;
            foreach (var result in _rouletteResult)
            {
                if (result.minAngle <= angle &&
                    result.maxAngle > angle)
                {
                    Debug.Log(result.value);
                }
            }
        });
    }
}

using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class UIRouletteArg : UIArg
{
    public float radius = 50f;
    public List<string> liText = new List<string>();
}

public class UIRoulette : UIBase
{
    UIRouletteArg _arg;

    [SerializeField] RectTransform _rouletteObject;
    [SerializeField] RectTransform _itemObject;
    [SerializeField] RectTransform _lineObject;

    public override void InitUI(UIArg arg)
    {
        base.InitUI(arg);

        _arg = arg as UIRouletteArg;

        int itemCount = _arg.liText.Count;
        if (itemCount == 0)
            return;

        _itemObject.gameObject.SetActive(true);
        _lineObject.gameObject.SetActive(true);

        if (itemCount == 1)
        {
            _lineObject.gameObject.SetActive(false);
            _itemObject.anchoredPosition = _rouletteObject.anchoredPosition;
        }
        else
        {
            float angle = 360f / itemCount;
            foreach (var text in _arg.liText)
            {
                GameObject newItemObject = Instantiate(_itemObject.gameObject, _rouletteObject.transform);
                newItemObject.transform.RotateXYPosition(_rouletteObject, _arg.radius, angle);
                newItemObject.transform.RotateZRoation(angle);

                GameObject newLineObject = Instantiate(_itemObject.gameObject, _rouletteObject.transform);
                newLineObject.transform.RotateZRoation(angle - angle / 2f);
            }
        }
    }
}

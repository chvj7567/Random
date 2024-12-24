using System;
using System.Collections.Generic;
using UnityEngine;


public class Rtest : MonoBehaviour
{
    [Serializable]
    class RouletteResult
    {
        public string value;
        public float minAngle;
        public float maxAngle;
    }

    [SerializeField]
    private RectTransform _rouletteObject;
    [SerializeField]
    private RouletteItem _itemObject;
    [SerializeField]
    private RectTransform _lineObject;
    [SerializeField]
    private List<string> _liText = new List<string>();

    [SerializeField]
    private List<RouletteResult> _rouletteResult = new List<RouletteResult>();

    private void OnEnable()
    {
        _itemObject.rectTransform.gameObject.SetActive(false);
        _lineObject.gameObject.SetActive(false);

        if (_liText.Count == 0)
            return;

        if (_liText.Count == 1)
        {
            _itemObject.rectTransform.gameObject.SetActive(true);
            _itemObject.rectTransform.anchoredPosition = _rouletteObject.anchoredPosition;
        }
        else
        {
            float distance = Vector2.Distance(_rouletteObject.anchoredPosition, _itemObject.rectTransform.anchoredPosition);
            float angle = 360f / _liText.Count;
            float nextAngle = angle;
            foreach (var text in _liText)
            {
                GameObject newItemObject = Instantiate(_itemObject.rectTransform.gameObject, _rouletteObject.transform);
                newItemObject.SetActive(true);
                newItemObject.transform.RotateXYPosition(_rouletteObject, distance, nextAngle);
                newItemObject.transform.RotateZRoation(nextAngle);
                newItemObject.GetComponent<RouletteItem>().text.text = text;

                GameObject newLineObject = Instantiate(_lineObject.gameObject, _rouletteObject.transform);
                newLineObject.SetActive(true);
                newLineObject.transform.RotateZRoation(nextAngle - angle / 2f);

                _rouletteResult.Add(new RouletteResult
                {
                    value = text,
                    minAngle = nextAngle - angle / 2f,
                    maxAngle = nextAngle + angle / 2f
                });

                nextAngle += angle;
            }
        }

        _rouletteObject.ZSpin((angle) =>
        {
            Debug.Log(angle);
            foreach (var result  in _rouletteResult)
            {
                if (result.minAngle < angle &&
                    result.maxAngle > angle)
                {
                    Debug.Log(result.value);
                }
            }
        });
    }
}

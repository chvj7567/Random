using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonEx : MonoBehaviour
{
    private Button _button;
    private TMP_Text _text;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _text = GetComponentInChildren<TMP_Text>();
    }

    public void SetText(string text)
    {
        if (_text == null)
            return;

        _text.text = text;
    }

    public void OnClick(Action callback)
    {
        _button.OnClickAsObservable().Subscribe(_ =>
        {
            callback?.Invoke();
        }).AddTo(this);
    }
}

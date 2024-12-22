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

    bool _initialize = false;

    public bool Interatable
    {
        get
        {
            return _button.interactable;
        }
        set
        {
            _button.interactable = value;
        }
    }

    private void Awake()
    {
        Init();
    }

    void Init()
    {
        if (_initialize)
            return;

        _initialize = true;
        _button = GetComponent<Button>();
        _text = GetComponentInChildren<TMP_Text>();
    }

    public void SetText(string text)
    {
        Init();

        _text.text = text;
    }

    public void OnClick(Action callback)
    {
        Init();

        _button.OnClickAsObservable().Subscribe(_ =>
        {
            AudioManager.Instance.Play(CommonEnum.EAudio.Click);
            callback?.Invoke();
        }).AddTo(this);
    }
}

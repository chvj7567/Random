using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class UIArg { }

public abstract class UIBase : MonoBehaviour
{
    [SerializeField] Button _backgroundButton;
    [SerializeField] Button _backButton;

    private void Awake()
    {
        if (_backgroundButton)
        {
            _backgroundButton.OnClickAsObservable().Subscribe(_ =>
            {
                UIManager.Instance.CloseUI(this);
            });
        }

        if (_backButton)
        {
            _backButton.OnClickAsObservable().Subscribe(_ =>
            {
                UIManager.Instance.CloseUI(this);
            });
        }
    }

    public virtual void InitArg(UIArg arg) { }

    public virtual void Close() { }
}

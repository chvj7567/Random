using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class UIArg { }

public abstract class UIBase : MonoBehaviour
{
    [SerializeField] private Button _backgroundButton;
    [SerializeField] private Button _backButton;

    protected CompositeDisposable closeDisposable = new CompositeDisposable();

    private void Awake()
    {
        if (_backgroundButton)
        {
            _backgroundButton.OnClickAsObservable().Subscribe(_ =>
            {
                UIManager.Instance.CloseUI(this);
            }).AddTo(closeDisposable);
        }

        if (_backButton)
        {
            _backButton.OnClickAsObservable().Subscribe(_ =>
            {
                UIManager.Instance.CloseUI(this);
            }).AddTo(closeDisposable);
        }
    }

    public virtual void InitArg(UIArg arg) { }

    public virtual void Close()
    {
        closeDisposable.Dispose();
    }
}

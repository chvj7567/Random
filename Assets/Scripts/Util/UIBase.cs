using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class UIArg { }

public class UIBase : MonoBehaviour
{
    [HideInInspector] public CommonEnum.EUI UIType;

    [SerializeField] private Button _backgroundButton;
    [SerializeField] private Button _backButton;

    protected CompositeDisposable closeDisposable = new CompositeDisposable();

    public virtual void InitUI(CommonEnum.EUI uiType, UIArg arg)
    {
        UIType = uiType;

        if (_backgroundButton)
        {
            _backgroundButton.OnClickAsObservable().Subscribe(_ =>
            {
                Close();
            }).AddTo(closeDisposable);
        }

        if (_backButton)
        {
            _backButton.OnClickAsObservable().Subscribe(_ =>
            {
                Close();
            }).AddTo(closeDisposable);
        }
    }

    public virtual void Close(bool reuse = true)
    {
        closeDisposable.Clear();

        if (reuse == false)
        {
            UIManager.Instance.RemoveCashingUI(UIType);
        }

        UIManager.Instance.CloseUI(this, reuse);
    }
}

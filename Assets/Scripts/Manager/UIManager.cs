using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviourSingletone<UIManager>
{
    private const string MyCanvasName = "UICanvas";
    private bool _initialize = false;
    private Transform _rootTransform;
    private ReactiveCollection<UIBase> _liCurrentUI = new ReactiveCollection<UIBase>();
    private ReactiveCollection<UIBase> _liWaitCloseUI = new ReactiveCollection<UIBase>();
    private Dictionary<CommonEnum.EUI, UIBase> _dicCashingUI = new Dictionary<CommonEnum.EUI, UIBase>();

    public bool CheckUI => _liCurrentUI.Count > 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_liCurrentUI.Count > 0)
                CloseUI(_liCurrentUI.Last());
        }
    }

    public async Task<bool> Init()
    {
        if (_initialize)
            return false;

        _initialize = true;

        TaskCompletionSource<bool> initComplete = new TaskCompletionSource<bool>();

        if (_rootTransform == null)
        {
            ResourceManager.Instance.LoadGameObject(MyCanvasName, (obj) =>
            {
                _rootTransform = obj.transform;
                DontDestroyOnLoad(_rootTransform);
                
                initComplete.SetResult(true);
            });
        }
        else
        {
            initComplete.SetResult(true);
        }

        _liCurrentUI.ObserveAdd().Subscribe(addUI =>
        {
            //# ���� UI ��⿭�� �߰��Ǿ����� �ݱ� UI ��Ͽ� ������ �ݱ�
            foreach (var waitUI in _liWaitCloseUI)
            {
                UIBase uiBase = addUI.Value;
                if (waitUI.UIType == addUI.Value.UIType)
                {
                    uiBase.gameObject.SetActive(false);

                    //# �ݾ����� ����
                    _liCurrentUI.Remove(uiBase);
                    _liWaitCloseUI.Remove(uiBase);
                    break;
                }
            }
        }).AddTo(this);

        _liWaitCloseUI.ObserveAdd().Subscribe(addUI =>
        {
            //# �ݱ� UI ��⿭�� �߰��Ǿ����� ���� UI ��Ͽ� ������ �ݱ�
            foreach (var curUI in _liCurrentUI)
            {
                UIBase uiBase = addUI.Value;
                if (curUI.UIType == uiBase.UIType)
                {
                    uiBase.gameObject.SetActive(false);

                    //# �ݾ����� ����
                    _liWaitCloseUI.Remove(uiBase);
                    _liCurrentUI.Remove(uiBase);
                    break;
                }
            }
        }).AddTo(this);

        return await initComplete.Task;
    }

    public void ShowUI(CommonEnum.EUI uiType, UIArg arg = null, Action<UIBase> callback = null)
    {
        //# �ش� UI�� �� ���̶� ���� ���� �ִٸ� ĳ���ϰ� �ִ� UI ����
        if (_dicCashingUI.TryGetValue(uiType, out var uiBase))
        {
            uiBase.InitUI(uiType, arg);
            uiBase.gameObject.SetActive(true);

            uiBase.transform.SetAsLastSibling();

            _liCurrentUI.Add(uiBase);

            callback?.Invoke(uiBase);
        }
        else
        {
            ResourceManager.Instance.LoadUI(uiType, (ui) =>
            {
                ui.transform.SetParent(_rootTransform);
                var rectTransform = ui.GetComponent<RectTransform>();
                //# Stretch ����
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);

                //# Offset ���� 0���� ����
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                //# ��ġ �� �� ������ ����
                rectTransform.anchoredPosition3D = Vector3.zero;
                rectTransform.localScale = Vector3.one;

                UIBase uiBase = ui.GetComponent<UIBase>();
                if (uiBase == null)
                    return;

                uiBase.InitUI(uiType, arg);
                _liCurrentUI.Add(uiBase);
                _dicCashingUI.Add(uiType, uiBase);

                callback?.Invoke(uiBase);
            });
        }
    }

    public void CloseUI(UIBase uiBase, bool reuse = false)
    {
        if (uiBase == null)
            return;

        _liWaitCloseUI.Add(uiBase);
    }

    public void RemoveCashingUI(CommonEnum.EUI uiType)
    {
        if (_dicCashingUI.TryGetValue(uiType, out var uiBase))
        {
            _dicCashingUI.Remove(uiBase.UIType);
            Destroy(uiBase.gameObject);
        }
    }
}

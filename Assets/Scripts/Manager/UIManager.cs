using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : Singletone<UIManager>
{
    const string MyCanvasName = "UICanvas";
    bool _initialize = false;

    Transform _rootTransform;

    LinkedList<UIBase> _liCurrentUI = new LinkedList<UIBase>();
    Dictionary<CommonEnum.EUI, UIBase> _dicCashingUI = new Dictionary<CommonEnum.EUI, UIBase>();

    public bool CheckUI => _liCurrentUI.Count > 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_liCurrentUI.Count > 0)
                CloseUI(_liCurrentUI.Last.Value);
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

        return await initComplete.Task;
    }

    public void ShowUI(CommonEnum.EUI uiType, UIArg arg = null, Action<UIBase> callback = null)
    {
        //# 해당 UI가 한 번이라도 열린 적이 있다면 캐싱하고 있는 UI 재사용
        if (_dicCashingUI.TryGetValue(uiType, out var uiBase))
        {
            uiBase.InitUI(uiType, arg);
            uiBase.gameObject.SetActive(true);
            _liCurrentUI.AddLast(uiBase);

            callback?.Invoke(uiBase);
        }
        else
        {
            ResourceManager.Instance.LoadUI(uiType, (ui) =>
            {
                ui.transform.SetParent(_rootTransform);
                var rectTransform = ui.GetComponent<RectTransform>();
                //# Stretch 설정
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);

                //# Offset 값을 0으로 설정
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                //# 위치 값 및 스케일 설정
                rectTransform.anchoredPosition3D = Vector3.zero;
                rectTransform.localScale = Vector3.one;

                UIBase uiBase = ui.GetComponent<UIBase>();
                if (uiBase == null)
                    return;

                uiBase.InitUI(uiType, arg);
                _liCurrentUI.AddLast(uiBase);
                _dicCashingUI.Add(uiType, uiBase);

                callback?.Invoke(uiBase);
            });
        }
    }

    public void CloseUI(UIBase uiBase)
    {
        if (uiBase == null)
            return;

        if (_liCurrentUI.Count == 0)
            return;

        uiBase.gameObject.SetActive(false);
        uiBase.Close();
        _liCurrentUI.RemoveLast();
    }

    public void CloseRecentUI()
    {
        if (_liCurrentUI.Count == 0)
            return;

        CloseUI(_liCurrentUI.Last());
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

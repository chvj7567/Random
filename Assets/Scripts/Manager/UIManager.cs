using System.Collections.Generic;
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

    public void ShowUI(CommonEnum.EUI uiType, UIArg arg)
    {
        //# �ش� UI�� �� ���̶� ���� ���� �ִٸ� ĳ���ϰ� �ִ� UI ����
        if (_dicCashingUI.TryGetValue(uiType, out var uiBase))
        {
            uiBase.InitUI(arg);
            uiBase.gameObject.SetActive(true);
            _liCurrentUI.AddLast(uiBase);
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

                uiBase.InitUI(arg);
                _liCurrentUI.AddLast(uiBase);
                _dicCashingUI.Add(uiType, uiBase);
            });
        }
    }

    public void CloseUI(UIBase uiBase)
    {
        if (uiBase == null)
            return;

        uiBase.Close();
        uiBase.gameObject.SetActive(false);
        _liCurrentUI.RemoveLast();
    }
}

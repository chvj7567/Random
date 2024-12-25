using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class UIAlarmArg : UIArg
{
    public string alarmText;
}

public class UIAlarm : UIBase
{
    UIAlarmArg _arg;

    [SerializeField] TMP_Text _alarmText;

    public override void InitUI(CommonEnum.EUI uiType, UIArg arg)
    {
        base.InitUI(uiType, arg);

        _arg = arg as UIAlarmArg;

        _alarmText.text = _arg.alarmText;
    }
}

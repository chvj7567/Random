using ChvjUnityInfra;
using TMPro;
using UnityEngine;

public class CustomScrollView : CHPoolingScrollView<CustomScrollViewItem, string>
{
    public override void InitItem(CustomScrollViewItem obj, string info, int index)
    {
        obj.Init(info);
    }

    public override void InitPoolingObject(CustomScrollViewItem obj)
    {

    }
}

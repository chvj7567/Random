using TMPro;
using UnityEngine;

public class CustomScrollView : CHScrollView<CustomScrollViewItem, string>
{
    public override void InitItem(CustomScrollViewItem obj, string info, int index)
    {
        obj.Init(info);
    }

    public override void InitPoolingObject(CustomScrollViewItem obj)
    {

    }
}

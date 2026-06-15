using ChvjUnityInfra;
using UnityEngine;

public class RouletteScrollView : CHPoolingScrollView<RouletteScrollViewItem, string>
{
    public override void InitItem(RouletteScrollViewItem item, string data, int index)
    {
        item.Init(data);
    }

    public override void InitPoolingObject(RouletteScrollViewItem item)
    {

    }
}

using UnityEngine;

public class RouletteScrollView : CHScrollView<RouletteScrollViewItem, string>
{
    public override void InitItem(RouletteScrollViewItem item, string data, int index)
    {
        item.Init(data);
    }

    public override void InitPoolingObject(RouletteScrollViewItem item)
    {
        
    }
}

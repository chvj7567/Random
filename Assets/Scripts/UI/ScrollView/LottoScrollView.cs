using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class LottoScrollView : CHScrollView<LottoScrollViewItem, LottoResponse>
{
    public override void InitItem(LottoScrollViewItem obj, LottoResponse info, int index)
    {
        obj.Init(index, info);
    }

    public override void InitPoolingObject(LottoScrollViewItem obj)
    {
        
    }
}

using UnityEngine;

public class UILotto : MonoBehaviour
{
    [SerializeField] private Lotto lotto;
    [SerializeField] private LottoScrollView scrollView;

    public void View()
    {
        scrollView.SetItemList(lotto.LottoResponseList);
    }
}

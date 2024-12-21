using TMPro;
using UnityEngine;

public class CustomScrollViewItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _customText;

    public void Init(string customText)
    {
        _customText.SetText(customText);
    }
}

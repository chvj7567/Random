using TMPro;
using UnityEngine;

public class RouletteScrollViewItem : MonoBehaviour
{
    [SerializeField] TMP_Text _text;

    public void Init(string data)
    {
        _text.text = data;
    }
}

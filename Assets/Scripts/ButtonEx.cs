using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonEx : MonoBehaviour
{
    [SerializeField] TMP_Text _text;

    private void Awake()
    {
        _text = GetComponentInChildren<TMP_Text>();
    }

    public void SetText(string text)
    {
        if (_text == null)
            return;

        _text.text = text;
    }
}

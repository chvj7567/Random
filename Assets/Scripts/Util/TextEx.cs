using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TextEx : MonoBehaviour
{
    [SerializeField] private int _stringID = 1;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private TMP_Text _text;

    private object[] arrArg;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _text = GetComponent<TMP_Text>();

        if (_stringID != -1)
        {
            _text.text = JsonManager.Instance.GetStringData(_stringID);
        }
    }

    public void SetText(params object[] arrArg)
    {
        this.arrArg = arrArg;
        _text.text = string.Format(JsonManager.Instance.GetStringData(_stringID), arrArg);
    }

    public void SetColor(Color _color)
    {
        _text.color = _color;
    }

    public void SetStringID(int _stringID)
    {
        this.arrArg = null;
        this._stringID = _stringID;
        _text.text = JsonManager.Instance.GetStringData(this._stringID);
    }

    public void SetPlusString(string _plusString)
    {
        if (string.IsNullOrEmpty(_plusString) == false)
        {
            _text.text = _text.text + " + " + _plusString;
        }
    }

    public string GetString()
    {
        return _text.text;
    }
}

using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class CustomRandomScene : MonoBehaviour
{
    [SerializeField] private TMP_InputField _customInput;
    [SerializeField] private Button _menuButton;
    [SerializeField] private CustomScrollView scrollView;
    [SerializeField] private Button _plusButton;
    [SerializeField] private Button _minusButton;
    [SerializeField] private ButtonEx _resultButton;

    private ReactiveCollection<string> _liCustomText = new ReactiveCollection<string>();
    private IMainSceneManager _mainSceneManager;

    private void OnEnable()
    {
        _resultButton.SetText("Random");
    }

    private void Start()
    {
        _menuButton.OnClickAsObservable().Subscribe(_ =>
        {
            Close();
        }).AddTo(this);

        _liCustomText.ObserveCountChanged().Subscribe(_ =>
        {
            scrollView.SetItemList(_liCustomText.ToList());
        }).AddTo(this);

        _plusButton.OnClickAsObservable().Subscribe(_ =>
        {
            _liCustomText.Add(_customInput.text);
        }).AddTo(this);

        _minusButton.OnClickAsObservable().Subscribe(_ =>
        {
            if (_liCustomText.Count == 0)
                return;

            _liCustomText.RemoveAt(_liCustomText.Count - 1);
        }).AddTo(this);

        _resultButton.OnClick(() =>
        {
            if (_liCustomText.Count == 0)
            {
                _resultButton.SetText(string.Empty);
            }
            else
            {
                _resultButton.SetText(_liCustomText[Random.Range(0, _liCustomText.Count)]);
            }
        });
    }

    public void SetManager(IMainSceneManager manager)
    {
        _mainSceneManager = manager;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _mainSceneManager.ShowMainScene();
    }
}
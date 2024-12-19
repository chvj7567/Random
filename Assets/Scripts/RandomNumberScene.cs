using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class RandomNumberScene : MonoBehaviour
{
    [SerializeField] private Button _menuButton;
    [SerializeField] private TMP_InputField _startNumberInput;
    [SerializeField] private TMP_InputField _endNumberInput;
    [SerializeField] private ButtonEx _randomButton;

    private IMainSceneManager _mainSceneManager;
    private int _startNumber = 0;
    private int _endNumber = 0;

    private void Start()
    {
        _menuButton.OnClickAsObservable().Subscribe(_ =>
        {
            gameObject.SetActive(false);
            _mainSceneManager.ShowMainScene();
        }).AddTo(this);

        _startNumberInput.onEndEdit.AddListener((str) =>
        {
            var result = CheckInteger(_startNumberInput);
            if (result.Item1)
            {
                _startNumber = result.Item2;
            }
        });

        _endNumberInput.onEndEdit.AddListener((str) =>
        {
            var result = CheckInteger(_endNumberInput);
            if (result.Item1)
            {
                _endNumber = result.Item2;
            }
        });

        _randomButton.OnClick(() =>
        {
            if (_startNumber > _endNumber)
            {
                _randomButton.SetText("숫자 범위 확인");
            }
            else
            {
                _randomButton.SetText($"{Random.Range(_startNumber, _endNumber + 1)}");
            }
        });
    }

    public void SetManager(IMainSceneManager manager)
    {
        _mainSceneManager = manager;
    }

    private (bool, int) CheckInteger(TMP_InputField input)
    {
        if (input == null)
            return (false, 0);

        if (int.TryParse(input.text, out int result) == false)
        {
            if (input.placeholder is TextMeshProUGUI placeholder)
            {
                placeholder.text = "정수 입력...";
            }

            return (false, 0);
        }

        return (true, result);
    }
}

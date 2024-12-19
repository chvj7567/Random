using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class CustomRandomScene : MonoBehaviour
{
    [SerializeField] private Button _menuButton;

    private IMainSceneManager _mainSceneManager;

    private void Start()
    {
        _menuButton.OnClickAsObservable().Subscribe(_ =>
        {
            gameObject.SetActive(false);
            _mainSceneManager.ShowMainScene();
        }).AddTo(this);
    }

    public void SetManager(IMainSceneManager manager)
    {
        _mainSceneManager = manager;
    }
}

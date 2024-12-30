using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    [SerializeField] private SystemLanguage _language;
    [SerializeField] private CommonEnum.EScene _nextSceneType;

    private async void Start()
    {
        //# 매니저들 초기화
        await GameManagement.Instance.InitManager();

#if UNITY_EDITOR
        GameManagement.Instance.Language = _language;
#else
        GameManagement.Language = Application.systemLanguage;
#endif

        switch (_nextSceneType)
        {
            case CommonEnum.EScene.Roulette:
                {
                    SceneManager.LoadScene(1);
                }
                break;
            case CommonEnum.EScene.Lotto:
                {
                    SceneManager.LoadScene(2);
                }
                break;
        }
    }
}

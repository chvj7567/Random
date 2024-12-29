using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    [SerializeField] private CommonEnum.EScene _nextSceneType;

    private async void Start()
    {
        //# 매니저들 초기화
        await GameManagement.InitManager();

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

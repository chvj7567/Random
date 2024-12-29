using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    [SerializeField] private CommonEnum.EScene _nextSceneType;

    private async void Start()
    {
        //# �Ŵ����� �ʱ�ȭ
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

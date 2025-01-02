using System.Threading.Tasks;
using UnityEngine;

public class Capture : MonoBehaviour
{
    [SerializeField] private ButtonEx _captureButton;

    private void Awake()
    {
        _captureButton.OnClick(() =>
        {
            CaptureScreenshot();
        });
    }

    private async Task CaptureScreenshot()
    {
        await Task.Yield();

        string fileName = $"Screenshot_{System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.png";

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();

        NativeGallery.SaveImageToGallery(screenshot, "Random Roulette_Screenshot", fileName, (success, path) =>
        {
            if (success)
            {
                UIManager.Instance.ShowUI(CommonEnum.EUI.UIAlarm, new UIAlarmArg
                {
                    alarmText = $"스크린 샷이 저장되었습니다."
                });
            }
        });

        Destroy(screenshot);
    }
}

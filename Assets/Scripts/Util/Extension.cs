using DG.Tweening;
using System;
using UnityEngine;

public static class Extension
{
    public static void ZSpin(this RectTransform rectTransform,
        Action<float> zValueCallback = null,
        float rotationDuration = 3f,
        int minRotationCount = 3,
        int maxRotationCount = 6,
        float finalRotationMin = 0f,
        float finalRotationMax = 360f)
    {
        int rotationCount = UnityEngine.Random.Range(minRotationCount, maxRotationCount + 1);
        float finalRotation = UnityEngine.Random.Range(finalRotationMin, finalRotationMax);
        float totalRotation = rotationCount * 360f + finalRotation;

        rectTransform.DORotate(new Vector3(0, 0, totalRotation), rotationDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.OutCirc)
            .OnComplete(() =>
            {
                Debug.Log($"{rectTransform.rotation.eulerAngles.z} 회전 완료!");
                zValueCallback?.Invoke(rectTransform.rotation.eulerAngles.z);
            });
    }
}

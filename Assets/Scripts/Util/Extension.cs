using DG.Tweening;
using System;
using UnityEngine;

public static class Extension
{
    /// <summary>
    /// z축 기준 룰렛 회전
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="zValueCallback"></param>
    /// <param name="rotationDuration"></param>
    /// <param name="minRotationCount"></param>
    /// <param name="maxRotationCount"></param>
    /// <param name="finalRotationMin"></param>
    /// <param name="finalRotationMax"></param>
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
                zValueCallback?.Invoke(rectTransform.rotation.eulerAngles.z);
            });
    }

    /// <summary>
    /// 중심 위치에서 특정 각도만큼 회전한 위치로 이동
    /// </summary>
    /// <param name="transform">이동할 Transform</param>
    /// <param name="baseTransform">중심 위치</param>
    /// <param name="distance">중심 위치에서 떨어진 거리</param>
    /// <param name="angle">이동할 각도</param>
    public static void RotateXYPosition(this Transform transform, Transform baseTransform, float distance, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;

        Vector3 basePos = baseTransform.position;

        float x = basePos.x + distance * Mathf.Cos(radian);
        float y = basePos.y + distance * Mathf.Sin(radian);

        transform.position = new Vector3(x, y, basePos.z);
    }

    /// <summary>
    /// 중심 위치에서 특정 각도만큼 회전
    /// </summary>
    /// <param name="transform">중심 위치</param>
    /// <param name="angle">회전할 각도</param>
    public static void RotateZRoation(this Transform transform, float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}

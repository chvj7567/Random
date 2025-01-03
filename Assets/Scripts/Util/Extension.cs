using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public static class Extension
{
    /// <summary>
    /// z축 회전
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="zValueCallback"></param>
    /// <param name="rotationDuration"></param>
    /// <param name="minRotationCount"></param>
    /// <param name="maxRotationCount"></param>
    /// <param name="finalRotationMin"></param>
    /// <param name="finalRotationMax"></param>
    public static void Spin(this RectTransform rectTransform,
        Action<float> zValueCallback = null,
        int rotationCount = 1,
        float rotationDuration = 1f,
        float finalRotationMin = 0f,
        float finalRotationMax = 360f)
    {
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
    /// 중심 위치에서 특정 각도만큼 회전
    /// </summary>
    /// <param name="transform">중심 위치</param>
    /// <param name="angle">회전할 각도</param>
    public static void RotateZRoation(this Transform transform, float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// 중심 위치에서 특정 각도만큼 회전한 위치로 이동
    /// </summary>
    /// <param name="rectTransform">이동할 RectTransform</param>
    /// <param name="baseRectTransform">중심 위치</param>
    /// <param name="distance">중심 위치에서 떨어진 거리</param>
    /// <param name="angle">이동할 각도</param>
    public static void RotateXYPosition(this RectTransform rectTransform, RectTransform baseRectTransform, float distance, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;

        Vector2 basePos = baseRectTransform.anchoredPosition;

        float x = basePos.x + distance * Mathf.Cos(radian);
        float y = basePos.y + distance * Mathf.Sin(radian);

        rectTransform.anchoredPosition = new Vector2(x, y);
    }
}

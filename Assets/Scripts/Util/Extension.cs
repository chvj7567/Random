using DG.Tweening;
using System;
using UnityEngine;

public static class Extension
{
    /// <summary>
    /// z�� ���� �귿 ȸ��
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
    /// �߽� ��ġ���� Ư�� ������ŭ ȸ���� ��ġ�� �̵�
    /// </summary>
    /// <param name="transform">�̵��� Transform</param>
    /// <param name="baseTransform">�߽� ��ġ</param>
    /// <param name="distance">�߽� ��ġ���� ������ �Ÿ�</param>
    /// <param name="angle">�̵��� ����</param>
    public static void RotateXYPosition(this Transform transform, Transform baseTransform, float distance, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;

        Vector3 basePos = baseTransform.position;

        float x = basePos.x + distance * Mathf.Cos(radian);
        float y = basePos.y + distance * Mathf.Sin(radian);

        transform.position = new Vector3(x, y, basePos.z);
    }

    /// <summary>
    /// �߽� ��ġ���� Ư�� ������ŭ ȸ��
    /// </summary>
    /// <param name="transform">�߽� ��ġ</param>
    /// <param name="angle">ȸ���� ����</param>
    public static void RotateZRoation(this Transform transform, float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}

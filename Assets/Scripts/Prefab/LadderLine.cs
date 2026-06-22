using ChvjUnityInfra;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

//# 사다리 선(세로/가로) 풀링 셀 — Image 를 캡슐화하고 색/alpha 의도 API 만 노출.
//# base 라인·강조 라인이 같은 풀을 공유하므로 OnEnable 에서 prefab 원본 색으로 리셋해 색 누수 차단 (Rule 03 §4).
public class LadderLine : MonoBehaviour
{
    [SerializeField] private Image _image;

    //# prefab 에 저작된 원본 색 — Awake 1회 캡처. base 라인은 원래 코드에서 색을 칠하지 않아 이 값이 곧 원래 외형.
    private Color _baseColor;

    private void Awake()
    {
        if (_image == null)
            return;

        _baseColor = _image.color;
    }

    private void OnEnable()
    {
        //# 풀 재사용 진입마다 원본 색으로 복구 — 강조 라인이 칠한 색/잔존 alpha 누수 차단.
        if (_image == null)
            return;

        _image.color = _baseColor;
    }

    //# 강조 색 적용 — alpha 는 인자 그대로 반영(강조 라인은 0 으로 시작해 페이드인).
    public void SetColor(Color color)
    {
        if (_image == null)
            return;

        _image.color = color;
    }

    public void SetAlpha(float alpha)
    {
        if (_image == null)
            return;

        Color color = _image.color;
        color.a = alpha;
        _image.color = color;
    }

    //# DOFade Tweener 반환 — 호출부 시퀀스에 Append. _image 는 캡슐화 유지(Rule 02 §6.1).
    public Tweener FadeIn(float duration)
    {
        if (_image == null)
            return null;

        return _image.DOFade(1f, duration);
    }
}

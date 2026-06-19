using ChvjUnityInfra;
using UnityEngine;
using UnityEngine.UI;

//# 메뉴 카드 셀 (Rule 03 §3 3-class 중 Item). 아이콘 + 제목/설명 CHText + CHButton.
//# 풀 재사용 — 클릭 핸들러는 SetupClick 1회 등록, 표시는 Bind 매 재사용 갱신.
public class MenuCardCell : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private CHText _title;
    [SerializeField] private CHText _desc;
    [SerializeField] private CHButton _button;

    //# 현재 셀이 표시 중인 데이터. 클릭 람다가 이 값을 읽어 진입(stale 캡처 방지).
    private MenuCardData _data;
    private IRandomSceneAccess _access;
    private bool _clickBound;

    private void OnEnable()
    {
        //# 풀 재사용 시 이전 셀의 아이콘 잔존 방지 (Rule 03 §4). 텍스트는 Bind 가 항상 덮어씀.
        if (_icon != null)
            _icon.enabled = false;
    }

    //# 클릭 핸들러 셀당 1회만 등록. CHButton.OnClick 은 RemoveListener 가 없어 매번 등록 시 누적되므로
    //# _clickBound 가드로 1회만 등록하고, 클릭 시점에 _data 를 읽어 진입한다(풀 재사용 안전).
    //# InitItem 경유로 호출 — origin 셀은 InitPoolingObject 를 받지 않으므로 InitItem 에서 등록해야 누락이 없다.
    public void SetupClick(IRandomSceneAccess access)
    {
        _access = access;

        if (_clickBound)
            return;

        if (_button == null)
            return;

        _clickBound = true;

        _button.OnClick(() =>
        {
            if (_data == null)
                return;

            _access?.ShowScene(_data.menu);
        });
    }

    //# 표시 갱신 — 풀 재사용마다 호출. 아이콘은 비동기 로드.
    public void Bind(MenuCardData data)
    {
        _data = data;

        if (_title != null)
            _title.SetText(JsonManager.Instance.GetStringData(data.titleStringID));

        if (_desc != null)
            _desc.SetText(JsonManager.Instance.GetStringData(data.descStringID));

        LoadIcon(data.iconKey);
    }

    private void LoadIcon(CommonEnum.EMenuIcon iconKey)
    {
        //# 콜백 도착 시점 셀이 다른 데이터로 재바인딩됐을 수 있어, 도착 시 키 재확인 후 반영.
        CHMResource.Instance.Load<Sprite>(iconKey, sprite =>
        {
            if (_icon == null)
                return;

            if (_data == null || _data.iconKey != iconKey)
                return;

            if (sprite == null)
            {
                _icon.enabled = false;
                return;
            }

            _icon.sprite = sprite;
            _icon.enabled = true;
        });
    }
}

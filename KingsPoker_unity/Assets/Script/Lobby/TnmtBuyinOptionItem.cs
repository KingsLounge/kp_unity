using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 토너먼트 신청 팝업의 바이인 옵션 한 줄. TnmtApplyPopup 이 옵션 수만큼 복제해서 채운다.
/// 한 줄 = 결제 방법 하나 ("2칩", "2칩 + 2KP", "데일리 시드권 1장" …).
/// 필드는 toggle 만 필수이고 나머지는 연결 안 해도 된다.
/// </summary>
public class TnmtBuyinOptionItem : MonoBehaviour
{
    [SerializeField]
    private Toggle toggle; // 선택 토글 (ToggleGroup 은 팝업이 넣어 준다)

    [SerializeField]
    private Text priceText; // 가격: "2칩 + 2KP"

    [SerializeField]
    private Text holdText; // 보유: "보유 15칩 · 340KP" (모자란 재화는 색으로 표시 — Rich Text 켜 둘 것)

    [SerializeField]
    private CanvasGroup canvasGroup; // 살 수 없는 옵션을 흐리게 (미연결 시 생략)

    [SerializeField]
    private GameObject disabledMark; // 살 수 없을 때만 켜는 표시 (미연결 시 생략)

    [SerializeField]
    private float disabledAlpha = 0.45f;

    private Action onSelected;
    private bool listening = false;

    public bool Affordable { get; private set; }

    public void Set(string price, string hold, bool affordable, ToggleGroup group, Action onSelected)
    {
        this.onSelected = onSelected;
        Affordable = affordable;

        if (priceText != null)
        {
            priceText.text = price;
        }
        if (holdText != null)
        {
            holdText.text = hold;
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = affordable ? 1f : disabledAlpha;
        }
        if (disabledMark != null)
        {
            disabledMark.SetActive(!affordable);
        }
        if (toggle != null)
        {
            toggle.group = group;
            toggle.interactable = affordable;
            if (!listening)
            {
                toggle.onValueChanged.AddListener(OnToggleChanged);
                listening = true;
            }
        }
    }

    public void SetSelected(bool selected)
    {
        if (toggle != null)
        {
            toggle.SetIsOnWithoutNotify(selected);
        }
    }

    private void OnToggleChanged(bool on)
    {
        if (on)
        {
            onSelected?.Invoke();
        }
    }
}

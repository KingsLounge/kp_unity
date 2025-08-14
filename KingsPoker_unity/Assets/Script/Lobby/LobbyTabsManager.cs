using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LobbyTabsManager : MonoBehaviour
{
    private static LobbyTabsManager instance;
    public static LobbyTabsManager Instance
    {
        get { return instance; }
    }

    public enum LobbyTabEnum
    {
        CafeList = 3,
        GameTableList = 0,
        Shop = 5,
        Profile = 6
    }

    private int topIdx = 0;
    private int bottomIdx = 0;

    [SerializeField]
    private LobbyTabEnum defaltTabIdx = LobbyTabEnum.CafeList;
    private int cafeTabIdx = 0;
    private int popupIdx = 0;
    public int tabType { get; private set; } = 0;
    private string currentCafeKey;
    public Toggle cafeGameTab;

    [Header("InactiveParents")]
    [SerializeField]
    private Transform topInactiveParent;

    [SerializeField]
    private Transform defaltTabInactiveParent;

    [SerializeField]
    private Transform bottomInactiveParent;

    [Header("activeParents")]
    [SerializeField]
    private Transform topActiveParent;

    [SerializeField]
    private Transform defaltTabActiveParent;

    [SerializeField]
    private Transform bottomActiveParent;

    public string defaltKey = "defaltCafe";
    public CafeTab defaltCafe;

    private string cafeKey;

    [SerializeField]
    List<Transform> topList;

    [SerializeField]
    List<Transform> bottomList;

    [SerializeField]
    List<Transform> defaltTabList;

    [SerializeField]
    List<Transform> popupTabList;

    //[SerializeField] List<Transform> cafeTabList;
    [SerializeField]
    Dictionary<string, CafeTab> cafesDic;

    [SerializeField]
    private Button cafeLeaveButton;

    [SerializeField]
    private Button cafeTabBackButton;

    private void Awake()
    {
        cafesDic = new Dictionary<string, CafeTab>();
        cafesDic.Add(defaltKey, defaltCafe);
        cafeKey = defaltKey;
        SoundManager.Instance.PlayBGM(Sound_TableEnum.BGM_MAINLOBBY);
        Init();
    }

    void Start() { }

    private void Init()
    {
        instance = this;
        TopIdxSet(0);
        BottomIdxSet(0);
        DefaltTabIdxSet(defaltTabIdx);
    }

    public void TabReset()
    {
        switch (tabType)
        {
            case 0:
                defaltTabList[(int)defaltTabIdx].GetComponent<LobbyContent>()?.InitContent();
                break;
            case 1:
                Cafe.instance.GetCafeInfo((int)Cafe.instance.curEnterCafeInfo["cafe"]["idx"]);
                break;
            case 2:
                popupTabList[(int)popupIdx].GetComponent<LobbyContent>()?.InitContent();
                break;
        }
    }

    public void TopIdxSet(int idx)
    {
        var befortr = topList[topIdx] as RectTransform;
        var tr = topList[idx] as RectTransform;
        tr.SetParent(topActiveParent);
        var tabRect = defaltTabActiveParent as RectTransform;
        var offsetMax = tabRect.offsetMax;
        offsetMax.y = -tr.rect.height;
        tabRect.offsetMax = offsetMax;

        if (idx == topIdx)
        {
            tr.localPosition = Vector2.zero;
        }
        else if (idx > topIdx)
        {
            Vector2 start = new Vector2(tr.rect.width + 50, 0);
            DOTween
                .To(
                    () => start,
                    (v) =>
                    {
                        tr.localPosition = v;
                        befortr.localPosition = v - start;
                    },
                    Vector2.zero,
                    0.3f
                )
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    befortr.SetParent(topInactiveParent);
                });
        }
        else
        {
            Vector2 start = new Vector2(-tr.rect.width - 50, 0);
            DOTween
                .To(
                    () => start,
                    (v) =>
                    {
                        tr.localPosition = v;
                        befortr.localPosition = v - start;
                    },
                    Vector2.zero,
                    0.3f
                )
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    befortr.SetParent(topInactiveParent);
                });
        }
        topIdx = idx;
    }

    public void BottomIdxSet(int idx)
    {
        var befortr = bottomList[bottomIdx] as RectTransform;
        var tr = bottomList[idx] as RectTransform;
        tr.SetParent(bottomActiveParent);
        var tabRect = defaltTabActiveParent as RectTransform;
        var offsetMin = tabRect.offsetMin;
        offsetMin.y = tr.rect.height;
        tabRect.offsetMin = offsetMin;

        if (idx == bottomIdx)
        {
            tr.localPosition = Vector2.zero;
        }
        else if (idx > bottomIdx)
        {
            Vector2 start = new Vector2(tr.rect.width, 0);
            DOTween
                .To(
                    () => start,
                    (v) =>
                    {
                        tr.localPosition = v;
                        befortr.localPosition = v - start;
                    },
                    Vector2.zero,
                    0.3f
                )
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    befortr.SetParent(bottomInactiveParent);
                });
        }
        else
        {
            Vector2 start = new Vector2(-tr.rect.width, 0);
            DOTween
                .To(
                    () => start,
                    (v) =>
                    {
                        tr.localPosition = v;
                        befortr.localPosition = v - start;
                    },
                    Vector2.zero,
                    0.3f
                )
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    befortr.SetParent(bottomInactiveParent);
                });
        }
        bottomIdx = idx;
    }

    public void DefaltTabIdxSet(int idx)
    {
        if (tabType != 0)
        {
            TabTypeSet(0);
        }
        DefaltTabIdxSet((LobbyTabEnum)idx);
    }

    public void PopupTabIdxSet(int idx)
    {
        if (tabType != 2)
        {
            TabTypeSet(2);
        }

        PopupIdxSetting(idx);
    }

    public void DefaltTabIdxSet(LobbyTabEnum idx)
    {
        var befortr = defaltTabList[(int)defaltTabIdx] as RectTransform;
        var tr = defaltTabList[(int)idx] as RectTransform;
        tr.SetParent(defaltTabActiveParent);
        tr.localScale = Vector2.one;

        if (idx == defaltTabIdx)
        {
            tr.offsetMax = Vector2.zero;
            tr.offsetMin = Vector2.zero;
        }
        else
        {
            Vector2 start;
            if ((int)idx > (int)defaltTabIdx)
            {
                start = new Vector2(tr.rect.width + 50, 0);
            }
            else
            {
                start = new Vector2(-tr.rect.width - 50, 0);
            }

            ScreenGuard.instance.Activate();

            DOTween
                .To(
                    () => start,
                    (v) =>
                    {
                        tr.offsetMin = v;
                        tr.offsetMax = v;
                        befortr.offsetMin = v - start;
                        befortr.offsetMax = v - start;
                    },
                    Vector2.zero,
                    0.3f
                )
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    befortr.SetParent(defaltTabInactiveParent);
                    ScreenGuard.instance.Inactivate();
                });
        }

        var lobbyContnent = tr.GetComponent<LobbyContent>();

        if (lobbyContnent)
        {
            lobbyContnent.InitContent();
            var top = topList[topIdx];
            top.GetComponent<LobbyTop>()?.SetTopName(lobbyContnent.ContentName);
        }

        defaltTabIdx = idx;

        // 기본

        Packet p;
        switch (idx)
        {
            case LobbyTabEnum.GameTableList: // game table list

            case LobbyTabEnum.CafeList: // cafes

                break;
            case LobbyTabEnum.Shop: // shop
                break;
            case LobbyTabEnum.Profile: // game tables
                break;
        }
    }

    public void PopupIdxSetting(int idx)
    {
        var befortr = popupTabList[(int)popupIdx] as RectTransform;
        var tr = popupTabList[(int)idx] as RectTransform;
        tr.SetParent(defaltTabActiveParent);
        tr.localScale = Vector2.one;

        if (idx == popupIdx)
        {
            tr.offsetMax = Vector2.zero;
            tr.offsetMin = Vector2.zero;
        }
        else
        {
            Vector2 start;
            if ((int)idx > (int)popupIdx)
            {
                start = new Vector2(tr.rect.width, 0);
            }
            else
            {
                start = new Vector2(-tr.rect.width, 0);
            }

            ScreenGuard.instance.Activate();

            DOTween
                .To(
                    () => start,
                    (v) =>
                    {
                        tr.offsetMin = v;
                        tr.offsetMax = v;
                        befortr.offsetMin = v - start;
                        befortr.offsetMax = v - start;
                    },
                    Vector2.zero,
                    0.3f
                )
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    befortr.SetParent(defaltTabInactiveParent);
                    ScreenGuard.instance.Inactivate();
                });
        }

        var lobbyContnent = tr.GetComponent<LobbyContent>();

        if (lobbyContnent)
        {
            lobbyContnent.InitContent();
            if (topList != null)
            {
                var top = topList[topIdx];
                top.GetComponent<LobbyTop>()?.SetTopName(lobbyContnent.ContentName);
            }
        }
        popupIdx = idx;
    }

    public void LobbyDisable()
    {
        var pos = transform.localPosition;
        pos.y = 10000;
        transform.localPosition = pos;
        SoundManager.Instance.StopBGM();
        //DefaltTabIdxSet(defaltTabIdx);
    }

    public void LobbyEnable(float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
    {
        SoundManager.Instance.PlayBGM(Sound_TableEnum.BGM_MAINLOBBY);
        transform.localPosition = Vector3.zero;
        (transform as RectTransform).offsetMin = new Vector2(left, bottom);
        (transform as RectTransform).offsetMax = new Vector2(-right, -top);
        //Init();
    }

    public void CafeTabIdxSet(int idx)
    {
        var cafe = cafesDic[cafeKey];
        var befortr = cafe.Tabs[cafeTabIdx] as RectTransform;

        cafe = cafesDic[cafeKey];
        var tr = cafe.Tabs[idx] as RectTransform;
        tr.SetParent(defaltTabActiveParent);

        cafeTabBackButton.gameObject.SetActive(idx != 0);
        cafeLeaveButton.gameObject.SetActive(idx == 0);

        if (idx == cafeTabIdx)
        {
            tr.offsetMax = Vector2.zero;
            tr.offsetMin = Vector2.zero;
        }
        else if (idx > cafeTabIdx)
        {
            ScreenGuard.instance.Activate();
            Vector2 start = new Vector2(tr.rect.width, 0);
            DOTween
                .To(
                    () => start,
                    (v) =>
                    {
                        tr.offsetMin = v;
                        tr.offsetMax = v;
                        befortr.offsetMin = v - start;
                        befortr.offsetMax = v - start;
                    },
                    Vector2.zero,
                    0.3f
                )
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    befortr.SetParent(cafe.transform);
                    ScreenGuard.instance.Inactivate();
                });
        }
        else
        {
            ScreenGuard.instance.Activate();
            Vector2 start = new Vector2(-tr.rect.width, 0);
            DOTween
                .To(
                    () => start,
                    (v) =>
                    {
                        tr.offsetMin = v;
                        tr.offsetMax = v;
                        befortr.offsetMin = v - start;
                        befortr.offsetMax = v - start;
                    },
                    Vector2.zero,
                    0.3f
                )
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    befortr.SetParent(cafe.transform);
                    ScreenGuard.instance.Inactivate();
                });
        }

        cafeTabIdx = idx;

        //  카페 관련

        // TODO: 각 항목 update 패킷을 보낸다.


        switch (idx)
        {
            case 0: // game tables
                break;
            case 1: // game tables
                break;
            case 2: // game tables
                break;
            case 3: // game tables
                break;
            case 4:
                break;
        }
    }

    public void CafeSet(string key)
    {
        cafeKey = key;
        cafeTabIdx = 0;

        TabTypeSet(1);
    }

    public void PlaySet(string key)
    {
        cafeKey = key;
        cafeTabIdx = 0;

        TabTypeSet(1);
    }

    public void TabTypeSet(int type)
    {
        TopIdxSet(type);
        BottomIdxSet(type);
        RectTransform befortr = null;
        switch (tabType)
        {
            case 0:
                befortr = defaltTabList[(int)defaltTabIdx] as RectTransform;
                break;
            case 1:
                var cafe = cafesDic[cafeKey];
                befortr = cafe.Tabs[cafeTabIdx] as RectTransform;
                break;
            case 2:
                befortr = popupTabList[popupIdx] as RectTransform;
                break;
        }

        RectTransform tr = null;
        switch (type)
        {
            case 0:
                tr = defaltTabList[(int)defaltTabIdx] as RectTransform;
                tr.SetParent(defaltTabActiveParent);
                break;
            case 1:
                cafeGameTab.isOn = true;
                cafeTabIdx = 0;
                var cafe = cafesDic[cafeKey];
                tr = cafe.Tabs[cafeTabIdx] as RectTransform;
                tr.SetParent(defaltTabActiveParent);
                break;
            case 2:
                tr = popupTabList[popupIdx] as RectTransform;
                break;
        }

        if (type == tabType)
        {
            tr.offsetMax = Vector2.zero;
            tr.offsetMin = Vector2.zero;
        }
        else
        {
            Vector2 start;
            if (type > tabType)
            {
                start = new Vector2(tr.rect.width, 0);
            }
            else
            {
                start = new Vector2(-tr.rect.width, 0);
            }
            ScreenGuard.instance.Activate();

            DOTween
                .To(
                    () => start,
                    (v) =>
                    {
                        tr.offsetMin = v;
                        tr.offsetMax = v;
                        befortr.offsetMin = v - start;
                        befortr.offsetMax = v - start;
                    },
                    Vector2.zero,
                    0.3f
                )
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    if (tabType == 0)
                    {
                        befortr.SetParent(defaltTabInactiveParent);
                    }
                    else if (tabType == 1)
                    {
                        var cafe = cafesDic[cafeKey];
                        befortr.SetParent(cafe.transform);
                    }
                    befortr.position = Vector3.up * 50000;
                    ScreenGuard.instance.Inactivate();
                });
        }

        tabType = type;
    }
}

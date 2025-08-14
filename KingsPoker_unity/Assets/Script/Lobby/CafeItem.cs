using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CafeItem : MonoBehaviour
{
    public CafeLogoData logoList;
    public Cafe manager;
    public CafeInfo cafe;
    public Image cafe_logo;
    public Text txt_cafeIdx;
    public Text txt_title;
    public Image icon_cafe_stop;
    public Text txt_pw;
    public Text txt_zuice;
    public Text txt_tableNo;
    public Text txt_memberNo;
    public GameObject noticeIcon;
    public List<Variation> tierVariation = new List<Variation>();
    public List<Variation> permitVariation = new List<Variation>();

    public Text txt_cafe_my_chip; // 이 카페의 나의 칩 
    public Text txt_cafe_my_borrowed_chip; // 이 카페의 나의 (빌린)칩 

    private void Awake()
    {
    }
    private void OnDestroy()
    {
        // LobbyManager.Instance.roomUpdate.RemoveListener(UpdateCanJoin);
    }
    //public virtual void UpdateCanJoin()
    //{
    //    if (gameType == GAME_TYPE.nlh || gameType == GAME_TYPE.mtt)
    //    {
    //        SetEnable(emn <= MyStatus.gc);
    //    }
    //    else
    //    {
    //        SetEnable(emn <= MyStatus.sc); 

    //    }
    //}

    public async void Set(CafeInfo data)
    {
        if (data == null)
        {
            return;
        }

        cafe = data;
        int icon = (int)cafe.info["icon"];
        if(icon == 0)
        {
            string url = "";
            if (cafe.info.ContainsKey("photoUrl"))
                url = cafe.info["photoUrl"].ToString();
            else if(cafe.info.ContainsKey("photourl"))
                url = cafe.info["photourl"].ToString();

            if (!string.IsNullOrEmpty(url))
                cafe_logo.sprite = (await ImageDatabase.LoadImageTexture(url, Application.persistentDataPath + "/cafeLogo", cafe.info["idx"].ToString())).ToSprite();
            else
                cafe_logo.sprite = null;
        }
        else
        {
            cafe_logo.sprite = logoList.sprites[icon - 1];
        }
        txt_cafeIdx.text = "#" + cafe.info["idx"].ToString();
        txt_title.text = cafe.info["name"].ToString();
        icon_cafe_stop.gameObject.SetActive((long)cafe.info["leftSeconds"] <= 0);  // 시간이 모두 0이 되면 '시계+스톱' icon이 활성화된다.  Alberto 2021-04-25

        txt_pw.text = cafe.info["pw"].ToString();
        txt_zuice.text = cafe.info["zc"].ToString();
        int tier = (int)cafe.info["tier"];
        txt_tableNo.text = cafe.info["gameCount"].ToString() + "/" + manager.cafeTierData[tier - (int)CAFE_TIER.bronze, (int)CAFE_TIER_OPTION.table_max];
        txt_memberNo.text = cafe.info["memberCount"].ToString() + "/" + manager.cafeTierData[tier - (int)CAFE_TIER.bronze, (int)CAFE_TIER_OPTION.member_max];


        TierSetting((CAFE_TIER)tier);

        if (cafe.cafeMembers.Count > 0)
        {
            long dc = cafe.cafeMembers[0].dc;
            long debt= cafe.cafeMembers[0].debt;
            CAFE_MEMBER_PERMIT permit = (CAFE_MEMBER_PERMIT)cafe.cafeMembers[0].permit;

            txt_cafe_my_chip.text = MoneyToString.Converting(dc);
            txt_cafe_my_borrowed_chip.text = debt.ToString();

            PermitSetting(permit);

            bool bPermit = permit == CAFE_MEMBER_PERMIT.manager || permit == CAFE_MEMBER_PERMIT.owner;
            int total_notice_count = cafe.info.ValueOrDefault("joinReqCount", 0) + cafe.info.ValueOrDefault("orderReqCount", 0);
            noticeIcon.SetActive(bPermit && total_notice_count > 0);
        }
    }

    private void TierSetting(CAFE_TIER tier)
    {
        for (int i = 0; i < tierVariation.Count; i++)
            tierVariation[i].SetVariation(tier.ToString());
    }

    private void PermitSetting(CAFE_MEMBER_PERMIT permit)
    {
        for (int i = 0; i < permitVariation.Count; i++)
            permitVariation[i].SetVariation(permit.ToString());
    }


    //public virtual void SetEnable(bool en)
    //{
    //    GetComponent<Button>().interactable = en;
    //}
    public void OnClickEnterButton()
    {
        Cafe.instance.EnterCafe(cafe.info.ValueOrDefault("idx",0));
    }
   
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Newtonsoft.Json.Linq;

enum LabelType
{
    Text,
    SpriteFont
}

public class RoomInfoItem : MonoBehaviour
{
    public GameObject spriteFontContainer = null;
    public Text userCountText;
    //public SpriteFontData spriteFont = null;
    public Text blindText = null;
    public Text buyinText = null;
    public GAME_TYPE gameType;
    public List<Variation> chipTypeVariations;

    public Text gameTypeText;
    public Text channelNoText;
    public SpriteFontParent spriteParent;
    protected RoomData roomData;

    private void Awake()
    {
        LobbyManager.Instance.roomUpdate.AddListener(UpdateCanJoin);
        InfoManager.Instance.ZcCountChangeAddLisener(UserCountUpdate);
        LocalizeManager.AddEvent(UpdateLang);
    }
    private void OnDestroy()
    {
        LobbyManager.Instance.roomUpdate.RemoveListener(UpdateCanJoin);
        InfoManager.Instance.ZcCountChangeRemoveLisener(UserCountUpdate);
        LocalizeManager.RemoveEvent(UpdateLang);
    }

    public virtual void UpdateLang()
    {
        Setting();
    }

    public void UserCountUpdate()
    {
        var userCountarr = InfoManager.Instance.ZcCountArr;
        var count = 0;
        for(int i = 0; i < userCountarr.Count; ++i)
        {
            JObject obj = userCountarr[i] as JObject;
            if (obj.ValueOrDefault("option_level", 0) == roomData.level)
            {
                count = obj.ValueOrDefault("user_count", 0);
                break;
            }
        }
        userCountText.text = count.ToString();
    }
    public virtual void UpdateCanJoin()
    {
        switch(roomData.chip_type)
        {
            case CHIP_TYPE.cc:
                SetEnable(roomData.emn <= MyStatus.cc);
                break;
            case CHIP_TYPE.dc:
                SetEnable(roomData.emn <= MyStatus.dc);
                break;
            case CHIP_TYPE.zc:
                SetEnable(roomData.emn <= MyStatus.zc);
                break;

        }
    }
    public virtual void Setting(RoomData rd, GAME_TYPE gameType)
    {
        roomData = rd;
        this.gameType = gameType;
        Setting();
    }

    public virtual void Setting()
    {
        //List<Sprite> sprites = spriteFont.ConvertSpriteFont(spriteFontText);
        var tr = spriteFontContainer.transform;

        UpdateCanJoin();
        //spriteFontContainer.transform.DestroyChildren();
        var anteString = roomData.ante > 0 ? $"({MoneyToString.Converting(roomData.ante)})" : "";
        string roleText = $"{MoneyToString.Converting(roomData.sb)}/{MoneyToString.Converting(roomData.bb)}{anteString}";
        if (blindText)
            blindText.text = roleText;

        buyinText.text = MoneyToString.Converting(roomData.emn);

        foreach(var vari in chipTypeVariations)
        {
            vari.SetVariation(roomData.chip_type.ToString());
        }

        gameTypeText.text = roomData.gameType.ToString().ToUpper();
        channelNoText.text = $"#{roomData.level}";

        spriteParent?.SetSpriteFont(roleText);
    }
    //public virtual void Setting(long bng, long emn, long sb, long bb, GAME_TYPE gameType)
    //{
    //    this.gameType = gameType;
    //    this.bng = bng;
    //    this.emn = emn;

    //    string spriteFontText = MoneyToString.Converting(sb) + '/' + MoneyToString.Converting(bb);
    //    //List<Sprite> sprites = spriteFont.ConvertSpriteFont(spriteFontText);
    //    var tr = spriteFontContainer.transform;

    //    UpdateCanJoin();
    //    //spriteFontContainer.transform.DestroyChildren();

    //    buyinText.text = MoneyToString.Converting(emn);
        
    //    spriteParent?.SetSpriteFont(spriteFontText);
    //}

    public virtual void SetEnable(bool en)
    {
        GetComponentInChildren<Button>().interactable = en;
    }
    public void OnClickEnterButton()
    {
        LoadingCircle.Instance.StartSpin(CPProtocol.CP_ROOM_ENTER);
        WebSocketManager.defaultCli.Send("{\"p\":103,\"c\":{\"gtn\":0,\"game_type\":\"" + gameType + "\",\"blind\":" + roomData.ante + "}}");
    }
    public void OnClickPlayGameEnterButton()
    {
        LoadingCircle.Instance.StartSpin(CPProtocol.CP_PLAY_GAME_ENTER);
        var p = new Packet(CPProtocol.CP_PLAY_GAME_ENTER);
        p.Add("gtn",0);
        p.Add("chip_type", (int)roomData.chip_type);
        p.Add("option_level", roomData.level);
        WebSocketManager.defaultCli.Send(p);
    }

    [SerializeField]
    public struct ChipIconData
    {
        public CHIP_TYPE chipType;
        public Sprite icon;
    }
}

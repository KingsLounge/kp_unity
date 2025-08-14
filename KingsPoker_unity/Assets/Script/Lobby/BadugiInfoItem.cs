using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BadugiInfoItem : MonoBehaviour
{
    public GameObject spriteFontContainer = null;
    public long bng = 0;
    public SpriteFontData spriteFont = null;
    public Text buyinText = null;
    public GAME_TYPE gameType;
    public List<SpriteFontPrefab> spriteFontScripts = new List<SpriteFontPrefab>();
    public long emn = 0;
    public SpriteFontParent spriteParent;
    public Material effectMaterial;
    public Image effectImage;
    public GameObject disableImage;
    public float effectBetweenOfset = 0.32f;
    public List<ColorSet> colorSetList;
    private void Awake()
    {
        LobbyManager.Instance.roomUpdate.AddListener(UpdateCanJoin);
    }
    private void OnDestroy()
    {
        LobbyManager.Instance.roomUpdate.RemoveListener(UpdateCanJoin);
    }
    public virtual void UpdateCanJoin()
    {
        if (gameType == GAME_TYPE.nlh || gameType == GAME_TYPE.mtt)
        {
            SetEnable(emn <= MyStatus.dc);
        }
        else
        {
            SetEnable(emn <= MyStatus.zc);

        }
    }

    
    public virtual void Setting(long bng, long emn, long sb, long bb, GAME_TYPE gameType)
    {
        this.gameType = gameType;
        this.bng = bng;
        this.emn = emn;

        string spriteFontText = MoneyToString.Converting(emn);
        List<Sprite> sprites = spriteFont.ConvertSpriteFont(spriteFontText);
        var tr = spriteFontContainer.transform;
        UpdateCanJoin();
        //spriteFontContainer.transform.DestroyChildren();

        buyinText.text = string.Format( "{0} {1}", LocalizeManager.GetLocalString("lobby_badugi_channel_seed"),MoneyToString.Converting(bng));
        spriteParent.SetSpriteFont(spriteFontText);
        SetEffectImage();
    }
    public void SetEffectImage()
    {
        var parent = transform.parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            if (transform == parent.GetChild(i))
            {
                var material = new Material(effectMaterial);
                effectImage.material = material;
                material.SetFloat("_InitUOffset2", effectBetweenOfset * i);
                Color co = material.GetColor("_TintColor");
                for (int j = 0; j < colorSetList.Count; j++)
                {
                    if (colorSetList[j].emn < emn)
                    {
                        co = colorSetList[j].color;
                    }
                    else
                    {
                        break;
                    }
                }
                material.SetColor("_TintColor", co);
                break;
            }
        }//_TintColor

    }

    public void SetEnable(bool en)
    {
        GetComponent<Button>().interactable = en;
        effectImage.enabled = en;
        disableImage.SetActive(!en);
    }

    public void OnClickEnterButton()
    {
        LoadingCircle.Instance.StartSpin(CPProtocol.CP_ROOM_ENTER);
        WebSocketManager.defaultCli.Send("{\"p\":103,\"c\":{\"gtn\":0,\"game_type\":\"" + gameType + "\",\"blind\":" + bng + "}}");
    }

}


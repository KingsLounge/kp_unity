using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class MiniGame : MonoBehaviour
{
    [SerializeField]
    private List<MinigameSlot> slots;

    public float speed = 100f;
    public float time = 5f;
    public float intabal = 200f;
    public float resultPos = -182f;
    public float bottomLine = -510f;
    public float defoltBottomLine = -400f;
    public float currBottom;
    private float timer = 0;
    private bool spin = false;
    private long reward = 0;
    private long bonusRatio = 0;
    private long bonus = 0;
    private DateTime next;
    private JObject update = null;
    private bool result = false;
    private int resultIdx = 0;
    
    [SerializeField]
    float startDelayTime = 0.5f;
    [SerializeField]
    SpriteFontData spritefont = null;
    [SerializeField]
    SpriteFontParent spritefontParent;
    [SerializeField]
    Text rewardText;
    [SerializeField]
    Text bonusRatioText;
    [SerializeField]
    Text bonusText;
    [SerializeField]
    GameObject resultPanel;

    public List<SpriteFontPrefab> spritefonts = new List<SpriteFontPrefab>();

    public List<int> chipData;
    [Header("Test")]
    public bool test = false;
    public int testResult = 100000;

    private void OnEnable()
    {
        SetMachine();
        SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_MINIGAME_01_OPEN);
        Invoke("UseSlotSkill", startDelayTime);
    }
    public void SetMachine()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetSlot(chipData[Random.Range(0, chipData.Count)]);
            //slots[i].transform.localPosition = Vector3.down * resultPos + vector3
        }
    }

    public void UseSlotSkill()
    {
        if(test)
        {
            TestSpin();
        }
        else
        {
            PublisherApiManager.Instance.UseSkill(1, UseSkillCallback);
        }
        
    }

    public void UseSkillCallback(long statusCode, JObject data)
    {
        switch (statusCode)
        {
            case 200:
                Spin(data);
                break;
        }
    }
    public void Spin(JObject data)
    {
        next = DateTimeParser.Parse(data["cooldown_date"].ToObject<string>());
        bonusRatio = data["continuedBonusPercent"].ToObject<long>();
        bonus = data["continuedBonus"].ToObject<long>();
        reward = data["reward"].ToObject<long>();
        update = data["update"].ToObject<JObject>();
        spin = true;
        currBottom = defoltBottomLine;
        rewardText.text = reward.ToString();
        bonusRatioText.text = string.Format("{0}%", bonusRatio.ToString());
        bonusText.text = (reward + bonus).ToString();
        SetResult();
        PointIn(update);
        SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_MINIGAME_01_START);
        StartCoroutine(SlotSpin());
        LobbyManager.Instance.StartMiniGameCountDown(data["cooldown_date"].ToObject<string>());
    }
    public void TestSpin()
    {
        next = DateTime.Now;
        bonusRatio = 20;
        reward = testResult;
        bonus = testResult * bonusRatio;
        spin = true;
        currBottom = defoltBottomLine;
        rewardText.text = reward.ToString();
        bonusRatioText.text = string.Format("{0}%", bonusRatio.ToString());
        bonusText.text = (reward + bonus).ToString();
        SetResult();
        PointIn(update);
        SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_MINIGAME_01_START);
        StartCoroutine(SlotSpin());
        //LobbyManager.Instance.StartMiniGameCountDown(data["cooldown_date"].ToObject<string>());
    }

    private IEnumerator SlotSpin()
    {
        //Sound_TableEnum.SFX_MINIGAME_01_LOOP;

        //Sound_TableEnum.SFX_MINIGAME_01_RESULT;
        //Sound_TableEnum.SFX_MINIGAME_01_START;
        SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_MINIGAME_01_LOOP);
        while (spin)
        {
            if (time > timer)
            {
                timer += Time.deltaTime;
                currBottom = defoltBottomLine - timer * speed;
                for (int i = 0; i < slots.Count; i++)
                {
                    var change = SetPos(slots[i], currBottom + i * intabal);
                    if (change)
                    {
                        slots[i].SetSlot(chipData[Random.Range(0, chipData.Count)]);
                    }
                    if (time > timer)
                    {
                        result = true;
                    }
                }
            }
            else
            {
                timer += Time.deltaTime;
                currBottom = defoltBottomLine - timer * speed;
                if (!result)
                {
                    float pos = currBottom + resultIdx * intabal;
                    while (pos < bottomLine)
                    {
                        pos += intabal * slots.Count;
                    }
                    if (resultPos > pos)
                    {
                        currBottom += resultPos - pos;
                        spin = false;
                    }
                }
                for (int i = 0; i < slots.Count; i++)
                {
                    var change = SetPos(slots[i], currBottom + i * intabal);
                    if (change)
                    {
                        if (result)
                        {
                            resultIdx = i;
                            slots[i].SetSlot(reward);
                            result = false;
                        }
                        else
                        {
                            slots[i].SetSlot(chipData[Random.Range(0, chipData.Count)]);
                        }
                    }
                }
            }
            yield return 0;
        }
        Invoke("ResultPopup", 1);
        timer = 0;
    }

    public void ResultPopup()
    {
        SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_MINIGAME_01_RESULT);
        resultPanel.SetActive(true);
    }
    private bool SetPos(MinigameSlot slot, float pos)
    {
        bool change = false;
        Vector3 curPos = slot.GetComponent<RectTransform>().anchoredPosition;
        while (pos < bottomLine)
        {
            pos += intabal * slots.Count;
        }
        if (pos > curPos.y)
        {
            //           Console.Error(string.Format("{0} : {1}", curPos.y, pos));
            change = true;
        }
        curPos.y = pos;
        slot.GetComponent<RectTransform>().anchoredPosition = curPos;
        return change;
    }

    public async UniTask PointIn(JObject data)
    {
        try
        {
            long gold = data["gold"].ToObject<long>();
            
            if (gold > 0)
            {
                await PublisherApiManager.Instance.PointInAPI("gold", gold, SendUserInfoCheck);
            }
        }
        catch { Console.Log("failed to Point In"); }
        try
        {
            long silver = data["silver"].ToObject<long>();
            if (silver > 0)
            {
                await PublisherApiManager.Instance.PointInAPI("silver", silver, SendUserInfoCheck);
            }
        }
        catch { Console.Log("failed to Point In"); }
    }

    private void SendUserInfoCheck(long statusCode, JObject json)
    {
        var packet = new Packet((int)CPProtocol.CP_CONSOLE_USERINFO);
        WebSocketManager.defaultCli.Send(packet.ToJson());
        InfoManager.Instance.GetMyItems();
    }
    private void SetResult()
    {
        string spriteFontText = string.Format("{0:0,000}", reward + bonus);
        
        List<Sprite> sprites = spritefont.ConvertSpriteFont(spriteFontText);
        spritefontParent.SetSpriteFont(spriteFontText);
        //for (int i = 0; i < sprites.Count; i++) {
        //    SpriteFontPrefab go = null;
        //    if (spritefonts.Count > i) {
        //        go = spritefonts[i];
        //    }
        //    else {
        //        go = ObjectPoolManager.Instance.spritfontPool.Pop();
        //        spritefonts.Add(go);
        //        go.rect.SetParent(spritefontParent);

        //    }

        //    go.image.sprite = sprites[i];
        //    go.image.SetNativeSize();
        //    go.rect.localScale = new Vector3(1, 1, 1);
        //}

        //while (spritefonts.Count > sprites.Count)
        //{
        //    var child = spritefonts[spritefonts.Count - 1];
        //    spritefonts.RemoveAt(spritefonts.Count - 1);
        //    ObjectPoolManager.Instance.spritfontPool.Push(child);
        //}
    }

}

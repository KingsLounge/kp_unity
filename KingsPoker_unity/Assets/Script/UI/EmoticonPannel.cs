using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI;

public class EmoticonPannel : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonTemplate;

    [SerializeField]
    private Transform container;

    [SerializeField]
    private EmoticonData emoticonData;

    public TableManager tableManager;

    public GameObject menuPupup;
    public PlayerManager playerManager;

    private void Awake()
    {
        List<EmoticonInfo> emoticons = emoticonData.emoticons;
        for (int i = 0; i < emoticons.Count; i++)
        {
            EmoticonInfo info = emoticons[i];
            int idx = i;
            if (!info.includeList)
                continue;

            if (info.moveable)
            {
                GameObject template = Instantiate(buttonTemplate);
                template.GetComponentInChildren<Image>().sprite = info.iconTexture;
                template.GetComponentInChildren<Text>().text = LocalizeManager.GetLocalString(
                    info.description
                );
                template
                    .GetComponentInChildren<Button>()
                    .onClick.AddListener(() =>
                    {
                        OnClickEmoticon(idx, info.key);
                    });
                template.SetActive(true);
                template.transform.SetParent(container);
                template.transform.CleanIdentity();
            }
        }

        for (int i = 0; i < emoticons.Count; i++)
        {
            EmoticonInfo info = emoticons[i];
            int idx = i;
            if (!info.includeList)
                continue;

            if (!info.moveable)
            {
                GameObject template = Instantiate(buttonTemplate);
                template.GetComponentInChildren<Image>().sprite = info.iconTexture;
                template.GetComponentInChildren<Text>().text = LocalizeManager.GetLocalString(
                    info.description
                );
                template
                    .GetComponentInChildren<Button>()
                    .onClick.AddListener(() =>
                    {
                        OnClickEmoticon(idx, info.key);
                    });
                template.SetActive(true);
                template.transform.SetParent(container);
                template.transform.CleanIdentity();
            }
        }
    }

    public void OnClickEmoticon(int idx, Emoticon emoticon)
    {
        List<EmoticonInfo> emoticons = emoticonData.emoticons;
        EmoticonInfo info = emoticons[idx];
        long gtn = tableManager.GetRoomNumber();

        if (info.moveable)
        {
            playerManager.SeatInActiveEmosSetting(emoticon, info);
        }
        else
        {
            var p = new Packet((int)CPProtocol.CP_ROOM_CHAT);
            p.Add("gtn", gtn);
            p.Add("type", "emo");
            p.Add("to_gid", MyStatus.gid);
            p.Add("msg", emoticon.ToString());
            WebSocketManager.defaultCli.Send(p);
        }

        menuPupup.SetActive(false);
    }
}

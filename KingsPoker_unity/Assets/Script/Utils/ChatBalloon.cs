using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using Newtonsoft.Json.Linq;

public class ChatBalloon : MonoBehaviour
{
    [Header("My")]
    [SerializeField]
    private GameObject my_balloon;
    private UniformSize my_balloon_uniform;
    [SerializeField]
    private GameObject my_text_balloon;
    [SerializeField]
    private GameObject my_emo_balloon;

    [Header("Other")]
    [SerializeField]
    private GameObject other_balloon;
    private UniformSize other_balloon_uniform;
    [SerializeField]
    private GameObject other_text_balloon;
    [SerializeField]
    private GameObject other_emo_balloon;

    [Space]
    [SerializeField]
    private UniformSize uniformSize;
    [SerializeField]
    private List<Image> profile_images = new List<Image>();
    [SerializeField]
    private List<Text> nick_texts = new List<Text>();
    [SerializeField]
    private List<Text> texts = new List<Text>();
    [SerializeField]
    private List<Text> time_texts = new List<Text>();
    [SerializeField]
    private List<Image> emoticons = new List<Image>();
    [SerializeField]
    private float max_width = 500f;
    [SerializeField]
    private SpriteAtlas characterAtlas;
    [SerializeField]
    private EmoticonData emoticonData;

    public void InGameSetData(JObject data)
    {
        string gid = data.ValueOrDefault("to_gid", "");
        RoomUserData userData = InfoManager.Instance.GetRoom(ChatManager.instance.roomNumber).userList.Find(u => u.gid == gid);
        string type = data.ValueOrDefault("type", "msg");
        string msg = data.ValueOrDefault("msg", "");
        string time = data.ValueOrDefault("time", "");
        SetData(userData, msg, type, time);
    }

    public async void SetData(RoomUserData userData, string msg, string type, string time)
    {
        bool isMe = MyStatus.gid == userData.gid;
        string profile_url = userData.photourl;
        int icon_no = userData.icon_no;
        Sprite sprite = null;
        if (icon_no == 0)
        {
            Texture2D texture = await ImageDatabase.LoadImageTexture(profile_url, Application.persistentDataPath + "/profileImg", userData.gid);

            if (texture)
            {
                sprite = texture.ToSprite();
            }

        }
        else
        {
            string imageStr = "";
            try
            {
                imageStr = TableDataManager.characterTable[icon_no]["2d_index"].ToString();
            }
            catch
            {

            }
            sprite = characterAtlas.GetSprite(imageStr);
        }
        nick_texts.ForEach(t => t.text = userData.nick);
        profile_images.ForEach(p => p.sprite = sprite);
        uniformSize.target = (isMe ? my_balloon.transform : other_balloon.transform) as RectTransform;
        my_balloon.SetActive(isMe);
        other_balloon.SetActive(!isMe);
        bool isEmo = type == "emo";
        my_text_balloon.SetActive(!isEmo);
        other_text_balloon.SetActive(!isEmo);
        my_emo_balloon.SetActive(isEmo);
        other_emo_balloon.SetActive(isEmo);
        if (my_balloon_uniform == null)
            my_balloon_uniform = my_balloon.GetComponent<UniformSize>();
        if (other_balloon_uniform == null)
            other_balloon_uniform = other_balloon.GetComponent<UniformSize>();
        my_balloon_uniform.target = (isEmo ? my_emo_balloon : my_text_balloon).transform as RectTransform;
        other_balloon_uniform.target = (isEmo ? other_emo_balloon : other_text_balloon).transform as RectTransform;
        emoticons.ForEach(e => {
            if (isEmo)
                e.sprite = emoticonData.Find(msg).texture;
        });
        texts.ForEach(t => SetText(t, msg));
        System.DateTime sendTime;
        if(!System.DateTime.TryParse(time,out sendTime))
            sendTime = System.DateTime.Now;
        time_texts.ForEach(t => t.text = sendTime.ToLocalTime().ToString("yyyy/MM/dd HH:mm"));
    }

    private void SetText(Text text, string msg)
    {
        text.text = "";
        int last_space = -1;
        for(int i = 0; i < msg.Length; i++)
        {
            char c = msg[i];
            if (c == ' ')
                last_space = i;
            else if (c == '\n')
                last_space = -1;
            text.text += c;
            if(text.preferredWidth > max_width)
            {
                if(last_space == -1)
                {
                    int lastIdx = text.text.Length - 1;
                    text.text = text.text.Insert(lastIdx, "\n");
                }
                else
                {
                    text.text = text.text.Remove(last_space, 1).Insert(last_space, "\n");
                }
                last_space = -1;
            }
        }
    }
}

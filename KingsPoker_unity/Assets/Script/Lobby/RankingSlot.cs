using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class RankingSlot : MonoBehaviour
{
    [SerializeField]
    private RawImage profileImage;
    [SerializeField]
    private Text nickText;
    [SerializeField]
    private Text rankText;
    [SerializeField]
    private Text expText;
    private string uid;
    private string url;
    public void SetSlat(JObject data, int type) 
    {
        
        rankText.text = data["rank"].ToObject<int>().ToString();
        nickText.text = data["nick"].ToObject<string>().ToString();
        if(type == 1)
        {
            expText.text = data["exp_d"].ToObject<string>();
        }
        else
        {   
            expText.text = data["exp_w"].ToObject<string>();
        }
        uid = data["userid"].ToObject<string>();
        url = data["photourl"].ToObject<string>();
        gameObject.SetActive(true);
        Setprofile();
    }

    public async void Setprofile()
    {
        profileImage.enabled = false;
        if(!string.IsNullOrEmpty(url))
        {
            profileImage.texture = await ImageDatabase.LoadImageTexture(url,Application.persistentDataPath + "/profileImg", uid);
            profileImage.enabled = true;
        }
        else
        {
            Console.Log("URL 없다!!");
        }
    }
}

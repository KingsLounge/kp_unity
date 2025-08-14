using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpriteFontInfo {
    public char key;
    public Sprite sprite = null;
}
[CreateAssetMenu(fileName = "SpriteFontData", menuName = "SpriteFontData")]
public class SpriteFontData : ScriptableObject
{
    public List<SpriteFontInfo> spriteInfo = new List<SpriteFontInfo>();
    public Dictionary<char, Sprite> infoDic;
    public List<Sprite> ConvertSpriteFont(string str) {
        if(infoDic == null)
        {
            infoDic = new Dictionary<char, Sprite>();
            for(int i = 0; i < spriteInfo.Count; i++)
            {
                var info = spriteInfo[i];
                if(!infoDic.ContainsKey(info.key))
                {
                    infoDic.Add(info.key, info.sprite);
                }
                else
                {
                    Debug.LogError(string.Format("alredy input : {0}",info.key));
                }
            }
        }
        List<Sprite> result = new List<Sprite>();
        for(int i = 0; i < str.Length; i++) {
            Sprite sprite = infoDic[str[i]];
            // spriteInfo.Find(delegate(SpriteFontInfo d){
            //     return d.key == str[i];
            // });
            if(sprite != null)
                result.Add(sprite);
        }
        return result;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFontParent : MonoBehaviour
{
    public SpriteFontData spriteFont;
    private Transform thisTransform;
    private List<SpriteFontPrefab> spriteFontScripts = new List<SpriteFontPrefab>();
    [SerializeField]
    private Color fontColor = Color.white;
    // Start is called before the first frame update
    private void Awake()
    {
        thisTransform = transform;
    }

    public void SetSpriteFont(string str)
    {
        
        List<Sprite> sprites = spriteFont.ConvertSpriteFont(str);
        for (int i = 0; i < sprites.Count; i++)
        {
            SpriteFontPrefab go = null;
            if (spriteFontScripts.Count > i)
            {
                go = spriteFontScripts[i];
            }
            else
            {

                go = ObjectPoolManager.Instance.spritfontPool.Pop();
                spriteFontScripts.Add(go);
                if(!thisTransform)
                {
                    thisTransform = transform;
                }
                go.rect.SetParent(thisTransform);

            }

            //rect.sizeDelta = new Vector2(sprites[i].texture.width, sprites[i].texture.height);
            
            go.rect.transform.localPosition = new Vector3(0, 0, 0);
            go.image.sprite = sprites[i];
            go.image.SetNativeSize();
            go.image.color = fontColor;
            go.rect.localScale = new Vector3(1, 1, 1);
        }

        while (spriteFontScripts.Count > sprites.Count)
        {
            var child = spriteFontScripts[spriteFontScripts.Count - 1];
            spriteFontScripts.RemoveAt(spriteFontScripts.Count - 1);
            ObjectPoolManager.Instance.spritfontPool.Push(child);
        }
    }
}

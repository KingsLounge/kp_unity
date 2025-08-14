using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP;
using UnityEngine.UI;

public class EnterChipButton : MonoBehaviour
{
    private Button btn = null;
    public GameObject spriteFontContainer = null;
    public long bng = 0;
    public SpriteFontData spriteFont = null;
    public Text buyinText = null;
    public List<SpriteFontPrefab> spriteFontScripts = new List<SpriteFontPrefab>();

    void Awake() {
        btn = this.gameObject.GetComponent<Button>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Setting(long bng,long emn,long sb,long bb) { 
        this.bng = bng;
        


        string spriteFontText = MoneyToString.Converting(sb) + '/' + MoneyToString.Converting(bb);
        List<Sprite> sprites = spriteFont.ConvertSpriteFont(spriteFontText);
        //var tr = spriteFontContainer.transform;
        
        //spriteFontContainer.transform.DestroyChildren();
        buyinText.text = MoneyToString.Converting(emn);
        for(int i = 0; i < sprites.Count; i++) {
            SpriteFontPrefab go = null;
            if(spriteFontScripts.Count > i + 1) {
                go = spriteFontScripts[i];
            }
            else {
                go = ObjectPoolManager.Instance.spritfontPool.Pop();
                spriteFontScripts.Add(go);
                go.rect.SetParent(spriteFontContainer.transform);
            }
            
            //rect.sizeDelta = new Vector2(sprites[i].texture.width, sprites[i].texture.height);
            
            go.image.sprite = sprites[i];
            go.image.SetNativeSize();
            
            
            go.rect.localScale = new Vector3(1,1,1);
        }
        while(spriteFontScripts.Count > sprites.Count)
        {
            var child = spriteFontScripts[spriteFontScripts.Count-1];
            spriteFontScripts.RemoveAt(spriteFontScripts.Count-1);
            ObjectPoolManager.Instance.spritfontPool.Push(child);
        }
    }
   
}



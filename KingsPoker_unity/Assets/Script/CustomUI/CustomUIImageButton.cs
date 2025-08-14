using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class CustomUIImageButton : CustomUIButton
{
    protected override string defaultStyle => "anchor=center,center;position=0,0;text-align=center;icon=null;icon-align=right";
    private Vector2 originalSize = Vector2.zero;
    public float minHeight = 75;
    public Image iconImg;

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        //normal, hover, pressed, disable
        JArray images = data.ContainsKey("data") ? data["data"] as JArray : new JArray(new string[1] { "" });
        SpriteState spriteState = btn.spriteState;
        spriteState.highlightedSprite = null;
        spriteState.pressedSprite = null;
        spriteState.selectedSprite = null;
        spriteState.disabledSprite = null;
        List<Sprite> sprites = new List<Sprite>();
        for(int i = 0; i < images.Count; i++)
        {
            if(images[i].Type == JTokenType.String)
            {
                sprites.Add(root.GetSprite(images[i].ToString()));
            }
            else
            {
                sprites.Add(null);
            }
        }
        ChangeSprite(sprites);
        RectTransform rect = (transform as RectTransform);
        Vector2 newSize = rect.sizeDelta;
        newSize.y = originalSize.y < minHeight ? minHeight : originalSize.y;
        rect.sizeDelta = newSize;
    }

    public void ChangeSprite(List<Sprite> images)
    {
        SpriteState spriteState = btn.spriteState;
        spriteState.highlightedSprite = null;
        spriteState.pressedSprite = null;
        spriteState.selectedSprite = null;
        spriteState.disabledSprite = null;

        if (images.Count == 1)
            btn.transition = Selectable.Transition.ColorTint;
        else
            btn.transition = Selectable.Transition.SpriteSwap;
        Image img = btn.targetGraphic as Image;

        if (images.Count > 0)
            img.sprite = images[0];
        else
            img.sprite = null;

        if (images.Count > 1 && images[1] != null)
            spriteState.highlightedSprite = images[1];

        if (images.Count > 2 && images[2] != null)
            spriteState.pressedSprite = images[2];

        if (images.Count > 3 && images[3] != null)
            spriteState.disabledSprite = images[3];

        if (spriteState.highlightedSprite == null)
            spriteState.highlightedSprite = img.sprite;

        if (spriteState.pressedSprite == null)
            spriteState.pressedSprite = img.sprite;

        if (spriteState.selectedSprite == null)
            spriteState.selectedSprite = img.sprite;

        if (spriteState.disabledSprite == null)
            spriteState.disabledSprite = img.sprite;

        btn.spriteState = spriteState;
        img.SetNativeSize();
        if (img.sprite != null)
            originalSize = new Vector2(img.sprite.rect.width, img.sprite.rect.height);
        else
            originalSize = Vector2.zero;
    }

    protected override void SetStyle(List<KeyValuePair<string, string>> data)
    {
        base.SetStyle(data);
        RectTransform rect = transform as RectTransform;
        RectTransform buttonRect = btn.targetGraphic.rectTransform;
        string icon_align_str = "";
        bool sizeChange = false;
        bool positionChange = false;
        Vector2 newSize = new Vector2(originalSize.x, originalSize.y);
        Vector2 newPosition = buttonRect.anchoredPosition;
        for (int i = 0; i < data.Count; i++)
        {
            KeyValuePair<string, string> style = data[i];
            switch(style.Key)
            {
                case "anchor":
                    {
                        string[] anchorStr = style.Value.Split(',');
                        Vector2 anchor = buttonRect.anchorMin;
                        if (anchorStr.Length > 0)
                        {
                            if (anchorStr[0] == "left")
                                anchor.x = 0;
                            else if (anchorStr[0] == "center")
                                anchor.x = 0.5f;
                            else if (anchorStr[0] == "right")
                                anchor.x = 1;
                        }
                        if (anchorStr.Length > 1)
                        {
                            if (anchorStr[1] == "bottom")
                                anchor.y = 0;
                            else if (anchorStr[1] == "center")
                                anchor.y = 0.5f;
                            else if (anchorStr[1] == "top")
                                anchor.y = 1;
                        }
                        buttonRect.anchorMin = buttonRect.anchorMax = buttonRect.pivot = anchor;
                        positionChange = true;
                    }
                    break;
                case "position":
                    {
                        string[] posStr = style.Value.Split(',');
                        float x,y;
                        if (posStr.Length > 0 && float.TryParse(posStr[0],out x))
                        {
                            newPosition.x = x;
                            positionChange = true;
                        }
                        if (posStr.Length > 1 && float.TryParse(posStr[1], out y))
                        {
                            newPosition.y = y;
                            positionChange = true;
                        }
                    }
                    break;
                case "width":
                    {
                        float temp;
                        if(float.TryParse(style.Value, out temp))
                        {
                            newSize.x = temp;
                            sizeChange = true;
                        }
                    }
                    break;
                case "height":
                    {
                        float temp;
                        if (float.TryParse(style.Value, out temp))
                        {
                            newSize.y = temp;
                            sizeChange = true;
                        }
                    }
                    break;
                case "text-align":
                    {
                        if (style.Value == "center")
                            text.alignment = TextAnchor.MiddleCenter;
                        else if (style.Value == "left")
                            text.alignment = TextAnchor.MiddleLeft;
                        else if (style.Value == "right")
                            text.alignment = TextAnchor.MiddleRight;
                    }
                    break;
                case "icon":
                    {
                        if(style.Value == "null")
                        {
                            iconImg.gameObject.SetActive(false);
                        }
                        else
                        {
                            iconImg.gameObject.SetActive(true);
                            iconImg.sprite = root.GetSprite(style.Value);
                            iconImg.SetNativeSize();
                        }
                    }
                    break;
                case "icon-align":
                    {
                        icon_align_str = style.Value;
                    }
                    break;
            }
        }
        if(sizeChange)
        {
            buttonRect.sizeDelta = newSize;

            Vector2 size = rect.sizeDelta;
            size.y = newSize.y < minHeight ? minHeight : newSize.y;
            rect.sizeDelta = size;
        }
        if (positionChange)
            buttonRect.anchoredPosition = newPosition;
        if (!string.IsNullOrEmpty(icon_align_str))
        {
            RectTransform myRect = btn.targetGraphic.rectTransform;
            RectTransform iconRect = iconImg.rectTransform;
            if (icon_align_str == "center")
                iconRect.anchoredPosition = new Vector2((myRect.sizeDelta.x - iconRect.sizeDelta.x) / 2f, 0);
            else if (icon_align_str == "left")
                iconRect.anchoredPosition = new Vector2(0, 0);
            else if (icon_align_str == "right")
                iconRect.anchoredPosition = new Vector2(myRect.sizeDelta.x - (iconRect.sizeDelta.x / 2f), 0);
        }
    }
}

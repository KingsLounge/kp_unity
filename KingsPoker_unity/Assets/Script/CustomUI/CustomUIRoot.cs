using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;

public class CustomUIRoot : MonoBehaviour
{
    public Text title;
    public delegate void CustomUIEvent(CustomUI ui, JObject data, JObject evt);
    [SerializeField]
    private RectTransform background;
    [SerializeField]
    private Transform container;
    [SerializeField]
    private Transform topContainer;
    [SerializeField]
    private Transform subContainer;
    public Dictionary<string, CustomUIEvent> events = new Dictionary<string, CustomUIEvent>();
    [SerializeField]
    private CustomUIModuleList moduleList = null;
    private List<CustomUI> currentUIs = new List<CustomUI>();
    private Dictionary<string, ObjectPool2<CustomUI>> objectPools = new Dictionary<string, ObjectPool2<CustomUI>>();
    private GameObject objectPoolContainer;
    [SerializeField]
    private SpriteAtlas spriteAtlas;
    private LayoutGroup layoutGroup;
    private LayoutGroup subLayoutGroup;
    public string defaultStyle = "";
    [SerializeField]
    private ScrollRect scroll;

    private bool isInit = false;
    public Vector2 viewportSize
    {
        get
        {
            if(scroll)
            {
                Rect rect = scroll.viewport ? scroll.viewport.rect : scroll.content.rect;
                return new Vector2(rect.width, rect.height);
            }
            else
            {
                Rect a = (container as RectTransform).rect;
                return new Vector2(a.width,a.height);
            }
        }
    }

    public Vector2 containerSize
    {
        get
        {
            Rect a = (container as RectTransform).rect;
            return new Vector2(a.width, a.height);
        }
    }

    public float totalUseY
    {
        get
        {
            float result = 0;
            currentUIs.ForEach(ui => result += (ui.transform as RectTransform).rect.height);
            return result;
        }
    }

    private void Awake()
    {
       
    }
    private void Init()
    {
        objectPoolContainer = new GameObject("objectPoolContainer - Generate by ObjectPool (" + gameObject.name + ")");
        objectPoolContainer.AddComponent<RectTransform>();
        objectPoolContainer.transform.SetParent(transform.parent);
        objectPoolContainer.transform.localPosition = Vector3.zero;
        layoutGroup = GetComponent<LayoutGroup>();
        if (subContainer)
            subLayoutGroup = subContainer.GetComponent<LayoutGroup>();
        isInit = true;
    }
    public void Clear()
    {
        for(int i = 0; i < currentUIs.Count; i++)
        {
            CustomUI cur = currentUIs[i];
            string type = cur.type;
            if (objectPools.ContainsKey(type))
            {
                objectPools[type].ReturnObject(cur);
            }
        }
        currentUIs.Clear();
    }

    public void RemoveUI(CustomUI ui)
    {
        if (ui == null)
            return;
        string type = ui.type;
        if (objectPools.ContainsKey(type))
        {
            objectPools[type].ReturnObject(ui);
        }
        currentUIs.Remove(ui);
    }

    public List<CustomUI> GetGroup(string groupName)
    {
        List<CustomUI> result = new List<CustomUI>();
        for(int i = 0; i < currentUIs.Count; i++)
        {
            if(currentUIs[i].IsBelongToAGroup(groupName))
            {
                result.Add(currentUIs[i]);
            }
        }
        return result;
    }

    public CustomUI GetChildUIWithKey(string key)
    {
        for (int i = 0; i < currentUIs.Count; i++)
        {
            if (currentUIs[i].key == key)
            {
                return currentUIs[i];
            }
        }
        return null;
    }

    public List<CustomUI> GetChildUIsWithKey(string key)
    {
        List<CustomUI> result = new List<CustomUI>();
        for (int i = 0; i < currentUIs.Count; i++)
        {
            if (currentUIs[i].key == key)
            {
                result.Add(currentUIs[i]);
            }
        }
        return result;
    }

    public void Setting(JArray arr)
    {
        container.localScale = Vector3.zero;

        if( subContainer != null)
        {
            subContainer.localScale = Vector3.zero;
        }
        
        Clear();
        string style = "";
        for (int i = 0; i < arr.Count; i++)
        {
            JObject cur = arr[i] as JObject;
            string type = cur.ValueOrDefault("type","");
            if (type == "ui")
            {
                style = cur.ValueOrDefault("style", "");
            }
        }


        string[] split = style.Split(';');
        List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
        for (int i = 0; i < split.Length; i++)
        {
            if (string.IsNullOrEmpty(split[i])) continue;
            string[] style_arr = split[i].Split('=');
            string style_key = style_arr[0];
            string style_value = style_arr[1];
            list.Add(new KeyValuePair<string, string>(style_key, style_value));
        }


        string[] defaultSplit = defaultStyle.Split(';');
        for (int i = 0; i < defaultSplit.Length; i++)
        {
            if (string.IsNullOrEmpty(defaultSplit[i])) continue;
            string[] style_arr = defaultSplit[i].Split('=');
            string style_key = style_arr[0];
            string style_value = style_arr[1];
            if (list.FindIndex(data => data.Key == style_key) == -1)
                list.Add(new KeyValuePair<string, string>(style_key, style_value));
        }
        SetStyle(list);

        int sub_mode = 0;
        for (int i = 0; i < arr.Count; i++)
        {
            JObject cur = arr[i] as JObject;
            if (cur.ContainsKey("type"))
            {
                string type = cur["type"].ToString();
                if (type == "sub_start" || type == "sub_end")
                {
                    sub_mode = (type == "sub_start") ? 1 : 0;
                    continue;
                }

                if(type == "top_start" || type == "top_end")
                {
                    sub_mode = (type == "top_start") ? 2 : 0;
                    continue;
                }

                if (type == "ui") continue;
                CustomUI ui = GetUIInstance(type, (sub_mode <= 0) ? container : (sub_mode == 1) ? subContainer : topContainer);
                if (ui)
                {
                    currentUIs.Add(ui);
                    ui.root = this;
                    ui.SetUI(cur);
                }
                else
                {
                    Debug.LogWarning("Cannot find [" + type + "] type in module");
                }
            }
            else
            {
                Debug.LogWarning("Cannot find type property in JSON data");
            }
        }

        for (int i = 0; i < currentUIs.Count; i++)
        {
            currentUIs[i].CompleteSetting();
        }
        if (gameObject.activeInHierarchy)
            StartCoroutine(CompleteSetting());
        else
            CustomUpdateCaller.Instance.StartCoroutine(CompleteSetting());
    }

    private void SetStyle(List<KeyValuePair<string,string>> styles)
    {
        Vector2 leftBottom = background ? background.offsetMin : Vector2.zero;
        Vector2 rightTop = background ? background.offsetMax : Vector2.zero;
        Vector2 size = new Vector2(background ? background.rect.width : 0, background ? background.rect.height : 0);
        bool stretchChange = false;
        bool sizeChange = false;
        for (int i = 0; i < styles.Count; i++)
        {
            KeyValuePair<string, string> style = styles[i];
            switch(style.Key)
            {
                case "left":
                    leftBottom.x = float.Parse(style.Value);
                    stretchChange = true;
                    break;
                case "right":
                    rightTop.x = -float.Parse(style.Value);
                    stretchChange = true;
                    break;
                case "top":
                    rightTop.y = -float.Parse(style.Value);
                    stretchChange = true;
                    break;
                case "bottom":
                    leftBottom.y = float.Parse(style.Value);
                    stretchChange = true;
                    break;
                case "width":
                    size.x = float.Parse(style.Value);
                    sizeChange = true;
                    break;
                case "height":
                    size.y = float.Parse(style.Value);
                    sizeChange = true;
                    break;
                case "title":
                    if (title) title.text = LocalizeManager.GetLocalString(style.Value);
                    break;
                case "title-align":
                    if (title)
                    {
                        if (style.Value == "left") title.alignment = TextAnchor.MiddleLeft;
                        else if (style.Value == "center") title.alignment = TextAnchor.MiddleCenter;
                        else if (style.Value == "right") title.alignment = TextAnchor.MiddleRight;
                        break;
                    }
                    break;

            }
        }
        if(background)
        {
            if (stretchChange)
            {
                background.offsetMin = leftBottom;
                background.offsetMax = rightTop;
            }
            if (sizeChange)
            {
                RectTransform parentRect = (background.parent as RectTransform);
                Vector2 parentSize = new Vector2(parentRect.rect.width, parentRect.rect.height);
                parentSize.x -= size.x;
                parentSize.y -= size.y;
                background.offsetMin = new Vector2(parentSize.x / 2, parentSize.y / 2);
                background.offsetMax = new Vector2(-parentSize.x / 2, -parentSize.y / 2);
            }
        }
    }

    private IEnumerator CompleteSetting()
    {
        if(layoutGroup)
            layoutGroup.enabled = false;
        if (subLayoutGroup)
            subLayoutGroup.enabled = false;
        yield return null;
        if (layoutGroup)
            layoutGroup.enabled = true;
        if (subLayoutGroup)
            subLayoutGroup.enabled = true;
        container.localScale = Vector3.one;

        if (subContainer != null)
        {
            subContainer.localScale = Vector3.one;
        }
    }

    public Sprite GetSprite(string name)
    {
        return spriteAtlas.GetSprite(name);
    }

    private CustomUI GetUIInstance(string type, Transform parent = null)
    {
        if (!isInit)
        {
            Init();
        }
        if (moduleList == null)
            return null;
        if(!objectPools.ContainsKey(type))
        {
            GameObject prefab = moduleList.Find((value) => { return value.GetComponent<CustomUI>().type == type; });
            if(prefab)
            {
                ObjectPool2<CustomUI> objectPool = new ObjectPool2<CustomUI>();
                objectPool.activator = (obj) =>
                {
                    obj.gameObject.SetActive(true);
                };
                objectPool.deactivator = (obj) =>
                {
                    obj.transform.SetParent(objectPoolContainer.transform);
                    obj.gameObject.SetActive(false);
                };
                objectPool.generator = () =>
                {
                    GameObject go = Instantiate(prefab);
                    return go.GetComponent<CustomUI>();
                };
                objectPools.Add(type, objectPool);
            }
            else
            {
                return null;
            }
        }
        CustomUI result = objectPools[type].GetObject();
        if(parent == null)
            parent = container;
        result.transform.SetParent(parent);
        result.transform.CleanIdentity();
        return result;
    }

    public JObject GetJObject()
    {
        JObject obj = new JObject();
        for (int i = 0; i < currentUIs.Count; i++)
        {
            JProperty value = currentUIs[i].GetProperty();
            if (value != null)
            {
                if(obj.ContainsKey(value.Name))
                {
                    if(currentUIs[i].gameObject.activeInHierarchy)
                    {
                        obj.Remove(value.Name);
                        obj.Add(value);
                    }
                }
                else
                {
                    obj.Add(value);
                }
            }
        }
        return obj;
    }

    public string GetJSON()
    {
        return GetJObject().ToString();
    }
}

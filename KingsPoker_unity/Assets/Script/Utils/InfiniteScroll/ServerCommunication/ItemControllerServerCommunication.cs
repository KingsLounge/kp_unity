using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.LowLevel;
using UnityEngine.UI;

public class ItemControllerServerCommunication : UIBehaviour, IInfiniteScrollSetup
{
    public JArray dataArray = new JArray();
    public delegate void UpdateItemDelegate(GameObject go, JToken data);
    public UpdateItemDelegate updateItemCallback;
    public delegate void ChangePageDelegate(bool next);
    public ChangePageDelegate changePageCallback;
    public float menualReloadYRange = 100f;
    public bool reload = true;
    private InfiniteScroll _infiniteScroll;
    private ScrollRect _scrollRect;
    public Direction direction = Direction.Vertical;
    private ObjectPool2<GameObject> pool;
    private bool poolInit = false;

    [SerializeField]
    private Type type = Type.infinite;

    [Header("Normal")]
    [SerializeField]
    private GameObject poolObj = null;

    [SerializeField]
    private Transform poolParent;

    [SerializeField]
    private Transform contentParent;

    private List<GameObject> list = new List<GameObject>();

    public enum Type
    {
        infinite,
        normal
    }

    public enum Direction
    {
        Vertical,
        Horizontal,
    }

    private InfiniteScroll infiniteScroll
    {
        get
        {
            if (_infiniteScroll == null)
                _infiniteScroll = GetComponent<InfiniteScroll>();
            return _infiniteScroll;
        }
    }
    private ScrollRect scrollRect
    {
        get
        {
            if (_scrollRect == null)
                _scrollRect = GetComponentsInParent<ScrollRect>(true)[0];
            return _scrollRect;
        }
    }
    private bool mousedown = false;
    private bool menualOut = false;
    private bool canLoad = true;
    private bool boundaryMode = false;

    public void SetScrollOnTop()
    {
        scrollRect.verticalNormalizedPosition = 1;
    }

    public void OnPostSetupItems()
    {
        infiniteScroll.onUpdateItem.AddListener(OnUpdateItem);
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        Refresh();
    }

    public void Refresh()
    {
        try
        {
            if (type == Type.infinite)
            {
                scrollRect.enabled = true;
                var rectTransform = GetComponent<RectTransform>();
                var delta = rectTransform.sizeDelta;
                var count = dataArray.Count + infiniteScroll.presetItem.Count;
                switch (direction)
                {
                    case Direction.Vertical:
                        delta.y =
                            infiniteScroll.itemScale * count
                            + infiniteScroll.borderTop
                            + infiniteScroll.borderBottom;
                        break;
                    case Direction.Horizontal:
                        delta.x =
                            infiniteScroll.itemScale * count
                            + infiniteScroll.borderLeft
                            + infiniteScroll.borderRight;
                        break;
                }
                rectTransform.sizeDelta = delta;
                infiniteScroll.Refresh();
            }
            else
            {
                if (!poolInit)
                {
                    PoolInit();
                }
                for (int i = 0; i < dataArray.Count; i++)
                {
                    GameObject obj;
                    if (i < list.Count)
                    {
                        obj = list[i];
                    }
                    else
                    {
                        obj = pool.GetObject();
                    }
                    OnUpdateItem(i, obj);
                }
                while (list.Count > dataArray.Count)
                {
                    pool.ReturnObject(list[dataArray.Count]);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e, gameObject);
        }
    }

    private void PoolInit()
    {
        if (!poolInit)
        {
            pool = new ObjectPool2<GameObject>();
            pool.activator = (go) =>
            {
                go.SetActive(true);
                list.Add(go);
                go.transform.SetParent(contentParent);
            };
            pool.deactivator = (go) =>
            {
                go.transform.SetParent(poolParent);
                list.Remove(go);
                go.SetActive(false);
            };
            pool.generator = () =>
            {
                GameObject go = Instantiate(poolObj, poolParent);
                return go;
            };
            poolInit = true;
        }
    }

    public void Update()
    {
        if (!reload)
            return;
        float curPos = 0;
        switch (direction)
        {
            case Direction.Horizontal:
                curPos = scrollRect.normalizedPosition.x;
                break;
            case Direction.Vertical:
                curPos = scrollRect.normalizedPosition.y;
                break;
        }

        if (Input.GetMouseButtonDown(0))
        {
            mousedown = true;
            boundaryMode = (curPos >= 1 || curPos <= 0);
        }
        if (Input.GetMouseButtonUp(0))
        {
            mousedown = false;
        }
        float pixelPerRange = 1 / scrollRect.content.rect.height;
        bool outOfRange = curPos <= -pixelPerRange || curPos >= 1 + pixelPerRange;
        if (mousedown)
        {
            menualOut = boundaryMode && outOfRange;
        }
        else
        {
            if (outOfRange)
            {
                if (canLoad)
                {
                    if (menualOut)
                    {
                        float range = pixelPerRange * menualReloadYRange;
                        bool outOfMenualRange = curPos <= -range || curPos >= 1 + range;
                        if (outOfMenualRange)
                        {
                            bool next = curPos < -range;
                            if (next && changePageCallback != null)
                            {
                                scrollRect.enabled = false;
                            }
                            changePageCallback?.Invoke(next);
                        }
                    }
                    else
                    {
                        bool next = curPos < 0;
                        if (next && changePageCallback != null)
                        {
                            scrollRect.enabled = false;
                        }
                        changePageCallback?.Invoke(next);
                    }
                }
                canLoad = false;
            }
            else
            {
                canLoad = true;
            }
        }
    }

    public void OnUpdateItem(int itemCount, GameObject obj)
    {
        if (itemCount < 0 || itemCount >= dataArray.Count)
        {
            obj.SetActive(false);
        }
        else
        {
            obj.SetActive(true);
            updateItemCallback?.Invoke(obj, dataArray[itemCount]);
        }
    }
}

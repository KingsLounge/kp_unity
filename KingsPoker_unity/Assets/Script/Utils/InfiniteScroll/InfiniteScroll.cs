using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InfiniteScroll : UIBehaviour
{
    [SerializeField]
    private RectTransform itemPrototype;

    [SerializeField, Range(0, 50)]
    int instantateItemCount = 9;

    [SerializeField]
    private Direction direction;

    [SerializeField]
    private float interval = 0f;

    public OnItemPositionChange onUpdateItem = new OnItemPositionChange();
    public List<RectTransform> presetItem = new List<RectTransform>();

    [System.NonSerialized]
    public LinkedList<RectTransform> itemList = new LinkedList<RectTransform>();

    protected float diffPreFramePosition = 0;

    protected int currentItemNo = 0;

    public float borderLeft = 0;

    public float borderRight = 0;

    public float borderTop = 0;

    public float borderBottom = 0;

    public enum Direction
    {
        Vertical,
        Horizontal,
    }

    // cache component

    private RectTransform _rectTransform;
    protected RectTransform rectTransform
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }

    private float anchoredPosition
    {
        get
        {
            return direction == Direction.Vertical
                ? -rectTransform.anchoredPosition.y
                : rectTransform.anchoredPosition.x;
        }
    }

    private float _itemScale = -1;
    public float itemScale
    {
        get
        {
            if (itemPrototype != null && _itemScale == -1)
            {
                _itemScale =
                    direction == Direction.Vertical
                        ? itemPrototype.sizeDelta.y + interval
                        : itemPrototype.sizeDelta.x + interval;
            }
            return _itemScale;
        }
    }

    protected override void Start()
    {
        Init();
    }

    public void Refresh()
    {
        var controllers = GetComponents<MonoBehaviour>()
            .Where(item => item is IInfiniteScrollSetup)
            .Select(item => item as IInfiniteScrollSetup)
            .ToList();
        for (int i = 0; i < itemList.Count; i++)
        {
            LinkedListNode<RectTransform> cur = itemList.First;

            for (int j = 0; j < i; j++)
            {
                cur = cur.Next;
            }
            foreach (var controller in controllers)
            {
                controller.OnUpdateItem(currentItemNo + i, cur.Value.gameObject);
            }
        }
    }

    private void Init()
    {
        var controllers = GetComponents<MonoBehaviour>()
            .Where(item => item is IInfiniteScrollSetup)
            .Select(item => item as IInfiniteScrollSetup)
            .ToList();

        // create items

        var scrollRect = GetComponentInParent<ScrollRect>();
        scrollRect.horizontal = direction == Direction.Horizontal;
        scrollRect.vertical = direction == Direction.Vertical;
        scrollRect.content = rectTransform;

        itemPrototype.gameObject.SetActive(false);
        for (int i = 0; i < presetItem.Count; i++)
        {
            presetItem[i].anchoredPosition =
                direction == Direction.Vertical
                    ? new Vector2(0, -itemScale * i - borderTop)
                    : new Vector2(itemScale * i + borderLeft, 0);
        }
        diffPreFramePosition = -(presetItem.Count * itemScale + borderLeft);
        
        for (int i = 0; i < instantateItemCount || ItemRectOver(scrollRect, i); i++)
        {
            var itemIdx = i + presetItem.Count;
            var item = GameObject.Instantiate(itemPrototype) as RectTransform;
            item.SetParent(transform, false);
            item.name = i.ToString();
            item.anchoredPosition =
                direction == Direction.Vertical
                    ? new Vector2(0, -itemScale * itemIdx - borderTop)
                    : new Vector2(itemScale * itemIdx + borderLeft, 0);
            itemList.AddLast(item);

            item.gameObject.SetActive(true);

            foreach (var controller in controllers)
            {
                controller.OnUpdateItem(i, item.gameObject);
            }
        }

        foreach (var controller in controllers)
        {
            controller.OnPostSetupItems();
        }
    }

    // 아이템 포지션이 부모 오브젝트 밖으로 나가면 true 반환
    public bool ItemRectOver(ScrollRect scrollRect, int itemIdx)
    {
        var scrollRectRect = scrollRect.GetComponent<RectTransform>();
        if (direction == Direction.Vertical)
        {
            var y = itemScale * (itemIdx - 2) + borderTop;
            return y < scrollRectRect.rect.height;
        }
        else
        {
            var x = itemScale * (itemIdx - 2) + borderLeft;
            return x < scrollRectRect.rect.width;
        }
    }

    void Update()
    {
        if (itemList.First == null)
        {
            return;
        }

        while (anchoredPosition - diffPreFramePosition < -itemScale * 2)
        {
            diffPreFramePosition -= itemScale;

            var item = itemList.First.Value;
            itemList.RemoveFirst();
            itemList.AddLast(item);

            var pos =
                itemScale * instantateItemCount + itemScale * (currentItemNo + presetItem.Count);
            item.anchoredPosition =
                (direction == Direction.Vertical)
                    ? new Vector2(0, -pos - borderTop)
                    : new Vector2(pos + borderLeft, 0);

            onUpdateItem.Invoke(currentItemNo + instantateItemCount, item.gameObject);

            currentItemNo++;
        }

        while (anchoredPosition - diffPreFramePosition > 0)
        {
            diffPreFramePosition += itemScale;

            var item = itemList.Last.Value;
            itemList.RemoveLast();
            itemList.AddFirst(item);

            currentItemNo--;

            var pos = itemScale * (currentItemNo + presetItem.Count);
            item.anchoredPosition =
                (direction == Direction.Vertical)
                    ? new Vector2(0, -pos - borderTop)
                    : new Vector2(pos + borderLeft, 0);
            onUpdateItem.Invoke(currentItemNo, item.gameObject);
        }
    }

    [System.Serializable]
    public class OnItemPositionChange : UnityEngine.Events.UnityEvent<int, GameObject> { }
}

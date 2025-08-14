using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

namespace Jeckl.InfinityScroll
{
    public class InfinityScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private bool initialized = false;
        private JArray dataArray_ = new JArray();
        public JArray dataArray
        {
            get => dataArray_;
            set
            {
                dataArray_ = value;
                UpdateDataArray();
            }
        }

        private ScrollRect _scroll;
        private ScrollRect scroll {
            get
            {
                if (_scroll == null)
                    _scroll = GetComponent<ScrollRect>();
                return _scroll;
            }
        }
        private RectTransform container
        {
            get => scroll.content;
        }
        private RectTransform viewport
        {
            get => scroll.viewport;
        }
        public RectTransform template;
        private RectTransform firstChildren;
        private RectTransform lastChildren;
        private bool dragging = false;
        public int generated = 10;
        public delegate void UpdateItem(RectTransform rect, JToken data);
        public delegate void OutOfRange(bool first);
        public delegate void LastUpdate(bool first);
        public UpdateItem updateItem;
        public OutOfRange outOfRange;
        public LastUpdate lastUpdate;
        private float viewportSize
        {
            get
            {
                if (scroll == null || scroll.viewport == null)
                    return 0;
                var size = scroll.viewport.rect;
                return direction == Direction.Vertical ? size.height : size.width;
            }
        }
        private float containerSize
        {
            get
            {
                if (container == null)
                    return 0;
                var size = container.sizeDelta;
                return direction == Direction.Vertical ? size.y : size.x;
            }
        }
        public float maxOverRange = 100f;
        private int itemIdx = 0;
        private float anchoredPosition
        {
            get => direction == Direction.Vertical ? container.anchoredPosition.y : -container.anchoredPosition.x;
            set
            {
                var pos = container.anchoredPosition;
                if (direction == Direction.Vertical)
                    pos.y = value;
                else
                    pos.x = -value;
                container.anchoredPosition = pos;
            }
        }
        private PointerEventData e;
        [System.Serializable]
        public enum Direction
        {
            Vertical,
            Horizontal
        }
        public Direction direction = Direction.Vertical;
        public Vector2 velocity
        {
            get => scroll.velocity;
            set => scroll.velocity = value;
        }

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            if (initialized) return;
            initialized = true;
            float t = 0;
            for (int i = 0; i < generated; i++)
            {
                RectTransform item = Instantiate(template);
                if (i == 0) firstChildren = item;
                else if (i == generated - 1) lastChildren = item;
                item.SetParent(container);
                anchoredPosition = t;
                item.gameObject.SetActive(false);
                item.gameObject.name = template.gameObject.name + "_" + i.ToString();
                item.localScale = template.localScale;
                item.localPosition = template.localPosition;
                t += GetItemSize(item);
            }
        }

        public void Refresh()
        {
            UpdateDataArray();
        }

        public void OnBeginDrag(PointerEventData e)
        {
            this.e = e;
            dragging = true;
        }
        public void OnEndDrag(PointerEventData e)
        {
            this.e = e;
            dragging = false;
        }

        private void UpdateDataArray()
        {
            if (!initialized) Init();
            if (dataArray_.Count > generated + itemIdx)
            {
                for (int i = 0; i < generated; i++)
                {
                    RectTransform item = container.GetChild(i) as RectTransform;
                    CallUpdateItem(item, dataArray_[i + itemIdx]);
                    item.gameObject.SetActive(true);
                }
            }
            else
            {
                itemIdx = dataArray_.Count - generated;
                if (itemIdx < 0)
                    itemIdx = 0;
                for (int i = 0; i < generated; i++)
                {
                    RectTransform item = container.GetChild(i) as RectTransform;
                    if (itemIdx + i >= dataArray_.Count)
                    {
                        item.gameObject.SetActive(false);
                    }
                    else
                    {
                        CallUpdateItem(item, dataArray_[i + itemIdx]);
                        item.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void CallUpdateItem(RectTransform rect, JToken item) {
            updateItem?.Invoke(rect, item);
        }

        private float GetItemSize(RectTransform rect)
        {
            return direction == Direction.Vertical ? rect.sizeDelta.y : rect.sizeDelta.x;
        }

        private void LateUpdate()
        {
            if (!initialized) Init();
            float container_size = containerSize; //containerSize와 viewportSize는 getter에서 계산을 위한 Vector2 인스턴스를 생성할수있다, 불필요한 인스턴스생성을 방지하기위해 값을 한번만 가져온다
            float viewport_size = viewportSize;
            float maxPos = container_size >= viewport_size ? container_size - viewport_size + maxOverRange : maxOverRange;
            if (anchoredPosition >= maxPos)
            {
                anchoredPosition = maxPos;
                if (!dragging)
                {
                    outOfRange?.Invoke(false);
                }
            }
            else if (anchoredPosition <= -maxOverRange)
            {
                anchoredPosition = -maxOverRange;
                if (!dragging)
                {
                    outOfRange?.Invoke(true);
                }
            }
            while (anchoredPosition < 0 && itemIdx > 0)
            {
                itemIdx--;
                lastChildren.SetSiblingIndex(0);
                firstChildren = lastChildren;
                lastChildren = container.GetChild(generated - 1) as RectTransform;
                CallUpdateItem(firstChildren, dataArray_[itemIdx]);
                anchoredPosition += GetItemSize(firstChildren);
                if(!firstChildren.gameObject.activeSelf)
                    firstChildren.gameObject.SetActive(true);
                if (dragging)
                {
                    scroll.OnEndDrag(e);
                    scroll.OnBeginDrag(e);
                }
                if(itemIdx == 0)
                {
                    lastUpdate?.Invoke(true);
                }
            }
            while (anchoredPosition > GetItemSize(firstChildren) && (itemIdx + generated) < dataArray_.Count)
            {
                itemIdx++;
                firstChildren.SetSiblingIndex(generated - 1);
                lastChildren = firstChildren;
                firstChildren = container.GetChild(0) as RectTransform;
                anchoredPosition -= GetItemSize(lastChildren);
                CallUpdateItem(lastChildren, dataArray_[(itemIdx + generated - 1)]);
                if (!lastChildren.gameObject.activeSelf)
                    lastChildren.gameObject.SetActive(true);
                if (dragging)
                {
                    scroll.OnEndDrag(e);
                    scroll.OnBeginDrag(e);
                }
                bool condition = (itemIdx + generated - 1) == dataArray_.Count - 1;
                if (condition)
                {
                    lastUpdate?.Invoke(false);
                }
            }
        }
        public void OnDrag(PointerEventData eventData)
        {
            e = eventData;
        }
    }

}

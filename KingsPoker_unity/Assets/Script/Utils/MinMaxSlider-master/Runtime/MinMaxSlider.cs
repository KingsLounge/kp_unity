using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Min_Max_Slider
{
	[RequireComponent(typeof(RectTransform))]
	public class MinMaxSlider : Selectable, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		private enum DragState
		{
			Both,
			Min,
			Max
		}

		/// <summary>
		/// Floating point tolerance
		/// </summary>
		private const double FLOAT_TOL = 0.01f;

		[Header("UI Controls")]
		[SerializeField] private Camera customCamera = null;
		[SerializeField] private RectTransform sliderBounds = null;
		[SerializeField] private RectTransform minHandle = null;
		[SerializeField] private RectTransform maxHandle = null;
		[SerializeField] private RectTransform middleGraphic = null;

		// text components (optional)
		[Header("Display Text (Optional)")]
		[SerializeField] private Text minText = null;
		[SerializeField] private Text maxText = null;
		[SerializeField] public string textFormat = "{0}";

		// values
		[Header("Limits")]
		[SerializeField] private double minLimit = 0;
		[SerializeField] private double maxLimit = 100;

		[Header("Values")]
		public bool wholeNumbers;
		[SerializeField] private double minValue = 25;
		[SerializeField] private double maxValue = 75;

		public MinMaxValues Values => new MinMaxValues(minValue, maxValue, minLimit, maxLimit);

		/// <summary>
		/// Event invoked when either slider value has changed
		/// <para></para>
		/// T0 = min, T1 = max
		/// </summary>
		[Serializable]
		public class SliderEvent : UnityEvent<double, double>
		{
		}
		
		public SliderEvent onValueChanged = new SliderEvent();

		private Vector3 dragStartPosition;
		private double dragStartMinValue01;
		private double dragStartMaxValue01;
		private DragState dragState;
		private readonly Vector3[] worldCorners = new Vector3[4];
		private bool passDragEvents; // this allows drag events to be passed through to scrollers

		private Camera mainCamera;
		private Canvas parentCanvas;
		private bool isOverlayCanvas;
        public delegate string TextFormatter(double value);
        public TextFormatter textFormatter = null;

		protected override void Start()
		{
			base.Start();

			if (!sliderBounds)
				sliderBounds = transform as RectTransform;

			parentCanvas = GetComponentInParent<Canvas>();
			isOverlayCanvas = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay;
			mainCamera = customCamera != null ? customCamera : Camera.main;
		}

        private void Update()
        {
            if(transform.hasChanged)
            {
                Refresh();
                transform.hasChanged = false;
            }
        }

        private void Refresh()
        {
            RefreshSliders();
            middleGraphic.ForceUpdateRectTransforms();
            UpdateText();
            UpdateMiddleGraphic();
        }

        private long RoundToInt(double a)
        {
            return (long)a;
        }

        private double Clamp(double a, double min, double max)
        {
            if (a > max) return max;
            if (a < min) return min;
            return a;
        }

        private double Abs(double a)
        {
            return a >= 0 ? a : -a;
        }

        private double Lerp(double s, double e, double v)
        {
            return s + ((e - s) * v);
        }

		public void SetLimits(double minLimit, double maxLimit)
		{
			this.minLimit = wholeNumbers ? RoundToInt(minLimit) : minLimit;
			this.maxLimit = wholeNumbers ? RoundToInt(maxLimit) : maxLimit;
		}

		public void SetValues(MinMaxValues values)
		{
			SetValues(values.minValue, values.maxValue, values.minLimit, values.maxLimit);
		}

		public void SetValues(double minValue, double maxValue)
		{
			SetValues(minValue, maxValue, minLimit, maxLimit);
		}

		public void SetValues(double minValue, double maxValue, double minLimit, double maxLimit)
		{
			this.minValue = wholeNumbers ? RoundToInt(minValue) : minValue;
			this.maxValue = wholeNumbers ? RoundToInt(maxValue) : maxValue;
			SetLimits(minLimit, maxLimit);
			RefreshSliders();
			UpdateText();
			UpdateMiddleGraphic();

			// event
			onValueChanged.Invoke(this.minValue, this.maxValue);
		}

		private void RefreshSliders()
		{
			SetSliderAnchors();

			double clampedMin = Clamp(minValue, minLimit, maxLimit);
			SetHandleValue01(minHandle, GetPercentage(minLimit, maxLimit, clampedMin));

			double clampedMax = Clamp(maxValue, minLimit, maxLimit);
			SetHandleValue01(maxHandle, GetPercentage(minLimit, maxLimit, clampedMax));
		}

		private void SetSliderAnchors()
		{
			minHandle.anchorMin = new Vector2(0, 0.5f);
			minHandle.anchorMax = new Vector2(0, 0.5f);
			minHandle.pivot = new Vector2(0.5f, 0.5f);

			maxHandle.anchorMin = new Vector2(1, 0.5f);
			maxHandle.anchorMax = new Vector2(1, 0.5f);
			maxHandle.pivot = new Vector2(0.5f, 0.5f);
		}

		public void UpdateText()
		{
			if (minText)
				minText.text = textFormatter == null ? string.Format(textFormat,minValue) : textFormatter.Invoke(minValue);

			if (maxText)
                maxText.text = textFormatter == null ? string.Format(textFormat, maxValue) : textFormatter.Invoke(maxValue);
        }

		private void UpdateMiddleGraphic()
		{
			if (!middleGraphic)
				return;

			middleGraphic.anchorMin = Vector2.zero;
			middleGraphic.anchorMax = Vector2.one;
			middleGraphic.offsetMin = new Vector2(minHandle.anchoredPosition.x, 0);
			middleGraphic.offsetMax = new Vector2(maxHandle.anchoredPosition.x, 0);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			var clickPosition = isOverlayCanvas
				? (Vector3) eventData.position
				: mainCamera.ScreenToWorldPoint(eventData.position);

			passDragEvents = Abs(eventData.delta.x) < Abs(eventData.delta.y);

			if (passDragEvents)
			{
				PassDragEvents<IBeginDragHandler>(x => x.OnBeginDrag(eventData));
			}
			else
			{
				dragStartPosition = clickPosition;
				dragStartMinValue01 = GetValue01(minHandle.position.x);
				dragStartMaxValue01 = GetValue01(maxHandle.position.x);

				// set drag state
				if (dragStartPosition.x < minHandle.position.x || IsWithinRect(minHandle, dragStartPosition))
				{
					dragState = DragState.Min;
					minHandle.SetAsLastSibling();
				}
				else if (dragStartPosition.x > maxHandle.position.x || IsWithinRect(maxHandle, dragStartPosition))
				{
					dragState = DragState.Max;
					maxHandle.SetAsLastSibling();
				}
				else
					dragState = DragState.Both;
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			var clickPosition = isOverlayCanvas
				? (Vector3) eventData.position
				: mainCamera.ScreenToWorldPoint(eventData.position);

			if (passDragEvents)
			{
				PassDragEvents<IDragHandler>(x => x.OnDrag(eventData));
			}
			else if (minHandle && maxHandle)
			{
				SetSliderAnchors();

				if (dragState == DragState.Min || dragState == DragState.Max)
				{
					double dragPosition01 = GetValue01(clickPosition.x);
					double minHandleValue = GetValue01(minHandle.position.x);
					double maxHandleValue = GetValue01(maxHandle.position.x);

					if (dragState == DragState.Min)
						SetHandleValue01(minHandle, Clamp(dragPosition01, 0, maxHandleValue));
					else if (dragState == DragState.Max)
						SetHandleValue01(maxHandle, Clamp(dragPosition01, minHandleValue, 1));
				}
				else
				{
					var sliderBoundsRect = sliderBounds.rect;
					var rectStart = sliderBoundsRect.position;
					var rectEnd = rectStart;
					rectEnd.x += sliderBoundsRect.width;

					var worldWidth = isOverlayCanvas
						? sliderBoundsRect.width
						: mainCamera.ScreenToWorldPoint(rectEnd).x - mainCamera.ScreenToWorldPoint(rectStart).x;

					double distancePercent = (clickPosition.x - dragStartPosition.x) / worldWidth;
					SetHandleValue01(minHandle, dragStartMinValue01 + distancePercent);
					SetHandleValue01(maxHandle, dragStartMaxValue01 + distancePercent);

				}

				// set values
				double min = Lerp(minLimit, maxLimit, GetValue01(minHandle.position.x));
				double max = Lerp(minLimit, maxLimit, GetValue01(maxHandle.position.x));
				SetValues(min, max);

				UpdateText();
				UpdateMiddleGraphic();
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (passDragEvents)
			{
				PassDragEvents<IEndDragHandler>(x => x.OnEndDrag(eventData));
			}
			else
			{
				double minHandleValue = GetValue01(minHandle.position.x);
				double maxHandleValue = GetValue01(maxHandle.position.x);

				// this safe guards a possible situation where the slides can get stuck
				if (Abs(minHandleValue) < FLOAT_TOL && Abs(maxHandleValue) < FLOAT_TOL)
					maxHandle.SetAsLastSibling();
				else if (Abs(minHandleValue - 1) < FLOAT_TOL && Abs(maxHandleValue - 1) < FLOAT_TOL)
					minHandle.SetAsLastSibling();
			}
		}

		private void PassDragEvents<T>(Action<T> callback) where T : IEventSystemHandler
		{
			Transform parent = transform.parent;

			while (parent != null)
			{
				foreach (var component in parent.GetComponents<Component>())
				{
					if (!(component is T))
						continue;

					callback.Invoke((T) (IEventSystemHandler) component);
					return;
				}

				parent = parent.parent;
			}
		}

		/// <summary>
		/// Generates rectTransforms world corners
		/// </summary>
		private void GetWorldCorners()
		{
			sliderBounds.GetWorldCorners(worldCorners);
		}

		/// <summary>
		/// Sets handles position 
		/// </summary>
		/// <param name="handle"></param>
		/// <param name="value01"></param>
		private void SetHandleValue01(Transform handle, double value01)
		{
			GetWorldCorners();
            Vector2 pos = new Vector2(
                Convert.ToSingle(Lerp(worldCorners[0].x, worldCorners[2].x, value01)),
                worldCorners[0].y + (worldCorners[1].y - worldCorners[0].y) / 2f);
            if (Double.IsNaN(pos.x) || Double.IsNaN(pos.y))
            {
                throw new Exception("isNaN");
            }
            handle.position = new Vector3(pos.x, pos.y, handle.position.z);
        }

		/// <summary>
		/// Returns a values from 0-1 based on this rects world corners
		/// </summary>
		/// <param name="worldPositionX"></param>
		/// <returns></returns>
		private double GetValue01(double worldPositionX)
		{
			GetWorldCorners();
			double posX = Clamp(worldPositionX, worldCorners[0].x, worldCorners[2].x);
			return GetPercentage(worldCorners[0].x, worldCorners[2].x, posX);
		}

		/// <summary>
		/// Returns percentage of input based on min and max values
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		private static double GetPercentage(double min, double max, double input)
		{
			return (input - min) / (max - min);
		}

		private static bool IsWithinRect(RectTransform rect, Vector2 worldPosition)
		{
			Vector3[] corners = new Vector3[4];
			rect.GetWorldCorners(corners);
			return worldPosition.x > corners[0].x && worldPosition.x < corners[2].x;
		}

		[Serializable]
		public struct MinMaxValues
		{
			public double minValue, maxValue, minLimit, maxLimit;
			public static MinMaxValues DEFUALT = new MinMaxValues(25, 75, 0, 100);

			public MinMaxValues(double minValue, double maxValue, double minLimit, double maxLimit)
			{
				this.minValue = minValue;
				this.maxValue = maxValue;
				this.minLimit = minLimit;
				this.maxLimit = maxLimit;
			}
			
			/// <summary>
            /// Constructor for when values equal limits
            /// </summary>
            /// <param name="minValue"></param>
            /// <param name="maxValue"></param>
            public MinMaxValues(double minValue, double maxValue)
            {
            	this.minValue = minValue;
            	this.maxValue = maxValue;
            	this.minLimit = minValue;
            	this.maxLimit = maxValue;
            }
            private double Abs(double a)
            {
                return a >= 0 ? a : -a;
            }
            public bool IsAtMinAndMax()
            {
            	return Abs(minValue - minLimit) < FLOAT_TOL && Abs(maxValue - maxLimit) < FLOAT_TOL;
            }

			public override string ToString()
			{
				return $"Values(min:{minValue}, max:{maxValue}) | Limits(min:{minLimit}, max:{maxLimit})";
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Math = System.Math;

namespace AdvancedCullingSystem.Examples
{
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public Vector2 value { get; private set; }

        [SerializeField] private Image _joyImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private float _maxMoveRadius = 75;

        private RectTransform _rectTransform;
        private RectTransform _joyTransform;
        private Vector2 _localClickPoint;


        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _joyTransform = _joyImage.GetComponent<RectTransform>();
        }

        public void ResetJoy()
        {
            _joyTransform.localPosition = Vector2.zero;

            value = Vector2.zero;
        }


        public virtual void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            Vector2 touchPosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform,
                eventData.position, eventData.pressEventCamera, out touchPosition);

            Vector2 localPosition = _localClickPoint + (touchPosition - _localClickPoint).normalized * Mathf.Min((_localClickPoint - touchPosition).magnitude, _maxMoveRadius);
            _joyTransform.localPosition = localPosition;

            value = new Vector2(
                (float)Math.Round((localPosition - _localClickPoint).x / _maxMoveRadius, 1),
                (float)Math.Round((localPosition - _localClickPoint).y / _maxMoveRadius, 1));

            if ((value.x < 0.20f && value.x > -0.20f) && (value.y < 0.20f && value.y > -0.20f))
                value = Vector2.zero;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            ResetJoy();
        }
    }
}

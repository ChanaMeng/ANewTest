using UnityEngine;
using UnityEngine.EventSystems;

namespace SDClub.UIFrameWork
{
    public class CustomButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public System.Action OnClickAction;
        public System.Action OnPointerDownAction;
        public System.Action OnPointerUpAction;
        
        public void OnPointerClick(PointerEventData eventData) => OnClickAction?.Invoke();
        public void OnPointerDown(PointerEventData eventData) => OnPointerDownAction?.Invoke();
        public void OnPointerUp(PointerEventData eventData) => OnPointerUpAction?.Invoke();
    }
}

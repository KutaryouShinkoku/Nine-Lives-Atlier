using Game.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.UI.Common.Components
{
    public class SelectableUICardView : UISubView, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField]
        private CommonCardUIView root;

        public UnityEvent<SelectableUICardView> onCardEnter;
        public UnityEvent<SelectableUICardView> onCardExit;
        public UnityEvent<SelectableUICardView> onCardDown;
        public UnityEvent<SelectableUICardView> onCardUp;
        public UnityEvent<SelectableUICardView> onCardClick;

        public void SetCard(CardIds id) => root.SetCard(id);

        public void OnPointerEnter(PointerEventData eventData) => onCardEnter?.Invoke(this);
        public void OnPointerExit(PointerEventData eventData) => onCardExit?.Invoke(this);
        public void OnPointerDown(PointerEventData eventData) => onCardDown?.Invoke(this);
        public void OnPointerUp(PointerEventData eventData) => onCardUp?.Invoke(this);
        public void OnPointerClick(PointerEventData eventData) => onCardClick?.Invoke(this);

        public void SetRootDataModel(DataModelBase dataModel)
        {
            root.DataModel = dataModel;
        }
    }
}
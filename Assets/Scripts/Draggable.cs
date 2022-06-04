using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

  public Transform parentToReturnTo = null;
  GameManager gameManager;
  Card c;

  private void Start() {
    gameManager = GetComponent<Card>().gameManager;
    c = GetComponent<Card>();
  }
  public void OnBeginDrag(PointerEventData eventData) {
    parentToReturnTo = transform.parent;

    if(parentToReturnTo.name != "Card Pile") { //If the card was taken from a property slot
      //Reset the open panel icon
      parentToReturnTo.parent.GetComponent<PropertySlot>().openPropertySlotButton.GetComponent<Image>().sprite = gameManager.normal;
      parentToReturnTo.parent.GetComponent<PropertySlot>().openPropertySlotButton.GetComponent<Image>().color = Color.white;
    }


    transform.SetParent(parentToReturnTo.name != "Card Pile" ? transform.parent.parent.parent.parent : transform.parent.parent);

    //deduct any panel bonus
    c.ChangeBonus("panel_bonus", 1);

    GetComponent<CanvasGroup>().blocksRaycasts = false;
  }
  public void OnDrag(PointerEventData eventData) {
    if(c.tenants) {
      return;
    }

    if(Input.GetMouseButton(1)) return;
    transform.position = eventData.position;
  }
  public void OnEndDrag(PointerEventData eventData) {
    transform.SetParent(parentToReturnTo);

    // If we haven't ended up back on the card pile, we're likely on a property slot. Add bonuses (if any)
    if(parentToReturnTo.name != "Card Pile") {
      float panelBonus = parentToReturnTo.parent.GetComponent<PropertySlot>().panelBonus;
      c.ChangeBonus("panel_bonus", panelBonus);
    }

    GetComponent<CanvasGroup>().blocksRaycasts = true;
  }
}

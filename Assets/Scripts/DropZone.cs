using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler {
  public GameManager gameManager;
  public PropertySlot parentPropertySlot;

  public void Start() {
    parentPropertySlot = transform.parent.GetComponent<PropertySlot>();
  }

  public void OnPointerEnter(PointerEventData eventData) {
    //Add some cool border effects on the card when the pointer hovers over the card
  }
  public void OnPointerExit(PointerEventData eventData) {
    //Clear the cool border effects on the card when the pointer leaves the card
  }
  public void OnDrop(PointerEventData eventData) {
    Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
    Card c = d.GetComponent<Card>();

    if(d != null) {
      if(parentPropertySlot.isOwned && d.parentToReturnTo.name != "Card Pile") {
        Image OriginalPanelButtonImage = d.parentToReturnTo.GetComponent<DropZone>().parentPropertySlot.GetComponent<PropertySlot>().openPropertySlotButton.GetComponentInChildren<Image>();
        OriginalPanelButtonImage.sprite = (c.tenants) ? gameManager.normal : gameManager.exclamation;
        OriginalPanelButtonImage.color = (OriginalPanelButtonImage.sprite == gameManager.normal) ? Color.white : Color.red;
        return; // Buy panel is locked 
      }
      // Check there isn't already a property in the slot and that we aren't the original slot
      if(parentPropertySlot.DropZone.childCount > 0 && d.parentToReturnTo != parentPropertySlot.transform) {
        GameObject card2 = parentPropertySlot.DropZone.GetChild(0).gameObject;

        // Can't relocate the house if it's being leased, or if the user has no action points left!
        if(c.tenants || card2.GetComponent<Card>().tenants || gameManager.actionPoints == 0) return;

        // Change any bonuses (moving cards from left panels to right panels, or right to left)
        card2.GetComponent<Card>().ChangeBonus("panel_bonus", 1);
        card2.GetComponent<Card>().ChangeBonus("panel_bonus", d.parentToReturnTo.GetComponent<PropertySlot>().panelBonus);

        // Move the card in the destination slot to the original slot
        card2.GetComponent<Draggable>().parentToReturnTo = d.parentToReturnTo;
        card2.transform.SetParent(card2.GetComponent<Draggable>().parentToReturnTo);

        Image buttonImage2 = card2.GetComponent<Draggable>().parentToReturnTo.GetComponent<PropertySlot>().openPropertySlotButton.GetComponent<Image>();
        buttonImage2.sprite = (card2.GetComponent<Card>().tenants) ? gameManager.normal : gameManager.exclamation;
        buttonImage2.color = (buttonImage2.sprite == gameManager.normal) ? Color.white : Color.red;
      } else if(!c.purchased) { // Check if the property is owned
        //Property is not owned and was dropped on an open property slot

        // Can the user afford the property? Is the property unlocked? Does the user have enough action points left?
        if(gameManager.bank.money < Mathf.FloorToInt(c.cost * gameManager.supplyDemandIndex) || parentPropertySlot.DropZone.childCount > 0 || gameManager.actionPoints == 0) return;

        c.purchasePrice = Mathf.FloorToInt(c.cost * gameManager.supplyDemandIndex);
        gameManager.bank.AddMoney(-c.purchasePrice, "Property Purchase");
        gameManager.GameStats["TotalMoneySpent"] += c.purchasePrice;

        c.purchased = true;

        parentPropertySlot.GetComponent<PropertySlot>().openPropertySlotButton.GetComponent<Image>().sprite = gameManager.exclamation;
        parentPropertySlot.GetComponent<PropertySlot>().openPropertySlotButton.GetComponent<Image>().color = Color.red;

        //Check for highest property purchase price highscore
        if(Mathf.FloorToInt(c.cost * gameManager.supplyDemandIndex) > gameManager.GameStats["MostExpensivePurchased"]) {
          gameManager.GameStats["MostExpensivePurchased"] = Mathf.FloorToInt(c.cost * gameManager.supplyDemandIndex);
        }

        // Check for property value high score
        if(c.cost > gameManager.GameStats["MostExpensiveOwned"]) {
          gameManager.GameStats["MostExpensiveOwned"] = Mathf.FloorToInt(c.cost * gameManager.supplyDemandIndex);
        }

        if(c.rent > gameManager.GameStats["HighestRental"]) {
          gameManager.GameStats["HighestRental"] = c.rent;
        }

        gameManager.GameStats["TotalPropertiesOwned"]++;

        //Generate a new card if possible
        gameManager.CheckPile();
      }

      // Property currently has tenants, or user has no action points
      if(c.tenants || gameManager.actionPoints == 0) return;

      //Panel is owned and empty, property is owned by the player, check if leased for the openPropertyPanelButton icon
      Image buttonImage = parentPropertySlot.GetComponent<PropertySlot>().openPropertySlotButton.GetComponent<Image>();
      buttonImage.sprite = (c.tenants) ? gameManager.normal : gameManager.exclamation;
      buttonImage.color = (buttonImage.sprite == gameManager.normal) ? Color.white : Color.red;

      gameManager.UpdateActionPoints(-1);
      d.parentToReturnTo = transform;
    }
  }
}

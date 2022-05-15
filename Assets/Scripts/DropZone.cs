using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler {
  public GameManager gameManager;
  public Image managerImage;

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
      if(!GetComponent<BuyPanel>().isOwned) {
        Image OriginalPanelButtonImage = d.parentToReturnTo.GetComponent<BuyPanel>().openPropertySlotButton.GetComponentInChildren<Image>();
        OriginalPanelButtonImage.sprite = (c.tenants) ? gameManager.normal : gameManager.exclamation;
        OriginalPanelButtonImage.color = (OriginalPanelButtonImage.sprite == gameManager.normal) ? Color.white : Color.red;
        return; // Buy panel is locked 
      }
      // Check there isn't already a property in the slot and that we aren't the original slot
      if(transform.childCount == 2 && d.parentToReturnTo != transform) {
        GameObject card2 = transform.GetChild(1).gameObject;

        // Can't relocate the house if it's being leased
        if(c.tenants || card2.GetComponent<Card>().tenants) return;

        // Change any bonuses (moving cards from left panels to right panels, or right to left)
        card2.GetComponent<Card>().ChangeBonus("panel_bonus", 1);
        card2.GetComponent<Card>().ChangeBonus("panel_bonus", d.parentToReturnTo.GetComponent<BuyPanel>().panelBonus);

        // Move the card in the destination slot to the original slot
        card2.GetComponent<Draggable>().parentToReturnTo = d.parentToReturnTo;
        card2.transform.SetParent(card2.GetComponent<Draggable>().parentToReturnTo);

        Image buttonImage2 = card2.GetComponent<Draggable>().parentToReturnTo.GetComponent<BuyPanel>().openPropertySlotButton.GetComponentInChildren<Image>();
        buttonImage2.sprite = (card2.GetComponent<Card>().tenants) ? gameManager.normal : gameManager.exclamation;
        buttonImage2.color = (buttonImage2.sprite == gameManager.normal) ? Color.white : Color.red;
      } else if (!c.purchased) { // Check if the property is owned

        //Property is not owned and was dropped on an open property slot
        if(gameManager.bank.money < c.cost || transform.childCount == 2) return;

        c.purchasePrice = c.cost;
        gameManager.bank.AddMoney(-c.cost, "Property Purchase");
        gameManager.GameStats["TotalMoneySpent"] += c.cost;

        c.purchased = true;

        GetComponent<BuyPanel>().openPropertySlotButton.GetComponent<Image>().sprite = gameManager.exclamation;
        GetComponent<BuyPanel>().openPropertySlotButton.GetComponent<Image>().color = Color.red;

        //Check for highscores
        if(c.cost > gameManager.GameStats["MostExpensivePurchased"]) gameManager.GameStats["MostExpensivePurchased"] = c.cost;
        if(c.rent > gameManager.GameStats["HighestRental"]) gameManager.GameStats["HighestRental"] = c.rent;
        gameManager.GameStats["TotalPropertiesOwned"]++;

        //Generate a new card if possible
        gameManager.CheckPile();
      }
      if(c.tenants) return;

      //Panel is owned and empty, property is owned by the player, check if leased for the openPropertyPanelButton icon
      Image buttonImage = GetComponent<BuyPanel>().openPropertySlotButton.GetComponentInChildren<Image>();
      buttonImage.sprite = (c.tenants) ? gameManager.normal : gameManager.exclamation;
      buttonImage.color = (buttonImage.sprite == gameManager.normal) ? Color.white : Color.red;

      managerImage.sprite = c.assignedManagerImage;

      d.parentToReturnTo = transform;
    }
  }
}

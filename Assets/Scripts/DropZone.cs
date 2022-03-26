using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler{
    public GameManager gameManager;

    public void OnPointerEnter(PointerEventData eventData){
        //Add some cool border effects on the card when the pointer hovers over the card
    }

    public void OnPointerExit(PointerEventData eventData){
        //Clear the cool border effects on the card when the pointer leaves the card
    }

    public void OnDrop(PointerEventData eventData){
        if (gameManager.sounds){
            gameManager.ambientSource.PlayOneShot(gameManager.placeCardSounds[Random.Range(0, gameManager.placeCardSounds.Length)]);
        }

        Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
        Card c = d.GetComponent<Card>();

        if (d != null){
            if (!GetComponent<BuyPanel>().isOwned) return;

            if (transform.childCount == 2 && d.parentToReturnTo != transform){ //Check there isn't already a property in the slot
                //Move the card in the destination slot to the original slot
                GameObject card2 = transform.GetChild(1).gameObject;

                card2.GetComponent<Card>().ChangeBonus("panel_bonus", 1);
                card2.GetComponent<Card>().ChangeBonus("panel_bonus", d.parentToReturnTo.GetComponent<BuyPanel>().panelBonus);

                card2.GetComponent<Draggable>().parentToReturnTo = d.parentToReturnTo;
                card2.transform.SetParent(card2.GetComponent<Draggable>().parentToReturnTo);

                Image buttonImage2 = card2.GetComponent<Draggable>().parentToReturnTo.GetComponent<BuyPanel>().openPropertySlotButton.GetComponentInChildren<Image>();
                buttonImage2.sprite = (card2.GetComponent<Card>().tenants) ? gameManager.normal : gameManager.exclamation;
                buttonImage2.color = (buttonImage2.sprite == gameManager.normal) ? Color.white : Color.red;
            }else{ //Property is not owned and was dropped on an open property slot
                if (gameManager.money < c.cost || transform.childCount == 2) return;

                gameManager.AddMoney(-c.cost, "Property Purchase");
                c.purchased = true;

                GetComponent<BuyPanel>().openPropertySlotButton.GetComponent<Image>().sprite = gameManager.exclamation;
                GetComponent<BuyPanel>().openPropertySlotButton.GetComponent<Image>().color = Color.red;

                //Check for highscores
                if(c.cost > gameManager.GameStats["MostExpensivePurchased"]){
                    gameManager.GameStats["MostExpensivePurchased"] = c.cost;
                }
                if(c.rent > gameManager.GameStats["HighestRental"]){
                    gameManager.GameStats["HighestRental"] = c.rent;
                }

                gameManager.GameStats["TotalPropertiesOwned"]++;
                gameManager.GameStats["TotalMoneySpent"] += c.cost; 
                
                //Generate a new card if possible
                gameManager.CheckPile();

                if (gameManager.sounds){
                    gameManager.ambientSource.PlayOneShot(gameManager.buySellProperty, 0.2f);
                }       
            }

            //Panel is owned and empty, check if tenants are needed to change the panel options button
            Image buttonImage = GetComponent<BuyPanel>().openPropertySlotButton.GetComponentInChildren<Image>();
            buttonImage.sprite = (c.tenants) ? gameManager.normal : gameManager.exclamation;
            buttonImage.color = (buttonImage.sprite == gameManager.normal) ? Color.white : Color.red;

            d.parentToReturnTo = transform;
        }
    }
}

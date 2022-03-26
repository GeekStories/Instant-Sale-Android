using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler{

    public Transform parentToReturnTo = null;
    GameManager gameManager;
    Card c;

    private void Start(){
        gameManager = GetComponent<Card>().gameManager;
        c = GetComponent<Card>();
    }

    public void OnBeginDrag(PointerEventData eventData){
        if (gameManager.sounds) gameManager.ambientSource.PlayOneShot(gameManager.pickUpCard);

        parentToReturnTo = transform.parent;

        if(parentToReturnTo.tag != "CardPile") { //If the card was taken from a property slot
            //Reset the open panel icon
            parentToReturnTo.GetComponent<BuyPanel>().openPropertySlotButton.GetComponentInChildren<UnityEngine.UI.Image>().sprite = gameManager.normal;
            parentToReturnTo.GetComponent<BuyPanel>().openPropertySlotButton.GetComponentInChildren<UnityEngine.UI.Image>().color = Color.white;
        }

        transform.SetParent(transform.parent.parent);

        //deduct any panel bonus and update the rent 
        c.ChangeBonus("panel_bonus", 1);

        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData){
        if (Input.GetMouseButton(1)) return;

        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData){
        transform.SetParent(parentToReturnTo);
        
        if(parentToReturnTo.name != "Card Pile"){
            GetComponent<Card>().ChangeBonus("panel_bonus", parentToReturnTo.GetComponent<BuyPanel>().panelBonus); 
        }
        
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (gameManager.sounds){
            gameManager.ambientSource.PlayOneShot(gameManager.placeCardSounds[Random.Range(0, gameManager.placeCardSounds.Length)]);
        }
    }
}

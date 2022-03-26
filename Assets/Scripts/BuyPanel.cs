using UnityEngine.UI;
using UnityEngine;

public class BuyPanel : MonoBehaviour{

    public GameManager gameManager;
    public GameObject panelLock;

    public float panelBonus;

    public Button openPropertySlotButton;

    public int cost;

    public Sprite locked, unlocked;
    
    public bool isOwned = false;

    public void Start(){
        InvokeRepeating("CheckMoney", 0, 0.5f);
    }

    public void CheckMoney(){
        if (gameManager.money >= cost && !isOwned){
            panelLock.GetComponent<Image>().sprite = unlocked;
        }else{
            panelLock.GetComponent<Image>().sprite = locked;
        }
    }

    public void Unlock(){
        if (gameManager.money >= cost){
            gameManager.AddMoney(-cost, "Land Purchase");
            gameManager.GameStats["TotalMoneySpent"] += cost;

            // GetComponent<GridLayoutGroup>().cellSize = new Vector2(200, 300);
            GetComponent<GridLayoutGroup>().enabled = true;

            panelLock.SetActive(false);

            if (gameManager.sounds){
                gameManager.ambientSource.PlayOneShot(gameManager.buttonClick);
                gameManager.ambientSource.PlayOneShot(gameManager.buySellProperty);
            }

            isOwned = true;
        }
    }
}

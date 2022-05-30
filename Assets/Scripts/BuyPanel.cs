using UnityEngine;
using UnityEngine.UI;

public class BuyPanel : MonoBehaviour {

  public GameManager gameManager;
  public GameObject panelLock;
  public Button openPropertySlotButton;
  public Sprite locked, unlocked;

  public int cost;
  public bool isOwned = false;
  public float panelBonus;

  public void Start() {
    InvokeRepeating(nameof(CheckMoney), 0, 0.5f);
  }

  public void CheckMoney() {
    if(gameManager.bank.money >= cost && !isOwned) panelLock.GetComponent<Image>().sprite = unlocked;
    else panelLock.GetComponent<Image>().sprite = locked;
  }

  public void Unlock() {
    if(gameManager.bank.money >= cost && gameManager.actionPoints > 0) {
      gameManager.bank.AddMoney(-cost, "Land Purchase");
      gameManager.GameStats["TotalMoneySpent"] += cost;

      // GetComponent<GridLayoutGroup>().cellSize = new Vector2(200, 300);
      GetComponent<GridLayoutGroup>().enabled = true;

      panelLock.SetActive(false);
      gameManager.UpdateActionPoints(-1);
      isOwned = true;
    }
  }
}

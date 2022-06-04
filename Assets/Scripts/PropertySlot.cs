using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PropertySlot : MonoBehaviour {

  public GameManager gameManager;
  public Image lockImage;
  public GameObject unlockObject;
  public Transform DropZone;
  public Button openPropertySlotButton;
  public Sprite locked, unlocked;
  public TextMeshProUGUI tenancyTermText;

  public int cost;
  public bool isOwned = false;
  public float panelBonus;

  public void Start() {
    InvokeRepeating(nameof(CheckMoney), 0, 0.5f);
  }

  public void CheckMoney() {
    if(gameManager.bank.money >= cost && !isOwned) lockImage.GetComponent<Image>().sprite = unlocked;
    else {
      if(gameObject.name != "PropertySlot_1") {
        lockImage.GetComponent<Image>().sprite = locked;
      }
    }
  }

  public void Unlock() {
    if(gameManager.bank.money >= cost && gameManager.actionPoints > 0) {
      gameManager.bank.AddMoney(-cost, "Land Purchase");
      gameManager.GameStats["TotalMoneySpent"] += cost;

      unlockObject.SetActive(false);
      gameManager.UpdateActionPoints(-1);
      isOwned = true;
    }
  }
}

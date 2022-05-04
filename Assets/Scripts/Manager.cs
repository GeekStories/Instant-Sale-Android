using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour {
  public GameManager gameManager;

  public int weeklyPay;
  public int hireCost;

  public bool hired = false;

  public string description = "Manager Details\n";

  public float bonusAmount;

  public Image checkmarkImage;

  public void SelectMe() {
    if(!transform.parent.CompareTag("PropertyManagerBox")) {
      gameManager.SelectManager(gameObject);
      return;
    }

    if(transform.GetComponent<Toggle>().isOn) transform.parent.parent.parent.parent.GetComponent<PropertyPanel>().AssignManager(gameObject);
  }
}

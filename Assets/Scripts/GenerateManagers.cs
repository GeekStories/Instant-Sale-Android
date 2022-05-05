using UnityEngine;
using UnityEngine.UI;

public class GenerateManagers : MonoBehaviour {
  public GameObject managerObject;
  public Sprite[] managerSprites;

  public GameObject GenerateManager(GameObject managersForHirePanel, GameManager gameManager) {
    GameObject newManager = Instantiate(managerObject, managersForHirePanel.transform);

    newManager.GetComponent<Toggle>().group = managersForHirePanel.GetComponent<ToggleGroup>();
    newManager.name = $"manager_{managersForHirePanel.transform.childCount}";

    Manager newManagerComponent = newManager.GetComponent<Manager>();

    newManagerComponent.gameManager = gameManager;

    newManagerComponent.weeklyPay = Random.Range(500, 999);
    newManagerComponent.hireCost = newManagerComponent.weeklyPay * Random.Range(4, 9);

    float newManagerBonusAmount = (float)System.Math.Round(Random.Range(1.01f, 1.08f), 2);
    newManagerComponent.bonusAmount = newManagerBonusAmount;

    newManagerComponent.description =
      $"- ${newManagerComponent.hireCost} to hire, \n" +
      $"${newManagerComponent.weeklyPay:#,##0} weekly pay \n\n" +
      $"- Increases rent by {((newManagerComponent.bonusAmount - 1) * 100):F2}% \n" +
      $"- Auto lease to tenants";

    newManager.transform.GetChild(0).GetComponent<Image>().sprite = managerSprites[Random.Range(0, 40)];

    return newManager;
  }
}

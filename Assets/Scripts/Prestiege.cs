using UnityEngine;
using UnityEngine.UI;

public class Prestiege : MonoBehaviour {

  GameManager gameManager;

  public GameObject[] prestiegeUpgrades;

  public Text unclaimedPointsText;
  public Text pointsText;

  public int points, unclaimedPoints;

  private void Start() {
    gameManager = GetComponent<GameManager>();

    unclaimedPoints = PlayerPrefs.GetInt("UnclaimedPoints");
    points = PlayerPrefs.GetInt("prestiegePoints");
    pointsText.text = $"{points:#,##0}";

    CheckPurchasableUpgrades();
  }

  public void CheckPurchasableUpgrades() {
    foreach(GameObject upgrade in prestiegeUpgrades) {
      int upgradeCost = upgrade.GetComponent<PrestiegeUpgrade>().Cost;
      if(points >= upgradeCost) upgrade.GetComponent<Button>().interactable = true;
      upgrade.transform.GetChild(0).GetComponent<Text>().text = $"{upgradeCost:#,##0}";
    }
    // Reset the first prestiege upgrade to say Free instead of 0
    prestiegeUpgrades[0].transform.GetChild(0).GetComponent<Text>().text = "Free";
  }

  public void OpenPrestiegeUpgradePanel() { }

  public void ClaimPrestiegeUpgrade() { }

  public void SetUnclaimedPoints(int amount) {
    unclaimedPoints = amount;
    unclaimedPointsText.text = $"Unclaimed: {unclaimedPoints:#,##0}";
  }

  public void ClaimPoints() {
    if(unclaimedPoints == 0) return;

    points += unclaimedPoints;

    PlayerPrefs.SetInt("UnclaimedPoints", unclaimedPoints);
    PlayerPrefs.SetInt("prestiegePoints", points);

    unclaimedPoints = 0;

    //Reset the game
    gameManager.ResetGame();
  }
}

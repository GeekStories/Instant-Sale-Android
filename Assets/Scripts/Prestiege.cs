using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Prestiege : MonoBehaviour {

  GameManager gameManager;

  public GameObject[] upgradeButtons;

  public TextMeshProUGUI upgradeTitleText;
  public TextMeshProUGUI upgradeDescriptionText;

  public Text unclaimedPointsText;
  public Text pointsText;

  public int points, unclaimedPoints;
  public int selectedUpgradeId;

  public TextAsset prestiegeUpgrades;

  [System.Serializable]
  public class Upgrade {
    public int id;
    public string name;
    public int cost;
    public string description;

    public string modifier;
    public float adjustment;
  }

  [System.Serializable]
  public class UpgradeList {
    public Upgrade[] upgrade;
  }

  public UpgradeList currentUpgradeList = new();

  private void Start() {
    currentUpgradeList = JsonUtility.FromJson<UpgradeList>(prestiegeUpgrades.text);
    SelectUpgrade(1);

    gameManager = GetComponent<GameManager>();
    unclaimedPoints = PlayerPrefs.GetInt("UnclaimedPoints");
    points = PlayerPrefs.GetInt("prestiegePoints");
    pointsText.text = $"{points:#,##0}";
  }

  public void OpenPrestiegeUpgradePanel() {
    SelectUpgrade(1);
  }

  public void SelectUpgrade(int id) {
    Upgrade selectedUpgrade = (Upgrade)currentUpgradeList.upgrade.GetValue(id - 1);
    upgradeTitleText.text = $"{selectedUpgrade.name} - {selectedUpgrade.id}";
    upgradeDescriptionText.text = $"Cost: {selectedUpgrade.cost}\n{selectedUpgrade.description}";
    selectedUpgradeId = selectedUpgrade.id;
  }
  public void PurchaseUpgrade() {
    Upgrade selectedUpgrade = (Upgrade)currentUpgradeList.upgrade.GetValue(selectedUpgradeId);
    if(selectedUpgrade != null) {
      if(points >= selectedUpgrade.cost) {
        points -= selectedUpgrade.cost;
        pointsText.text = $"{points:#,##0}";

        PlayerPrefs.SetInt("prestiegePoints", points);        
      }
    }
  }
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

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievmentsManager : MonoBehaviour {

  public GameManager gameManager;
  public Button claimButton;
  public TextMeshProUGUI detailsText;

  int[] rewards = {
    50000, //Rent
    25000, //Owned Properties
    15250, //Sold Properties
    5000, //Passed Properties
    16500 //Upgrades
  };

  int[] requirements =  {
    1000, //Rent
    3, //Owned Properties
    5, //Sold Properties
    15, //Passed Properties
    15 //Upgrades
  };

  int activePanel;

  private void Start() {
    activePanel = 0;
  }

  public void OpenAchievementsManager() {
    UpdatePanel(0);
    gameObject.SetActive(true);
  }

  public void ChangePanel(int p) {
    if(activePanel == p) {
      UpdatePanel(p);
      return;
    }

    activePanel = p;
    UpdatePanel(p);
  }
  void UpdatePanel(int p) {
    switch(p) {
      case 0: //Rent
        if(gameManager.GameStats["HighestRental"] >= requirements[0]) claimButton.interactable = true;
        else claimButton.interactable = false;

        detailsText.text =
          $"${gameManager.GameStats["HighestRental"]:#,##0}/${requirements[0]:#,##0}\n\n" +
          $"Reward: ${rewards[0]:#,##0}";
        break;
      case 1: //Owned Properties
        if(gameManager.GameStats["TotalPropertiesOwned"] >= requirements[1]) claimButton.interactable = true;
        else claimButton.interactable = false;

        detailsText.text =
          $"{gameManager.GameStats["TotalPropertiesOwned"]}/{requirements[1]}\n\n" +
          $"Reward: ${rewards[1]:#,##0}";
        break;
      case 2: //Sold Properties
        if(gameManager.GameStats["TotalPropertiesSold"] >= requirements[2]) claimButton.interactable = true;
        else claimButton.interactable = false;

        detailsText.text =
          $"{gameManager.GameStats["TotalPropertiesSold"]}/{requirements[2]}\n\n" +
          $"Reward: ${rewards[2]:#,##0}";
        break;
      case 3: //Passed Properties
        if(gameManager.GameStats["TotalPassedProperties"] >= requirements[3]) claimButton.interactable = true;
        else claimButton.interactable = false;

        detailsText.text =
          $"{gameManager.GameStats["TotalPassedProperties"]}/{requirements[3]}\n\n" +
          $"Reward: ${rewards[3]:#,##0}";
        break;
      case 4: //Upgrades
        if(gameManager.GameStats["TotalUpgrades"] >= requirements[4]) claimButton.interactable = true;
        else claimButton.interactable = false;

        detailsText.text =
          $"{gameManager.GameStats["TotalUpgrades"]}/{requirements[4]}\n\n" +
          $"Reward: ${rewards[4]:#,##0}";
        break;
      default:
        break;
    }
  }
  public void ClaimReward() {
    switch(activePanel) {
      case 0: //RENT
        gameManager.bank.AddMoney(rewards[0], "Subsidy Payment");
        requirements[0] += Random.Range(1000, 3500);
        rewards[0] += Random.Range(1000, 2500);
        break;
      case 1: //OWNED PROPERTIES
        gameManager.bank.AddMoney(rewards[1], "Subsidy Payment");
        requirements[1] += Random.Range(2, 5);
        rewards[1] += Random.Range(5000, 15000);
        break;
      case 2: //SOLD PROPERTIES
        gameManager.bank.AddMoney(rewards[2], "Subsidy Payment");
        requirements[2] += Random.Range(2, 4);
        rewards[2] += Random.Range(1000, 3500);
        break;
      case 3: //PASSED PROPERTIES
        gameManager.bank.AddMoney(rewards[3], "Subsidy Payment");
        requirements[3] += Random.Range(5, 11);
        rewards[3] += Random.Range(2750, 4000);
        break;
      case 4: //UPGRADES
        gameManager.bank.AddMoney(rewards[4], "Subsidy Payment");
        requirements[4] += Random.Range(8, 35);
        rewards[4] += Random.Range(1500, 4560);
        break;
      default:
        break;
    }

    UpdatePanel(activePanel);
  }
}

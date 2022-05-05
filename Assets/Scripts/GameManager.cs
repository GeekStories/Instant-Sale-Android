using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
  public Bank bank;

  public Text[] tenancyTexts;

  public Text netWorthText, rawIncomeText;
  public Text yearsText, weeksPerSecondText, supplyDemandText, weeksLeft;
  public Text primaryStatsText, otherStatsText, finalScoreText;
  public Text cardsLeftText;

  public int rent, score;
  public int cardsLeft, cardsLeftMax;
  public int year, month, week, weeksPerMinute, maxWeeksPerMinute = 0;
  public int networth, addMoneyAmnt;
  public int nextUpgrade = 5;

  public float timeBuffer = 0.75f;
  public float sellBuffer = 0.25f;
  public float supplyDemandIndex = 1.00f;

  public List<GameObject> hiredManagers = new();
  public List<GameObject> managersForHire = new();

  public GameObject[] buyPanels;

  public GameObject card;
  public GameObject gameOverPanel;
  public GameObject managersForHirePanel;
  public GameObject currentManagersPanel;
  public GameObject selectedManagerPanel;
  public GameObject selectedHiredManagerPanel;
  public GameObject propertyManagersPanel;
  public GameObject selectedManager;
  public GameObject selectedHiredManager;

  public AudioClip[] backgroundMusic;
  public AudioClip[] upgradeSounds;
  public AudioClip[] placeCardSounds;

  public AudioClip pickUpCard;
  public AudioClip buySellProperty;
  public AudioClip buttonClick;

  public AudioSource ambientSource;
  public AudioSource source;

  public Image selectedHiredManagerIcon, selectedManagerIcon;

  public Sprite defaultManagerSprite;
  public Sprite musicOn, musicOff, soundOn, soundOff, exclamation, normal;

  public bool music = false, sounds = false, repeatingWeek = false;

  public Dictionary<string, int> deductions = new();
  public Dictionary<string, float> GameStats = new() {
      { "TotalPropertiesOwned", 0 },
      { "TotalMoneySpent", 0 },
      { "TotalPropertiesSold", 0 },
      { "MoneySpentOnUpgrades", 0 },
      { "TotalPassedProperties", 0 },
      { "TotalUpgrades", 0 },
      { "MostExpensiveProperty", 0 },
      { "MostExpensivePurchased", 0 },
      { "HighestRental", 0 },
      { "TotalNumberTenants", 0 },
      { "TotalMonthsStayed", 0 },
      { "HighestNetworth", 0 },
      { "MostAmountOfMoney", 0 }
  };

  public Transform CardPile;

  public Canvas tutorial;

  public Button musicToggle, soundToggle, fireManagerButton;

  private void Start() {
    source = GetComponent<AudioSource>();
    BackgroundMusic();

    rent = 0;
    cardsLeftMax = 5;
    cardsLeft = cardsLeftMax;
    year = 0;

    supplyDemandText.text = $"{supplyDemandIndex:F2}";

    int x = Random.Range(3, 10); // Number of managers to generate
    for(int i = 0; i < x; i++) {
      GameObject newManager = GetComponent<GenerateManagers>().GenerateManager(managersForHirePanel, this);
      managersForHire.Add(newManager);
    }

    NextWeek();
    InvokeRepeating(nameof(RepeatingWeeks), 0, (60 - weeksPerMinute) * timeBuffer);
    InvokeRepeating(nameof(UpdateNetworth), 0, 0.5f);
  }
  public void UpdateNetworth () {
    float networth = Calculate.NetWorth(buyPanels, sellBuffer, supplyDemandIndex, bank.money, bank.Loans);
    netWorthText.text = $"NetWorth: ${networth:#,##0}";

    if(networth > GameStats["HighestNetworth"]) GameStats["HighestNetworth"] = networth;

    float inOut = Calculate.RawIncome(buyPanels, bank.Loans, hiredManagers);

    if(inOut == 0) {
      rawIncomeText.color = Color.grey;
      rawIncomeText.text = $"(${inOut:#,##0})";
    }

    if(inOut > 0) {
      rawIncomeText.color = Color.green;
      rawIncomeText.text = $"(+${inOut:#,##0})";
    } else {
      rawIncomeText.color = Color.red;
      rawIncomeText.text = $"(${inOut:#,##0})";
    }

    GetComponent<Prestiege>().SetUnclaimedPoints(Mathf.RoundToInt(networth / 10000));
  }
  public void BackgroundMusic() {
    CancelInvoke("BackgroundMusic");

    int x = Random.Range(0, 2);
    if(music) source.PlayOneShot(backgroundMusic[x], 0.1f);

    InvokeRepeating(nameof(BackgroundMusic), backgroundMusic[x].length, backgroundMusic[x].length);
  }
  public void CheckPile() {
    if(CardPile.childCount > 0) {
      //Are there any cards currently in the pile?
      Card c = CardPile.GetChild(0).GetComponent<Card>(); //Grab that card
      c.weeksLeft--;

      //Check if we have any weeks left before expiry
      if(c.weeksLeft > 0) {
        cardsLeft = cardsLeftMax - 1;
        weeksLeft.text = $"Expires in {c.weeksLeft} weeks";
      } else {
        c.Destroy(); //No weeks left, destroy the card
        CreateCard();
      }
    } else {
      //No card, so generate one!
      CreateCard();
    }
  }
  public void CreateCard() {
    // Any cards left in the pile?
    if(cardsLeft <= 0) {
      weeksLeft.text = "Out of cards!";
      return;
    }

    GameObject x = Instantiate(card, CardPile);
    int newWeeksLeft = Random.Range(3, 10);
    x.GetComponent<Card>().weeksLeft = newWeeksLeft;
    weeksLeft.text = $"Expires in {newWeeksLeft} weeks";

    x.GetComponent<Card>().assignedManagerImage = defaultManagerSprite;

    cardsLeft--;
    cardsLeftText.text = $"{cardsLeft}/{cardsLeftMax}";
  }
  public void NextWeek() {
    if(sounds) ambientSource.PlayOneShot(buttonClick);

    //Start the next week
    week++;

    if(week == 5) {
      month++;

      //Add random power usage
      foreach(GameObject panel in buyPanels) {
        if(panel.transform.childCount > 1) {
          Card c = panel.transform.GetChild(1).GetComponent<Card>();

          if(c.tenants) c.powerUse = Random.Range(150, 450);
        }
      }

      week = 1;
    }

    if(month == 13) {
      year++;
      month = 0;
    }

    if(year == 50) {
      CalculateScore();
      gameOverPanel.SetActive(true);
    }

    //Every 5 years, you'll gain 1 extra card per week!
    if(year == nextUpgrade) {
      nextUpgrade += 5;
      cardsLeftMax++;
    }

    GameStats["NetWorth"] = networth;
    GameStats["Money"] = bank.money;

    yearsText.text = $"W{week} : M{month} : Y{year}";

    bank.TakeRepayments();
    CheckTenantTerms(); //Checks term amount and deal rent payments
    PayHiredManagers(); // Pay managers

    cardsLeft = cardsLeftMax;
    CheckPile();

    //Start of new week
    supplyDemandIndex += Random.Range(-0.01f, 0.012f);
    supplyDemandText.text = $"{supplyDemandIndex:F2}";
    cardsLeftText.text = $"{cardsLeft}/{cardsLeftMax}";
  }
  public void PayHiredManagers() {
    int totalPayment = 0;
    foreach(GameObject manager in hiredManagers) {
      totalPayment += manager.GetComponent<Manager>().weeklyPay;
    }

    if(totalPayment > 0) bank.AddMoney(-totalPayment, "Manager Payroll");
  }
  public void SelectManager(GameObject manager) {
    Manager m = manager.GetComponent<Manager>();

    if(m.hired) {
      //Manager is an employee, fill out the selectedHiredManagerPanel
      selectedHiredManagerIcon.GetComponent<Image>().sprite = manager.transform.GetChild(0).GetComponent<Image>().sprite;
      selectedHiredManagerPanel.transform.GetChild(1).GetComponent<Text>().text = m.description;
      selectedHiredManager = manager;

      fireManagerButton.interactable = true;
    } else {
      //Manager is NOT an employee, fill out the selectedManagerPanel
      selectedManagerIcon.GetComponent<Image>().sprite = manager.transform.GetChild(0).GetComponent<Image>().sprite;
      selectedManagerPanel.transform.GetChild(1).GetComponent<Text>().text = m.description;

      selectedManager = manager;
    }
  }
  public void HireManager() {
    if(!selectedManager || currentManagersPanel.transform.childCount == 36) return;

    Manager hiredManager = selectedManager.GetComponent<Manager>();

    if(bank.money < hiredManager.hireCost) return;

    bank.AddMoney(-hiredManager.hireCost, "Staff Hire Payment");
    deductions.Add(hiredManager.name, hiredManager.weeklyPay);

    hiredManager.hired = true;

    managersForHire.Remove(selectedManager);

    selectedManager.transform.SetParent(currentManagersPanel.transform);
    selectedManager.GetComponent<Toggle>().group = currentManagersPanel.GetComponent<ToggleGroup>();

    GameObject duplicateManager = Instantiate(selectedManager);
    duplicateManager.transform.SetParent(propertyManagersPanel.transform);
    duplicateManager.name = hiredManager.name;
    duplicateManager.GetComponent<Toggle>().group = propertyManagersPanel.GetComponent<ToggleGroup>();
    duplicateManager.transform.localScale = Vector3.one;

    hiredManagers.Add(duplicateManager);

    if(managersForHire.Count == 0) {
      GameObject newManager = GetComponent<GenerateManagers>().GenerateManager(managersForHirePanel, this);
      managersForHire.Add(newManager);

      SelectManager(managersForHire[0]);
    }
  }
  public void FireManager() {
    // Remove wage from payroll
    deductions.Remove(selectedHiredManager.name);

    // Remove from hired managers panel
    GameObject managerToDelete = GameObject.Find(selectedHiredManager.name);
    Destroy(managerToDelete);

    // Remove from property panel
    Destroy(hiredManagers.Find(g => g.name == selectedHiredManager.name));

    // Remove the manager from the hiredManagers list
    hiredManagers.Remove(hiredManagers.Find(g => g.name == selectedHiredManager.name));

    if(hiredManagers.Count == 0) {
      selectedHiredManagerIcon.GetComponent<Image>().sprite = exclamation;
      selectedHiredManagerPanel.transform.GetChild(1).GetComponent<Text>().text = "Select Manager";
      selectedHiredManager = null;
      return;
    }

    SelectManager(hiredManagers[0]);
  }
  public void CheckTenantTerms() {
    foreach(GameObject panel in buyPanels) {
      if(panel.transform.childCount > 1) {
        Card c = panel.transform.GetChild(1).GetComponent<Card>();
        int propertySlot = int.Parse(panel.name[^1..]) - 1; // Grabs the last character (the panel number)

        if(!c.tenants) {
          tenancyTexts[propertySlot].text = "";
          continue;
        }

        if(week == c.tenantMoveInWeek) {
          c.tenantTermRemaining--;
          tenancyTexts[propertySlot].text = $"{c.tenantTermRemaining}";
          GameStats["TotalMonthsStayed"]++;
        }

        //Check if the tenants term has run out
        if(c.tenantTermRemaining == 0) {
          c.tenants = false;
          c.tenantTerm = 0;
          c.tenantTermRemaining = 0;
          tenancyTexts[propertySlot].text = "";

          bank.AddMoney(-c.bondCost, "Bond Reclaim");

          c.transform.parent.GetComponent<BuyPanel>().openPropertySlotButton.GetComponent<Image>().sprite = exclamation;
          c.transform.parent.GetComponent<BuyPanel>().openPropertySlotButton.GetComponent<Image>().color = Color.red;
          continue;
        } else {
          bank.AddMoney(c.rent, "Rent Income");
        }
      }
    }
  }
  public void RepeatingWeeks() {
    if(weeksPerMinute > 0) NextWeek();
  }
  public void IncreaseWeeksPerSecond() {
    if(weeksPerMinute < maxWeeksPerMinute) {
      weeksPerMinute++;
      weeksPerSecondText.text = $"{weeksPerMinute}";
      CancelInvoke("RepeatingWeeks");
      InvokeRepeating(nameof(RepeatingWeeks), (60 - weeksPerMinute) * timeBuffer, (60 - weeksPerMinute) * timeBuffer);
    }
  }
  public void DecreaseWeeksPerSecond() {
    if(weeksPerMinute > 0) {
      weeksPerMinute--;
      weeksPerSecondText.text = $"{weeksPerMinute}";

      CancelInvoke("RepeatingWeeks");
      InvokeRepeating(nameof(RepeatingWeeks), (60 - weeksPerMinute) * timeBuffer, (60 - weeksPerMinute) * timeBuffer);
    }
  }
  public void PassCard() {
    if(CardPile.childCount > 0) {
      CardPile.GetChild(0).GetComponent<Card>().Destroy();
      CardPile.DetachChildren();

      GameStats["TotalPassedProperties"]++;

      CheckPile();
    }
  }
  public void ToggleSound() {
    sounds = !sounds;

    if(sounds) soundToggle.GetComponent<Image>().sprite = soundOn;
    else soundToggle.GetComponent<Image>().sprite = soundOff;

    if(sounds) ambientSource.PlayOneShot(buttonClick);
  }
  public void ToggleMusic() {
    music = !music;

    if(music) {
      musicToggle.GetComponent<Image>().sprite = musicOn;
      BackgroundMusic();
    } else {
      musicToggle.GetComponent<Image>().sprite = musicOff;
      source.Stop();
    }

    if(sounds) ambientSource.PlayOneShot(buttonClick);
  }
  public void CalculateScore() {
    float score =
        GameStats["TotalPropertiesOwned"]
        + (GameStats["TotalMoneySpent"] / 100000)
        + GameStats["TotalPropertiesSold"]
        + (GameStats["MoneySpentOnUpgrades"] / 100000)
        + GameStats["TotalPassedProperties"]
        + GameStats["TotalUpgrades"]
        + (GameStats["MostExpensiveProperty"] / 100000)
        + (GameStats["MostExpensivePurchased"] / 100000)
        + (GameStats["HighestRental"] / 10000)
        + GameStats["TotalNumberTenants"]
        + GameStats["TotalMonthsStayed"]
        + (GameStats["HighestNetworth"] / 100000)
        + (GameStats["MostAmountOfMoney"] / 100000);

    finalScoreText.text = $"Score: {score:#,##0}";

    primaryStatsText.text =
      $"Properties Owned: {GameStats["TotalPropertiesOwned"]} \n " +
      $"Proprties Sold: {GameStats["TotalPropertiesSold"]} \n " +
      $"Properties Skipped: {GameStats["TotalPassedProperties"]} \n\n" +
      $"Total Spend On Upgrades: {GameStats["MoneySpentOnUpgrades"]:#,##0} \n" +
      $"Total Upgrades Purchased: {GameStats["TotalUpgrades"]}" +
      $"Total Money Spent: {GameStats["TotalMoneySpent"]:#,##0}";

    otherStatsText.text =
      $"Most Expensive Property Owned: {GameStats["MostExpensiveProperty"]:#,##0} \n" +
      $"Most Expensive Property Purchased: {GameStats["MostExpensivePurchased"]:#,##0} \n" +
      $"Highest Rental Cost: {GameStats["HighestRental"]:#,##0} \n" +
      $"Number Of Tenants Housed: {GameStats["TotalNumberTenants"]} \n" +
      $"Combined Months Tenants Have Leased: {GameStats["TotalMonthsStayed"]} \n\n" +
      $"Highest Networth Obtained: {GameStats["HighestNetworth"]:#,##0} \n" +
      $"Most Amount Of Money Had: {GameStats["MostAmountOfMoney"]:#,##0}";

    PlayerPrefs.SetFloat("UnclaimedPoints", score);
  }
  public void ResetGame() {
    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
  }
}

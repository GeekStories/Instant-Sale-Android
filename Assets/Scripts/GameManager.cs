using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
  public Bank bank;
  public PropertyPanel propertyPanel;

  public Text[] tenancyTexts;

  public Text netWorthText, rawIncomeText;
  public Text weekText, monthYearText, weeksPerSecondText, supplyDemandText, weeksLeft;
  public Text primaryStatsText, otherStatsText, finalScoreText;
  public Text cardsLeftText;

  public int rent, score;
  public int cardsLeft, cardsLeftMax;
  public int year = 0, month = 1, week = 1, weeksPerMinute, maxWeeksPerMinute = 0;
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

  public Image selectedHiredManagerIcon, selectedManagerIcon;

  public Sprite defaultManagerSprite;
  public Sprite exclamation, normal;

  public bool repeatingWeek = false;

  public Dictionary<string, int> deductions = new();
  public Dictionary<string, float> GameStats = new() {
    { "TotalPropertiesOwned", 0 },
    { "TotalMoneySpent", 0 },
    { "TotalPropertiesSold", 0 },
    { "TotalValueOfPropertiesSold", 0 },
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

    // NextWeek();
    CheckPile();
    InvokeRepeating(nameof(RepeatingWeeks), 0, (60 - weeksPerMinute) * timeBuffer);
    InvokeRepeating(nameof(UpdateNetworth), 0, 0.5f);
  }
  public void UpdateNetworth() {
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
    //Start the next week
    week++;

    foreach(GameObject panel in buyPanels) {
      if(panel.transform.childCount > 1) {
        Card c = panel.transform.GetChild(1).GetComponent<Card>();
        //Add random water usage
        if(c.tenants) c.waterUsage += Random.Range(15, Random.Range(25, 45));

        // Check renovations
        if(c.underRenovation) {
          if(c.renovationTime == 1) {
            c.underRenovation = false;
            c.baseRent = c.newRent;
            c.newRent = 0;
            c.UpdateRent();
          } else {
            c.renovationTime--;
            c.UpdateRenoTime();
          }
        }

        // Check if listed
        if(c.currentlyListed) {
          c.listingTime--;
          if(c.listingTime > 0) {
            List<Dictionary<string, int>> updatedOffers = new();


            if(c.totalOffersHad < c.estimatedOffers) {
              c.offers.ForEach((Dictionary<string, int> offer) => {
                offer["expires"]--;
                if(offer["expires"] > 0) updatedOffers.Add(offer);
              });

              updatedOffers.Sort((o1, o2) => o1["expires"].CompareTo(o2["expires"]));

              int x = Random.Range(0, c.estimatedOffers / 2);
              c.totalOffersHad += x;
              for(int i = 0; i < x; i++) {
                Dictionary<string, int> newOffer = new();
                newOffer.Add("key", Random.Range(1111, 9999));
                newOffer.Add("amount", Mathf.FloorToInt((c.cost * supplyDemandIndex) + Random.Range(-5000, 15000)));
                newOffer.Add("expires", Random.Range(1, c.listingTime));
                updatedOffers.Add(newOffer);
              }

              c.offers = updatedOffers;
            }
          } else {
            c.currentlyListed = false;
            c.listingBudget = 0;
            c.listingTime = 0;
            c.totalOffersHad = 0;

            c.offers.Clear();

            foreach(Transform child in propertyPanel.offersPanel.transform) {
              child.GetComponent<DestroySelf>().SelfDestruct();
            }
          }
        }
      }
    }

    if(week == 5) {
      month++;
      week = 1;
    }

    if(month == 13) {
      year++;
      month = 1;

      // Charge water rates
      int waterCost = 0;
      foreach(GameObject panel in buyPanels) {
        if(panel.transform.childCount > 1) {
          Card c = panel.transform.GetChild(1).GetComponent<Card>();
          waterCost += propertyPanel.GetWaterCost(c.waterUsage);
          c.waterUsage = 0;
        }

      }
      if(waterCost > 0) bank.AddMoney(-waterCost, $"Water Rates Payment");
    }

    //Every 5 years, you'll gain 1 extra card per week!
    if(year == nextUpgrade) {
      nextUpgrade += 5;
      cardsLeftMax++;
    }

    GameStats["NetWorth"] = networth;
    GameStats["Money"] = bank.money;

    weekText.text = $"Week {week}";
    monthYearText.text = $"{month:00}/{1980 + year}";

    bank.TakeRepayments();
    CheckTenantTerms(); //Checks term amount and deal rent payments
    PayHiredManagers(); // Pay managers

    if(year == 50) {
      CalculateScore();
      gameOverPanel.SetActive(true);
    }

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
          bank.UpdatePropertyIncomes(propertySlot, c.rent);
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
      $"Properties Owned\n{GameStats["TotalPropertiesOwned"]}\n\n" +
      $"Proprties Sold\n{GameStats["TotalPropertiesSold"]} (${GameStats["TotalValueOfPropertiesSold"]})\n\n" +
      $"Properties Skipped\n{GameStats["TotalPassedProperties"]}\n\n" +
      $"Total Upgrades Purchased\n{GameStats["TotalUpgrades"]} (${GameStats["MoneySpentOnUpgrades"]:#,##0})\n\n" +
      $"Total Money Spent\n${GameStats["TotalMoneySpent"]:#,##0}";

    otherStatsText.text =
      $"Most Expensive Property Owned\n${GameStats["MostExpensiveProperty"]:#,##0}\n\n" +
      $"Most Expensive Property Purchased\n${GameStats["MostExpensivePurchased"]:#,##0}\n\n" +
      $"Highest Rental Cost\n${GameStats["HighestRental"]:#,##0}\n\n" +
      $"Number Of Tenants Housed\n{GameStats["TotalNumberTenants"]} (over {GameStats["TotalMonthsStayed"]} months)\n\n" +
      $"Highest Networth Obtained\n${GameStats["HighestNetworth"]:#,##0}\n\n" +
      $"Most Amount Of Money Had\n${GameStats["MostAmountOfMoney"]:#,##0}";

    PlayerPrefs.SetFloat("UnclaimedPoints", score);
  }
  public void ResetGame() {
    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
  }
}

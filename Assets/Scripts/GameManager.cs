using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
  public Bank bank;
  public PropertyPanel propertyPanel;

  public TextMeshProUGUI netWorthText, rawIncomeText, actionPointsText;
  public TextMeshProUGUI weeksPerSecondText, weekText, monthYearText, supplyDemandText;
  public TextMeshProUGUI cardsLeftText, weeksLeft;
  public TextMeshProUGUI primaryStatsText, otherStatsText, finalScoreText;

  public Image[] displayCards;

  public int rent, score;
  public int cardsLeft, cardsLeftMax;
  public int actionPointsMax, actionPoints;
  public int year = 0, month = 1, week = 1, weeksPerMinute, maxWeeksPerMinute = 0;
  public int networth, addMoneyAmnt;
  public int nextUpgrade = 5;

  public int cardMinCost, cardMaxCost, cardMinRent, cardMaxRent;

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
  public GameObject statsPanel;

  public Image selectedHiredManagerIcon, selectedManagerIcon;

  public Sprite defaultManagerSprite;
  public Sprite exclamation, normal;
  public Sprite[] houseImages;

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
    { "MostExpensiveOwned", 0 },
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

    UpdateActionPoints(actionPointsMax);
    // NextWeek();
    CheckPile();
    InvokeRepeating(nameof(RepeatingWeeks), 0, (60 - weeksPerMinute) * timeBuffer);
    InvokeRepeating(nameof(UpdateNetworth), 0, 0.5f);
  }
  public void UpdateNetworth() {

    float networth = Calculate.NetWorth(buyPanels, sellBuffer, supplyDemandIndex, bank.money, bank.Loans);

    netWorthText.text = $"${networth:#,##0}";
    netWorthText.color = networth > 0 ? Color.green : networth < 0 ? Color.red : Color.gray;
    if(networth > GameStats["HighestNetworth"]) GameStats["HighestNetworth"] = networth;


    float inOut = Calculate.RawIncome(buyPanels, bank.Loans, hiredManagers);
    rawIncomeText.color = inOut > 0 ? Color.green : inOut < 0 ? Color.red : Color.gray;
    rawIncomeText.text = inOut > 0 ? $"+${inOut:#,##0}" : inOut < 0 ? $"${inOut:#,##0}" : $"${inOut:#,##0}";

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
        c.cost = Mathf.FloorToInt(c.cost * supplyDemandIndex);
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

    int newWeeksLeft = Random.Range(3, 10);

    GameObject newCardObject = Instantiate(card, CardPile);
    Card newCard = newCardObject.GetComponent<Card>();
    newCard.baseRent = Random.Range(cardMinRent, cardMaxRent);
    newCard.cost = Mathf.FloorToInt(Random.Range(cardMinCost, cardMaxCost) * supplyDemandIndex);
    newCard.weeksLeft = newWeeksLeft;
    newCard.houseImage.sprite = houseImages[Random.Range(0, houseImages.Length - 1)];
    newCard.assignedManagerImage = defaultManagerSprite;

    weeksLeft.text = $"Expires in {newWeeksLeft} weeks";

    // displayCards[cardsLeft - 1].gameObject.SetActive(false);
    cardsLeft--;
    cardsLeftText.text = $"{cardsLeft}/{cardsLeftMax}";

  }
  public void NextWeek() {
    //Start the next week
    week++;
    foreach(GameObject panel in buyPanels) {
      PropertySlot ps = panel.GetComponent<PropertySlot>();
      if(ps.DropZone.childCount > 0) {
        Card c = ps.DropZone.GetChild(0).GetComponent<Card>();

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

      bank.AddMoney(848, "Job Payment");
      bank.AddMoney(-300, "Couch Rent");
      bank.AddMoney(-270, "Living Costs");
    }

    if(month == 13) {
      year++;
      month = 1;

      // Charge water rates
      int waterCost = 0;
      foreach(GameObject panel in buyPanels) {
        PropertySlot ps = panel.GetComponent<PropertySlot>();
        if(ps.DropZone.childCount > 0) {
          Card c = ps.DropZone.GetChild(0).GetComponent<Card>();
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

    weekText.text = $"W{week}";
    monthYearText.text = $"{month:00}/{1980 + year}";

    bank.TakeRepayments();
    CheckTenantTerms(); //Checks term amount and deal rent payments
    PayHiredManagers(); // Pay managers

    if(year == 50) {
      CalculateScore();
      gameOverPanel.SetActive(true);
    }

    actionPoints = actionPointsMax;
    actionPointsText.text = $"{actionPoints} / {actionPointsMax}";

    cardsLeft = cardsLeftMax;

    //    for(int i = 0; i < displayCards.Length - 1; i++) {
    //      displayCards[i].gameObject.SetActive(true);
    //    }

    CheckPile();

    //Start of new week
    supplyDemandIndex += Random.Range(-0.00005f, 0.00007f);

    foreach(GameObject panel in buyPanels) {
      PropertySlot ps = panel.GetComponent<PropertySlot>();
      if(ps.DropZone.childCount > 0) {
        Card c = ps.DropZone.GetChild(0).GetComponent<Card>();
        c.cost = Mathf.FloorToInt(c.cost * supplyDemandIndex);

        // Check for property value high score
        if(c.cost > GameStats["MostExpensiveOwned"]) {
          GameStats["MostExpensiveOwned"] = Mathf.FloorToInt(c.cost * supplyDemandIndex);
        }

        c.UpdateRent();
      }
    }

    supplyDemandText.text = $"{supplyDemandIndex:F4}";
    cardsLeftText.text = $"{cardsLeft}/{cardsLeftMax}";
  }
  public void UpdateActionPoints(int amount) {
    actionPoints += amount;
    actionPointsText.text = $"{actionPoints} / {actionPointsMax}";
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
    if(!selectedManager || currentManagersPanel.transform.childCount == 36 || actionPoints == 0) return;

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
    UpdateActionPoints(-1);

    if(managersForHire.Count == 0) {
      GameObject newManager = GetComponent<GenerateManagers>().GenerateManager(managersForHirePanel, this);
      managersForHire.Add(newManager);

      SelectManager(managersForHire[0]);
    }
  }
  public void FireManager() {
    if(actionPoints == 0) return;

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

    UpdateActionPoints(-1);
    SelectManager(hiredManagers[0]);
  }
  public void CheckTenantTerms() {
    foreach(GameObject panel in buyPanels) {
      PropertySlot ps = panel.GetComponent<PropertySlot>();
      if(ps.DropZone.childCount > 0) {
        Card c = ps.DropZone.GetChild(0).GetComponent<Card>();
        string number = panel.name[^2..]; // Last 2 digits of panel name ie 12 or _3
        int propertySlot = int.Parse(number.Replace("_", "")) - 1;

        if(!c.tenants) {
          ps.tenancyTermText.text = "";

          // Assigned manager AND no current lease, auto lease!!
          if(c.assignedManager != "") {
            propertyPanel.card = c;
            propertyPanel.GenerateTenancy();
            propertyPanel.card = null;

            ps.openPropertySlotButton.GetComponent<Image>().sprite = normal;
            ps.openPropertySlotButton.GetComponent<Image>().color = Color.white;

            ps.tenancyTermText.text = c.tenantTerm.ToString();
          }
          continue;
        }

        if(week == c.tenantMoveInWeek) {
          if(c.tenantTermRemaining > 0) {
            bank.AddMoney(c.rent, "Rent Income");
            bank.UpdatePropertyIncomes(propertySlot, c.rent);
          }

          c.tenantTermRemaining--;
          ps.tenancyTermText.text = $"{c.tenantTermRemaining}";
          GameStats["TotalMonthsStayed"]++;
        }

        //Check if the tenants term has run out
        if(c.tenantTermRemaining == 0) {
          c.tenants = false;
          c.tenantTerm = 0;
          c.tenantTermRemaining = 0;
          ps.tenancyTermText.text = "";

          bank.AddMoney(-c.bondCost, "Bond Reclaim");

          ps.openPropertySlotButton.GetComponent<Image>().sprite = exclamation;
          ps.openPropertySlotButton.GetComponent<Image>().color = Color.red;
          continue;
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
  public float CalculateScore() {
    return
        GameStats["TotalPropertiesOwned"]
        + (GameStats["TotalMoneySpent"] / 100000)
        + GameStats["TotalPropertiesSold"]
        + (GameStats["MoneySpentOnUpgrades"] / 100000)
        + GameStats["TotalPassedProperties"]
        + GameStats["TotalUpgrades"]
        + (GameStats["MostExpensiveOwned"] / 100000)
        + (GameStats["MostExpensivePurchased"] / 100000)
        + (GameStats["HighestRental"] / 10000)
        + GameStats["TotalNumberTenants"]
        + GameStats["TotalMonthsStayed"]
        + (GameStats["HighestNetworth"] / 100000)
        + (GameStats["MostAmountOfMoney"] / 100000);
  }

  public void OpenStatsPanel() {
    float score = CalculateScore();
    finalScoreText.text = $"Score: {score:#,##0}";

    primaryStatsText.text =
      $"Properties Owned\n{GameStats["TotalPropertiesOwned"]}\n\n" +
      $"Proprties Sold\n{GameStats["TotalPropertiesSold"]} (${GameStats["TotalValueOfPropertiesSold"]})\n\n" +
      $"Properties Skipped\n{GameStats["TotalPassedProperties"]}\n\n" +
      $"Total Upgrades Purchased\n{GameStats["TotalUpgrades"]} (${GameStats["MoneySpentOnUpgrades"]:#,##0})\n\n" +
      $"Total Money Spent\n${GameStats["TotalMoneySpent"]:#,##0}";

    otherStatsText.text =
      $"Most Expensive Property Owned\n${GameStats["MostExpensiveOwned"]:#,##0}\n\n" +
      $"Most Expensive Property Purchased\n${GameStats["MostExpensivePurchased"]:#,##0}\n\n" +
      $"Highest Rental Cost\n${GameStats["HighestRental"]:#,##0}\n\n" +
      $"Number Of Tenants Housed\n{GameStats["TotalNumberTenants"]} (over {GameStats["TotalMonthsStayed"]} months)\n\n" +
      $"Highest Networth Obtained\n${GameStats["HighestNetworth"]:#,##0}\n\n" +
      $"Most Amount Of Money Had\n${GameStats["MostAmountOfMoney"]:#,##0}";

    statsPanel.SetActive(true);
  }
  public void ResetGame() {
    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
  }
}

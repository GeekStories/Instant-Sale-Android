using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropertyPanel : MonoBehaviour {
  public GameManager gameManager;
  public GameObject sellCoverPanel, tenantsCoverPanel, renovationCoverPanel;
  public PropertySlot[] propertySlots;
  public GameObject offersPanelContainer, offersPanel, offerObject;
  public GameObject PropertyManagersBoxPanel;

  public Slider renovationBudgetSlider;
  public Slider listingBudgetSlider, listingTimeSlider;

  public TextMeshProUGUI tenancyTermText, tenancyRiskText, propertyBondAmount, tenancyEmptyText;
  public Text renovationCostText, renovationTimeText, rentIncreaseFromRenovationText;
  public Text listingBudgetText, listingTimeText, listingOffersEstimateText, offersPanelTimeLeftText;

  public TextMeshProUGUI valueText, rentText, slotBonusText, managerBonusText, waterUsageText;

  public Button renovateButton, sellButton, findNewTenantButton;

  public Card card = null;

  public Sprite defaultManagerSprite;

  public int currentOffer = 0, activeSlot, renovationBudget;
  public float upgradeRentValueModifier = 0.05f, waterRate = 1.35f;

  PropertySlot selectedSlot;

  public void OpenPropertyPanel(int slotNumber) {
    gameObject.SetActive(!gameObject.activeInHierarchy); // Show or hide the property panel
    if(!gameObject.activeInHierarchy) return;

    activeSlot = slotNumber;
    selectedSlot = propertySlots[slotNumber - 1];

    sellCoverPanel.SetActive(false);
    tenantsCoverPanel.SetActive(false);
    renovationCoverPanel.SetActive(false);
    offersPanelContainer.SetActive(false);

    card = (selectedSlot.DropZone.childCount > 0) ? selectedSlot.DropZone.GetChild(0).GetComponent<Card>() : null;

    if(card == null) {
      renovationCostText.text = "No property in current slot!";
      renovationTimeText.text = "";
      rentIncreaseFromRenovationText.text = "";

      renovateButton.interactable = false;
      renovationBudgetSlider.interactable = false;

      sellButton.interactable = false;

      tenancyEmptyText.text = "C";

      tenancyTermText.text = "";
      tenancyRiskText.text = "";
      propertyBondAmount.text = "";

      findNewTenantButton.interactable = false;
      ClearManager();

      return;
    }

    // Block all the panels if the card is under construction
    if(card.underRenovation) {
      tenantsCoverPanel.SetActive(true);
      tenantsCoverPanel.transform.GetChild(0).GetComponent<Text>().text = "Currently renovating..";

      sellCoverPanel.SetActive(true);
      sellCoverPanel.transform.GetChild(0).GetComponent<Text>().text = "Currently renovating..";

      renovationCoverPanel.SetActive(true);
      renovationCoverPanel.transform.GetChild(0).GetComponent<Text>().text = $"Under Renovation\n{card.renovationTime} weeks left..";
    }

    if(card.currentlyListed) {
      offersPanelContainer.SetActive(true);

      renovationCoverPanel.SetActive(true);
      renovationCoverPanel.transform.GetChild(0).GetComponent<Text>().text = "Currently listed on market";
      tenantsCoverPanel.SetActive(true);
      tenantsCoverPanel.transform.GetChild(0).GetComponent<Text>().text = "Currently listed on market";

      if(offersPanel.transform.childCount > 0) {
        foreach(Transform child in offersPanel.transform) {
          child.GetComponent<DestroySelf>().SelfDestruct();
        }
      }

      card.offers.Sort((o1, o2) => o1["expires"].CompareTo(o2["expires"]));
      offersPanelTimeLeftText.text = $"{card.listingTime} weeks left | {card.offers.Count} offers";
      foreach(Dictionary<string, int> offer in card.offers) {
        GameObject newOfferObject = Instantiate(offerObject, offersPanel.transform);
        newOfferObject.name = "offer_" + offer["key"];
        newOfferObject.transform.GetChild(0).GetComponent<Text>().text = $"${offer["amount"]:#,##0} (%{CalculateDifferenceAsPercentage(card.purchasePrice, offer["amount"]):F2})\nExpires in {offer["expires"]} weeks";
        newOfferObject.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate () { RejectOffer(offer["key"]); });
        newOfferObject.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate () { AcceptOffer(offer["key"]); });
      }
    }

    // Can we get tenants?
    findNewTenantButton.interactable = !card.tenants;

    sellButton.interactable = !card.tenants;
    if(gameManager.bank.money > 10000) {
      listingBudgetSlider.maxValue = gameManager.bank.money;
    } else {
      listingBudgetSlider.maxValue = 10000;
      sellButton.interactable = false;
    }

    renovateButton.interactable = !card.tenants;
    renovationBudgetSlider.interactable = true;
    if(gameManager.bank.money > 1000000) {
      renovationBudgetSlider.maxValue = 1000000;
    } else if(gameManager.bank.money > 100000) {
      renovationBudgetSlider.maxValue = gameManager.bank.money;
    } else {
      renovationBudgetSlider.maxValue = 100000;
    }

    UpdateRenovationText();
    UpdateTenantsText();
    UpdatePropertyDetailsText();
  }
  public void AssignManager(GameObject newManager) {
    if(selectedSlot.assignedManager != null) ClearManager();

    //Assign the new, selected manager
    if(card != null) card.ChangeBonus("manager_bonus", (newManager.GetComponent<Manager>().bonusAmount));

    newManager.transform.GetChild(2).gameObject.SetActive(true);
    newManager.GetComponent<Toggle>().interactable = false;

    selectedSlot.assignedManager = newManager;
    selectedSlot.managerImage.sprite = newManager.transform.GetChild(0).GetComponent<Image>().sprite;

    UpdatePropertyDetailsText();
  }
  public void ClearManager() {
    foreach(Transform manager in PropertyManagersBoxPanel.transform) {
      if(manager.name == selectedSlot.assignedManager.GetComponent<Manager>().name) {
        manager.GetComponent<Toggle>().isOn = true;
        manager.transform.GetChild(2).gameObject.SetActive(false);
        manager.GetComponent<Toggle>().interactable = true;
      }
    }

    if(card != null) card.ChangeBonus("manager_bonus", 1.00f);
    selectedSlot.assignedManager = null;
    selectedSlot.managerImage.sprite = defaultManagerSprite;

    UpdatePropertyDetailsText();
  }
  public void UpdatePropertyDetailsText() {
    if(card != null) {
      // string tenants = (card.tenants) ? "Yes" : card.underRenovation ? "(unable to lease while renovating)" : "No";
      string slot = $"{(card.bonuses["panel_bonus"] == 0.00f ? "None" : $"%{((card.bonuses["panel_bonus"] - 1) * 100):F2}")}";
      int waterCost = GetWaterCost(card.waterUsage);
      int totalInvested = card.purchasePrice + card.spentOnUpgrades + card.spentOnWaterRates;
      int totalReturned = card.cost - card.purchasePrice + card.totalRentCollected + card.totalRentInAdvance;
      float ROI = CalculateDifferenceAsPercentage(totalInvested, totalReturned);

      valueText.text = $"${card.cost:#,##0} (%{ROI:F2})";
      rentText.text = $"${card.rent:#,##0}";
      slotBonusText.text = $"{slot}";
      waterUsageText.text = $"{card.waterUsage:#,##0}L (${waterCost})";
    }

    string managerBonus = $"{(selectedSlot.assignedManager == null ? "None" : $"%{(selectedSlot.assignedManager.GetComponent<Manager>().bonusAmount - 1) * 100:F2}")}";
    managerBonusText.text = $"{managerBonus}";
  }
  public void PurchaseUpgrade() {
    if(gameManager.bank.money < renovationBudget || card.tenants || gameManager.actionPoints == 0) return;

    // Deduct the money (Check for new high score)
    gameManager.bank.AddMoney(-renovationBudget, "Property Renovation");
    gameManager.GameStats["MoneySpentOnUpgrades"] += renovationBudget;
    card.spentOnUpgrades += renovationBudget;

    // Increase the value of the property
    card.cost += Mathf.FloorToInt(renovationBudget * 1.15f);

    // Update the cards text
    card.newRent = GetRenovationRentIncrease(renovationBudget) + card.rent;
    card.baseRent = 0;

    // Set underRenovations to true
    card.underRenovation = true;
    card.renovationTime = GetRenovationTime(renovationBudget);
    card.UpdateRenoTime();

    // Block all the panels
    sellCoverPanel.SetActive(true);

    tenantsCoverPanel.SetActive(true);
    tenantsCoverPanel.transform.GetChild(0).GetComponent<Text>().text = "Currently renovating..";

    renovationCoverPanel.SetActive(true);
    renovationCoverPanel.transform.GetChild(0).GetComponent<Text>().text = $"Under Renovation\n{card.renovationTime} weeks left..";

    UpdateRenovationText();
    UpdatePropertyDetailsText();

    gameManager.GameStats["TotalUpgrades"]++;
    gameManager.UpdateActionPoints(-1);

    // Check for rent value high score
    if(card.rent > gameManager.GameStats["HighestRental"]) {
      gameManager.GameStats["HighestRental"] = card.rent;
    }

    // Check for property value high score
    if(card.cost > gameManager.GameStats["MostExpensiveOwned"]) {
      gameManager.GameStats["MostExpensiveOwned"] = Mathf.FloorToInt(card.cost * gameManager.supplyDemandIndex);
    }
  }
  public void ListProperty() {
    int cost = (int)listingBudgetSlider.value;
    int time = (int)listingTimeSlider.value;

    if(cost > gameManager.bank.money || gameManager.actionPoints == 0) return;

    gameManager.bank.AddMoney(-cost, "Property Listing Costs");
    gameManager.GameStats["TotalMoneySpent"] += cost;

    card.currentlyListed = true;
    card.listingTime = time;
    card.listingBudget = cost;
    card.estimatedOffers = Mathf.FloorToInt(((int)listingBudgetSlider.value * 1.5f / 10000 * gameManager.supplyDemandIndex) + time * 1.5f);
    offersPanelTimeLeftText.text = $"{time} weeks left";
    offersPanelContainer.SetActive(true);

    renovationCoverPanel.SetActive(true);
    renovationCoverPanel.transform.GetChild(0).GetComponent<Text>().text = "Currently listed on market";
    tenantsCoverPanel.SetActive(true);
    tenantsCoverPanel.transform.GetChild(0).GetComponent<Text>().text = "Currently listed on market";

    gameManager.UpdateActionPoints(-1);
  }
  public void UpdateRenovationText() {
    renovationBudget = (int)renovationBudgetSlider.value;
    int renovationTime = GetRenovationTime(renovationBudget);
    renovationCostText.text = $"${renovationBudget:#,##0}";

    renovationTimeText.text = $"{renovationTime} {(renovationTime == 1 ? "week" : "weeks")}";

    int renovationRentIncrease = GetRenovationRentIncrease(renovationBudget);
    rentIncreaseFromRenovationText.text = $"Base Rent\n${card.baseRent:#,##0} -> ${card.baseRent + renovationRentIncrease:#,##0}\n+${renovationRentIncrease}";
  }
  public void UpdateListingText() {
    listingBudgetText.text = $"Budget: ${listingBudgetSlider.value:#,##0}";
    listingTimeText.text = $"Open for {listingTimeSlider.value} weeks";
    listingOffersEstimateText.text = $"~{GetNumberOfPotentialOffers((int)listingBudgetSlider.value, (int)listingTimeSlider.value, gameManager.supplyDemandIndex)} offers";
  }
  public void UpdateTenantsText() {
    tenancyEmptyText.text = "";
    tenancyTermText.text = $"{card.tenantTermRemaining}/{card.tenantTerm} months";
    tenancyRiskText.text = "Low";
    propertyBondAmount.text = $"${card.bondCost:#,##0}";
  }
  public void AcceptOffer(int key) {
    if(gameManager.actionPoints == 0) return;

    Dictionary<string, int> acceptedOffer = card.offers.Find(offer => offer["key"] == key);
    gameManager.bank.AddMoney(acceptedOffer["amount"], "Property Sale");
    gameManager.GameStats["TotalPropertiesSold"]++;
    card.Destroy();
    OpenPropertyPanel(0);
    GameObject.Find("OpenPropertyPanel_" + selectedSlot.name[^1]).GetComponent<Image>().sprite = gameManager.normal;
    GameObject.Find("OpenPropertyPanel_" + selectedSlot.name[^1]).GetComponent<Image>().color = Color.white;

    if(offersPanel.transform.childCount > 0) {
      foreach(Transform child in offersPanel.transform) {
        child.GetComponent<DestroySelf>().SelfDestruct();
      }
    }

    gameManager.UpdateActionPoints(-1);
  }
  public void RejectOffer(int key) {
    Dictionary<string, int> rejectedOffer = card.offers.Find(offer => offer["key"] == key);
    card.offers.Remove(rejectedOffer);
    Transform item = offersPanel.transform.Find("offer_" + key);
    item.gameObject.GetComponent<DestroySelf>().SelfDestruct();
  }
  public void FindTenant() {
    if(gameManager.actionPoints == 0) return;

    GenerateTenancy();

    findNewTenantButton.interactable = false;
    renovateButton.interactable = false;
    renovationBudgetSlider.interactable = false;
    findNewTenantButton.interactable = false;
    sellButton.interactable = false;

    PropertySlot ps = GameObject.Find("PropertySlot_" + activeSlot).GetComponent<PropertySlot>();

    ps.openPropertySlotButton.GetComponent<Image>().sprite = gameManager.normal;
    ps.openPropertySlotButton.GetComponent<Image>().color = Color.white;

    UpdateTenantsText();
    UpdatePropertyDetailsText();

    ps.tenancyTermText.text = card.tenantTerm.ToString();
    gameManager.UpdateActionPoints(-1);
  }

  public void GenerateTenancy() {
    card.tenants = true;
    card.tenantTerm = Random.Range(1, 7) * 3;
    card.tenantTermRemaining = card.tenantTerm;
    card.tenantMoveInWeek = gameManager.week;

    gameManager.bank.AddMoney(card.rent * 4, "Bond Payment");
    gameManager.bank.UpdatePropertyIncomes(activeSlot - 1, card.bondCost);

    card.bondCost = card.rent * 4;

    gameManager.bank.AddMoney(card.rent * 4, "Rent in Advance Payment");
    gameManager.bank.UpdatePropertyIncomes(activeSlot - 1, card.rent * 4);

    card.totalRentInAdvance += card.rent * 4;
    gameManager.GameStats["TotalNumberTenants"] += Random.Range(1, 5);
  }
  public int GetWaterCost(int usage) {
    return usage > 1000 ? Mathf.FloorToInt(waterRate * (usage - 1000)) : 0;
  }

  float CalculateDifferenceAsPercentage(int initialValue, int currentValue) {
    if(initialValue == currentValue) return 0;

    float difference = currentValue - initialValue;
    return (difference / initialValue) * 100;
  }
  int GetRenovationTime(int budget) {
    return Mathf.FloorToInt(budget / 5000) + 1;
  }
  int GetNumberOfPotentialOffers(int budget, int time, float supplyDemand) {
    return Mathf.FloorToInt((budget * 1.5f / 10000 * supplyDemand) + time * 1.5f);
  }
  int GetRenovationRentIncrease(int budget) {
    int x = Mathf.FloorToInt(budget / (950 * gameManager.supplyDemandIndex));
    return x;
  }
}

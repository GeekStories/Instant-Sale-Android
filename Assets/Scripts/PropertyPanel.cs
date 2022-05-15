using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropertyPanel : MonoBehaviour {
  public GameManager gameManager;
  public GameObject selectedPanel, sellCoverPanel, tenantsCoverPanel, renovationCoverPanel;
  public GameObject offersPanelContainer, offersPanel, offerObject;
  public GameObject PropertyManagersBoxPanel;

  public Slider renovationBudgetSlider;
  public Slider listingBudgetSlider, listingTimeSlider;

  public Text tenantText;
  public Text propertyDetailsText, renovationCostText, renovationTimeText, rentIncreaseFromRenovationText;
  public Text listingBudgetText, listingTimeText, listingOffersEstimateText, offersPanelTimeLeftText;

  public Button renovateButton, sellButton, findNewTenantButton;

  public Card card = null;

  public int currentOffer = 0, activeSlot, renovationBudget;
  public float upgradeRentValueModifier = 0.05f, waterRate = 1.35f;
  public void OpenPropertyPanel(string name) {
    gameObject.SetActive(!gameObject.activeInHierarchy); // Show or hide the property panel

    if(!gameObject.activeInHierarchy) return;

    GameObject propertySlot = GameObject.Find("PropertySlot_" + name);
    selectedPanel = propertySlot;
    activeSlot = int.Parse(name);

    sellCoverPanel.SetActive(false);
    tenantsCoverPanel.SetActive(false);
    renovationCoverPanel.SetActive(false);
    offersPanelContainer.SetActive(false);

    card = (propertySlot.transform.childCount > 1) ? propertySlot.GetComponentInChildren<Card>() : null;

    if(card == null) {
      renovationCostText.text = "No property in current slot!";
      renovationTimeText.text = "";
      rentIncreaseFromRenovationText.text = "";

      renovateButton.interactable = false;
      renovationBudgetSlider.interactable = false;

      sellButton.interactable = false;

      tenantText.text = "No property available!";
      findNewTenantButton.interactable = false;

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
        newOfferObject.transform.GetChild(0).GetComponent<Text>().text = $"${offer["amount"]:#,##0} (%{CalculateDifference(card.purchasePrice, offer["amount"]):F2})\nExpires in {offer["expires"]} weeks";
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

    UpdateRenovationText();
    UpdateTenantsText();
    UpdatePropertyDetailsText();
  }
  public void AssignManager(GameObject newManager) {
    if(card.assignedManager != "") ClearManager();

    //Assign the new, selected manager
    card.assignedManager = newManager.name;
    card.ChangeBonus("manager_bonus", (newManager.GetComponent<Manager>().bonusAmount));
    newManager.transform.GetChild(2).gameObject.SetActive(true);
    Debug.Log("Assigned manager: " + newManager.name);

    newManager.GetComponent<Toggle>().interactable = false;

    card.assignedManagerImage = newManager.transform.GetChild(0).GetComponent<Image>().sprite;
    selectedPanel.GetComponent<DropZone>().managerImage.sprite = card.assignedManagerImage;

    UpdatePropertyDetailsText();
  }
  public void ClearManager() {
    foreach(Transform manager in PropertyManagersBoxPanel.transform) {
      if(manager.name == card.assignedManager) {
        manager.GetComponent<Toggle>().isOn = false;
        manager.transform.GetChild(2).gameObject.SetActive(false);
        manager.GetComponent<Toggle>().interactable = true;
      }
    }

    card.ChangeBonus("manager_bonus", 1.00f);
    card.assignedManager = "";
    card.assignedManagerImage = gameManager.defaultManagerSprite;
    selectedPanel.GetComponent<DropZone>().managerImage.sprite = gameManager.defaultManagerSprite;

    UpdatePropertyDetailsText();
  }
  public void UpdatePropertyDetailsText() {
    // string tenants = (card.tenants) ? "Yes" : card.underRenovation ? "(unable to lease while renovating)" : "No";
    string slot = $"{(card.bonuses["panel_bonus"] == 0.00f ? "None" : $"%{((card.bonuses["panel_bonus"] - 1) * 100):F2}")}";
    string managerBonus = $"{(card.assignedManager == "" ? "None" : $"%{(GameObject.Find(card.assignedManager).GetComponent<Manager>().bonusAmount - 1) * 100:F2}")}";
    int waterCost = GetWaterCost(card.waterUsage);

    propertyDetailsText.text =
      $"-=Info=-\n" +
      $"Value: ${card.cost:#,##0}\n" +
      $"Rent: ${card.rent}\n" +
      $"Base Rent: ${card.baseRent:#,##0}\n\n" +
      $"-=Bonuses=-\n" +
      $"Slot: {slot}\n" +
      $"Manager: {managerBonus}\n\n" +
      $"-=Misc=-\n" +
      $"Water Usage\n{card.waterUsage:#,##0} L (${waterCost})";
  }
  public void PurchaseUpgrade() {
    if(gameManager.bank.money < renovationBudget || card.tenants) return;

    // Deduct the money (Check for new high score)
    gameManager.bank.AddMoney(-renovationBudget, "Property Renovation");
    gameManager.GameStats["MoneySpentOnUpgrades"] += renovationBudget;

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

    // Check for high score
    if(card.rent > gameManager.GameStats["HighestRental"]) gameManager.GameStats["HighestRental"] = card.rent;
    if(card.cost > gameManager.GameStats["MostExpensiveProperty"]) gameManager.GameStats["MostExpensiveProperty"] = card.cost;
  }
  public void ListProperty() {
    int cost = (int)listingBudgetSlider.value;
    int time = (int)listingTimeSlider.value;

    if(cost > gameManager.bank.money) return;

    gameManager.bank.AddMoney(-cost, "Property Listing Costs");
    gameManager.GameStats["TotalMoneySpent"] += cost;

    card.currentlyListed = true;
    card.listingTime = time;
    card.listingBudget = (int)listingBudgetSlider.value;
    card.estimatedOffers = Mathf.FloorToInt(((int)listingBudgetSlider.value * 1.5f / 10000 * gameManager.supplyDemandIndex) + time * 1.5f);
    offersPanelTimeLeftText.text = $"{time} weeks left";
    offersPanelContainer.SetActive(true);

    renovationCoverPanel.SetActive(true);
    renovationCoverPanel.transform.GetChild(0).GetComponent<Text>().text = "Currently listed on market";
    tenantsCoverPanel.SetActive(true);
    tenantsCoverPanel.transform.GetChild(0).GetComponent<Text>().text = "Currently listed on market";
  }
  public void UpdateRenovationText() {
    renovationBudget = (int)renovationBudgetSlider.value;
    int renovationTime = GetRenovationTime(renovationBudget);
    renovationCostText.text = $"${renovationBudget:#,##0}";

    renovationTimeText.text = $"{renovationTime} {(renovationTime == 1 ? "week" : "weeks")}";

    int renovationRentIncrease = GetRenovationRentIncrease(renovationBudget);
    rentIncreaseFromRenovationText.text = $"Base Rent\n${card.baseRent:#,##0} -> ${card.baseRent + renovationRentIncrease:#,##0}";
  }
  public void UpdateListingText() {
    listingBudgetText.text = $"Budget: ${listingBudgetSlider.value:#,##0}";
    listingTimeText.text = $"Open for {listingTimeSlider.value} weeks";
    listingOffersEstimateText.text = $"~{GetNumberOfPotentialOffers((int)listingBudgetSlider.value, (int)listingTimeSlider.value, gameManager.supplyDemandIndex)} offers";
  }
  public void UpdateTenantsText() {
    tenantText.text =
      $"Current Tenants: {card.tenants}\n" +
      $"Lease Term: {card.tenantTermRemaining}/{card.tenantTerm} Months\n" +
      $"Tenant Risk: Low\n" +
      $"Bond: ${card.bondCost:#,##0}";
  }
  public void AcceptOffer(int key) {
    Dictionary<string, int> acceptedOffer = card.offers.Find(offer => offer["key"] == key);
    gameManager.bank.AddMoney(acceptedOffer["amount"], "Property Sale");
    gameManager.GameStats["TotalPropertiesSold"]++;
    card.Destroy();
    OpenPropertyPanel("none");
    GameObject.Find("OpenPropertyPanel_" + selectedPanel.name[^1]).GetComponent<Image>().sprite = gameManager.normal;
    GameObject.Find("OpenPropertyPanel_" + selectedPanel.name[^1]).GetComponent<Image>().color = Color.white;

    if(offersPanel.transform.childCount > 0) {
      foreach(Transform child in offersPanel.transform) {
        child.GetComponent<DestroySelf>().SelfDestruct();
      }
    }
  }
  public void RejectOffer(int key) {
    Dictionary<string, int> rejectedOffer = card.offers.Find(offer => offer["key"] == key);
    card.offers.Remove(rejectedOffer);
    Transform item = offersPanel.transform.Find("offer_" + key);
    item.gameObject.GetComponent<DestroySelf>().SelfDestruct();
  }
  public void FindTenant() {
    GenerateTenancy();

    findNewTenantButton.interactable = false;
    renovateButton.interactable = false;
    renovationBudgetSlider.interactable = false;
    findNewTenantButton.interactable = false;
    sellButton.interactable = false;

    GameObject.Find("OpenPropertyPanel_" + selectedPanel.name[^1]).GetComponent<Image>().sprite = gameManager.normal;
    GameObject.Find("OpenPropertyPanel_" + selectedPanel.name[^1]).GetComponent<Image>().color = Color.white;

    UpdateTenantsText();
    UpdatePropertyDetailsText();

    gameManager.tenancyTexts[activeSlot - 1].text = card.tenantTerm.ToString();
  }

  public void GenerateTenancy() {
    card.tenants = true;
    card.tenantTerm = Random.Range(1, 7) * 3;
    card.tenantTermRemaining = card.tenantTerm;
    card.tenantMoveInWeek = gameManager.week;
    gameManager.bank.AddMoney(card.rent * 4, "Bond Payment");
    card.bondCost = card.rent * 4;
    gameManager.bank.AddMoney(card.rent * 2, "Rent in Advance Payment");
    gameManager.GameStats["TotalNumberTenants"] += Random.Range(1, 5);
  }
  public int GetWaterCost(int usage) {
    return usage > 1000 ? Mathf.FloorToInt(waterRate * (usage - 1000)) : 0;
  }

  float CalculateDifference(int initialValue, int currentValue) {
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
    int x = Mathf.FloorToInt(budget / 2500) + 10;
    return x;
  }
}

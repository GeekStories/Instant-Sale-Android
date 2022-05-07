using UnityEngine;
using UnityEngine.UI;

public class PropertyPanel : MonoBehaviour {
  public GameManager gameManager;
  public GameObject selectedPanel, sellCoverPanel, tenantsCoverPanel, renovationCoverPanel;
  public GameObject PropertyManagersBoxPanel;

  public Slider renovationBudgetSlider;

  public Text currentOfferText, tenantText;
  public Text propertyDetailsText, renovationCostText, renovationTimeText, rentIncreaseFromRenovationText;

  public Button renovateButton, sellButton, findNewTenantButton;

  public Card card = null;

  public int currentOffer = 0, activeSlot, renovationBudget;
  public float upgradeRentValueModifier = 0.05f, waterRate = 1.35f;
  public void OpenPropertyPanel(string name) {
    gameObject.SetActive(!gameObject.activeInHierarchy); // Show the property panel

    if(gameObject.activeInHierarchy == false) return;

    GameObject propertySlot = GameObject.Find("PropertySlot_" + name);
    selectedPanel = propertySlot;
    activeSlot = int.Parse(name);

    sellCoverPanel.SetActive(false);
    tenantsCoverPanel.SetActive(false);
    renovationCoverPanel.SetActive(false);

    card = (propertySlot.transform.childCount > 1) ? propertySlot.GetComponentInChildren<Card>() : null;

    if(card == null) {
      renovationCostText.text = "No property in current slot!";
      renovationTimeText.text = "";
      rentIncreaseFromRenovationText.text = "";

      renovateButton.interactable = false;
      renovationBudgetSlider.interactable = false;

      currentOfferText.text = "No property available!";
      sellButton.interactable = false;

      tenantText.text = "No property available!";
      findNewTenantButton.interactable = false;

      return;
    }

    renovateButton.interactable = true;
    renovationBudgetSlider.interactable = true;
    findNewTenantButton.interactable = true;

    // Block all the panels if the card is under construction
    if(card.underRenovation) {
      sellCoverPanel.SetActive(true);
      tenantsCoverPanel.SetActive(true);
      renovationCoverPanel.SetActive(true);
      renovationCoverPanel.transform.GetChild(0).GetComponent<Text>().text = $"Under Renovation\n{card.renovationTime} weeks left..";
    }

    // Can we get tenants?
    findNewTenantButton.interactable = !card.tenants;

    UpdateRenovationText();
    TenantsText();
    SellPropertyText();
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
    string tenants = (card.tenants) ? "Yes" : card.underRenovation ? "(unable to lease while renovating)" : "No";
    string slot = $"{(card.bonuses["panel_bonus"] == 0.00f ? "None" : $"%{((card.bonuses["panel_bonus"] - 1) * 100):F2}")}";
    string managerBonus = $"{(card.assignedManager == "" ? "None" : $"%{(GameObject.Find(card.assignedManager).GetComponent<Manager>().bonusAmount - 1) * 100:F2}")}";
    int waterCost = GetWaterCost(card.waterUsage);

    propertyDetailsText.text =
      $"-=Info=-\n\n" + 
      $"Value\n${card.cost:#,##0}\n\n" +
      $"Base Rent: ${card.baseRent:#,##0}\n" +
      $"Rent: ${card.rent}\n" +
      $"Bond: ${card.bondCost:#,##0}\n\n" +
      $"-=Bonuses=-\n\n" +
      $"Slot: {slot}\n" +
      $"Manager: {managerBonus}\n\n" +
      $"-=Current Tennants=-\n\n" +
      $"Occupied\n{tenants}\n\n" +
      $"Term\n{card.tenantTermRemaining}/{card.tenantTerm}\n\n" +
      $"-=Misc=-\n\n" +
      $"Risk: Low\n\n" +
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
    renovationCoverPanel.SetActive(true);
    renovationCoverPanel.transform.GetChild(0).GetComponent<Text>().text = $"Under Renovation\n{card.renovationTime} weeks left..";

    UpdateRenovationText();
    SellPropertyText();
    UpdatePropertyDetailsText();

    gameManager.GameStats["TotalUpgrades"]++;

    // Check for high score
    if(card.rent > gameManager.GameStats["HighestRental"]) gameManager.GameStats["HighestRental"] = card.rent;
    if(card.cost > gameManager.GameStats["MostExpensiveProperty"]) gameManager.GameStats["MostExpensiveProperty"] = card.cost;
  }
  public void SellProperty() {
    gameManager.bank.AddMoney(currentOffer, "Property Sale");

    card.transform.parent.GetComponent<BuyPanel>().openPropertySlotButton.GetComponent<Image>().sprite = gameManager.normal;
    card.transform.parent.GetComponent<BuyPanel>().openPropertySlotButton.GetComponent<Image>().color = Color.white;

    gameManager.GameStats["TotalValueOfPropertiesSold"] += currentOffer;
    card.Destroy();

    gameManager.GameStats["TotalPropertiesSold"]++;
    OpenPropertyPanel("none");
  }
  public void UpdateRenovationText() {
    renovationBudget = (int)renovationBudgetSlider.value;
    int renovationTime = GetRenovationTime(renovationBudget);
    renovationCostText.text = $"${renovationBudget:#,##0}";

    renovationTimeText.text = $"{renovationTime} {(renovationTime == 1 ? "week" : "weeks")}";

    int renovationRentIncrease = GetRenovationRentIncrease(renovationBudget);
    rentIncreaseFromRenovationText.text = $"Base Rent\n${card.baseRent:#,##0} -> ${card.baseRent + renovationRentIncrease:#,##0}";
  }
  public void FindTenant() {
    card.tenants = true;
    card.tenantTerm = Random.Range(1, 7) * 3;
    card.tenantTermRemaining = card.tenantTerm;
    card.tenantMoveInWeek = gameManager.week;
    gameManager.bank.AddMoney(card.rent * 4, "Bond Payment");
    card.bondCost = card.rent * 4;
    gameManager.bank.AddMoney(card.rent * 2, "Rent in Advance Payment");
    gameManager.GameStats["TotalNumberTenants"] += Random.Range(1, 5);

    findNewTenantButton.interactable = false;

    card.transform.parent.GetComponent<BuyPanel>().openPropertySlotButton.GetComponent<Image>().sprite = gameManager.normal;
    card.transform.parent.GetComponent<BuyPanel>().openPropertySlotButton.GetComponent<Image>().color = Color.white;

    TenantsText();
    UpdatePropertyDetailsText();

    gameManager.tenancyTexts[activeSlot - 1].text = card.tenantTerm.ToString();
  }
  public int GetWaterCost(int usage) {
    return usage > 1000 ? Mathf.FloorToInt(waterRate * (usage - 1000)) : 0;
  }

  void SellPropertyText() {
    currentOffer = ValueProperty();
    sellButton.interactable = true;

    currentOfferText.text =
      $"Property Value: {card.cost:#,##0}\n\n" +
      $"Current Offer: ${currentOffer:#,##0}";
  }
  int ValueProperty() {
    return Mathf.FloorToInt(card.cost * gameManager.supplyDemandIndex);
  }
  void TenantsText() {
    tenantText.text =
      $"Current Tenants: {card.tenants} \n\n" +
      $"Lease Term: {card.tenantTermRemaining}/{card.tenantTerm} Months";
  }
  int GetRenovationTime(int budget) {
    return Mathf.FloorToInt(budget / 5000) + 1;
  }
  int GetRenovationRentIncrease(int budget) {
    int x = Mathf.FloorToInt(budget / 2500) + 10;
    return x;
  }
}

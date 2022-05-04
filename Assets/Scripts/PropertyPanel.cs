using UnityEngine;
using UnityEngine.UI;

public class PropertyPanel : MonoBehaviour {
  public GameManager gameManager;
  public GameObject selectedPanel;
  public GameObject PropertyManagersBoxPanel;

  public Text upgradeText, currentOfferText, tenantText;
  public Text propertyDetailsText;

  public Button upgradeButton, sellButton, findNewTenantButton;

  public Card card;

  public int cost, rentUpgradeAmount, currentOffer, activeSlot;

  public void OpenPropertyPanel(string name) {
    gameObject.SetActive(!gameObject.activeInHierarchy); // Show the property panel

    if(gameObject.activeInHierarchy == false) return;

    GameObject propertySlot = GameObject.Find("PropertySlot_" + name);
    selectedPanel = propertySlot;
    activeSlot = int.Parse(name);

    card = (propertySlot.transform.childCount > 1) ? propertySlot.GetComponentInChildren<Card>() : null;

    if(card == null) {
      upgradeText.text = "No property in current slot!";
      upgradeButton.interactable = false;

      currentOfferText.text = "No property available!";
      sellButton.interactable = false;

      tenantText.text = "No property available!";
      findNewTenantButton.interactable = false;

      return;
    }

    // Can we get tenants?
    findNewTenantButton.interactable = !card.tenants;

    UpgradeText();
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
    string tenants = (!card.tenants) ? "No" : "Yes";
    string man = (card.assignedManager == "") ? "No" : "Yes";
    string slot = $"{(card.bonuses["panel_bonus"] == 0.00f ? "None" : "%")}{((card.bonuses["panel_bonus"] - 1) * 100):F2}";
    string managerBonus = $"{(card.assignedManager == "" ? "None" : "%")}{(GameObject.Find(card.assignedManager).GetComponent<Manager>().bonusAmount - 1) * 100:F2}";

    propertyDetailsText.text =
      $"Tenants: {tenants}\n" +
      $"Manager: {man}\n\n" +
      $"Value: {card.cost:#,##0}\n" +
      $"Base Rent: {card.baseRent:#,##0}\n" +
      $"Actual Rent: {card.rent}\n\n" +
      $"-=Bonuses=-\n\n" +
      $"Slot: {slot}\n" +
      $"Manager: {managerBonus}\n\n" +
      $"-=Current Tennants=-\n\n" +
      $"Term: {card.tenantTermRemaining}/{card.tenantTerm}\n" +
      $"Risk: Low\n\n" +
      $"Power Usage: {card.powerUse:#,##0} KWH";
  }

  public void PurchaseUpgrade() {
    if(gameManager.addMoneyAmnt < cost) return;

    //Deduct the money
    gameManager.bank.AddMoney(-cost, "Upgrade Purchase");

    //Apply the upgrade
    card.baseRent += rentUpgradeAmount;

    //Increase the value of the property
    card.cost += Mathf.FloorToInt(card.cost * 0.28f);

    //Update the cards text
    card.UpdateRent();

    UpgradeText();
    SellPropertyText();
    UpdatePropertyDetailsText();

    gameManager.GameStats["MoneySpentOnUpgrades"] += cost;
    gameManager.GameStats["TotalUpgrades"]++;

    //check for high score
    if(card.rent > gameManager.GameStats["HighestRental"]) gameManager.GameStats["HighestRental"] = card.rent;
    if(card.cost > gameManager.GameStats["MostExpensiveProperty"]) gameManager.GameStats["MostExpensiveProperty"] = card.cost;
  }

  public void SellProperty() {
    gameManager.bank.AddMoney(currentOffer, "Property Sale");

    card.transform.parent.GetComponent<BuyPanel>().openPropertySlotButton.GetComponent<Image>().sprite = gameManager.normal;
    card.transform.parent.GetComponent<BuyPanel>().openPropertySlotButton.GetComponent<Image>().color = Color.white;

    card.Destroy();

    gameManager.GameStats["TotalPropertiesSold"]++;
    OpenPropertyPanel("none");
  }

  void UpgradeText() {
    rentUpgradeAmount = Mathf.FloorToInt(card.baseRent * 0.05f);
    cost = Mathf.FloorToInt(card.cost * card.upgradeMultiplier);
    upgradeButton.interactable = true;

    upgradeText.text =
      $"Upgrade Cost: ${cost:#,##0}\n" +
      $"(${card.baseRent}) -> ${(card.baseRent + rentUpgradeAmount):#,##0}";
  }

  void SellPropertyText() {
    currentOffer = ValueProperty();
    sellButton.interactable = true;

    currentOfferText.text =
      $"Property Value: {card.cost:#,##0} \n\n" +
      $"Current Offer: ${currentOffer:#,##0}";
  }

  int ValueProperty() {
    int baseValue = card.cost;
    return Mathf.FloorToInt(baseValue * gameManager.supplyDemandIndex);
  }

  public void FindTenant() {
    card.tenants = true;
    card.tenantTerm = Random.Range(1, 7) * 3;
    card.tenantTermRemaining = card.tenantTerm;
    card.tenantMoveInWeek = gameManager.week;

    gameManager.GameStats["TotalNumberTenants"] += Random.Range(1, 5);

    findNewTenantButton.interactable = false;

    card.transform.parent.GetComponent<BuyPanel>().openPropertySlotButton.GetComponent<Image>().sprite = gameManager.normal;
    card.transform.parent.GetComponent<BuyPanel>().openPropertySlotButton.GetComponent<Image>().color = Color.white;

    TenantsText();
    UpdatePropertyDetailsText();

    gameManager.tenancyTexts[activeSlot - 1].text = card.tenantTerm.ToString();
  }

  void TenantsText() {
    tenantText.text =
      $"Current Tenants: {card.tenants} \n\n" +
      $"Lease Term: {card.tenantTermRemaining}/{card.tenantTerm} Months \n\n" +
      $"Monthly Power: {card.powerUse}";
  }
}

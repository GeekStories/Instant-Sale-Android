using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour {
  public int cost, minCost, maxCost, bondCost;
  public int baseRent, rent, rentMin, rentMax;
  public int tenantTerm, tenantTermRemaining, tenantMoveInWeek;
  public int powerUse;
  public int rentBuffer = 6;
  public int weeksLeft;
  public int upgradeCost;

  public string assignedManager = "";

  public bool purchased;
  public bool tenants = false;

  public GameManager gameManager;

  public Dictionary<string, float> bonuses;
  public Text cardText;

  public Sprite[] houses;
  public Sprite assignedManagerImage;

  public Image spriteImage;

  private void Start() {
    gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

    bonuses = new Dictionary<string, float>() {
      {"panel_bonus", 1.00f},
      {"manager_bonus", 1.00f}
    };

    cardText = transform.GetChild(1).GetComponent<Text>();

    spriteImage = transform.GetChild(2).GetComponent<Image>();
    spriteImage.sprite = houses[Random.Range(0, houses.Length - 1)];

    cost = Mathf.FloorToInt(Random.Range(minCost, maxCost) * gameManager.supplyDemandIndex);
    baseRent = Random.Range(rentMin, rentMax);
    upgradeCost = Mathf.FloorToInt(cost * 0.25f);

    UpdateRent();
  }
  public void Destroy() {
    Destroy(gameObject);
  }
  public void ChangeBonus(string b, float n) {
    bonuses[b] = n;
    UpdateRent();
  }
  public void UpdateRent() {
    rent = baseRent;
    //Calculate all the bonuses into the rent
    foreach(KeyValuePair<string, float> bonus in bonuses) {
      rent += Mathf.FloorToInt((baseRent * bonus.Value) - baseRent);
    }

    cardText.text =
      $"Value: ${cost:#,##0}\n" +
      $"Rent: +${rent}";
  }
}

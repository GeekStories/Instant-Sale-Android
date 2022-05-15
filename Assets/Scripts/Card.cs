﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour {
  public int cost, purchasePrice = 0, minCost, maxCost, bondCost;
  public int baseRent, rent, rentMin, rentMax;
  public int tenantTerm, tenantTermRemaining, tenantMoveInWeek;
  public int waterUsage = 0;
  public int weeksLeft;
  public int newRent;
  public int renovationTime = 0;

  public int listingTime = 0;
  public int listingBudget = 0;
  public int totalOffersHad = 0;
  public int estimatedOffers = 0;

  public string assignedManager = "";

  public bool purchased;
  public bool tenants = false;
  public bool underRenovation = false;
  public bool currentlyListed = false;

  public GameManager gameManager;

  public Text cardText;

  public Sprite houseImage;
  public Sprite assignedManagerImage;

  public Image spriteImage;

  public List<Dictionary<string, int>> offers = new();

  public Dictionary<string, float> bonuses = new() {
    { "panel_bonus", 1.00f },
    { "manager_bonus", 1.00f }
  };

  private void Start() {
    gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

    cardText = transform.GetChild(1).GetComponent<Text>();

    cost = Mathf.FloorToInt(Random.Range(minCost, maxCost) * gameManager.supplyDemandIndex);
    baseRent = Random.Range(rentMin, rentMax);

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

  public void UpdateRenoTime() {
    cardText.text =
      $"Value: ${cost:#,##0}\n" +
      $"Reno: {renovationTime} weeks";
  }
}

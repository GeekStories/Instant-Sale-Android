using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Card : MonoBehaviour{

    public int cost;
    public int rent;
    public int baseRent;
    public int tenantTerm;
    public int tenantTermRemaining;
    public int powerUse;
    public bool tenants = false;

    public int tenantMoveInWeek;

    public Dictionary<string, float> bonuses;

    public string assignedManager = "";

    public Sprite[] houses;

    public bool purchased;

    [HideInInspector] public int rentBuffer = 6;

    public int upgradeCost;
    public float upgradeMultiplier = 1.25f;

    int rentMin = 560, rentMax = 1149;

    public int weeksLeft;

    public Text cardText;

    public GameManager gameManager;
    Image spriteImage;

    private void Start(){
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        bonuses = new Dictionary<string, float>(){
            {"panel_bonus", 1.00f},
            {"manager_bonus", 1.00f}
        };

        cardText = transform.GetChild(1).GetComponent<Text>();
        spriteImage = transform.GetChild(2).GetComponent<Image>();

        spriteImage.sprite = houses[Random.Range(0, houses.Length-1)];

        cost = Random.Range(gameManager.minCost, gameManager.maxCost) * 1000;

        baseRent = Random.Range(rentMin, rentMax);

        UpdateRent();
    }

    public void Destroy(){
        Destroy(gameObject);
    }

    public void ChangeBonus(string b, float n){
        bonuses[b] = n;
        UpdateRent();
    }

    public void UpdateRent(){
        rent = baseRent;
        //Calculate all the bonuses into the rent
        foreach (KeyValuePair<string, float> bonus in bonuses) {
            rent += Mathf.FloorToInt((baseRent * bonus.Value) - baseRent);
        }

        cardText.text = "Value: " + "$" + cost.ToString("#,##0") + "\nRent: " + "+$" + rent.ToString();
    }
}

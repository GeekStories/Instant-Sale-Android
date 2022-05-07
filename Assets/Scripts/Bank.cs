using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bank : MonoBehaviour {
  public GameManager gameManager;

  public GameObject transactionObject;

  public Dictionary<int, Text> contractsText = new();
  public Dictionary<int, Text> defaultTexts = new();
  public Dictionary<int, Text> loansAccountText = new();
  public List<Dictionary<string, int>> Loans = new();

  public int creditLimit = 15000;
  public float money = 0, totalDebt = 0;

  public Toggle[] termSelectors;
  public Slider loanAmountSlider;

  public Text moneyText, creditLimitText, loanAmountText, loanTotalText, debtText;
  public Text[] propertyIncomeTexts;
  public int[] propertyIncomes;

  public GameObject LoanObject, LoanObjectContainer;
  public GameObject loansPanel, stocksPanel, savingsPanel, balanceSheetPanel, transactionsPanel, transactionsTable;
  private GameObject currentPanel;

  private void Start() {
    currentPanel = loansPanel;
    creditLimitText.text = $"Credit limit: ${creditLimit:#,##0}";
    AddMoney(100000, "Startup Loan");
  }
  public void ChangePanel(string panelName) {
    switch(panelName) {
      case "loans":
        if(currentPanel.name == "Loans") return;
        currentPanel.SetActive(false);
        currentPanel = loansPanel;
        break;
      case "stocks":
        if(currentPanel.name == "Stocks") return;
        currentPanel.SetActive(false);
        currentPanel = stocksPanel;
        break;
      case "savings":
        if(currentPanel.name == "Savings") return;
        currentPanel.SetActive(false);
        currentPanel = savingsPanel;
        break;
      case "trans":
        if(currentPanel.name == "Transactions") return;
        currentPanel.SetActive(false);
        currentPanel = transactionsPanel;
        break;
      case "BalanceSheet":
        if(currentPanel.name == "BalanceSheet") return;
        currentPanel.SetActive(false);
        currentPanel = balanceSheetPanel;
        break;
    }

    currentPanel.SetActive(true);
  }
  public void AddMoney(float amount, string purchaseType) {
    money += amount;
    moneyText.text = $"${money:#,##0}";

    GameObject newTransaction = Instantiate(transactionObject, transactionsTable.transform);
    newTransaction.transform.SetSiblingIndex(0);
    newTransaction.transform.GetChild(0).GetComponent<Text>().text = $"{(amount > 0 ? $"${amount:#,##0}" : $"${(amount * -1):#,##0}")}";
    newTransaction.transform.GetChild(1).GetComponent<Text>().color = amount > 0 ? Color.green : Color.red;
    newTransaction.transform.GetChild(1).GetComponent<Text>().text = $"{(amount > 0 ? "deposit" : "withdraw")}";
    newTransaction.transform.GetChild(2).GetComponent<Text>().text = $"${money:#,##0}";
    newTransaction.transform.GetChild(3).GetComponent<Text>().text = $"{purchaseType}";
    newTransaction.transform.GetChild(4).GetComponent<Text>().text = $"W{gameManager.week}:M{gameManager.month}:Y{gameManager.year}";

    if(money > gameManager.GameStats["MostAmountOfMoney"]) gameManager.GameStats["MostAmountOfMoney"] = money;
  }
  public void TakeRepayments() {
    if(Loans.Count > 0) {
      int i;
      foreach(Dictionary<string, int> loan in Loans) {
        i = loan["key"];
        int index = Loans.FindIndex(loan => loan["key"] == i);

        int defaults = loan["defaults"];
        float amountOwing = loan["total"] - loan["totalPaid"];

        if(amountOwing == 0) continue;

        if(defaults < 3) {
          //Check the loan hasn't defaulted
          float paymentWeek = loan["paymentWeek"];

           //Check if this week is the payment week
          if(gameManager.week == paymentWeek) {
            int repayments = loan["repayments"];

             //Check if the user has defaulted on this loan
            if(repayments > money && amountOwing > money) {
              Loans[index]["defaults"]++; //Add a strike to the defaults and move to the next loan
              continue;
            }

            //Check if this is our final payment
            if(repayments >= amountOwing) {
              //This payment will put us in credit{
              AddMoney(-amountOwing, "Final Loan Payment"); //Pay the final difference

              GameObject.Find(i.ToString()).transform.GetChild(2).gameObject.SetActive(true);
              GameObject.Find(i.ToString()).transform.GetChild(3).GetComponent<Button>().interactable = false;

              UpdateLoanText(i);

              //Increase credit limit by 5% of the finialized loan
              creditLimit += Mathf.FloorToInt(loan["total"] * 0.075f);
              creditLimitText.text = $"Credit Limit: ${creditLimit:#,##0}";
              loanAmountSlider.maxValue = creditLimit;
              continue;
            }

            //User can make a payment this month if we reach this line
            AddMoney(-repayments, "Loan Payment"); //Take payment
            Loans[index]["totalPaid"] += repayments;

            //Add the amount back to our credit limit
            creditLimit += repayments;
            creditLimitText.text = $"Credit Limit: ${creditLimit:#,##0}";
            loanAmountSlider.maxValue = creditLimit;
          }
        } else {
          //Force sell random assets until debt is paid.
          float collectedValue = money;
          List<Card> propertiesToBeRepod = new();

          foreach(GameObject panel in gameManager.buyPanels) {
            if(panel.transform.childCount > 1) {
              Card c = panel.transform.GetChild(1).GetComponent<Card>();

              collectedValue += c.cost * 0.90f;
              propertiesToBeRepod.Add(c);

              if(collectedValue > amountOwing) break;
            }
          }

          //Force sell the property
          foreach(Card property in propertiesToBeRepod) {
            int salePrice = Mathf.FloorToInt(property.cost * .90f);
            AddMoney(salePrice, "Property Sale"); //Force sell the property at %90 its value
            gameManager.GameStats["TotalPropertiesSold"]++;
            gameManager.GameStats["TotalValueOfPropertiesSold"] += salePrice;
            property.Destroy();
          }

          //Pay off the loan, leaving the difference in collectedValue and AmountOwing
          AddMoney(-amountOwing, "Loan Payment");
          gameManager.GameStats["TotalMoneySpent"] += amountOwing;
          Loans[index]["totalPaid"] = loan["total"];

          loanAmountSlider.maxValue = creditLimit;

          Transform loanObject = LoanObjectContainer.transform.Find(i.ToString());
          loanObject.GetChild(2).gameObject.SetActive(true);
          loanObject.GetChild(4).GetComponent<Button>().interactable = false;

          //Increase credit limit by 5% of the finialized loan
          creditLimit += Mathf.FloorToInt(loan["total"] * 0.05f);
          creditLimitText.text = "Credit Limit: " + creditLimit.ToString("#,##0");
          loanAmountSlider.maxValue = creditLimit;

          if(money < 0) {
            //Game Over at this point?
            gameManager.gameOverPanel.SetActive(true);
            gameManager.CalculateScore();

            CancelInvoke("CalculateNetWorth");
            return;
          }
        }

        UpdateLoanText(i);
      }
    }
  }
  public void UpdateLoanText(int key) {
    Dictionary<string, int> loan = Loans.Find(loan => loan["key"] == key);

    contractsText[key].text = $"Account #{loan["key"]}";
    loansAccountText[key].text =
      $"Payable: ${(loan["total"] - loan["totalPaid"]):#,##0}\n" +
      $"Monthly: ${loan["repayments"]:#,##0} (W{loan["paymentWeek"]})\n" +
      $"Term: {loan["term"]}";

    defaultTexts[key].text = $"Defaults: {loan["defaults"]}/3";
    debtText.text = $"Total Debt: -${Calculate.TotalDebt("debt", Loans):#,##0}";
  }
  public void TakeLoan() {
    int term = GetSelectedToggle();
    int amnt = (int)loanAmountSlider.value;

    if(amnt <= creditLimit) {
      int key = Random.Range(11111, 99999);

      GameObject newLoanObj = Instantiate(LoanObject, LoanObjectContainer.transform);
      newLoanObj.name = key.ToString();

      contractsText[key] = newLoanObj.transform.GetChild(0).GetComponent<Text>();
      loansAccountText[key] = newLoanObj.transform.GetChild(1).GetComponent<Text>();
      defaultTexts[key] = newLoanObj.transform.GetChild(3).GetComponent<Text>();

      float interestRate = GetInterestRate(term);
      int repayments = CalculatePayments(amnt, interestRate, term);
      int totalToPay = Mathf.FloorToInt(repayments * term);
      int paymentStartdate = gameManager.week + gameManager.month + gameManager.year;

      Dictionary<string, int> loan = new();
      loan.Add("term", term);
      loan.Add("principle", amnt);
      loan.Add("total", totalToPay);
      loan.Add("totalPaid", 0);
      loan.Add("defaults", 0);
      loan.Add("paymentWeek", gameManager.week);
      loan.Add("issueDate", paymentStartdate);
      loan.Add("repayments", repayments);
      loan.Add("key", key);

      Loans.Add(loan);

      newLoanObj.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(delegate () { PayLoanFull(key); });

      AddMoney(amnt, "Loan Deposit");

      creditLimit -= amnt;
      creditLimitText.text = $"Credit Limit: {creditLimit:#,##0}";
      loanAmountSlider.maxValue = creditLimit;

      totalDebt = Calculate.TotalDebt("totalDebt", Loans);

      UpdateLoanText(key);
    }
  }
 
  public void PayLoanFull(int key) {
    Dictionary<string, int> loan = Loans.Find(loan => loan["key"] == key);

    int amountOwing = loan["total"] - loan["totalPaid"];

    if(money >= amountOwing) {
      AddMoney(-amountOwing, "Final Loan Payment");

      creditLimit += Mathf.FloorToInt(amountOwing);
      creditLimit += Mathf.FloorToInt(loan["total"] * 0.07f);
      creditLimitText.text = $"Credit Limit: ${creditLimit:#,##0}";

      loanAmountSlider.maxValue = creditLimit;

      GameObject.Find(key.ToString()).transform.GetChild(4).GetComponent<Button>().interactable = false;
      GameObject.Find(key.ToString()).transform.GetChild(2).gameObject.SetActive(true);

      loan["totalPaid"] = loan["total"];
      loan["repayments"] = 0;

      int loanIndex = Loans.FindIndex(loan => loan["key"] == key);
      Loans[loanIndex] = loan;
      UpdateLoanText(key);
    }

    return;
  }
  public float CalculateAmountPayable(int amnt) {
    int term = GetSelectedToggle();
    float interestRate = GetInterestRate(term);
    float repayments = CalculatePayments(amnt, interestRate, term);
    return repayments * term;
  }
  public int CalculatePayments(int amount, float interestRate, int term) {
    float r = (interestRate / 12) / 100;
    return Mathf.FloorToInt(amount * (r * (Mathf.Pow(1 + r, term)) / (Mathf.Pow(1 + r, term) - 1)));
  }
  public float GetInterestRate(int term) {
    return term switch {
      12 => 5.25f,
      24 => 5.85f,
      36 => 6.15f,
      48 => 6.35f,
      _ => 0,
    };
  }
  public void UpdateLoanAmountSliderText() {
    loanAmountText.text = "$" + loanAmountSlider.value.ToString("#,##0");
    float amountPayable = CalculateAmountPayable((int)loanAmountSlider.value);
    loanTotalText.text =
      $"Total Repayable: ${amountPayable:#,##0}\n" +
      $"Total Interest: ${amountPayable - loanAmountSlider.value:F2}\n" +
      $"Monthly Repayments: ${(amountPayable / GetSelectedToggle())} p/m";
  }
  public void UpdatePropertyIncomes(int panel, int amount) {
    propertyIncomes[panel] += amount;
    propertyIncomeTexts[panel].text = $"Property {panel + 1}: ${propertyIncomes[panel]:#,##0}";
  }
  int GetSelectedToggle() {
    foreach(Toggle t in termSelectors)
      if(t.isOn) return int.Parse(t.name); //returns selected toggle

    return 99;
  }
}

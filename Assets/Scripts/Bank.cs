using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bank : MonoBehaviour {
  public GameManager gameManager;

  public GameObject transactionObject;

  public Dictionary<int, Text> contractsText = new();
  public Dictionary<int, Text> defaultTexts = new();
  public Dictionary<int, float[]> Loans = new() { };
  public Dictionary<int, Text> loansAccountText = new();

  public float creditLimit = 15000, money = 0, totalDebt = 0;

  public Toggle[] termSelectors;
  public Slider loanAmountSlider;

  public Text moneyText, creditLimitText, loanAmountText, loanTotalText, debtText;

  public GameObject LoanObject, LoanObjectContainer;
  public GameObject loansPanel, stocksPanel, savingsPanel, transactionsPanel, transactionsTable;
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
        if(currentPanel.name == "History") return;
        currentPanel.SetActive(false);
        currentPanel = transactionsPanel;
        break;
    }

    currentPanel.SetActive(true);
  }
  public void AddMoney(float amount, string purchaseType) {
    money += amount;
    moneyText.text = $"${money:#,##0}";

    GameObject newTransaction = Instantiate(transactionObject, transactionsTable.transform);
    newTransaction.transform.SetSiblingIndex(0);
    newTransaction.transform.GetChild(0).GetComponent<Text>().text = $"${amount:#,##0}";
    newTransaction.transform.GetChild(0).GetComponent<Text>().color = amount > 0 ? Color.green : Color.red;
    newTransaction.transform.GetChild(1).GetComponent<Text>().text = $"{(amount > 0 ? "deposit" : "withdraw")}";
    newTransaction.transform.GetChild(2).GetComponent<Text>().text = $"${money:#,##0}";
    newTransaction.transform.GetChild(3).GetComponent<Text>().text = $"{purchaseType}";

    if(money > gameManager.GameStats["MostAmountOfMoney"]) gameManager.GameStats["MostAmountOfMoney"] = money;
  }
  public void TakeRepayments() {
    if(Loans.Count > 0) {
      int i;
      foreach(KeyValuePair<int, float[]> loan in Loans) {
        i = loan.Key;
        int defaults = (int)loan.Value[4];
        float amountOwing = loan.Value[2] - loan.Value[3];

        if(amountOwing == 0) continue;

        if(defaults < 3) {
          //Check the loan hasn't defaulted
          float paymentWeek = loan.Value[5];

          if(gameManager.week == paymentWeek && loan.Value[2] > loan.Value[3]) {
            //Check if this week is the payment week
            float repayments = loan.Value[7];

            if(repayments > money && amountOwing > money) {
              //Check if the user has defaulted on this loan
              loan.Value[4]++; //Add a strike to the defaults and move to the next loan
              continue;
            }

            //Check if this is out final payment
            if(repayments >= amountOwing) {
              //This payment will put us in credit{
              AddMoney(-amountOwing, "Final Loan Payment"); //Pay the final difference
              loan.Value[3] = loan.Value[2];

              GameObject.Find(loan.Key.ToString()).transform.GetChild(2).gameObject.SetActive(true);
              GameObject.Find(loan.Key.ToString()).transform.GetChild(3).GetComponent<Button>().interactable = false;

              UpdateLoanText(loan.Key);

              //Increase credit limit by 5% of the finialized loan
              creditLimit += loan.Value[2] * 0.075f;
              creditLimitText.text = $"Credit Limit: ${creditLimit:#,##0}";
              loanAmountSlider.maxValue = creditLimit;
              continue;
            }

            //User can make a payment this month if we reach this line
            AddMoney(-loan.Value[7], "Loan Payment"); //Take payment
            loan.Value[3] += loan.Value[7];

            //Add the amount back to our credit limit
            creditLimit += loan.Value[7];
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
            AddMoney(property.cost * 0.90f, "Property Sale"); //Force sell the property at %90 its value
            property.Destroy();
          }

          //Pay off the loan, leaving the difference in collectedValue and AmountOwing
          AddMoney(-amountOwing, "Loan Payment");

          //Close the loan account
          loan.Value[3] = loan.Value[2];

          loanAmountSlider.maxValue = creditLimit;

          GameObject.Find(loan.Key.ToString()).transform.GetChild(2).gameObject.SetActive(true);
          GameObject.Find(loan.Key.ToString()).transform.GetChild(3).GetComponent<Button>().interactable = false;

          //Increase credit limit by 5% of the finialized loan
          creditLimit += loan.Value[2] * 0.05f;
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
    contractsText[key].text = $"Account #{Loans[key][8]}";
    loansAccountText[key].text =
      $"Payable: ${(Loans[key][2] - Loans[key][3]):#,##0}\n" +
      $"Monthly: ${Loans[key][7]:#,##0} (W{Loans[key][5]})\n" +
      $"Term: {Loans[key][0]}";

    defaultTexts[key].text = $"Defaults: {Loans[key][4]}/3";
    debtText.text = $"Total Debt: -${Calculate.TotalDebt("debt", Loans):#,##0}";
  }
  public void TakeLoan() {
    int term = GetSelectedToggle();
    int amnt = (int)loanAmountSlider.value;

    if(creditLimit >= amnt /*&& (rent * 4) < repayments*/) {
      //Maybe in the future, add rental income as a factor?
      int key = Random.Range(11111, 99999);

      GameObject newLoanObj = Instantiate(LoanObject, LoanObjectContainer.transform);
      newLoanObj.name = key.ToString();

      contractsText[key] = newLoanObj.transform.GetChild(0).GetComponent<Text>();
      loansAccountText[key] = newLoanObj.transform.GetChild(1).GetComponent<Text>();
      defaultTexts[key] = newLoanObj.transform.GetChild(2).GetComponent<Text>();

      float interestRate = GetInterestRate(term);
      float repayments = CalculatePayments(amnt, interestRate, term);
      float totalToPay = repayments * term;
      int paymentStartdate = gameManager.week + gameManager.month + gameManager.year;

      /* 0 = Loan Term.
      1 = Amount Loaned
      2 = Total Loan Cost (Amount Loaned + Total Interest (to be) Paid)
      3 = Total Amount Paid Back
      4 = Default Strike Count
      5 = Payment Date
      6 = Loan Issue Date
      7 = Repayments
      8 = Random Loan Account Number*/
      float[] loan = { term, amnt, totalToPay, 0, 0, gameManager.week, paymentStartdate, repayments, key };

      Loans[key] = loan;

      newLoanObj.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(delegate () { PayLoanFull(key); });

      AddMoney(amnt, "Loan Deposit");

      creditLimit -= amnt;
      creditLimitText.text = $"Credit Limit: {creditLimit:#,##0}";
      loanAmountSlider.maxValue = creditLimit;

      totalDebt = Calculate.TotalDebt("totalDebt", Loans);

      UpdateLoanText(key);
    }
  }
  public void PayLoanFull(int key) {
    float amountOwing = Loans[key][2] - Loans[key][3];
    if(money >= amountOwing) {
      AddMoney(-amountOwing, "Final Loan Payment");

      creditLimit += amountOwing;
      creditLimit += Loans[key][2] * 0.07f;
      creditLimitText.text = $"Credit Limit: ${creditLimit:#,##0}";

      loanAmountSlider.maxValue = creditLimit;

      GameObject.Find(key.ToString()).transform.GetChild(3).GetComponent<Button>().interactable = false;
      GameObject.Find(key.ToString()).transform.GetChild(4).gameObject.SetActive(true);

      Loans[key][3] = Loans[key][2];
      Loans[key][7] = 0;
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
  public float CalculatePayments(int amount, float interestRate, int term) {
    float r = (interestRate / 12) / 100;
    return Mathf.RoundToInt(amount * (r * (Mathf.Pow(1 + r, term)) / (Mathf.Pow(1 + r, term) - 1)));
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
  int GetSelectedToggle() {
    foreach(Toggle t in termSelectors)
      if(t.isOn) return int.Parse(t.name); //returns selected toggle

    return 99;
  }
}

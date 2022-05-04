using System.Collections.Generic;
using UnityEngine;
public static class Calculate {
  public static int TotalPropertyValue(GameObject[] buyPanels, float sellBuffer, float supplyDemandIndex) {
    int x = 0;

    foreach(GameObject item in buyPanels) {
      if(item.GetComponent<BuyPanel>().isOwned && item.transform.childCount > 1) {
        Card c = item.GetComponentInChildren<Card>();
        x += Mathf.FloorToInt((c.cost * sellBuffer) * supplyDemandIndex);
      }
    }

    return x;
  }

  public static int TotalInvestments() {
    return 0;
  }

  public static int TotalDebt(string f, Dictionary<int, int[]> Loans) {
    int debt = 0;

    if(Loans.Count == 0)
      return 0;

    if(f == "networth") {
      foreach(KeyValuePair<int, int[]> loan in Loans) {
        if(loan.Value[0] != 12)
          debt += (loan.Value[2] - loan.Value[3]);
      }

      return debt;
    }

    foreach(KeyValuePair<int, int[]> loan in Loans) {
      debt += (loan.Value[2] - loan.Value[3]);
    }

    return debt;
  }

  public static int RawIncome(GameObject[] buyPanels, Dictionary<int, int[]> Loans, List<GameObject> hiredManagers) {
    int x = 0; //Start at 0

    //Add up all the rent currently being collected
    foreach(GameObject item in buyPanels) {
      if(item.GetComponent<BuyPanel>().isOwned && item.transform.childCount > 1) {
        Card c = item.GetComponentInChildren<Card>();
        if(c.tenants) {
          x += c.rent;
        }
      }
    }

    //Deduct all the loan repayments
    foreach(KeyValuePair<int, int[]> loan in Loans) {
      x -= loan.Value[7] / 4;
    }

    // Deduct payroll
    foreach(GameObject manager in hiredManagers) {
      x -= manager.GetComponent<Manager>().weeklyPay;
    }

    //Return whatever the numer ends up being
    return x;
  }

  public static int NetWorth(GameObject[] buyPanels, float sellBuffer, float supplyDemandIndex, int money, Dictionary<int, int[]> Loans) {
    int propertyValues = TotalPropertyValue(buyPanels, sellBuffer, supplyDemandIndex);
    int investmentsValues = TotalInvestments();
    int currentCashNonLoaned = money;
    int totalDebt = TotalDebt("networth", Loans);

    return (propertyValues + investmentsValues + currentCashNonLoaned) - totalDebt;
  }
}

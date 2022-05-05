using System.Collections.Generic;
using UnityEngine;
public static class Calculate {
  public static float TotalPropertyValue(GameObject[] buyPanels, float sellBuffer, float supplyDemandIndex) {
    float x = 0;

    foreach(GameObject item in buyPanels) {
      if(item.GetComponent<BuyPanel>().isOwned && item.transform.childCount > 1) {
        Card c = item.GetComponentInChildren<Card>();
        x += (c.cost * sellBuffer) * supplyDemandIndex;
      }
    }

    return x;
  }

  public static float TotalInvestments() {
    return 0;
  }

  public static float TotalDebt(string f, Dictionary<int, float[]> Loans) {
    float debt = 0;

    if(Loans.Count == 0)
      return 0;

    if(f == "networth") {
      foreach(KeyValuePair<int, float[]> loan in Loans) {
        if(loan.Value[0] != 12)
          debt += (loan.Value[2] - loan.Value[3]);
      }

      return debt;
    }

    foreach(KeyValuePair<int, float[]> loan in Loans) {
      debt += (loan.Value[2] - loan.Value[3]);
    }

    return debt;
  }

  public static float RawIncome(GameObject[] buyPanels, Dictionary<int, float[]> Loans, List<GameObject> hiredManagers) {
    float x = 0; //Start at 0

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
    foreach(KeyValuePair<int, float[]> loan in Loans) {
      x -= loan.Value[7] / 4;
    }

    // Deduct payroll
    foreach(GameObject manager in hiredManagers) {
      x -= manager.GetComponent<Manager>().weeklyPay;
    }

    //Return whatever the numer ends up being
    return x;
  }

  public static float NetWorth(GameObject[] buyPanels, float sellBuffer, float supplyDemandIndex, float money, Dictionary<int, float[]> Loans) {
    float propertyValues = TotalPropertyValue(buyPanels, sellBuffer, supplyDemandIndex);
    float investmentsValues = TotalInvestments();
    float currentCashNonLoaned = money;
    float totalDebt = TotalDebt("networth", Loans);

    return (propertyValues + investmentsValues + currentCashNonLoaned) - totalDebt;
  }
}

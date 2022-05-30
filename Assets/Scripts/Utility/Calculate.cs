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

  public static float TotalDebt(string f, List<Dictionary<string, int>> Loans) {
    int debt = 0;

    if(Loans.Count == 0)
      return 0;

    if(f == "networth") {
      Loans.ForEach(loan => debt += loan["term"] != 12 ? loan["total"] - loan["totalPaid"] : 0);
      return debt;
    }

    Loans.ForEach(loan => debt += loan["total"] - loan["totalPaid"]);
    return debt;
  }

  public static float RawIncome(GameObject[] buyPanels, List<Dictionary<string, int>> Loans, List<GameObject> hiredManagers) {
    float x = 278; //Start at 278 (amount remaining after rent and living costs from main job income)

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
    foreach(Dictionary<string, int> loan in Loans) {
      if(loan["totalPaid"] == loan["total"]) continue;
      x -= loan["repayments"];
    }

    // Deduct payroll
    foreach(GameObject manager in hiredManagers) {
      x -= manager.GetComponent<Manager>().weeklyPay;
    }

    //Return whatever the numer ends up being
    return x;
  }

  public static float NetWorth(GameObject[] buyPanels, float sellBuffer, float supplyDemandIndex, float money, List<Dictionary<string, int>> Loans) {
    float propertyValues = TotalPropertyValue(buyPanels, sellBuffer, supplyDemandIndex);
    float investmentsValues = TotalInvestments();
    float currentCashNonLoaned = money;
    float totalDebt = TotalDebt("networth", Loans);

    return (propertyValues + investmentsValues + currentCashNonLoaned) - totalDebt;
  }
}

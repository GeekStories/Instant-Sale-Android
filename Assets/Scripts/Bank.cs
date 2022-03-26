using UnityEngine;

public class Bank : MonoBehaviour{

    public GameObject loansPanel, stocksPanel, savingsPanel, historyPanel;
    private GameObject currentPanel;

    private void Start(){
       currentPanel = loansPanel; 
    }

    public void ChangePanel(string panelName){
        switch (panelName){
            case "loans":
                if(currentPanel.name == "Loans")
                    return;

                currentPanel.SetActive(false);
                currentPanel = loansPanel;
                break;
            case "stocks":
                if(currentPanel.name == "Stocks")
                    return;

                currentPanel.SetActive(false);
                currentPanel = stocksPanel;
                break;
            case "savings":
                if(currentPanel.name == "Savings")
                    return;

                currentPanel.SetActive(false);
                currentPanel = savingsPanel;
                break;
            case "trans":
                if(currentPanel.name == "History")
                    return;

                currentPanel.SetActive(false);
                currentPanel = historyPanel;
                break;
        }

        currentPanel.SetActive(true);
    }
}

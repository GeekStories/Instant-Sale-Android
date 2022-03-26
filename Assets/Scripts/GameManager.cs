using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text moneyText,
        netWorthText,
        debtText,
        rawIncomeText;

    public Text yearsText,
        weeksPerSecondText,
        supplyDemandText,
        weeksLeft;

    public Text primaryStatsText,
        otherStatsText,
        finalScoreText;

    public Text creditLimitText,
        loanAmountText,
        loanTotalText,
        defaultsText,
        transactionsText;

    public Text cardsLeftText;

    public Text[] tenancyTexts;

    public int money,
        rent,
        score;

    public int cardsLeft,
        cardsLeftMax;

    public int year,
        month,
        week;

    public int weeksPerMinute,
        maxWeeksPerMinute = 0;

    public int minCost = 3,
        maxCost = 15,
        rentMax = 75,
        totalDebt,
        networth,
        addMoneyAmnt,
        creditLimit;

    public int nextUpgrade = 5;

    public float timeBuffer = 0.75f;

    public float sellBuffer = 0.25f;

    public float supplyDemandIndex = 1.00f;

    public GameObject[] buyPanels;

    public List<GameObject> hiredManagers;

    public GameObject card;

    public GameObject LoanObject,
        LoanObjectContainer;

    public GameObject gameOverPanel;

    public GameObject managersForHirePanel;

    public GameObject currentManagersPanel;

    public GameObject selectedManagerPanel;

    public GameObject selectedHiredManagerPanel;

    public GameObject propertyManagersPanel;

    public GameObject managerObject;

    public GameObject selectedManager;

    public GameObject selectedHiredManager;

    public AudioClip[] backgroundMusic;

    public AudioClip[] upgradeSounds;

    public AudioClip[] placeCardSounds;

    public AudioClip pickUpCard;

    public AudioClip buySellProperty;

    public AudioClip buttonClick;

    public AudioSource ambientSource;

    public AudioSource source;

    public Image selectedHiredManagerIcon,
        selectedManagerIcon;
    public Sprite[] managerSprites;

    public Sprite musicOn,
        musicOff,
        soundOn,
        soundOff,
        exclamation,
        normal;

    public bool music = false,
        sounds = false;

    public bool repeatingWeek = false;

    public Dictionary<int, Text> loansAccountText;

    public Dictionary<int, Text> contractsText;

    public Dictionary<int, Text> defaultTexts;

    public Dictionary<int, int[]> Loans;

    public Dictionary<string, int> GameStats;

    public Transform CardPile;

    public Canvas tutorial;

    public Button musicToggle,
        soundToggle,
        fireManagerButton;

    public Toggle[] termSelectors;

    public Slider loanAmountSlider;

    public List<GameObject> managersForHire;

    public Dictionary<string, int> deductions;

    public void Start()
    {
        GameStats = new Dictionary<string, int>()
        {
            { "TotalPropertiesOwned", 0 },
            { "TotalMoneySpent", 0 },
            { "TotalPropertiesSold", 0 },
            { "MoneySpentOnUpgrades", 0 },
            { "TotalPassedProperties", 0 },
            { "TotalUpgrades", 0 },
            { "MostExpensiveProperty", 0 },
            { "MostExpensivePurchased", 0 },
            { "HighestRental", 0 },
            { "TotalNumberTenants", 0 },
            { "TotalMonthsStayed", 0 },
            { "HighestNetworth", 0 },
            { "MostAmountOfMoney", 0 }
        };

        source = GetComponent<AudioSource>();

        Loans = new Dictionary<int, int[]>();
        deductions = new Dictionary<string, int>();

        contractsText = new Dictionary<int, Text>();
        loansAccountText = new Dictionary<int, Text>();
        defaultTexts = new Dictionary<int, Text>();

        managersForHire = new List<GameObject>();
        hiredManagers = new List<GameObject>();

        BackgroundMusic();

        rent = 0;
        cardsLeftMax = 5;
        cardsLeft = cardsLeftMax;
        year = 0;

        supplyDemandText.text = supplyDemandIndex.ToString("F2");
        creditLimitText.text = "Credit limit: $" + creditLimit.ToString("#,##0");

        GenerateManagersForHire();

        NextWeek();

        InvokeRepeating("CalculateNetWorth", 0, 0.5f);
        InvokeRepeating("RepeatingWeeks", 0, (60 - weeksPerMinute) * timeBuffer);
    }

    public void BackgroundMusic()
    {
        CancelInvoke("BackgroundMusic");

        int x = Random.Range(0, 2);

        if (music)
            source.PlayOneShot(backgroundMusic[x], 0.1f);

        InvokeRepeating("BackgroundMusic", backgroundMusic[x].length, backgroundMusic[x].length);
    }

    public void CheckPile()
    {
        if (CardPile.childCount > 0)
        {
            //Are there any cards currently in the pile?
            Card c = CardPile.GetChild(0).GetComponent<Card>(); //Grab that card
            c.weeksLeft--;

            //Check if we have any weeks left before expiry
            if (c.weeksLeft > 0)
            {
                cardsLeft = cardsLeftMax - 1;
                weeksLeft.text = "Expires in " + c.weeksLeft.ToString() + " weeks";
            }
            else
            {
                c.Destroy(); //No weeks left, destroy the card
                CreateCard();
            }
        }
        else
        {
            //No card, so generate one!
            CreateCard();
        }
    }

    public void CreateCard()
    {
        if (cardsLeft > 0)
        {
            //Do we have any left in the pile?
            GameObject x = Instantiate(card, CardPile); //Yes! Generate a new card!
            x.GetComponent<Card>().weeksLeft = Random.Range(3, 10);
            weeksLeft.text = "Expires in " + x.GetComponent<Card>().weeksLeft.ToString() + " weeks";

            cardsLeft--;
            cardsLeftText.text = cardsLeft.ToString() + "/" + cardsLeftMax.ToString();
        }
        else
        {
            weeksLeft.text = "Out of cards!";
        }
    }

    public void AddMoney(int amount, string purchaseType)
    {
        money += amount;
        moneyText.text = "$" + money.ToString("#,##0");

        if (amount != 0)
        {
            //<purchaseType>: (<amount>) | <balance after>
            string oldText = transactionsText.text;
            transactionsText.text =
                purchaseType
                + ": ($"
                + amount.ToString("#,##0")
                + ") | $"
                + money.ToString("#,##0")
                + "\n"
                + oldText;
        }

        if (money > GameStats["MostAmountOfMoney"])
        {
            GameStats["MostAmountOfMoney"] = money;
        }
    }

    public void CalculateNetWorth()
    {
        int propertyValues = CalculateTotalPropertyValue();
        int investmentsValues = CalculateTotalInvestments();
        int currentCashNonLoaned = CalculateTotalCashAsset();
        int totalDebt = CalculateTotalDebt("networth");

        int NetWorth = (propertyValues + investmentsValues + currentCashNonLoaned) - totalDebt;

        netWorthText.text = "NetWorth: $" + NetWorth.ToString("#,##0");

        if (NetWorth > GameStats["HighestNetworth"])
        {
            GameStats["HighestNetworth"] = NetWorth;
        }

        int inOut = CalculateRawIncome();

        if (inOut > 0)
        {
            rawIncomeText.color = Color.green;
            rawIncomeText.text = "($+" + inOut.ToString("#,##0") + ")";
        }
        else if (inOut < 0)
        {
            rawIncomeText.color = Color.red;
            rawIncomeText.text = "($" + inOut.ToString("#,##0") + ")";
        }
        else
        {
            rawIncomeText.color = Color.grey;
            rawIncomeText.text = "($" + inOut.ToString("#,##0") + ")";
        }
    }

    public int CalculateTotalPropertyValue()
    {
        int x = 0;

        foreach (GameObject item in buyPanels)
        {
            if (item.GetComponent<BuyPanel>().isOwned && item.transform.childCount > 1)
            {
                Card c = item.GetComponentInChildren<Card>();
                x += Mathf.FloorToInt((c.cost * sellBuffer) * supplyDemandIndex);
            }
        }

        return x;
    }

    public int CalculateTotalInvestments()
    {
        return 0;
    }

    public int CalculateTotalCashAsset()
    {
        return money;
    }

    public int CalculateTotalDebt(string f)
    {
        int debt = 0;

        if (Loans.Count == 0)
            return 0;

        if (f == "networth")
        {
            foreach (KeyValuePair<int, int[]> loan in Loans)
            {
                if (loan.Value[0] != 12)
                    debt += (loan.Value[2] - loan.Value[3]);
            }

            return debt;
        }

        foreach (KeyValuePair<int, int[]> loan in Loans)
        {
            debt += (loan.Value[2] - loan.Value[3]);
        }

        return debt;
    }

    public void NextWeek()
    {
        if (sounds)
        {
            ambientSource.PlayOneShot(buttonClick);
        }

        //Start the next week
        week++;

        if (week == 5)
        {
            month++;

            //Add random power usage
            foreach (GameObject panel in buyPanels)
            {
                if (panel.transform.childCount > 1)
                {
                    Card c = panel.transform.GetChild(1).GetComponent<Card>();

                    if (c.tenants)
                    {
                        c.powerUse = Random.Range(150, 450);
                    }
                }
            }

            week = 1;
        }

        if (month == 13)
        {
            year++;
            month = 0;
        }

        if (year == 50)
        {
            CalculateScore();
            gameOverPanel.SetActive(true);
        }

        //Every 5 years, you'll gain 1 extra card per week!
        if (year == nextUpgrade)
        {
            nextUpgrade += 5;
            cardsLeftMax++;
        }

        GameStats["NetWorth"] = networth;
        GameStats["Money"] = money;

        yearsText.text =
            "W" + week.ToString() + " : M" + month.ToString() + " : Y" + year.ToString();

        TakeRepayments();
        CheckTenantTerms(); //Checks term amount and deal rent payments
        PayHiredManagers(); // Pay managers

        cardsLeft = cardsLeftMax;
        CheckPile();

        //Start of new week
        supplyDemandIndex += Random.Range(-0.015f, 0.02f);
        supplyDemandText.text = supplyDemandIndex.ToString("F2");

        cardsLeftText.text = cardsLeft.ToString() + "/" + cardsLeftMax.ToString();
    }

    public int CalculateRawIncome()
    {
        int x = 0; //Start at 0

        //Add up all the rent currently being collected
        foreach (GameObject item in buyPanels)
        {
            if (item.GetComponent<BuyPanel>().isOwned && item.transform.childCount > 1)
            {
                Card c = item.GetComponentInChildren<Card>();
                if (c.tenants)
                {
                    x += c.rent;
                }
            }
        }

        //Deduct all the loan repayments
        foreach (KeyValuePair<int, int[]> loan in Loans)
        {
            x -= loan.Value[7] / 4;
        }

        // Deduct payroll
        foreach (GameObject manager in hiredManagers)
        {
            x -= manager.GetComponent<Manager>().weeklyPay;
        }

        //Return whatever the numer ends up being
        return x;
    }

    public void PayHiredManagers()
    {
        int totalPayment = 0;
        foreach (GameObject manager in hiredManagers)
        {
            totalPayment += manager.GetComponent<Manager>().weeklyPay;
        }

        AddMoney(-totalPayment, "Manager Payroll");
    }

    public void GenerateManagersForHire()
    {
        int x = Random.Range(3, 20); // Number of managers to generate

        for (int i = 0; i < x; i++)
        {
            GameObject newManager = Instantiate(managerObject, managersForHirePanel.transform);

            newManager.GetComponent<Toggle>().group =
                managersForHirePanel.GetComponent<ToggleGroup>();
            newManager.name = "manager_" + i.ToString();

            Manager newManagerComponent = newManager.GetComponent<Manager>();

            newManagerComponent.gameManager = this;

            newManagerComponent.weeklyPay = Random.Range(100, 250);
            newManagerComponent.hireCost = Random.Range(1000, 5000);

            float newManagerBonusAmount = (float)System.Math.Round(Random.Range(1.01f, 1.08f), 2);
            newManagerComponent.bonusAmount = newManagerBonusAmount;

            newManagerComponent.description +=
                "- $"
                + newManagerComponent.hireCost
                + " to hire, $"
                + newManagerComponent.weeklyPay.ToString("#,##0")
                + " weekly pay\n\n- Auto find tenants\n- Increases rent by "
                + ((newManagerComponent.bonusAmount - 1) * 100).ToString("F2")
                + "%";
            newManager.transform.GetChild(0).GetComponent<Image>().sprite = managerSprites[
                Random.Range(0, 40)
            ];

            managersForHire.Add(newManager);
        }
    }

    public void SelectManager(GameObject manager)
    {
        Manager m = manager.GetComponent<Manager>();

        if (m.hired)
        {
            //Manager is an employee, fill out the selectedHiredManagerPanel
            selectedHiredManagerIcon.GetComponent<Image>().sprite =
                manager.transform.GetChild(0).GetComponent<Image>().sprite;

            selectedHiredManagerPanel.transform.GetChild(1).GetComponent<Text>().text =
                m.description;

            selectedHiredManager = manager;

            fireManagerButton.interactable = true;
        }
        else
        {
            //Manager is NOT an employee, fill out the selectedManagerPanel
            selectedManagerIcon.GetComponent<Image>().sprite =
                manager.transform.GetChild(0).GetComponent<Image>().sprite;

            selectedManagerPanel.transform.GetChild(1).GetComponent<Text>().text = m.description;

            selectedManager = manager;
        }
    }

    public void HireManager()
    {
        if (!selectedManager || currentManagersPanel.transform.childCount == 36)
            return;

        Manager hiredManager = selectedManager.GetComponent<Manager>();

        if (money < hiredManager.hireCost)
            return;

        AddMoney(-hiredManager.hireCost, "Staff Hire Payment");
        deductions.Add(hiredManager.name, hiredManager.weeklyPay);

        hiredManager.hired = true;

        managersForHire.Remove(selectedManager);

        selectedManager.transform.SetParent(currentManagersPanel.transform);
        selectedManager.GetComponent<Toggle>().group =
            currentManagersPanel.GetComponent<ToggleGroup>();

        GameObject duplicateManager = Instantiate(selectedManager);
        duplicateManager.transform.SetParent(propertyManagersPanel.transform);

        duplicateManager.name = hiredManager.name;

        duplicateManager.GetComponent<Toggle>().group =
            propertyManagersPanel.GetComponent<ToggleGroup>();

        duplicateManager.transform.localScale = Vector3.one;

        hiredManagers.Add(duplicateManager);

        if (managersForHire.Count == 0)
            GenerateManagersForHire();

        SelectManager(managersForHire[0]);
    }

    public void FireManager()
    {
        // Remove wage from payroll
        deductions.Remove(selectedHiredManager.name);

        // Remove from hired managers panel
        GameObject managerToDelete = GameObject.Find(selectedHiredManager.name);
        Destroy(managerToDelete);

        // Remove from property panel
        Destroy(hiredManagers.Find(g => g.name == selectedHiredManager.name));

        // Remove the manager from the hiredManagers list
        hiredManagers.Remove(hiredManagers.Find(g => g.name == selectedHiredManager.name));

        if (hiredManagers.Count == 0)
        {
            selectedHiredManagerIcon.GetComponent<Image>().sprite = exclamation;
            selectedHiredManagerPanel.transform.GetChild(1).GetComponent<Text>().text =
                "Select Manager";
            selectedHiredManager = null;
            return;
        }

        SelectManager(hiredManagers[0]);
    }

    public void CheckTenantTerms()
    {
        foreach (GameObject panel in buyPanels)
        {
            if (panel.transform.childCount > 1)
            {
                Card c = panel.transform.GetChild(1).GetComponent<Card>();
                int propertySlot = int.Parse(panel.name.Substring(panel.name.Length - 1)) - 1;

                if (!c.tenants)
                {
                    tenancyTexts[propertySlot].text = "";
                    continue;
                }

                //Check if the tenants term has run out
                if (c.tenantTermRemaining == 0)
                {
                    c.tenants = false;
                    c.tenantTerm = 0;
                    c.tenantTermRemaining = 0;

                    c.transform.parent
                        .GetComponent<BuyPanel>()
                        .openPropertySlotButton.GetComponent<Image>().sprite = exclamation;
                    c.transform.parent
                        .GetComponent<BuyPanel>()
                        .openPropertySlotButton.GetComponent<Image>().color = Color.red;
                    continue;
                }

                //Check if the tenants rent is due
                AddMoney((c.rent), "Rent Income");

                if (week == c.tenantMoveInWeek)
                {
                    c.tenantTermRemaining--;
                    tenancyTexts[propertySlot].text = c.tenantTermRemaining.ToString();
                    GameStats["TotalMonthsStayed"]++;
                }
            }
        }
    }

    public void TakeRepayments()
    {
        if (Loans.Count > 0)
        {
            int i = 0;
            foreach (KeyValuePair<int, int[]> loan in Loans)
            {
                i = loan.Key;
                int defaults = loan.Value[4];
                int amountOwing = loan.Value[2] - loan.Value[3];

                if (amountOwing == 0)
                    continue;

                if (defaults < 3)
                {
                    //Check the loan hasn't defaulted
                    int paymentWeek = loan.Value[5];

                    if (week == paymentWeek && loan.Value[2] > loan.Value[3])
                    {
                        //Check if this week is the payment week
                        int repayments = loan.Value[7];

                        if (repayments > money && amountOwing > money)
                        {
                            //Check if the user has defaulted on this loan
                            loan.Value[4]++; //Add a strike to the defaults and move to the next loan
                            continue;
                        }

                        //Check if this is out final payment
                        if (repayments >= amountOwing)
                        {
                            //This payment will put us in credit{
                            AddMoney(-amountOwing, "Final Loan Payment"); //Pay the final difference
                            loan.Value[3] = loan.Value[2];

                            GameObject
                                .Find(loan.Key.ToString())
                                .transform.GetChild(2)
                                .gameObject.SetActive(true);
                            GameObject
                                .Find(loan.Key.ToString())
                                .transform.GetChild(3)
                                .GetComponent<Button>().interactable = false;

                            UpdateLoanText(loan.Key);

                            //Increase credit limit by 5% of the finialized loan
                            creditLimit += Mathf.FloorToInt(loan.Value[2] * 0.075f);
                            creditLimitText.text = "Credit Limit: " + creditLimit.ToString("#,##0");
                            loanAmountSlider.maxValue = creditLimit;
                            continue;
                        }

                        //User can make a payment this month if we reach this line
                        AddMoney(-loan.Value[7], "Loan Payment"); //Take payment
                        loan.Value[3] += loan.Value[7];

                        //Add the amount back to our credit limit
                        creditLimit += loan.Value[7];
                        creditLimitText.text = "Credit Limit: $" + creditLimit.ToString("#,##0");
                        loanAmountSlider.maxValue = creditLimit;
                    }
                }
                else
                {
                    //Force sell random assets until debt is paid.
                    int collectedValue = money;
                    List<Card> propertiesToBeRepod = new List<Card>();

                    foreach (GameObject panel in buyPanels)
                    {
                        if (panel.transform.childCount > 1)
                        {
                            Card c = panel.transform.GetChild(1).GetComponent<Card>();

                            collectedValue += Mathf.FloorToInt(c.cost * 0.90f);
                            propertiesToBeRepod.Add(c);

                            if (collectedValue > amountOwing)
                                break;
                        }
                    }

                    int totalPayable = collectedValue;

                    //Force sell the property
                    foreach (Card property in propertiesToBeRepod)
                    {
                        AddMoney(Mathf.FloorToInt(property.cost * 0.90f), "Property Sale"); //Force sell the property at %90 its value
                        property.Destroy();
                    }

                    //Pay off the loan, leaving the difference in collectedValue and AmountOwing
                    AddMoney(-amountOwing, "Loan Payment");

                    //Close the loan account
                    loan.Value[3] = loan.Value[2];

                    loanAmountSlider.maxValue = creditLimit;

                    GameObject
                        .Find(loan.Key.ToString())
                        .transform.GetChild(2)
                        .gameObject.SetActive(true);
                    GameObject
                        .Find(loan.Key.ToString())
                        .transform.GetChild(3)
                        .GetComponent<Button>().interactable = false;

                    //Increase credit limit by 5% of the finialized loan
                    creditLimit += Mathf.FloorToInt(loan.Value[2] * 0.05f);
                    creditLimitText.text = "Credit Limit: " + creditLimit.ToString("#,##0");
                    loanAmountSlider.maxValue = creditLimit;

                    if (money < 0)
                    {
                        //Game Over at this point?
                        gameOverPanel.SetActive(true);

                        CalculateScore();

                        CancelInvoke("CalculateNetWorth");
                        return;
                    }
                }

                UpdateLoanText(i);
            }
        }
    }

    public void UpdateLoanText(int key)
    {
        contractsText[key].text = "Account #" + Loans[key][8].ToString();
        loansAccountText[key].text =
            "Payable: $"
            + (Loans[key][2] - Loans[key][3]).ToString("#,##0")
            + "\nMonthly: $"
            + Loans[key][7].ToString("#,##0")
            + " ("
            + Loans[key][5].ToString()
            + ")\nTerm: "
            + Loans[key][0].ToString();
        defaultTexts[key].text = "Defaults: " + Loans[key][4].ToString() + "/3";
        debtText.text = "Debt: -$" + CalculateTotalDebt("debt").ToString("#,##0");
    }

    public void RepeatingWeeks()
    {
        if (weeksPerMinute > 0)
        {
            NextWeek();
        }
    }

    public void IncreaseWeeksPerSecond()
    {
        if (weeksPerMinute < maxWeeksPerMinute)
        {
            weeksPerMinute++;
            weeksPerSecondText.text = weeksPerMinute.ToString();
            CancelInvoke("RepeatingWeeks");
            InvokeRepeating(
                "RepeatingWeeks",
                (60 - weeksPerMinute) * timeBuffer,
                (60 - weeksPerMinute) * timeBuffer
            );
        }
    }

    public void DecreaseWeeksPerSecond()
    {
        if (weeksPerMinute > 0)
        {
            weeksPerMinute--;
            weeksPerSecondText.text = weeksPerMinute.ToString();

            CancelInvoke("RepeatingWeeks");
            InvokeRepeating(
                "RepeatingWeeks",
                (60 - weeksPerMinute) * timeBuffer,
                (60 - weeksPerMinute) * timeBuffer
            );
        }
    }

    public void PassCard()
    {
        if (CardPile.childCount > 0)
        {
            CardPile.GetChild(0).GetComponent<Card>().Destroy();

            CardPile.DetachChildren();
            GameStats["TotalPassedProperties"]++;

            CheckPile();
        }
    }

    public void TakeLoan()
    {
        int term = GetSelectedToggle();
        int amnt = (int)loanAmountSlider.value;

        int amntPayable = CalculateAmountPayable(term, amnt);

        if (
            creditLimit >= amnt /*&& (rent * 4) < repayments*/
        )
        {
            //Maybe in the future, add rental income as a factor?
            int key = Random.Range(11111, 99999);

            GameObject newLoanObj = Instantiate(LoanObject, LoanObjectContainer.transform);
            newLoanObj.name = key.ToString();

            contractsText[key] = newLoanObj.transform.GetChild(0).GetComponent<Text>();
            loansAccountText[key] = newLoanObj.transform.GetChild(1).GetComponent<Text>();
            defaultTexts[key] = newLoanObj.transform.GetChild(2).GetComponent<Text>();

            int repayments = amntPayable / term;
            int paymentStartdate = int.Parse(week.ToString() + month.ToString() + year.ToString());

            /* 0 = Loan Term.
            1 = Amount Loaned
            2 = Total Loan Cost (Amount Loaned + Total Interest (to be) Paid)
            3 = Total Amount Paid Back
            4 = Default Strike Count
            5 = Payment Date
            6 = Loan Issue Date
            7 = Repayments
            8 = Random Loan Account Number*/
            int[] loan = { term, amnt, amntPayable, 0, 0, week, paymentStartdate, repayments, key };

            Loans[key] = loan;

            newLoanObj.transform
                .GetChild(3)
                .GetComponent<Button>()
                .onClick.AddListener(
                    delegate()
                    {
                        PayLoanFull(key);
                    }
                );

            AddMoney(amnt, "Loan Deposit");

            creditLimit -= amnt;
            creditLimitText.text = "Credit Limit: " + creditLimit.ToString("#,##0");
            loanAmountSlider.maxValue = creditLimit;

            totalDebt = CalculateTotalDebt("totalDebt");

            UpdateLoanText(key);
        }
    }

    public void PayLoanFull(int key)
    {
        int amountOwing = Loans[key][2] - Loans[key][3];
        if (money >= amountOwing)
        {
            AddMoney(-amountOwing, "Final Loan Payment");

            creditLimit += amountOwing;
            creditLimit += Mathf.FloorToInt(Loans[key][2] * 0.07f);
            creditLimitText.text = "Credit Limit: $" + creditLimit.ToString("#,##0");

            loanAmountSlider.maxValue = creditLimit;

            GameObject
                .Find(key.ToString())
                .transform.GetChild(3)
                .GetComponent<Button>().interactable = false;
            GameObject.Find(key.ToString()).transform.GetChild(4).gameObject.SetActive(true);

            Loans[key][3] = Loans[key][2];
            Loans[key][7] = 0;
            UpdateLoanText(key);
        }

        return;
    }

    public int CalculateAmountPayable(int term, int amnt)
    {
        int totalPayable = amnt;

        term = GetSelectedToggle();

        float interestRate = 0.00f;

        switch (term)
        {
            case 12:
                interestRate = 2.55f;
                break;
            case 24:
                interestRate = 1.89f;
                break;
            case 36:
                interestRate = 1.29f;
                break;
            case 48:
                interestRate = 0.29f;
                break;
        }

        totalPayable = Mathf.FloorToInt(amnt * (1 + (interestRate / 100)));

        return totalPayable;
    }

    int GetSelectedToggle()
    {
        foreach (Toggle t in termSelectors)
            if (t.isOn)
                return int.Parse(t.name); //returns selected toggle

        return 99;
    }

    public void UpdateLoanAmountSliderText()
    {
        loanAmountText.text = "$" + loanAmountSlider.value.ToString("#,##0");
        int amountPayable = CalculateAmountPayable(
            GetSelectedToggle(),
            (int)loanAmountSlider.value
        );
        loanTotalText.text =
            "Total Repayable: $"
            + amountPayable.ToString("#,##0")
            + "\nTotal Interest: $"
            + (amountPayable - loanAmountSlider.value).ToString("#,##0")
            + "\n Monthly Repayments: $"
            + (amountPayable / GetSelectedToggle()).ToString()
            + " p/m";
    }

    public void ToggleSound()
    {
        sounds = !sounds;

        if (sounds)
            soundToggle.GetComponent<Image>().sprite = soundOn;
        else
            soundToggle.GetComponent<Image>().sprite = soundOff;

        if (sounds)
            ambientSource.PlayOneShot(buttonClick);
    }

    public void ToggleMusic()
    {
        music = !music;

        if (music)
        {
            musicToggle.GetComponent<Image>().sprite = musicOn;
            BackgroundMusic();
        }
        else
        {
            musicToggle.GetComponent<Image>().sprite = musicOff;
            source.Stop();
        }

        if (sounds)
            ambientSource.PlayOneShot(buttonClick);
    }

    public void CalculateScore()
    {
        int score =
            GameStats["TotalPropertiesOwned"]
            + (GameStats["TotalMoneySpent"] / 100000)
            + GameStats["TotalPropertiesSold"]
            + (GameStats["MoneySpentOnUpgrades"] / 100000)
            + GameStats["TotalPassedProperties"]
            + GameStats["TotalUpgrades"]
            + (GameStats["MostExpensiveProperty"] / 100000)
            + (GameStats["MostExpensivePurchased"] / 100000)
            + (GameStats["HighestRental"] / 10000)
            + GameStats["TotalNumberTenants"]
            + GameStats["TotalMonthsStayed"]
            + (GameStats["HighestNetworth"] / 100000)
            + (GameStats["MostAmountOfMoney"] / 100000);

        finalScoreText.text = "Score: " + score.ToString("#,##0");

        primaryStatsText.text =
            "Properties Owned: "
            + GameStats["TotalPropertiesOwned"]
            + "\n"
            + "Properties Sold:"
            + GameStats["TotalPropertiesSold"]
            + "\n"
            + "Properties Skipped: "
            + GameStats["TotalPassedProperties"]
            + "\n\n"
            + "Total Spend On Upgrades: $"
            + GameStats["MoneySpentOnUpgrades"].ToString("#,##0")
            + "\n"
            + "Total Upgrades Purchased: "
            + GameStats["TotalUpgrades"]
            + "\n"
            + "Total Money Spent: $"
            + GameStats["TotalMoneySpent"].ToString("#,##0");

        otherStatsText.text =
            "Most Expensive Property Owned: $"
            + GameStats["MostExpensiveProperty"].ToString("#,##0")
            + "\n"
            + "Most Expensive Property Purchased: $"
            + GameStats["MostExpensivePurchased"].ToString("#,##0")
            + "\n"
            + "Highest Rental Cost: $"
            + GameStats["HighestRental"].ToString("#,##0")
            + "\n"
            + "Number Of Tenants Housed: "
            + GameStats["TotalNumberTenants"]
            + "\n"
            + "Combined Months Tenants Have Leased: "
            + GameStats["TotalMonthsStayed"]
            + "\n\n"
            + "Highest Networth Obtained: $"
            + GameStats["HighestNetworth"].ToString("#,##0")
            + "\n"
            + "Most Amount Of Money Had: $"
            + GameStats["MostAmountOfMoney"].ToString("#,##0");

        PlayerPrefs.SetInt("UnclaimedPoints", score);
    }

    public void ResetGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}

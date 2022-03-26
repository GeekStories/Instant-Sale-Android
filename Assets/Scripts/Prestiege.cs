using UnityEngine;
using UnityEngine.UI;

public class Prestiege : MonoBehaviour {
    public Text unclaimedPointsText;
    public Text pointsText;

    public int points, unclaimedPoints;

    GameManager gameManager;

    private void Start() {
        gameManager = GetComponent<GameManager>();
        unclaimedPoints = PlayerPrefs.GetInt("UnclaimedPoints");
    }

    public void UpdatePointsText(){
        unclaimedPointsText.text = "Unclaimed: " + unclaimedPoints.ToString("#,##0");
    }

    public void ClaimPoints(){
        if (unclaimedPoints !> 0)
            return;

        points += unclaimedPoints;
        unclaimedPoints = 0;

        //Reset the game
        gameManager.ResetGame();
    }
}

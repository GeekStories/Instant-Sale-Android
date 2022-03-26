using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour {
    public Image currentImage;

    public Sprite[] tutorialImages;
     
    private int section = 0;

    private void Start(){
        currentImage.sprite = tutorialImages[0];
    }

    public void OpenSection(int section){
        currentImage.sprite = tutorialImages[section];
    }

    public void OpenCloseTutorial(){
        //Reset tutorial values
        currentImage.sprite = tutorialImages[0];
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }
}

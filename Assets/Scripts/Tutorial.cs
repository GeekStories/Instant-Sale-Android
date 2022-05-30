using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour {
  public Image currentImage;
  public ScrollRect navigationButtonsScrollContainer;
  public Sprite[] tutorialImages;

  private void Start() {
    currentImage.sprite = tutorialImages[0];
  }
  public void OpenSection(int section) {
    navigationButtonsScrollContainer.verticalNormalizedPosition = 1;
    currentImage.sprite = tutorialImages[section];
  }
  public void OpenCloseTutorial() {
    //Reset tutorial values
    currentImage.sprite = tutorialImages[0];
    gameObject.SetActive(!gameObject.activeInHierarchy);
  }
}

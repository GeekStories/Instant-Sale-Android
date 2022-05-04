using UnityEngine;

public enum prestiegeType {
  STARTING_CASH,
  FIRST_UPGRADE,
  RENT_MODIFIER,
  MANAGER_MODIFIER
}

[CreateAssetMenu(
    fileName = "PrestiegeUpgrade_",
    menuName = "Prestiege Objects/PrestiegeUpgrade",
    order = 1
)]
public class PrestiegeUpgrade : MonoBehaviour {
  public string Name;
  public string Description;
  public prestiegeType Type;
  public float ModifierAmount;
  public int Cost;
  public bool Purchased;
}

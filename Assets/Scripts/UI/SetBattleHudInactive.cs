using UnityEngine;

public class SetBattleHudInactive : MonoBehaviour
{
    public void SetBattleHudInactiveEvent()
    {
        FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>().battleHud.SetActive(false);
    }
}

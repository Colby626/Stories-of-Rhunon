using UnityEngine;

public class SetBattleHudInactive : MonoBehaviour
{
    public void SetBattleHudInactiveEvent() //Called from animation event in BattleHudAnimator
    {
        FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>().battleHud.SetActive(false);
    }
}

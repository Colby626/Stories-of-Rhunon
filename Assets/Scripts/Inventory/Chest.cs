using UnityEngine;

public class Chest : MonoBehaviour
{
    [Tooltip("The distance away from enemies a chest must be to unlock")]
    public float lockDistance = 7;
    private bool enemiesNearby = true;
    private void Update()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, lockDistance);
        enemiesNearby = false;
        foreach (Collider2D collider in colliders)
        {
            if (collider.GetComponent<CharacterSheet>())
            {
                if (!collider.GetComponent<CharacterSheet>().isPlayer) //If they are an enemy, lock the chest
                {
                    transform.GetChild(0).gameObject.SetActive(true);
                    enemiesNearby = true;
                }
            }
        }
        if (!enemiesNearby) //If no enemies nearby, unlock the chest
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    } 
}

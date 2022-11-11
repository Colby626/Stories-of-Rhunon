using UnityEngine;

public class EnemyMovement : MonoBehaviour //Base EnemyMovement class that certain enemy AIs will derive from
{
    public int viewRange;
    public int wanderRange;

    public void Wander()
    {
        //pick random spots around it and wander around 

        //Take current node and get collection of neighbors around it with a distance of wanderRange
        //Pick a random node from that list and move to it
        //Do this again and again until the player engages
    }

    public void Approach()
    {
        //Move towards the player's party

        //Find the node closest to the player's party from the side the enemy is on and start moving towards it 
    }
}

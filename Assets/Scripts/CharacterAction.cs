using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterAction : MonoBehaviour
{
    public int test;
    public UnityEvent attackFinish;

    public void FinishedAttack()
    {
        attackFinish.Invoke();
    }

    public virtual void DoAction(GameObject character)
    {
    }
}

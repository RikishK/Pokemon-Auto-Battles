using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    public int energyCost;
    public int currEnergy;
    public float castTime;

    public virtual void useAbility(Pokemon target, int myAtk, int mySpAtk)
    {
        Debug.Log("ABILITY");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineWhip : Ability
{
    public GameObject vinewhip_obj;
    public override void useAbility(Pokemon target, int myAtk, int mySpAtk)
    {
        StartCoroutine(spawnVineWhip(target, myAtk));
        currEnergy = 0;
        Debug.Log("VINEWHIP");
        
    }

    IEnumerator spawnVineWhip(Pokemon target, int damage)
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(timedDeath(target, damage));
        
    }

    IEnumerator timedDeath(Pokemon target, int damage)
    {
        GameObject vwhip = Instantiate(vinewhip_obj);
        vwhip.transform.position = target.transform.position;
        target.takeAtkDamage((int)(60 + damage * 0.6));
        yield return new WaitForSeconds(0.7f);
        Destroy(vwhip);
    }
}

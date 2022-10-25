using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ember : Ability
{
    public GameObject ember_obj;
    public override void useAbility(Pokemon target, int myAtk, int mySpAtk)
    {
        StartCoroutine(spawnEmber(target, mySpAtk));
        currEnergy = 0;
        Debug.Log("EMBER");

    }

    IEnumerator spawnEmber(Pokemon target, int damage)
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(timedDeath(target, damage));

    }

    IEnumerator timedDeath(Pokemon target, int damage)
    {
        GameObject em = Instantiate(ember_obj);
        em.transform.position = target.transform.position;
        target.takeSpAtkDamage((int)(60 + damage * 0.6));
        yield return new WaitForSeconds(0.7f);
        currEnergy = 0;
        Destroy(em);
    }
}

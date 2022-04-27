using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineWhip : Ability
{
    public GameObject vinewhip_obj;
    public override void useAbility(Pokemon target)
    {
        StartCoroutine(spawnVineWhip(target));
        currEnergy = 0;
        Debug.Log("VINEWHIP");
        
    }

    IEnumerator spawnVineWhip(Pokemon target)
    {
        yield return new WaitForSeconds(1f);
        GameObject vwhip = Instantiate(vinewhip_obj);
        vwhip.transform.position = target.transform.position;
    }
}

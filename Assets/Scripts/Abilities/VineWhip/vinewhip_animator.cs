using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vinewhip_animator : MonoBehaviour
{
    public float castTime;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Vinewhip Spawned");
        StartCoroutine(timedDeath());   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator timedDeath()
    {
        yield return new WaitForSeconds(castTime);
        Destroy(gameObject);
    }
}

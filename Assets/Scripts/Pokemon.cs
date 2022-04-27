using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pokemon : MonoBehaviour
{
    //Stats
    public int health;
    public int currHealth;
    public int atk;
    public int spatk;
    public int def;
    public int spdef;
    public float speed;
    public int range;

    //Health Bar and Mana Bar
    public Slider healthSlider;
    public Slider energySlider;

    //Ability Stuff
    public GameObject myAbilityHolder;
    public Ability myAbility;

    //Arena Stuff
    public Arena myArena;
    public ArenaNode myNode;
    public Arena.Team myTeam;


    //Battle Stuff
    public BattleState battleState = BattleState.Searching;
    Pokemon myTarget;

    //Animation Stuff
    public Animator animator;


    public enum BattleState
    {
        Searching, Moving, Attacking, Ability
    };

    // Start is called before the first frame update
    void Start()
    {
        myNode.occupied = true;
        currHealth = health;
        myAbilityHolder = Instantiate(myAbilityHolder);
        myAbility = myAbilityHolder.GetComponent<Ability>();
        setupBars();
        setupAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void setupAnimation()
    {
        animator.SetFloat("BasicAttackSpeed", 1f / speed);
    }
    void setupBars()
    {
        healthSlider.maxValue = health;
        healthSlider.value = currHealth;
        energySlider.maxValue = myAbility.energyCost;
        energySlider.value = myAbility.currEnergy;
    }

    void updateBar()
    {
        healthSlider.maxValue = health;
        healthSlider.value = currHealth;
        energySlider.maxValue = myAbility.energyCost;
        energySlider.value = myAbility.currEnergy;
    }
    public void battle()
    {
        switch (battleState)
        {
            case (BattleState.Searching):
                myTarget = myArena.getClosestTarget(myTeam, myNode);
                if(myTarget != null)
                {
                    battleState = BattleState.Moving;
                    battle();
                }
                break;
            case (BattleState.Moving):
                
                ArenaNode nextNode = myArena.moveInRange(myNode, myArena.getClosestEmptyNeighbour(myNode, myTarget), myTarget, range);
                //Debug.Log(myNode.x + "," + myNode.y + " | " + nextNode.x + "," + nextNode.y);
                if ((nextNode.x == myNode.x) && (nextNode.y == myNode.y))
                {
                    //Debug.Log("Close enough");
                    battleState = BattleState.Attacking;
                    battle();
                }
                else
                {
                    animator.SetBool("Walking", false);
                    faceNode(nextNode);
                    StartCoroutine(MoveToSpot(nextNode));
                    
                }
                break;
            case (BattleState.Attacking):

                if (checkAbility())
                {
                    battleState = BattleState.Ability;
                    battle();
                }
                else
                {
                    int d = myArena.distance_BFS(myNode, myTarget.myNode);
                    if(d <= range) //Do basic Attack
                    {
                        StartCoroutine(BasicAttack());
                    
                    }
                    else //Search for target
                    {
                        battleState = BattleState.Searching;
                        battle();
                    }
                }

                

                break;
            case (BattleState.Ability):
                
                useMyAbility();
                break;
        }
    }

    IEnumerator MoveToSpot(ArenaNode targetNode)
    {

        animator.SetBool("Walking", true);
        changeNode(targetNode);
        transform.position = Vector3.MoveTowards(transform.position, myNode.transform.position, 0.1f);
        yield return new WaitForSeconds(0.25f);
        animator.SetBool("Walking", false);
        if (transform.position == myNode.transform.position)
        {
            
            
            battle();
        }
        else
        {
            StartCoroutine(MoveToSpot(targetNode));
        }

    }

    IEnumerator BasicAttack()
    {
        faceNode(myTarget.myNode);
        animator.SetBool("Basic_Attacking", true);
        yield return new WaitForSeconds(1f/speed);
        dealAtkDamage(atk, myTarget);
        animator.SetBool("Basic_Attacking", false);
        
        battle();
    }

    void faceNode(ArenaNode targetNode)
    {
        resetFaceDirection();
        int x = targetNode.x - myNode.x;
        int y = targetNode.y - myNode.y;

        if(y == 0)
        {
            if (x > 0)
            {
                animator.SetBool("R", true);
            }
            if (x < 0)
            {
                animator.SetBool("L", true);
            }
        }
        if(y < 0)
        {
            if(myNode.y % 2 == 0)
            {
                if(x == 0)
                {
                    animator.SetBool("UR", true);
                }
                if ( x == -1)
                {
                    animator.SetBool("UL", true);
                }
            }
            else
            {
                if (x == 0)
                {
                    animator.SetBool("UL", true);
                }
                if (x == 1)
                {
                    animator.SetBool("UR", true);
                }
            }
        }
        if (y > 0)
        {
            if (myNode.y % 2 == 0)
            {
                if (x == 0)
                {
                    animator.SetBool("DR", true);
                }
                if (x == -1)
                {
                    animator.SetBool("DL", true);
                }
            }
            else
            {
                if (x == 0)
                {
                    animator.SetBool("DL", true);
                }
                if (x == 1)
                {
                    animator.SetBool("DR", true);
                }
            }
        }
    }

    void changeNode(ArenaNode newNode)
    {
        myNode.occupied = false;
        myNode = newNode;
        myNode.occupied = true;
    }
    void resetFaceDirection()
    {
        animator.SetBool("R", false);
        animator.SetBool("UR", false);
        animator.SetBool("DR", false);
        animator.SetBool("L", false);
        animator.SetBool("UL", false);
        animator.SetBool("DL", false);
    }

    void resetAnim()
    {
        animator.SetBool("Walking", false);
        animator.SetBool("Basic_Attacking", false);
        animator.SetBool("CastingAbility", false);
    }

    void dealAtkDamage(int damage, Pokemon target)
    {
        myAbility.currEnergy += 10;
        //Debug.Log("Enegery: " + myAbility.currEnergy + " FROM " + myTeam.ToString());
        target.takeAtkDamage(damage);
    }

    public void takeAtkDamage(int damage)
    {
        //Debug.Log("Taking Damage");
        int d = Mathf.Clamp(damage - def, 1, damage);
        currHealth = Mathf.Clamp(currHealth - d, 0, health);
        myAbility.currEnergy += 5;
        updateBar();


    }

    bool checkAbility()
    {
        return myAbility.currEnergy >= myAbility.energyCost;
    }

    void useMyAbility()
    {
        StartCoroutine(castAbility());
    }

    IEnumerator castAbility()
    {
        float secs = myAbility.castTime;

        //do animation
        resetAnim();
        animator.SetBool("CastingAbility", true);
        faceNode(myTarget.myNode);
        myAbility.useAbility(myTarget);
        yield return new WaitForSeconds(secs);
        
        //Debug.Log("Ability Done");
        resetAnim();
        battleState = BattleState.Searching;
        updateBar();
        battle();
    }
}

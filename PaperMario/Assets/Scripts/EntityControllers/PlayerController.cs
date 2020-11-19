using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerBattleSelections { Sword,Jump,Item,Special,Misc}

public class PlayerController : Entity
{

    [Header("Player Controller Variables")]
    public int playersMaxHealth;
    public bool isLockout;// Locks out the character when dying


    [Header("Player Controller Audio SFX")]
    //public AudioClip fireBallSFX;

    //For Jumping
    float jumpButtonHeldDown;
    float time;
    const float JUMP_BUTTON_QUICK_RELEASE_TIME = 0.25f;

    //Battle
    public PlayerBattleSelections playerBattleState;

    //Reference to the partner
    GameObject partner;
    PartnerController partnerController;

    //For the Player UI, if no input then display stats

    public override void Awake()
    {
        #region Starting Setters

        base.Awake();

        base.setMaxHealth = playersMaxHealth;

        if (inBattle == false)
        {
            partner = GameObject.Find("Partner");

            partnerController = partner.GetComponent<PartnerController>();
        }

        #endregion

        #region Precautions Check

        if (!isLockout)
        {
            isLockout = true;

            Debug.Log("Player Starts Locked Out");
        }

        if (getMaxHealth < 10 && inBattle == false)
        {
            Debug.Log("Players max health starts at less than 10, setting to 10");
            setMaxHealth = 10;
        }

        //Testing The UI
        currHealth = getMaxHealth;

        #endregion
    }


    void Update()
    {
        //Test Area

        if (Input.GetKeyDown(KeyCode.Q))
        {
            DamageTaken(1);
        }

        if (Input.anyKeyDown)
        {

        }

        //End of Test Area

        base.CalledEveryUpdate();

        if (isLockout)
        {

            if (inBattle == false)
            {

                leftRightMoveValue = Input.GetAxisRaw("Horizontal");
                upDownMoveValue = Input.GetAxisRaw("Vertical");

                if (Input.GetButtonDown("Interact"))
                {
                    Jump();
                    jumpButtonHeldDown = 0.05f;
                }
                if (Input.GetButton("Interact"))
                {
                    time += Time.deltaTime;
                }

                if (Input.GetButtonUp("Interact"))
                {
                    jumpButtonHeldDown = -0.05f;

                    if (time < JUMP_BUTTON_QUICK_RELEASE_TIME)
                    {
                        jumpButtonHeldDown = 0;
                    }
                }


                rb.velocity = new Vector3(leftRightMoveValue * speed, rb.velocity.y + jumpButtonHeldDown, upDownMoveValue * speed);
                //Debug.Log(rb.velocity + " Players Velocity");
                if (isGrounded)
                    time = 0;

            }
            else //Player is in battle
            {

            }
        }

        base.AnimatorUpdate();

    }

    void OnCollisionEnter(Collision c)
    {
        if (c.transform.GetComponent<EnemyController>() && inBattle == false)
        {
            GM.BattleSceneEntered(c.transform.GetComponent<EnemyController>().enemyTeam);
        }
    }

    #region Entity Override

    protected override void Jump()
    {
        base.Jump();
        if (isGrounded && inBattle == false)
        {
            partnerController.StartCoroutine("PartnerJump");
        }
    }

    public override void Flip()
    {
        if (inBattle == false)
        {
            partnerController.PlayerFlippped();
        }
        base.Flip();
    }

    public override void DamageTaken(int damageTaken)
    {
        base.DamageTaken(damageTaken);

    }

    public override void DeathStart()
    {
        base.DeathStart();
        Debug.Log("Player Should die here");
    }

    #endregion

    #region Skills - Player Jump

    public void BattleJump()
    {

    }

    #endregion

    #region Skills - Player Jump

    public void BattleSlash()
    {
        Debug.Log("Slash");
    }

    #endregion

    #region Overworld Functions

    //void PlayerTakesDamageOverworld(int damage)
    //{
    //    StartCoroutine(GM.GeneralPlayerUI.PlayerDamagedInOverworld(currHealth, getMaxHealth, 1));
    //    currHealth -= damage;

    //    if(currHealth <=0)
    //    {
    //        //Call game over function
            
    //    }
    //}

    #endregion

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Partners { Rock = 1, Slug, Brock, Troy }

public enum PartnerBattleSelections { Attack, Item, Misc }

public class PartnerController : Entity {

    [Header("Partner")]
    public Partners whichPartner;

    [Header("Partner Controller Variables")]//Must be above a public variable to show in inspector
    public int partnersMaxHealth;
    [Tooltip("Distance between the player and partner before they start following the player again")]
    public float playerDistanceThreshold;
    [System.NonSerialized]
    public bool playerOutOfRange;
    float distance;//distance between player & partner in the outer world
    [Tooltip("Delay between players jump and partners jump")]
    public float jumpDelay;
    float timeOutOfRange;

    //Battle
    public PartnerBattleSelections partnerBattleState;
    public Sprite partnerBattleHealthSpriteUI;

    //Reference to Player
    GameObject player;
    //Reference to the follow position connected to the player
    Transform playerFollowTrans;

    // Use this for initialization
    public override void Awake() {

        base.setMaxHealth = partnersMaxHealth;

        #region Starting Setters

        base.Awake();

        if(inBattle == false)
        {
            player = GameObject.Find("Player");
            playerFollowTrans = GameObject.Find("Player/Graphics/PartnerFollowLocation").transform;
        }

        #endregion

        #region Precautions Check

        if (playerDistanceThreshold <=5)
        {
            Debug.Log("playerDistanceThreshold is too low or not set, setting to 6.5");
            playerDistanceThreshold = 6.5f;
        }

        if(jumpDelay <= 0)
        {
            Debug.Log("Jump Delay not set, setting to 1 second");
            jumpDelay = 1;
        }

        #endregion

    }

    // Update is called once per frame
    void Update()
    {
        base.CalledEveryUpdate();

        if (inBattle == false)
        {

            distance = Vector3.Distance(this.transform.position, player.transform.position);

            if (distance > playerDistanceThreshold)
            {
                playerOutOfRange = true;
            }

            if (playerOutOfRange)
            {
                timeOutOfRange += Time.deltaTime;

                leftRightMoveValue = playerFollowTrans.position.x - transform.position.x;
                Mathf.Clamp(leftRightMoveValue, -1, 1);
                leftRightMoveValue *= timeOutOfRange;
                Mathf.Clamp(leftRightMoveValue, -1, 1);

                upDownMoveValue = playerFollowTrans.position.z - transform.position.z;
                Mathf.Clamp(upDownMoveValue, -1, 1);
                upDownMoveValue *= timeOutOfRange;
                Mathf.Clamp(upDownMoveValue, -1, 1);

                rb.velocity = new Vector3(leftRightMoveValue * speed, rb.velocity.y, upDownMoveValue * speed);

                if (Vector3.Distance(playerFollowTrans.position, transform.position) < 1 && player.GetComponent<Rigidbody>().velocity.x == 0 && player.GetComponent<Rigidbody>().velocity.z == 0)
                {
                    rb.velocity = new Vector3(0, rb.velocity.y, 0);
                    playerOutOfRange = false;
                    timeOutOfRange = 0;
                }
            }
        }
        else
        {

        }

        base.AnimatorUpdate();

	}

    public void PlayerFlippped()
    {
        playerOutOfRange = false;//this is to stop the player from following the spining player follow location
        timeOutOfRange = 0;
    }

    IEnumerator PartnerJump()
    {
        float t = 0f;
        while (t < jumpDelay)
        {
            yield return null;
            t+=Time.deltaTime;
        }

        base.Jump();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { MovingToStartingPosition,CurrentTurn ,WaitingToAttack ,FinishedTurn ,CurrentlyAttacking ,Dead}

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class Entity : MonoBehaviour {

    //Components
    protected Rigidbody rb;
    protected AudioSource aSource;
    public Animator anim;

    //Movement Values
    protected float speed;
    protected float jumpSpeed;
    protected float flipSpeed;
    protected float leftRightMoveValue;
    protected float upDownMoveValue;
    bool _isFacingLeft = false;//All units start facing right
    bool _isFacingUp = false;//All units start with their front facing down

    //Graphics
    float originalYOffset;
    float scalefactorX = 0.1f;
    Transform graphics;
    protected LayerMask obstacleMask;

    //Ground Check Requirements
    protected bool isGrounded;
    protected LayerMask isGroundLayer;
    protected Transform groundCheck;
    protected float groundCheckRadius;

    //Battle
    bool _inBattle;
    public BattleState battleState;

    public delegate void myAttackStart();
    public myAttackStart myattackStart;

    public delegate void myAttackFinish();
    public myAttackFinish myattackFinish;

    //Reference To Managers
    protected GameManager GM;

    //[Header("Entitys Health")]
    int _maxHealth;
    //[System.NonSerialized]
    public int currHealth;

    protected int attack;
    protected int defense;

    [Header("Entity Audio SFX")]
    public AudioClip jumpSFX;

    #region Getters/Setters

    protected virtual bool isFacingLeft
    {
        get { return _isFacingLeft; }
        set
        {
            _isFacingLeft = value;
        }
    }

    protected virtual bool isFacingUp
    {
        get { return _isFacingUp; }
        set
        {
            _isFacingUp = value;
        }
    }

    public bool inBattle
    {
        get { return _inBattle; }
        set
        {
            _inBattle = value;
        }
    }

    public int getMaxHealth
    {
        get { return _maxHealth; }
    }
    
    protected int setMaxHealth
    {
        set
        {
            _maxHealth = value;
        }
    }

    #endregion

    // Use this for initialization
    public virtual void Awake() {

        GM = GameManager.instance;

        #region Starting Setters

        rb = GetComponent<Rigidbody>();

        aSource = GetComponent<AudioSource>();


        groundCheck = transform.Find("GroundCheck");

        graphics = this.transform.Find("Graphics");
        anim = graphics.GetComponent<Animator>();

        //Graphics offset
        originalYOffset = graphics.localEulerAngles.y;

        //Game Manager Settings
        //Movement Values
        speed = GM.speed;
        jumpSpeed = GM.jumpSpeed;
        flipSpeed = GM.flipSpeed;

        //Ground Check Values
        isGroundLayer = GM.isGroundLayer;
        groundCheckRadius = GM.groundCheckRadius;

        //LayerMasks
        obstacleMask = GM.obstacleMask;

        if(GM.inBattle == true)
        {
            inBattle = true;
        }
        else
        {
            inBattle = false;
        }

        #endregion

        #region Precautions Check

        //Component Values
        if (!rb)
        {
            Debug.Log("RigidBody not found on " + name);
        }

        if (!aSource)
        {
            Debug.Log("Audio Source not found on " + name);
        }

        if (!anim)
        {
            Debug.Log("Animator not found on " + name);
        }

        //Movement Values
        if (speed <= 0)
        {
            speed = 5.0f;

            Debug.Log("Speed not set. Defaulting to " + speed);
        }

        if (jumpSpeed <= 0)
        {
            jumpSpeed = 5.0f;

            Debug.Log("Jump Speed not set. Defaulting to " + jumpSpeed);
        }

        if (flipSpeed <= 0)
        {
            flipSpeed = 5.0f;

            Debug.Log("Flip Speed not set. Defaulting to " + flipSpeed);
        }

        //Ground Check
        if (!groundCheck)
        {
            Debug.Log("GroundCheck not found on " + name);
        }

        if (groundCheckRadius <= 0)
        {
            groundCheckRadius = 0.1f;

            Debug.Log("GroundCheckRadius not set. Defaulting to " + groundCheckRadius);
        }


        //Health
        if (getMaxHealth <= 0)
        {
            Debug.Log("Entity max health starts at less than 1, setting to 1 on " + gameObject.name);
            setMaxHealth = 1;
        }

        currHealth = getMaxHealth;

        #endregion
    }

    protected virtual void CalledEveryUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, isGroundLayer);

        //Takes and Flips The Entity
        if (leftRightMoveValue > 0 && isFacingLeft)
            Flip();
        else if (leftRightMoveValue < 0 && !isFacingLeft)
            Flip();

        if (upDownMoveValue < 0 && isFacingUp  && inBattle == false)
        {
            isFacingUp = false;
            scalefactorX = -scalefactorX;
            graphics.localScale = new Vector3(scalefactorX, 1, 1);
        }
        else if (upDownMoveValue > 0 && !isFacingUp && inBattle == false)
        {
            isFacingUp = true;
            scalefactorX = -scalefactorX;
            graphics.localScale = new Vector3(scalefactorX, 1, 1);
        }
    }

    protected virtual void AnimatorUpdate()
    {
        //Split due to the fact that it can = 0 if they are going downright of upleft causing it to think its speed is 0
        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z));
        anim.SetBool("GroundCheck", isGrounded);
    }

    protected virtual void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
            PlaySingleSound(jumpSFX);
        }
    }

    public void JumpedOnOpponentsWeakspot()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
        //PlaySingleSound(Bounce);
    }

    public virtual void DamageTaken(int damageTaken)
    {
        //Animator bool damage taken shall activate here

        GM.GeneralPlayerUI.EntityDamageTaken(this,damageTaken);
        //current health is deducted after so the same numbers exsist 
        //on the current UI and inside the entity
        currHealth -= damageTaken;

        if(currHealth <=0)
        {
            DeathStart();
        }

    }

    public virtual void DeathStart()
    {

    }

    #region Flipping Functions

    public virtual void Flip()
    {
        if(!isFacingLeft)
        {
            if(isFacingUp)
            {
                graphics.localEulerAngles = new Vector3(0, -0.1f + originalYOffset, 0);
            }
            else if(!isFacingUp)
            {
                graphics.localEulerAngles = new Vector3(0, 0.1f + originalYOffset, 0);
            }
        }
        else
        {
            if (isFacingUp)
            {
                graphics.localEulerAngles = new Vector3(0, -179.9f + originalYOffset, 0);
            }
            else
            {
                graphics.localEulerAngles = new Vector3(0, 179.9f + originalYOffset, 0);
            }
        }

        isFacingLeft = !isFacingLeft;

        Quaternion scaleFactor;

        if (isFacingLeft == true)
        {
            scaleFactor = Quaternion.Euler(0, 180f + originalYOffset, 0);
        }
        else
        {
            scaleFactor = Quaternion.Euler(0, 0 + originalYOffset, 0);
        }

        StopCoroutine("FlipAnimation");

        StartCoroutine(FlipAnimation(flipSpeed, scaleFactor, isFacingLeft,isFacingUp));
    }

    IEnumerator FlipAnimation(float speed, Quaternion targetRot, bool facingLeft,bool facingUp)
    {
        float dur = Quaternion.Angle(graphics.rotation, targetRot) / speed;

        Quaternion start = graphics.rotation;
        bool entityScalorFlipped = false;
        float t = 0f;
        while (t < dur)
        {
            if (isFacingLeft != facingLeft)
            {
                Debug.Log("Switch");
                yield break;
            }

            if(isFacingLeft && !facingUp)
            {
                if(graphics.localEulerAngles.y > 90 + originalYOffset && entityScalorFlipped == false)//DownLeft
                {
                    entityScalorFlipped = true;
                    scalefactorX = -scalefactorX;
                    graphics.localScale = new Vector3(scalefactorX, 1, 1);
                }
            }
            else if(!isFacingLeft && !facingUp)
            {
                if (graphics.localEulerAngles.y < 90 + originalYOffset && entityScalorFlipped == false)//DownRight
                {
                    entityScalorFlipped = true;
                    scalefactorX = -scalefactorX;
                    graphics.localScale = new Vector3(scalefactorX, 1, 1);
                }
            }
            else if(isFacingLeft && facingUp)
            {
                if (graphics.localEulerAngles.y < 270 + originalYOffset && entityScalorFlipped == false)//UpLeft
                {
                    entityScalorFlipped = true;
                    scalefactorX = -scalefactorX;
                    graphics.localScale = new Vector3(scalefactorX, 1, 1);
                }
            }
            else if(!isFacingLeft && facingUp)
            {
                if (graphics.localEulerAngles.y > 270 + originalYOffset && entityScalorFlipped == false)//Upright
                {
                    entityScalorFlipped = true;
                    scalefactorX = -scalefactorX;
                    graphics.localScale = new Vector3(scalefactorX, 1, 1);
                }
            }

            yield return null;
            t += Time.deltaTime;
            graphics.rotation = Quaternion.Slerp(start, targetRot, t / dur);
        }
        graphics.rotation = targetRot;
    }

    //public void FlipRight()
    //{
    //    if (!isFacingLeft)
    //    {
    //        if (isFacingUp)
    //        {
    //            graphics.localEulerAngles = new Vector3(0, -0.1f + originalYOffset, 0);
    //        }
    //        else if (!isFacingUp)
    //        {
    //            graphics.localEulerAngles = new Vector3(0, 0.1f + originalYOffset, 0);
    //        }
    //    }
    //    else
    //    {
    //        if (isFacingUp)
    //        {
    //            graphics.localEulerAngles = new Vector3(0, -179.9f + originalYOffset, 0);
    //        }
    //        else
    //        {
    //            graphics.localEulerAngles = new Vector3(0, 179.9f + originalYOffset, 0);
    //        }
    //    }

    //    isFacingLeft = false;

    //    Quaternion scaleFactor = Quaternion.Euler(0, 0 + originalYOffset, 0);

    //    StopCoroutine("FlipAnimation");

    //    StartCoroutine(FlipAnimation(flipSpeed, scaleFactor, isFacingLeft, isFacingUp));
    //}

    #endregion

    #region In Battle Functions

    public IEnumerator MoveToPresetBattlePosition(Vector3 position)
    {
        if(rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        while (Vector3.Distance(this.transform.position,position) > 0.1f)
        {
            leftRightMoveValue = position.x - this.transform.position.x;
            leftRightMoveValue = Mathf.Clamp(leftRightMoveValue, -1, 1);
            upDownMoveValue = position.z - this.transform.position.z;
            upDownMoveValue = Mathf.Clamp(upDownMoveValue, -1, 1);
            rb.velocity = new Vector3(leftRightMoveValue * speed,0, upDownMoveValue * speed);
            yield return null;
        }

        rb.velocity = Vector3.zero;
        transform.position = position;
        battleState = BattleState.WaitingToAttack;
        GM.CM.BattlePositionsSet();
    }

    /// <summary>
    /// The entity shall run to the given destination
    /// </summary>
    /// <param name="position">Where the player shall run to</param>
    /// <returns></returns>
    public IEnumerator GoToDestination(Vector3 position)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        while (Vector3.Distance(this.transform.position, position) > 0.1f)
        {
            leftRightMoveValue = position.x - this.transform.position.x;
            leftRightMoveValue = Mathf.Clamp(leftRightMoveValue, -1, 1);
            upDownMoveValue = position.z - this.transform.position.z;
            upDownMoveValue = Mathf.Clamp(upDownMoveValue, -1, 1);
            rb.velocity = new Vector3(leftRightMoveValue * speed,0, upDownMoveValue * speed);
            yield return null;
        }

        rb.velocity = Vector3.zero;
        transform.position = position;

        if(battleState == BattleState.CurrentTurn)
        {
            battleState = BattleState.CurrentlyAttacking;

            if(myattackStart != null)
            {
                myattackStart.Invoke();
            }
        }

        if(battleState == BattleState.FinishedTurn)
        {
            GM.playerSelection.AttackerReturnedAndFinished();
            myattackStart = null;
            myattackFinish = null;
        }

    }

    #endregion

    #region Audio

    public void PlaySingleSound(AudioClip clip, float volume = 1.0f)
    {
        aSource.clip = clip;
        aSource.volume = volume;
        aSource.Play();
    }

    #endregion

    protected virtual void OnDrawGizmos()
    {
        //Debug.Log(transform.root.name);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
    }
}
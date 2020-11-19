using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState { Idle, Patrol ,PlayerFound, Confused}

public class EnemyController : Entity {

    [Header("Enemy Controller Variables")]
    public Bestiary selfReferenceBestiary;
    //Eye Sight Variables
    public float eyeSightRadius;
    [Range(0,360)]
    public float eyeSightAngle;
    Transform eyeSightForward;

    //Reference to player
    GameObject player;
    float distanceBetweenPlayer;//distance between player & enemy in the outer world
    Transform weakspot;

    //When Player Is Found
    public bool _playerSpotted;
    RaycastHit hitPlayer;
    GameObject exclamationMark;
    bool _chasePlayer;
    [Tooltip("Delay between when the player is spotted to when the enemy starts to head after the player")]
    public float chaseDelay;
    
    //State
    EnemyState currentState;

    //Patrol State
    Vector3 originalStartingPosition;//Starting Original Spot and radius that they will walk around in.
    Vector3 patrolPosition = Vector3.zero;//new spot where the player shall patrol to.
    public float patrolRadius;
    public float patrolSpeed = 2.5f;

    //Confused State, when in confused state or lost them
    bool _isconfused;
    GameObject questionMark;

    //Battle
    [Header("Enemy Battle")]
    [Tooltip("Allies that shall be deployed in battle along with this one, the unit shall always have a refence to itself for the front position")]
    public List<Bestiary> enemyTeam;
    public bool isGroundTarget;
    public bool isFlyingTarget;
    [System.NonSerialized]
    public Transform healthBarTransform;
    [Tooltip("Will this unit move to a forward position if its available?")]
    public bool willMoveForwardIfFree;
    public int experienceGiven;

    //universal timer within the Enemy
    float time;

    //Constant Variables
    const float IDLE_STATE_TIMER = 2.5f;
    const float MAX_TIME_CONFUSTED = 3;

    #region Getters/Setters

    bool isconfused
    {
        get { return _isconfused; }
        set
        {
            _isconfused = value;

            if(_isconfused)
            {
                questionMark.SetActive(true);
            }
            else
            {
                questionMark.SetActive(false);
            }
        }
    }

    bool playerSpotted
    {
        get { return _playerSpotted; }
        set
        {
            _playerSpotted = value;
            if(_playerSpotted == true && chasePlayer==false)
            {
                exclamationMark.SetActive(true);
            }
            else
            {
                exclamationMark.SetActive(false);
            }
        }
    }

    bool chasePlayer
    {
        get { return _chasePlayer; }
        set
        {
            _chasePlayer = value;
            if (_chasePlayer == true)
            {
                exclamationMark.SetActive(false);
            }
        }
    }

    #endregion

    public override void Awake()
    {
        EnemySizeChecker();

        GetStatsFromBestiary();

        #region Starting Setters

        base.Awake();

        player = GameObject.Find("Player");

        eyeSightForward = transform.Find("EyeSight");
        exclamationMark = transform.Find("ExclamationMark").gameObject;
        questionMark = transform.Find("QuestionMark").gameObject;
        healthBarTransform = transform.Find("HealthDisplay");
        weakspot = transform.Find("Weakspot");
        originalStartingPosition = transform.position;

        currentState = EnemyState.Idle;

        UpdateEyeSight();

        #endregion

        #region Precautions Check

        if (inBattle == false)
        {
            if(patrolSpeed <= 0)
            {
                Debug.Log("Patrol Speed not set, setting to 3");
                patrolSpeed = 3;              
            }

            if(chaseDelay <=0)
            {
                Debug.Log("Chase Delay is not set, setting to 1");
                chaseDelay = 1;                
            }

            if(patrolRadius <= 0)
            {
                Debug.Log("original Patrol Radius not set, setting to 10");
                patrolRadius = 10;
            }
        }
        else  //inBattle == false
        {
            if(experienceGiven <=0)
            {
                Debug.LogWarning(name + "'s experienceGiven is not set or less then 0");
            }
        }

        //Battle
        if(enemyTeam.Count <=0)
        {
            Debug.Log("This prefab does not have its team set up for combat",gameObject);
        }

        #endregion

    }

    void Update()
    {
        base.CalledEveryUpdate();

        if (inBattle == false)
        {


            distanceBetweenPlayer = Vector3.Distance(this.transform.position, player.transform.position);

            if (distanceBetweenPlayer < eyeSightRadius)
            {
                CanSeePlayer();
            }
            else
            {
                playerSpotted = false;
            }


            switch (currentState)
            {
                case EnemyState.Idle:

                    CurrentStateIdle();

                    break;
                case EnemyState.Patrol:

                    CurrentStatePatrol();

                    break;
                case EnemyState.PlayerFound:

                    CurrentStatePlayerFound();

                    break;
                case EnemyState.Confused:

                    CurrentStateConfused();

                    break;
            }
        }

        base.AnimatorUpdate();

    }

    #region Entity Override

    protected override bool isFacingLeft
    {
        get
        {
            return base.isFacingLeft;
        }

        set
        {
            base.isFacingLeft = value;
            UpdateEyeSight();
        }
    }

    protected override bool isFacingUp
    {
        get
        {
            return base.isFacingUp;
        }

        set
        {
            base.isFacingUp = value;
            UpdateEyeSight();
        }
    }

    #endregion

    #region Called On Awake

    public void EnemySizeChecker()
    {
        if (enemyTeam.Count > 4)
        {
            List<Bestiary> x = enemyTeam;
            enemyTeam = new List<Bestiary>();

            for (int i = 0; i < 4; i++)
            {
                enemyTeam.Add(x[i]);
            }
        }

        enemyTeam.Insert(0, selfReferenceBestiary);
    }

    public void GetStatsFromBestiary()
    {
        base.setMaxHealth = selfReferenceBestiary.hitPoints;
        base.currHealth = base.getMaxHealth;
        base.attack = selfReferenceBestiary.attack;
        base.defense = selfReferenceBestiary.defense;
    }

    #endregion

    #region Enemy State Machine

    void CurrentStateIdle()
    {
        time += Time.deltaTime;
        if(time>IDLE_STATE_TIMER)
        {
            time = 0;
            currentState = EnemyState.Patrol;
        }

        if(playerSpotted)
        {
            time = 0;
            currentState = EnemyState.PlayerFound;
        }
    }

    void CurrentStatePatrol()
    {
        if (patrolPosition == Vector3.zero)
        {
            patrolPosition = GetNewPatrolLocation();
        }

        time += Time.deltaTime;
        Debug.DrawLine(transform.position, patrolPosition,Color.red);

        leftRightMoveValue = patrolPosition.x - transform.position.x;
        leftRightMoveValue = Mathf.Clamp(leftRightMoveValue, -1, 1);
        leftRightMoveValue *= time;
        leftRightMoveValue = Mathf.Clamp(leftRightMoveValue, -1, 1);

        upDownMoveValue = patrolPosition.z - transform.position.z;
        upDownMoveValue = Mathf.Clamp(upDownMoveValue, -1, 1);
        upDownMoveValue *= time;
        upDownMoveValue = Mathf.Clamp(upDownMoveValue, -1, 1);

        rb.velocity = new Vector3(leftRightMoveValue * patrolSpeed, rb.velocity.y, upDownMoveValue * patrolSpeed);
        //Debug.Log(rb.velocity + "Enemy Velocity");

        if (Vector3.Distance(patrolPosition, transform.position) < 1)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            time = 0;
            currentState = EnemyState.Idle;
            patrolPosition = Vector3.zero;
        }

        if (playerSpotted)
        {
            time = 0;
            currentState = EnemyState.PlayerFound;
        }
    }

    void CurrentStatePlayerFound()
    {
        if (playerSpotted)
        {
            time += Time.deltaTime;

            if(time > chaseDelay && chasePlayer == false)
            {
                chasePlayer = true;
                time = 0;
            }

            if(chasePlayer)
            {
                leftRightMoveValue = player.transform.position.x - transform.position.x;
                Mathf.Clamp(leftRightMoveValue, -1, 1);
                leftRightMoveValue *= time;
                Mathf.Clamp(leftRightMoveValue, -1, 1);

                upDownMoveValue = player.transform.position.z - transform.position.z;
                Mathf.Clamp(upDownMoveValue, -1, 1);
                upDownMoveValue *= time;
                Mathf.Clamp(upDownMoveValue, -1, 1);

                rb.velocity = new Vector3(leftRightMoveValue * speed, rb.velocity.y, upDownMoveValue * speed);
            }
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            time = 0;
            chasePlayer = false;
            currentState = EnemyState.Confused;
        }
    }

    void CurrentStateConfused()
    {
        if(isconfused == false)
        {
            isconfused = true;
            time = 0;
        }
        else
        {
            time += Time.deltaTime;
            if(time > MAX_TIME_CONFUSTED)
            {
                isconfused = false;
                currentState = EnemyState.Idle;
            }
        }
    }

    #endregion

    #region State Helpers

    Vector3 GetNewPatrolLocation()
    {
        Vector3 startingPosition = Vector3.zero;

        while (startingPosition == Vector3.zero)
        {
            startingPosition = originalStartingPosition + (Random.insideUnitSphere * patrolRadius);
            startingPosition.y = originalStartingPosition.y;

            //if (Physics.SphereCast(currentLocation, 1, 0, obstacleMask)) ;
            //do spherecast to see if the enemy can go to where its trying to go to and no obstacles in the way
        }
        return startingPosition;
    }

    #endregion

    #region EyeSight

    void CanSeePlayer()
    {
        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
        if(Vector3.Angle(eyeSightForward.forward, dirToPlayer) < eyeSightAngle/2)
        {
            float distanceBetweenTarget = Vector3.Distance(transform.position, player.transform.position);
            if(!Physics.Raycast(transform.position ,dirToPlayer ,out hitPlayer ,distanceBetweenTarget ,obstacleMask))
            {
                playerSpotted = true;
                currentState = EnemyState.PlayerFound;
                if(isconfused == true)
                {
                    isconfused = false;
                    chasePlayer = true;
                    time = 1;
                }

                Debug.DrawLine(transform.position, player.transform.position,Color.green);
            }
            else
            {
                playerSpotted = false;
            }
        }
        else
        {
            playerSpotted = false;
        }
    }

    public Vector3 CurrentEyeSightAngle(float angleInDegrees)//used by the editor to help show units sight
    {
        angleInDegrees = CorrectingEyeSightAngle(angleInDegrees);

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    void UpdateEyeSight()
    {
        eyeSightForward.eulerAngles = new Vector3(0, CorrectingEyeSightAngle(), 0);
    }

    float CorrectingEyeSightAngle(float angle = 0)
    {
        if (isFacingLeft)
        {
            angle -= 90;

            if (isFacingUp)
            {
                angle += eyeSightAngle / 4;
            }
            else
            {
                angle -= eyeSightAngle / 4;
            }
        }
        else
        {
            angle += 90;

            if (isFacingUp)
            {
                angle -= eyeSightAngle / 4;
            }
            else
            {
                angle += eyeSightAngle / 4;
            }
        }

        return angle;
    }

    #endregion

    #region Hit By Player/Partner

    private void OnTriggerEnter(Collider c)
    {
        if (c.transform.root.GetComponent<Entity>())
        {
            c.transform.root.GetComponent<Entity>().JumpedOnOpponentsWeakspot();
        }
    }

    #endregion

    #region Battle

    //List<GameObject> CorrectingBattleStartUpPrefabList(List<GameObject> originalList)
    //{
    //    List<GameObject> newList = new List<GameObject>();
    //    GameObject temp;
    //    for (int i = 0; i < originalList.Count; i++)
    //    {
    //        temp = (GameObject)Instantiate(myPrefab)

    //        if (i >=5)
    //        {
    //            Debug.Log("This enemy has a list greater then 5");
    //            break;
    //        }
    //    }

    //    return newList;
    //}

    protected void InBattleAttack()
    {

    }

    protected void OuterWorldAttackTowardsPoint(Vector3 targetLocation)
    {

    }

    #endregion

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(originalStartingPosition, patrolRadius);
    }

}
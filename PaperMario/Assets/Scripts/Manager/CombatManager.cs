using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FirstStrike { NA,Player,Enemy,PowerStruggle}

public struct BattlePositionAndAvailability
{
    Vector3 _position;
    Entity _currentOccupier;

    public Vector3 position
    {
        get { return _position; }
        set
        {
            _position = value;
        }
    }

    /// <summary>
    /// checks to see if there is a current unit occupying the position available
    /// </summary>
    public bool isOccupied
    {
        get
        {
            if (_currentOccupier == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public Entity currentOccupier
    {
        get { return _currentOccupier; }
        set
        {
            _currentOccupier = value;
        }
    }

    public BattlePositionAndAvailability(Vector3 battlePosition)
    {
        _position = battlePosition;
        _currentOccupier = null;
    }
}

public class CombatManager : MonoBehaviour {

    GameManager GM;
    //[System.NonSerialized]
    public GameObject player;
    //[System.NonSerialized]
    public GameObject partner;

    GameObject currentGameobjectAttacking;//For the collider aspect so it doesnt push the others around when making contact

    public List<GameObject> enemies;
    List<Entity> entities = new List<Entity>();

    Vector3 playerSpawn;
    Vector3 enemySpawn;
    int playersCurrentPosition;

    //Positions
    Vector3[] playerPositions;
    BattlePositionAndAvailability[] enemyPositions;
    Vector3[] switchPositions;
    Vector3 multiPurposePosition;

    //Timer for Attacks
    public delegate void ChargeAttackFinished();
    public ChargeAttackFinished chargeAttackFinished;
    float timer;
    float chargeTime;
    bool timerIsActive;

    //Charging Attack Times;
    float beamSlashChargeTime;

    GameObject[] targets;

    public FirstStrike firstStrike;

    const float WAIT_SECONDS_BETWEEN_TIME_SPAWN = 0.5f;

    // Use this for initialization
    void Start () {
        playersCurrentPosition = 0;
        GM = GameManager.instance;
        GM.CM = this;

        SetStartingPositions();
        SetChargingVariableTimes();

        StartCoroutine(SpawnInPlayerUnits());
        StartCoroutine(SpawnInEnemyUnits());

        timerIsActive = false;

    }

    void Update()//Testing
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            BattleHasEnded();
        }

        if(Input.GetKeyDown(KeyCode.Q))
        {
            //player.GetComponent<PlayerController>().isFacingLeft = !player.GetComponent<PlayerController>().isFacingLeft;
            player.GetComponent<PlayerController>().Flip();
        }

        if(timerIsActive)
        {
            if(timer >= chargeTime)
            {
                chargeAttackFinished.Invoke();
            }
            timer += Time.deltaTime;
        }
    }

    #region Starting Combat Setters

    void SetStartingPositions()
    {
        playerSpawn = GameObject.Find("Environment/Spawns/LeftSpawner").transform.position;
        enemySpawn = GameObject.Find("Environment/Spawns/RightSpawner").transform.position;

        playerPositions = new Vector3[]
        {
            GameObject.Find("Environment/StagePositions/LeftPosition1").transform.position,
            GameObject.Find("Environment/StagePositions/LeftPosition2").transform.position
        };

        enemyPositions = new BattlePositionAndAvailability[]
        {
            new BattlePositionAndAvailability(GameObject.Find("Environment/StagePositions/RightPosition1").transform.position),
            new BattlePositionAndAvailability(GameObject.Find("Environment/StagePositions/RightPosition2").transform.position),
            new BattlePositionAndAvailability(GameObject.Find("Environment/StagePositions/RightPosition3").transform.position),
            new BattlePositionAndAvailability(GameObject.Find("Environment/StagePositions/RightPosition4").transform.position),
            new BattlePositionAndAvailability(GameObject.Find("Environment/StagePositions/RightPosition5").transform.position)
        };

        switchPositions = new Vector3 []
        {
            GameObject.Find("Environment/StagePositions/SwitchUpPosition").transform.position,
            GameObject.Find("Environment/StagePositions/SwitchDownPosition").transform.position
        };

        multiPurposePosition = GameObject.Find("Environment/StagePositions/MultiPurposePosition").transform.position;
    }

    IEnumerator SpawnInPlayerUnits()
    {
        //Player
        player = Instantiate(GM.player, playerSpawn, Quaternion.identity);
        PlayerController playerController = player.GetComponent<PlayerController>();
        StartCoroutine(playerController.MoveToPresetBattlePosition(playerPositions[0]));
        entities.Add(playerController);

        if(firstStrike == FirstStrike.Player)
        {
            playerController.battleState = BattleState.WaitingToAttack;
        }
        else
        {
            playerController.battleState = BattleState.MovingToStartingPosition;
        }

        yield return new WaitForSeconds(WAIT_SECONDS_BETWEEN_TIME_SPAWN);

        //Partner
        partner = Instantiate(GM.partners[0], playerSpawn, Quaternion.identity);//will be filled with a enum
        PartnerController partnerController = partner.GetComponent<PartnerController>();
        entities.Add(partnerController);
        partnerController.battleState = BattleState.MovingToStartingPosition;
        StartCoroutine(partnerController.MoveToPresetBattlePosition(playerPositions[1]));
    }

    IEnumerator SpawnInEnemyUnits()
    {
        EnemyController enemyController;

        //for loop to spawn the enemies that shall be loaded from the enemy list
        enemies = BeastsForBattle();

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i] = Instantiate(enemies[i], enemySpawn, Quaternion.identity);
            enemyController = enemies[i].GetComponent<EnemyController>();
            StartCoroutine(enemyController.MoveToPresetBattlePosition(enemyPositions[i].position));
            enemyPositions[i].currentOccupier = enemyController;
            entities.Add(enemyController);
            enemyController.battleState = BattleState.MovingToStartingPosition;

            if(enemyController.selfReferenceBestiary.examined == true)
            {
                enemies[i].AddComponent<EnemyHealthDisplay>();
            }

            if(i == 0 && firstStrike == FirstStrike.Enemy)
            {
                enemyController.battleState = BattleState.WaitingToAttack;
            }

            yield return new WaitForSeconds(WAIT_SECONDS_BETWEEN_TIME_SPAWN);
        }
    }

    public void BattlePositionsSet()
    {
        foreach (Entity child in entities)
        {
            if(child.battleState != BattleState.WaitingToAttack)
            {
                return;
            }
        }

        for (int i = 0; i < entities.Count; i++)
        {
            entities[i].inBattle = true;
        }

        SetAllAvailableSkills();
        GM.playerSelection.CalledOnceAllUnitsInPosition();
        player.GetComponent<PlayerController>().battleState = BattleState.CurrentTurn;
    }

    #endregion

    #region Return Methods

    /// <summary>
    /// this returns all enemy controllers from all enemies within the current battle
    /// </summary>
    /// <returns></returns>
    public EnemyController[] ReturnAllEnemies()
    {
        EnemyController[] currentList = new EnemyController[enemies.Count];

        for (int i = 0; i < enemies.Count; i++)
        {
            currentList[i] = enemies[i].GetComponent<EnemyController>();
        }

        return currentList;
    }

    /// <summary>
    /// Converts The Bestiary list on the enemy controller into a GameObject list to spawn the units in
    /// </summary>
    /// <returns></returns>
    List<GameObject> BeastsForBattle()
    {
        List<GameObject> x = new List<GameObject>();

        foreach (Bestiary beast in GM.enemiesForBattle)
        {
            x.Add(beast.prefab);
        }

        return x;
    }

    #endregion

    void BattleHasEnded()
    {
        Destroy(player);
        Destroy(partner);
        for (int i = 0; i < GM.enemiesForBattle.Count; i++)
        {
            Destroy(enemies[i]);
        }
        GameManager.instance.BattleSceneExited();
    }

    #region Organising Available Selections

    void SetAllAvailableSkills()
    {
        foreach (Skills skill in GM.allSkills)
        {
            if(skill.abilityActive == true)
            {

                switch (skill.skillType)
                {
                    case TypeOfSkill.playerSword:
                        GM.playerSelection.AvailableSwordAttacks.Add(skill);
                        break;
                    case TypeOfSkill.playerJump:
                        GM.playerSelection.AvailableJumpAttacks.Add(skill);
                        break;
                    case TypeOfSkill.playerSpecial:
                        //This version of available shit
                        break;
                    case TypeOfSkill.partnerAttack:
                        PartnerAttack currentskill = (PartnerAttack)skill;
                        if(currentskill.whichPartner == partner.GetComponent<PartnerController>().whichPartner)
                        {
                            GM.playerSelection.AvailablePartnerAttacks.Add(skill);
                        }
                        break;
                    case TypeOfSkill.item:
                        //This version of available shit
                        break;
                    case TypeOfSkill.misc:
                        //This version of available shit
                        break;
                }

            }
        }

        //Sorts the list depending on priority
        GM.playerSelection.AvailableSwordAttacks.Sort((a, b) => a.priorityListing.CompareTo(b.priorityListing));
        GM.playerSelection.AvailableJumpAttacks.Sort((a, b) => a.priorityListing.CompareTo(b.priorityListing));

        GM.playerSelection.PresetInstantiateSelectionMenuSize();
    }

    #endregion

    #region Skills

    public void SkillUsed(Skills currentSkill,List<GameObject> targets = null)
    {
        switch (currentSkill.skillType)
        {
            case TypeOfSkill.playerSword:

                if(currentSkill.skillName == "Slash")
                {
                    SlashAttack(targets[0]);
                    break;
                }
                else if(currentSkill.skillName == "Beam Slash")
                {
                    BeamSlashAttack(targets);
                    break;
                }
                break;
            case TypeOfSkill.playerJump:
                break;
            case TypeOfSkill.playerSpecial:
                break;
            case TypeOfSkill.partnerAttack:

                if(currentSkill.skillName == "Rock Smash")
                {
                    RockSmashAttack(targets[0]);
                    break;
                }
                break;
            case TypeOfSkill.item:
                break;
            case TypeOfSkill.misc:
                break;
            default:
                break;
        }
    }

    //***** Player Skills *****//

    //Sword Skills

    void SlashAttack(GameObject target)
    {
        //Turn on UI to help aid the player in what to do
        Vector3 playersDestination = target.transform.position + new Vector3(-1.25f, 0, 0);

        TurnOffColliderTemporarily(player);

        PlayerController playerController = player.GetComponent<PlayerController>();
        StartCoroutine(playerController.GoToDestination(playersDestination));

        playerController.myattackStart += () =>
        {
            playerController.anim.SetTrigger("SlashAttack");
        };

        playerController.myattackFinish += () =>
        {
            StartCoroutine(playerController.GoToDestination(playerPositions[0]));//To be set to the players dynamic location
        };
    }

    void BeamSlashAttack(List<GameObject> targets)
    {
        Vector3 playersDestination = multiPurposePosition;

        PlayerController playerController = player.GetComponent<PlayerController>();
        StartCoroutine(playerController.GoToDestination(playersDestination));

        playerController.myattackStart += () =>
        {
            playerController.anim.SetTrigger("BeamSlashAttack");
            ChargeTime(3, playerController);
        };

        playerController.myattackFinish += () =>
        {
            StartCoroutine(playerController.GoToDestination(playerPositions[0]));//To be set to the players dynamic location
        };

        Debug.Log("Beam Slash Attack Called");
    }

    //Jump Skills

    //void PlayerReturnToPosition()
    //{
    //    PlayerController playerController = player.GetComponent<PlayerController>();
    //    StartCoroutine(playerController.GoToDestination(playerPositions[playersCurrentPosition]));
    //    playerController.battleState = BattleState.FinishedTurn;
    //}


    //***** Partner Skills *****//

    void RockSmashAttack(GameObject target)
    {
        //Turn on UI to help aid the player in what to do
        Vector3 partnersDestination = target.transform.position + new Vector3(-1.25f, 0, 0);

        TurnOffColliderTemporarily(partner);

        PartnerController partnerController = partner.GetComponent<PartnerController>();
        StartCoroutine(partnerController.GoToDestination(partnersDestination));

        partnerController.myattackStart += () =>
        {
            partnerController.anim.SetTrigger("RockSmashAttack");
        };

        partnerController.myattackFinish += () =>
        {
            StartCoroutine(partnerController.GoToDestination(playerPositions[1]));//To be set to the players dynamic location
        };
    }

    #endregion

    #region Charging/Timer Functions

    void SetChargingVariableTimes()
    {
        beamSlashChargeTime = GM.allSkills.Find(x => x.skillName == "Beam Slash").chargeTime;
    }

    void ChargeTime(float howManySeconds, Entity entity)
    {
        timerIsActive = true;
        timer = 0;
        chargeTime = howManySeconds;

        Animator anim = entity.anim;

        chargeAttackFinished = null;

        chargeAttackFinished += () =>
        {
            anim.SetTrigger("TimerFinished");
            timerIsActive = false;
        };
    }

    #endregion

    #region EnemyCombatManager

    public void StartEnemyTurn()
    {
        Debug.Log("Starting Enemies Turn");

        MoveForwardPositionIfAvailable();

        EnemyController currentAttackingEnemy;

        for (int i = 0; i < enemies.Count; i++)
        {
            if(enemies[i].GetComponent<EnemyController>().battleState != BattleState.Dead || enemies[i].GetComponent<EnemyController>().battleState != BattleState.FinishedTurn)
            {
                currentAttackingEnemy = enemies[i].GetComponent<EnemyController>();
                break;
            }

            //Skills currentSkillBeingUsed = //Find this enemies list of attacks and choose one;
        }
    }

    GameObject EnemyTargetEvaluate(bool randomTarget)
    {
        GameObject target = null;

        if(randomTarget == true)
        {
            return RandomTarget();
        }
        else
        {
            float playersHealthPercentage = player.GetComponent<Entity>().currHealth / player.GetComponent<PlayerController>().playersMaxHealth;
            float partnersHealthPercentage = partner.GetComponent<Entity>().currHealth / partner.GetComponent<PartnerController>().partnersMaxHealth;

            if(playersHealthPercentage <= 0.2)
            {
                return player;
            }
            else if(partnersHealthPercentage <= 0.2)
            {
                return partner;
            }
            else
            {
                RandomTarget();
            }
        }

        return target;
    }

    GameObject RandomTarget()
    {
        int rnd = Random.Range(0, 2);
        if (rnd == 0)
        {
            return player;
        }
        else
        {
            return partner;
        }
    }

    /// <summary>
    /// Checks if there is a position available if the current enemy can move forward.
    /// </summary>
    public void MoveForwardPositionIfAvailable()
    {

        //Checks every enemy position available
        for (int i = 0; i < enemyPositions.Length; i++)
        {
            //If this area is not occupied then it shall start checking every other unit afterwards that can move
            if(enemyPositions[i].isOccupied == false)
            {
                for (int j = i; j < enemyPositions.Length; j++)
                {
                    if(enemyPositions[j].currentOccupier != null)
                    {
                        EnemyController currentQuestionedEnemy = (EnemyController)enemyPositions[j].currentOccupier;

                        if (currentQuestionedEnemy.willMoveForwardIfFree == true)
                        {

                            currentQuestionedEnemy.MoveToPresetBattlePosition(enemyPositions[i].position);
                            enemyPositions[j].currentOccupier = null;
                            enemyPositions[i].currentOccupier = currentQuestionedEnemy;
                        }
                    }
                }
            }
        }

        //Reorganize Enemy List
        enemies = new List<GameObject>();

        for (int i = 0; i < enemyPositions.Length; i++)
        {
            if(enemyPositions[i].isOccupied == true)
            {
                enemies.Add(enemyPositions[i].currentOccupier.gameObject);
            }
        }
    }

    #endregion

    #region After Enitity Attacks

    public bool areAllEnemiesDead()
    {
        if(enemies.Count == 0)
        {
            return true;
        }

        //Check to see if there is a more foward position available and move to that if they will do so.

        return false;
    }

    public void PlayerWonBattle()
    {

    }

    #endregion

    #region On/Off Colliders When Attacking

    void TurnOffColliderTemporarily(GameObject gameObject)
    {
        Collider [] cols;
        cols = gameObject.GetComponentsInChildren<Collider>();

        gameObject.GetComponent<Rigidbody>().useGravity = false;

        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].enabled = false;
        }

        currentGameobjectAttacking = gameObject;
    }

    public void TurnOnColliderTemporarily()
    {
        Collider[] cols;
        cols = currentGameobjectAttacking.GetComponentsInChildren<Collider>();

        currentGameobjectAttacking.GetComponent<Rigidbody>().useGravity = true;

        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].enabled = true;
        }

        currentGameobjectAttacking = null;
    }

    #endregion

    #region Switch Positions

    public IEnumerator SwitchPositions()
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        PartnerController partnerController = partner.GetComponent<PartnerController>();

        while (Vector3.Distance(player.transform.position, switchPositions[0]) > 0.1f)
        {
            StartCoroutine(playerController.GoToDestination(switchPositions[0]));
            StartCoroutine(partnerController.GoToDestination(switchPositions[1]));
            yield return null;
        }

        while (Vector3.Distance(player.transform.position, playerPositions[1]) > 0.1f)
        {
            StartCoroutine(playerController.GoToDestination(playerPositions[1]));
            StartCoroutine(partnerController.GoToDestination(playerPositions[2]));
            yield return null;
        }
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

struct SelectionBubbles {
    public GameObject gameObject;
    public Image imageUI;
    public Text textUI;
}

struct SelectionMenu{
    public GameObject gameObject;
    public Image imageUI;
    public Text skillNameUI;
    public Text skillPpCost;
}

public enum PlayerSelectionState { FirstSelectionMenu, SecondSelectionMenu, SelectingEnemyToAttack, SelectingTargetToHeal, AttackAnimating}

public class PlayerSelection : MonoBehaviour {

    #region Variables Declared

    bool ignoreInput;
    bool _isAnimating;
    float playerInputHorizontal;
    float playerInputVertical;
    float angleToRotate;
    float leftoverAngleToRotate;
    int originalAngleStartedAt;
    bool rotatingBubbles;
    int playersCurrentSelection = 0;
    int maxAmountOfDecisions;

    //References
    GameManager GM;
    CombatManager CM;

    //Menu Shared Variables
    PlayerSelectionState playerSelectionState;
    float cursorTimer;
    bool _playerCurrentlySelecting;//If true player is picking the decision and false is the partner selection
    int animationBubbleSpeed;//the speed shall change depending on whom the current person selecting is
    Skills currentSkillBeingUsed;
    bool _playerCanCurrenltySwitch;

    //First Menu Bubble Choices
    GameObject playerSelectionParent;
    public GameObject selectionBubblePrefab;
    float radiusOfSelectionBubbles = 2f;
    SelectionBubbles[] selectionBubbles;

    //Second Menu Lists of Selections
    GameObject selectionMenu;
    Transform selectionMenuChildParent;
    public GameObject selectionMenuOptionsPrefab;
    GameObject secondSelectionMenu_Selector;
    Vector3 secondSelectionMenu_SelectorOriginalPosition;
    SelectionMenu[] selectionMenuChoices;
    public List<Skills> AvailableSwordAttacks;//This is set per selection for organization
    public List<Skills> AvailableJumpAttacks;
    public List<Skills> AvailableItemSelections;
    public List<Skills> AvailableSpecialSelections;
    public List<Skills> AvailableMiscSelections;
    public List<Skills> AvailablePartnerAttacks;

    //Target Selecting
    GameObject[] targetSelector;
    public GameObject targetSelectorPrefab;
    int currentEnemySelected = 0;
    List<EnemyController> availableEnemiesToAttack;
    public List<GameObject> enemyTargets;

    const int ANIMATION_BUBBLE_SPEED_PLAYER = 300;
    const int ANIMATION_BUBBLE_SPEED_PARTNER = 500;
    const float CONTROLLER_SENSITIVITY = 0.25f;//Left/Right Sensitivity
    const float OFFSET_FOR_STARTING_POSITION_ANGLE = -45;//since right is the starting angle the offset is X degrees to correct this
    const float BUBBLE_FLOATING_HEIGHT = 2f;
    const float SELECTION_MENU_OPTIONS_STARTING_HEIGHT = 0.6f;
    const float SELECTION_MENU_OPTIONS_HEIGHT_DIFFERENCE = 0.62f;//Height difference between each option to pick
    const float SELECTION_MENU_SELECTOR_POINTER_HEIGHT_DIFFERENCE = -0.32f;//Height Difference between what the pointer is pointing at due to center of the image
    const float TIME_BETWEEN_EACH_SELECTION = 0.35F;//prevents the cursor from spazzing
    const float TARGET_SELECTOR_HEIGHT_ADDITION = 0.5f;//Height added ontop of the target selector between the top point of the mesh and bottom of the pointer

#endregion

    #region Getters/Setters/Return

    /// <summary>
    /// prevent the player from spamming input while something is animating
    /// <para>Cursor timer is always reset to 0</para>  
    /// </summary>
    bool isAnimating
    {
        get { return _isAnimating; }
        set
        {
            _isAnimating = value;
            if(value == false)
            {
                cursorTimer = 0;
            }
        }
    }

    Skills GetSkillFromList(List<Skills> currentList, int posOfSkill = 0)
    {
        return currentList[posOfSkill];
    }

    List<Skills> ReturnList()
    {
        if (playerCurrentlySelecting == true)
        {
            switch (CM.player.GetComponent<PlayerController>().playerBattleState)
            {
                case PlayerBattleSelections.Sword:
                    return AvailableSwordAttacks;

                case PlayerBattleSelections.Jump:
                    return AvailableJumpAttacks;

                case PlayerBattleSelections.Item:
                    return null;

                case PlayerBattleSelections.Special:
                    return null;
                case PlayerBattleSelections.Misc:
                    return null;
            }
        }
        else
        {
            switch (CM.partner.GetComponent<PartnerController>().partnerBattleState)
            {
                case PartnerBattleSelections.Attack:
                    return AvailablePartnerAttacks;
                case PartnerBattleSelections.Item:
                    break;
                case PartnerBattleSelections.Misc:
                    break;
                default:
                    break;
            }
        }
        return null;
    }

    bool playerCanCurrentlySwitch
    {
        get { return _playerCanCurrenltySwitch; }

        set
        {
            _playerCanCurrenltySwitch = value;
            //turn the UI on or off to indicate positions can be switched
        }
    }

    bool playerCurrentlySelecting
    {
        get { return _playerCurrentlySelecting; }
        
        set
        {
            _playerCurrentlySelecting = value;
            if(value ==true)
            {
                animationBubbleSpeed = ANIMATION_BUBBLE_SPEED_PLAYER;
            }
            else
            {
                animationBubbleSpeed = ANIMATION_BUBBLE_SPEED_PARTNER;
            }
            //Debug.Log(animationBubbleSpeed + "Animation Bubble Speed" + value);
        }
    }

    #endregion

    #region Called From Other Scripts

    //Called from CombatManager
    public void CalledOnceAllUnitsInPosition()//Once all scripts are initialized
    {
        CM = GM.CM;
        StartPlayersTurn();
    }

    #endregion

    // Use this for initialization
    void Start () {

        GM = GameManager.instance;
        GM.playerSelection = this;

        isAnimating = false;
        ignoreInput = true;
        rotatingBubbles = false;
        playerCurrentlySelecting = true;

        playerSelectionParent = this.gameObject;

        //First Menu
        selectionMenu = GameObject.Find("SelectionMenu");
        selectionMenuChildParent = selectionMenu.transform.Find("Canvas");

        //Second Menu
        secondSelectionMenu_Selector = selectionMenu.transform.Find("Canvas/SelectorPointer").gameObject;
        secondSelectionMenu_SelectorOriginalPosition = secondSelectionMenu_Selector.transform.position;
        SecondMenuClosed();

        //Third Menu
        PresetInstantiateTargetSelectionSize();
        DeactivateTargetSelector();

        PresetInstantiateBubbleMenuSize();
        SetBubbleLocations(selectionBubbles.Length, selectionBubbles);
        SelectionBubblesActive(false);
    }

    // Update is called once per frame
    void Update () {

        if (ignoreInput == false)
        {
            switch (playerSelectionState)
            {
                case PlayerSelectionState.FirstSelectionMenu:

                    if (isAnimating == false)
                    {
                        playerInputHorizontal = Input.GetAxis("Horizontal");

                        if (playerInputHorizontal > CONTROLLER_SENSITIVITY && playerSelectionParent.activeInHierarchy)//Right
                        {
                            angleToRotate = RotateSelectionBubbles(true);
                        }
                        else if (playerInputHorizontal < -CONTROLLER_SENSITIVITY && playerSelectionParent.activeInHierarchy)//Left
                        {
                            angleToRotate = RotateSelectionBubbles(false);
                        }
                    }
                    else
                    {
                        if (rotatingBubbles == true)
                        {
                            AnimatingRotationOfSelectionBubbles(animationBubbleSpeed);
                        }
                    }

                    if (Input.GetButtonDown("Interact"))
                    {
                        SetBubblesToSetLocation();

                        //PlayerBattleSelections { Sword = 0 ,Jump,Item,Special,Misc}
                        if (playersCurrentSelection == 0 && playerCurrentlySelecting)//Sword
                        {
                            LoadSkillOptionsList(AvailableSwordAttacks);
                            CM.player.GetComponent<PlayerController>().playerBattleState = PlayerBattleSelections.Sword;
                        }
                        else if (playersCurrentSelection == 1 && playerCurrentlySelecting)//Jump
                        {
                            LoadSkillOptionsList(AvailableJumpAttacks);
                            CM.player.GetComponent<PlayerController>().playerBattleState = PlayerBattleSelections.Jump;
                        }
                        else if (playersCurrentSelection == 2 && playerCurrentlySelecting)//Item
                        {
                            //LoadSkillOptionsList(Items to skill conversion)
                            //CM.player.GetComponent<PlayerController>().playerBattleState = PlayerBattleSelections.Sword;
                            //SecondMenuOpened();
                        }
                        else if (playersCurrentSelection == 3 && playerCurrentlySelecting)//Special
                        {
                            //LoadSkillOptionsList(Special List)
                            //SecondMenuOpened();
                        }
                        else if (playersCurrentSelection == 4 && playerCurrentlySelecting)//Misc
                        {
                            //LoadSkillOptionsList(Misc Decisions)
                        }

                        //PartnerBattleSelections { Attack, Item, Misc }
                        else if (playersCurrentSelection == 0 && playerCurrentlySelecting == false)//Parter Attack
                        {
                            LoadSkillOptionsList(AvailablePartnerAttacks);
                            CM.partner.GetComponent<PartnerController>().partnerBattleState = PartnerBattleSelections.Attack;
                        }


                        if(maxAmountOfDecisions > 1)
                        {
                            SecondMenuOpened();
                        }
                        else
                        {
                            currentSkillBeingUsed = GetSkillFromList(ReturnList());
                            ActivateTargetSelector(currentSkillBeingUsed);
                        }
                    }

                    if (Input.GetButtonDown("B Button"))
                    {
                        BackMenuButtonPressed();
                    }

                    break;
                case PlayerSelectionState.SecondSelectionMenu:

                    if (isAnimating == false)
                    {
                        playerInputVertical = Input.GetAxis("Vertical");

                        if (playerInputVertical > CONTROLLER_SENSITIVITY && selectionMenu.activeInHierarchy)//Up
                        {
                            UpOrDownSecondSelectionMenu(true);
                        }
                        else if (playerInputVertical < -CONTROLLER_SENSITIVITY && selectionMenu.activeInHierarchy)//Down
                        {
                            UpOrDownSecondSelectionMenu(false);
                        }

                    }
                    else
                    {
                        cursorTimer += Time.deltaTime;
                        isAnimating = CursorTimerLimitReahed(TIME_BETWEEN_EACH_SELECTION);
                    }

                    if (Input.GetButtonDown("Interact"))
                    {
                        currentSkillBeingUsed = GetSkillFromList(ReturnList(), playersCurrentSelection);
                        ActivateTargetSelector(currentSkillBeingUsed);
                    }

                    if(Input.GetButtonDown("B Button"))
                    {
                        BackMenuButtonPressed();
                    }

                    break;
                case PlayerSelectionState.SelectingEnemyToAttack:

                    //Will give the player the ability to only change target this is only meant to hit the first target
                    if(currentSkillBeingUsed.frontOpponentTargetOnly == false)
                    {
                        if (isAnimating == false)
                        {
                            playerInputHorizontal = Input.GetAxis("Horizontal");

                            if(currentSkillBeingUsed.multiTarget ==false)// as long as its not hitting multiple targets it cannot move left and right
                            {
                                if (playerInputHorizontal > CONTROLLER_SENSITIVITY && targetSelector[0].activeInHierarchy)//Right
                                {
                                    FindTarget(true, currentSkillBeingUsed);
                                }
                                else if (playerInputHorizontal < -CONTROLLER_SENSITIVITY && targetSelector[0].activeInHierarchy)//Left
                                {
                                    FindTarget(false, currentSkillBeingUsed);
                                }
                            }

                        }
                        else
                        {
                            cursorTimer += Time.deltaTime;
                            isAnimating = CursorTimerLimitReahed(TIME_BETWEEN_EACH_SELECTION);
                        }
                    }

                    if (Input.GetButtonDown("Interact"))
                    {
                        DeactivateTargetSelector();
                        CM.SkillUsed(currentSkillBeingUsed, enemyTargets);
                        playerSelectionState = PlayerSelectionState.AttackAnimating;
                    }

                    if (Input.GetButtonDown("B Button"))
                    {
                        BackMenuButtonPressed();
                    }

                    break;
                case PlayerSelectionState.AttackAnimating:

                    //Apply bonus multiplier here

                    break;
                default:
                    break;
            }
        }

	}

    #region StartPlayersTurn



    #endregion

    #region Selection Bubble Menu (First Menu)

    void PresetInstantiateBubbleMenuSize()
    {
        maxAmountOfDecisions = System.Enum.GetNames(typeof(PlayerBattleSelections)).Length;

        if (maxAmountOfDecisions < System.Enum.GetNames(typeof(PartnerBattleSelections)).Length)
        {
            maxAmountOfDecisions = System.Enum.GetNames(typeof(PartnerBattleSelections)).Length;
        }

        selectionBubbles = new SelectionBubbles[maxAmountOfDecisions];

        for (int i = 0; i < maxAmountOfDecisions; i++)
        {
            GameObject go = Instantiate(selectionBubblePrefab, this.transform.position, Quaternion.identity);
            go.transform.parent = playerSelectionParent.transform;
            selectionBubbles[i].gameObject = go.gameObject;
            selectionBubbles[i].imageUI = go.transform.Find("Canvas/Sprite").GetComponent<Image>();
            selectionBubbles[i].textUI = go.transform.Find("Canvas/Text").GetComponent<Text>();
            selectionBubbles[i].textUI.text = ((PlayerBattleSelections)i).ToString();
        }
    }

    /// <summary>
    /// This will show load what the bubbles need depnding on whos turn it currently is,
    /// upon finishing this shall turn the bubbles on
    /// </summary>
    /// <param name="isPlayersTurn"> is it the players turn or the partners turn</param>
    void LoadCurrentPlayersBubbles(bool isPlayersTurn)
    {
        SelectionBubblesActive(false);

        if (isPlayersTurn == true)
        {
            maxAmountOfDecisions = System.Enum.GetNames(typeof(PlayerBattleSelections)).Length;
        }
        else //isPlayersTurn == false
        {
            maxAmountOfDecisions = System.Enum.GetNames(typeof(PartnerBattleSelections)).Length;
        }

        selectionBubbles = new SelectionBubbles[maxAmountOfDecisions];

        for (int i = 0; i < maxAmountOfDecisions; i++)
        {
            selectionBubbles[i].gameObject = playerSelectionParent.transform.GetChild(i).gameObject;
            selectionBubbles[i].imageUI = playerSelectionParent.transform.GetChild(i).Find("Canvas/Sprite").GetComponent<Image>();
            selectionBubbles[i].textUI = playerSelectionParent.transform.GetChild(i).Find("Canvas/Text").GetComponent<Text>();
            if(isPlayersTurn == true)
            {
                selectionBubbles[i].textUI.text = ((PlayerBattleSelections)i).ToString();
            }
            else
            {
                selectionBubbles[i].textUI.text = ((PartnerBattleSelections)i).ToString();
            }
            
        }

        SetBubbleLocations(maxAmountOfDecisions, selectionBubbles);
        currentEnemySelected = 0;
        playersCurrentSelection = 0;

        SelectionBubblesActive(true);
    }

    void SetBubbleLocations(int howMany,SelectionBubbles[] bubbles)
    {
        for (int i = 0; i < howMany; i++)
        {
            float angle = i * Mathf.PI * 2f / howMany;
            angle += OFFSET_FOR_STARTING_POSITION_ANGLE;
            Vector3 newPos = new Vector3(Mathf.Cos(angle) * radiusOfSelectionBubbles, BUBBLE_FLOATING_HEIGHT, Mathf.Sin(angle) * radiusOfSelectionBubbles);
            newPos = newPos + playerSelectionParent.transform.position;
            bubbles[i].gameObject.transform.position = newPos;
        }
    }

    public void SelectionBubblesActive(bool isOn)
    {
        //playerSelectionParent.gameObject.SetActive(isOn);
        for (int i = 0; i < selectionBubbles.Length; i++)
        {
            selectionBubbles[i].gameObject.SetActive(isOn);
        }
    }

    float RotateSelectionBubbles(bool goRight)
    {
        isAnimating = true;
        rotatingBubbles = true;
        float angle = Mathf.PI * 2f / selectionBubbles.Length;
        angle *= Mathf.Rad2Deg;
        originalAngleStartedAt = Mathf.RoundToInt(playerSelectionParent.transform.eulerAngles.y);
        leftoverAngleToRotate = Mathf.Abs(angle);
        if (goRight==false)
        {
            playersCurrentSelection--;
            angle = -angle;
        }
        else
        {
            playersCurrentSelection++;
        }

        if(playersCurrentSelection < 0)
        {
            playersCurrentSelection = maxAmountOfDecisions - 1;
        }
        else if(playersCurrentSelection > maxAmountOfDecisions - 1)
        {
            playersCurrentSelection = 0;
        }

        return angle;
    }

    void AnimatingRotationOfSelectionBubbles(float speed)
    {
        float angle = speed / angleToRotate;
        playerSelectionParent.transform.localEulerAngles = new Vector3(0, playerSelectionParent.transform.eulerAngles.y + angle, 0);
        for (int i = 0; i < selectionBubbles.Length; i++)
        {
            selectionBubbles[i].gameObject.transform.eulerAngles = new Vector3(0, selectionBubbles[i].gameObject.transform.eulerAngles.y - angle, 0);
        }

        leftoverAngleToRotate -= Mathf.Abs(angle);

        if(leftoverAngleToRotate <=0)
        {
            rotatingBubbles = false;
            isAnimating = false;
            playerSelectionParent.transform.localEulerAngles = new Vector3(0, originalAngleStartedAt + angleToRotate, 0);
            for (int i = 0; i < selectionBubbles.Length; i++)
            {
                selectionBubbles[i].gameObject.transform.localEulerAngles = new Vector3(0, -playerSelectionParent.transform.localEulerAngles.y, 0);
            }
        }
    }

    /// <summary>
    /// This is for when you back out of the menu, if the player goes into the menu prior to 
    /// the animation spinning finishing it will set the bubbles to the corresponding locations
    /// as well as deactiving the bubbles
    /// </summary>
    void SetBubblesToSetLocation()
    {
        SelectionBubblesActive(false);
        playerSelectionParent.transform.localEulerAngles = new Vector3(0, originalAngleStartedAt + angleToRotate, 0);
        for (int i = 0; i < selectionBubbles.Length; i++)
        {
            selectionBubbles[i].gameObject.transform.localEulerAngles = new Vector3(0, -playerSelectionParent.transform.localEulerAngles.y, 0);
        }
    }

    #endregion

    #region SelectionMenu (Second Menu)

    public void PresetInstantiateSelectionMenuSize()
    {
        int maxSize = 0;
        int[] listSizes =
        {
            AvailableSwordAttacks.Count,
            AvailableJumpAttacks.Count,
            AvailablePartnerAttacks.Count
        };

        for (int i = 0; i < listSizes.Length; i++)
        {
            if(maxSize < listSizes[i])
            {
                maxSize = listSizes[i];
            }
        }

        selectionMenuChoices = new SelectionMenu[maxSize];

        for (int i = 0; i < maxSize; i++)
        {
            GameObject go = Instantiate(selectionMenuOptionsPrefab, selectionMenuChildParent.position, Quaternion.identity);
            go.transform.parent = selectionMenuChildParent;

            selectionMenuChoices[i].gameObject = go.gameObject;
            selectionMenuChoices[i].imageUI = go.transform.Find("Sprite").GetComponent<Image>();
            selectionMenuChoices[i].skillNameUI = go.transform.Find("Name").GetComponent<Text>();
            selectionMenuChoices[i].skillPpCost = go.transform.Find("PowerPoints").GetComponent<Text>();
            selectionMenuChoices[i].gameObject.transform.localPosition = new Vector3(0, SELECTION_MENU_OPTIONS_STARTING_HEIGHT - (SELECTION_MENU_OPTIONS_HEIGHT_DIFFERENCE * i), 0);
            selectionMenuChoices[i].gameObject.SetActive(false);
        }
    }

    void DeactivateSelectionMenuChoices()
    {
        for (int i = 0; i < selectionMenuChoices.Length; i++)
        {
            selectionMenuChoices[i].gameObject.SetActive(false);
        }
    }

    void LoadSkillOptionsList(List<Skills> currentList)
    {
        // Zoom in camera On Player Selecting
        maxAmountOfDecisions = currentList.Count;

        DeactivateSelectionMenuChoices();

        if(maxAmountOfDecisions <= 1)
        {
            return;
        }

        for (int i = 0; i < currentList.Count; i++)
        {
            selectionMenuChoices[i].imageUI.sprite = currentList[i].skillSprite;
            selectionMenuChoices[i].skillNameUI.text = currentList[i].skillName;
            if (currentList[i].powerPoints == 0)
            {
                selectionMenuChoices[i].skillPpCost.text = "";
            }
            else
            {
                selectionMenuChoices[i].skillPpCost.text = currentList[i].powerPoints.ToString() + " PP";
            }
            selectionMenuChoices[i].gameObject.SetActive(true);
        }
    }

    void SecondMenuOpened()
    {
        selectionMenu.SetActive(true);
        playerSelectionState = PlayerSelectionState.SecondSelectionMenu;
        playersCurrentSelection = 0;
        secondSelectionMenu_Selector.transform.position = secondSelectionMenu_SelectorOriginalPosition;
        isAnimating = false;
    }

    void SecondMenuClosed()
    {
        selectionMenu.SetActive(false);
    }

    void UpOrDownSecondSelectionMenu(bool goUp)
    {
        isAnimating = true;

        if (goUp == true)
        {
            playersCurrentSelection--;
        }
        else
        {
            playersCurrentSelection++;
        }

        if (playersCurrentSelection < 0)
        {
            playersCurrentSelection = maxAmountOfDecisions - 1;
        }
        else if (playersCurrentSelection > maxAmountOfDecisions-1)
        {
            playersCurrentSelection = 0;
        }

        secondSelectionMenu_Selector.transform.position = new Vector3(secondSelectionMenu_Selector.transform.position.x, selectionMenuChoices[playersCurrentSelection].gameObject.transform.position.y + SELECTION_MENU_SELECTOR_POINTER_HEIGHT_DIFFERENCE, secondSelectionMenu_Selector.transform.position.z);
        //adjust pivot point for each position that the finger can point
        //make the point move to the bottom of the list if there are multiple selections
    }

    bool CursorTimerLimitReahed(float timelimit)//for the isAnimating, so it will return the opposite because animating shall be done
    {
        if(cursorTimer > timelimit)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    #endregion

    #region Select Target

    /// <summary>
    /// This shall spawn the max amount of target selectors that would be needed for the battle scene
    /// </summary>
    void PresetInstantiateTargetSelectionSize()
    {
        int maxAmountOfTargets = GM.enemiesForBattle.Count;
        targetSelector = new GameObject[maxAmountOfTargets];

        for (int i = 0; i < maxAmountOfTargets; i++)
        {
            targetSelector[i] = Instantiate(targetSelectorPrefab, Vector3.zero, Quaternion.identity);
        }
    }

    /// <summary>
    /// Activate's the Selector GameObject and puts it over the first enemy towards the left, if it is a ground only move then it will find the first ground target 
    /// </summary>
    void ActivateTargetSelector(Skills currentskill)
    {
        bool isGroundAttack = currentskill.groundOnlyAttack;
        bool isAirOnlyAttack = currentskill.airOnlyAttack;
        bool isEnemy = currentskill.offensiveSkill;
        bool attacksMultipleTargets = currentskill.multiTarget;

        //Deactive Second Menu Requirements
        SecondMenuClosed();

        //Check if its a ground type attack or air or both
        if (isEnemy == true)
        {
            playerSelectionState = PlayerSelectionState.SelectingEnemyToAttack;
            enemyTargets = new List<GameObject>();
            if (attacksMultipleTargets == false)
            {
                targetSelector[0].SetActive(true);

                if (isGroundAttack == true || isAirOnlyAttack == true)
                {
                    EnemyController enemy = null;

                    //This will go through and find the first available target and put the target selector over them
                    for (int i = 0; i < CM.enemies.Count; i++)
                    {
                        enemy = CM.enemies[i].GetComponent<EnemyController>();
                        if (enemy.isGroundTarget == true && isGroundAttack == true)
                        {
                            MoveTargetSelector(CM.enemies[i]);
                            currentEnemySelected = i;
                            break;
                        }
                        else if (enemy.isFlyingTarget == true && isAirOnlyAttack == true)
                        {
                            MoveTargetSelector(CM.enemies[i]);
                            currentEnemySelected = i;
                            break;
                        }
                    }
                }
                else
                {
                    MoveTargetSelector(CM.enemies[0]);//Shall be fixed due to the fact that this unit could be dead
                    enemyTargets.Add(CM.enemies[0]);
                }
            }
            else //Attacking Multiple Targets
            {

                for (int i = 0; i < CM.enemies.Count; i++)
                {
                    if (isGroundAttack == true || isAirOnlyAttack == true)
                    {
                        EnemyController enemy = null;
                        enemy = CM.enemies[i].GetComponent<EnemyController>();

                        if (enemy.isGroundTarget == true && isGroundAttack == true)
                        {
                            enemyTargets.Add(enemy.gameObject);
                            continue;
                        }
                        else if (enemy.isFlyingTarget == true && isAirOnlyAttack == true)
                        {
                            enemyTargets.Add(enemy.gameObject);
                            continue;
                        }

                    }
                    else
                    {
                        enemyTargets.Add(CM.enemies[i]);
                        continue;
                    }
                }

                MultipleTargetsSelected(enemyTargets);
            }
        }
        else //not an enemy Target
        {
            playerSelectionState = PlayerSelectionState.SelectingTargetToHeal;

            MoveTargetSelector(CM.player);
        }
    }

    void DeactivateTargetSelector()
    {
        for (int i = 0; i < targetSelector.Length; i++)
        {
            targetSelector[i].SetActive(false);
        }
    }

    /// <summary>
    /// this shall go to the next target depending on the players input received
    /// </summary>
    /// <param name="isGroundAttack">Can the enemy get hit by ground only attacks</param>
    /// <param name="isAirOnlyAttack">Is the attack a air only attack?</param>
    /// <param name="isEnemy">Shall always remain true unless the partner is trying to use an ability or item that shall benefit the player or partner</param>
    void FindTarget(bool right,Skills currentSkill)
    {
        bool isGroundAttack = currentSkill.groundOnlyAttack;
        bool isAirOnlyAttack = currentSkill.airOnlyAttack;
        bool isEnemy = currentSkill.offensiveSkill;

        isAnimating = true;

        EnemyController enemy = null;
        //Mesh bound extent on the y axis
        if (isEnemy == true)
        {
            //grabs all available targets that can be hit
            availableEnemiesToAttack = new List<EnemyController>();
            for (int i = 0; i < CM.enemies.Count; i++)
            {
                enemy = CM.enemies[i].GetComponent<EnemyController>();
                if(isGroundAttack == true)
                {
                    if(enemy.isGroundTarget == true)
                    {
                        availableEnemiesToAttack.Add(enemy);
                    }

                    continue;
                }
                else if(isAirOnlyAttack == true)
                {
                    if (enemy.isFlyingTarget == true)
                    {
                        availableEnemiesToAttack.Add(enemy);
                    }
                    continue;
                }
                availableEnemiesToAttack.Add(enemy);
            }

            if(availableEnemiesToAttack.Count <= 1)
            {
                return;
            }

            enemy = CM.enemies[currentEnemySelected].GetComponent<EnemyController>();
            int nextPos;
            nextPos = availableEnemiesToAttack.IndexOf(enemy);

            if (right == true)
            {
                if(nextPos < availableEnemiesToAttack.Count -1)//-1 is to corrrent it to the index of the array
                {
                    nextPos++;
                }
                else
                {
                    nextPos = 0;
                }

            }
            else //Left
            {
                if (nextPos <= 0)
                {
                    nextPos = availableEnemiesToAttack.Count - 1;
                }
                else
                {
                    nextPos--;
                }
            }
            enemy = availableEnemiesToAttack[nextPos].GetComponent<EnemyController>();
            currentEnemySelected = CM.enemies.IndexOf(enemy.gameObject);

            MoveTargetSelector(CM.enemies[currentEnemySelected]);
        }
    }

    void MoveTargetSelector(GameObject currentTarget)
    {
        enemyTargets = new List<GameObject>();
        enemyTargets.Add(currentTarget);
        Transform tempTrans = currentTarget.transform;
        targetSelector[0].transform.position = new Vector3(tempTrans.position.x, tempTrans.position.y + tempTrans.localScale.y + TARGET_SELECTOR_HEIGHT_ADDITION, tempTrans.position.z);
    }

    void MultipleTargetsSelected(List<GameObject> enemyTargets)
    {
        for (int i = 0; i < enemyTargets.Count; i++)
        {
            targetSelector[i].SetActive(true);
            Transform tempTrans = enemyTargets[i].transform;
            targetSelector[i].transform.position = new Vector3(tempTrans.position.x, tempTrans.position.y + tempTrans.localScale.y + TARGET_SELECTOR_HEIGHT_ADDITION, tempTrans.position.z);
        }
    }

    #endregion

    #region Start/Finish Turn

    /// <summary>
    /// This is called whenever the players turn is going to start and after all the special starting requirements are met
    /// After Power Struggle and First strikes are finished or when enemies turns are finished
    /// </summary>
    void StartPlayersTurn()
    {
        playerSelectionState = PlayerSelectionState.FirstSelectionMenu;
        SelectionBubblesActive(true);
        ignoreInput = false;
    }


    /// <summary>
    /// This is called whenever the current attacker has returned to his place location on the battle field, 
    /// this is to ensure that their turn has ended and all animations are complete
    /// </summary>
    public void AttackerReturnedAndFinished()
    {
        PlayerController playerController = CM.player.GetComponent<PlayerController>();
        PartnerController partnerController = CM.partner.GetComponent<PartnerController>();

        EnemyController[] enemyController = CM.ReturnAllEnemies();

        if(CM.areAllEnemiesDead() == true)
        {
            CM.PlayerWonBattle();
        }

        CM.TurnOnColliderTemporarily();

        if(playerController.battleState != BattleState.FinishedTurn || partnerController.battleState != BattleState.FinishedTurn)
        {//First attacker has completed their turn and now its the seconds slots turn

            playerCanCurrentlySwitch = false;//players battle positions are set at this point, no switching allowed
            playerSelectionState = PlayerSelectionState.FirstSelectionMenu;

            if(playerController.battleState != BattleState.FinishedTurn)
            {
                playerCurrentlySelecting = true;
                playerController.battleState = BattleState.WaitingToAttack;
            }
            else //partnerController.battleState != BattleState.FinishedTurn
            {
                playerCurrentlySelecting = false;
                partnerController.battleState = BattleState.CurrentTurn;
            }

            LoadCurrentPlayersBubbles(playerCurrentlySelecting);
            SelectionBubblesActive(true);
        }
        else if(playerController.battleState == BattleState.FinishedTurn && partnerController.battleState == BattleState.FinishedTurn)
        {
            //go to the enemies turns,this shall be set in the combat manager
            ignoreInput = true;
            CM.StartEnemyTurn();
        }

    }

    #endregion

    #region Back Menu

    void BackMenuButtonPressed()
    {
        switch (playerSelectionState)
        {
            case PlayerSelectionState.FirstSelectionMenu:
                break;
            case PlayerSelectionState.SecondSelectionMenu:

                SecondMenuClosed();
                playerSelectionState = PlayerSelectionState.FirstSelectionMenu;

                if (playerCurrentlySelecting == true)
                {
                    maxAmountOfDecisions = System.Enum.GetNames(typeof(PlayerBattleSelections)).Length;
                    SelectionBubblesActive(true);
                    playersCurrentSelection = (int)CM.player.GetComponent<PlayerController>().playerBattleState;
                }
                else
                {
                    //Partners Bs
                }

                break;
            case PlayerSelectionState.SelectingEnemyToAttack:

                DeactivateTargetSelector();
                currentEnemySelected = 0;

                //This double chacks if there is only one attack
                if (maxAmountOfDecisions > 1)
                {
                    playerSelectionState = PlayerSelectionState.SecondSelectionMenu;
                    LoadSkillOptionsList(ReturnList());
                    SecondMenuOpened();
                    playersCurrentSelection = ReturnList().IndexOf(currentSkillBeingUsed);
                    secondSelectionMenu_Selector.transform.position = new Vector3(secondSelectionMenu_Selector.transform.position.x, selectionMenuChoices[playersCurrentSelection].gameObject.transform.position.y + SELECTION_MENU_SELECTOR_POINTER_HEIGHT_DIFFERENCE, secondSelectionMenu_Selector.transform.position.z);
                }
                else
                {
                    playerSelectionState = PlayerSelectionState.FirstSelectionMenu;

                    maxAmountOfDecisions = System.Enum.GetNames(typeof(PlayerBattleSelections)).Length;
                    SelectionBubblesActive(true);
                    playersCurrentSelection = (int)CM.player.GetComponent<PlayerController>().playerBattleState;
                }

                break;
            case PlayerSelectionState.SelectingTargetToHeal:
                break;
        }
    }

    #endregion

    #region Switch Positions

    void SwitchPositions()
    {
        if(playerCanCurrentlySwitch)
        {
            //Make units switch position
            playerSelectionState = PlayerSelectionState.FirstSelectionMenu;
            if(playerCurrentlySelecting == true)//Players turn
            {
                playerCurrentlySelecting = false;
            }
            else
            {
                playerCurrentlySelecting = true;
            }

            SecondMenuClosed();
            DeactivateTargetSelector();
            LoadCurrentPlayersBubbles(playerCurrentlySelecting);
        }
    }

    #endregion
}

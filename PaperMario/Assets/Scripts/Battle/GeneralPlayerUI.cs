using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct HealthBar{
    public Image healthImage;
    public Text healthText;
}

public class GeneralPlayerUI : MonoBehaviour {

    /*
        This is the same Script for the player user world interface and the combat user interface,
        While in the overworld this shall remain hidden, unless triggered by a controller input. 
        If so this shall come down and stay down as long as there is no player input. If there is 
        after X amount of seconds then the UI shall be displayed and then go upwards off the screen 
        and hide. If the player takes damage or is healed this shall popdown and show the player 
        either loosing or gaining health.

     
     */


    //Player
    [Header("Player")]
    [SerializeField]
    GameObject playerHealthBarGameObject;
    [SerializeField]
    Image playerHealthImage;
    [SerializeField]
    Image playerExperience;
    [SerializeField]
    Text playerHealthText;
    Vector2 playerHealthGUILocation;
    HealthBar playerHealthBar;
    bool currentlyGoingToDisplayPlayersHealth;
    Coroutine playerHealthCoroutine = null;
    Coroutine playerAnimatedHealthUICoroutine = null;
    

    //Partner
    [Header("Partner")]
    [SerializeField]
    GameObject partnerHealthBarGameObject;
    [SerializeField]
    Image partnerSprite;
    [SerializeField]
    Image partnerHealth;
    Text partnerHealthText;
    Vector2 partnerHealthGUILocation;
    HealthBar partnerHealthBar;
    bool currentlyGoingToDisplayPartnersHealth;
    Coroutine partnerHealthCoroutine = null;
    Coroutine partnerAnimatedHealthUICoroutine = null;

    PlayerController playerController;
    PartnerController partnerController;

    //Enemy
    [Header("Enemy")]
    [SerializeField]
    GameObject enemyHealthBarPrefab;
    List<HealthBar> enemyHealthBarObjectPool;

    //Const
    const int Y_POSITION_OFFSCREEN = 175;
    const float SPEED_OF_BAR_MOVEMENT_HEALTH_LOSS = 1.5f;
    const float SPEED_OF_UI_ANIMATION = 3f;


    //Testing Only
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            ShowAllGUIInterfaceAnimated();
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            HideAllGUIInterfaceAnimated();
        }
    }


    void Start()
    {
        GameManager GM = GameManager.instance;
        GM.GeneralPlayerUI = this;

        //used as a temp reference of where the player/partners UI starts
        RectTransform startingTempRectTrans = null;

        //Player
        playerController = GM.player.GetComponent<PlayerController>();
        startingTempRectTrans = playerHealthBarGameObject.GetComponent<RectTransform>();
        playerHealthGUILocation = new Vector2(startingTempRectTrans.anchoredPosition.x ,startingTempRectTrans.anchoredPosition.y);
        playerHealthBar = new HealthBar
        {
            healthImage = playerHealthImage,
            healthText = playerHealthText
        };

        //Partner
        //partnerController = GameManager.instance.currentPartner.GetComponent<PartnerController>();
        startingTempRectTrans = partnerHealthBarGameObject.GetComponent<RectTransform>();
        partnerHealthGUILocation = new Vector2(startingTempRectTrans.anchoredPosition.x, startingTempRectTrans.anchoredPosition.y);

        enemyHealthBarObjectPool = new List<HealthBar>();
        //5 is due to the max amount of units available in the battle
        for (int i = 0; i < GM.MAX_AMOUNT_OF_UNITS_ON_FIELD; i++)
        {
            //GameObject temp = Instantiate(enemyHealthBarPrefab, this.transform);

            //HealthBar tempHealthbar = new HealthBar();

            //tempHealthbar.healthImage =
            //tempHealthbar.healthText = 

            //enemyHealthBarObjectPool.Add(tempHealthbar);
        }



    }

    void OnEnable()
    {
        //partnerSprite.sprite = partnerController.partnerBattleHealthSpriteUI;
    }


    /// <summary>
    /// Should be only applied in the overworld when the player is either damaged 
    /// or uses a healing item or a item that restores PP. Can still apply if the 
    /// partner gets hurt but there should be no reason that they do in the overworld
    /// </summary>
    /// <param name="currentUI"> Current UI that shall be effected</param>
    /// <param name="targetLocation">Original location of target UI</param>
    /// <returns></returns>
    bool CurrentUIBeingDisplayed(GameObject currentUI,Vector2 targetLocation)
    {
        if(currentUI.activeInHierarchy == false)
        {
            //Activate coroutine that shall move into position
            return false;
        }

        RectTransform tempTrans = currentUI.GetComponent<RectTransform>();

        if (tempTrans.anchoredPosition.x != targetLocation.x || tempTrans.anchoredPosition.y != targetLocation.y)
        {
            //Current Coroutine should be active and on route
            //A bool should be created if it is in motion to see if its on route or not
            return false;
        }

        return true;
    }

    public void EntityDamageTaken(Entity currentTarget,int damageTaken)
    {
        if(currentTarget is PlayerController)
        {
            if(CurrentUIBeingDisplayed(playerHealthBarGameObject,playerHealthGUILocation) == true)
            {
                if (playerHealthCoroutine == null)
                {
                    playerHealthCoroutine = StartCoroutine(DamageTakenDisplay(currentTarget.currHealth, currentTarget.getMaxHealth, damageTaken, playerHealthBar, playerHealthCoroutine));
                }
                else
                {
                    StopCoroutine(playerHealthCoroutine);
                    playerHealthCoroutine = StartCoroutine(DamageTakenDisplay(currentTarget.currHealth, currentTarget.getMaxHealth, damageTaken, playerHealthBar, playerHealthCoroutine));
                }
            }
            else
            {
                if (playerAnimatedHealthUICoroutine == null)
                {
                    IEnumerator playerHealthIEnumerator = DamageTakenDisplay(currentTarget.currHealth, currentTarget.getMaxHealth, damageTaken, playerHealthBar, playerHealthCoroutine);
                    playerAnimatedHealthUICoroutine = StartCoroutine(AnimatedUIToPosition(playerHealthBarGameObject, playerHealthGUILocation, playerAnimatedHealthUICoroutine, playerHealthCoroutine, playerHealthIEnumerator));
                }
                else
                {
                    StopCoroutine(playerAnimatedHealthUICoroutine);
                    IEnumerator playerHealthIEnumerator = DamageTakenDisplay(currentTarget.currHealth, currentTarget.getMaxHealth, damageTaken, playerHealthBar, playerHealthCoroutine);
                    playerAnimatedHealthUICoroutine = StartCoroutine(AnimatedUIToPosition(playerHealthBarGameObject, playerHealthGUILocation, playerAnimatedHealthUICoroutine, playerHealthCoroutine, playerHealthIEnumerator));
                }
            }
        }
        else if (currentTarget is PartnerController)
        {
            if(CurrentUIBeingDisplayed(partnerHealthBarGameObject,partnerHealthGUILocation) == true)
            {
                if(partnerHealthCoroutine == null)
                {
                    partnerHealthCoroutine = StartCoroutine(DamageTakenDisplay(currentTarget.currHealth, currentTarget.getMaxHealth, damageTaken, partnerHealthBar, partnerHealthCoroutine));
                }
                else
                {
                    StopCoroutine(partnerHealthCoroutine);
                    partnerHealthCoroutine = StartCoroutine(DamageTakenDisplay(currentTarget.currHealth, currentTarget.getMaxHealth, damageTaken, partnerHealthBar,partnerHealthCoroutine));
                }
            }
            else
            {
                //This should never be called
            }
        }
        else if(currentTarget is EnemyController)
        {
            EnemyController enemyController = currentTarget as EnemyController;
            
            if(enemyController.selfReferenceBestiary.examined == true)
            {
                //Enemy Style Damage Here
            }
        }
        else
        {
            Debug.LogError("current Target is giving an error", gameObject);
            //This is if an entity besides the player, partner or enemy controller is taking damage somehow
        }
    }

    IEnumerator DamageTakenDisplay(int currentHealth,int maxHealth,int damageTaken, HealthBar targetHealthBar,Coroutine thisCoroutine)
    {
        float t = 0;
        float speedOfBar = SPEED_OF_BAR_MOVEMENT_HEALTH_LOSS;

        //This shall not jump even if there are multiple hits detected even when the target has already been hit
        float sizeOfBarLost = -(((float)currentHealth - (float)damageTaken)/(float)maxHealth) + targetHealthBar.healthImage.fillAmount;
        float currentBarAmount = targetHealthBar.healthImage.fillAmount;

        while (t/speedOfBar < damageTaken)
        {
            targetHealthBar.healthImage.fillAmount = currentBarAmount - (sizeOfBarLost *(t / speedOfBar));
            targetHealthBar.healthText.text = HitPointText((Mathf.RoundToInt(targetHealthBar.healthImage.fillAmount*maxHealth)), maxHealth);
            yield return null;
            t += Time.deltaTime;
        }

        targetHealthBar.healthImage.fillAmount = ((float)currentHealth - (float)damageTaken) / (float)maxHealth;
        targetHealthBar.healthText.text = HitPointText((currentHealth - damageTaken), maxHealth);

        thisCoroutine = null;
    }

    void HealthChangeEvent(int currentHealth, int maxHealth, HealthBar targetHealthBar)
    {
        targetHealthBar.healthImage.fillAmount = currentHealth / maxHealth;
        targetHealthBar.healthText.text = HitPointText(currentHealth, maxHealth);
    }

    /// <summary>
    /// Returns the text of what the targets health is in string format
    /// </summary>
    /// <param name="currentHealth">What the Targets current health shall be</param>
    /// <param name="maxHealth">Total Health of the target unit</param>
    /// <returns></returns>
    string HitPointText(int currentHealth, int maxHealth)
    {
        string currentHP = "HP " + currentHealth.ToString() + "/" + maxHealth.ToString();
        return currentHP;
    }

    void ShowAllGUIInterfaceInstant()
    {

    }

    void ShowAllGUIInterfaceAnimated()
    {
        if(playerAnimatedHealthUICoroutine == null)
        {
            playerAnimatedHealthUICoroutine = StartCoroutine(AnimatedUIToPosition(playerHealthBarGameObject, playerHealthGUILocation, playerAnimatedHealthUICoroutine));
        }
        else
        {
            StopCoroutine(playerAnimatedHealthUICoroutine);
            playerAnimatedHealthUICoroutine = StartCoroutine(AnimatedUIToPosition(playerHealthBarGameObject, playerHealthGUILocation, playerAnimatedHealthUICoroutine));
        }

        if(partnerAnimatedHealthUICoroutine == null)
        {
            partnerAnimatedHealthUICoroutine = StartCoroutine(AnimatedUIToPosition(partnerHealthBarGameObject, partnerHealthGUILocation, partnerAnimatedHealthUICoroutine));
        }
        else
        {
            StopCoroutine(partnerAnimatedHealthUICoroutine);
            partnerAnimatedHealthUICoroutine = StartCoroutine(AnimatedUIToPosition(partnerHealthBarGameObject, partnerHealthGUILocation, partnerAnimatedHealthUICoroutine));
        }
    }

    void HideAllGUIInterfaceAnimated()
    {
        if (playerAnimatedHealthUICoroutine == null)
        {
            playerAnimatedHealthUICoroutine = StartCoroutine(AnimatedUIToPosition(playerHealthBarGameObject, new Vector2(playerHealthGUILocation.x, playerHealthGUILocation.y + Y_POSITION_OFFSCREEN) , playerAnimatedHealthUICoroutine));
        }
        else
        {
            StopCoroutine(playerAnimatedHealthUICoroutine);
            playerAnimatedHealthUICoroutine = StartCoroutine(AnimatedUIToPosition(playerHealthBarGameObject, new Vector2(playerHealthGUILocation.x, playerHealthGUILocation.y + Y_POSITION_OFFSCREEN), playerAnimatedHealthUICoroutine));
        }

        if (partnerAnimatedHealthUICoroutine == null)
        {
            partnerAnimatedHealthUICoroutine = StartCoroutine(AnimatedUIToPosition(partnerHealthBarGameObject, new Vector2(partnerHealthGUILocation.x, partnerHealthGUILocation.y + Y_POSITION_OFFSCREEN), partnerAnimatedHealthUICoroutine));
        }
        else
        {
            StopCoroutine(partnerAnimatedHealthUICoroutine);
            partnerAnimatedHealthUICoroutine = StartCoroutine(AnimatedUIToPosition(partnerHealthBarGameObject, new Vector2(partnerHealthGUILocation.x, partnerHealthGUILocation.y + Y_POSITION_OFFSCREEN), partnerAnimatedHealthUICoroutine));
        }
    }

    void InstantDisplayPlayerHealthUI()
    {
        playerHealthBarGameObject.SetActive(true);
        playerHealthBarGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(playerHealthGUILocation.x, playerHealthGUILocation.y, 0);
    }

    void InstantHidePlayerHealthUI()
    {
        playerHealthBarGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(playerHealthGUILocation.x, playerHealthGUILocation.y+Y_POSITION_OFFSCREEN, 0);
        playerHealthBarGameObject.SetActive(false);
    }

    void InstantDisplayPartnerHealthUI()
    {
        partnerHealthBarGameObject.SetActive(true);
        partnerHealthBarGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(partnerHealthGUILocation.x, partnerHealthGUILocation.y, 0);
    }

    void InstantHidePartnerHealthUI()
    {
        partnerHealthBarGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(partnerHealthGUILocation.x, partnerHealthGUILocation.y + Y_POSITION_OFFSCREEN, 0);
        partnerHealthBarGameObject.SetActive(false);
    }

    /// <summary>
    /// This is to animate without using update to move the current UI in question to the target position 
    /// only by moving it in the Y position, once this is moved if there is another coroutine tied to the 
    /// animated GUI in question it shall activate that coroutine upon finishing.
    /// </summary>
    /// <param name="targetUI">Current UI Gameobject To Be Shown</param>
    /// <param name="targetPosition">Position the current UI in question will be moving towards</param>
    /// <param name="thisCoroutine">A reference to this coroutine to Null out upon being finished</param>
    /// <param name="coroutineCalledUponFinishing">If there is a coroutine involved with the UI in question, 
    /// this shall be activated once this target UI is in position</param>
    /// <param name="iEnumeratorCalledUponFinishing">If there is a coroutine involved with the UI in question, 
    /// These are the parameters involved with the current UI in question</param>
    /// <returns></returns>
    IEnumerator AnimatedUIToPosition(GameObject targetUI,Vector2 targetPosition,Coroutine thisCoroutine, Coroutine coroutineCalledUponFinishing = null,IEnumerator iEnumeratorCalledUponFinishing = null)
    {
        RectTransform tempRectTrans = targetUI.GetComponent<RectTransform>();

        float speed = SPEED_OF_UI_ANIMATION;

        if(targetPosition.y < tempRectTrans.anchoredPosition.y)
        {
            speed = -SPEED_OF_UI_ANIMATION;

            while (targetPosition.y < tempRectTrans.anchoredPosition.y)
            {
                tempRectTrans.anchoredPosition = new Vector2(tempRectTrans.anchoredPosition.x, tempRectTrans.anchoredPosition.y + speed);
                yield return null;
            }
        }
        else
        {

            while (targetPosition.y > tempRectTrans.anchoredPosition.y)
            {
                tempRectTrans.anchoredPosition = new Vector2(tempRectTrans.anchoredPosition.x, tempRectTrans.anchoredPosition.y + speed);
                yield return null;
            }
        }

        tempRectTrans.anchoredPosition = new Vector2(targetPosition.x,targetPosition.y);


        if(iEnumeratorCalledUponFinishing != null)
        {
            coroutineCalledUponFinishing = StartCoroutine(iEnumeratorCalledUponFinishing);
        }

        thisCoroutine = null;
    }

}

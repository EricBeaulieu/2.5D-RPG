using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

//Used for organization for when to check the skills in a dynamic finder. returning what abilities are available and not available
public enum TypeOfSkill{ playerSword, playerJump, playerSpecial, partnerAttack, item, misc }

public class Skills : ScriptableObject {

    public bool abilityActive;
    public string skillName;
    public Sprite skillSprite;
    public int powerPoints;
    [Tooltip("For Targeting Ground Units only")]
    public bool groundOnlyAttack;
    [Tooltip("For Targeting Air Units only")]
    public bool airOnlyAttack;
    [Tooltip("Will this only target the Front Unit")]
    public bool frontOpponentTargetOnly;
    [Tooltip("Will this be used on yourself or your ally")]
    public bool offensiveSkill;
    [Tooltip("Will this hit more then one target")]
    public bool multiTarget;
    [Tooltip("Will only work with abilities that have a charge involved")]
    public float chargeTime = 0;
    public string skillDescription;
    [Tooltip("This is for organizing the list, the lower the priority the higher on the list")]
    public int priorityListing;
    [System.NonSerialized]
    public TypeOfSkill skillType;

}

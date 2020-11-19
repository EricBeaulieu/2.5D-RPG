using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "000 - New Beast", menuName = "Bestiary/New Index")]
public class Bestiary : ScriptableObject {

    [Tooltip("Name of the unit. Normal if regular unit, Bold if Boss")]
    public string bestiaryName;
    [Tooltip("Is this a Boss unit")]
    public bool isBoss;
    [Tooltip("Units Gameobject")]
    public GameObject prefab;
    [Tooltip("Icon of the Gameobject")]
    public Sprite icon;

    [Header("Stats")]
    [Tooltip("The Units HP stat at the start of the battle")]
    public int hitPoints;
    [Tooltip("The Units Attack stat, if there are multiple attacks then this shall state how many and what they are in the description")]
    public int attack;
    [Tooltip("The Units Defense stat")]
    public int defense;

    [Header("Status Affliction")]
    //private Status Affliction

    [Header("Misc.")]
    [Tooltip("Base percentage chance that the player will have when trying to run away from the unit")]
    public float runAwayPercentage;
    [Tooltip("The Description of the enemy when looking through the bestiary, if there are multiple attacks on the unit " +
        "this shall describe more info related to the attack")]
    public string description;
    [Tooltip("Has this unit been inspected?")]
    public bool examined;
    [Tooltip("What the Partner will state when the enemy is first examined")]
    public string whenExamined;

    [Header("Item Table")]
    [Tooltip("Min amount of coins the unit can drop in the overworld upon death")]
    public int minimumCoinDrop;
    [Tooltip("Max amount of coins the unit can drop in the overworld upon death")]
    public int maximumCoinDrop;
    [Tooltip("Items that this unit could potentially drop in the overworld upon death")]
    public ItemPlusPercentage[] itemDrops;
    [Tooltip("items that this unit will be carrying in the overworld and will drop 100% upon death," +
        " the second window will be the % it has on being found with that item")]
    public ItemPlusPercentage[] itemOverworldCarry;

    public Bestiary()
    {
        bestiaryName = "New Beast";
        isBoss = false;
        prefab = null;
        icon = null;

        hitPoints = 10;
        attack = 1;
        defense = 1;

        //status affliction

        runAwayPercentage = 10;
        description = null;
        examined = false;

        minimumCoinDrop = 0;
        maximumCoinDrop = 3;
        itemDrops = new ItemPlusPercentage[0];
        itemOverworldCarry = new ItemPlusPercentage[0];
    }

    public int CoinDrop()
    {
        return Random.Range(minimumCoinDrop, maximumCoinDrop+1);
    }
}

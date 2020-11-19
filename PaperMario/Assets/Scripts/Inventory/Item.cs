using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

    public bool canBeUsedInBattle;
    public bool canBeUsedInOverworld;
    public bool SpecialItem;

    public int shopCost;
    [TextArea]
    public string itemDescription;
}

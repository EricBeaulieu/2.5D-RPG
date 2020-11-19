using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemPlusPercentage{

    public Item item;
    [Range(0,100)]
    public float percentage;

}

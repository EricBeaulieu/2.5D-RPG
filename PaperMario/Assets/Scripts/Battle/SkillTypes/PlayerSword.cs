using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New PlayerSword", menuName = "Skill/PlayerSword")]
public class PlayerSword : Skills {

    public PlayerSword()
    {
        skillType = TypeOfSkill.playerSword;
    }
}

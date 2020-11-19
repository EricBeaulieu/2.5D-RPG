using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New PlayerJump", menuName = "Skill/PlayerJump")]
public class PlayerJump : Skills {

    public PlayerJump()
    {
        skillType = TypeOfSkill.playerJump;
    }
	
}
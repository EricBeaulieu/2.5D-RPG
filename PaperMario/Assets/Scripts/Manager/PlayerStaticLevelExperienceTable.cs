using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerStaticLevelExperienceTable {

    static int[] experienceRequired =
    {
        1,//1
        2,
        3,
        4,
        5,//5
        //This will Max out around 50 or 100
    };

	public static int ExperienceRequiredToNextLevel(int currentLevel)
    {
        int nextLevelExperienceRequired;

        nextLevelExperienceRequired = experienceRequired[currentLevel];

        return nextLevelExperienceRequired;
    }
}

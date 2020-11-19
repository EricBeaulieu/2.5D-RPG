using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ElementalType { Normal, Fire, Water, Earth, Rock, Ice, Wind }

public static class ElementalTypeManager {

    //Effectiveness
    const float veryWeakEffective = 0.6f;
    const float weakEffective = 0.8f;
    const float normalEffective = 1f;
    const float strongEffective = 1.25f;
    const float veryStrongEffective = 1.5f;

    static float[,] elementalTypeGridBonus =
    {
        /*Attack Type -->*/ //Normal, Fire, Water, Earth, Rock, Ice, Wind

        /*Defense Type*/
        /*Normal*/          {normalEffective,normalEffective,normalEffective,normalEffective,normalEffective,normalEffective,normalEffective},
        /*Fire*/            {normalEffective,weakEffective,veryStrongEffective,strongEffective,veryStrongEffective,weakEffective,veryWeakEffective},
        /*Water*/           {normalEffective,veryWeakEffective,weakEffective,weakEffective,veryWeakEffective,veryStrongEffective,strongEffective},
        /*Earth*/           {normalEffective,weakEffective,strongEffective,normalEffective,normalEffective,veryStrongEffective,weakEffective },
        /*Rock*/            {weakEffective,normalEffective,strongEffective,veryStrongEffective,weakEffective,veryWeakEffective,normalEffective },
        /*Ice*/             {weakEffective,veryStrongEffective,veryWeakEffective,weakEffective,veryStrongEffective,weakEffective,weakEffective},
        /*Wind*/            {normalEffective,strongEffective,weakEffective,veryWeakEffective,weakEffective,strongEffective,normalEffective }
    };

public static float ReturnDamageMultiplier(ElementalType attackType,ElementalType defenseType)
    {
        float dmgMul;

        dmgMul = elementalTypeGridBonus[(int)defenseType, (int)attackType];

        return dmgMul;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Partner Attack", menuName = "Skill/PartnerAttack")]
public class PartnerAttack : Skills {

    public Partners whichPartner;

    public PartnerAttack()
    {
        skillType = TypeOfSkill.partnerAttack;
    }
}

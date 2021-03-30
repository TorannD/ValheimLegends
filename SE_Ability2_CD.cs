using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_Ability2_CD : StatusEffect
    {
        public static Sprite AbilityIcon = ValheimLegends.Ability2_Sprite;
        public static GameObject GO_SEFX;

        public SE_Ability2_CD()
        {
            base.name = "SE_VL_Ability2_CD";
            m_icon = ValheimLegends.Ability2_Sprite;
            m_tooltip = ValheimLegends.Ability2_Name + " Cooldown";
            m_name = ValheimLegends.Ability2_Name + " Cooldown";
        }

        public override bool CanAdd(Character character)
        {
            return character.IsPlayer();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_Ability1_CD : StatusEffect
    {
        public static Sprite AbilityIcon = ValheimLegends.Ability1_Sprite;
        public static GameObject GO_SEFX;

        public SE_Ability1_CD()
        {
            base.name = "SE_VL_Ability1_CD";
            m_icon = ValheimLegends.Ability1_Sprite;
            m_tooltip = ValheimLegends.Ability1_Name + " Cooldown";
            m_name = ValheimLegends.Ability1_Name + " Cooldown";
        }

        public override bool CanAdd(Character character)
        {
            return character.IsPlayer();
        }
    }
}

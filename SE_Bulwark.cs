using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_Bulwark : StatusEffect
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_Bulwark")]
        public static float m_baseTTL = 15f;
        private float m_timer = 0f;

        public SE_Bulwark()
        {
            base.name = "SE_VL_Bulwark";
            m_icon = AbilityIcon;
            m_tooltip = "Bulwark";
            m_name = "Bulwark";
            m_ttl = m_baseTTL;
        }

        public override bool CanAdd(Character character)
        {
            return character.IsPlayer();
        }
    }
}

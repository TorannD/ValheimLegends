using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_Riposte : StatusEffect
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_Riposte")]
        public static float m_baseTTL = 2f;
        private float m_timer = 0f;

        public SE_Riposte()
        {
            base.name = "SE_VL_Riposte";
            m_icon = AbilityIcon;
            m_tooltip = "Riposte";
            m_name = "Riposte";
            m_ttl = m_baseTTL;
        }

        public override bool CanAdd(Character character)
        {
            return character.IsPlayer();
        }
    }
}

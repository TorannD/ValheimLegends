using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_Slow : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_Slow")]
        public static float m_baseTTL = 4f;
        public float speedDuration = 3f;
        public float speedAmount = .4f;
        private float m_timer = 1f;

        public SE_Slow()
        {
            base.name = "SE_VL_Slow";
            m_icon = AbilityIcon;
            m_tooltip = "Slow";
            m_name = "Slow";
            m_ttl = m_baseTTL;
        }

        public override void ModifySpeed(float baseSpeed, ref float speed)
        {
            speed *= speedAmount;
            base.ModifySpeed(baseSpeed, ref speed);
        }

        public override bool CanAdd(Character character)
        {
            return true;
        }
    }
}

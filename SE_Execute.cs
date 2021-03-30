using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_Execute : StatusEffect
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_Execute")]
        public static float m_baseTTL = 3f;
        public float staggerForce = 1.2f;
        public float damageBonus = 1.5f;
        public int hitCount = 3;

        public SE_Execute()
        {
            base.name = "SE_VL_Execute";
            m_icon = AbilityIcon;
            m_tooltip = "Execute";
            m_name = "Execute";
            m_ttl = m_baseTTL;
        }

        public override void UpdateStatusEffect(float dt)
        {
            m_ttl = hitCount;
            m_time = 0;
            base.UpdateStatusEffect(dt);
        }

        public override bool CanAdd(Character character)
        {
            return character.IsPlayer();
        }
    }
}

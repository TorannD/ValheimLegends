using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_RootsBuff : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_RootsBuff")]
        public static float m_baseTTL = 200f;
        public float staminaRegen = 1f;
        public float damageModifier = 1f;
        private float m_timer = 0f;
        private float m_interval = 1f;
        public Player summoner;
        public Vector3 centerPoint;

        public SE_RootsBuff()
        {
            base.name = "SE_VL_RootsBuff";
            m_icon = AbilityIcon;
            m_tooltip = "RootsBuff";
            m_name = "RootsBuff";
            m_ttl = m_baseTTL;          
        }

        public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);
            m_timer -= dt;
            if (m_timer <= 0f)
            {
                m_timer = m_interval;
                if(summoner != null)
                {
                    if(summoner.IsDead())
                    {
                        m_time = m_ttl + 1;
                    }
                    else if (centerPoint != null && Vector3.Distance(summoner.transform.position, centerPoint) <= 5f)
                    {
                        summoner.AddStamina(staminaRegen);
                    }                    
                }
                else
                {
                    m_time = m_ttl + 1;
                }
            }
        }

        public override bool CanAdd(Character character)
        {
            return !character.IsPlayer();
        }
    }
}

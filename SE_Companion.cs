using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_Companion : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_Companion")]
        public static float m_baseTTL = 600f;
        public float speedModifier = 1.2f;
        public float healthRegen = 1f;
        public float damageModifier = 1f;
        private float m_timer = 0f;
        private float m_interval = 5f;
        public Player summoner;

        public SE_Companion()
        {
            base.name = "SE_VL_Companion";
            m_icon = AbilityIcon;
            m_tooltip = "Companion";
            m_name = "Companion";
            m_ttl = m_baseTTL;          
        }

        public override void ModifySpeed(ref float speed)
        {
            speed *= speedModifier;
        }

        public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);
            m_timer -= dt;
            if (m_timer <= 0f)
            {
                m_timer = m_interval;
                m_character.Heal(healthRegen, true);
            }
        }

        public override bool IsDone()
        {
            if(m_ttl > 0f && m_time > m_ttl)
            {
                ZLog.Log("killing " + m_character.m_name);
                HitData hit = new HitData();
                hit.m_damage.m_spirit = 99999f;
                base.m_character.ApplyDamage(hit, true, true, HitData.DamageModifier.VeryWeak);

            }
            return base.IsDone();
        }

        public override bool CanAdd(Character character)
        {
            return !character.IsPlayer();
        }
    }
}

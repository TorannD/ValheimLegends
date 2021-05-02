using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_Ranger : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_Ranger")]
        public static float m_baseTTL = 2f;
        private float m_timer = 0f;
        public float hitCount = 0f;
        private float m_interval = 1f;
        private int maxHitCount = 5;

        public SE_Ranger()
        {
            base.name = "SE_VL_Ranger";
            m_icon = AbilityIcon;
            m_tooltip = "Ranger";
            m_name = "Ranger";
            m_ttl = m_baseTTL;
        }

        public override void ModifySpeed(ref float speed)
        {
            if(hitCount > 0)
            {
                speed *= 2f;
            }
            base.ModifySpeed(ref speed);
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {
            hit.m_damage.m_poison *= .75f;
            base.OnDamaged(hit, attacker);
        }

        public override void ModifyRunStaminaDrain(float baseDrain, ref float drain)
        {
            drain *= .75f;
            base.ModifyRunStaminaDrain(baseDrain, ref drain);
        }

        public override void UpdateStatusEffect(float dt)
        {
            m_timer -= dt;
            if (m_timer <= 0f)
            {
                m_timer = m_interval;
                hitCount--;
                hitCount = Mathf.Clamp(hitCount, 0, maxHitCount);
            }
            m_ttl = hitCount;
            m_time = 0;
            
            base.UpdateStatusEffect(dt);
        }

        public override bool IsDone()
        {
            return ValheimLegends.vl_player.vl_class != ValheimLegends.PlayerClass.Ranger;
        }

        public override bool CanAdd(Character character)
        {
            return character.IsPlayer() && ValheimLegends.vl_player.vl_class == ValheimLegends.PlayerClass.Ranger;
        }
    }
}

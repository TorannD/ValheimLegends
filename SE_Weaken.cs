using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_Weaken : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_Weaken")]
        public static float m_baseTTL = 30f;
        public float interval = 1f;
        public float speedReduction = .8f;
        public float damageReduction = .25f;
        public float staminaDrain = .1f;
        private float m_timer = 1f;

        public SE_Weaken()
        {
            base.name = "SE_VL_Weaken";
            m_icon = AbilityIcon;
            m_tooltip = "Weaken";
            m_name = "Weaken";
            m_ttl = m_baseTTL;
        }

        public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);
            m_timer -= dt;
            if (m_timer <= 0f)
            {
                m_timer = interval;                
                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_WeakenStatus"), m_character.GetEyePoint(), Quaternion.identity);
            }
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {
            if(attacker.IsPlayer())
            {
                attacker.AddStamina(5f + hit.GetTotalDamage() * staminaDrain);
            }
            base.OnDamaged(hit, attacker);
        }

        public override void ModifySpeed(ref float speed)
        {
            speed *= speedReduction;
            base.ModifySpeed(ref speed);
        }

        public override bool CanAdd(Character character)
        {
            return true;
        }
    }
}

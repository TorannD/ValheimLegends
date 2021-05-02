using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_SeedRegeneration : StatusEffect
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_SeedRegeneration")]
        public float m_damageInterval = 1f;
        public static float m_baseTTL = 6f;
        public float m_HealAmount = 5f;
        private float m_timer = 0f;
        public bool doOnce = true;

        public SE_SeedRegeneration()
        {
            base.name = "SE_VL_SeedRegeneration";
            m_icon = AbilityIcon;
            m_tooltip = "SeedRegeneration";
            m_name = "SeedRegeneration";
            m_activationAnimation = "vfx_Potion_health_medium";
            m_ttl = m_baseTTL;
            doOnce = true;
        }

        public override void UpdateStatusEffect(float dt)
        {
            m_timer -= dt;
            if (m_timer <= 0f)
            {
                m_timer = m_damageInterval;
                m_character.AddStamina(m_HealAmount);
                //GO_SEFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_Potion_health_medium"), m_character.GetCenterPoint(), Quaternion.identity);
            }
            base.UpdateStatusEffect(dt);
        }

        public override bool CanAdd(Character character)
        {
            return character.IsPlayer();
        }
    }
}

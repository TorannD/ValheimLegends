using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_BiomeMeadows : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_BiomeMeadows")]
        public static float m_baseTTL = 600f;
        public float regenBonus = 1f;
        private float m_timer = 0f;
        private float m_interval = 5f;
        public bool doOnce = true;

        public SE_BiomeMeadows()
        {
            base.name = "SE_VL_BiomeMeadows";
            m_icon = AbilityIcon;
            m_tooltip = "Biome Meadows Buff";
            m_name = "VL_BiomeMeadows";
            m_ttl = m_baseTTL;
            doOnce = true;
        }

        public override void UpdateStatusEffect(float dt)
        {
            if (doOnce)
            {
                doOnce = false;
                //ZLog.Log("setting up shell, level is " + m_character.GetLevel());
                float sLevel = m_character.GetSkills().GetTotalSkill() / (float)m_character.GetSkills().GetSkillList().Count;
                m_ttl = m_baseTTL + (3 * sLevel);
                regenBonus = (1f + (.1f * sLevel)) * VL_GlobalConfigs.g_DamageModifer;
            }
            m_timer -= dt;
            if (m_timer <= 0f)
            {
                m_timer = m_interval;
                m_character.Heal(regenBonus, true);
                m_character.AddStamina(regenBonus *2);
            }
            base.UpdateStatusEffect(dt);
        }

        public override bool CanAdd(Character character)
        {
            return true;
        }
    }
}

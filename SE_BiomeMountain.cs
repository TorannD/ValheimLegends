using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_BiomeMountain : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_BiomeMountain")]
        public static float m_baseTTL = 600f;
        public float resistModifier = .8f;
        public float staminaRegen = 5f;
        private float m_timer = 0f;
        private float m_interval = 5f;
        public bool doOnce = true;

        public SE_BiomeMountain()
        {
            base.name = "SE_VL_BiomeMountain";
            m_icon = AbilityIcon;
            m_tooltip = "BiomeMountain";
            m_name = "VL_BiomeMountain";
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
                m_ttl = m_baseTTL + (3f * sLevel);
                staminaRegen = (5f + (.075f * sLevel));
                resistModifier = .8f - (.002f * sLevel);
            }
            m_timer -= dt;
            if (m_timer <= 0f)
            {
                m_timer = m_interval;
                m_character.AddStamina(staminaRegen);
            }
            base.UpdateStatusEffect(dt);
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {
            hit.m_damage.m_frost *= resistModifier;
            base.OnDamaged(hit, attacker);
        }

        public override bool CanAdd(Character character)
        {
            return true;
        }
    }
}

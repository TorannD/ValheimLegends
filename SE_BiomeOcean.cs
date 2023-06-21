using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_BiomeOcean : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_BiomeOcean")]
        public static float m_baseTTL = 600f;
        public float resistModifier = .8f;
        public float swimSpeed = 1.5f;
        public bool doOnce = true;

        public SE_BiomeOcean()
        {
            base.name = "SE_VL_BiomeOcean";
            m_icon = AbilityIcon;
            m_tooltip = "BiomeOcean";
            m_name = "VL_BiomeOcean";
            m_ttl = m_baseTTL;
            doOnce = true;
        }

        public override void ModifySpeed(float baseSpeed, ref float speed)
        {
            if (m_character.IsSwimming())
            {
                speed *= swimSpeed;
            }
            base.ModifySpeed(baseSpeed, ref speed);
        }

        public override void UpdateStatusEffect(float dt)
        {
            if (doOnce)
            {
                doOnce = false;
                //ZLog.Log("setting up shell, level is " + m_character.GetLevel());
                float sLevel = m_character.GetSkills().GetTotalSkill() / (float)m_character.GetSkills().GetSkillList().Count;
                m_ttl = m_baseTTL + (3f * sLevel);
                swimSpeed = 1.5f + (.01f * sLevel);
                resistModifier = .8f - (.002f * sLevel);
            }
            base.UpdateStatusEffect(dt);
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {            
            hit.m_damage.m_lightning *= resistModifier;
            base.OnDamaged(hit, attacker);
        }

        public override bool CanAdd(Character character)
        {
            return true;
        }
    }
}

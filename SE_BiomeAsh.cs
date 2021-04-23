using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_BiomeAsh : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_BiomeAsh")]
        public static float m_baseTTL = 600f;
        public float resistModifier = .8f;
        public float fireDamageOffset = 26f;
        public bool doOnce = true;

        public SE_BiomeAsh()
        {
            base.name = "SE_VL_BiomeAsh";
            m_icon = AbilityIcon;
            m_tooltip = "BiomeAsh";
            m_name = "VL_BiomeAsh";
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
                m_ttl = m_baseTTL + (5f * sLevel);
                fireDamageOffset = (26f + (.4f * sLevel)) * VL_GlobalConfigs.g_DamageModifer;
                resistModifier = .8f - (.002f * sLevel);
            }
            base.UpdateStatusEffect(dt);
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {            
            hit.m_damage.m_fire *= resistModifier;
            hit.m_damage.m_poison *= resistModifier;
            base.OnDamaged(hit, attacker);
        }

        public override bool CanAdd(Character character)
        {
            return true;
        }
    }
}

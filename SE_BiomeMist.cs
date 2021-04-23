using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_BiomeMist : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_BiomeMist")]
        public static float m_baseTTL = 600f;
        public float resistModifier = .8f;
        public float iceDamageOffset = 20f;
        public bool doOnce = true;

        public SE_BiomeMist()
        {
            base.name = "SE_VL_BiomeMist";
            m_icon = AbilityIcon;
            m_tooltip = "BiomeMist";
            m_name = "VL_BiomeMist";
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
                iceDamageOffset = (20f + (.3f * sLevel)) * VL_GlobalConfigs.g_DamageModifer;
                resistModifier = .8f - (.002f * sLevel);
            }
            base.UpdateStatusEffect(dt);
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {
            hit.m_damage.m_frost *= resistModifier;
            hit.m_damage.m_spirit *= resistModifier;
            base.OnDamaged(hit, attacker);            
        }

        public override bool CanAdd(Character character)
        {
            return true;
        }
    }
}

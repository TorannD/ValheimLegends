using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_BiomePlains : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_BiomePlains")]
        public static float m_baseTTL = 600f;
        public float resistModifier = .8f;
        public float speedBonus = 1.1f;
        public bool doOnce = true;

        public SE_BiomePlains()
        {
            base.name = "SE_VL_BiomePlains";
            m_icon = AbilityIcon;
            m_tooltip = "BiomePlains";
            m_name = "VL_BiomePlains";
            m_ttl = m_baseTTL;
            doOnce = true;
        }

        public override void ModifySpeed(float baseSpeed, ref float speed)
        {
            speed *= speedBonus;
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
                speedBonus = (1.1f + (.001f * sLevel));
                resistModifier = .8f - (.002f * sLevel);
            }
            base.UpdateStatusEffect(dt);
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {            
            hit.m_damage.m_fire *= resistModifier;
            base.OnDamaged(hit, attacker);
        }

        public override bool CanAdd(Character character)
        {
            return true;
        }
    }
}

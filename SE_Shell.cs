using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_Shell : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_Shell")]
        public static float m_baseTTL = 35f;
        public float resistModifier = .6f;
        public float spiritDamageOffset = 10f;
        public bool doOnce = true;

        public SE_Shell()
        {
            base.name = "SE_VL_Shell";
            m_icon = AbilityIcon;
            m_tooltip = "Shell";
            m_name = "Shell";
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
                m_ttl = m_baseTTL + (.35f * sLevel);
                spiritDamageOffset = 10f + (1.5f * sLevel);
                resistModifier = .6f - (.006f * sLevel);
            }
            base.UpdateStatusEffect(dt);
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {
            base.OnDamaged(hit, attacker);
            hit.m_damage.m_fire *= resistModifier;
            hit.m_damage.m_frost *= resistModifier;
            hit.m_damage.m_lightning *= resistModifier;
            hit.m_damage.m_poison *= resistModifier;
            hit.m_damage.m_spirit *= resistModifier;
        }

        public override bool CanAdd(Character character)
        {
            return true;
        }
    }
}

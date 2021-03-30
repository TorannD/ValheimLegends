using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_ShadowStalk : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_ShadowStalk")]
        public static float m_baseTTL = 30f;
        public float speedDuration = 3f;
        public float speedAmount = 1.75f;
        private float m_timer = 1f;

        public SE_ShadowStalk()
        {
            base.name = "SE_VL_ShadowStalk";
            m_icon = AbilityIcon;
            m_tooltip = "ShadowStalk";
            m_name = "ShadowStalk";
            m_ttl = m_baseTTL;
            m_stealthModifier = .1f;
            m_noiseModifier = .1f;
            m_raiseSkill = Skills.SkillType.Sneak;
            m_raiseSkillModifier = 2f;
        }

        public override void ModifySpeed(ref float speed)
        {
            if(m_character.IsSneaking())
            {
                speed *= 1.5f + (.01f * m_character.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AlterationSkillDef).m_level);
            }
            else if(speedDuration > 0)
            {
                speed *= speedAmount;
            }
            base.ModifySpeed(ref speed);
        }

        public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);
            m_timer -= dt;
            if (m_timer <= 0f)
            {
                m_timer = 1f;
                speedDuration--;
            }

        }

        public override bool CanAdd(Character character)
        {
            return character.IsPlayer();
        }
    }
}

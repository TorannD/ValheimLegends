using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_Monk : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_Monk")]
        public static float m_baseTTL = 5f;
        private float m_timer = 0f;
        public int hitCount = 0;
        private float m_interval = 12f;
        private int maxHitCount = 5;

        public SE_Monk()
        {
            base.name = "SE_VL_Monk";
            m_icon = AbilityIcon;
            m_tooltip = "Monk";
            m_name = "Monk";
            m_ttl = m_baseTTL;
        }

        public override void ModifySpeed(ref float speed)
        {

            base.ModifySpeed(ref speed);
        }

        public override void UpdateStatusEffect(float dt)
        {
            m_timer -= dt;
            if (m_timer <= 0f)
            {
                maxHitCount = 10 + Mathf.RoundToInt(m_character.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level * .2f);
                m_timer = m_interval;
                hitCount--;
                hitCount = Mathf.Clamp(hitCount, 0, maxHitCount);
            }
            m_ttl = hitCount;
            m_time = 0;
            
            base.UpdateStatusEffect(dt);
        }

        public override bool IsDone()
        {
            return ValheimLegends.vl_player.vl_class != ValheimLegends.PlayerClass.Monk;
        }

        public override bool CanAdd(Character character)
        {
            return character.IsPlayer() && ValheimLegends.vl_player.vl_class == ValheimLegends.PlayerClass.Monk;
        }
    }
}

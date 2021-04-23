using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_BiomeBlackForest : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_BiomeBlackForest")]
        public static float m_baseTTL = 600f;
        public float carryModifier = 50;
        public bool underRoof = true;
        public bool doOnce = true;

        public SE_BiomeBlackForest()
        {
            base.name = "SE_VL_BiomeBlackForest";
            m_icon = AbilityIcon;
            m_tooltip = "BiomeBlackForest";
            m_name = "VL_BiomeBlackForest";
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
                carryModifier = 50 + sLevel;
            }
            base.UpdateStatusEffect(dt);
        }

        public override bool CanAdd(Character character)
        {
            return true;
        }
    }
}

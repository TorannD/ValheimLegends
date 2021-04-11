using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_Enrage : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_Enrage")]
        public static float m_baseTTL = 20f;
        public float speedModifier = 1.25f;
        private float m_timer = 0f;
        private float m_interval = 1f;
        public float staminaModifier = 10f;
        public bool doOnce = true;

        public SE_Enrage()
        {
            base.name = "SE_VL_Enrage";
            m_icon = AbilityIcon;
            m_tooltip = "Enrage";
            m_name = "Enrage";
            m_ttl = m_baseTTL;
            doOnce = true;
        }

        public override void ModifySpeed(ref float speed)
        {
            speed *= speedModifier;
        }

        public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);
            if(doOnce)
            {
                doOnce = false;
                float sLevel = m_character.GetSkills().GetTotalSkill() / (float)m_character.GetSkills().GetSkillList().Count;
                m_ttl = 20f + (.2f * sLevel);
                staminaModifier = 5f + (.1f * sLevel);
                speedModifier = 1.25f + (.0025f * sLevel);  //1.4f
            }
            m_timer -= dt;
            if (m_timer <= 0f)
            {
                m_timer = m_interval;
                m_character.AddStamina(staminaModifier);
                //GO_SEFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_Potion_health_medium"), m_character.GetCenterPoint(), Quaternion.identity);
            }
        }

        public override bool CanAdd(Character character)
        {
            return true;
        }
    }
}

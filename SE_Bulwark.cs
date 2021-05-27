using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_Bulwark : StatusEffect
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_Bulwark")]
        public static float m_baseTTL = 12f;
        private float m_timer = 0f;

        public SE_Bulwark()
        {
            base.name = "SE_VL_Bulwark";
            m_icon = AbilityIcon;
            m_tooltip = "Bulwark";
            m_name = "Bulwark";
            m_ttl = m_baseTTL;
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {
            float modifier = .75f - (m_character.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AbjurationSkillDef).m_level / 200f);
            hit.m_damage.Modify(modifier * VL_GlobalConfigs.c_valkyrieBulwark);
            base.OnDamaged(hit, attacker);
        }

        public override bool CanAdd(Character character)
        {
            return character.IsPlayer();
        }
    }
}

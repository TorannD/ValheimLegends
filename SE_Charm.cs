using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_Charm : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_Charm")]
        public static float m_baseTTL = 30f;
        public float speedModifier = 1.2f;
        public float healthRegen = 1f;
        public float damageModifier = 1f;
        private float m_timer = 0f;
        private float m_interval = 1f;
        public Player summoner;
        public Character.Faction originalFaction;

        public SE_Charm()
        {
            base.name = "SE_VL_Charm";
            m_icon = AbilityIcon;
            m_tooltip = "Charm";
            m_name = "VL_Charm";
            m_ttl = m_baseTTL;          
        }

        public override void UpdateStatusEffect(float dt)
        {            
            m_timer -= dt;
            if (m_timer <= 0f)
            {
                m_timer = m_interval;
                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_boar_pet"), m_character.GetEyePoint(), Quaternion.identity);
                if(GetRemaningTime() <= m_interval)
                {
                    m_character.m_faction = originalFaction;
                }
            }
            base.UpdateStatusEffect(dt);
        }

        public override bool CanAdd(Character character)
        {
            return !character.IsPlayer();
        }
    }
}

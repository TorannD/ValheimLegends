using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_BiomeSwamp : SE_Stats
    {
        public static Sprite AbilityIcon;
        public static GameObject GO_SEFX;

        [Header("SE_VL_BiomeSwamp")]
        public static float m_baseTTL = 600f;
        public float resistModifier = .8f;
        public bool doOnce = true;
        private float m_timer = 0f;
        private float m_interval = 3f;
        public bool doLight = true;
        public Light biomeLight;

        public SE_BiomeSwamp()
        {
            base.name = "SE_VL_BiomeSwamp";
            m_icon = AbilityIcon;
            m_tooltip = "BiomeSwamp";
            m_name = "VL_BiomeSwamp";
            m_ttl = m_baseTTL;
            doOnce = true;
            doLight = true;
        }

        public override void UpdateStatusEffect(float dt)
        {
            if (doOnce)
            {
                doOnce = false;
                float sLevel = m_character.GetSkills().GetTotalSkill() / (float)m_character.GetSkills().GetSkillList().Count;
                m_ttl = m_baseTTL + (3f * sLevel);
                resistModifier = .8f - (.002f * sLevel);
            }
            if(doLight)
            {
                doLight = false;
                m_character.gameObject.AddComponent<Light>();
                m_character.GetComponent<Light>().range = 30;
                m_character.GetComponent<Light>().color = new Color(233, 240, 226);
                m_character.GetComponent<Light>().intensity = .0035f;
                m_character.GetComponent<Light>().enabled = true;
                biomeLight = m_character.GetComponent<Light>();
            }
            m_timer -= dt;
            if (m_timer <= 0f)
            {
                m_timer = m_interval;
                //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_boar_pet"), m_character.GetEyePoint(), Quaternion.identity);
                if (GetRemaningTime() <= m_interval)
                {
                    if(biomeLight != null)
                    {
                        UnityEngine.Object.Destroy(biomeLight);
                    }
                   
                }
            }
            base.UpdateStatusEffect(dt);
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {
            hit.m_damage.m_poison *= resistModifier;
            base.OnDamaged(hit, attacker);
        }

        public override bool CanAdd(Character character)
        {
            return true;
        }
    }
}

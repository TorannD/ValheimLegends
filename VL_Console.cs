using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public static class VL_Console
    {
        public static bool CheatRaiseSkill(Skills skill_instance, string name, float value, Player player)
        {
            foreach(Skills.SkillDef skill in ValheimLegends.legendsSkills)
            {
                if (skill.m_skill.ToString() == "781" && name.ToLower() == "discipline")
                {                    
                    player.RaiseSkill(skill.m_skill, value);
                    return true;
                }
                else if (skill.m_skill.ToString() == "791" && name.ToLower() == "abjuration")
                {
                    player.RaiseSkill(skill.m_skill, value);
                    return true;
                }
                else if (skill.m_skill.ToString() == "792" && name.ToLower() == "alteration")
                {
                    player.RaiseSkill(skill.m_skill, value);
                    return true;
                }
                else if (skill.m_skill.ToString() == "793" && name.ToLower() == "conjuration")
                {
                    player.RaiseSkill(skill.m_skill, value);
                    return true;
                }
                else if (skill.m_skill.ToString() == "794" && name.ToLower() == "evocation")
                {
                    player.RaiseSkill(skill.m_skill, value);
                    return true;
                }
                else if (skill.m_skill.ToString() == "795" && name.ToLower() == "illusion")
                {
                    player.RaiseSkill(skill.m_skill, value);
                    return true;
                }
            }
            return false;
        }

        public static void CheatChangeClass(string className)
        {
            bool flag = false;
            if (className.ToLower() == "berserker")
            {
                ValheimLegends.vl_player.vl_class = ValheimLegends.PlayerClass.Berserker;                
                flag = true;
            }
            else if (className.ToLower() == "druid")
            {
                ValheimLegends.vl_player.vl_class = ValheimLegends.PlayerClass.Druid;
                flag = true;
            }
            else if (className.ToLower() == "mage")
            {
                ValheimLegends.vl_player.vl_class = ValheimLegends.PlayerClass.Mage;
                flag = true;
            }
            else if (className.ToLower() == "priest")
            {
                ValheimLegends.vl_player.vl_class = ValheimLegends.PlayerClass.Priest;
                flag = true;
            }
            //else if (className.ToLower() == "necromancer")
            //{
            //    ValheimLegends.vl_player.vl_class = ValheimLegends.PlayerClass.Necromancer;
            //    flag = true;
            //}
            else if (className.ToLower() == "monk")
            {
                ValheimLegends.vl_player.vl_class = ValheimLegends.PlayerClass.Monk;
                flag = true;
            }
            else if (className.ToLower() == "duelist")
            {
                ValheimLegends.vl_player.vl_class = ValheimLegends.PlayerClass.Duelist;
                flag = true;
            }
            else if (className.ToLower() == "enchanter")
            {
                ValheimLegends.vl_player.vl_class = ValheimLegends.PlayerClass.Enchanter;
                flag = true;
            }
            else if (className.ToLower() == "rogue")
            {
                ValheimLegends.vl_player.vl_class = ValheimLegends.PlayerClass.Rogue;
                flag = true;
            }
            else if (className.ToLower() == "ranger")
            {
                ValheimLegends.vl_player.vl_class = ValheimLegends.PlayerClass.Ranger;
                flag = true;
            }
            else if (className.ToLower() == "shaman")
            {
                ValheimLegends.vl_player.vl_class = ValheimLegends.PlayerClass.Shaman;
                flag = true;
            }
            else if (className.ToLower() == "valkyrie")
            {
                ValheimLegends.vl_player.vl_class = ValheimLegends.PlayerClass.Valkyrie;
                flag = true;
            }
            else if (className.ToLower() == "metavoker")
            {
                ValheimLegends.vl_player.vl_class = ValheimLegends.PlayerClass.Metavoker;
                flag = true;
            }
            else if (className.ToLower() == "none")
            {
                ValheimLegends.vl_player.vl_class = ValheimLegends.PlayerClass.None;
                flag = true;
            }
            if (flag)
            {

                Console.instance.Print("Class changed to " + className);

                ValheimLegends.UpdateVLPlayer(Player.m_localPlayer);
                ValheimLegends.NameCooldowns();
                if (ValheimLegends.abilitiesStatus != null)
                {
                    foreach (RectTransform ability in ValheimLegends.abilitiesStatus)
                    {
                        if (ability.gameObject != null)
                        {
                            UnityEngine.Object.Destroy(ability.gameObject);
                        }
                    }
                    ValheimLegends.abilitiesStatus.Clear();
                }
            }
        }
    }
}

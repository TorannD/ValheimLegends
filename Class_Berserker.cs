using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace ValheimLegends
{
    public class Class_Berserker
    {
        private static int Script_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock");        

        private static GameObject GO_CastFX;

        public static void Execute_Dash(Player player, ref float altitude)
        {
            UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_perfectblock"), player.transform.position, Quaternion.identity);
            UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_stonegolem_attack_hit"), player.transform.position, Quaternion.identity);

            float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;
            float sDamageMultiplier = .8f + (sLevel * .005f);

            RaycastHit hitInfo = default(RaycastHit);
            Vector3 vector = player.GetLookDir();
            vector.y = 0f;
            Vector3 hitVec = default(Vector3);
            player.transform.rotation = Quaternion.LookRotation(vector);
            vector = player.transform.forward;
            Vector3 moveVec= player.transform.position;
            Vector3 yVec = player.transform.position;
            yVec.y += 0.1f;
            List<int> list = new List<int>();
            int i = 0;
            for (; i <= 10; i++)
            {
                bool flag = false;
                for (int j = 0; j <= 10; j++)
                {
                    Vector3 _v = Vector3.MoveTowards(player.transform.position, player.transform.position + vector * 100f, (float)((float)i + (float)j * 0.1f));
                    _v.y = yVec.y;
                    if (_v.y < ZoneSystem.instance.GetGroundHeight(_v))
                    {
                        yVec.y = ZoneSystem.instance.GetGroundHeight(_v) + 1f;
                        _v.y = yVec.y;
                    }
                    flag = Physics.SphereCast(_v, 0.05f, vector, out hitInfo, float.PositiveInfinity, ~Script_Layermask);
                    if (flag && (bool)hitInfo.collider)
                    {
                        hitVec = hitInfo.point;
                        break;
                    }
                }
                moveVec= Vector3.MoveTowards(player.transform.position, player.transform.position + vector * 100f, (float)i);
                moveVec.y = ((ZoneSystem.instance.GetSolidHeight(moveVec) - ZoneSystem.instance.GetGroundHeight(moveVec) <= 1f) ? ZoneSystem.instance.GetSolidHeight(moveVec) : ZoneSystem.instance.GetGroundHeight(moveVec));
                //GameObject go_dasheffects = ZNetScene.instance.GetPrefab("vfx_stonegolem_attack_hit");
                //go_dasheffects.transform.localScale = Vector3.one * .5f;
                if (flag && Vector3.Distance(new Vector3(moveVec.x, yVec.y, moveVec.z), hitVec) <= 1.5f)
                {
                    yVec = Vector3.MoveTowards(hitVec, yVec, 1f);
                    //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_stonegolem_attack_hit"), moveVec, Quaternion.identity);
                    //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_barley_destroyed"), moveVec, Quaternion.identity);
                    UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_beehive_hit"), yVec, Quaternion.identity);
                    //UnityEngine.Object.Instantiate(go_dasheffects, moveVec, Quaternion.identity);
                    break;
                }
                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_beehive_hit"), yVec, Quaternion.identity);
                yVec = new Vector3(moveVec.x, yVec.y, moveVec.z);
                foreach (Character ch in Character.GetAllCharacters())
                {
                    HitData hitData = new HitData();                    
                    hitData.m_damage = player.GetCurrentWeapon().GetDamage();
                    hitData.ApplyModifier(UnityEngine.Random.Range(.8f, 1.2f) * sDamageMultiplier);
                    hitData.m_point = ch.GetCenterPoint();
                    hitData.m_dir = (ch.transform.position - moveVec);
                    float num = Vector3.Distance(ch.transform.position, moveVec);
                    if (!ch.IsPlayer() && num <= 3f && !list.Contains(ch.GetInstanceID()))
                    {
                        ch.ApplyDamage(hitData, true, true, HitData.DamageModifier.Normal);
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_crit"), ch.GetCenterPoint(), Quaternion.identity);
                        list.Add(ch.GetInstanceID());
                    }
                }
            }
            list.Clear();
            if (i > 10 && ZoneSystem.instance.GetSolidHeight(yVec) - yVec.y <= 2f)
            {
                yVec.y = ZoneSystem.instance.GetSolidHeight(yVec);
            }
            player.transform.position = yVec;
            altitude = 0f;
            player.transform.rotation = Quaternion.LookRotation(vector);
        }

        public static void Process_Input(Player player, ref float altitude)
        {
            System.Random rnd = new System.Random();
            Vector3 pVec = default(Vector3);
            if (Input.GetKeyDown(ValheimLegends.Ability3_Hotkey.Value.ToLower()))
            {
                //dash forward and hit enemies along the way
                //player.Message(MessageHud.MessageType.Center, "Dash");
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
                {
                    if (player.GetStamina() > VL_Utility.GetDashCost(player))
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability3_CD)ScriptableObject.CreateInstance(typeof(SE_Ability3_CD));
                        se_cd.m_ttl = VL_Utility.GetDashCooldown(player);
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetDashCost(player));

                        //Effects, animations, and sounds
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("swing_longsword2");                        
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_Potion_stamina_medium"), player.transform.position, Quaternion.identity);

                        //Lingering effects

                        //Apply effects
                        ValheimLegends.isChargingDash = true;
                        ValheimLegends.dashCounter = 0;

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetDashSkillGain(player));
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Dash: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetDashCost(player) + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
            else if(Input.GetKeyDown(ValheimLegends.Ability2_Hotkey.Value.ToLower()))
            {
                //enters unique enraged state (take periodic damage, no delay to stamina regen, absorb life on hit, bonus damage, increased move speed)
                //player.Message(MessageHud.MessageType.Center, "Berserk Rage");
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability2_CD"))
                {
                    if (player.GetStamina() > VL_Utility.GetBerserkCost(player))
                    {
                        
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability2_CD)ScriptableObject.CreateInstance(typeof(SE_Ability2_CD));
                        se_cd.m_ttl = VL_Utility.GetBerserkCooldown(player);
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetBerserkCost(player));

                        //Effects, animations, and sounds
                        ValheimLegends.shouldUseGuardianPower = false;
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("gpower");
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetSpeed(2f);
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_GP_Stone"), player.GetCenterPoint(), Quaternion.identity);

                        //Lingering effects
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AlterationSkillDef).m_level;
                        SE_Berserk se_berserk = (SE_Berserk)ScriptableObject.CreateInstance(typeof(SE_Berserk));
                        se_berserk.m_ttl = SE_Berserk.m_baseTTL;
                        se_berserk.speedModifier = 1.2f + (.004f * sLevel);
                        se_berserk.damageModifier = 1.2f + (.003f * sLevel);
                        se_berserk.healthAbsorbPercent = .2f + (.002f * sLevel);

                        //Apply effects
                        player.GetSEMan().AddStatusEffect(se_berserk);

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.AlterationSkill, VL_Utility.GetBerserkSkillGain(player));
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Berserk: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetBerserkCost(player) + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
            else if (Input.GetKeyDown(ValheimLegends.Ability1_Hotkey.Value.ToLower()))
            {
                //next attack is a garunteed crit
                //player.Message(MessageHud.MessageType.Center, "Execute");
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability1_CD"))
                {
                    if (player.GetStamina() > VL_Utility.GetExecuteCost(player))
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability1_CD)ScriptableObject.CreateInstance(typeof(SE_Ability1_CD));
                        se_cd.m_ttl = VL_Utility.GetExecuteCooldown(player);
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetExecuteCost(player));

                        //Effects, animations, and sounds
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("cheer");
                        player.StartEmote("point");
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_backstab"), player.GetCenterPoint(), Quaternion.identity);

                        //Lingering effects
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;
                        SE_Execute se_execute = (SE_Execute)ScriptableObject.CreateInstance(typeof(SE_Execute));
                        se_execute.hitCount = Mathf.RoundToInt(3f + (.04f * sLevel));
                        se_execute.damageBonus = 1.4f + (.005f * sLevel);
                        se_execute.staggerForce = 1.5f + (.005f * sLevel);

                        //Apply effects
                        player.GetSEMan().AddStatusEffect(se_execute);

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetExecuteSkillGain(player));
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Execute: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetExecuteCost(player) + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
        }
    }
}

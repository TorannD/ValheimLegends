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
    public class Class_Valkyrie
    {
        private static int Script_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock");        

        private static GameObject GO_CastFX;

        public static bool inFlight = false;
        public static bool isBlocking = false;

        public static void Process_Input(Player player)
        {
            System.Random rnd = new System.Random();
            Vector3 pVec = default(Vector3);
            
            if (VL_Utility.Ability3_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
                {
                    if (player.GetStamina() >= VL_Utility.GetLeapCost)
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability3_CD)ScriptableObject.CreateInstance(typeof(SE_Ability3_CD));
                        se_cd.m_ttl = VL_Utility.GetLeapCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetLeapCost);

                        //Effects, animations, and sounds
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("jump");

                        //Apply effects
                        Vector3 velVec = player.GetVelocity();
                        Rigidbody playerBody = Traverse.Create(root: player).Field(name: "m_body").GetValue<Rigidbody>();
                        //ZLog.Log("player velocity is " + playerBody.velocity + " body velocity is " + playerBody.velocity);
                        inFlight = true;
                        Vector3 playerHorVec = Vector3.zero;
                        playerHorVec.z = playerBody.velocity.z;
                        playerHorVec.x = playerBody.velocity.x;
                        playerBody.velocity = (velVec * 2f) + new Vector3(0, 15f, 0f) + (playerHorVec * 3f);
                        playerBody.velocity *= (.8f + player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level * .005f);
                        //ZLog.Log("player look vector is " + player.GetLookDir() + " player hor vec is " + playerHorVec);
                        GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_perfectblock"), player.transform.position, Quaternion.identity);
                        GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_perfectblock"), player.transform.position, Quaternion.identity);
                        //ZLog.Log("adjusted velocity is " + playerBody.velocity);

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetLeapSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Leap: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetLeapCost + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
            else if(VL_Utility.Ability2_Input_Down)
            {
                //player.Message(MessageHud.MessageType.Center, "Stagger");
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability2_CD"))
                {
                    if (player.GetStamina() >= VL_Utility.GetStaggerCost)
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability2_CD)ScriptableObject.CreateInstance(typeof(SE_Ability2_CD));
                        se_cd.m_ttl = VL_Utility.GetStaggerCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetStaggerCost);

                        //Effects, animations, and sounds
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("battleaxe_attack1");
                        GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_troll_rock_destroyed"), player.transform.position, Quaternion.identity);
                        GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_sledge_iron_hit"), player.transform.position, Quaternion.identity);

                        //Lingering effects

                        //Apply effects
                        List<Character> allCharacters = Character.GetAllCharacters();
                        foreach (Character ch in allCharacters)
                        {
                            if ((BaseAI.IsEnemy(player, ch) && (ch.transform.position - player.transform.position).magnitude <= 6f))
                            {
                                Vector3 direction = (ch.transform.position - player.transform.position);
                                ch.Stagger(direction);
                            }
                        }

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetStaggerSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to Stagger: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetStaggerCost + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
            else if (VL_Utility.Ability1_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability1_CD"))
                {                    
                    
                    //player.Message(MessageHud.MessageType.Center, "Bulwark");
                    if (player.GetStamina() >= VL_Utility.GetBulwarkCost)
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability1_CD)ScriptableObject.CreateInstance(typeof(SE_Ability1_CD));
                        se_cd.m_ttl = VL_Utility.GetBulwarkCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetBulwarkCost);

                        //Effects, animations, and sounds
                        ValheimLegends.shouldUseGuardianPower = false;
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("gpower");
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetSpeed(1.8f);
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_guardstone_deactivate"), player.GetCenterPoint(), Quaternion.identity);
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_metal_blocked"), player.transform.position, Quaternion.identity);

                        //Lingering effects
                        StatusEffect SE_bulwark = (SE_Bulwark)ScriptableObject.CreateInstance(typeof(SE_Bulwark));
                        SE_bulwark.m_ttl = SE_Bulwark.m_baseTTL + Mathf.RoundToInt(player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AbjurationSkillDef).m_level * .2f);
                        player.GetSEMan().AddStatusEffect(SE_bulwark);

                        //Apply effects

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.AbjurationSkill, VL_Utility.GetBulwarkSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Bulwark: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetBulwarkCost + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
        }

        public static void Impact_Effect(Player player, float altitude)
        {
            List<Character> allCharacters = Character.GetAllCharacters();
            //player.Message(MessageHud.MessageType.Center, "valkyrie impact");
            inFlight = false;
            foreach (Character ch in allCharacters)
            {
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;
                if (BaseAI.IsEnemy(player, ch) && (ch.transform.position - player.transform.position).magnitude <= (8f + (.05f * sLevel)))
                {
                    Vector3 direction = (ch.transform.position - player.transform.position);
                    HitData hitData = new HitData();
                    hitData.m_damage.m_blunt = 5 + (3f * altitude) + 2f * UnityEngine.Random.Range(sLevel, 3f * sLevel) * ValheimLegends.abilityDamageMultiplier.Value;
                    hitData.m_pushForce = 20f * ValheimLegends.abilityDamageMultiplier.Value;
                    hitData.m_point = ch.GetEyePoint();
                    hitData.m_dir = (player.transform.position - ch.transform.position);
                    ch.ApplyDamage(hitData, true, true, HitData.DamageModifier.Normal);                    
                    ch.Stagger(direction);
                }
            }
            ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).StopAllCoroutines();
            ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("battleaxe_attack2");
            GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_gdking_stomp"), player.transform.position, Quaternion.identity);
        }
    }
}

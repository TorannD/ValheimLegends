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
using Unity;

namespace ValheimLegends
{
    public class Class_Rogue
    {
        private static int ScriptChar_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "piece", "terrain", "vehicle", "viewblock", "character", "character_noenv", "character_trigger", "Water");
        public static Vector3 fadePoint;
        public static Vector3 backstabPoint;
        public static Vector3 backstabVector;
        public static bool throwDagger = false;
        public static bool canDoubleJump = true;
        public static bool canGainTrick = false;

        public static bool PlayerUsingDaggerOnly
        {
            get
            {
                Player p = Player.m_localPlayer;
                if (p.GetCurrentWeapon() != null)
                {
                    ItemDrop.ItemData.SharedData sid = p.GetCurrentWeapon().m_shared;
                    if (sid != null && (sid.m_name.ToLower().Contains("knife") || sid.m_name.Contains("dagger")) && p.GetLeftItem() == null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static void Execute_Throw(Player player)
        {
            if (!throwDagger)
            {
                //Skill influence
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AlterationSkillDef).m_level;

                Vector3 vector = player.GetEyePoint() + player.GetLookDir() * .2f + player.transform.up * .3f + player.transform.right * .28f;
                GameObject prefab = ZNetScene.instance.GetPrefab("VL_PoisonBomb");
                GameObject GO_Bomb = UnityEngine.Object.Instantiate(prefab, vector, Quaternion.identity);
                GameObject GOB_Explosion = ZNetScene.instance.GetPrefab("VL_PoisonBombExplosion");
                Aoe aoe = GOB_Explosion.gameObject.GetComponentInChildren<Aoe>();
                aoe.m_damage.m_poison = 10f + sLevel;
                aoe.m_ttl = 4f + .1f * sLevel;
                aoe.m_hitInterval = .5f - (.0025f * sLevel);
                Projectile P_Bomb = GO_Bomb.GetComponent<Projectile>();
                P_Bomb.name = "Poison Bomb";
                P_Bomb.m_respawnItemOnHit = false;
                P_Bomb.m_spawnOnHit = null;
                P_Bomb.m_ttl = 10f;
                P_Bomb.transform.localRotation = Quaternion.LookRotation(player.GetAimDir(vector));
                P_Bomb.m_spawnOnHit = GOB_Explosion;
                GO_Bomb.transform.localScale = Vector3.one * .5f;
                RaycastHit hitInfo = default(RaycastHit);
                Vector3 position = player.transform.position;
                Vector3 target = (!Physics.Raycast(vector, player.GetLookDir(), out hitInfo, float.PositiveInfinity, ScriptChar_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                HitData hitData = new HitData();
                hitData.m_skill = ValheimLegends.AlterationSkill;
                Vector3 a = Vector3.MoveTowards(GO_Bomb.transform.position, target, 1f);
                P_Bomb.Setup(player, (a - GO_Bomb.transform.position) * 25f, -1f, hitData, null);
                Traverse.Create(root: P_Bomb).Field("m_skill").SetValue(ValheimLegends.AlterationSkill);
                GO_Bomb = null;
            }
            else
            {
                //Skill influence
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;

                Vector3 vector = player.GetEyePoint() + player.GetLookDir() * .2f + player.transform.up * .3f + player.transform.right * .28f;
                GameObject prefab = ZNetScene.instance.GetPrefab("VL_ThrowingKnife");
                GameObject GO_ThrowingKnife = UnityEngine.Object.Instantiate(prefab, vector, Quaternion.identity);
                Projectile P_ThrowingKnife = GO_ThrowingKnife.GetComponent<Projectile>();
                P_ThrowingKnife.name = "ThrowingKnife";
                P_ThrowingKnife.m_respawnItemOnHit = false;
                P_ThrowingKnife.m_spawnOnHit = null;
                P_ThrowingKnife.m_ttl = 10f;                
                P_ThrowingKnife.transform.localRotation = Quaternion.LookRotation(player.GetAimDir(vector));
                GO_ThrowingKnife.transform.localScale = Vector3.one * .2f;
                RaycastHit hitInfo = default(RaycastHit);
                Vector3 position = player.transform.position;
                Vector3 target = (!Physics.Raycast(vector, player.GetLookDir(), out hitInfo, float.PositiveInfinity, ScriptChar_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                HitData hitData = new HitData();
                hitData.m_damage.m_pierce = UnityEngine.Random.Range(5f + sLevel, 10f + 2f * sLevel);
                hitData.m_skill = ValheimLegends.DisciplineSkill;
                Vector3 a = Vector3.MoveTowards(GO_ThrowingKnife.transform.position, target, 1f);
                P_ThrowingKnife.Setup(player, (a - GO_ThrowingKnife.transform.position) * 30f, -1f, hitData, null);
                Traverse.Create(root: P_ThrowingKnife).Field("m_skill").SetValue(ValheimLegends.AlterationSkill);
                GO_ThrowingKnife = null;
            }

        }

        public static void Process_Input(Player player, ref Rigidbody playerBody, ref float altitude)
        {
            if (ZInput.GetButtonDown("Jump") && !player.IsDead() && !player.InAttack() && !player.IsEncumbered() && !player.InDodge() && !player.IsKnockedBack())
            {
                SE_Rogue se_r = (SE_Rogue)player.GetSEMan().GetStatusEffect("SE_VL_Rogue");
                if (!player.IsOnGround() && canDoubleJump && se_r != null && se_r.hitCount > 0)
                {
                    Vector3 velVec = player.GetVelocity();
                    velVec.y = 0f;
                    ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("jump");
                    playerBody.velocity = (velVec * 2f) + new Vector3(0, 8f, 0f);
                    se_r.hitCount--;
                    canDoubleJump = false;
                    altitude = 0;
                }
                else if (player.IsOnGround())
                {
                    canDoubleJump = true;
                }
            }

            if (player.IsBlocking() && ZInput.GetButtonDown("Attack"))
            {
                SE_Rogue se_r = (SE_Rogue)player.GetSEMan().GetStatusEffect("SE_VL_Rogue");
                if (player.GetStamina() >= VL_Utility.GetPoisonBombCost && se_r != null && se_r.hitCount > 0)
                {
                    se_r.hitCount--;

                    ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("throw_bomb");

                    ValheimLegends.isChargingDash = true;
                    ValheimLegends.dashCounter = 0;
                    throwDagger = true;

                    //knife_secondary
                    //sword_secondary
                    //swing_longsword2
                    //knife_stab2
                    //throw_bomb

                    //Skill gain
                    player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetPoisonBombSkillGain *.3f);
                }
            }
            if (VL_Utility.Ability3_Input_Down)
            {
                RaycastHit hitInfo = default(RaycastHit);
                Vector3 position = player.transform.position;
                Vector3 target = (!Physics.Raycast(player.GetEyePoint(), player.GetLookDir(), out hitInfo, float.PositiveInfinity, ScriptChar_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                Physics.SphereCast(player.GetEyePoint(), 0.1f, player.GetLookDir(), out hitInfo, 150f, ScriptChar_Layermask);
                if (hitInfo.collider != null && hitInfo.collider.gameObject != null)
                {
                    Character ch = null;
                    hitInfo.collider.gameObject.TryGetComponent<Character>(out ch);
                    bool flag = ch != null;
                    if(ch == null)
                    {
                        ch = (Character)hitInfo.collider.GetComponentInParent(typeof(Character));
                        flag = ch != null;
                        if(ch == null)
                        {
                            ch = (Character)hitInfo.collider.GetComponentInChildren<Character>();
                            flag = ch != null;
                        }
                    }

                    if (flag && !ch.IsPlayer())
                    {
                        //ZLog.Log("collider " + ch.m_name);
                        //ZLog.Log("collider distance " + hitInfo.distance);

                        if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
                        {
                            //player.Message(MessageHud.MessageType.Center, "Meteor - starting");
                            if (player.GetStamina() >= VL_Utility.GetBackstabCost)
                            {
                                //Ability Cooldown
                                StatusEffect se_cd = (SE_Ability3_CD)ScriptableObject.CreateInstance(typeof(SE_Ability3_CD));
                                se_cd.m_ttl = VL_Utility.GetBackstabCooldownTime;
                                player.GetSEMan().AddStatusEffect(se_cd);

                                //Ability Cost
                                player.UseStamina(VL_Utility.GetBackstabCost);

                                //Skill influence
                                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;

                                //Effects, animations, and sounds
                                //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("gpower");                        
                                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_Smokeburst"), player.GetEyePoint(), Quaternion.identity);

                                backstabVector = (ch.transform.position - player.transform.position) / Vector3.Distance(ch.transform.position, player.transform.position);
                                //ZLog.Log("move towards vector is " + backstabVector);
                                float pos = -1.5f;
                                if (hitInfo.collider.bounds != null && hitInfo.collider.bounds.size != null)
                                {
                                    pos = (hitInfo.collider.bounds.size.x + hitInfo.collider.bounds.size.z) / 2f;
                                    pos = Mathf.Clamp(pos, .6f, 2f);
                                    pos *= -1f;
                                }
                                backstabPoint = ch.transform.position + ch.transform.forward * pos;
                                backstabPoint.y += .1f;
                                //Lingering effects
                                //Apply effects
                                player.transform.position = backstabPoint;                                
                                player.transform.rotation = ch.transform.rotation;
                                ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("knife_stab2");
                                if (BaseAI.IsEnemy(player, ch))
                                {
                                    Vector3 direction = (ch.transform.position - player.transform.position);
                                    HitData hitData = new HitData();
                                    hitData.m_damage = player.GetCurrentWeapon().GetDamage();
                                    hitData.m_damage.Modify(UnityEngine.Random.Range(.6f, .8f) * (1f + .005f * sLevel));
                                    hitData.m_pushForce = 10f + .5f * sLevel;
                                    hitData.m_point = ch.GetEyePoint();
                                    hitData.m_dir = (player.transform.position - ch.transform.position);
                                    hitData.m_skill = ValheimLegends.DisciplineSkill;
                                    ch.Damage(hitData);
                                }
                                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_Smokeburst"), backstabPoint, Quaternion.identity);
                                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_Shadowburst"), backstabPoint + player.transform.up * .5f, Quaternion.LookRotation(player.GetLookDir()));
                                altitude = 0;
                                //Skill gain
                                player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetBackstabSkillGain);
                            }
                            else
                            {
                                player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Backstab: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetBackstabCost + ")");
                            }
                        }
                        else
                        {
                            player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                        }
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Invalid target");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "No target");
                }
            }            
            else if(VL_Utility.Ability2_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability2_CD"))
                {
                    if (player.GetStamina() >= VL_Utility.GetFadeCost)
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability2_CD)ScriptableObject.CreateInstance(typeof(SE_Ability2_CD));
                        se_cd.m_ttl = VL_Utility.GetFadeCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetFadeCost);

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.EvocationSkillDef).m_level;

                        //Effects, animations, and sounds
                        GameObject effect = ZNetScene.instance.GetPrefab("vfx_odin_despawn");
                        UnityEngine.Object.Instantiate(effect, player.GetCenterPoint(), Quaternion.identity);
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_wraith_death"), player.transform.position, Quaternion.identity);

                        //Lingering effects

                        //Apply effects
                        fadePoint = player.transform.position;
                        canGainTrick = true;

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.EvocationSkill, VL_Utility.GetFadeSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to set Fade point: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetFadeCost + ")");
                    }
                }
                else if(player.GetSEMan().HaveStatusEffect("SE_VL_Ability2_CD"))
                {
                    GameObject effect = ZNetScene.instance.GetPrefab("vfx_odin_despawn");
                    UnityEngine.Object.Instantiate(effect, player.GetCenterPoint(), Quaternion.identity);
                    UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_wraith_death"), player.transform.position, Quaternion.identity);
                    player.transform.position = fadePoint;
                    if (canGainTrick)
                    {
                        SE_Rogue se_r = (SE_Rogue)player.GetSEMan().GetStatusEffect("SE_VL_Rogue");
                        se_r.hitCount++;
                        canGainTrick = false;
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
                    if (player.GetStamina() >= VL_Utility.GetPoisonBombCost)
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability1_CD)ScriptableObject.CreateInstance(typeof(SE_Ability1_CD));
                        se_cd.m_ttl = VL_Utility.GetPoisonBombCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetPoisonBombCost);

                        //Effects, animations, and sounds
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("throw_bomb");
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetSpeed(3f);
                        //VL_Utility.RotatePlayerToTarget(player);

                        //Lingering effects

                        //Apply effects
                        ValheimLegends.isChargingDash = true;
                        ValheimLegends.dashCounter = 0;
                        throwDagger = false;
                        //knife_secondary
                        //sword_secondary
                        //swing_longsword2
                        //knife_stab2
                        //throw_bomb

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.AlterationSkill, VL_Utility.GetPoisonBombSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to throw Poison Bomb: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetPoisonBombCost +")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
            else
            {
                ValheimLegends.isChanneling = false;
            }
        }
    }
}

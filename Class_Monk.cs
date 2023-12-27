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
    public class Class_Monk
    {
        private static int Script_Solidmask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock");
        private static int Script_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "character", "character_noenv", "character_trigger", "character_net", "character_ghost");
        private static int ScriptChar_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "piece", "terrain", "vehicle", "viewblock", "character", "character_noenv", "character_trigger", "character_net", "character_ghost", "Water");

        public enum MonkAttackType
        {
            MeteorPunch = 12,
            MeteorSlam = 13,
            FlyingKick = 1,
            FlyingKickStart = 8,
            Surge = 20,
            Psibolt = 15
        }

        public static MonkAttackType QueuedAttack;

        private static int fkickCount;
        private static int fkickCountMax;
        private static Vector3 kickDir;
        private static List<int> kicklist;

        public static bool PlayerIsUnarmed
        {
            get
            {
                Player p = Player.m_localPlayer;
                if (p.GetCurrentWeapon() != null)
                {
                    
                    ItemDrop.ItemData shield = Traverse.Create(root: p).Field(name: "m_leftItem").GetValue<ItemDrop.ItemData>();
                    ItemDrop.ItemData.SharedData sid = p.GetCurrentWeapon().m_shared;
                    if (sid != null && ((sid.m_name.ToLower() == "unarmed") || sid.m_attachOverride == ItemDrop.ItemData.ItemType.Hands) && shield == null)
                    {
                        //ZLog.Log("unarmed attack");
                        return true;
                    }
                }
                return false;
            }
        }

        public static void Impact_Effect(Player player, float altitude)
        {
            List<Character> allCharacters = Character.GetAllCharacters();
            ValheimLegends.shouldValkyrieImpact = false;
            foreach (Character ch in allCharacters)
            {
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;
                if (BaseAI.IsEnemy(player, ch) && (ch.transform.position - player.transform.position).magnitude <= 6f + (.03f * sLevel) && VL_Utility.LOS_IsValid(ch, player.transform.position))
                {
                    Vector3 direction = (ch.transform.position - player.transform.position);
                    HitData hitData = new HitData();
                    hitData.m_damage.m_blunt = 5 + (3f * altitude) + UnityEngine.Random.Range(1f * sLevel, 2f * sLevel) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_monkChiSlam;
                    hitData.m_pushForce = 20f * VL_GlobalConfigs.g_DamageModifer;
                    hitData.m_point = ch.GetEyePoint();
                    hitData.m_dir = direction;
                    hitData.m_skill = ValheimLegends.DisciplineSkill;
                    ch.Damage(hitData);
                    //ch.Stagger(direction);
                }
            }
            UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_MeteorSlam"), player.transform.position + player.transform.up * .2f, Quaternion.identity);
        }

        public static void Execute_Attack(Player player, ref Rigidbody playerBody, ref float altitude)
        {
            if(QueuedAttack == MonkAttackType.MeteorPunch)
            {
                Vector3 effects = player.GetEyePoint() + player.GetLookDir() * .2f + player.transform.up * -.4f + player.transform.right * -.4f;
                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_Shockwave"), effects, Quaternion.LookRotation(player.transform.forward));
                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ReverseLightburst"), effects, Quaternion.LookRotation(player.transform.forward));

                //Skill influence
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;

                RaycastHit hitInfo = default(RaycastHit);
                Vector3 position = player.transform.position;
                Vector3 target = (!Physics.Raycast(player.GetEyePoint(), player.GetLookDir(), out hitInfo, float.PositiveInfinity, ScriptChar_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                Physics.SphereCast(player.GetEyePoint(), 0.1f, player.GetLookDir(), out hitInfo, 4f, ScriptChar_Layermask);
                if (hitInfo.collider != null && hitInfo.collider.gameObject != null)
                {
                    Character colliderChar = null;
                    hitInfo.collider.gameObject.TryGetComponent<Character>(out colliderChar);
                    bool flag = colliderChar != null;
                    if (colliderChar == null)
                    {
                        colliderChar = (Character)hitInfo.collider.GetComponentInParent(typeof(Character));
                        flag = colliderChar != null;
                        if (colliderChar == null)
                        {
                            colliderChar = (Character)hitInfo.collider.GetComponentInChildren<Character>();
                            flag = colliderChar != null;
                        }
                    }

                    List<Character> allCharacters = new List<Character>();
                    allCharacters.Clear();
                    Character.GetCharactersInRange(effects + player.transform.forward * 2f, 2.5f, allCharacters);

                    if (flag && !colliderChar.IsPlayer())
                    {
                        allCharacters.Add(colliderChar);
                    }

                    foreach (Character ch in allCharacters)
                    {
                        if (BaseAI.IsEnemy(player, ch))
                        {
                            Vector3 direction = (ch.transform.position - player.transform.position);
                            HitData hitData = new HitData();
                            hitData.m_damage.m_blunt = UnityEngine.Random.Range(12f + (.5f * sLevel), 24f + (1f * sLevel)) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_monkChiPunch;
                            hitData.m_pushForce = 45f + (.5f * sLevel);
                            hitData.m_point = ch.GetEyePoint();
                            hitData.m_dir = direction;
                            hitData.m_skill = ValheimLegends.DisciplineSkill;
                            ch.Damage(hitData);
                        }
                    }
                }
            }
            else if(QueuedAttack == MonkAttackType.MeteorSlam)
            {
                playerBody.velocity += new Vector3(0f, -8f, 0f);
                for (int i = 0; i < 4; i++)
                {
                    UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ReverseLightburst"), player.transform.position + player.transform.up * UnityEngine.Random.Range(0, .5f) + player.transform.right * UnityEngine.Random.Range(-.3f, .3f), Quaternion.LookRotation(new Vector3(0f, -1f, 0f)));
                }
                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_perfectblock"), player.transform.position, Quaternion.identity);
                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_perfectblock"), player.GetCenterPoint(), Quaternion.identity);

            }
            else if (QueuedAttack == MonkAttackType.Surge)
            {
                SE_Monk se_monk = (SE_Monk)player.GetSEMan().GetStatusEffect("SE_VL_Monk".GetStableHashCode());
                se_monk.surging = true;
                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_Potion_frostresist"), player.transform.position, Quaternion.identity);
                ValheimLegends.isChanneling = false;
            }
            else if(QueuedAttack == MonkAttackType.Psibolt)
            {
                //Skill influence
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;
                SE_Monk se_monk = (SE_Monk)player.GetSEMan().GetStatusEffect("SE_VL_Monk".GetStableHashCode());
                Vector3 vector = player.GetEyePoint() + player.GetLookDir() * .4f + player.transform.up * .1f + player.transform.right * .22f;
                GameObject prefab = ZNetScene.instance.GetPrefab("VL_PsiBolt");
                GameObject GO_PsiBolt = UnityEngine.Object.Instantiate(prefab, vector, Quaternion.identity);
                Projectile P_PsiBolt = GO_PsiBolt.GetComponent<Projectile>();
                P_PsiBolt.name = "PsiBolt";
                P_PsiBolt.m_respawnItemOnHit = false;
                P_PsiBolt.m_spawnOnHit = null;
                P_PsiBolt.m_ttl = 12f;
                P_PsiBolt.m_rayRadius = .01f;
                P_PsiBolt.transform.localRotation = Quaternion.LookRotation(player.GetAimDir(vector));
                GO_PsiBolt.transform.localScale = Vector3.one;
                RaycastHit hitInfo = default(RaycastHit);
                Vector3 position = player.transform.position;
                Vector3 target = (!Physics.Raycast(vector, player.GetLookDir(), out hitInfo, float.PositiveInfinity, ScriptChar_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                HitData hitData = new HitData();
                hitData.m_damage.m_slash = UnityEngine.Random.Range(1f + (.2f *sLevel), 4f + (.4f * sLevel)) * se_monk.hitCount * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_monkChiBlast;
                hitData.m_damage.m_spirit= UnityEngine.Random.Range((.2f * sLevel), 1 + (.2f * sLevel)) * se_monk.hitCount * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_monkChiBlast;
                hitData.m_skill = ValheimLegends.DisciplineSkill;
                hitData.SetAttacker(player);
                Vector3 a = Vector3.MoveTowards(GO_PsiBolt.transform.position, target, 1f);
                P_PsiBolt.Setup(player, (a - GO_PsiBolt.transform.position) * 60f, -1f, hitData, null, null);
                Traverse.Create(root: P_PsiBolt).Field("m_skill").SetValue(ValheimLegends.DisciplineSkill);
                se_monk.hitCount = 0;
                GO_PsiBolt = null;
            }
            else if(QueuedAttack == MonkAttackType.FlyingKick || QueuedAttack == MonkAttackType.FlyingKickStart)
            {
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;
                float sDamageMultiplier = .5f + (sLevel * .005f) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_monkFlyingKick;
                SE_Monk se_monk = (SE_Monk)player.GetSEMan().GetStatusEffect("SE_VL_Monk".GetStableHashCode());
                //RaycastHit hitInfo = default(RaycastHit);
                Vector3 lookVec = player.GetLookDir();
                lookVec.y = 0f;
                player.transform.rotation = Quaternion.AngleAxis(40 * fkickCount, player.transform.up) * Quaternion.LookRotation(kickDir);

                Vector3 hitVec = default(Vector3);
                Vector3 fwdVec = kickDir;
                Vector3 moveVec = player.transform.position;
                Vector3 yVec = player.transform.position;
                Vector3 effectVec = player.transform.position + player.transform.forward * .2f + player.transform.up * .7f;
                yVec.y += 0.1f;                
                int i = 0;
                for (; i <= 1; i++)
                {
                    RaycastHit hitInfo = default(RaycastHit);
                    bool flag = false;
                    for (int j = 0; j <= 10; j++)
                    {
                        Vector3 _v = Vector3.MoveTowards(player.transform.position, player.transform.position + fwdVec * 100f, (float)((float)i + (float)j * 0.1f));
                        _v.y = yVec.y;
                        if (_v.y < ZoneSystem.instance.GetGroundHeight(_v))
                        {
                            yVec.y = ZoneSystem.instance.GetGroundHeight(_v) + 1f;
                            _v.y = yVec.y;
                        }
                        flag = Physics.SphereCast(_v, 0.05f, fwdVec, out hitInfo, float.PositiveInfinity, ScriptChar_Layermask);
                        if (flag && (bool)hitInfo.collider)
                        {
                            hitVec = hitInfo.point;
                            break;
                        }
                    }
                    moveVec = Vector3.MoveTowards(player.transform.position, player.transform.position + fwdVec * 100f, (float)i *.6f);
                    moveVec.y = ((ZoneSystem.instance.GetSolidHeight(moveVec) - ZoneSystem.instance.GetGroundHeight(moveVec) <= 1f) ? ZoneSystem.instance.GetSolidHeight(moveVec) : ZoneSystem.instance.GetGroundHeight(moveVec));
                    //GameObject go_dasheffects = ZNetScene.instance.GetPrefab("vfx_stonegolem_attack_hit");
                    //go_dasheffects.transform.localScale = Vector3.one * .5f;
                    if (flag && Vector3.Distance(new Vector3(moveVec.x, yVec.y, moveVec.z), hitVec) <= 1.5f)// && !flag2) //yVec.y
                    {
                        //yVec = Vector3.MoveTowards(hitVec, yVec, 1f);
                        yVec = Vector3.MoveTowards(hitVec, yVec, 1f);
                        //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_stonegolem_attack_hit"), moveVec, Quaternion.identity);
                        //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_barley_destroyed"), moveVec, Quaternion.identity);
                        //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_beehive_hit"), yVec, Quaternion.identity);
                        //UnityEngine.Object.Instantiate(go_dasheffects, moveVec, Quaternion.identity);
                        break;
                    }
                    //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_beehive_hit"), yVec, Quaternion.identity);
                    yVec = new Vector3(moveVec.x, yVec.y, moveVec.z);
                    foreach (Character ch in Character.GetAllCharacters())
                    {
                        HitData hitData = new HitData();
                        hitData.m_damage = player.GetCurrentWeapon().GetDamage();
                        hitData.ApplyModifier(UnityEngine.Random.Range(.8f, 1.2f) * sDamageMultiplier);
                        hitData.m_point = ch.GetCenterPoint();
                        hitData.m_pushForce = 4f;
                        hitData.m_dir = ch.transform.position - moveVec;
                        hitData.m_skill = ValheimLegends.DisciplineSkill;
                        float num = Vector3.Distance(ch.transform.position, player.transform.position);
                        if (BaseAI.IsEnemy(ch, player) && num <= 2.5f && !kicklist.Contains(ch.GetInstanceID()))
                        {
                            ch.Damage(hitData);
                            UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_HeavyCrit"), ch.GetCenterPoint(), Quaternion.identity);
                            kicklist.Add(ch.GetInstanceID());
                        }
                    }
                }
                if (i > 10 && ZoneSystem.instance.GetSolidHeight(yVec) - yVec.y <= 2f)
                {
                    yVec.y = ZoneSystem.instance.GetSolidHeight(yVec);
                }
                if(fkickCount <= (fkickCountMax * .4f))
                {
                    RaycastHit hitInfoUp = default(RaycastHit);
                    Vector3 targetUp = (!Physics.Raycast(player.GetEyePoint(), player.transform.up, out hitInfoUp, 1f, Script_Solidmask) || !(bool)hitInfoUp.collider) ? (player.transform.position + Vector3.up * 10f) : hitInfoUp.point;
                    if (Vector3.Distance(hitInfoUp.point, player.GetEyePoint()) > .8f)
                    {
                        yVec.y += .15f;
                    }
                    else
                    {
                        yVec.y -= .18f;
                    }
                }        
                if(fkickCount % 3 == 0)
                {
                    kicklist.Clear();
                }
                if(fkickCount < fkickCountMax)
                {
                    fkickCount++;
                    ValheimLegends.isChargingDash = true;
                    ValheimLegends.dashCounter = 0;
                }
                //player.transform.position = yVec;
                playerBody.position = yVec;
                bool flagHitE = false;
                if (fkickCount >= 9)
                {
                    RaycastHit hitE = default(RaycastHit);
                    bool flagHit = false;
                    flagHit = Physics.SphereCast(yVec, 0.05f, fwdVec, out hitE, 1f, ScriptChar_Layermask);
                    if (flagHit && (bool)hitE.collider && hitE.collider.gameObject != null)
                    {
                        hitVec = hitE.point;
                        Character colliderChar = null;
                        hitE.collider.gameObject.TryGetComponent<Character>(out colliderChar);
                        flagHitE = colliderChar != null;
                        if (colliderChar == null)
                        {
                            colliderChar = (Character)hitE.collider.GetComponentInParent(typeof(Character));
                            flagHitE = colliderChar != null;
                        }
                        if (flagHitE && BaseAI.IsEnemy(colliderChar, player))
                        {
                            HitData hitData = new HitData();
                            hitData.m_damage = player.GetCurrentWeapon().GetDamage();
                            hitData.ApplyModifier(UnityEngine.Random.Range(1f, 1.5f) * sDamageMultiplier);
                            hitData.m_point = hitVec;
                            hitData.m_pushForce = 10f;
                            hitData.m_dir = hitVec - player.transform.position;
                            hitData.m_skill = ValheimLegends.DisciplineSkill;
                            colliderChar.Damage(hitData);
                            UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_perfectblock"), player.transform.position, Quaternion.identity);
                            UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_perfectblock"), hitVec, Quaternion.identity);
                        }
                        else
                        {
                            flagHitE = false;
                        }
                    }
                }
                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ChiPulse"), effectVec + player.transform.up *.2f, Quaternion.LookRotation(player.transform.forward));
                QueuedAttack = MonkAttackType.FlyingKick;
                if(flagHitE)
                {                    
                    VL_Utility.RotatePlayerToTarget(player);
                    ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("jump");
                    fwdVec.y = 0;
                    playerBody.velocity = (fwdVec * -1.5f) + new Vector3(0, 10f, 0f);
                    ValheimLegends.isChargingDash = false;
                    se_monk.hitCount += 2;
                }
                altitude = 0f;
                //player.transform.rotation = Quaternion.LookRotation(fwdVec);
            }
        }

        public static void Process_Input(Player player, ref Rigidbody playerBody, ref float altitude, ref Animator anim)
        {
            SE_Monk se_monk = (SE_Monk)player.GetSEMan().GetStatusEffect("SE_VL_Monk".GetStableHashCode());

            if (VL_Utility.Ability3_Input_Down)
            {
                if (PlayerIsUnarmed)
                {
                    if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
                    {
                        //player.Message(MessageHud.MessageType.Center, "PsiBolt - starting");
                        if (se_monk.hitCount >= 1)
                        {
                            //Ability Cooldown
                            StatusEffect se_cd = (SE_Ability3_CD)ScriptableObject.CreateInstance(typeof(SE_Ability3_CD));
                            se_cd.m_ttl = VL_Utility.GetPsiBoltCooldownTime;
                            player.GetSEMan().AddStatusEffect(se_cd);

                            if (player.IsBlocking())
                            {
                                player.StartEmote("challenge", true);
                                QueuedAttack = MonkAttackType.Surge;
                                ValheimLegends.isChanneling = true;
                                ValheimLegends.isChargingDash = true;
                                ValheimLegends.dashCounter = 0;
                            }
                            else
                            {

                                VL_Utility.RotatePlayerToTarget(player);
                                //Skill influence
                                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;

                                //Effects, animations, and sounds
                                ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("swing_axe2");
                                //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_GP_Stone"), player.GetEyePoint(), Quaternion.identity);

                                //Lingering effects


                                //Apply effects
                                QueuedAttack = MonkAttackType.Psibolt;
                                ValheimLegends.isChargingDash = true;
                                ValheimLegends.dashCounter = 0;
                    
                            }
                            //Skill gain
                            player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetPsiBoltSkillGain * se_monk.hitCount);
                        }
                        else
                        {
                            player.Message(MessageHud.MessageType.TopLeft, "Not enough energy for Chi Blast : (" + se_monk.hitCount.ToString("#") + "/" + VL_Utility.GetPsiBoltCost + ")");
                        }
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Must be unarmed to use this ability");
                }
            }
            else if (VL_Utility.Ability2_Input_Down)
            {
                if (PlayerIsUnarmed)
                {
                    if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability2_CD"))
                    {
                        //player.Message(MessageHud.MessageType.Center, "Frost Nova");
                        if (player.GetStamina() >= VL_Utility.GetFlyingKickCost)
                        {
                            //Ability Cooldown
                            StatusEffect se_cd = (SE_Ability2_CD)ScriptableObject.CreateInstance(typeof(SE_Ability2_CD));
                            se_cd.m_ttl = VL_Utility.GetFlyingKickCooldownTime;
                            player.GetSEMan().AddStatusEffect(se_cd);

                            //Ability Cost
                            player.UseStamina(VL_Utility.GetFlyingKickCost);

                            //Skill influence
                            float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;

                            //Effects, animations, and sounds
                            //player.StartEmote("cheer");
                            ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("unarmed_kick");
                            //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_guardstone_activate"), player.transform.position, Quaternion.identity);

                            //Lingering effects

                            //Apply effects
                            QueuedAttack = MonkAttackType.FlyingKickStart;
                            ValheimLegends.isChargingDash = true;
                            ValheimLegends.dashCounter = 0;
                            kickDir = new Vector3(player.GetLookDir().x, 0f, player.GetLookDir().z);
                            fkickCount = 0;
                            fkickCountMax = 18;
                            kicklist = new List<int>();
                            kicklist.Clear();
                            //Skill gain
                            player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetFlyingKickSkillGain);
                        }
                        else
                        {
                            player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Flying Kick: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetFlyingKickCost + ")");
                        }
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                    }
                }            
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Must be unarmed to use this ability");
                }
            }
            else if (VL_Utility.Ability1_Input_Down)
            {
                if (PlayerIsUnarmed)
                {
                    if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability1_CD"))
                    {
                        //player.Message(MessageHud.MessageType.Center, "MeteorPunch");                    
                        if (se_monk.hitCount >= VL_Utility.GetMeteorPunchCost)
                        {
                            VL_Utility.RotatePlayerToTarget(player);
                            float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;
                            //Ability Cooldown
                            StatusEffect se_cd = (SE_Ability1_CD)ScriptableObject.CreateInstance(typeof(SE_Ability1_CD));
                            se_cd.m_ttl = VL_Utility.GetMeteorPunchCooldownTime;
                            player.GetSEMan().AddStatusEffect(se_cd);

                            //Ability Cost
                            player.AddStamina(10f + (.3f * sLevel));
                            se_monk.hitCount -= Mathf.RoundToInt(VL_Utility.GetMeteorPunchCost);

                            if (player.IsOnGround() || (player.transform.position.y - ZoneSystem.instance.GetSolidHeight(player.transform.position) < 2f))
                            {
                                //Effects, animations, and sounds
                                ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("unarmed_attack1");
                                //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetSpeed(5f);

                                //Lingering effects

                                //Apply effects
                                QueuedAttack = MonkAttackType.MeteorPunch;
                                ValheimLegends.isChargingDash = true;
                                ValheimLegends.dashCounter = 0;

                            }
                            else
                            {
                                //Effects, animations, and sounds
                                ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("battleaxe_attack2");
                                //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetSpeed(5f);

                                //Lingering effects

                                //Apply effects
                                QueuedAttack = MonkAttackType.MeteorSlam;
                                ValheimLegends.isChargingDash = true;
                                ValheimLegends.dashCounter = 0;
                                ValheimLegends.shouldValkyrieImpact = true;
                            }
                            //Skill gain
                            player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetMeteorPunchSkillGain);
                        }
                        else
                        {
                            player.Message(MessageHud.MessageType.TopLeft, "Not enough energy for Chi Strike: (" + se_monk.hitCount.ToString("#") + "/" + (VL_Utility.GetMeteorPunchCost) + ")");
                        }
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Must be unarmed to use this ability");
                }
            }
            else
            {
                ValheimLegends.isChanneling = false;
            }            
        }
    }
}

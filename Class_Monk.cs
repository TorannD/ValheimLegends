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
        private static int Script_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "character", "character_noenv", "character_trigger");
        private static int ScriptChar_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "piece", "terrain", "vehicle", "viewblock", "character", "character_noenv", "character_trigger", "Water");

        public enum MonkAttackType
        {
            MeteorPunch = 12,
            MeteorSlam = 13,
            FlyingKick = 1,
            FlyingKickStart = 8,
            Psibolt = 15
        }

        public static MonkAttackType QueuedAttack;

        private static int fkickCount;
        private static int fkickCountMax;
        private static Vector3 kickDir;
        private static List<int> kicklist;

        public static bool PlayerUsingUnarmed
        {
            get
            {
                Player p = Player.m_localPlayer;
                if (p.GetCurrentWeapon() != null)
                {
                    ItemDrop.ItemData shield = Traverse.Create(root: p).Field(name: "m_leftItem").GetValue<ItemDrop.ItemData>();
                    ItemDrop.ItemData.SharedData sid = p.GetCurrentWeapon().m_shared;
                    
                    if (sid != null && (sid.m_name.ToLower() == "unarmed") && shield == null)
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
                if (BaseAI.IsEnemy(player, ch) && (ch.transform.position - player.transform.position).magnitude <= 6f + (.03f * sLevel))
                {
                    Vector3 direction = (ch.transform.position - player.transform.position);
                    HitData hitData = new HitData();
                    hitData.m_damage.m_blunt = 5 + (3f * altitude) + UnityEngine.Random.Range(1f * sLevel, 2f * sLevel) * VL_GlobalConfigs.g_DamageModifer;
                    hitData.m_pushForce = 20f * VL_GlobalConfigs.g_DamageModifer;
                    hitData.m_point = ch.GetEyePoint();
                    hitData.m_dir = (player.transform.position - ch.transform.position);
                    hitData.m_skill = ValheimLegends.DisciplineSkill;
                    ch.ApplyDamage(hitData, true, true, HitData.DamageModifier.Normal);
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
                            hitData.m_damage.m_blunt = UnityEngine.Random.Range(12f + (.5f * sLevel), 24f + (1f * sLevel)) * VL_GlobalConfigs.g_DamageModifer;
                            hitData.m_pushForce = 45f + (.5f * sLevel);
                            hitData.m_point = ch.GetEyePoint();
                            hitData.m_dir = (ch.transform.position - player.transform.position);
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
            else if(QueuedAttack == MonkAttackType.Psibolt)
            {
                //Skill influence
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;

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
                hitData.m_damage.m_blunt = UnityEngine.Random.Range(10f + sLevel, 40f + 2f * sLevel);
                hitData.m_damage.m_spirit= UnityEngine.Random.Range(10f + sLevel, 20f + sLevel);
                hitData.m_skill = ValheimLegends.DisciplineSkill;
                Vector3 a = Vector3.MoveTowards(GO_PsiBolt.transform.position, target, 1f);
                P_PsiBolt.Setup(player, (a - GO_PsiBolt.transform.position) * 60f, -1f, hitData, null);
                Traverse.Create(root: P_PsiBolt).Field("m_skill").SetValue(ValheimLegends.DisciplineSkill);
                GO_PsiBolt = null;
            }
            else if(QueuedAttack == MonkAttackType.FlyingKick || QueuedAttack == MonkAttackType.FlyingKickStart)
            {
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;
                float sDamageMultiplier = .8f + (sLevel * .005f) * VL_GlobalConfigs.g_DamageModifer;

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
                    moveVec = Vector3.MoveTowards(player.transform.position, player.transform.position + fwdVec * 100f, (float)i);
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
                        hitData.m_dir = (moveVec - ch.transform.position);
                        hitData.m_skill = ValheimLegends.DisciplineSkill;
                        float num = Vector3.Distance(ch.transform.position, player.transform.position);
                        if (!ch.IsPlayer() && num <= 2.5f && !kicklist.Contains(ch.GetInstanceID()))
                        {
                            ch.ApplyDamage(hitData, true, true, HitData.DamageModifier.Normal);
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
                    yVec.y += .15f;
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
                player.transform.position = yVec;
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
                        if (flagHitE && !colliderChar.IsPlayer())
                        {
                            HitData hitData = new HitData();
                            hitData.m_damage = player.GetCurrentWeapon().GetDamage();
                            hitData.ApplyModifier(UnityEngine.Random.Range(.8f, 1.2f) * sDamageMultiplier);
                            hitData.m_point = hitVec;
                            hitData.m_dir = (player.transform.position - hitVec);
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
                }
                altitude = 0f;
                //player.transform.rotation = Quaternion.LookRotation(fwdVec);
            }
        }

        public static void Process_Input(Player player, ref Rigidbody playerBody, ref float altitude, ref Animator anim)
        {
            SE_Monk se_monk = (SE_Monk)player.GetSEMan().GetStatusEffect("SE_VL_Monk");
            if (PlayerUsingUnarmed)
            {
                if (VL_Utility.Ability3_Input_Down)
                {
                    if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
                    {
                        //player.Message(MessageHud.MessageType.Center, "PsiBolt - starting");
                        if (se_monk.hitCount >= VL_Utility.GetPsiBoltCost)
                        {
                            //Ability Cooldown
                            StatusEffect se_cd = (SE_Ability3_CD)ScriptableObject.CreateInstance(typeof(SE_Ability3_CD));
                            se_cd.m_ttl = VL_Utility.GetPsiBoltCooldownTime;
                            player.GetSEMan().AddStatusEffect(se_cd);

                            //Ability Cost
                            se_monk.hitCount -= Mathf.RoundToInt(VL_Utility.GetPsiBoltCost);

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

                            //Skill gain
                            player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetPsiBoltSkillGain);
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
                else if (VL_Utility.Ability2_Input_Down)
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
                            kickDir = player.GetLookDir();
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
                else if (VL_Utility.Ability1_Input_Down)
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
                    ValheimLegends.isChanneling = false;
                }
            }
            else
            {
                player.Message(MessageHud.MessageType.TopLeft, "Must be unarmed to use this ability");
            }
        }
    }
}

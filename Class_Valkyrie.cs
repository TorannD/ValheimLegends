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
        private static int ScriptChar_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "piece", "terrain", "vehicle", "viewblock", "character", "character_noenv", "character_trigger", "character_net", "character_ghost", "Water");


        private static GameObject GO_CastFX;

        public static bool inFlight = false;
        public static bool isBlocking = false;

        public enum ValkyrieAttackType
        {
            ShieldRelease = 12,
            HarpoonPull = 20
        }

        public static ValkyrieAttackType QueuedAttack;


        public static void Execute_Attack(Player player, ref Rigidbody playerBody, ref float altitude)
        {
            SE_Valkyrie se_v = (SE_Valkyrie)player.GetSEMan().GetStatusEffect("SE_VL_Valkyrie".GetStableHashCode());
            if (QueuedAttack == ValkyrieAttackType.ShieldRelease)
            {
                Vector3 effects = player.GetEyePoint() + player.GetLookDir() * .2f + player.transform.up * -.4f + player.transform.right * -.4f;
                for(int i = 0; i < se_v.hitCount; i++)
                {
                    UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ShieldRelease"), effects, Quaternion.LookRotation(player.transform.forward));
                }
                //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ShieldRelease"), effects, Quaternion.LookRotation(player.transform.forward));
                //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ReverseLightburst"), effects, Quaternion.LookRotation(player.transform.forward));

                //Skill influence
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AbjurationSkillDef).m_level;

                List<Character> allCharacters = new List<Character>();
                allCharacters.Clear();
                List<Character> allCharacters2 = new List<Character>();
                allCharacters2.Clear();
                Character.GetCharactersInRange(effects + player.transform.forward * 2f, 2.5f, allCharacters);
                Character.GetCharactersInRange(effects + player.transform.forward * 6f, 3f, allCharacters2);
                allCharacters.AddRange(allCharacters2);

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

                    if (flag && BaseAI.IsEnemy(colliderChar, player) && !allCharacters.Contains(colliderChar))
                    {
                        allCharacters.Add(colliderChar);
                    }
                }

                foreach (Character ch in allCharacters)
                {
                    if (BaseAI.IsEnemy(player, ch))
                    {
                        Vector3 direction = (ch.transform.position - player.transform.position);
                        HitData hitData = new HitData();
                        hitData.m_damage.m_spirit = UnityEngine.Random.Range(se_v.hitCount * (1f + (.02f * sLevel)), se_v.hitCount * (2f + (.015f * sLevel))) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_valkyrieBonusChillWave;
                        hitData.m_damage.m_frost = UnityEngine.Random.Range(se_v.hitCount * (1f + (.02f * sLevel)), se_v.hitCount * (2f + (.015f * sLevel))) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_valkyrieBonusChillWave;
                        hitData.m_point = ch.GetEyePoint();
                        hitData.m_dir = direction;
                        hitData.m_skill = ValheimLegends.AbjurationSkill;
                        ch.Damage(hitData);
                    }
                }

                se_v.hitCount = 0;
            }
            else if(QueuedAttack == ValkyrieAttackType.HarpoonPull)
            {
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;
                Vector3 vector = player.GetEyePoint() + player.GetLookDir() * .2f + player.transform.up * .1f + player.transform.right * .28f;
                GameObject prefab = ZNetScene.instance.GetPrefab("VL_ValkyrieSpear");
                GameObject GO_Harpoon = UnityEngine.Object.Instantiate(prefab, vector, Quaternion.identity);
                Projectile P_Harpoon = GO_Harpoon.GetComponent<Projectile>();
                P_Harpoon.name = "VL_ValkyrieSpear";
                P_Harpoon.m_respawnItemOnHit = false;
                P_Harpoon.m_spawnOnHit = null;
                P_Harpoon.m_ttl = 6f;
                P_Harpoon.m_gravity = 2f;
                P_Harpoon.m_aoe = 1f;
                P_Harpoon.transform.localRotation = Quaternion.LookRotation(player.GetAimDir(vector));
                GO_Harpoon.transform.localScale = Vector3.one * .8f;
                RaycastHit hitInfo = default(RaycastHit);
                Vector3 position = player.transform.position;
                Vector3 target = (!Physics.Raycast(vector, player.GetLookDir(), out hitInfo, float.PositiveInfinity, ScriptChar_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                HitData hitData = new HitData();
                //hitData.m_pushForce = 1f;
                hitData.m_skill = ValheimLegends.DisciplineSkill;
                hitData.m_dir = player.GetLookDir() * -1f;
                hitData.m_damage.m_frost = UnityEngine.Random.Range((1f + (.2f * sLevel)), (2f + (.3f * sLevel))) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_valkyrieBonusIceLance;
                hitData.m_damage.m_spirit= UnityEngine.Random.Range((1f + (.2f * sLevel)), (2f + (.3f * sLevel))) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_valkyrieBonusIceLance;
                hitData.SetAttacker(player);
                Vector3 a = Vector3.MoveTowards(GO_Harpoon.transform.position, target, 1f);
                P_Harpoon.Setup(player, (a - GO_Harpoon.transform.position) * 40f, -1f, hitData, null, null);
                Traverse.Create(root: P_Harpoon).Field("m_skill").SetValue(ValheimLegends.DisciplineSkill);
                GO_Harpoon = null;
            }
        }

        public static bool PlayerUsingShield
        {
            get
            {
                Player p = Player.m_localPlayer;
                ItemDrop.ItemData shield = Traverse.Create(root: p).Field(name: "m_leftItem").GetValue<ItemDrop.ItemData>();
                if (shield != null)
                {
                    ItemDrop.ItemData.SharedData sid = shield.m_shared;
                    if (sid != null && sid.m_itemType == ItemDrop.ItemData.ItemType.Shield)
                    {
                        //ZLog.Log("using shield");
                        return true;
                    }
                }                
                return false;
            }
        }

        public static void Impact_Effect(Player player, float altitude)
        {
            List<Character> allCharacters = Character.GetAllCharacters();
            //player.Message(MessageHud.MessageType.Center, "valkyrie impact");
            inFlight = false;
            ValheimLegends.shouldValkyrieImpact = false;
            foreach (Character ch in allCharacters)
            {
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;
                if (BaseAI.IsEnemy(player, ch) && (ch.transform.position - player.transform.position).magnitude <= (6f + (.03f * sLevel)) && VL_Utility.LOS_IsValid(ch, player.transform.position, player.GetCenterPoint()))
                {
                    Vector3 direction = (ch.transform.position - player.transform.position);
                    HitData hitData = new HitData();
                    hitData.m_damage.m_blunt = 5 + (3f * altitude) + UnityEngine.Random.Range(1.5f * sLevel, 2.5f * sLevel) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_valkyrieLeap;
                    hitData.m_pushForce = 20f * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_valkyrieLeap;
                    hitData.m_point = ch.GetEyePoint();
                    hitData.m_dir = direction;
                    hitData.m_skill = ValheimLegends.DisciplineSkill;
                    ch.Damage(hitData);
                    //ch.Stagger(direction);
                }
            }
            ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).StopAllCoroutines();
            ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("battleaxe_attack2");
            GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_gdking_stomp"), player.transform.position, Quaternion.identity);
        }

        public static void Process_Input(Player player)
        {
            System.Random rnd = new System.Random();
            Vector3 pVec = default(Vector3);

            if (player.IsBlocking() && ZInput.GetButtonDown("Attack"))
            {
                SE_Valkyrie se_v = (SE_Valkyrie)player.GetSEMan().GetStatusEffect("SE_VL_Valkyrie".GetStableHashCode());
                if (se_v.hitCount >= VL_Utility.GetHarpoonPullCost)
                {
                    //Ability Cost
                    se_v.hitCount -= (int)VL_Utility.GetHarpoonPullCost;

                    VL_Utility.RotatePlayerToTarget(player);
                    ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).StopAllCoroutines();
                    ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("spear_throw");

                    ValheimLegends.isChargingDash = true;
                    ValheimLegends.dashCounter = 0;
                    QueuedAttack = ValkyrieAttackType.HarpoonPull;

                    //Skill gain
                    player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetHarpoonPullSkillGain);
                }
            }

            if (VL_Utility.Ability3_Input_Down)
            {
                SE_Valkyrie se_v = (SE_Valkyrie)player.GetSEMan().GetStatusEffect("SE_VL_Valkyrie".GetStableHashCode());
                if (player.IsBlocking())
                {
                    if (PlayerUsingShield && se_v != null && se_v.hitCount > 0)
                    {
                        VL_Utility.RotatePlayerToTarget(player);
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).StopAllCoroutines();
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("unarmed_attack1");

                        ValheimLegends.isChargingDash = true;
                        ValheimLegends.dashCounter = 0;
                        QueuedAttack = ValkyrieAttackType.ShieldRelease;

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.AbjurationSkill, VL_Utility.GetShieldReleaseSkillGain * se_v.hitCount);
                    }
                }
                else if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
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
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("knife_secondary");
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetSpeed(.3f);
                        //player.StartEmote("cheer");
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
                        se_cd.m_ttl = VL_Utility.GetStaggerCooldownTime * VL_GlobalConfigs.c_valkyrieStaggerCooldown;
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
                            if ((BaseAI.IsEnemy(player, ch) && (ch.transform.position - player.transform.position).magnitude <= 6f) && VL_Utility.LOS_IsValid(ch, player.transform.position, player.GetCenterPoint()))
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
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("gpower");
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetSpeed(5f);
                        player.StartEmote("challenge");
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_guardstone_deactivate"), player.GetCenterPoint(), Quaternion.identity);
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_metal_blocked"), player.transform.position, Quaternion.identity);

                        //Lingering effects
                        SE_Bulwark SE_bulwark = (SE_Bulwark)ScriptableObject.CreateInstance(typeof(SE_Bulwark));
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
    }
}

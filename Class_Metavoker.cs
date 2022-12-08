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
    public class Class_Metavoker
    {
        private static int Warp_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock", "Water", "character", "character_net", "character_ghost");
        private static int Light_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock", "character", "character_net", "character_ghost");
        private static int SafeFall_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock", "Water");

        private static GameObject GO_CastFX;

        private static GameObject GO_Light;        
        private static Projectile P_Light;
        private static StatusEffect SE_Root;

        private static GameObject GO_RootDefender;
    
        private static float warpCount;
        private static float warpDistance;
        private static int warpGrowthTrigger;

        public enum MetavokerAttackType
        {
            ForceWave = 16
        }

        public static MetavokerAttackType QueuedAttack;

        public static void Execute_Attack(Player player, ref Rigidbody playerBody, ref float altitude)
        {
            if (QueuedAttack == MetavokerAttackType.ForceWave)
            {
                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ForceWall"), player.GetEyePoint(), Quaternion.LookRotation(player.GetLookDir()));
                List<Character> allCharacters = new List<Character>();
                allCharacters.Clear();
                Vector3 center = player.GetCenterPoint() + player.transform.forward * 6f;
                Character.GetCharactersInRange(center, 6f, allCharacters);
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.EvocationSkillDef).m_level;
                List<Projectile> projs = GameObject.FindObjectsOfType<Projectile>().ToList();
                if(projs != null && projs.Count > 0)
                {
                    foreach(Projectile proj in projs)
                    {
                        if(Vector3.Distance(proj.transform.position, center) <= 6f)
                        {
                            proj.m_ttl = .05f;
                            string name = proj.name.Substring(0, proj.name.IndexOf('('));
                            GameObject prefab = ZNetScene.instance.GetPrefab(name);
                            if (prefab != null)
                            {
                                GameObject GO_DupeProj = UnityEngine.Object.Instantiate(prefab, proj.transform.position, Quaternion.identity);
                                Projectile P_DupeProj = GO_DupeProj.GetComponent<Projectile>();
                                P_DupeProj.name = "DupeProj";
                                P_DupeProj.m_respawnItemOnHit = false;
                                P_DupeProj.m_spawnOnHit = null;
                                P_DupeProj.m_ttl = 4f;
                                P_DupeProj.transform.localRotation = Quaternion.LookRotation(proj.transform.forward * -1f);
                                GO_DupeProj.transform.localScale = proj.transform.localScale;
                                //RaycastHit hitInfo = default(RaycastHit);
                                //Vector3 position = player.transform.position;
                                //Vector3 target = (!Physics.Raycast(vector, player.GetLookDir(), out hitInfo, float.PositiveInfinity, ScriptChar_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                                HitData hitData = new HitData();
                                hitData.m_damage = proj.m_damage;
                                hitData.SetAttacker(player);
                                hitData.m_skill = ValheimLegends.EvocationSkill;
                                //Vector3 a = Vector3.MoveTowards(GO_DupeProj.transform.position, target, 1f);
                                P_DupeProj.Setup(player, proj.GetVelocity() * -1f, -1f, hitData, null, null);
                                Traverse.Create(root: P_DupeProj).Field("m_skill").SetValue(ValheimLegends.EvocationSkill);
                                GO_DupeProj = null;
                            }
                        }
                    }
                }
                foreach (Character ch in allCharacters)
                {
                    if (BaseAI.IsEnemy(player, ch) && VL_Utility.LOS_IsValid(ch, player.GetCenterPoint(), player.transform.position))
                    {
                        Vector3 direction = (ch.transform.position - player.transform.position);
                        float distanceFromPlayer = direction.magnitude;

                        Rigidbody chBody = Traverse.Create(root: ch).Field(name: "m_body").GetValue<Rigidbody>();                        

                        if (chBody != null)
                        {
                            float mass = chBody.mass;
                            //ZLog.Log("" + ch.m_name + " vector from player: " + direction + " distance from player " + distanceFromPlayer + " mass " + mass);
                            if(UnityEngine.Random.value * (1f - (mass / 100f)) > .5f)
                            {
                                ch.Stagger(direction);
                            }
                            mass *= .02f;                            
                            
                            Vector3 vel = direction * ((15f - distanceFromPlayer) / mass) + new Vector3(0f, Mathf.Clamp(3f / mass, 1f, 5f), 0f);
                            vel *= VL_GlobalConfigs.c_metavokerBonusForceWave;
                            //ZLog.Log("" + ch.m_name + " pushed " + vel);

                            Traverse.Create(root: ch).Field(name: "m_pushForce").SetValue(vel);
                            HitData hitData = new HitData();
                            hitData.m_damage.m_damage = distanceFromPlayer * UnityEngine.Random.Range(.75f, 1.25f) * (1f + (.02f * sLevel)) * VL_GlobalConfigs.c_metavokerBonusForceWave;
                            hitData.m_point = ch.GetEyePoint();
                            hitData.m_dir = direction;
                            hitData.m_skill = ValheimLegends.EvocationSkill;
                            ch.Damage(hitData);
                        }                            
                    }
                }
            }
        }

        public static void Process_Input(Player player, ref float altitude, ref Rigidbody playerBody)
        {

            if (player.IsBlocking() && ZInput.GetButtonDown("Attack"))
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability2_CD") && player.GetStamina() >= VL_Utility.GetForceWaveCost)
                {
                    //Ability Cooldown
                    StatusEffect se_cd = (SE_Ability2_CD)ScriptableObject.CreateInstance(typeof(SE_Ability2_CD));
                    se_cd.m_ttl = VL_Utility.GetForceWaveCooldown;
                    player.GetSEMan().AddStatusEffect(se_cd);

                    //Ability Cost
                    player.UseStamina(VL_Utility.GetForceWaveCost);

                    VL_Utility.RotatePlayerToTarget(player);
                    ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).StopAllCoroutines();
                    ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("battleaxe_attack2");

                    ValheimLegends.isChargingDash = true;
                    ValheimLegends.dashCounter = 0;
                    QueuedAttack = MetavokerAttackType.ForceWave;

                    //knife_secondary
                    //sword_secondary
                    //swing_longsword2
                    //knife_stab2
                    //throw_bomb

                    //Skill gain
                    player.RaiseSkill(ValheimLegends.EvocationSkill, VL_Utility.GetForceWaveSkillGain);
                }
            }

            if (P_Light != null)
            {
                P_Light.transform.position = player.GetEyePoint() + player.transform.up * .4f + player.transform.right * -.8f;
            }

            if(ZInput.GetButton("Jump"))
            {
                if (!player.IsOnGround() && !player.IsDead() && !player.InAttack() && !player.IsEncumbered() && !player.InDodge() && !player.IsKnockedBack())
                {
                    //Rigidbody playerBody = Traverse.Create(root: player).Field(name: "m_body").GetValue<Rigidbody>();
                    Vector3 velocity = playerBody.velocity;
                    if (velocity.y < 0)
                    {
                        bool flag = true;
                        if (!player.HaveStamina(1f))
                        {
                            if (player.IsPlayer())
                            {
                                Hud.instance.StaminaBarEmptyFlash();
                            }
                            flag = false;
                        }
                        if (flag)
                        {
                            player.UseStamina(.6f * VL_GlobalConfigs.c_metavokerBonusSafeFallCost);
                            //ZSyncAnimation zanim = Traverse.Create(root: player).Field(name: "m_zanim").GetValue<ZSyncAnimation>();
                            //Animator anim = Traverse.Create(root: player).Field(name: "m_animator").GetValue<Animator>();
                            RaycastHit hitInfo = default(RaycastHit);
                            Vector3 position = player.transform.position;
                            Vector3 target = (!Physics.Raycast(position, new Vector3(0f, -1f, 0f), out hitInfo, float.PositiveInfinity, SafeFall_Layermask) || !(bool)hitInfo.collider) ? (position + player.transform.up * -1000f) : hitInfo.point;
                            //float heightAboveGround = ZoneSystem.instance.GetSolidHeight(player.transform.position);
                            float groundHeight = hitInfo.point.y;
                            
                            float maxHeightAboveGround = altitude - groundHeight;
                            //ZLog.Log("player height: " + player.transform.position.y + " max altitude " + altitude + " ground level " + hitInfo.point + " height above ground " + groundHeight + " height diff " + maxHeightAboveGround);
                            float _vy = Mathf.Clamp(-.15f * velocity.y, 0f, 1.5f);
                            float v_r = _vy / (-velocity.y);
                            //ZLog.Log("v_r " + v_r);
                            float alt_r = maxHeightAboveGround * v_r;
                            //ZLog.Log("adjusting " + velocity.y + " by " + _vy);
                            playerBody.velocity = velocity + new Vector3(0f, _vy, 0f);
                            //ZLog.Log("velocity y " + _vy + " adjusted to " + velocity.y);
                            //ZLog.Log("max height " + altitude + " adjusted to " + (altitude - alt_r));
                            altitude = (altitude - alt_r);
                            //player.StartEmote("sit");
                            //RuntimeAnimatorController PlayerRAC;
                            //PlayerRAC = player.gameObject.GetComponentInChildren<Animator>().runtimeAnimatorController;
                            //AnimationClipOverrides animationClipOverrides = new AnimationClipOverrides(PlayerRAC.animationClips.Length);
                            //AnimatorOverrideController PlayerFlyingAOC = new AnimatorOverrideController(PlayerRAC);
                            //PlayerFlyingAOC.GetOverrides(animationClipOverrides);
                            //AnimationClip[] animationClips = PlayerRAC.animationClips;
                            //foreach (AnimationClip animationClip in animationClips)
                            //{
                            //    if (animationClip.name == "jump")
                            //    {
                            //        animationClipOverrides[animationClip.name] = ValheimLegends.anim_player_float;
                            //    }
                            //}
                            //PlayerFlyingAOC.ApplyOverrides(animationClipOverrides);
                            //anim.SetTrigger("pre_landing");
                            UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ReverseLightburst"), player.transform.position, Quaternion.LookRotation(new Vector3(0f, 1f, 0f)));
                        }
                    }
                }
            }

            if (VL_Utility.Ability3_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
                {
                    ValheimLegends.shouldUseGuardianPower = false;
                    //player.Message(MessageHud.MessageType.Center, "root - starting");
                    if (player.GetStamina() >= VL_Utility.GetWarpCost && !ValheimLegends.isChanneling)
                    {
                        
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability3_CD)ScriptableObject.CreateInstance(typeof(SE_Ability3_CD));
                        se_cd.m_ttl = VL_Utility.GetWarpCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetWarpCost);

                        //Effects, animations, and sounds
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("gpower");
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetSpeed(.3f);
                        VL_Utility.RotatePlayerToTarget(player);
                        player.StartEmote("point");
                        ValheimLegends.isChanneling = true;
                        
                        //Lingering effects

                        //Skill influence

                        //Apply effects
                        warpDistance = 15f;
                        warpGrowthTrigger = 10;

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.EvocationSkill, VL_Utility.GetWarpSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to initiate Warp: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetWarpCost + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
            else if (VL_Utility.Ability3_Input_Pressed && player.GetStamina() > VL_Utility.GetWarpCostPerUpdate && ValheimLegends.isChanneling && Mathf.Max(0f, altitude - player.transform.position.y) <= 2f)
            {
                VL_Utility.SetTimer();
                warpCount++;
                player.UseStamina(VL_Utility.GetWarpCostPerUpdate);
                //player.transform.rotation = Quaternion.LookRotation(player.GetLookDir());
                ValheimLegends.isChanneling = true;
                if (warpCount >= warpGrowthTrigger)
                {
                    warpCount = 0;
                    //Skill gain
                    UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ParticleLightSuction"), player.transform.position, Quaternion.identity);                    
                    player.RaiseSkill(ValheimLegends.EvocationSkill, .06f);
                    warpDistance += 5f; 
                }
            }
            else if (((VL_Utility.Ability3_Input_Up || player.GetStamina() <= VL_Utility.GetWarpCostPerUpdate || player.GetStamina() <= 2f) && ValheimLegends.isChanneling))// || Mathf.Max(0f, altitude - player.transform.position.y) > 2f)
            {
                //player.Message(MessageHud.MessageType.Center, "root - deactivate");
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.EvocationSkillDef).m_level;
                warpDistance = warpDistance * (1f + (.01f * sLevel)) * VL_GlobalConfigs.c_metavokerWarpDistance;
                //ZLog.Log("triggering warp with  distance of " + warpDistance);
                ValheimLegends.isChanneling = false;
                RaycastHit hitInfo = default(RaycastHit);
                //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("unarmed_attack0");
                Vector3 position = player.GetEyePoint();
                Vector3 target = (!Physics.Raycast(player.GetEyePoint(), player.GetLookDir(), out hitInfo, float.PositiveInfinity, Warp_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;

                Vector3 a = Vector3.MoveTowards(position, target, 1f);
                float distanceMagnitude = (hitInfo.point - position).magnitude;
                float warpMagnitude = (warpDistance * player.GetLookDir()).magnitude;
                //ZLog.Log("distance mag: " + distanceMagnitude + " warp mag: " + warpMagnitude);
                //ZLog.Log("hitinfo distance " + hitInfo.distance);
                float flagDamage = 0f;
                if(warpMagnitude > distanceMagnitude)
                {
                    flagDamage = warpMagnitude - distanceMagnitude;
                    warpMagnitude = distanceMagnitude;
                }
                bool flagLoadWarp = warpMagnitude >= 140 ? true : false;

                Vector3 moveVec = Vector3.MoveTowards(player.transform.position, target, (float)warpMagnitude);
                //moveVec.y = ((ZoneSystem.instance.GetSolidHeight(moveVec) - ZoneSystem.instance.GetGroundHeight(moveVec) <= 1f) ? ZoneSystem.instance.GetSolidHeight(moveVec) : ZoneSystem.instance.GetGroundHeight(moveVec));
                Vector3 effectVec = (moveVec + (player.GetLookDir() * -10f));
                
                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ParticleLightburst"), player.GetEyePoint(), Quaternion.LookRotation(player.GetLookDir()));
                if (warpMagnitude > 0f)
                {
                    //ZLog.Log("damage magnitude is " + flagDamage);

                    //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_eikthyr_forwardshockwave"), effectVec, Quaternion.LookRotation(player.GetLookDir()));
                    UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ForwardLightningShock"), effectVec, Quaternion.LookRotation(player.GetLookDir()));
                    //Apply effects
                    List<Character> allCharacters = new List<Character>();
                    allCharacters.Clear();
                    Character.GetCharactersInRange(moveVec, 8f + (.02f * sLevel), allCharacters);
                    bool anyHitFlag = false;
                    foreach (Character ch in allCharacters)
                    {
                        if (BaseAI.IsEnemy(player, ch) && VL_Utility.LOS_IsValid(ch, player.transform.position))
                        {
                            Vector3 direction = (ch.transform.position - player.transform.position);
                            HitData hitData = new HitData();
                            hitData.m_damage.m_lightning = UnityEngine.Random.Range(flagDamage * (sLevel/15f), flagDamage * (sLevel/10f)) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_metavokerWarpDamage;
                            hitData.m_pushForce = (flagDamage + sLevel) * .1f;
                            hitData.m_point = ch.GetEyePoint();
                            hitData.m_dir = (ch.transform.position - player.transform.position);
                            hitData.m_skill = ValheimLegends.EvocationSkill;
                            ch.Damage(hitData);
                            anyHitFlag = true;
                        }
                    }
                    if(!anyHitFlag && !flagLoadWarp)
                    {
                        float stamReturnAmount = flagDamage * 1.5f;
                        player.AddStamina(stamReturnAmount);
                    }
                }

                if (flagLoadWarp)
                {
                    bool flagFarWarp = warpMagnitude >= 200 ? true : false;
                    if (flagFarWarp)
                    {
                        player.TeleportTo(moveVec, player.transform.rotation, true);
                    }
                    else
                    {
                        player.TeleportTo(moveVec, player.transform.rotation, false);
                    }
                }
                else
                {
                    //ZLog.Log("zone loaded?" + ZoneSystem.instance.IsZoneLoaded(moveVec));
                    player.transform.position = moveVec;
                }
                
                
                //player.TeleportTo(moveVec, player.transform.rotation, false);
                altitude = 0f;
                
            }
            else if(VL_Utility.Ability2_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability2_CD"))
                {
                    //player.Message(MessageHud.MessageType.Center, "Plant defenders");
                    if (player.GetStamina() >= VL_Utility.GetReplicaCost)
                    {
                        Vector3 lookVec = player.GetLookDir();
                        lookVec.y = 0f;
                        player.transform.rotation = Quaternion.LookRotation(lookVec);

                        ValheimLegends.shouldUseGuardianPower = false;
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability2_CD)ScriptableObject.CreateInstance(typeof(SE_Ability2_CD));
                        se_cd.m_ttl = VL_Utility.GetReplicaCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetReplicaCost);

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.IllusionSkillDef).m_level;

                        //Effects, animations, and sounds
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("gpower");
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetSpeed(.7f);
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_Replica"), player.transform.position + player.transform.up *.6f, Quaternion.identity);

                        //Lingering effects

                        //Apply effects

                        //Apply effects
                        List<Character> allCharacters = new List<Character>();
                        foreach(Character chr in Character.GetAllCharacters())
                        {
                            if (!chr.IsBoss())
                            {
                                allCharacters.Add(chr);
                            }
                        }
                        for(int i = 0; i < allCharacters.Count; i++)
                        {
                            Character ch = allCharacters[i];
                            if ((BaseAI.IsEnemy(player, ch) && (ch.transform.position - player.transform.position).magnitude <= 18f + (.05f * sLevel)))
                            {
                                string name = ch.name.Substring(0, ch.name.IndexOf('('));
                                GameObject original = ZNetScene.instance.GetPrefab(name);
                                if (original != null)
                                {
                                    if (original.GetComponent<CharacterTimedDestruction>() == null)
                                    {
                                        original.AddComponent<CharacterTimedDestruction>();
                                    }
                                    original.GetComponent<CharacterTimedDestruction>().m_timeoutMin = 8f + (.2f * sLevel);
                                    original.GetComponent<CharacterTimedDestruction>().m_timeoutMax = 8f + (.2f * sLevel); 
                                    Vector3 rootVec = ch.transform.position;
                                    rootVec.x += (5f * UnityEngine.Random.Range(-1f, 1f));
                                    GameObject replica = UnityEngine.Object.Instantiate(original, rootVec, Quaternion.Inverse(ch.transform.rotation));
                                    CharacterTimedDestruction td = replica.GetComponent<CharacterTimedDestruction>();
                                    if (td != null)
                                    {
                                        //ZLog.Log("td valid: " + td.isActiveAndEnabled + " timeout min " + td.m_timeoutMin + " timeout max " + td.m_timeoutMax);                                      
                                        td.m_timeoutMin = 8f + (.2f * sLevel);
                                        td.m_timeoutMax = td.m_timeoutMin;
                                        td.Trigger();
                                    }
                                    Character repCh = replica.GetComponent<Character>();
                                    repCh.SetMaxHealth(1f + sLevel);
                                    repCh.transform.localScale = (0.8f) * Vector3.one;
                                    SE_Companion se_companion = (SE_Companion)ScriptableObject.CreateInstance(typeof(SE_Companion));
                                    se_companion.m_ttl = 8f + (.2f * sLevel);
                                    se_companion.damageModifier = .05f + (.0075f * sLevel) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_metavokerReplica;
                                    se_companion.summoner = player;
                                    repCh.GetSEMan().AddStatusEffect(se_companion);
                                    repCh.m_faction = Character.Faction.Players;
                                    CharacterDrop comp = repCh.GetComponent<CharacterDrop>();
                                    if (comp != null)
                                    {
                                        comp.m_drops.Clear();
                                    }
                                    repCh.name = "VL_" + repCh.name;
                                    repCh.m_name = "(" + repCh.m_name + ")";
                                    UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ReplicaCreate"), repCh.transform.position + repCh.transform.up * .2f, Quaternion.identity);
                                }
                                else
                                {
                                    //ZLog.Log("replica failed for " + ch.name);
                                }
                                //replica.SetTamed(true);
                            }
                        }

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.IllusionSkill, VL_Utility.GetReplicaSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to create illusions: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetReplicaCost + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
            else if (VL_Utility.Ability1_Input_Down)
            {                
                if(P_Light != null && (P_Light.transform.position - player.GetEyePoint()).magnitude < 2f)
                {
                    float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.IllusionSkillDef).m_level;
                    P_Light.m_ttl = .05f;
                    
                    HitData hitData = new HitData();
                    hitData.m_skill = ValheimLegends.EvocationSkill;
                    //P_Light.Setup(player, new Vector3(0, -1000, 0), -1, hitData, null);
                    //Traverse.Create(root: P_Light).Field("m_skill").SetValue(ValheimLegends.IllusionSkill);
                    //UnityEngine.Object.Destroy(P_Light.gameObject);
                    //if (P_Light != null)
                    //{
                    //    P_Light = null;
                    //}

                    Vector3 vector = player.GetEyePoint() + player.transform.up * .5f + player.transform.right * -1f;
                    GameObject prefab = ZNetScene.instance.GetPrefab("VL_Light");
                    GameObject GO_LL = UnityEngine.Object.Instantiate(prefab, vector, Quaternion.identity);
                    Projectile P_LL = GO_LL.GetComponent<Projectile>();
                    P_LL.m_respawnItemOnHit = false;
                    P_LL.m_spawnOnHit = null;
                    P_LL.m_ttl = 5f;
                    P_LL.m_gravity = .25f;
                    P_LL.m_rayRadius = .1f;
                    P_LL.m_aoe = 4f + (.04f * sLevel);
                    P_LL.transform.localRotation = Quaternion.LookRotation(player.GetAimDir(vector));
                    //P_Light.m_respawnItemOnHit = false;
                    //P_Light.m_spawnOnHit = null;
                    //P_Light.m_ttl = 3f;
                    //P_Light.m_gravity = .1f;
                    //P_Light.m_rayRadius = .1f;
                    //P_Light.transform.localRotation = Quaternion.LookRotation(player.GetAimDir(vector));
                    GO_LL.transform.localScale = Vector3.zero;

                    RaycastHit hitInfo = default(RaycastHit);
                    Vector3 position = player.transform.position;
                    Vector3 target = (!Physics.Raycast(player.GetEyePoint(), player.GetLookDir(), out hitInfo, float.PositiveInfinity, Light_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                    hitData.m_damage.m_lightning = UnityEngine.Random.Range(5f + (.3f * sLevel), 10f + (.6f*sLevel)) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_meteavokerLight;
                    hitData.m_damage.m_pierce = UnityEngine.Random.Range(5f + (.3f * sLevel), 10f + (.6f*sLevel)) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_meteavokerLight;
                    hitData.m_pushForce = (100f + 2*sLevel) * VL_GlobalConfigs.c_meteavokerLight;
                    hitData.SetAttacker(player);
                    Vector3 a = Vector3.MoveTowards(GO_LL.transform.position, target, 1f);
                    P_LL.Setup(player, (a - GO_LL.transform.position) * 80f, -1f, hitData, null, null);
                    Traverse.Create(root: P_LL).Field("m_skill").SetValue(ValheimLegends.IllusionSkill);
                    //P_Light.Setup(player, (a - GO_Light.transform.position) * 80f, -1f, hitData, null);
                    //Traverse.Create(root: P_Light).Field("m_skill").SetValue(ValheimLegends.IllusionSkill);
                    GO_LL = null;
                    GO_Light = null;

                }
                else if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability1_CD"))
                {
                    //player.Message(MessageHud.MessageType.Center, "Light");
                    if (player.GetStamina() >= VL_Utility.GetLightCost)
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability1_CD)ScriptableObject.CreateInstance(typeof(SE_Ability1_CD));
                        se_cd.m_ttl = VL_Utility.GetLightCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetLightCost);

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.IllusionSkillDef).m_level;

                        //Effects, animations, and sounds
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("gpower");
                        player.StartEmote("cheer");
                        //GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_guardstone_permitted_add"), player.GetCenterPoint(), Quaternion.identity);

                        //Lingering effects
                        VL_Utility.SetTimer();

                        //Apply effects

                        Vector3 vector = player.GetEyePoint() + player.transform.up * .4f + player.transform.right * -.8f;
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_Lightburst"), vector, Quaternion.identity);
                        GameObject prefab = ZNetScene.instance.GetPrefab("VL_Light");
                        GO_Light = UnityEngine.Object.Instantiate(prefab, vector, Quaternion.identity);
                        P_Light = GO_Light.GetComponent<Projectile>();
                        P_Light.m_respawnItemOnHit = false;
                        P_Light.m_spawnOnHit = null;
                        P_Light.m_ttl = 300f;
                        P_Light.m_gravity = 0f;
                        P_Light.m_rayRadius = .1f;
                        P_Light.transform.localRotation = Quaternion.LookRotation(player.GetAimDir(vector));
                        GO_Light.transform.localScale = Vector3.zero;

                        RaycastHit hitInfo = default(RaycastHit);
                        Vector3 position = player.transform.position;
                        Vector3 target = (!Physics.Raycast(vector, player.GetLookDir(), out hitInfo, float.PositiveInfinity, Light_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                        HitData hitData = new HitData();
                        //hitData.m_damage.m_fire = UnityEngine.Random.Range(10f + (2f * sLevel), 40f + (2f * sLevel)) * VL_GlobalConfigs.g_DamageModifer;
                        //hitData.m_damage.m_blunt = UnityEngine.Random.Range(5f + (1f * sLevel), 20f + (1f * sLevel)) * VL_GlobalConfigs.g_DamageModifer;
                        //hitData.m_pushForce = 2f;
                        hitData.m_skill = ValheimLegends.EvocationSkill;
                        Vector3 a = Vector3.MoveTowards(GO_Light.transform.position, target, 1f);                        
                        P_Light.Setup(player, Vector3.zero, -1f, hitData, null, null);
                        Traverse.Create(root: P_Light).Field("m_skill").SetValue(ValheimLegends.IllusionSkill);
                        //GO_Light = null;

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.IllusionSkill, VL_Utility.GetLightSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to for Light: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetLightCost + ")");
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

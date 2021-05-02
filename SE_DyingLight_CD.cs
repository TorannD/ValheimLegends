using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class SE_DyingLight_CD : StatusEffect
    {
        public static Sprite AbilityIcon = ZNetScene.instance.GetPrefab("TrophySkeleton").GetComponent<ItemDrop>().m_itemData.GetIcon();
        public static GameObject GO_SEFX;

        public SE_DyingLight_CD()
        {
            base.name = "SE_VL_DyingLight_CD";
            m_icon = ZNetScene.instance.GetPrefab("TrophySkeleton").GetComponent<ItemDrop>().m_itemData.GetIcon();
            m_tooltip = "Dying Light has prevented a killing blow. Dying Light will not trigger again until this cooldown expires.";
            m_name = "Dying Light";           
        }

        public override bool CanAdd(Character character)
        {
            return character.IsPlayer();
        }
    }
}

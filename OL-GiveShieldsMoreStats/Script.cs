using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Reflection;

namespace ItemMods
{
    public class Script : MonoBehaviour
    {
        public ItemMod itemMod;

        public Character localPlayer;

        public void Initialise()
        {
            // Get the CharacterEquipment hooks
            On.CharacterEquipment.GetEquipmentDamageResistance += new On.CharacterEquipment.hook_GetEquipmentDamageResistance(GetResistanceHook);
            On.CharacterEquipment.GetEquipmentDamageProtection += new On.CharacterEquipment.hook_GetEquipmentDamageProtection(GetProtectionHook);

            // display extra stats
            On.Weapon.InitDisplayedStats += new On.Weapon.hook_InitDisplayedStats(WeaponStatDisplayHook);
        }

        public float GetResistanceHook(On.CharacterEquipment.orig_GetEquipmentDamageResistance orig, CharacterEquipment self, DamageType.Types _type)
        {
            EquipmentSlot[] m_equipmentSlots = self.EquipmentSlots;
            float num = 0f;
            for (int i = 0; i < m_equipmentSlots.Length; i++)
            {
                if (self.HasItemEquipped(i))
                {
                    Equipment equippedItem = m_equipmentSlots[i].EquippedItem;
                    EquipmentStats equipStats = equippedItem.GetComponent<EquipmentStats>();
                    num += equipStats.GetDamageResistance(_type);
                }
            }
            return num;
        }

        public float GetProtectionHook(On.CharacterEquipment.orig_GetEquipmentDamageProtection orig, CharacterEquipment self, DamageType.Types _type)
        {
            EquipmentSlot[] m_equipmentSlots = self.EquipmentSlots;

            float num = 0f;
            for (int i = 0; i < m_equipmentSlots.Length; i++)
            {
                if (self.HasItemEquipped((EquipmentSlot.EquipmentSlotIDs)i))
                {
                    Equipment equippedItem = m_equipmentSlots[i].EquippedItem;
                    EquipmentStats equipStats = equippedItem.GetComponent<EquipmentStats>();
                    num += equipStats.GetDamageProtection(_type);
                }
            }
            return num;
        }

        private void WeaponStatDisplayHook(On.Weapon.orig_InitDisplayedStats orig, Weapon self)
        {
            Item item = self as Item;
            if (self.Type != Weapon.WeaponType.Shield)
            {
                // if it's a weapon, just call the original method
                orig(self);
            }
            else
            {
                ItemDetailsDisplay.DisplayedInfos[] newDetails = new ItemDetailsDisplay.DisplayedInfos[]
                {
                // add our new items to the list
                ItemDetailsDisplay.DisplayedInfos.DamageResistance,
                ItemDetailsDisplay.DisplayedInfos.DamageModifier,
                ItemDetailsDisplay.DisplayedInfos.DamageProtection,
                // this is the original list
                ItemDetailsDisplay.DisplayedInfos.ImpactResistance,
                ItemDetailsDisplay.DisplayedInfos.ManaCost,
                ItemDetailsDisplay.DisplayedInfos.HeatRegenPenalty,
                ItemDetailsDisplay.DisplayedInfos.StamUsePenalty,
                ItemDetailsDisplay.DisplayedInfos.MovementPenalty,
                ItemDetailsDisplay.DisplayedInfos.Durability
                };
                // use reflection to set the protected value outside the container class
                ReflectionTools.ReflectionSetValue(newDetails, typeof(Item), item, "m_displayedInfos");
            }
        }
    }    
}

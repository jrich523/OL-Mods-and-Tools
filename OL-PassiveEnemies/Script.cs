using System;
using System.IO;
using System.Linq;
using UnityEngine;
using static CustomKeybindings;

namespace PassiveEnemies
{
    public class Script : MonoBehaviour
    {
        public PassiveEnemies StatCheck;

        public bool EnemiesAggressive = false;
        public string AggroToggleKey = "Aggro Toggle";

        public void Initialise()
        {
            Patch();
            CustomKeybindings.AddAction(AggroToggleKey, KeybindingsCategory.Actions, ControlType.Both, 5);
        }

        public void Patch()
        {
            On.AICEnemyDetection.Update += new On.AICEnemyDetection.hook_Update(AIEnemyDetectionHook);
            On.AIESwitchState.SwitchState += new On.AIESwitchState.hook_SwitchState(AISwitchStateHook);
            On.LocalCharacterControl.UpdateInteraction += new On.LocalCharacterControl.hook_UpdateInteraction(LocalCharacterControl_UpdateInteraction);
        }

        public void LocalCharacterControl_UpdateInteraction(On.LocalCharacterControl.orig_UpdateInteraction orig, LocalCharacterControl self)
        {
            orig(self);

            if (self.InputLocked)
            {
                return;
            }

            var localplayerID = self.Character.OwnerPlayerSys.PlayerID;

            if (CustomKeybindings.m_playerInputManager[localplayerID].GetButtonUp(AggroToggleKey))
            {
                EnemiesAggressive = !EnemiesAggressive;
            }

        }

        // disable AI aggression
        private void AIEnemyDetectionHook(On.AICEnemyDetection.orig_Update orig, AICEnemyDetection self)
        {
            if (EnemiesAggressive == true)
            {
                orig(self);
                return;
            }
            return;
        }
        private void AISwitchStateHook(On.AIESwitchState.orig_SwitchState orig, AIESwitchState self)
        {
            if (EnemiesAggressive == true)
            {
                orig(self);
                return;
            }
            return;
        }
    }
}

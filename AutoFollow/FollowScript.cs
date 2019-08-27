using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static CustomKeybindings;

namespace AutoFollow
{
    public class FollowScript : MonoBehaviour
    {
        public List<Character> PlayerCharacters = new List<Character>(); // list of all player characters
        public Dictionary<int, Character> LocalPlayers = new Dictionary<int, Character>(); // list of local characters, and their IDs
        public Dictionary<string, string> CharactersFollowing = new Dictionary<string, string>(); // key: follower UID, value: target UID

        public string FollowKey = "Toggle Auto-Follow";

        public float MinFollowDistance = 1.5f; // minimum distance for autofollow
        public float RotateSpeed = 5f; // speed of camera rotation

        public void Init()
        {
            AddAction(FollowKey, KeybindingsCategory.Actions, ControlType.Both, 5);

            // sprint input hook
            On.ControlsInput.Sprint += new On.ControlsInput.hook_Sprint(SprintHook);
        }

        // update runs once per frame
        internal void Update()
        {
            // if no local characters, return (and clear lists)
            if (CharacterManager.Instance.PlayerCharacters.Count <= 0)
            {
                if (CharactersFollowing.Count > 0)
                {
                    CharactersFollowing.Clear();
                    LocalPlayers.Clear();
                }
                return;
            }

            // update character list on change
            if (CharacterManager.Instance.PlayerCharacters.Count != PlayerCharacters.Count())
            {
                PlayerCharacters.Clear();
                LocalPlayers.Clear();
                int localID = 0;
                foreach (string uid in CharacterManager.Instance.PlayerCharacters.Values)
                {
                    Character c = CharacterManager.Instance.GetCharacter(uid);

                    PlayerCharacters.Add(c);

                    if (c.IsLocalPlayer)
                    {
                        LocalPlayers.Add(localID, c);
                        localID++;
                    }
                }
            }

            // check each local character input every frame for follow input
            foreach (KeyValuePair<int, Character> player in LocalPlayers)
            {
                if (m_playerInputManager[player.Key].GetButtonDown(FollowKey))
                {
                    ToggleFollow(player.Value);
                }
            }
        }

        // toggle the autofollow on and off
        public void ToggleFollow(Character c)
        {
            string uid = c.UID;

            // if currently following, remove us from the following list (breaks the follow function automatically)
            if (CharactersFollowing.ContainsKey(uid))
            {
                CharactersFollowing.Remove(uid);
            }
            else // otherwise, toggle it on
            {
                // find closest player character and follow it
                float currentLowest = -1;
                Character newTarget = null;

                // for each player character who's UID is not this character's UID
                foreach (Character c2 in PlayerCharacters.Where(x => x.UID != uid))
                {
                    if (currentLowest == -1 || Vector3.Distance(c2.transform.position, c.transform.position) < currentLowest)
                    {
                        newTarget = c2;
                        currentLowest = Vector3.Distance(c2.transform.position, c.transform.position);
                    }
                }

                if (newTarget && currentLowest > 0)
                {
                    CharactersFollowing.Add(uid, newTarget.UID);
                    StartCoroutine(FollowTarget(c, newTarget));
                }
            }
        }

        // follow target coroutine. runs until the UID is removed from the CharactersFollowing list.
        public IEnumerator FollowTarget(Character c, Character target)
        {
            // get the autoRun private field (bool)
            FieldInfo autoRun = c.CharacterControl.GetType().GetField("m_autoRun", BindingFlags.Instance | BindingFlags.NonPublic);

            while (CharactersFollowing.ContainsKey(c.UID))
            {
                // null check for player and target
                if (!c || !target)
                {
                    CharactersFollowing.Remove(c.UID);
                    break;
                }

                // check distance and handle autorun
                float distance = Vector3.Distance(c.transform.position, target.transform.position);

                if (distance > MinFollowDistance)
                {
                    autoRun.SetValue(c.CharacterControl, true);

                    if (target.Sprinting && !c.Sprinting)
                    {
                        FieldInfo m_speedModifier = typeof(CharacterStats).GetField("m_speedModifier", BindingFlags.NonPublic | BindingFlags.Instance);
                        Stat curModif = m_speedModifier.GetValue(c.Stats) as Stat;


                    }
                }
                else
                {
                    autoRun.SetValue(c.CharacterControl, false);
                }

                // rotate the camera to follow the target
                var targetRot = Quaternion.LookRotation(target.transform.position - c.transform.position);
                c.CharacterCamera.transform.rotation = Quaternion.Lerp(c.CharacterCamera.transform.rotation, targetRot, Mathf.Min(RotateSpeed * Time.deltaTime, 1));

                yield return null;
            }

            // force stop autorun on exit
            if (c) { autoRun.SetValue(c.CharacterControl, false); }
        }

        // try sprint if target is sprinting
        public bool SprintHook(On.ControlsInput.orig_Sprint orig, int _playerID)
        {
            // get the UID of this local _playerID, and see if it is currently following anything
            if (CharactersFollowing.ContainsKey(LocalPlayers[_playerID].UID))
            {
                // get the target character from our CharactersFollowing dictionary
                Character target = CharacterManager.Instance.GetCharacter(CharactersFollowing[LocalPlayers[_playerID].UID]);
                if (target)
                {
                    return target.Sprinting; // if target sprints, we sprint
                }
            }

            // fallback to orig function. its static, so no need for orig(self, _playerID), just orig(_playerID)
            return orig(_playerID);
        }
    }
}

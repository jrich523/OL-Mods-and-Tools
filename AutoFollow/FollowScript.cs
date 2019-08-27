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
        public List<string> CharactersFollowing = new List<string>(); // list of UID strings for characters currently following another player

        public string FollowKey = "Toggle Auto-Follow";

        public float MinFollowDistance = 1.5f;

        public void Init()
        {
            AddAction(FollowKey, KeybindingsCategory.Actions, ControlType.Both, 5);
        }

        // update runs once per frame
        internal void Update()
        {
            // if no local characters, return (and clear list)
            if (CharacterManager.Instance.PlayerCharacters.Count <= 0)
            {
                if (CharactersFollowing.Count > 0) { CharactersFollowing.Clear(); }
                return;
            }

            // update character list on change
            if (CharacterManager.Instance.PlayerCharacters.Count != PlayerCharacters.Count())
            {
                PlayerCharacters.Clear();
                foreach (string uid in CharacterManager.Instance.PlayerCharacters.Values)
                {
                    PlayerCharacters.Add(CharacterManager.Instance.GetCharacter(uid));
                }
            }

            // check each local character input every frame for follow input
            int ID = 0;
            foreach (Character c in PlayerCharacters.Where(c => c.IsLocalPlayer))
            {
                if (m_playerInputManager[ID].GetButtonDown(FollowKey))
                {
                    ToggleFollow(c);
                }

                ID++; // increase 1 to the ID count every loop. the order of the PlayerCharacters list will be the same ID order for the playerInputManager
            }
        }

        // toggle the autofollow on and off
        public void ToggleFollow(Character c)
        {
            string uid = c.UID;

            // if currently following, remove us from the following list (breaks the follow function automatically)
            if (CharactersFollowing.Contains(uid))
            {
                CharactersFollowing.Remove(uid);
            }
            else
            {
                // find closest player character and follow it.
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
                    CharactersFollowing.Add(uid);
                    StartCoroutine(FollowTarget(c, newTarget));
                }
            }
        }

        // follow target coroutine. runs until the UID is removed from the CharactersFollowing list.
        public IEnumerator FollowTarget(Character c, Character target)
        {
            // get the autoRun private field (bool)
            var autoRun = c.CharacterControl.GetType().GetField("m_autoRun", BindingFlags.Instance | BindingFlags.NonPublic);

            while (CharactersFollowing.Contains(c.UID))
            {
                // null check for player and target
                if (!c || !target)
                {
                    CharactersFollowing.Remove(c.UID);
                    break;
                }

                // check distance and handle autorun
                float distance = Vector3.Distance(c.transform.position, target.transform.position);

                if (distance > MinFollowDistance) { autoRun.SetValue(c.CharacterControl, true); }
                else { autoRun.SetValue(c.CharacterControl, false); }

                // rotate the camera to follow the target
                var targetRot = Quaternion.LookRotation(target.transform.position - c.transform.position);
                c.CharacterCamera.transform.rotation = Quaternion.Lerp(c.CharacterCamera.transform.rotation, targetRot, Mathf.Min(5f * Time.deltaTime, 1));

                yield return null;
            }

            // force stop autorun on exit
            if (c) { autoRun.SetValue(c.CharacterControl, false); }
        }
    }
}

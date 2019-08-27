using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
//using OModAPI;
using static CustomKeybindings;

namespace AutoFollow
{
    public class FollowScript : MonoBehaviour
    {
        public List<Character> PlayerCharacters = new List<Character>();

        // list of UID strings for characters currently following another player
        public List<string> CharactersFollowing = new List<string>();

        public string FollowKey = "Toggle Auto-Follow";

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

                ID++; // increase 1 to the ID count every loop. each local character will be in the same order of the PlayerCharacters list and their ID number.
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
                foreach (string uid2 in CharacterManager.Instance.PlayerCharacters.Values.Where(x => x != uid))
                {
                    if (CharacterManager.Instance.GetCharacter(uid2) is Character c2)
                    {
                        if (currentLowest == -1 || Vector3.Distance(c2.transform.position, c.transform.position) < currentLowest)
                        {
                            newTarget = c2;
                            currentLowest = Vector3.Distance(c2.transform.position, c.transform.position);
                        }
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

                if (distance > 1) { autoRun.SetValue(c.CharacterControl, true); }
                else              { autoRun.SetValue(c.CharacterControl, false); }

                // player and camera rotation:

                // get look rotation (target - self)
                var targetRot = Quaternion.LookRotation(target.transform.position - c.transform.position);

                // set rotation speed per delta time (time of last frame)
                var str = Mathf.Min(5f * Time.deltaTime, 1);

                // never want to rotate Y or Z axis of the character, set them to 0
                Quaternion fix = new Quaternion(targetRot.x, 0, 0, targetRot.w); 

                // rotate the player smoothly
                c.transform.rotation = Quaternion.Lerp(c.transform.rotation, fix, str); 

                // rotate camera too (but dont use the Z axis fix, use actual target rotation)
                c.CharacterCamera.transform.rotation = Quaternion.Lerp(c.transform.rotation, targetRot, str); 

                yield return null;
            }

            // force stop autorun on exit
            if (c) { autoRun.SetValue(c.CharacterControl, false); } 
        }
    }
}

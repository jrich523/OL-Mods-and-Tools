using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using OModAPI;
using static CustomKeybindings;

namespace AutoFollow
{
    public class FollowScript : MonoBehaviour
    {
        public List<string> CharactersFollowing = new List<string>();

        public string FollowKey = "Follow Co-Op Partner";

        public void Init()
        {
            AddAction(FollowKey, KeybindingsCategory.Actions, ControlType.Both, 5);
        }

        internal void Update()
        {
            if (CharacterManager.Instance.PlayerCharacters.Count <= 0)
            {
                if (CharactersFollowing.Count > 0) { CharactersFollowing.Clear(); }
                return;
            }

            int ID = 0;
            foreach (string uid in CharacterManager.Instance.PlayerCharacters.Values)
            {
                Character c1 = CharacterManager.Instance.GetCharacter(uid);
                if (c1 && c1.IsPhotonPlayerLocal)
                {
                    if (m_playerInputManager[ID].GetButtonDown(FollowKey))
                    {
                        if (CharactersFollowing.Contains(uid))
                        {
                            CharactersFollowing.Remove(uid);
                        }
                        else
                        {
                            CharactersFollowing.Add(uid);
                            foreach (string uid2 in CharacterManager.Instance.PlayerCharacters.Values.Where(x => x != uid))
                            {
                                Character c2 = CharacterManager.Instance.GetCharacter(uid2);

                                if (c2) { StartCoroutine(FollowTarget(c1, c2, ID)); }
                            }
                        }
                    }

                    ID++;
                }
            }
        }


        public IEnumerator FollowTarget(Character c, Character target, int localID)
        {
            var autoRun = c.CharacterControl.GetType().GetField("m_autoRun", BindingFlags.Instance | BindingFlags.NonPublic);

            while (CharactersFollowing.Contains(c.UID))
            {
                float distance = Vector3.Distance(c.transform.position, target.transform.position);

                if (distance > 1)
                {
                    autoRun.SetValue(c.CharacterControl, true);

                    if (distance > 5)
                    {
                        ControlsInput.Sprint(localID);
                    }
                }
                else
                {
                    autoRun.SetValue(c.CharacterControl, false);
                }

                var targetRot = Quaternion.LookRotation(target.transform.position - c.transform.position);
                var str = Mathf.Min(10f * Time.deltaTime, 1);

                Quaternion fix = new Quaternion(targetRot.x, targetRot.y, 0, targetRot.w); // dont want to rotate Z axis

                c.transform.rotation = Quaternion.Lerp(c.transform.rotation, fix, str);

                c.CharacterCamera.transform.rotation = Quaternion.Lerp(c.transform.rotation, targetRot, str);

                yield return null;
            }

            autoRun.SetValue(c.CharacterControl, false);
        }
    }
}

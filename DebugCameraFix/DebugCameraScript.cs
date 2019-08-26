using System;
using System.Reflection;
using UnityEngine;

namespace DebugCameraFix
{
    public class DebugCameraScript : MonoBehaviour
    {
        public int localPlayerID = -1;
        public bool freeCameraFlag = true;

        public void Initialise()
        {
            On.VideoCamera.Update += new On.VideoCamera.hook_Update(MouseFixHook);
        }

        private void MouseFixHook(On.VideoCamera.orig_Update orig, VideoCamera self)
        {
            orig(self);

            var m_active = GetValue(typeof(VideoCamera), self, "m_active");
            var m_character = GetValue(typeof(VideoCamera), self, "m_character");

            if (m_active is bool active && m_character is Character character)
            {
                if (active == false && character != null)
                {
                    if (freeCameraFlag && localPlayerID == -1)
                    {
                        localPlayerID = ControlsInput.GetMouseOwner();
                    }
                    if (!freeCameraFlag)
                    {
                        freeCameraFlag = true;
                        Cursor.lockState = CursorLockMode.Locked;
                        if (localPlayerID != -1)
                        {
                            ControlsInput.AssignMouseKeyboardToPlayer(localPlayerID);
                        }
                    }
                }
                if (active == true && freeCameraFlag == true)
                {
                    freeCameraFlag = false;
                }
            }
        }

        public static void SetValue<T>(T value, Type type, object obj, string field)
        {
            FieldInfo fieldInfo = type.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(obj, value);
        }

        public static object GetValue(Type type, object obj, string value)
        {
            FieldInfo fieldInfo = type.GetField(value, BindingFlags.NonPublic | BindingFlags.Instance);
            return fieldInfo.GetValue(obj);
        }
    }
}

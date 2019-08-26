using Partiality.Modloader;
using UnityEngine;
//using OModAPI;

namespace DebugCameraFix
{
    public class DebugCameraMod : PartialityMod
    {
        public static GameObject _obj = null;
        public static DebugCameraScript camScript;

        public DebugCameraMod()
        {
            this.ModID = "Debug Cam Fix";
            this.Version = "1.0";
            this.author = "Sinai";
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (_obj == null)
            {
                _obj = new GameObject("Debug_Camera_Fix");
                GameObject.DontDestroyOnLoad(_obj);
            }

            camScript = _obj.AddComponent<DebugCameraScript>();
            camScript.Initialise();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }
    }
}

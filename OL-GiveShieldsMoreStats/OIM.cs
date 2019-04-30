using System;
using UnityEngine;
using Partiality.Modloader;
using On;

namespace ItemMods
{
    public class ItemMod : PartialityMod
    {
        public static Script script;

        public ItemMod()
        {
            this.ModID = "GiveShieldsMoreStats";
            this.Version = "1";
            this.author = "Outlander";
        }

        public override void Init()
        {
            base.Init();
        }

        public override void OnLoad()
        {
            base.OnLoad();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            GameObject go = new GameObject();
            script = go.AddComponent<Script>();

            script.itemMod = this;

            script.Initialise();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }
    }
}


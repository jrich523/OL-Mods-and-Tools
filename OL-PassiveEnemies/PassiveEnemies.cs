using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PassiveEnemies
{
    public class PassiveEnemies : PartialityMod
    {
        public static Script script;

        public PassiveEnemies()
        {
            this.ModID = "Passive Enemies";
            this.Version = "1";
            this.author = "Outlander";
        }
        
        public override void OnEnable()
        {
            base.OnEnable();

            GameObject go = new GameObject();
            script = go.AddComponent<Script>();

            script.StatCheck = this;

            script.Initialise();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }
    }    
}

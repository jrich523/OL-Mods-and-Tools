using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PassiveEnemies
{
    public class Script : MonoBehaviour
    {
        public PassiveEnemies StatCheck;

        public void Initialise()
        {
            Patch();
        }

        public void Patch()
        {
            On.AICEnemyDetection.Update += new On.AICEnemyDetection.hook_Update(AIUpdateHook);
        }        

        private void AIUpdateHook(On.AICEnemyDetection.orig_Update orig, AICEnemyDetection self)
        {
            return;
        }

    }
}

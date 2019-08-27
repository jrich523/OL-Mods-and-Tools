using Partiality.Modloader;
using UnityEngine;

namespace LearnAllRecipes
{
    public class LearnAllRecipesMod : PartialityMod
    {
        public static GameObject _obj = null;
        public static RecipeScript recipeScript;

        public LearnAllRecipesMod()
        {
            this.ModID = "Recipe Learner";
            this.Version = "1.0";
            this.author = "Sinai";
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (_obj == null)
            {
                _obj = new GameObject("RECIPE_LEARNER");
                GameObject.DontDestroyOnLoad(_obj);
            }

            recipeScript = _obj.AddComponent<RecipeScript>();
            recipeScript.Initialise();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }
    }
}

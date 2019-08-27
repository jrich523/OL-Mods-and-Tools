using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
//using OModAPI;

namespace LearnAllRecipes
{
    public class RecipeScript : MonoBehaviour
    {
        public bool LearnedRecipes = false;

        public void Initialise()
        {
            //OLogger.Log("Initialised script! waiting...");            
        }

        public void Update()
        {
            if (   !NetworkLevelLoader.Instance.InLoading
                && !NetworkLevelLoader.Instance.IsGameplayPaused
                && !MenuManager.Instance.IsInMainMenuScene
                && !MenuManager.Instance.InFade
                && !MenuManager.Instance.IsMasterLoadingDisplayed)
            {
                if (!LearnedRecipes)
                {
                    LearnAllRecipes();
                }
            }
            else if (MenuManager.Instance.IsInMainMenuScene)
            {
                if (LearnedRecipes)
                {
                    LearnedRecipes = false;
                    //OLogger.Log("Reset learned recipes flag!");
                }
            }
        }

        private void LearnAllRecipes()
        {
            CharacterRecipeKnowledge charRecipes = CharacterManager.Instance.GetFirstLocalCharacter().Inventory.RecipeKnowledge;
            Dictionary<string, Recipe> recipes = typeof(RecipeManager).GetField("m_recipes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(RecipeManager.Instance) as Dictionary<string, Recipe>;

            if (recipes != null && charRecipes != null)
            {
                LearnedRecipes = true;

                foreach (KeyValuePair<string, Recipe> entry in recipes)
                {
                    charRecipes.LearnRecipe(entry.Value);
                }
            }
        }
    }
}
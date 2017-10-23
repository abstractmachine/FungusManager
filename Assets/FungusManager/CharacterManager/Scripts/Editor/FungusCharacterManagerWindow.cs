using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Fungus
{

    public class FungusCharacterManagerWindow : FungusManagerWindow
    {
        #region Members

        bool newCharacterFoldout = true;
        string characterName = "CharacterName";
        bool charactersFoldout = true;

        /// <summary>
        /// The list of all the character GameObjects, along with their references SayDialog
        /// </summary>
        Dictionary<GameObject, string> characters = new Dictionary<GameObject, string>();

        #endregion


        #region Window

        // Add menu item
        [MenuItem("Tools/Fungus/Character Manager")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow<FungusCharacterManagerWindow>("Characters");
        }

        #endregion


        #region GUI

        override protected void OnGUI()
        {
            base.OnGUI();

            if (sceneManagerIsLoaded)
            {
                DisplayCharacterManager();
            }
        }


        private void DisplayCharacterManager()
        {
            // spacing

            GUILayout.Space(20);

            // scene controls

            GUILayout.BeginHorizontal();

            GUILayout.Space(20);

            ////////////////////// CHARACTERS ////////////////////////////

            GUILayout.BeginVertical();

            // CLOSE WINDOW

            if (!sceneManagerIsActive)
            {

                // convert the above string into ligatures and print out into console
                if (GUILayout.Button("Close 'SceneManager'"))
                {
                    CloseFungusSceneManager();
                }

                GUILayout.Space(20);

            }

            newCharacterFoldout = EditorGUILayout.Foldout(newCharacterFoldout, "New Character");

            if (newCharacterFoldout)
            {
                characterName = EditorGUILayout.TextField("", characterName);

                GUILayout.BeginHorizontal();

                // convert the above string into ligatures and print out into console
                if (GUILayout.Button("New Character"))
                {
                    Debug.Log("Create new character named '" + characterName + "'");
                }

                GUILayout.EndHorizontal();

            } // if (newCharacter)

            GUILayout.Space(20);

            charactersFoldout = EditorGUILayout.Foldout(charactersFoldout, "Current Characters (" + characters.Count + ")");

            if (charactersFoldout)
            {
                DisplayCharacters();
            }

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            // FLEXIBLE SPACE


        }


        private void DisplayCharacters()
        {
            // check to see what the current character names are
            CheckCharacterNames();

            foreach(KeyValuePair<GameObject,string> characterKeyPair in characters)
            {
                DisplayCharacter(characterKeyPair.Key, characterKeyPair.Value);
            }
        }


        private void DisplayCharacter(GameObject characterGameObject, string name)
        {

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("DELETE"))
            {
                Debug.Log("Delete Character " + name);
            }

            EditorGUILayout.LabelField(name);

            GUILayout.EndHorizontal();
        }

        #endregion


        #region Characters

        void CheckCharacterNames()
        {
            // note if there were any changes
            bool didChange = false;
            // get the SceneManager scene reference
            Scene managerScene = GetSceneManager();
            // find all the characters currently available in the SceneManager
            Character[] currentSceneCharacters = FindObjectsOfType<Character>();
            // if the amount of characters is different than the dictionary is different
            if (currentSceneCharacters.Length != characters.Count)
            {
                // note difference
                didChange = true;
            }
            else // otherwise, we're still the same count
            {
                // go through each of these characters
                foreach (Character character in currentSceneCharacters)
                {
                    // ignore any GameObject that is not in the ManagerScene
                    if (character.gameObject.scene != managerScene) continue;
                    // see if this is not yet in the dictionary
                    if (!characters.ContainsKey(character.gameObject))
                    {
                        // not yet in dictionary, note change
                        didChange = true;
                        break;
                    }
                    // check to see if the name has changed
                    if (characters[character.gameObject] != character.NameText)
                    {
                        didChange = true;
                        break;
                    }
                } // foreach
            } // if (Length != Count

            // ok, there was a change
            if (didChange)
            {
                // erase current dictionary
                characters.Clear();
                // go through each of these characters
                foreach (Character character in currentSceneCharacters)
                {
                    // add this character
                    characters.Add(character.gameObject, character.NameText);
                }
            }

        } // GetCharacterNames

        #endregion
    }

}

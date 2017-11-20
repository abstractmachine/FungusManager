//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEditor;
//using UnityEditor.SceneManagement;
//using System.Collections;
//using System.Collections.Generic;

//namespace Fungus
//{

//    public class FungusCharacterManagerWindow : FungusManagerWindow
//    {
//        #region Members

//        //bool newCharacterFoldout = true;
//        //string characterName = "CharacterName";
//        //bool charactersFoldout = true;

//        /// <summary>
//        /// The list of all the character GameObjects, along with their references SayDialog
//        /// </summary>
//        //Dictionary<GameObject, string> characters = new Dictionary<GameObject, string>();

//        #endregion


//        #region Window

//        // Add menu item
//        [MenuItem("Tools/Fungus Manager/Character Manager Window")]
//        public static void ShowWindow()
//        {
//            //Show existing window instance. If one doesn't exist, make one.
//            EditorWindow.GetWindow<FungusCharacterManagerWindow>("Characters");
//        }

//        #endregion


//        #region GUI

//        override protected void OnGUI()
//        {
//            base.OnGUI();

//            //// if the scene manager is not already loaded
//            //if (sceneManagerIsLoaded)
//            //{
//            //    DisplaySceneManager();
//            //    return;
//            //}
//            //
//            //// check to see if there is at least one scene manager in the project
//            //if (!ProjectContainsSceneNamed("SceneManager.unity"))
//            //{
//            //    CreateSceneManagerButton();
//            //}
//            //// now check to see that there is a start scene
//            //else if (!ProjectContainsSceneNamed("Start.unity"))
//            //{
//            //    CreateSceneManagerButton();
//            //}
//            //else if (!startSceneIsLoaded)
//            //{
//            //    LoadSceneButton("Load 'Start' scene", GetSceneAssetPath("SceneManager.unity"));
//            //}

//            // check to see if there is at least one scene manager in the project
//            if (!projectContainsStartScene)
//            {
//                //string textLabel = "There must be a managed 'Start' scene in order to manage Characters. ";
//                //textLabel += "Open SceneManager window and create a scene named 'Start' with 'Create Characters prefab' activated.";

//                //GUIStyle style = GUI.skin.box;
//                //style.alignment = TextAnchor.MiddleCenter;

//                //EditorWindow window = EditorWindow.GetWindow<FungusCharacterManagerWindow>("Characters");

//                //// make the Box double sized
//                //GUI.Box(new Rect(0.0f, 0.0f, window.position.width, window.position.height), textLabel);
//            }
//            else
//            {
//                if (!startSceneIsLoaded)
//                {
//                    // load the SceneManager and place it on top
//                    LoadSceneButton("SceneManager", GetSceneAssetPath("SceneManager.unity"), true);
//                }

//                // if the scene manager is not already loaded
//                if (startSceneIsLoaded)
//                {
//                    DisplayCharacterManager();
//                    return;
//                }
//            }
//            // if (!projectContainsStartScene

//        }

//        #endregion


//        #region Display

//        private void DisplayCharacterManager()
//        {
//            //    // spacing

//            //    GUILayout.Space(20);

//            //    // scene controls

//            //    GUILayout.BeginHorizontal();

//            //    GUILayout.Space(20);

//            //    ////////////////////// CHARACTERS ////////////////////////////

//            //    GUILayout.BeginVertical();

//            //    newCharacterFoldout = EditorGUILayout.Foldout(newCharacterFoldout, "New Character");

//            //    if (newCharacterFoldout)
//            //    {
//            //        characterName = EditorGUILayout.TextField("", characterName);

//            //        GUILayout.BeginHorizontal();

//            //        // convert the above string into ligatures and print out into console
//            //        if (GUILayout.Button("New Character"))
//            //        {
//            //            CreateCharacter(characterName);
//            //        }

//            //        GUILayout.EndHorizontal();

//            //    } // if (newCharacter)

//            //    GUILayout.Space(20);

//            //    charactersFoldout = EditorGUILayout.Foldout(charactersFoldout, "Current Characters (" + characters.Count + ")");

//            //    if (charactersFoldout)
//            //    {
//            //        DisplayCharacters();
//            //    }

//            //    GUILayout.EndVertical();

//            //    GUILayout.FlexibleSpace();

//            //    GUILayout.EndHorizontal();

//            //    // FLEXIBLE SPACE


//        }


//        //private void DisplayCharacters()
//        //{
//        //    // check to see what the current character names are
//        //    CheckCharacterNames();

//        //    foreach (KeyValuePair<GameObject, string> characterKeyPair in characters)
//        //    {
//        //        DisplayCharacter(characterKeyPair.Key, characterKeyPair.Value);
//        //    }
//        //}


//        //private void DisplayCharacter(GameObject characterGameObject, string name)
//        //{
//        //    GUILayout.BeginHorizontal();

//        //    if (GUILayout.Button("DELETE"))
//        //    {
//        //        Debug.Log("Delete Character " + name);
//        //    }

//        //    EditorGUILayout.LabelField(name);

//        //    GUILayout.EndHorizontal();
//        //}

//        #endregion


//        #region Characters

//        //void CreateCharacter(string characterName)
//        //{
//        //    // find all the characters currently available in the SceneManager
//        //    Character[] currentSceneCharacters = FindObjectsOfType<Character>();
//        //    // see if this is not yet in the dictionary
//        //    foreach (Character character in currentSceneCharacters)
//        //    {
//        //        if (character.gameObject.name == characterName)
//        //        {
//        //            Debug.LogWarning("Character " + characterName + " already exists");
//        //            return;
//        //        }
//        //    }
//        //    //// ok, create it
//        //    //GameObject characterPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/FungusManager/CharacterManager/Prefabs/Character.prefab", typeof(GameObject));
//        //    //PrefabUtility.InstantiatePrefab(characterPrefab, startScene);
//        //    //// try to save
//        //    //bool startSaveSuccess = EditorSceneManager.SaveScene(startScene, path + "/Start.unity", false);

//        //    //if (!startSaveSuccess)
//        //    //{
//        //    //    Debug.LogWarning("Couldn't create Start scene.");
//        //    //    return;
//        //    //}
//        //}


//        //void CheckCharacterNames()
//        //{
//        //    // note if there were any changes
//        //    bool didChange = false;
//        //    // get the SceneManager scene reference
//        //    Scene managerScene = GetSceneManagerScene();
//        //    // find all the characters currently available in the SceneManager
//        //    Character[] currentSceneCharacters = FindObjectsOfType<Character>();
//        //    // if the amount of characters is different than the dictionary is different
//        //    if (currentSceneCharacters.Length != characters.Count)
//        //    {
//        //        // note difference
//        //        didChange = true;
//        //    }
//        //    else // otherwise, we're still the same count
//        //    {
//        //        // go through each of these characters
//        //        foreach (Character character in currentSceneCharacters)
//        //        {
//        //            // ignore any GameObject that is not in the ManagerScene
//        //            if (character.gameObject.scene != managerScene) continue;
//        //            // see if this is not yet in the dictionary
//        //            if (!characters.ContainsKey(character.gameObject))
//        //            {
//        //                // not yet in dictionary, note change
//        //                didChange = true;
//        //                break;
//        //            }
//        //            // check to see if the name has changed
//        //            if (characters[character.gameObject] != character.NameText)
//        //            {
//        //                didChange = true;
//        //                break;
//        //            }
//        //        } // foreach
//        //    } // if (Length != Count

//        //    // ok, there was a change
//        //    if (didChange)
//        //    {
//        //        // erase current dictionary
//        //        characters.Clear();
//        //        // go through each of these characters
//        //        foreach (Character character in currentSceneCharacters)
//        //        {
//        //            // add this character
//        //            characters.Add(character.gameObject, character.NameText);
//        //        }
//        //    }

//        //} // GetCharacterNames

//        #endregion
//    }

//}

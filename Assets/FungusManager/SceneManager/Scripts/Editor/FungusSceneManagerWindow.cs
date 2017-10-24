using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Fungus
{
    public class FungusSceneManagerWindow : FungusManagerWindow
    {
        #region Members

        private bool addHyperzoomControls = true;
        private bool addControllerInput = true;
        private bool createCharactersPrefab = true;

        private bool newSceneFoldout = true;
        private string sceneName = "Start";
        //private bool managedScenesFoldout = true;

        //private FungusSceneManager fungusSceneManager = null;

        //private List<string> managedScenes = new List<string>();
        //private List<string> availableScenes = new List<string>();

        #endregion


        #region Window

        // Add menu item
        [MenuItem("Tools/Fungus/Scene Manager")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow<FungusSceneManagerWindow>("Scene Manager");
        }

        #endregion


        #region GUI

        override protected void OnGUI()
        {
            base.OnGUI();

            CheckScenes();

            // check to see if there is at least one scene manager in the project
            if (!projectContainsSceneManager)
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.BeginVertical();

                if (!projectContainsSceneManager)
                {
                    if (GUILayout.Button("Create 'SceneManager'"))
                    {
                        CreateFungusSceneManager();
                        return;
                    }
                }

                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }
            else
            {
                if (!sceneManagerIsLoaded)
                {
                    // load the SceneManager and place it on top
                    LoadSceneButton("SceneManager", GetSceneAssetPath("SceneManager.unity"), true);
                }

                // if the scene manager is not already loaded
                if (sceneManagerIsLoaded)
                {
                    DisplaySceneManager();
                    return;
                }
            }

        }


        void GUIDrawSceneOptions()
        {
            createCharactersPrefab = GUILayout.Toggle(createCharactersPrefab, "Create Characters prefab", GUILayout.MinWidth(80), GUILayout.MaxWidth(200));

            addHyperzoomControls = GUILayout.Toggle(addHyperzoomControls, "Add Hyperzoom", GUILayout.MinWidth(80), GUILayout.MaxWidth(200));

            // the joystick controller is attached to the hyperzoom
            if (addHyperzoomControls)
            {
                addControllerInput = GUILayout.Toggle(addControllerInput, "Add Joystick Controller input");
            }
        }

        #endregion


        #region Create New Scene

        /// <summary>
        /// Try to set the attention onto another scene than the main scene
        /// </summary>
       
        void SetActiveSceneToOther()
        {
            int sceneManagerIndex = SceneManagerIndex();
            // if this is not an Untitled Scene, return a new sub-scene
            if (sceneManagerIndex > -1) // && EditorSceneManager.GetActiveScene().name != ""
            {
                Scene sceneMananger = GetSceneManagerScene();
                // move scene to the top
                if (sceneManagerIndex > 0)
                {
                    MoveSceneToTop(sceneMananger);
                }
                // 
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    Scene scene = EditorSceneManager.GetSceneAt(i);
                    // if this is not the SceneManager
                    if (scene != sceneMananger)
                    {
                        // set this scene as the active scene
                        EditorSceneManager.SetActiveScene(scene);
                        break;
                    }
                    // if (scene != sceneManager
                }
                // for(sceneCount
            }
            // if (sceneManagerIndex > -1
        }


        protected Scene GetCleanScene(bool isSceneManager = false)
        {
            // try to set the attention onto another scene than the main scene
            SetActiveSceneToOther();

            // if this is the SceneManager at 0
            int sceneManagerIndex = SceneManagerIndex();
            if (EditorSceneManager.sceneCount == 1 && sceneManagerIndex == 0)
            {
                return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            }

            // ok, we need to return the current scene

            // get access to this scene
            Scene activeScene = EditorSceneManager.GetActiveScene();
            // get this scene's root objects
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            // if this is the scene manager, we have to clean up
            // go through each root object
            for (int i = rootObjects.Length - 1; i >= 0; i--)
            {
                // reference to this root object
                GameObject rootObject = rootObjects[i];
                // if there is a camera here and this is the sceneManager
                // or it's not and we're adding Hyperzoom controls (it has its own camera)
                if ((rootObject.GetComponent<Camera>() != null) && (isSceneManager || (!isSceneManager && addHyperzoomControls)))
                {
                    // destroy camera
                    DestroyImmediate(rootObject);
                    // move on to next
                    continue;
                }
                // destroy lights
                if (isSceneManager && rootObject.GetComponent<Light>() != null) DestroyImmediate(rootObject);
            }
            // for

            // return this current scene
            return activeScene;
        }


        protected void CreateFungusSceneManager()
        {
            // tell the user to select a path
            string path = EditorUtility.SaveFolderPanel("Select a folder for the 'SceneManager' scene", "Assets/", "");

            // check the path
            if (!IsPathValid(path)) return;

            // remove full data path
            path = CleanUpPath(path);

            // Create the SceneManager

            // either create a new sub-scene, or erase the current scene
            Scene sceneManager = GetCleanScene(true);

            // add prefabs to scene
            GameObject sceneManagerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/FungusManager/SceneManager/Prefabs/SceneManager.prefab", typeof(GameObject));
            GameObject sceneManagerGameObject = PrefabUtility.InstantiatePrefab(sceneManagerPrefab, sceneManager) as GameObject;
            // disconnect this object from the prefab (in package folder) that created it
            PrefabUtility.DisconnectPrefabInstance(sceneManagerGameObject);

            GameObject flowchartPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/FungusManager/SceneManager/Prefabs/Flowcharts.prefab", typeof(GameObject));
            GameObject flowchartGameObject = PrefabUtility.InstantiatePrefab(flowchartPrefab, sceneManager) as GameObject;
            // disconnect this object from the prefab (in package folder) that created it
            PrefabUtility.DisconnectPrefabInstance(flowchartGameObject);

            // try to save
            if (!EditorSceneManager.SaveScene(sceneManager, path + "/SceneManager.unity", false))
            {
                Debug.LogWarning("Couldn't create FungusSceneManager");
            }

            CheckScenes();

        }


        void CreateNewScene(string sceneName)
        {
            // if the scene exists already
            if (DoesSceneExist(sceneName))
            {
                Debug.LogWarning("Scene '" + sceneName + "' already exists.");
                return;
            }

            // tell the user to select a path
            string path = EditorUtility.SaveFolderPanel("Select a folder for the '" + sceneName + "' scene", "Assets/", "");

            // check the path
            if (!IsPathValid(path)) return;

            // remove full data path
            path = CleanUpPath(path);

            // either create a new sub-scene, or erase the current scene
            Scene startScene = GetCleanScene(false);

            // add prefabs to scene

            // hyperzoom is optional
            if (addHyperzoomControls)
            {
                GameObject hyperzoomPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/FungusManager/Hyperzoom/Prefabs/Hyperzoom.prefab", typeof(GameObject));
                GameObject hyperzoomGameObject = PrefabUtility.InstantiatePrefab(hyperzoomPrefab, startScene) as GameObject;

                // controller input is optional
                if (!addControllerInput)
                {
                    Joystick joystick = hyperzoomGameObject.GetComponent<Joystick>();
                    DestroyImmediate(joystick);
                }
            }

            if (createCharactersPrefab)
            {
                // this is the path to the prefab
                string charactersPrefabPath = "Assets/FungusManager/CharacterManager/Prefabs/FungusCharacters.prefab";
                // find out if there already is a prefab in our project
                string projectCharactersPrefabPath = GetPrefabPath("FungusCharacters");
                // if we found something
                if (projectCharactersPrefabPath != "")
                {
                    // use this prefab path instead of the one in the project path
                    charactersPrefabPath = projectCharactersPrefabPath;
                }

                GameObject charactersPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(charactersPrefabPath, typeof(GameObject));
                GameObject charactersGameObject = PrefabUtility.InstantiatePrefab(charactersPrefab, startScene) as GameObject;

                // if this is a new prefab
                if (projectCharactersPrefabPath == "")
                {
                    // make sure this prefab goes into the same folder at the Start scene's folder
                    string newPrefabFolder = path + "/FungusCharacters.prefab";
                    // save it to new position
                    GameObject newPrefab = PrefabUtility.CreatePrefab(newPrefabFolder, charactersGameObject) as GameObject;
                    // set this as our prefab
                    PrefabUtility.ConnectGameObjectToPrefab(charactersGameObject, newPrefab);
                }
            }

            // try to save
            if (!EditorSceneManager.SaveScene(startScene, path + "/" + sceneName + ".unity", false))
            {
                Debug.LogWarning("Couldn't create 'Start' scene");
            }

            CheckScenes();
        }

        #endregion


        #region Display

        private void DisplaySceneManager()
        {
            // spacing

            GUILayout.Space(20);

            // scene controls

            GUILayout.BeginHorizontal();

            GUILayout.Space(20);

            GUILayout.BeginVertical();

            // CREATE NEW SCENE

            newSceneFoldout = EditorGUILayout.Foldout(newSceneFoldout, "New Scene");

            if (newSceneFoldout)
            {
                sceneName = EditorGUILayout.TextField("", sceneName, GUILayout.ExpandWidth(false));

                // convert the above string into ligatures and print out into console
                if (GUILayout.Button("Create New Scene", GUILayout.ExpandWidth(false)))
                {
                    CreateNewScene(sceneName);
                    return;
                }

                GUIDrawSceneOptions();

            } // if (newScene)

            GUILayout.Space(20);

            //// convert the above string into ligatures and print out into console
            //if (GUILayout.Button("Update scenes", GUILayout.ExpandWidth(false)))
            //{
            //    UpdateScenes();
            //}

            //managedScenesFoldout = EditorGUILayout.Foldout(managedScenesFoldout, "Current Scenes (" + managedScenes.Count + ")");

            //if (managedScenesFoldout)
            //{
            //    DisplayScenes();
            //}

            GUILayout.EndVertical();

            GUILayout.Space(40);

            GUILayout.BeginVertical();

            GUILayout.Space(20);

            ////availableScenesFoldout = EditorGUILayout.Foldout(availableScenesFoldout, "Available Scenes (" + availableScenes.Count + ")");

            ////if (availableScenesFoldout)
            ////{
            ////    DisplayAvailableScenes();
            ////}

            GUILayout.EndVertical();

            GUILayout.Space(20);

            GUILayout.EndHorizontal();

            //// FLEXIBLE SPACE
        }

        #endregion



        #region Scenes

        //void UpdateScenes()
        //{
        //    CheckForSceneManager();

        //    // create an empty list
        //    //List<string> scenePathsToAdd = new List<string>();
        //    List<string> scenesToAdd = new List<string>();

        //    // first add the 
        //    //scenesToAdd.Add(fungusSceneManager.gameObject.scene.name);

        //    // first load in all the current scenes in the build settings
        //    foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        //    {
        //        Debug.Log(fungusSceneManager.gameObject.scene.path);
        //        Debug.Log(buildScene.path);
        //        // if this is not the manager scene
        //        if (fungusSceneManager.gameObject.scene.path != buildScene.path)
        //        {
        //            // name without extension
        //            string sceneFileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(buildScene.path);
        //            //scenePathsToAdd.Add(buildScene.path);
        //            scenesToAdd.Add(sceneFileNameWithoutExtension);
        //        }
        //    }

        //    // tell the mananger to save it's paths
        //    fungusSceneManager.scenes = scenesToAdd;

        //    // set the current scene as "dirty"
        //    EditorSceneManager.MarkSceneDirty(fungusSceneManager.gameObject.scene);
        //}

        //override protected void CheckForSceneManager()
        //{
        //    base.CheckForSceneManager();

        //    if (fungusSceneManager == null)
        //    {
        //        fungusSceneManager = GetFungusSceneManagerScript();
        //    }

        //    UpdateManagedSceneList();
        //    UpdateAvailableSceneList();
        //}


        //private void UpdateManagedSceneList()
        //{
        //    if (fungusSceneManager == null) return;

        //    managedScenes = fungusSceneManager.scenes;
        //}


        //private void DisplayScenes()
        //{
        //    if (fungusSceneManager == null) return;

        //    foreach (string scene in fungusSceneManager.scenes)
        //    {
        //        DisplayScene(scene);
        //    }
        //}


        //private void UpdateAvailableSceneList()
        //{
        //    availableScenes = CurrentSceneAssets();
        //}


        //private void DisplayScene(string sceneName)
        //{
        //    GUILayout.BeginHorizontal();
        //    bool newState = GUILayout.Toggle(true, sceneName);
        //    if (newState == false)
        //    {
        //        RemoveScene(sceneName);
        //    }
        //    GUILayout.EndHorizontal();

        //}


        //private void DisplayAvailableScenes()
        //{
        //    foreach (string scene in availableScenes)
        //    {
        //        bool state = false;
        //        // name without extension
        //        string sceneFileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(scene);
        //        if (managedScenes.Contains(sceneFileNameWithoutExtension)) state = true;
        //        DisplayAvailableScene(sceneFileNameWithoutExtension, state);
        //    }
        //}


        //private void DisplayAvailableScene(string sceneName, bool state = false)
        //{
        //    GUILayout.BeginHorizontal();
        //    bool newState = GUILayout.Toggle(state, sceneName);
        //    GUILayout.EndHorizontal();

        //    if (newState != state)
        //    {

        //        string name = (new DirectoryInfo(sceneName).Name);
        //        Debug.Log(name);

        //        if (newState == true)
        //        {
        //            AddScene(sceneName);
        //        }
        //        else
        //        {
        //            RemoveScene(sceneName);
        //        }

        //        // set the manger scene as "dirty"
        //        EditorSceneManager.MarkSceneDirty(GetSceneManagerScene());
        //    }
        //}

        #endregion


        #region Add/Remove

        //void NewScene(string sceneName)
        //{
        //    Debug.Log("New : " + sceneName);
        //}

        //void AddScene(string sceneName)
        //{
        //    Debug.Log("Add : " + sceneName);

        //    //// create an empty list
        //    ////List<string> scenePathsToAdd = new List<string>();
        //    //List<string> scenesToAdd = new List<string>();

        //    //// first add the 
        //    //scenesToAdd.Add(fungusSceneManager.gameObject.scene.name);

        //    //// first load in all the current scenes in the build settings
        //    //foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        //    //{
        //    //    // if this is not the manager scene
        //    //    if (fungusSceneManager.gameObject.scene.path != buildScene.path)
        //    //    {
        //    //        // name without extension
        //    //        string sceneFileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(buildScene.path);
        //    //        //scenePathsToAdd.Add(buildScene.path);
        //    //        scenesToAdd.Add(sceneFileNameWithoutExtension);
        //    //    }
        //    //}

        //    //// tell the mananger to save it's paths
        //    //fungusSceneManager.scenes = scenesToAdd;

        //    //// set the current scene as "dirty"
        //    //EditorSceneManager.MarkSceneDirty(fungusSceneManager.gameObject.scene);
        //}

        //void RemoveScene(string sceneName)
        //{
        //    Debug.Log("Remove scene");

        //    //// create an empty list
        //    ////List<string> scenePathsToAdd = new List<string>();
        //    //List<string> scenesToAdd = new List<string>();

        //    //// first add the 
        //    //scenesToAdd.Add(fungusSceneManager.gameObject.scene.name);

        //    //// first load in all the current scenes in the build settings
        //    //foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        //    //{
        //    //    // if this is not the manager scene
        //    //    if (fungusSceneManager.gameObject.scene.path != buildScene.path)
        //    //    {
        //    //        // name without extension
        //    //        string sceneFileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(buildScene.path);

        //    //        if (sceneName == sceneFileNameWithoutExtension) continue;
        //    //        //scenePathsToAdd.Add(buildScene.path);
        //    //        scenesToAdd.Add(sceneFileNameWithoutExtension);
        //    //    }
        //    //}

        //    //// tell the mananger to save it's paths
        //    //fungusSceneManager.scenes = scenesToAdd;

        //    //// set the current scene as "dirty"
        //    //EditorSceneManager.MarkSceneDirty(fungusSceneManager.gameObject.scene);
        //}

        #endregion

    }

}
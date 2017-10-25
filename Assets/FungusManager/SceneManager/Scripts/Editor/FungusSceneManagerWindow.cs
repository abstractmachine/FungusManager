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

        private string lastSaveFolder = "Assets/";

        private bool newSceneFoldout = true;
        private bool addSceneFoldout = true;

        private string sceneName = "Start";
        private bool managedScenesFoldout = true;

        private Object addSceneObject = null;

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

            // ADD SCENE

            GUILayout.Space(20);

            addSceneFoldout = EditorGUILayout.Foldout(addSceneFoldout, "Add Scene");

            if (addSceneFoldout)
            {
                addSceneObject = EditorGUILayout.ObjectField(addSceneObject, typeof(Object), true, GUILayout.ExpandWidth(false));
                // 
                if (GUILayout.Button("Add Scene", GUILayout.ExpandWidth(false)))
                {
                    if (addSceneObject == null)
                    {
                        Debug.LogWarning("No scene to add");
                    } 
                    else if (addSceneObject.GetType() == typeof(SceneAsset))
                    {
                        SceneAsset addSceneAsset = addSceneObject as SceneAsset;
                        AddScene(addSceneAsset);
                    }
                    else
                    {
                        Debug.LogWarning("Asset type incorrect. Please select a Scene to add");
                    }
                    return;
                }
            }

            // UPDATE SCENE LIST

            GUILayout.Space(20);

            managedScenesFoldout = EditorGUILayout.Foldout(managedScenesFoldout, "Current Scenes");

            if (managedScenesFoldout)
            {
                // convert the above string into ligatures and print out into console
                if (GUILayout.Button("Update Scene List", GUILayout.ExpandWidth(false)))
                {
                    UpdateScenes();
                }

                GUILayout.Space(10);

                DisplayScenes();
            }

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


        #region Create New Scene

        protected Scene GetCleanScene(bool isSceneManager = false)
        {
            if (isSceneManager)
            {
                // get access to this scene
                Scene activeScene = EditorSceneManager.GetActiveScene();
                // does the scene need to be saved?
                if (activeScene.isDirty)
                {
                    Debug.LogWarning("The active scene is not empty. Create a new scene before creating a SceneManager");
                    return new Scene();
                }
                // get this scene's root objects
                GameObject[] rootObjects = activeScene.GetRootGameObjects();
                // if this is the scene manager, we have to clean up
                // go through each root object
                for (int i = rootObjects.Length - 1; i >= 0; i--)
                {
                    // reference to this root object
                    GameObject rootObject = rootObjects[i];
                    // destroy camera
                    DestroyImmediate(rootObject);
                }
                // for(rootObjects.Length
                return activeScene;
            }

            Scene managerScene = GetSceneManagerScene();

            // close the other scene
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                Scene scene = EditorSceneManager.GetSceneAt(i);
                // leave manager scene
                if (managerScene.IsValid() && managerScene == scene) continue;
                // close anything else
                if (!EditorSceneManager.CloseScene(scene, true))
                {
                    Debug.LogError("Couldn't close scene " + scene.name);
                    return new Scene();
                }
            }

            if (managerScene.IsValid())
            {
                EditorSceneManager.SetActiveScene(managerScene);
            }

            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

        }


        protected void CreateFungusSceneManager()
        {
            // tell the user to select a path
            string path = EditorUtility.SaveFolderPanel("Select a folder for the 'SceneManager' scene", "Assets/", lastSaveFolder);
            lastSaveFolder = path; // CleanUpPath(path + "/");

            // check the path
            if (!IsPathValid(path)) return;

            // remove full data path
            path = CleanUpPath(path);

            // Create the SceneManager

            // either create a new sub-scene, or erase the current scene
            Scene sceneManager = GetCleanScene(true);

            // make sure the scene we got back was valid
            if (!sceneManager.IsValid()) return;

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

            // add this new scene to the build settings
            SaveSceneToBuildSettings(sceneManager);

            CheckScenes();

        }


        void AddScene(SceneAsset addSceneAsset)
        {
            string addPath = AssetDatabase.GetAssetPath(addSceneAsset);
            AddScenePathToBuildSettings(addPath);
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
            string path = EditorUtility.SaveFolderPanel("Select a folder for the '" + sceneName + "' scene", "Assets/", lastSaveFolder);
            lastSaveFolder = path; // CleanUpPath(path + "/");

            // check the path
            if (!IsPathValid(path)) return;

            // remove full data path
            path = CleanUpPath(path);

            // either create a new sub-scene, or erase the current scene
            Scene newScene = GetCleanScene(false);

            // make sure the scene we got back was valid
            if (!newScene.IsValid()) return;

            // add prefabs to scene

            // hyperzoom is optional
            if (addHyperzoomControls)
            {
                GameObject hyperzoomPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/FungusManager/Hyperzoom/Prefabs/Hyperzoom.prefab", typeof(GameObject));
                GameObject hyperzoomGameObject = PrefabUtility.InstantiatePrefab(hyperzoomPrefab, newScene) as GameObject;

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
                GameObject charactersGameObject = PrefabUtility.InstantiatePrefab(charactersPrefab, newScene) as GameObject;

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
            if (!EditorSceneManager.SaveScene(newScene, path + "/" + sceneName + ".unity", false))
            {
                Debug.LogWarning("Couldn't create 'Start' scene");
            }

            // add this new scene to the build settings
            SaveSceneToBuildSettings(newScene);

            CheckScenes();
        }

        #endregion


        #region Scene List

        private void DisplayScenes()
        {
            FungusSceneManager fungusSceneManagerScript = GetFungusSceneManagerScript();

            foreach (string scene in fungusSceneManagerScript.scenes)
            {
                DisplayScene(scene);
            }
        }


        private void DisplayScene(string sceneName)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("REMOVE", GUILayout.ExpandWidth(false)))
            {
                RemoveSceneFromBuildSettings(sceneName); 
            }

            GUILayout.Space(20);

            GUILayout.Label(sceneName);

            GUILayout.EndHorizontal();

        }


        void UpdateScenes()
        {
            SaveBuildSettingsInSceneManager();
        }

        #endregion



        #region Scenes

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


        //private void UpdateAvailableSceneList()
        //{
        //    availableScenes = CurrentSceneAssets();
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

        #endregion

    }

}
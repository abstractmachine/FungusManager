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

        //private bool newSceneFoldout = true;
        //private string sceneName = "SceneName";
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
            if (!projectContainsSceneManager || !projectContainsStartScene)
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (!projectContainsSceneManager)
                {
                    if (GUILayout.Button("Create 'SceneManager'"))
                    {
                        CreateFungusSceneManager();
                        return;
                    }
                }

                if (!projectContainsStartScene)
                {
                    if (GUILayout.Button("Create 'Start' scene"))
                    {
                        CreateStartScene();
                        return;
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }
            else
            {

                if (!sceneManagerIsLoaded)
                {
                    LoadSceneButton("SceneManager", GetSceneAssetPath("SceneManager.unity"));
                }

                // if the scene manager is not already loaded
                if (sceneManagerIsLoaded)
                {
                    //DisplaySceneManager();
                    return;
                }
            }

        }


        protected void CreateStartSceneButton()
        {
        }

        #endregion


        #region Create

        protected void CreateFungusSceneManager()
        {
            // tell the user to select a path
            string path = EditorUtility.SaveFolderPanel("Select a folder for the SceneManager", "Assets/", "");

            // check the path
            if (!IsPathValid(path)) return;

            // remove full data path
            if (path.StartsWith(Application.dataPath))
            {
                // remove start of full data path, just take the characters after the word "Assets"
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            // Create the SceneManager

            Scene sceneManager = new Scene();

            // first check to see if we are in an Untitled Scene
            if (EditorSceneManager.GetActiveScene().name == "")
            {
                sceneManager = EditorSceneManager.GetActiveScene();
                GameObject[] rootObjects = sceneManager.GetRootGameObjects();
                for (int i = rootObjects.Length - 1; i >= 0; i--)
                {
                    GameObject rootObject = rootObjects[i];

                    if (rootObject.GetComponent<Camera>() != null) DestroyImmediate(rootObject);
                    else if (rootObject.GetComponent<Light>() != null) DestroyImmediate(rootObject);
                }
            }
            else
            {
                sceneManager = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            }

            // add prefabs to scene
            GameObject fungusSceneManagerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/FungusManager/SceneManager/Prefabs/FungusSceneManager.prefab", typeof(GameObject));
            PrefabUtility.InstantiatePrefab(fungusSceneManagerPrefab, sceneManager);

            GameObject fungusFlowchartPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/FungusManager/SceneManager/Prefabs/Flowcharts.prefab", typeof(GameObject));
            PrefabUtility.InstantiatePrefab(fungusFlowchartPrefab, sceneManager);

            // try to save
            bool saveSuccess = EditorSceneManager.SaveScene(sceneManager, path + "/FungusSceneManager.unity", false);

            if (!saveSuccess)
            {
                Debug.LogWarning("Couldn't create FungusSceneManager");
            }

            CheckScenes();

        }


        protected void CreateStartScene()
        {
            //Scene startScene = new Scene();

            //// try to find the start scene
            //string startScenePath = GetSceneAssetPath("Start.unity");
            //if (startScenePath != "")
            //{
            //    startScene = EditorSceneManager.OpenScene(startScenePath);
            //}
            //else // Create the Start scene
            //{
            //    // this will verify if we need to create start scene
            //    bool usingActiveScene = false;
            //    // first check to see if we are in an Untitled Scene
            //    Scene activeScene = EditorSceneManager.GetActiveScene();
            //    if (activeScene.name == "")
            //    {
            //        GameObject[] rootObjects = activeScene.GetRootGameObjects();
            //        if (rootObjects.Length <= 3 && rootObjects[0].name == "Main Camera")
            //        {
            //            usingActiveScene = true;
            //            DestroyImmediate(rootObjects[0]);
            //            startScene = activeScene;
            //            // add stuff
            //            // add prefabs to scene
            //            GameObject hyperzoomPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/FungusManager/Hyperzoom/Prefabs/Hyperzoom.prefab", typeof(GameObject));
            //            PrefabUtility.InstantiatePrefab(hyperzoomPrefab, startScene);
            //            GameObject inputPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/FungusManager/Hyperzoom/Prefabs/Input.prefab", typeof(GameObject));
            //            PrefabUtility.InstantiatePrefab(inputPrefab, startScene);
            //            // try to save
            //            bool startSaveSuccess = EditorSceneManager.SaveScene(startScene, path + "/Start.unity", false);

            //            if (!startSaveSuccess)
            //            {
            //                Debug.LogWarning("Couldn't create Start scene.");
            //                return;
            //            }
            //        }
            //    }  // if (activeName == "")

            //    if (!usingActiveScene)
            //    {
            //        Debug.LogWarning("Couldn't create Start scene. Create a New empty scene.");
            //        return;
            //    }
            //}
        }

        #endregion


        #region Display

        //private void DisplaySceneManager()
        //{
        //    int startingCount = managedScenes.Count;

        //    // spacing

        //    GUILayout.Space(20);

        //    // scene controls

        //    GUILayout.BeginHorizontal();

        //    GUILayout.Space(20);

        //    GUILayout.BeginVertical();

        //    // CREATE NEW SCENE

        //    newSceneFoldout = EditorGUILayout.Foldout(newSceneFoldout, "New Scene");

        //    if (newSceneFoldout)
        //    {
        //        sceneName = EditorGUILayout.TextField("", sceneName, GUILayout.ExpandWidth(false));

        //        // convert the above string into ligatures and print out into console
        //        if (GUILayout.Button("Create New Scene", GUILayout.ExpandWidth(false)))
        //        {
        //            NewScene(sceneName);
        //        }

        //    } // if (newScene)

        //    GUILayout.Space(20);

        //    // convert the above string into ligatures and print out into console
        //    if (GUILayout.Button("Update scenes", GUILayout.ExpandWidth(false)))
        //    {
        //        UpdateScenes();
        //    }

        //    managedScenesFoldout = EditorGUILayout.Foldout(managedScenesFoldout, "Current Scenes (" + managedScenes.Count + ")");

        //    if (managedScenesFoldout)
        //    {
        //        DisplayScenes();
        //    }

        //    GUILayout.EndVertical();

        //    GUILayout.Space(40);

        //    GUILayout.BeginVertical();

        //    GUILayout.Space(20);

        //    //availableScenesFoldout = EditorGUILayout.Foldout(availableScenesFoldout, "Available Scenes (" + availableScenes.Count + ")");

        //    //if (availableScenesFoldout)
        //    //{
        //    //    DisplayAvailableScenes();
        //    //}

        //    GUILayout.EndVertical();

        //    GUILayout.Space(20);

        //    GUILayout.EndHorizontal();

        //    // FLEXIBLE SPACE

        //    if (startingCount != managedScenes.Count)
        //    {
        //        UpdateManagedSceneList();
        //        UpdateAvailableSceneList();
        //    }
        //}

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
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Fungus
{

    public class FungusManagerWindow : EditorWindow
    {
        #region Members

        protected bool sceneManagerIsLoaded = false;
        protected bool sceneManagerIsActive = false;

        #endregion


        #region Init

        virtual protected void Awake()
        {
            CheckForSceneManager();
        }


        virtual protected void OnEnable()
        {
            EditorSceneManager.sceneOpened += SceneOpenedCallback;
            EditorSceneManager.sceneSaved -= SceneSavedCallback;
            EditorSceneManager.sceneClosed += SceneClosedCallback;
            EditorSceneManager.activeSceneChanged += ActiveSceneChangedCallback;
            EditorSceneManager.newSceneCreated += SceneCreatedCallback;
        }


        virtual protected void OnDisable()
        {
            EditorSceneManager.sceneOpened -= SceneOpenedCallback;
            EditorSceneManager.sceneSaved -= SceneSavedCallback;
            EditorSceneManager.sceneClosed -= SceneClosedCallback;
            EditorSceneManager.activeSceneChanged -= ActiveSceneChangedCallback;
            EditorSceneManager.newSceneCreated -= SceneCreatedCallback;

        }

        #endregion


        #region GUI

        virtual protected void OnGUI()
        {
            // if the scene manager is not already loaded
            if (!sceneManagerIsLoaded)
            {
                // get current scenes
                List<string> currentScenes = CurrentSceneAssets();
                // check to see if there is at least one scene manager in there
                bool projectContainsSceneManager = ContainsSceneManager(currentScenes);

                if (projectContainsSceneManager)
                {
                    string path = GetSceneManager(currentScenes);
                    LoadSceneManagerButton(path);
                }
                else
                {
                    CreateSceneManagerButton();
                } // if (projectContainsSceneManager

            } // if (!sceneManagerIsLoaded)
        }


        protected void LoadSceneManagerButton(string path)
        {
            if (!sceneManagerIsLoaded)
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Load 'SceneManager'"))
                {
                    LoadFungusSceneManager(path);
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            } 
        }


        protected void CreateSceneManagerButton()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Create 'SceneManager' + 'Start' scenes"))
            {
                CreateFungusSceneManager();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        #endregion


        #region Load/Unload

        protected void LoadFungusSceneManager(string scenePath)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        }


        protected void CreateFungusSceneManager()
        {
            // tell the user to select a path
            string path = EditorUtility.SaveFolderPanel("Select a folder for the SceneManager", "Assets/", "");
            // make sure there was a valid path
            if (path == "")
            {
                // send warning
                Debug.LogWarning("No folder selected");
                return;
            }
            // make sure this is not the root folder
            if (path == Application.dataPath)
            {
                Debug.LogWarning("Cannot save to root 'Assets/' folder. Please select a project sub-folder.");
                return;
            }
            // make sure this is not the FungusManager folder
            if (path.Contains(Application.dataPath + "/FungusManager"))
            {
                Debug.LogWarning("Cannot save into 'Assets/FungusManager' folder. Please select a different project sub-folder.");
                return;
            }
            // make sure this is not the Fungus folder
            if (path.Contains(Application.dataPath + "/Fungus"))
            {
                Debug.LogWarning("Cannot save into 'Assets/Fungus' folder. Please select a different project sub-folder.");
                return;
            }

            // remove full data path
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            Scene startScene = new Scene();

            // try to find the start scene
            string startScenePath = GetSceneAssetPath("Start.unity");
            if (startScenePath != "")
            {
                startScene = EditorSceneManager.OpenScene(startScenePath);
            }
            else // Create the Start scene
            {
                // this will verify if we need to create start scene
                bool usingActiveScene = false;
                // first check to see if we are in an Untitled Scene
                Scene activeScene = EditorSceneManager.GetActiveScene();
                if (activeScene.name == "")
                {
                    GameObject[] rootObjects = activeScene.GetRootGameObjects();
                    if (rootObjects.Length <= 3 && rootObjects[0].name == "Main Camera")
                    {
                        usingActiveScene = true;
                        DestroyImmediate(rootObjects[0]);
                        startScene = activeScene;
                        // add stuff
                        // add prefabs to scene
                        GameObject hyperzoomPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/FungusManager/Hyperzoom/Prefabs/Hyperzoom.prefab", typeof(GameObject));
                        PrefabUtility.InstantiatePrefab(hyperzoomPrefab, startScene);
                        GameObject inputPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/FungusManager/Hyperzoom/Prefabs/Input.prefab", typeof(GameObject));
                        PrefabUtility.InstantiatePrefab(inputPrefab, startScene);
                        // try to save
                        bool startSaveSuccess = EditorSceneManager.SaveScene(startScene, path + "/Start.unity", false);

                        if (!startSaveSuccess)
                        {
                            Debug.LogWarning("Couldn't create Start scene.");
                            return;
                        }
                    }
                }  // if (activeName == "")

                if (!usingActiveScene)
                {
                    Debug.LogWarning("Couldn't create Start scene. Create a New empty scene.");
                    return;
                }
            }

            // Create the SceneManager

            Scene sceneManager = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
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

            CheckForSceneManager();

        }


        string GetSceneManager(List<string> sceneAssets)
        {
            // is there at least one asset with the name SceneManager?
            foreach (string sceneAsset in sceneAssets)
            {
                // if this file name ends with SceneManager.unity
                // TODO: Find a better testing mechanism than scene name
                if (sceneAsset.EndsWith("SceneManager.unity")) return sceneAsset;
            }
            // couldn't find one
            return "";
        }


        bool ContainsSceneManager(List<string> sceneAssets)
        {
            // is there at least one asset with the name SceneManager?
            foreach (string sceneAsset in sceneAssets)
            {
                // if this file name ends with SceneManager.unity
                // TODO: Find a better testing mechanism than scene name
                if (sceneAsset.EndsWith("SceneManager.unity")) return true;
            }
            // couldn't find one
            return false;
        }


        protected List<string> CurrentSceneAssets()
        {
            // this is the final list of scenes
            List<string> projectScenes = new List<string>();
            // this is the list of valid folders we can search in
            List<string> searchableFolders = new List<string>();
            // get the list of all the root folders in our project
            string[] rootFolders = Directory.GetDirectories(Application.dataPath + "/");
            // go through each folder
            foreach (string subfolder in rootFolders)
            {
                // ignore these subfolders
                if (subfolder.EndsWith("Fungus") || subfolder.EndsWith("FungusManager")) continue;
                // ok, this is valid
                searchableFolders.Add("Assets/" + new DirectoryInfo(subfolder).Name);
            }

            string[] foundScenes = AssetDatabase.FindAssets("t:Scene", searchableFolders.ToArray());
            foreach(string scene in foundScenes){
                string path = AssetDatabase.GUIDToAssetPath(scene);
                projectScenes.Add(path);
            }

            return projectScenes;
        }


        protected string GetSceneAssetPath(string sceneName)
        {
            // this is the list of valid folders we can search in
            List<string> searchableFolders = new List<string>();
            // get the list of all the root folders in our project
            string[] rootFolders = Directory.GetDirectories(Application.dataPath + "/");
            // go through each folder
            foreach (string subfolder in rootFolders)
            {
                // ignore these subfolders
                if (subfolder.EndsWith("Fungus") || subfolder.EndsWith("FungusManager")) continue;
                // ok, this is valid
                searchableFolders.Add("Assets/" + new DirectoryInfo(subfolder).Name);
            }

            // this is the final list of scenes
            string[] foundScenes = AssetDatabase.FindAssets("t:Scene", searchableFolders.ToArray());
            foreach (string scene in foundScenes)
            {
                string path = AssetDatabase.GUIDToAssetPath(scene);
                // 
                if (path.EndsWith(sceneName)) {
                    return path;
                }
            }

            return "";
        }


        protected void CloseFungusSceneManager()
        {
            // first get a reference to the scene manager
            Scene managerScene = GetSceneManager();
            if (!managerScene.IsValid())
            {
                Debug.LogError("Scene Manager is already closed");
                return;
            }
            // make sure this is not the only loaded scene
            if (EditorSceneManager.GetActiveScene() == managerScene)
            {
                Debug.LogWarning("'SceneManager' cannot be closed because it is currently the active scene. Switch to another scene before closing.");
                return;
            }
            // close the scene
            EditorSceneManager.CloseScene(managerScene, true);
        }

        #endregion



        #region Scenes

        protected void CheckForSceneManager()
        {
            sceneManagerIsLoaded = IsSceneManagerLoaded();
            sceneManagerIsActive = IsSceneManagerActive();
        }


        protected bool IsSceneManagerLoaded()
        {
            // try to load the FungusSceneManager scene
            Scene managerScene = GetSceneManager();
            // if there is a valid scene here, return true
            if (managerScene.IsValid()) return true;
            // otherwise, we're not valid (result came up empty)
            return false;
        }


        protected bool IsSceneManagerActive()
        {
            // get a reference to the scene manager
            Scene managerScene = GetSceneManager();
            // check to see if the manager is the active scene
            if (EditorSceneManager.GetActiveScene() == managerScene) return true;
            return false;
        }


        protected Scene GetSceneManager()
        {
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                Scene scene = EditorSceneManager.GetSceneAt(i);

                // ignore scene that just closed
                if (!scene.IsValid() || !scene.isLoaded) continue;

                foreach (GameObject go in scene.GetRootGameObjects())
                {
                    FungusSceneManager fungusSceneManager = go.GetComponent<FungusSceneManager>();
                    if (fungusSceneManager != null)
                    {
                        return scene;
                    }
                }
            }

            Scene nullScene = new Scene();
            return nullScene;
        }

        #endregion


        #region callbacks

        virtual protected void SceneOpenedCallback(Scene newScene, OpenSceneMode mode)
        {
            CheckForSceneManager();
        }

        virtual protected void SceneSavedCallback(Scene scene)
        {
            CheckForSceneManager();
        }

        virtual protected void SceneClosedCallback(Scene closedScene)
        {
            CheckForSceneManager();
        }

        virtual protected void ActiveSceneChangedCallback(Scene oldScene, Scene newScene)
        {
            CheckForSceneManager();
        }

        virtual protected void SceneCreatedCallback(Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {
            CheckForSceneManager();
        }


        #endregion

    }

}
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

        protected bool projectContainsSceneManager = false;
        protected bool projectContainsStartScene = false;

        protected bool sceneManagerIsLoaded = false;
        protected bool sceneManagerIsActive = false;
        protected bool startSceneIsLoaded = false;
        protected bool startSceneIsActive = false;

        protected List<string> scenesInProject = new List<string>();

        #endregion


        #region Init

        virtual protected void Awake()
        {
            CheckScenes();
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

        }


        protected void LoadSceneButton(string sceneName, string path)
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Load '" + sceneName + "'"))
            {
                //LoadManagedScene(path, OpenSceneMode.Additive);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        #endregion



        #region Check

        virtual protected void CheckScenes()
        {
            // get the latest list of available scenes
            UpdateProjectSceneList();

            // get current state of Scene Manager
            projectContainsSceneManager = DoesSceneExist("SceneManager");
            // get current state of Start scene
            projectContainsStartScene = DoesSceneExist("Start");

            CheckForSceneManager();
            CheckForStartScene();
        }


        virtual protected void CheckForSceneManager()
        {
            sceneManagerIsLoaded = IsSceneLoaded(GetSceneManagerScene());
            sceneManagerIsActive = IsSceneActive(GetSceneManagerScene());
        }


        virtual protected void CheckForStartScene()
        {
            startSceneIsLoaded = IsSceneLoaded(GetLoadedSceneByName("Start"));
            startSceneIsActive = IsSceneActive(GetLoadedSceneByName("Start"));
        }


        protected bool IsSceneLoaded(Scene scene)
        {
            // if there is a valid scene here, return true
            if (scene.IsValid()) return true;
            // otherwise, we're not valid (result came up empty)
            return false;
        }


        protected bool IsSceneActive(Scene scene)
        {
            // check to see if the start scene is the active scene
            if (EditorSceneManager.GetActiveScene() == scene) return true;
            return false;
        }


        protected bool IsPathValid(string path)
        {
            // make sure there was a valid path
            if (path == "")
            {
                // send warning
                Debug.LogWarning("No folder selected");
                return false;
            }
            // make sure this is not the root folder
            if (path == Application.dataPath)
            {
                Debug.LogWarning("Cannot save to root 'Assets/' folder. Please select a project sub-folder.");
                return false;
            }
            // make sure this is not the FungusManager folder
            if (path.Contains(Application.dataPath + "/FungusManager"))
            {
                Debug.LogWarning("Cannot save into 'Assets/FungusManager' folder. Please select a different project sub-folder.");
                return false;
            }
            // make sure this is not the Fungus folder
            if (path.Contains(Application.dataPath + "/Fungus"))
            {
                Debug.LogWarning("Cannot save into 'Assets/Fungus' folder. Please select a different project sub-folder.");
                return false;
            }

            return true;
        }

        #endregion


        #region Find

        void UpdateProjectSceneList()
        {
            // get the latest list of available scenes
            scenesInProject = CurrentSceneAssets();
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
            // look inside those valid (non-Fungus) folders for Scenes
            string[] foundScenes = AssetDatabase.FindAssets("t:Scene", searchableFolders.ToArray());
            // go through each scene
            foreach (string scene in foundScenes)
            {
                string path = AssetDatabase.GUIDToAssetPath(scene);
                projectScenes.Add(path);
            }

            return projectScenes;
        }


        protected string GetSceneAssetPath(string sceneName)
        {
            // this is the list of valid folders we can search in
            List<string> currentSceneAssets = CurrentSceneAssets();

            foreach (string path in currentSceneAssets)
            {
                if (path.EndsWith(sceneName))
                {
                    return path;
                }
            }

            return "";
        }


        protected bool DoesSceneExist(string sceneName)
        {
            // convert to unity file name
            string sceneFileName = sceneName + ".unity";
            // go through all the scene names
            foreach (string name in scenesInProject)
            {
                // is this in here?
                if (name.EndsWith(sceneFileName))
                {
                    // ok, found it
                    return true;
                }
                // if
            }
            // foreach

            // couldn't find anything
            return false;
        }

        #endregion


        protected Scene GetLoadedSceneByName(string sceneName)
        {
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                Scene scene = EditorSceneManager.GetSceneAt(i);

                // ignore scene that just closed
                if (!scene.IsValid() || !scene.isLoaded) continue;

                if (scene.name == sceneName) return scene;
            }
            // return an empty scene
            return new Scene();
        }


        protected Scene GetSceneManagerScene()
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
            // return an empty scene
            return new Scene();
        }


        protected FungusSceneManager GetFungusSceneManagerScript()
        {
            Scene scene = GetSceneManagerScene();

            // make sure we actually got a scene
            if (!scene.IsValid()) return null;

            foreach (GameObject go in scene.GetRootGameObjects())
            {
                FungusSceneManager fungusSceneManager = go.GetComponent<FungusSceneManager>();
                if (fungusSceneManager != null)
                {
                    return fungusSceneManager;
                }
            }

            return null;
        }


        #region Load/Unload

        protected void LoadManagedScene(string scenePath, OpenSceneMode sceneMode, bool moveToTop = false)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, sceneMode);

            EditorSceneManager.SetActiveScene(scene);

            Scene firstScene = EditorSceneManager.GetSceneAt(0);

            if (moveToTop && firstScene != scene)
            {
                EditorSceneManager.MoveSceneBefore(scene, firstScene);
            }

            CheckScenes();
        }


        //protected List<string> CurrentSceneAssets()
        //{
        //    // this is the final list of scenes
        //    List<string> projectScenes = new List<string>();
        //    // this is the list of valid folders we can search in
        //    List<string> searchableFolders = new List<string>();
        //    // get the list of all the root folders in our project
        //    string[] rootFolders = Directory.GetDirectories(Application.dataPath + "/");
        //    // go through each folder
        //    foreach (string subfolder in rootFolders)
        //    {
        //        // ignore these subfolders
        //        if (subfolder.EndsWith("Fungus") || subfolder.EndsWith("FungusManager")) continue;
        //        // ok, this is valid
        //        searchableFolders.Add("Assets/" + new DirectoryInfo(subfolder).Name);
        //    }

        //    string[] foundScenes = AssetDatabase.FindAssets("t:Scene", searchableFolders.ToArray());
        //    foreach (string scene in foundScenes)
        //    {
        //        string path = AssetDatabase.GUIDToAssetPath(scene);
        //        projectScenes.Add(path);
        //    }

        //    return projectScenes;
        //}


        //protected void CloseFungusSceneManager()
        //{
        //    // first get a reference to the scene manager
        //    Scene managerScene = GetSceneManagerScene();
        //    if (!managerScene.IsValid())
        //    {
        //        Debug.LogError("Scene Manager is already closed");
        //        return;
        //    }
        //    // make sure this is not the only loaded scene
        //    if (EditorSceneManager.GetActiveScene() == managerScene)
        //    {
        //        Debug.LogWarning("'SceneManager' cannot be closed because it is currently the active scene. Switch to another scene before closing.");
        //        return;
        //    }
        //    // close the scene
        //    EditorSceneManager.CloseScene(managerScene, true);
        //}

        #endregion


        #region callbacks

        virtual protected void SceneOpenedCallback(Scene newScene, OpenSceneMode mode)
        {
            CheckScenes();
        }

        virtual protected void SceneSavedCallback(Scene scene)
        {
            CheckScenes();
        }

        virtual protected void SceneClosedCallback(Scene closedScene)
        {
            CheckScenes();
        }

        virtual protected void ActiveSceneChangedCallback(Scene oldScene, Scene newScene)
        {
            CheckScenes();
        }

        virtual protected void SceneCreatedCallback(Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {
            CheckScenes();
        }

        #endregion

    }

}

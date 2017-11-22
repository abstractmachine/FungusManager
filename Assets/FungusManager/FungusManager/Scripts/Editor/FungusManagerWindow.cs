using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Fungus
{
    public class FungusManagerWindow : EditorWindow
    {

        #region Members

        /// <summary>
        /// The folder where we last saved something
        /// </summary>
        protected string lastSaveFolder = "Assets/";

        /// <summary>
        /// Which folders should we avoid when saving or searching?
        /// </summary>
        protected string[] avoidFolders = { "Cinemachine", "Editor", "Fungus", "FungusManager", "FungusSceneManager", "Hyperzoom", "Metamorphoseon" };

        protected bool projectContainsSceneManager = false;
        protected bool projectContainsStartScene = false;
        protected bool projectContainsHyperzoom = false;

        protected bool sceneManagerIsLoaded = false;
        protected bool sceneManagerIsActive = false;
        protected bool startSceneIsLoaded = false;
        protected bool startSceneIsActive = false;

        protected List<string> scenesInProject = new List<string>();
        protected List<string> loadedScenes = new List<string>();

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

            EditorApplication.playModeStateChanged += PlayModeStateDidChange;

        }


        virtual protected void OnDisable()
        {
            EditorSceneManager.sceneOpened -= SceneOpenedCallback;
            EditorSceneManager.sceneSaved -= SceneSavedCallback;
            EditorSceneManager.sceneClosed -= SceneClosedCallback;
            EditorSceneManager.activeSceneChanged -= ActiveSceneChangedCallback;
            EditorSceneManager.newSceneCreated -= SceneCreatedCallback;

            EditorApplication.playModeStateChanged -= PlayModeStateDidChange;

        }

        #endregion


        #region GUI

        virtual protected void OnGUI()
        {
            // if there was a change in one of the assets
            if (FungusManagerAssetPostProcessor.didChange)
            {
                // recheck the state of the scenes
                CheckScenes();
                // reset flag
                FungusManagerAssetPostProcessor.ResetFlag();
            }
        }

        #endregion



        #region Check

        virtual protected void CheckScenes()
        {
            // 
            UpdateProjectSceneList();
            UpdateLoadedSceneList();
            // 
            CheckForSceneManager();
            CheckForStartScene();
            CheckForHyperzoom();
        }


        virtual protected void CheckForSceneManager()
        {
            // get current state of Scene Manager
            projectContainsSceneManager = DoesSceneExist("SceneManager");
            sceneManagerIsLoaded = IsSceneLoaded(GetSceneManagerScene());
            sceneManagerIsActive = IsSceneActive(GetSceneManagerScene());
        }


        virtual protected void CheckForStartScene()
        {
            // get current state of Start scene
            projectContainsStartScene = DoesSceneExist("Start");
            startSceneIsLoaded = IsSceneLoaded(GetLoadedSceneByName("Start"));
            startSceneIsActive = IsSceneActive(GetLoadedSceneByName("Start"));
        }


        virtual protected void CheckForHyperzoom()
        {
            // start under the assumption that it doesn't exist
            projectContainsHyperzoom = false;

            // if there isn't even the folder, abort
            if (!AssetDatabase.IsValidFolder("Assets/Hyperzoom")) return;

            // look inside those valid (non-Fungus) folders for Scenes
            string[] validScenes = { "Assets/Hyperzoom" };
            string[] foundScenes = AssetDatabase.FindAssets("t:Prefab", validScenes);
            // go through each scene
            foreach (string scene in foundScenes)
            {
                string path = AssetDatabase.GUIDToAssetPath(scene);
                // is this the one we're looking for?
                if (path.EndsWith("Cameras.prefab"))
                {
                    projectContainsHyperzoom = true;
                    return;
                }
            }
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

        #endregion


        #region Tools

        protected string GetFolderPath(string sceneName)
        {
            // if the scene exists already
            if (DoesSceneExist(sceneName))
            {
                // abort with an error
                Debug.LogWarning("Scene '" + sceneName + "' already exists.");
                return "";
            }

            // tell the user to select a path
            string path = EditorUtility.SaveFolderPanel("Select a folder for '" + sceneName + "'", lastSaveFolder, sceneName);
            lastSaveFolder = path;

            // check the path
            if (!IsPathValid(path)) return "";

            // remove full data path
            path = CleanUpPath(path);

            // does the scene need to be saved?
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                Debug.LogWarning("The active scene is not empty. Create a new scene before creating '" + sceneName + "'");
                return "";
            }

            // path is okay. Return the path string
            return path;
        }


        protected string CleanUpPath(string path)
        {
            // remove full data path
            if (path.StartsWith(Application.dataPath))
            {
                // remove start of full data path, just take the characters after the word "Assets"
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }
            // return cleaned up path
            return path;
        }


        protected bool DoesSceneExist(string sceneName)
        {
            // convert to unity file name
            string sceneFileName = sceneName;
            // if not already added, add the .unity extension to the filename
            if (!sceneName.EndsWith(".unity")) sceneFileName += ".unity";
            // go through all the scene names
            foreach (string name in scenesInProject)
            {
                // is this in here?
                if (name.EndsWith(sceneFileName)) return true;
            }
            // foreach

            // couldn't find anything
            return false;
        }


        protected List<string> CurrentScenePaths()
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
                // if this is one of the folders we should avoid, move on to next folder
                if (Array.IndexOf(avoidFolders, new DirectoryInfo(subfolder).Name) >= 0) continue;
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


        protected List<string> ManagedScenePaths()
        {
            List<string> scenePaths = new List<string>();
            // first load in all the current scenes in the build settings
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                scenePaths.Add(buildScene.path);
            }

            return scenePaths;
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
                Debug.LogWarning("Cannot save to root 'Assets/' folder. Please select a Project sub-folder.");
                return false;
            }

            // create a smaller path starting just from the Asset folder
            string assetPath = path;
            // if it's the full path
            if (assetPath.StartsWith(Application.dataPath))
            {
                // remove start of full data path, just take the characters after the word "Assets"
                assetPath = "Assets" + path.Substring(Application.dataPath.Length);
            }

            // go through each folder that we should avoid
            foreach (string avoidFolder in avoidFolders)
            {
                // get the root path
                string assetAvoidPath = "Assets/" + avoidFolder;
                // check with new name
                if (assetPath.StartsWith(assetAvoidPath))
                {
                    Debug.LogWarning("That is a restricted folder. Choose another Project folder than '" + assetPath + "'. Try placing your scene in 'Assets/Scenes'.");
                    return false;
                }
            }

            return true;
        }

        #endregion


        #region Find

        void UpdateProjectSceneList()
        {
            // get the latest list of available scenes
            scenesInProject = CurrentScenePaths();
        }


        protected void UpdateLoadedSceneList()
        {
            // clear the current list
            loadedScenes.Clear();
            // close the other scene
            for (int i = EditorSceneManager.sceneCount - 1; i >= 0; i--)
            {
                // get this scene
                Scene scene = EditorSceneManager.GetSceneAt(i);
                // add to list
                loadedScenes.Add(scene.name);
            }
        }


        protected string GetSceneAssetPath(string sceneName)
        {
            //foreach (string path in currentSceneAssets)
            foreach (string path in scenesInProject)
            {
                if (path.EndsWith(sceneName))
                {
                    return path;
                }
            }

            return "";
        }


        protected string GetPrefabPath(string prefabName)
        {
            // if necessary
            if (!prefabName.EndsWith(".prefab"))
            {
                // add .prefab at the end of this name
                prefabName += ".prefab";
            }
            // this is the list of valid folders we can search in
            List<string> searchableFolders = new List<string>();
            // get the list of all the root folders in our project
            string[] rootFolders = Directory.GetDirectories(Application.dataPath + "/");
            // go through each folder
            foreach (string subfolder in rootFolders)
            {
                // ignore these subfolders
                if (subfolder.EndsWith("Fungus") || subfolder.EndsWith("FungusManager") || subfolder.EndsWith("Hyperzoom") || subfolder.EndsWith("Cinemachine")) continue;
                // ok, this is valid
                searchableFolders.Add("Assets/" + new DirectoryInfo(subfolder).Name);
            }
            // look inside those valid (non-Fungus) folders for Scenes
            string[] foundScenes = AssetDatabase.FindAssets("t:Prefab", searchableFolders.ToArray());
            // go through each scene
            foreach (string scene in foundScenes)
            {
                string path = AssetDatabase.GUIDToAssetPath(scene);
                // is this the one we're looking for?
                if (path.EndsWith(prefabName))
                {
                    // send back path without the long complicated root path
                    return CleanUpPath(path);
                }
            }

            return "";
        }


        //protected int SceneManagerIndex()
        //{
        //    // first find the index of the scene manager
        //    int sceneManagerIndex = -1;
        //    // go through each scene
        //    for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        //    {
        //        Scene scene = EditorSceneManager.GetSceneAt(i);
        //        // if this is the scene manager
        //        if (scene.name.EndsWith("SceneManager"))
        //        {
        //            // return this index
        //            return i;
        //        }
        //    }
        //    // return the index
        //    return sceneManagerIndex;
        //}

        #endregion


        #region GetScenes

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

        #endregion


        #region Loading

        protected void LoadManagedScene(string scenePath, OpenSceneMode sceneMode, bool moveToTop = false, bool isSceneManager = false)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, sceneMode);

            Scene firstScene = EditorSceneManager.GetSceneAt(0);
            if (moveToTop && firstScene != scene)
            {
                MoveSceneToTop(scene);
            }

            if (!isSceneManager)
            {
                SetSceneToActive(scene);
            }

            CheckScenes();
        }


        protected void CloseOpenScenes()
        {
            Scene managerScene = GetSceneManagerScene();

            // close the other scene
            for (int i = EditorSceneManager.sceneCount - 1; i >= 0; i--)
            {
                Scene scene = EditorSceneManager.GetSceneAt(i);
                // leave manager scene
                if (managerScene == scene) continue;
                // close anything else
                if (!EditorSceneManager.CloseScene(scene, true))
                {
                    Debug.LogError("Couldn't close scene " + scene.name);
                    return;
                }
            }
        }


        protected void CloseOpenScene(string sceneName)
        {
            // close the other scene
            for (int i = EditorSceneManager.sceneCount - 1; i >= 0; i--)
            {
                Scene scene = EditorSceneManager.GetSceneAt(i);
                // leave manager scene
                if (sceneName != scene.name) continue;
                // close anything else
                if (!EditorSceneManager.CloseScene(scene, true))
                {
                    Debug.LogError("Couldn't close scene " + scene.name);
                    return;
                }
            }
        }


        protected void SaveOpenScene()
        {
            Scene managerScene = GetSceneManagerScene();

            // close the other scene
            for (int i = EditorSceneManager.sceneCount - 1; i >= 0; i--)
            {
                Scene scene = EditorSceneManager.GetSceneAt(i);
                // leave manager scene
                if (managerScene == scene) continue;
                // now check to see if this scene is dirty
                if (scene.isDirty)
                {
                    // save the scene
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }


        protected void SetSceneToActive(Scene scene)
        {
            EditorSceneManager.SetActiveScene(scene);
        }


        protected void MoveSceneToTop(Scene scene)
        {
            Scene firstScene = EditorSceneManager.GetSceneAt(0);
            EditorSceneManager.MoveSceneBefore(scene, firstScene);
        }

        #endregion


        #region BuildSettings

        protected void ClearBuildSettings()
        {
            // this is the final list of scenes
            List<EditorBuildSettingsScene> finalSceneList = new List<EditorBuildSettingsScene>();
            // Set the Build Settings window Scene list
            EditorBuildSettings.scenes = finalSceneList.ToArray();
            // save in local SceneManager variable
            SaveBuildSettingsInSceneManager();
        }


        protected void SaveSceneToBuildSettings(Scene newScene, bool isSceneManager = false)
        {
            AddScenePathToBuildSettings(newScene.path, isSceneManager);
        }


        protected void AddScenePathToBuildSettings(string addScenePath, bool isSceneManager = false)
        {
            // this is the final list of scenes
            List<EditorBuildSettingsScene> finalSceneList = new List<EditorBuildSettingsScene>();

            // we are going to check to see if the SceneManager is also in the build settings
            Scene sceneManagerScene = GetSceneManagerScene();

            if (!sceneManagerScene.IsValid())
            {
                Debug.LogError("SceneManager is invalid");
                return;
            }

            // always add the SceneManager first
            if (!isSceneManager)
            {
                EditorBuildSettingsScene buildSettingsScene = new EditorBuildSettingsScene(sceneManagerScene.path, true);
                finalSceneList.Add(buildSettingsScene);
            }

            bool newSceneAlreadyInBuildSettings = false;

            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                bool thisSceneAlreadyInList = false;
                foreach (EditorBuildSettingsScene checkScene in finalSceneList)
                {
                    if (buildScene.path == checkScene.path) thisSceneAlreadyInList = true;
                    if (buildScene.path == addScenePath) newSceneAlreadyInBuildSettings = true;
                    if (checkScene.path == addScenePath) newSceneAlreadyInBuildSettings = true;
                }
                // add to list
                if (!thisSceneAlreadyInList)
                {
                    finalSceneList.Add(buildScene);
                }
            }

            // if we are not already added to the list
            if (!newSceneAlreadyInBuildSettings)
            {   // add to the main build settings list
                finalSceneList.Add(new EditorBuildSettingsScene(addScenePath, true));
            }

            // Set the Build Settings window Scene list
            EditorBuildSettings.scenes = finalSceneList.ToArray();

            // save in local SceneManager variable
            SaveBuildSettingsInSceneManager();
        }


        protected void RemoveSceneFromBuildSettings(string sceneName)
        {
            // this is the final list of scenes
            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();

            string unitySceneName = sceneName + ".unity";

            // go through each item
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                // ignore this one
                if (buildScene.path.EndsWith("/" + unitySceneName)) continue;
                // add to list
                editorBuildSettingsScenes.Add(buildScene);
            }

            // Set the Build Settings window Scene list
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();

            // save in local SceneManager variable
            SaveBuildSettingsInSceneManager();

        }


        public void SaveBuildSettingsInSceneManager()
        {
            Scene sceneManagerScene = GetSceneManagerScene();
            // make sure there was a scene manager
            if (!sceneManagerScene.IsValid())
            {
                Debug.LogError("Couldn't find SceneManager");
                return;
            }
            // create an empty list
            List<string> scenesToAdd = new List<string>();

            // first load in all the current scenes in the build settings
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                // if this is not the manager scene
                if (sceneManagerScene.path != buildScene.path)
                {
                    // name without extension
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(buildScene.path);
                    //scenePathsToAdd.Add(buildScene.path);
                    scenesToAdd.Add(sceneName);
                }
            }

            // get access to the SceneManager
            FungusSceneManager fungusSceneManagerScript = GetFungusSceneManagerScript();
            // tell the mananger to save it's paths
            fungusSceneManagerScript.SetScenes(scenesToAdd);

            // force save
            EditorSceneManager.SaveScene(sceneManagerScene);
        }

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

        virtual protected void PlayModeStateDidChange(PlayModeStateChange state)
        {
            CheckScenes();
        }

        #endregion

    }

    #region Post-Processor

    class FungusManagerAssetPostProcessor : AssetPostprocessor
    {
        public static bool didChange = false;

        public static void ResetFlag()
        {
            didChange = false;
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            didChange = true;
        }
    }
    // class FungusManagerAssetPostProcessor

    #endregion

}
// Fungus

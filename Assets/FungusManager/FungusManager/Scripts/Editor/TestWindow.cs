using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

public class TestWindow : EditorWindow
{
    
    #region Properties

    /// <summary>
    /// The folder where we last saved something
    /// </summary>
    protected string lastSaveFolder = "Assets/";

    /// <summary>
    /// Which folders should we avoid when saving or searching?
    /// </summary>
    protected string[] avoidFolders = { "Cinemachine", "Editor", "Fungus", "FungusManager", "FungusSceneManager", "Hyperzoom", "Metamorphoseon" };

    #endregion


    #region Window

    //// Add menu item
    //[MenuItem("Tools/Test Window")]
    //public static void ShowWindow()
    //{
    //    //Show existing window instance. If one doesn't exist, make one.
    //    EditorWindow.GetWindow<TestWindow>("Test Window");
    //}

    #endregion


    #region GUI

    void OnGUI()
    {

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical();

        if (GUILayout.Button("Create Scene Manager"))
        {
            CreateFungusSceneManager();
            return;
        }

        if (GUILayout.Button("Create Start Scene"))
        {
            CreateNewScene("Start");
            return;
        }

        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
    }

    #endregion


    #region Create

    protected void CreateFungusSceneManager()
    {
        // open & verify a path where we can save the SceneManager
        string path = GetFolderPath("SceneManager");

        // make sure we got a valud path
        if (path == "") return;

        // Create the SceneManager
        Scene sceneManagerScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // make sure the scene we created was valid
        if (!sceneManagerScene.IsValid()) {
            Debug.LogError("Invalid Scene. Could not create scene '" + path + "'.");
            return;
        }

        // get all the cameras
        Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
        foreach (Camera camera in cameras)
        {
            // if this is our scene's camera
            if (camera == Camera.main)
            {
                // change the values
                camera.orthographic = true;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.cullingMask = 0;
                camera.depth = -99;
                camera.backgroundColor = Color.grey;
                camera.useOcclusionCulling = false;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                FlareLayer flareLayer = camera.gameObject.GetComponent<FlareLayer>();
                DestroyImmediate(flareLayer);
                camera.name = "Manager Camera";
            }
        }

        // remove the lights
        Light[] lights = GameObject.FindObjectsOfType<Light>();
        for (int i = lights.Length - 1; i >= 0; --i)
        {
            DestroyImmediate(lights[i].gameObject);
        }

        // try to save
        if (!EditorSceneManager.SaveScene(sceneManagerScene, path + "/SceneManager.unity", false))
        {
            Debug.LogWarning("Couldn't create FungusSceneManager");
        }

    }


    void CreateNewScene(string sceneName)
    {
        // open & verify a path where we can save this scene
        string path = GetFolderPath(sceneName);

        // make sure we got a valud path
        if (path == "") return;

        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);

        // try to save
        if (!EditorSceneManager.SaveScene(newScene, path + "/" + sceneName + ".unity", false))
        {
            Debug.LogWarning("Couldn't create '" + sceneName + "' scene");
        }

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
        lastSaveFolder = path; // CleanUpPath(path + "/");

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
        // if needed, add the .unity extension
        if (!sceneName.EndsWith(".unity")) sceneFileName += ".unity";
        // go through all the scene names
        List<string> scenePathsInProject = CurrentScenePaths();
        // go through each
        foreach (string name in scenePathsInProject)
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


    protected bool isFolderValid(string assetPath)
    {
        // go through each folder
        foreach (string avoidFolder in avoidFolders)
        {
            // get the root path
            string assetAvoidPath = "Assets/" + avoidFolder;
            // check with new name
            if (assetPath == assetAvoidPath) return false;
        }

        return true;
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
        // make sure this is not the FungusManager folder
        if (path.Contains(Application.dataPath + "/Fungus"))
        {
            Debug.LogWarning("Cannot save into '" + path +  "'. Please select a different sub-folder in your Project.");
            return false;
        }

        return true;
    }

    #endregion

}

using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;

using System.Collections.Generic;

namespace Fungus
{

    [CustomEditor(typeof(FungusSceneManager))]
    public class FungusSceneManagerEditor : Editor
    {
        #region Properties

        private bool foldoutScenes = true;

        #endregion


        #region GUI

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Separator();

            DrawScenes();

            EditorGUILayout.Separator();
        }
        // OnInspectorGUI()


        void DrawScenes()
        {
            // get access to the main script (we are an editor script)
            FungusSceneManager manager = (FungusSceneManager)target;

            // the Save Scene Button

            EditorGUILayout.Space();

            // the List of saved Scenes

            //EditorGUI.BeginChangeCheck();
            
            //if (GUILayout.Button("Save Build Settings Scene List"))
            //{
            //    SaveSceneList();
            //    Undo.RecordObject(target, "Save Scene List");
            //}
            //EditorGUI.EndChangeCheck();

            EditorGUILayout.Space();

            foldoutScenes = EditorGUILayout.Foldout(foldoutScenes, "Scenes (" + manager.scenes.Count + ")");

            if (foldoutScenes)
            {
                //foreach (string path in manager.paths)
                foreach (string sceneName in manager.scenes)
                {
                    //Scene scene = EditorSceneManager.GetSceneByName(sceneName);
                    //                string filename = System.IO.Path.GetFileName(path);
                    //string name = System.IO.Path.GetFileNameWithoutExtension(path);
                    EditorGUILayout.LabelField(sceneName);
                }

                this.Repaint();
            }

            EditorGUILayout.Space();
        }

        #endregion


        #region Save

        void SaveSceneList()
        {
            // get access to the main script (we are an editor script)
            FungusSceneManager manager = (FungusSceneManager)target;

            // create an empty list
            //List<string> scenePathsToAdd = new List<string>();
            List<string> scenesToAdd = new List<string>();

            // first load in all the current scenes in the build settings
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                // if this is not the manager scene
                if (manager.gameObject.scene.path != buildScene.path)
                {
                    // name without extension
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(buildScene.path);
                    //scenePathsToAdd.Add(buildScene.path);
                    scenesToAdd.Add(sceneName);
                }
            }

            // tell the mananger to save it's paths
            manager.scenes = scenesToAdd;

            // set the current scene as "dirty"
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        #endregion

    }
	// FungusSceneManagerEditor




	// <summary>
	// Saves all the scenes accompagnying the active scene
	// </summary>

	//void SaveScenes()
	//{
	//    // create an empty list
	//    List<string> scenePathsToAdd = new List<string>();

	//    // what is the current scene path?
	//    Scene currentScene = EditorSceneManager.GetActiveScene();

	//    // check all the loaded scenes to see if they're already loaded
	//    for (int i = 0; i < EditorSceneManager.sceneCount; ++i)
	//    {
	//        // if this is not the current scene
	//        if (EditorSceneManager.GetSceneAt(i) != currentScene)
	//        {
	//            // add this scene to list
	//            scenePathsToAdd.Add(EditorSceneManager.GetSceneAt(i).path);
	//        }
	//    }

	//    // get access to the main script (we are an editor script)
	//    Manager manager = (Manager)target;
	//    // tell it to save it's members
	//    manager.SaveScenePaths(scenePathsToAdd);

	//    // set the current scene as "dirty"
	//    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

	//    // now check the Build Settings to make sure each of these scenes is in the build settings
	//    SaveScenesInBuildSettings(scenePathsToAdd);

	//}

	// <summary>
	// Check the Build Settings to make sure each of these scenes is in the build settings
	// </summary>

	//void SaveScenesInBuildSettings(List<string> scenePathsToAdd)
	//{
	//    // what is the current scene?
	//    Scene currentScene = EditorSceneManager.GetActiveScene();
	//    // check to see if current scene is in build settings already
	//    bool currentSceneIsInBuild = false;

	//    // create a list for all the scenes we want in build settings
	//    List<EditorBuildSettingsScene> finalBuildScenes = new List<EditorBuildSettingsScene>();

	//    // first load in all the current scenes in the build settings
	//    foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
	//    {
	//        finalBuildScenes.Add(buildScene);
	//        // is this the current scene
	//        if (currentScene.path == buildScene.path)
	//        {
	//            currentSceneIsInBuild = true;
	//        }
	//    }

	//    // create a version of the current scene for the build settings
	//    EditorBuildSettingsScene currentBuildScene = new EditorBuildSettingsScene(currentScene.path, true);

	//    // if the current scene is not already in the list
	//    if (!currentSceneIsInBuild)
	//    {
	//        // add the current active scene to this list
	//        finalBuildScenes.Add(currentBuildScene);
	//    }

	//    // now go through the list of scenes we want to add
	//    foreach (string scenePathToAdd in scenePathsToAdd)
	//    {
	//        // first check to see if it's already in the list of scenes
	//        bool isAlreadyInSettings = false;

	//        // first load in all the current scenes in the build settings
	//        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
	//        {
	//            if (buildScene.path == scenePathToAdd)
	//            {
	//                isAlreadyInSettings = true;
	//                break;
	//            }
	//        }

	//        // if this scene is already in build settings
	//        if (isAlreadyInSettings)
	//        {
	//            // move on to next scene path to add
	//            continue;
	//        }

	//        // add this additional scenes
	//        finalBuildScenes.Add(new EditorBuildSettingsScene(scenePathToAdd, true));
	//    }

	//    // ok, we've got a full list now. Set the Build Settings window Scene list
	//    EditorBuildSettings.scenes = finalBuildScenes.ToArray();

	//}
	// SaveScenes()

}
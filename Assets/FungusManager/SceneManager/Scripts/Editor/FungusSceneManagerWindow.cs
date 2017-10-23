using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Fungus
{

    public class FungusSceneManagerWindow : FungusManagerWindow
    {
        #region Members

        private bool newSceneFoldout = true;
        private string sceneName = "SceneName";
        private bool managedScenesFoldout = true;
        private bool availableScenesFoldout = true;

        private FungusSceneManager fungusSceneManager = null;
        private int managedSceneCount = 0;
        private int availableSceneCount = 0;

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

            if (sceneManagerIsLoaded)
            {
                DisplaySceneManager();
            }
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
                sceneName = EditorGUILayout.TextField("", sceneName);

                // convert the above string into ligatures and print out into console
                if (GUILayout.Button("Create New Scene"))
                {
                    Debug.Log("Create new scene '" + sceneName + "'");
                }

            } // if (newScene)

            GUILayout.Space(20);

            managedScenesFoldout = EditorGUILayout.Foldout(managedScenesFoldout, "Current Scenes (" + managedSceneCount + ")");

            if (managedScenesFoldout)
            {
                DisplayScenes();
            }

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.Space(20);

            GUILayout.BeginVertical();

            GUILayout.Space(20);

            availableScenesFoldout = EditorGUILayout.Foldout(availableScenesFoldout, "Available Scenes (" + availableSceneCount + ")");

            if (availableScenesFoldout)
            {
                DisplayAvailableScenes();
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            // FLEXIBLE SPACE
        }

        #endregion



        #region Scenes

        override protected void CheckForSceneManager()
        {
            base.CheckForSceneManager();

            if (fungusSceneManager == null)
            {
                fungusSceneManager = GetFungusSceneManagerScript();
            }

            UpdateManagedSceneList();
            UpdateAvailableSceneList();
        }


        private void UpdateManagedSceneList()
        {
            if (fungusSceneManager == null) return;

            managedSceneCount = fungusSceneManager.scenes.Count;
        }


        private void DisplayScenes()
        {
            if (fungusSceneManager == null) return;

            foreach (string scene in fungusSceneManager.scenes)
            {
                DisplayScene(scene);
            }
        }


        private void UpdateAvailableSceneList()
        {
            
        }


        private void DisplayAvailableScenes()
        {
            
        }


        private void DisplayScene(string sceneName)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(sceneName);
            GUILayout.EndHorizontal();
        }

        #endregion

    }

}
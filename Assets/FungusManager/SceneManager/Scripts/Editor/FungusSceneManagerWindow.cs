using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Fungus
{

    public class FungusSceneManagerWindow : FungusManagerWindow
    {
        #region Members

        private bool newScene = true;
        private string sceneName = "SceneName";
        private bool currentScenes = true;

        private FungusSceneManager fungusSceneManager;

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

            // CLOSE WINDOW

            if (!sceneManagerIsActive)
            {
                // convert the above string into ligatures and print out into console
                if (GUILayout.Button("Close 'SceneManager'"))
                {
                    CloseFungusSceneManager();
                }

                GUILayout.Space(20);
            }

            // CREATE NEW SCENE

            newScene = EditorGUILayout.Foldout(newScene, "New Scene");

            if (newScene)
            {
                sceneName = EditorGUILayout.TextField("", sceneName);

                // convert the above string into ligatures and print out into console
                if (GUILayout.Button("Create New Scene"))
                {
                    Debug.Log("Create new scene '" + sceneName + "'");
                }

                if (GUILayout.Button("Update Scene List"))
                {
                    UpdateSceneList();
                }

            } // if (newScene)

            GUILayout.Space(20);

            currentScenes = EditorGUILayout.Foldout(currentScenes, "Current Scenes (" + 0 + ")");

            if (currentScenes)
            {
                DisplayScenes();
            }

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.Space(20);

            GUILayout.EndHorizontal();

            // FLEXIBLE SPACE
        }

        private void DisplayScenes()
        {
            DisplayScene("SceneName-A");
            DisplayScene("SceneName-B");
            DisplayScene("SceneName-C");
        }


        private void DisplayScene(string sceneName)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(sceneName);
            GUILayout.EndHorizontal();
        }

        #endregion



        #region Scenes

        private void UpdateSceneList()
        {

        }

        #endregion

    }

}
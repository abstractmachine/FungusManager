using UnityEngine;
using UnityEngine.UI;
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

        #region Properties

        private bool bakeLightingManually = true;

        private bool addHyperzoomControls = true;
        private bool addHyperzoomPointerInput = true;
        private bool addHyperzoomKeyboardInput = true;
        private bool addHyperzoomJoystickInput = false;

        //        //private bool createCharactersPrefab = true;

        private bool newSceneFoldout = true;
        private bool addSceneFoldout = true;

        private string sceneName = "Start";
        private bool managedScenesFoldout = true;

        private Object addSceneObject = null;

        private Vector2 displayScenesScroll = Vector2.zero;

        private Texture2D sceneIcon;

        #endregion


        #region Window

        // Add menu item
        [MenuItem("Tools/Fungus Manager/Scene Manager Window")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow<FungusSceneManagerWindow>("Scene Manager");
        }

        #endregion


        #region Callbacks

        override protected void OnEnable()
        {
            base.OnEnable();
            Lightmapping.completed += LightmappingCompleted;
        }


        override protected void OnDisable()
        {
            base.OnDisable();
            Lightmapping.completed -= LightmappingCompleted;
        }

        #endregion


        #region GUI

        override protected void OnGUI()
        {
            base.OnGUI();

            // check to see if there is at least one scene manager in the project
            if (!projectContainsSceneManager)
            {
                CreateSceneManagerButton();
                return;
            }

            if (!sceneManagerIsLoaded)
            {
                LoadSceneManagerButton();
                return;
            }

            // if the scene manager is not already loaded
            if (sceneManagerIsLoaded)
            {
                DisplaySceneManager();
                return;
            }

        }


        protected void CreateSceneManagerButton()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();

            GUILayout.Space(10);

            // Scene Manager

            if (GUILayout.Button("Create 'SceneManager'"))
            {
                CreateFungusSceneManager();
                return;
            }

            // lighting

            GUILayout.Space(10);

            bool previousBakeSetting = bakeLightingManually;
            bakeLightingManually = GUILayout.Toggle(bakeLightingManually, "Manually Bake Lighting");

            // did we change?
            if (bakeLightingManually != previousBakeSetting)
            {
                if (bakeLightingManually) SetBakeToManual();
                else SetBakeToAuto();
            }

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }


        protected void LoadSceneManagerButton() 
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();

            // if we've pushed the button
            if (GUILayout.Button("Load 'SceneManager'"))
            {
                OpenSceneMode openSceneMode = OpenSceneMode.Single;

                // go through each loaded scene
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    // get this scene
                    Scene editorScene = EditorSceneManager.GetSceneAt(i);
                    // if this is the scene manager move on
                    if (editorScene.name == "SceneManager") continue;
                    // now check to see if this is one of the managed scenes
                    foreach (EditorBuildSettingsScene buildSettingsScene in EditorBuildSettings.scenes)
                    {
                        // is this indeed one of the managed scenes?
                        if (editorScene.path == buildSettingsScene.path)
                        {
                            // set the flag
                            openSceneMode = OpenSceneMode.Additive;
                            // no need to check the others
                            break;
                        }
                        // if
                    }
                    // foreach(BuildSettingsScene
                }
                // for (EditorSceneManager

                // load the scene into the hierarchy
                LoadManagedScene(GetSceneAssetPath("SceneManager.unity"), openSceneMode, true, true);

                return;
            }

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }


        private void DisplaySceneManager()
        {
            // scene controls

            GUILayout.BeginVertical();

            GUILayout.Space(10);

            // lighting

            bool previousBakeSetting = bakeLightingManually;
            bakeLightingManually = GUILayout.Toggle(bakeLightingManually, "Manually Bake Lighting");

            // did we change?
            if (bakeLightingManually != previousBakeSetting)
            {
                if (bakeLightingManually) SetBakeToManual();
                else SetBakeToAuto();
            }

            // do we bake lighting manually (through this button) or automatically (off)?
            if (bakeLightingManually)
            {
                // start the bake process
                if (GUILayout.Button("Bake Scenes", GUILayout.ExpandWidth(false)))
                {
                    BakeAllScenes();
                }
            }

            GUILayout.Space(10);

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
                DisplayScenes();
            }

            GUILayout.EndVertical();

        }


        void GUIDrawSceneOptions()
        {
            // createCharactersPrefab = GUILayout.Toggle(createCharactersPrefab, "Create Characters prefab", GUILayout.MinWidth(80), GUILayout.MaxWidth(200));

            if (projectContainsHyperzoom)
            {
                addHyperzoomControls = GUILayout.Toggle(addHyperzoomControls, "Add Hyperzoom", GUILayout.MinWidth(80), GUILayout.MaxWidth(200));

                // the joystick controller is attached to the hyperzoom
                if (addHyperzoomControls)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    addHyperzoomPointerInput = GUILayout.Toggle(addHyperzoomPointerInput, "Touch & Mouse Pointer");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    addHyperzoomKeyboardInput = GUILayout.Toggle(addHyperzoomKeyboardInput, "Keyboard");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    addHyperzoomJoystickInput = GUILayout.Toggle(addHyperzoomJoystickInput, "Joystick Controller");
                    GUILayout.EndHorizontal();
                }
            }
        }


        #endregion


        #region Lighting

        protected void SetBakeToManual()
        {
            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
        }

        protected void SetBakeToAuto()
        {
            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.Iterative;
        }

        protected void BakeAllScenes()
        {
            // close all open scenes
            CloseOpenScenes();

            SetBakeToManual();
            // force save
            EditorSceneManager.SaveScene(GetSceneManagerScene());
            // start baking everything
            Lightmapping.BakeMultipleScenes(ManagedScenePaths().ToArray());
        }


        void LightmappingCompleted()
        {
        }


        /// <summary>
        /// This is purely a cosmetic feature. After baking all scenes,
        /// the scene foldouts are folded in the hierarchy view. This unfolds them
        /// </summary>
        void UnfoldScenesInHierarchy()
        {
            CloseOpenScenes();

            // get access to the hierarchy
            EditorApplication.ExecuteMenuItem("Window/Hierarchy");
            EditorWindow hierarchy = EditorWindow.focusedWindow;

            // go through each scene in the hierarchy
            for (int i = EditorSceneManager.sceneCount-1; i >= 0; i--)
            {
                // get this scene
                Scene scene = EditorSceneManager.GetSceneAt(i);
                UnfoldScene(hierarchy, scene);
            }
        }


        void UnfoldScene(EditorWindow hierarchy, Scene scene)
        { 
            // get this scene's root objects
            GameObject[] rootObjects = scene.GetRootGameObjects();
            // make sure there are some game objects
            if (rootObjects.Length == 0) return;
            // select the first root object
            Selection.activeObject = rootObjects[0];
            // unfold the object triangle
            hierarchy.SendEvent(new Event { keyCode = KeyCode.UpArrow, type = EventType.keyDown });
            hierarchy.SendEvent(new Event { keyCode = KeyCode.RightArrow, type = EventType.keyDown });
            // unselect object
            Selection.activeObject = null;
        }

        #endregion


        #region Create

        protected void CreateFungusSceneManager()
        {
            // open & verify a path where we can save the SceneManager
            string path = GetFolderPath("SceneManager");

            // make sure we got a valid path
            if (path == "") return;

            // Create the SceneManager
            Scene sceneManagerScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // make sure the scene we created was valid
            if (!sceneManagerScene.IsValid())
            {
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
                    camera.name = "ManagerCamera";
                }
            }

            // remove the lights
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            for (int i = lights.Length - 1; i >= 0; --i)
            {
                //lights[i].gameObject.SetActive(false);
                DestroyImmediate(lights[i].gameObject);
            }

            // add prefabs to scene

            // add the SceneManager prefab
            GameObject sceneManagerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/FungusManager/SceneManager/Prefabs/SceneManager.prefab", typeof(GameObject));
            if (sceneManagerPrefab == null)
            {
                Debug.LogError("Couldn't load SceneManager prefab");
                return;
            }
            GameObject sceneManagerGameObject = PrefabUtility.InstantiatePrefab(sceneManagerPrefab, sceneManagerScene) as GameObject;
            // disconnect this object from the prefab (in package folder) that created it
            PrefabUtility.DisconnectPrefabInstance(sceneManagerGameObject);

            // add the flowcharts empty object
            //GameObject flowchartsPrefab = Resources.Load<GameObject>("SceneManager/Prefabs/Flowcharts");
            GameObject flowchartsPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/FungusManager/SceneManager/Prefabs/Flowcharts.prefab", typeof(GameObject));
            if (flowchartsPrefab == null)
            {
                Debug.LogError("Couldn't load Flowcharts prefab");
                return;
            }
            GameObject flowchartsGameObject = PrefabUtility.InstantiatePrefab(flowchartsPrefab, sceneManagerScene) as GameObject;
            flowchartsGameObject.transform.SetSiblingIndex(3);
            // disconnect this object from the prefab (in package folder) that created it
            PrefabUtility.DisconnectPrefabInstance(flowchartsGameObject);

            // add an empty Flowchart from Fungus
            GameObject flowchartPrefab = Resources.Load<GameObject>("Prefabs/Flowchart");
            if (flowchartPrefab == null)
            {
                Debug.LogError("Couldn't load Fungus Flowchart prefab");
                return;
            }
            GameObject flowchartGameObject = PrefabUtility.InstantiatePrefab(flowchartPrefab, sceneManagerScene) as GameObject;
            PrefabUtility.DisconnectPrefabInstance(flowchartGameObject);

            flowchartGameObject.name = "SceneManagement";
            // attach this flowchart to the flowcharts GameObject
            flowchartGameObject.transform.parent = flowchartsGameObject.transform;

            // find the default block in Flowchart
            Block defaultBlock = flowchartGameObject.GetComponent<Block>();
            defaultBlock.BlockName = "Start";

            // by default, add a 'Start' scene to this Flowchart
            RequestManagedScene requestManagedScene = flowchartGameObject.AddComponent<RequestManagedScene>();
            requestManagedScene.sceneName = "Start";

            // get the flowchart script
            defaultBlock.CommandList.Add(requestManagedScene);

            // try to save
            if (!EditorSceneManager.SaveScene(sceneManagerScene, path + "/SceneManager.unity", false))
            {
                Debug.LogWarning("Couldn't create FungusSceneManager");
                return;
            }

            // add this new scene to the build settings
            ClearBuildSettings();
            SaveSceneToBuildSettings(sceneManagerScene, true);

            // if the SceneManager is loaded, make it active
            SetSceneToActive(sceneManagerScene);
            MoveSceneToTop(sceneManagerScene);

            if (bakeLightingManually) BakeAllScenes();

            CheckScenes();

        }
        // CreateFungusSceneManager()


        void CreateNewScene(string sceneName)
        {
            // open & verify a path where we can save this scene
            string path = GetFolderPath(sceneName);

            // make sure we got a valid path
            if (path == "") return;

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // make sure the scene we got back was valid
            if (!newScene.IsValid())
            {
                Debug.LogWarning("Invalid scene created.");
                return;
            }

            // add prefabs to scene

            // hyperzoom is optional
            if (projectContainsHyperzoom && addHyperzoomControls)
            {
                CreateHyperzoom(newScene);
            }

            //if (createCharactersPrefab)
            //{
            //    CreateCharacters(newScene);
            //}

            // try to save
            if (!EditorSceneManager.SaveScene(newScene, path + "/" + sceneName + ".unity", false))
            {
                Debug.LogWarning("Couldn't create 'Start' scene");
            }

            // load the scene manager
            LoadManagedScene(GetSceneAssetPath("SceneManager.unity"), OpenSceneMode.Additive, true);

            // add this new scene to the build settings
            SaveSceneToBuildSettings(newScene);

            // now that we've saved, switch back to SceneManager
            Scene sceneManagerScene = GetSceneManagerScene();
            if (sceneManagerScene.IsValid())
            {
                MoveSceneToTop(sceneManagerScene);
            }

            SetSceneToActive(newScene);

            CheckScenes();
        }


        void AddScene(SceneAsset addSceneAsset)
        {
            string addPath = AssetDatabase.GetAssetPath(addSceneAsset);
            AddScenePathToBuildSettings(addPath);
        }


        void CreateHyperzoom(Scene newScene)
        {
            // go through all the current cameras
            for (int i = Camera.allCamerasCount-1; i >= 0; i--)
            {
                // check each one
                Camera thisCamera = Camera.allCameras[i];
                // if this camera is in our scene
                if (thisCamera.gameObject.scene == newScene)
                {
                    // destroy the default camera
                    DestroyImmediate(thisCamera.gameObject);
                }
            }

            GameObject camerasPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Hyperzoom/Prefabs/Cameras.prefab", typeof(GameObject));
            GameObject camerasGameObject = PrefabUtility.InstantiatePrefab(camerasPrefab, newScene) as GameObject;

            // get access to the main camera
            GameObject mainCameraGameObject = camerasGameObject.transform.Find("Main").gameObject;

            // get access to the hyperzoom child
            GameObject hyperzoomGameObject = camerasGameObject.transform.Find("Hyperzoom").gameObject;

            // controller input is optional
            if (!addHyperzoomJoystickInput)
            {
                Component hyperzoomJoystick = hyperzoomGameObject.GetComponent("HyperzoomJoystick");
                DestroyImmediate(hyperzoomJoystick);
            }
            // controller input is optional
            if (!addHyperzoomKeyboardInput)
            {
                Component hyperzoomKeyboard = hyperzoomGameObject.GetComponent("HyperzoomKeyboard");
                DestroyImmediate(hyperzoomKeyboard);
            }
            // controller input is optional
            if (!addHyperzoomPointerInput)
            {
                Component hyperzoomPointer = hyperzoomGameObject.GetComponent("HyperzoomPointer");
                DestroyImmediate(hyperzoomPointer);
            }

            // set the background object
            GameObject backgroundPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Hyperzoom/Prefabs/Background.prefab", typeof(GameObject));
            GameObject backgroundGameObject = PrefabUtility.InstantiatePrefab(backgroundPrefab, newScene) as GameObject;
            // set it's name
            backgroundGameObject.name = "Background";
            // attach it to the cameras object
            backgroundGameObject.transform.SetParent(camerasGameObject.transform);
            // set the canvas on this background object to the main camera (this is due to a unity bug: cf. http://bit.ly/2hHfOaF)
            Canvas backgroundCanvas = backgroundGameObject.GetComponent<Canvas>();
            backgroundCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            backgroundCanvas.worldCamera = mainCameraGameObject.GetComponent<Camera>();
            // now set the background canvas to fit
            CanvasScaler backgroundCanvasScaler = backgroundGameObject.GetComponent<CanvasScaler>();
            backgroundCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            backgroundCanvasScaler.referenceResolution = new Vector2(4000, 4000);

        }


        //        void CreateCharacters(Scene newScene)
        //        {
        //            //GameObject charactersPrefab = Resources.Load<GameObject>("CharacterManager/Prefabs/Characters");
        //            //// this is the path to the prefab
        //            ////string charactersPrefabPath = "Assets/FungusManager/CharacterManager/Prefabs/FungusCharacters.prefab";
        //            //// find out if there already is a prefab in our project
        //            //string projectCharactersPrefabPath = GetPrefabPath("FungusCharacterManager");
        //            //// if we found something
        //            //if (projectCharactersPrefabPath != "")
        //            //{
        //            //    // use this prefab path instead of the one in the project path
        //            //    charactersPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(projectCharactersPrefabPath, typeof(GameObject));
        //            //}

        //            //GameObject charactersGameObject = PrefabUtility.InstantiatePrefab(charactersPrefab, newScene) as GameObject;

        //            //// if this is a new prefab
        //            //if (projectCharactersPrefabPath == "")
        //            //{
        //            //    // make sure this prefab goes into the same folder at the Start scene's folder
        //            //    string newPrefabFolder = path + "/FungusCharacterManager.prefab";
        //            //    // save it to new position
        //            //    GameObject newPrefab = PrefabUtility.CreatePrefab(newPrefabFolder, charactersGameObject) as GameObject;
        //            //    // set this as our prefab
        //            //    PrefabUtility.ConnectGameObjectToPrefab(charactersGameObject, newPrefab);
        //            //}
        //        }


        //        protected Scene GetCleanScene(bool emptyScene = true)
        //        {
        //            Scene managerScene = GetSceneManagerScene();

        //            // close the other scene
        //            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        //            {
        //                Scene scene = EditorSceneManager.GetSceneAt(i);
        //                // leave manager scene
        //                if (managerScene.IsValid() && managerScene == scene) continue;
        //                // close anything else
        //                if (!EditorSceneManager.CloseScene(scene, true))
        //                {
        //                    Debug.LogError("Couldn't close scene " + scene.name);
        //                    return new Scene();
        //                }
        //            }

        //            if (managerScene.IsValid())
        //            {
        //                SetSceneToActive(managerScene);
        //            }

        //            // return an empty or default scene depending on whether we're using the Hyperzoom system or not
        //            if (emptyScene) return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        //            return EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);

        //        }

        #endregion



        #region Scene List

        private void DisplayScenes()
        {
            FungusSceneManager fungusSceneManagerScript = GetFungusSceneManagerScript();

            if (fungusSceneManagerScript == null) return;

            List<string> scenes = fungusSceneManagerScript.scenes;

            displayScenesScroll = EditorGUILayout.BeginScrollView(displayScenesScroll);

            foreach (string scene in scenes)
            {
                if (scene != null)
                {
                    DisplayScene(scene);
                }
            }

            EditorGUILayout.EndScrollView();
        }


        private void DisplayLighting()
        {
        }


        private void DisplayScene(string sceneName)
        {
            GUILayout.BeginHorizontal();

            bool sceneIsLoaded = loadedScenes.Contains(sceneName);

            if (GUILayout.Button("REMOVE", GUILayout.ExpandWidth(false)))
            {
                // make sure the user is sure
                if (EditorUtility.DisplayDialog("Remove '" + sceneName + "'", "Are you sure you want to remove the scene '" + sceneName + "' from the current list of scenes? You can add it later via the 'Add Scene' button.", "Remove", "Cancel"))
                {
                    RemoveSceneFromBuildSettings(sceneName);
                }
            }

            if (!sceneIsLoaded)
            {
                if (GUILayout.Button("LOAD", GUILayout.ExpandWidth(false)))
                {
                    SaveOpenScene();
                    CloseOpenScenes();
                    UpdateLoadedSceneList();

                    string path = GetSceneAssetPath(sceneName + ".unity");
                    LoadManagedScene(path, OpenSceneMode.Additive, false, false);
                }
            }

            if (sceneIsLoaded)
            {
                if (GUILayout.Button("CLOSE", GUILayout.ExpandWidth(false)))
                {
                    SaveOpenScene();
                    CloseOpenScene(sceneName);
                    UpdateLoadedSceneList();
                }
            }

            GUILayout.Space(2);

            ////if (sceneIcon == null)
            //{
            sceneIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/FungusManager/FungusManager/Icons/Scene_Icon.png", typeof(Texture2D));
            //}  

            GUIStyle iconStyle = new GUIStyle(GUI.skin.label);
            iconStyle.fixedWidth = 22;
            iconStyle.fixedHeight = 22;
            GUILayout.Label(sceneIcon, iconStyle);

            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontStyle = FontStyle.Normal;
            textStyle.contentOffset = new Vector2(0, 3);
            GUILayout.Label(sceneName, textStyle);

            GUILayout.EndHorizontal();

        }


        void UpdateScenes()
        {
            SaveBuildSettingsInSceneManager();
        }

        #endregion

    }
    // class FungusSceneManagerWindow

}
// Fungus
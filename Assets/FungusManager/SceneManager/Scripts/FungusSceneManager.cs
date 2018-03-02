using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Fungus;

// TODO: Remove extra scene when starting with SceneManager and another scene already present
// TODO: Remove EventSystem in non-Hyperzoomed scenes
// TODO: RequestScene automatically runs when Hyperzoom is not present

namespace Fungus
{
    public class FungusSceneManager : MonoBehaviour
    {
        #region Events

        public static event Action<string, string> ZoomedIn;
        public static event Action<string> ZoomedOut;
        public static event Action<string> SceneLoaded;

        #endregion


        #region Properties

        /// <summary>
        /// Fade this scenes' BackgroundColor to loaded scene BackgroundColor
        /// </summary>
        [Tooltip("Fade this scenes' BackgroundColor to loaded scene BackgroundColor?")]
        public bool fadeBackgroundColor = true;

		/// <summary>
		/// Should we load/save the global barriers when we change scenes?
		/// </summary>
		[Tooltip("Should we load/save the global barriers when we change scenes?")]
        public bool persistVariablesAcrossScenes = true;

		/// <summary>
		/// If variables are persistent, reset values to default at game start
		/// </summary>
		[Tooltip("WARNING!! This deletes all playerPrefs.")]
        public bool resetVariablesAtStart = true;

        /// <summary>
        /// Should the SceneManager automatically turn off the audiolistener of managed scenes?
        /// </summary>
        [Tooltip("Should the SceneManager automatically turn off the audiolisteners of managed scenes?")]
        public bool turnOffManagedListeners = true;

        /// <summary>
        /// The Main Camera used by the Manager (used for background color changes)
        /// </summary>
        private Camera managerCamera;

        [SerializeField]
        [HideInInspector]
        public List<string> scenes = new List<string>();

        private string currentScene = "";
		private string requestedScene = "";

		private Color backgroundColor = Color.gray;
        private Coroutine backgroundColorCoroutine = null;

        List<Light> lights = new List<Light>();

        #endregion


        #region Init

        /// <summary>
        /// Whenever this object/script is enabled
        /// </summary>

        void OnEnable()
        {
            // events coming directly from Unity's SceneManager
            SceneManager.sceneLoaded += SceneManagerLoadedScene;
            SceneManager.sceneUnloaded += SceneManagerUnloadedScene;
            // events from Flowchart Block commands
            RequestManagedScene.RequestScene += RequestNextScene;
        }


        /// <summary>
        /// Whenever this object/script is disabled
        /// </summary>

        void OnDisable()
        {
            // events coming directly from Unity's SceneManager
            SceneManager.sceneLoaded -= SceneManagerLoadedScene;
            SceneManager.sceneUnloaded -= SceneManagerUnloadedScene;
            // events from Flowchart Block commands
            RequestManagedScene.RequestScene -= RequestNextScene;
        }


        private void Awake()
        {
			// if we need to reset the variables
			if (resetVariablesAtStart)
			{
				ResetVariables();
            }
            // get access to the manager camera
            foreach(GameObject go in this.gameObject.scene.GetRootGameObjects())
            {
                Camera mainCamera = go.GetComponent<Camera>();
                if (mainCamera != null)
                {
                    // get the camera in this manager
                    managerCamera = mainCamera;
                    break;
                }
            }

            // get access to the lights
            Light[] managerLights = FindObjectsOfType(typeof(Light)) as Light[];
            foreach (Light managerLight in managerLights) {
                if (managerLight.gameObject.scene == this.gameObject.scene)
                {
                    lights.Add(managerLight);
                    managerLight.gameObject.SetActive(false);
                }
            }
        }


        void Start()
        {
            backgroundColor = managerCamera.backgroundColor;

            // check how many scenes are present
            CloseOtherScenes();
        }


        private bool HyperzoomIsPresent()
        {
            Type hyperzoomType = Type.GetType("Hyperzoom");
            // verify that hyperzoom is present in the scene
            bool hyperzoomIsPresent = FindObjectOfType(hyperzoomType) != null;
            return hyperzoomIsPresent;
        }


        void CloseOtherScenes()
        {
            // if no other scenes present
            if (SceneManager.sceneCount < 2)
            {
                return;
            }
            // go through all the scenes
            for (int i = SceneManager.sceneCount-1; i >=0 ; i--)
            {
                // get this scene
                Scene scene = SceneManager.GetSceneAt(i);
                // if this is our current scene, move on to the next
                if (scene == this.gameObject.scene) continue;
                // otherwise close the scene
                MassacreEverythingInScene(scene.name);
            }

        }

        #endregion


        #region Load


        void RequestNextScene(string sceneName)
        {
            // if  there is no current scene (for example, at the beginning of the game)
            if (currentScene.Length == 0)
            {
                CloseOtherScenes();
                LoadScene(sceneName);
            }
            else if (!HyperzoomIsPresent())
            {
                SaveVariables();
                CloseOtherScenes();
                LoadScene(sceneName);
            }
            else // otherwise, request this to be the next scene
            {
                requestedScene = sceneName;
            }
        }


        void LoadRequestedScene()
        {
            if (requestedScene.Length == 0)
            {
                Debug.LogError("No scene has been requested");
                return;
            }

            LoadScene(requestedScene);
            // clean up this variable for the next use
            requestedScene = "";
        }


        void LoadScene(string sceneName)
        {
            // make sure it's valid
            if (sceneName == null)
            {
                Debug.LogError("Scene name is null");
                return;
            }

            // make sure it's not empty
            if (sceneName == "")
            {
                Debug.LogError("Empty scene name");
                return;
            }

            // check to see if this scene is in the list
            if (!scenes.Contains(sceneName))
            {
                Debug.LogError("There is no scene named '" + sceneName + "' in the list of Current Scenes.\nUse the Scene Manager to create a scene named '" + sceneName + "'.");
                return;
            }

            // check all the loaded scenes to see if they're already loaded
            bool alreadyLoaded = false;

            // go through scene by scene
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                // if this scene is loaded
                if (sceneName == SceneManager.GetSceneAt(i).name)
                {   // set the flag to true
                    alreadyLoaded = true;
                    break;
                }
            }

            // if we didn't find this scene in the currently loaded list
            if (!alreadyLoaded)
            {
                // load this scene
                //SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            }

            // remember which scene we're in
            currentScene = sceneName;
        }

        #endregion


        #region Save

        public void SetScenes(List<string> newScenes)
        {
            scenes = newScenes;
        }

        #endregion


        #region Variables

        void LoadVariables(string saveProfileKey = "")
        {
			// if the flag is not set
			if (!persistVariablesAcrossScenes) return;

			// get all the flowcharts
			Flowchart[] flowcharts = GameObject.FindObjectsOfType<Flowchart>();
            // go through each flowchart
			foreach (Flowchart flowchart in flowcharts)
			{
                // get all the variable names in this flowchart
                //string[] variableNames = flowchart.GetVariableNames();
                // get all the variables in this flowchart
                List<Variable> variables = flowchart.Variables;
                // go through each variable
                foreach(Variable variable in variables)
                {
                    //Debug.Log(flowchart.name + "\t" + variable.Key);
                    LoadVariable(saveProfileKey, flowchart, variable);
                }
			}

        }


        void LoadVariable(string saveProfileKey, Flowchart flowchart, Variable variable)
        {
            // // Prepend the current save profile (if any)
            //string prefsKey = saveProfileKey + "_" + flowchart.SubstituteVariables(variable.Key);
            string prefsKey = GetVariableKeyName(saveProfileKey, flowchart, variable);

			System.Type variableType = variable.GetType();

			if (variableType == typeof(BooleanVariable))
			{
				BooleanVariable booleanVariable = variable as BooleanVariable;
				if (booleanVariable != null)
				{
					// PlayerPrefs does not have bool accessors, so just use int
					booleanVariable.Value = (PlayerPrefs.GetInt(prefsKey) == 1);

					//Debug.Log("Loading '" + variable.Key + "' in '" + flowchart.name + "' with value '" + booleanVariable.Value + "'\t" + prefsKey);
				}
			}
			else if (variableType == typeof(IntegerVariable))
			{
				IntegerVariable integerVariable = variable as IntegerVariable;
				if (integerVariable != null)
				{
					integerVariable.Value = PlayerPrefs.GetInt(prefsKey);

					//Debug.Log("Loading '" + variable.Key + "' in '" + flowchart.name + "' with value '" + integerVariable.Value + "'\t" + prefsKey);
				}
			}
			else if (variableType == typeof(FloatVariable))
			{
				FloatVariable floatVariable = variable as FloatVariable;
				if (floatVariable != null)
				{
					floatVariable.Value = PlayerPrefs.GetFloat(prefsKey);

					//Debug.Log("Loading '" + variable.Key + "' in '" + flowchart.name + "' with value '" + floatVariable.Value + "'\t" + prefsKey);
				}
			}
			else if (variableType == typeof(StringVariable))
			{
				StringVariable stringVariable = variable as StringVariable;
				if (stringVariable != null)
				{
					stringVariable.Value = PlayerPrefs.GetString(prefsKey);

					//Debug.Log("Loading '" + variable.Key + "' in '" + flowchart.name + "' with value '" + stringVariable.Value + "'\t" + prefsKey);
				}
			}

        } // LoadVariable


        void SaveVariables(string saveProfileKey = "")
		{
			// if the flag is not set
			if (!persistVariablesAcrossScenes) return;

			// get all the flowcharts
			Flowchart[] flowcharts = GameObject.FindObjectsOfType<Flowchart>();
			// go through each flowchart
			foreach (Flowchart flowchart in flowcharts)
			{
				// get all the variables in this flowchart
				List<Variable> variables = flowchart.Variables;
				// go through each variable
				foreach (Variable variable in variables)
				{
					SaveVariable(saveProfileKey, flowchart, variable);
				}
			}
        }


        void SaveVariable(string saveProfileKey, Flowchart flowchart, Variable variable)
        {
			//  // Prepend the current save profile (if any)
			//string prefsKey = saveProfileKey + "_" + flowchart.SubstituteVariables(variable.Key);
			string prefsKey = GetVariableKeyName(saveProfileKey, flowchart, variable);

            //Debug.Log("Save " + prefsKey);
			System.Type variableType = variable.GetType();

			if (variableType == typeof(BooleanVariable))
			{
				BooleanVariable booleanVariable = variable as BooleanVariable;
				if (booleanVariable != null)
				{
					// PlayerPrefs does not have bool accessors, so just use int
					PlayerPrefs.SetInt(prefsKey, booleanVariable.Value ? 1 : 0);

					//Debug.Log("Saving: " + variable.Key + "\t" + flowchart.name + "\t" + prefsKey + "\t" + booleanVariable.Value);
				}
			}
			else if (variableType == typeof(IntegerVariable))
			{
				IntegerVariable integerVariable = variable as IntegerVariable;
				if (integerVariable != null)
				{
					PlayerPrefs.SetInt(prefsKey, integerVariable.Value);

					//Debug.Log("Saving: " + variable.Key + "\t" + flowchart.name + "\t" + prefsKey + "\t" + integerVariable.Value);
				}
			}
			else if (variableType == typeof(FloatVariable))
			{
				FloatVariable floatVariable = variable as FloatVariable;
				if (floatVariable != null)
				{
					PlayerPrefs.SetFloat(prefsKey, floatVariable.Value);

					//Debug.Log("Saving: " + variable.Key + "\t" + flowchart.name + "\t" + prefsKey + "\t" + floatVariable.Value);
				}
			}
			else if (variableType == typeof(StringVariable))
			{
				StringVariable stringVariable = variable as StringVariable;
				if (stringVariable != null)
				{
					PlayerPrefs.SetString(prefsKey, stringVariable.Value);

					//Debug.Log("Saving: " + variable.Key + "\t" + flowchart.name + "\t" + prefsKey + "\t" + stringVariable.Value);
				}
			}
        }


        void ResetVariables()
        {
            //Debug.Log("ResetVariables");
            PlayerPrefs.DeleteAll();
        }


        // TODO Create a third "global" variable scope
        string GetVariableKeyName(string saveProfileKey, Flowchart flowchart, Variable variable)
		{
            // get the save key for this variable
			string saveKey = saveProfileKey + "_" + variable.Key;
			// if this is not a global variable
			if (variable.Scope == VariableScope.Private)
			{
                // create a unique name so that it does not get confused with public variables of the same name
				saveKey = saveProfileKey + "_" + flowchart.gameObject.scene.name + "-" + flowchart.name + "-" + variable.Key;
                // replace spaces with "_"
                saveKey = saveKey.Replace(" ", "");
			}

            return saveKey;
        }

        #endregion


        #region Activations

        /// <summary>
        /// Determines if we need to activate/deactivate various aspects of loaded scenes
        /// </summary>

        protected void DisableEventSystems()
        {
            // get all the listeners
            EventSystem[] eventSystems = GameObject.FindObjectsOfType<EventSystem>();

            // go through each EventSystem
            foreach (EventSystem eventSystem in eventSystems)
            {
                // if this scene is not part of the Manager
                if (!eventSystem.gameObject.scene.Equals(this.gameObject.scene))
                {
                    //DestroyImmediate(eventSystem.gameObject);
                    eventSystem.gameObject.SetActive(false);
                }
            }

        }
		// DisableEventSystems()

		#endregion


		#region Scene Verification

		public string GetRequestedScene()
		{
			return requestedScene;
		}


		public bool HasRequestedScene()
		{
			return requestedScene.Length > 0;
		}


		public bool RequestedSceneIsValid()
		{
			// first check to see if there is a requested scene
			if (!HasRequestedScene()) return false;
			// now check to see if it's valid
			return IsValidScene(requestedScene);
		}


		public bool IsValidScene(string newSceneName)
		{
			// now check all scenes to see if this is a valid scene
			for (int i = 0; i < SceneManager.sceneCount; ++i)
			{
				// get this scene
				Scene scene = SceneManager.GetSceneAt(i);
				// if this is the manager scene, move on to next scene in the list
				if (scene == this.gameObject.scene) continue;
				// if this scene exists, return ok
				if (scene.name == newSceneName) return true;;
			}
			return false;
		}

		#endregion


		#region Scene Changes

		public void ZoomInStarted(string objectName)
        {
            // if there is a listener
            if (ZoomedIn != null)
            {
                // fire the event in the flowchart
                ZoomedIn(currentScene, objectName);
            }

            // tell all the flowcharts to save their variables
            SaveVariables();

            // clean up any remaining Flowchart activity that's remaining
            CleanUpFlowcharts();
        }


        public void ZoomOutStarted(string uslessString)
        {
            // if there is a listener
            if (ZoomedOut != null)
            {
                // fire the event in the flowchart
                ZoomedOut(currentScene);
			}

			// tell all the flowcharts to save their variables
			SaveVariables();

            // clean up any remaining Flowchart activity that's remaining
            CleanUpFlowcharts();
        }


        public void ZoomInFinished(string uslessString)
        {
            // if there is no requested scene
            if (requestedScene.Length == 0)
            {
                Debug.LogError("There is no Zoom In scene request. Current scene = '" + currentScene + "'");
                return;
            }

            ZoomFinished();
        }


        public void ZoomOutFinished(string uslessString)
        {
            // if there is no requested scene
            if (requestedScene.Length == 0)
            {
                Debug.LogError("There is no Zoom Out scene request. Current scene = '" + currentScene + "'");
                return;
            }

            ZoomFinished();
        }


        void ZoomFinished()
        {
            // check all the loaded scenes to see if they're already loaded
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                // if this is the manager scene, move on to next scene
                if (scene == this.gameObject.scene) continue;
                // if the current scene is already the one that we're requesting, abort
                // TODO: this should be possible, but currently it doesn't work. This line should be removed
                if (scene.name == requestedScene) continue;
                // ok, this is not a valid scene, unload it
                MassacreEverythingInScene(scene.name);
            }

            // ok, now we can load the requested scene
            LoadRequestedScene();
        }

        #endregion


        #region Callbacks

        /// <summary>
        /// Handles the scene lodaded event.
        /// </summary>
        /// <param name="scene">Scene.</param>
        /// <param name="mode">Mode.</param>

        void SceneManagerLoadedScene(Scene scene, LoadSceneMode mode)
        {
            // if we need to load variables
            if (persistVariablesAcrossScenes)
            {
				LoadVariables();
            }

            // if this is not the SceneManager
            if (scene != this.gameObject.scene)
            {
                // set this scene as the active scene
                SceneManager.SetActiveScene(scene);
                // turn off the audio listener
                if (turnOffManagedListeners) TurnOffListener();
            }

            // if there is a listener
            if (SceneLoaded != null)
            {
                // fire the event in the flowchart
                SceneLoaded(scene.name);
            }
		}


        /// <summary>
        /// Handles the scene unloaded event.
        /// </summary>
        /// <param name="scene">Scene.</param>

        void SceneManagerUnloadedScene(Scene scene)
        {
            //Debug.Log("SceneManagerUnloadedScene " + scene.name);
            //LoadRequestedScene();
        }


        #endregion


        #region Listener

        void TurnOffListener()
        {
            // get access to the lights
            AudioListener[] listeners = FindObjectsOfType(typeof(AudioListener)) as AudioListener[];
            foreach (AudioListener listener in listeners)
            {
                if (listener.gameObject.scene != this.gameObject.scene)
                {
                    DestroyImmediate(listener);
                }
            }
        }

        #endregion


        #region CleanUp

        void MassacreEverythingInScene(string sceneName)
        {
            // first, go through all the scenes
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                // get a scene pointer
                Scene scene = SceneManager.GetSceneAt(i);
                // make sure it's not another scene
                if (scene.name != sceneName) continue;
                // get all the root objects in this scene
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject rootObject in rootObjects)
                {
                    // kill this @#%€$* with extreme prejudice
                    DestroyImmediate(rootObject);
                    //Destroy(rootObject);
                }
            }
            // kill the scene itself
            SceneManager.UnloadSceneAsync(sceneName);
        }


        /// <summary>
        /// We need to stop all the currently executing blocks in certain Flowcharts
        /// </summary>

        void CleanUpFlowcharts()
        {
            // go through all the dialogues
            SayDialog[] sayDialogs = GameObject.FindObjectsOfType<SayDialog>();
            foreach (SayDialog sayDialog in sayDialogs)
            {
                //Debug.Log("SayDialog.Stop() " + sayDialog.name);
                //sayDialog.Stop();
            }
        }

        #endregion


        #region BackgroundColor

        public void BackgroundColorChanged(Color newColor)
        {
            if (fadeBackgroundColor)
            {
                // if there is a co-routine playing, stop it
                if (backgroundColorCoroutine != null) StopCoroutine(backgroundColorCoroutine);
                // start the ChangeBackgroundColor routine and remember it for future destruction if necessary
                backgroundColorCoroutine = StartCoroutine("BackgroundColorRoutine", newColor);
            }
        }


        IEnumerator BackgroundColorRoutine(Color newColor)
        {
            for (float t = 0.0f; t < 1.0f; t += 0.05f)
            {
                backgroundColor = Color.Lerp(backgroundColor, newColor, t);
                if (managerCamera != null)
                {
                    managerCamera.backgroundColor = backgroundColor; 
                }
                else
                {
                    Debug.LogWarning("managerCamera == null");
                }
                yield return new WaitForEndOfFrame();
            }

            yield return null;
        }

        #endregion

    }
    // class Manager

}
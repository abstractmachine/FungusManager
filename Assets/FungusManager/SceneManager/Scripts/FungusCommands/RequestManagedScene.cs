using System;
using UnityEngine;
using UnityEngine.Serialization;

using Fungus;

namespace Fungus
{

	/// <summary>
	/// Sends event to Manager to LoadScene the next scene with the provided name.
	/// </summary>
	[CommandInfo("SceneManager",
                 "Request Scene",
                 "Sends an event to Manager requesting a scene by name")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]

    public class RequestManagedScene : Command
    {
        #region Action

        public static event Action<string> RequestScene;

        #endregion


        #region Serialized Properties

        [Tooltip("Name of the scene to load. The scene must be added both to the build settings and to the Manager script's public 'Scenes' list.")]
        [SerializeField]
        public string sceneName = "";

        #endregion


        #region Command

        /// <summary>
        /// When this command is fired in a Flowchart Block
        /// </summary>

        public override void OnEnter()
        {
            // make sure there are listeners
            if (RequestScene != null)
            {
                // fire off this LoadScene(string) event
                RequestScene(sceneName);
			}

			// let the block continue on it's way
			Continue();
        }


        #endregion


        #region Interface

        public override string GetSummary()
        {
            if (sceneName.Length == 0)
            {
                return "Error: No scene name selected";
            }

            return "Load the '" + sceneName + "' scene";
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        #endregion
    }
}
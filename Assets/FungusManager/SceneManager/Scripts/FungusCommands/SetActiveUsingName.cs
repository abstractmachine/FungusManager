// This code is part of the Fungus library (http://fungusgames.com) maintained by Chris Gregan (http://twitter.com/gofungus).
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using UnityEngine.Serialization;

using Fungus;

namespace Fungus
{
    /// <summary>
    /// Sets a game object in the scene to be active / inactive.
    /// </summary>
    [CommandInfo("SceneManager", 
                 "Set Active By Name", 
                 "Sets the name of a game object to be activated / deactivated.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class SetActiveUsingName : Command
    {
        [Tooltip("Name of the game object to enable / disable")]
        [SerializeField] protected string targetName = "";

        [Tooltip("Set to true to enable the game object")]
        [SerializeField] protected BooleanData activeState;
    
        #region Command

        public override void OnEnter()
        {
            if (targetName.Length > 0)
            {
                GameObject targetObject = GameObject.Find(targetName);

                if (targetObject != null) 
                {
					targetObject.SetActive(activeState.Value);
                }
            }

            Continue();
		}

		#endregion

		#region Interface

		public override string GetSummary()
        {
            if (targetName.Length == 0)
            {
                return "Error: No game object name";
            }

            return targetName + " = " + activeState.GetDescription();
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        #endregion
    }
}
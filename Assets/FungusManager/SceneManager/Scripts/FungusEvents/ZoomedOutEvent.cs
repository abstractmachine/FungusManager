using UnityEngine;
using System.Collections;

namespace Fungus
{
    /// <summary>
    /// This handler fires when when a focusable object has been selected in the scene
    /// </summary>

    [EventHandlerInfo("SceneManager", "Zoomed Out", "When the player zooms out of a scene, fire this block.")]

    public class ZoomedOutEvent : EventHandler
    {
        [Tooltip("The name of the scene")]
        [SerializeField]
        protected string sceneName;

        void OnEnable()
        {
            FungusSceneManager.ZoomedOut += ZoomedOut;
        }


        private void OnDisable()
        {
            FungusSceneManager.ZoomedOut -= ZoomedOut;
        }


        /// <summary>
        /// Fire the ExecuteBlock method
        /// </summary>
        /// 
        public void ZoomedOut(string newSceneName)
        {
            
            if (newSceneName == sceneName)
			{
				// ok, start the block now that these variables have been set
				ExecuteBlock();
			} // if (objectName ==

        } // public void OnObjectSelected


        /// <summary>
        /// The summary of this Event
        /// </summary>

        //public override string GetSummary()
        //{
        //	return "Start this block when a focusable object has been selected in the scene.";
        //}

    }
}
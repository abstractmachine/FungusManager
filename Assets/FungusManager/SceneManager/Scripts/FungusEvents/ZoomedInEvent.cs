using UnityEngine;
using System.Collections;

namespace Fungus
{
    /// <summary>
    /// This handler fires when when a focusable object has been selected in the scene
    /// </summary>

    [EventHandlerInfo("SceneManager", "Zoomed In", "When the player zooms into an object, fire this block.")]

    public class ZoomedInEvent : EventHandler
    {
        [Tooltip("The name of the scene")]
        [SerializeField]
        protected string sceneName;

        [Tooltip("The name of the object the player zoomed into")]
        [SerializeField]
        protected string objectName;

        void OnEnable()
        {
            FungusSceneManager.ZoomedIn += ZoomedIn;
        }


        private void OnDisable()
        {
            FungusSceneManager.ZoomedIn -= ZoomedIn;
        }


        /// <summary>
        /// Fire the ExecuteBlock method
        /// </summary>
        /// 
        public void ZoomedIn(string newSceneName, string newObjectName)
        {

            if (objectName == newObjectName && newSceneName == sceneName)
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
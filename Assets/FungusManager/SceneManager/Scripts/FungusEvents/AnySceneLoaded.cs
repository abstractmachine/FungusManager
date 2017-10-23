using UnityEngine;
using System.Collections;

namespace Fungus
{
    /// <summary>
    /// This handler fires when ...
    /// </summary>

    [EventHandlerInfo("SceneManager", "Any Scene Loaded", "Start this block whenever any scene has loaded")]

    public class SceneLoaded : EventHandler
    {

        /// <summary>
        /// Fire the ExecuteBlock method
        /// </summary>
        /// 
        public void OnSceneLoaded(string newSceneName)
        {
            // ok, start the block now that these variables have been set
            ExecuteBlock();
        }

        void OnEnable()
        {
            FungusSceneManager.SceneLoaded += OnSceneLoaded;
        }


        private void OnDisable()
        {
            FungusSceneManager.SceneLoaded -= OnSceneLoaded;
        }


        /// <summary>
        /// The summary of this Event
        /// </summary>

        public override string GetSummary()
        {
            return "Start at SceneLoad()";
        }

    }
}
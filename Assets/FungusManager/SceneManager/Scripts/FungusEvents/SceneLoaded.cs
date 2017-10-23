using UnityEngine;
using System.Collections;

namespace Fungus
{
	/// <summary>
	/// This handler fires when ...
	/// </summary>

	[EventHandlerInfo("SceneManager", "Scene Loaded", "Start this block when a scene has loaded")]

	public class AnySceneLoaded : EventHandler
	{
		[Tooltip("The name of the scene")]
		[SerializeField]
		protected string sceneName;


		/// <summary>
		/// Fire the ExecuteBlock method
		/// </summary>
		/// 
		public void OnSceneLoaded(string newSceneName)
		{
			if (sceneName == newSceneName)
			{
				// ok, start the block now that these variables have been set
				ExecuteBlock();
			}
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
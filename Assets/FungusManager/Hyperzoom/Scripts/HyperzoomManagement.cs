using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using Fungus;

namespace Fungus
{
    public class HyperzoomManagement : MonoBehaviour
    {
        #region Actions

        public static event Action<Color> BackgroundColorChanged;

        #endregion


        #region Public Properties

        /// <summary>
        /// The Input Prefab (with EventSystem)
        /// </summary>
        [Tooltip("The Input Prefab (with EventSystem)")]
        public GameObject inputPrefab;

        // /// <summary>
        // /// The Phylactery Prefab (Menu & Say Dialogs)
        // /// </summary>
        //[Tooltip("The Phylactery Prefab (Menu & Say Dialogs)")]
        //public GameObject phylacteryPrefab;

        #endregion


        #region Properties

        protected bool managerIsPresent = false;

        protected Camera currentCamera;

        /// <summary>
        /// The list of all targets that can be renderered "Focusable"
        /// </summary>
        protected List<GameObject> focusableTargets = new List<GameObject>();

        /// <summary>
        /// The list of focusable renderers, associated with its root parent GameObject
        /// </summary>
        protected Dictionary<GameObject, GameObject> focusedFaders = new Dictionary<GameObject, GameObject>();

        /// <summary>
        /// The list of focusable renderers, associated with its root parent GameObject
        /// </summary>
        protected Dictionary<GameObject, GameObject> unfocusedFaders = new Dictionary<GameObject, GameObject>();

        #endregion


        #region Init

        virtual protected void Awake()
        {
            EnableEventSystem();
            //EnablePhylactery();
        }


        virtual protected void Start()
        {
            // memorize Renderers (for fading)
            MemorizeFaders();

            // check to see if there is a Manager
            if (GameObject.FindObjectOfType<FungusSceneManager>() != null)
            {
                managerIsPresent = true;
            }

            // setup the camera for zoom in/out
            currentCamera = this.GetComponent<Camera>();
            // if this isn't a camera
            if (currentCamera == null)
            {   // check in children
                currentCamera = this.GetComponentInChildren<Camera>();
                // last ditch fallback
                if (currentCamera == null)
                {
                    Debug.LogWarning("defaulting to main camera");
                    currentCamera = Camera.main;
                }
            }

            SendBackgroundColor(currentCamera.backgroundColor);

            // if the manager is present
            if (managerIsPresent)
            {
                // clean up everything in that case
                DisableAudioListeners();
                //DisableEventSystems();
                DisableCameraTransparency();
            }
            else // there is no Manager
            {
                // what to activate/deactivate when there is no manager
            }
        }

        #endregion


        #region Activations

        protected void EnableEventSystem()
        {
            // get all the listeners
            EventSystem[] eventSystems = GameObject.FindObjectsOfType<EventSystem>();
            //EventSystem[] eventSystems = GetComponents<EventSystem>();

            // if there are no EventSystems
            if (eventSystems.Length == 0)
            {
                // make sure we have a Prefab ready
                if (inputPrefab == null)
                {
                    Debug.LogError("Missing Input Prefab (to Load EventSystem)");
                    return;
                }

                // ok, there are no Inputs with EventSystems (this is probably a Scene-specific editing session)

                // Instantiate the Input system, with a name
                GameObject go = Instantiate(inputPrefab) as GameObject;
                go.name = "Input";
                // make it a child of this GameObject
                go.transform.parent = this.transform;
            }

        } // EnableEventSystems()


        //      void EnablePhylactery()
        //{
        // // make sure we have a Prefab ready
        //         if (phylacteryPrefab == null)
        //{
        //	Debug.LogError("Missing Phylactery Prefab (Menu & Say Dialogs");
        //	return;
        //}

        // // Instantiate the Input system, with a name
        //         GameObject go = Instantiate(phylacteryPrefab) as GameObject;
        //go.name = "Phylactery";
        // // make it a child of this GameObject
        //go.transform.parent = this.transform;
        //}

        #endregion


        #region Deactivations

        /// <summary>
        /// Determines if we need to activate/deactivate various aspects of loaded scenes
        /// </summary>

        protected void DisableAudioListeners()
        {
            // get all the listeners
            AudioListener[] listeners = GameObject.FindObjectsOfType<AudioListener>();

            // get the FungusSceneManager script
            FungusSceneManager fungusSceneManager = (FungusSceneManager)FindObjectOfType(typeof(FungusSceneManager));

            if (fungusSceneManager != null)
            {
                // go through each listener
                foreach (AudioListener listener in listeners)
                {
                    // if this scene is not part of the Manager
                    if (!listener.gameObject.scene.Equals(fungusSceneManager.gameObject.scene))
                    {
                        listener.enabled = false;
                    }

                } // foreach(AudioListener

            } // if (FungusSceneManager

        } // DisableAudioListeners()


        /// <summary>
        /// Determines if we need to activate/deactivate various aspects of loaded scenes
        /// </summary>

        protected void DisableEventSystems()
        {
            // get all the listeners
            EventSystem[] eventSystems = GameObject.FindObjectsOfType<EventSystem>();

            // get the FungusSceneManager script
            FungusSceneManager fungusSceneManager = (FungusSceneManager)FindObjectOfType(typeof(FungusSceneManager));
            Debug.Log("FungusSceneManager = " + fungusSceneManager);

            if (fungusSceneManager != null)
            {
                // go through each EventSystem
                foreach (EventSystem eventSystem in eventSystems)
                {
                    // if this scene is not part of the Manager
                    if (!eventSystem.gameObject.scene.Equals(fungusSceneManager.gameObject.scene))
                    {
                        //eventSystem.gameObject.SetActive(false);
                        DestroyImmediate(eventSystem.gameObject);
                    }
                } // foreach(EventSystem

            } // if (fungusSceneManager

        } // DisableEventSystems()


        public void DisableCameraTransparency()
        {
            // disable camera clear flags
            CameraClearFlags clearFlags = CameraClearFlags.Nothing;

            // get all the listeners
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();

            // get the FungusSceneManager script
            FungusSceneManager fungusSceneManager = (FungusSceneManager)FindObjectOfType(typeof(FungusSceneManager));

            if (fungusSceneManager != null)
            {
                // go through each EventSystem
                foreach (Camera camera in cameras)
                {
                    // if this scene is not part of the Manager
                    if (!camera.gameObject.scene.Equals(fungusSceneManager.gameObject.scene))
                    {
                        //camera.gameObject.SetActive(false);
                        camera.clearFlags = clearFlags;
                    }
                } // foreach(Camera

            } // if (fungusSceneManager

        } // DisableCameraTransparency

        #endregion


        #region Backgroud

        protected void SendBackgroundColor(Color color)
        {
            if (BackgroundColorChanged != null)
            {
                BackgroundColorChanged(color);
            }

        } // SendBackgroundColor

        #endregion


        #region Memorize

        protected void MemorizeTargets()
        {
            focusableTargets.Clear();

            Zoomable[] zoomableGameObjects = FindObjectsOfType<Zoomable>();
            foreach (Zoomable zoomableGameObject in zoomableGameObjects)
            {
                focusableTargets.Add(zoomableGameObject.gameObject);
            }
        } // MemorizeTargets()


        protected void MemorizeFaders()
        {
            // used for testing key-value pairs
            GameObject parentObjectTester;
            // first go through all the focusable objects
            Zoomable[] zoomableGameObjects = FindObjectsOfType<Zoomable>();
            foreach (Zoomable zoomableGameObject in zoomableGameObjects)
            {
                // get all the children of this focusable object
                Renderer[] childRenderersOfFocusableGameObject = zoomableGameObject.gameObject.GetComponentsInChildren<Renderer>();
                // go through all it's children
                foreach (Renderer childRenderer in childRenderersOfFocusableGameObject)
                {
                    // memorize all the children under this renderer that has a renderer
                    MemorizeChildFaders(zoomableGameObject.gameObject);
                }

                // TODO: Add Skinned Mesh Renderers to list
                SkinnedMeshRenderer[] childSkinMeshRenderersOfFocusableGameObject = zoomableGameObject.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                // go through all it's children
                foreach (SkinnedMeshRenderer childRenderer in childSkinMeshRenderersOfFocusableGameObject)
                {
                    // memorize all the children under this renderer that has a renderer
                    MemorizeChildFaders(zoomableGameObject.gameObject);
                }
            }

            // go through all the renderers in this scene
            Renderer[] possibleRenderers = FindObjectsOfType<Renderer>();
            foreach (Renderer possibleRenderer in possibleRenderers)
            {
                // if this is not a focusable object (and therefore not already in the list)
                if (possibleRenderer.gameObject.GetComponent<Zoomable>() == null)
                {
                    GameObject possibleRendererGameObject = possibleRenderer.gameObject;
                    // make sure it isn't already added
                    if (focusedFaders.TryGetValue(possibleRendererGameObject, out parentObjectTester)) continue;
                    if (unfocusedFaders.TryGetValue(possibleRendererGameObject, out parentObjectTester)) continue;
                    // ok, add it to the list of unfocuseable objects
                    unfocusedFaders.Add(possibleRendererGameObject, possibleRenderer.gameObject);

                } // if (possibleRenderer.gameObject

            } // foreach(Renderer

            // go through all the renderers in this scene
            SkinnedMeshRenderer[] possibleSkinnedMeshRenderers = FindObjectsOfType<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer possibleRenderer in possibleSkinnedMeshRenderers)
            {
                // if this is not a focusable object (and therefore not already in the list)
                if (possibleRenderer.gameObject.GetComponent<Zoomable>() == null)
                {
                    GameObject possibleRendererGameObject = possibleRenderer.gameObject;
                    // make sure it isn't already added
                    if (focusedFaders.TryGetValue(possibleRendererGameObject, out parentObjectTester)) continue;
                    if (unfocusedFaders.TryGetValue(possibleRendererGameObject, out parentObjectTester)) continue;
                    // ok, add it to the list of unfocuseable objects
                    unfocusedFaders.Add(possibleRendererGameObject, possibleRenderer.gameObject);

                } // if (possibleRenderer.gameObject

            } // foreach(Renderer

        } // MemorizeFaders()


        void MemorizeChildFaders(GameObject rootParentObject)
        {
            // we want to add all the children of this rootObject that contain Renderers
            Renderer[] childRenderers = rootParentObject.GetComponentsInChildren<Renderer>();
            // go through all it's children
            foreach (Renderer childRenderer in childRenderers)
            {
                // check to see if this child is already in the list
                MemorizationCheckChild(rootParentObject, childRenderer.gameObject);

            } // foreach (Renderer

        } // MemorizeChildFaders()


        void MemorizationCheckChild(GameObject rootParentObject, GameObject childGameObject)
        {
            // used for testing key-value pairs
            GameObject rootParentObjectTester;
            // if it's in the unfocused list
            if (unfocusedFaders.TryGetValue(childGameObject, out rootParentObjectTester))
            {
                // remove it from this list
                unfocusedFaders.Remove(childGameObject);
            }

            // make sure it isn't already added to the focused list
            if (!focusedFaders.TryGetValue(childGameObject, out rootParentObjectTester))
            {
                // add it to the dictionary, along with its root parent GameObject
                focusedFaders.Add(childGameObject, rootParentObject);
            }
        }

        #endregion

    } // FocusManagement

} // namespace

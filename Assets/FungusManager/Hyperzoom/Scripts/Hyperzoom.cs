using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using Fungus;

namespace Fungus
{

    public class Hyperzoom : HyperzoomManagement
    {
        #region Events

        /// <summary>
        /// Fires when scene change has started
        /// </summary>
        public static event Action<string> ZoomInStarted;

        /// <summary>
        /// Fires when scene change has finished
        /// </summary>
        public static event Action<string> ZoomInFinished;

        /// <summary>
        /// Fires when scene change has started
        /// </summary>
        public static event Action<string> ZoomOutStarted;

        /// <summary>
        /// Fires when scene change has finished
        /// </summary>
        public static event Action<string> ZoomOutFinished;

        #endregion


        #region Public Configurable Parameters

        /// <summary>
        /// The zoomed-in value for an orthographic camera
        /// </summary>
        [Tooltip("Define the focus value of a zoomed-in camera (orthographic)")]
        public float zoomMinimum = 5.0f;

        /// <summary>
        /// The starting zoom value for an orthographic camera
        /// </summary>
        [Tooltip("Define the starting focus the camera (orthographic)")]
        public float zoomStartingValue = 12.5f;

        /// <summary>
        /// The zoomed-out value for an orthographic camera
        /// </summary>
        [Tooltip("Define the focus value of a zoomed-out camera (orthographic)")]
        public float zoomMaximum = 20.0f;

        /// <summary>
        /// define the limits along the vertical axis
        /// </summary>
        [Tooltip("Should the rotation clamp to minimum/maximum angles when rotating vertically along X axis?")]
        public bool clampVerticalAngle = true;

        /// <summary>
        /// define the minimum limit along the vertical axis
        /// </summary>
        [Tooltip("Define the minimum limit along the vertical axis")]
        public float verticalMinimum = 2.0f;

        /// <summary>
        /// define the maximum limit along the vertical axis
        /// </summary>
        [Tooltip("Define the maximum limit along the vertical axis")]
        public float verticalMaximum = 80.0f;

        /// <summary>
        /// define the duration of x-ray mode
        /// </summary>
        [Tooltip("Define the duration (in seconds) of x-ray mode")]
        public float xrayDuration = 2.0f;

        /// <summary>
        /// draw the zoom curve for the targeted GameObject (and its children)
        /// </summary>
        [Tooltip("draw the zoom curve for the targeted GameObject (and its children)")]
        public AnimationCurve targetCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(0.05f, 0.968f), new Keyframe(0.25f, 1.0f), new Keyframe(0.75f, 1.0f), new Keyframe(0.95f, 0.0f));

        /// <summary>
        /// draw the zoom curve for focusable GameObjects (and their children)
        /// </summary>
        [Tooltip("draw the zoom curve for focusable GameObjects (and their children)")]
        public AnimationCurve focusableCurve = new AnimationCurve(new Keyframe(0.0f, 0.1f), new Keyframe(0.25f, 1.0f), new Keyframe(0.75f, 1.0f), new Keyframe(0.95f, 1.0f));

        /// <summary>
        /// draw the zoom curve for unfocusable GameObjects
        /// </summary>
        [Tooltip("draw the zoom curve for unfocusable GameObjects")]
        public AnimationCurve unfocusableCurve = new AnimationCurve(new Keyframe(0.11f, 0.0f), new Keyframe(0.25f, 1.0f), new Keyframe(0.75f, 1.0f), new Keyframe(0.9f, 0.0f));

        #endregion


        #region Zoom Properties

        private float zoomStartingPct = 0.0f;
        private float zoomTarget = 0.5f;
        private float zoomFadeMargin = 0.25f;
        private float zoomPointOfNoReturn = 0.5f;
        private float zoomSpread = 15.0f; // 20.0f - 5.0f
        private bool zoomIsSnapping = false;
        float zoomPointOfNoReturnLow = 0.0f;
        float zoomPointOfNoReturnHigh = 1.0f;

        private Quaternion startingRotation = Quaternion.identity;

        #endregion


        #region X-Ray Properties

        private bool xrayState = false;
        private float xrayOpacityValue = 1.0f;
        private float xraySpeed = 0.05f;
        //private float xrayCountdown = 0.0f;

        #endregion


        #region Focusing properties

        /// <summary>
        /// the current selected gameObject we are focused on
        /// </summary>
        [Tooltip("Define the starting selected gameObject we are focused on (optional - leave null for unselected-at-start)")]
        public GameObject target;

        #endregion


        #region General Properties

        /// <summary>
        /// rotation multiplier, determining how fast we rotate. This adapts to screen resolution (for touch).
        /// </summary>
        private float rotationSpeed = 500.0f;

        /// <summary>
        /// The different speed multipliers, depending on the input method
        /// </summary>
        private float zoomSpeedPerspective = 0.5f;
        private float zoomSpeedOrthographic = 0.1f;

        #endregion


        #region Listeners

        /// <summary>
        /// Whenever this Object/script is enabled
        /// </summary>

        void OnEnable()
        {
            // start listening for various types of interaction

            // mouse+multitouch interactions
            Interaction.DidRotate += Rotate;
            Interaction.DidZoom += Zoom;
            Interaction.DidFinishZoom += ZoomCleanup;
            // change of focus
            Interaction.DidChangeFocus += ChangeFocus;
            Interaction.DidScrollOverObject += ChangeFocus;
            // x-ray functions
            Interaction.TouchChanged += TouchChanged;
            Interaction.TouchDragged += TouchDragged;
            // controller analog stick
            Joystick.DidRotate += Rotate;
            Joystick.DidZoom += Zoom;
            Joystick.DidZoomIn += ZoomIn;
            Joystick.DidZoomOut += ZoomOut;
            Joystick.DidFinishZoom += ZoomCleanup;
            Joystick.DidSelectNextFocus += SelectNextFocus;
            Joystick.DidSelectPreviousFocus += SelectPreviousFocus;
            // clean up everything in that case
        }


        /// <summary>
        /// Whenever this Object/script is disabled
        /// </summary>

        void OnDisable()
        {
            // stop listening for various types of interaction

            // mouse+multitouch interactions
            Interaction.DidRotate -= Rotate;
            Interaction.DidZoom -= Zoom;
            Interaction.DidFinishZoom -= ZoomCleanup;
            // change of focus
            Interaction.DidChangeFocus -= ChangeFocus;
            Interaction.DidScrollOverObject -= ChangeFocus;
            // x-ray functions
            Interaction.TouchChanged -= TouchChanged;
            Interaction.TouchDragged -= TouchDragged;
            // controller analog stick
            Joystick.DidRotate -= Rotate;
            Joystick.DidZoom -= Zoom;
            Joystick.DidZoomIn -= ZoomIn;
            Joystick.DidZoomOut -= ZoomOut;
            Joystick.DidFinishZoom -= ZoomCleanup;
            Joystick.DidSelectNextFocus -= SelectNextFocus;
            Joystick.DidSelectPreviousFocus -= SelectPreviousFocus;
        }

        #endregion


        #region Init

        override protected void Start()
        {
            // prepare all the FocusManagement stuff before doing all our stuff here
            base.Start();

            FocusInit();

            // normalize interactions based on screen size/resolution
            // i.e. make it feel the same in Unity Editor, on Desktop, Tablet, Retina, Lo-Res, etc.
            rotationSpeed = (1.0f / Screen.width) * rotationSpeed;

			// if the camera is orthographic
			if (currentCamera.orthographic)
			{
				// force the camera to the starting value
				currentCamera.orthographicSize = zoomStartingValue;
			}
			else
			{
				// force the camera to the starting value
				currentCamera.fieldOfView = zoomStartingValue;
			}

            // figure out current zoom value
            float cameraZoom = currentCamera.orthographic ? currentCamera.orthographicSize : currentCamera.fieldOfView;
            zoomTarget = (cameraZoom - zoomMinimum) / (zoomMaximum - zoomMinimum);

            // remember this starting value
            zoomStartingPct = zoomTarget;

            // figure out the conversions to/from normalized 0.0f > 1.0f values
            zoomSpread = zoomMaximum - zoomMinimum;

            // figure out the points of no-return
            zoomPointOfNoReturnLow = zoomFadeMargin * zoomPointOfNoReturn;
            zoomPointOfNoReturnHigh = 1.0f - (zoomFadeMargin * zoomPointOfNoReturn);

            // memorize rotation
            startingRotation = this.transform.rotation;

            // fade in scene
            HideEverything();
            StartCoroutine("FadeInEverything");
        }


        float ConvertPctToZoom(float value)
        {
            // scale up to zoom scale
            value *= zoomSpread;
            // apply offset to fit into zoom min/max range
            value += zoomMinimum;
            // return result
            return value;
        }

        #endregion


        #region Loop

        void LateUpdate()
        {
            // start with a world-centered target
            Vector3 targetPosition = Vector3.zero;

            // if there is a target
            if (target != null)
            {   // snap to this target
                targetPosition = target.transform.position;
            }
            // follow this target
            this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, 0.5f);
        }

        #endregion


        #region Zoom


        void Zoom(float delta)
        {
            // if we're snapping, it's the end of the scene. Stop all incoming processes
            if (zoomIsSnapping) return;

            // turn off any snapping to original perspective
            CancelResetPerspective();

            // slow down delta
            delta = ZoomSlowDownDelta(delta);
            // calculate new value starting from current zoomTarget
            float newZoomValue = zoomTarget;
            // first apply this value to our target
            newZoomValue += delta * (currentCamera.orthographic ? zoomSpeedOrthographic : zoomSpeedPerspective);
            // clamp the field of view to make sure it doesn't go beyond 0.0f and 1.0f
            newZoomValue = Mathf.Clamp(newZoomValue, 0.05f, 0.95f);

            // set new value
            zoomTarget = newZoomValue;
            // zoom to this new value
            ZoomToPct(newZoomValue);

            // reset the xray countdown (if it's waiting)
            TurnOffXray();
            //ResetXrayCountdown();
        }


        void ZoomIn(bool useless = false)
        {
            // if we're snapping, it's the end of the scene. Stop all incoming processes
            if (zoomIsSnapping) return;
            // turn off any snapping to original perspective
            CancelResetPerspective();

            float newZoomTarget = zoomTarget;

            if (target != null && newZoomTarget < 0.4f) newZoomTarget = zoomPointOfNoReturnLow;
            else if (newZoomTarget >= 0.666f) newZoomTarget = 0.5f;
            else if (newZoomTarget > zoomFadeMargin) newZoomTarget = zoomFadeMargin;

            ZoomToward(newZoomTarget);
        }


        void ZoomOut(bool useless = false)
        {
            // if we're snapping, it's the end of the scene. Stop all incoming processes
            if (zoomIsSnapping) return;
            // turn off any snapping to original perspective
            CancelResetPerspective();

            float newZoomTarget = zoomTarget;

            if (newZoomTarget > 0.6f) newZoomTarget = zoomPointOfNoReturnHigh;
            else if (newZoomTarget < 0.333f) newZoomTarget = 0.5f;
            else if (newZoomTarget < (1.0f - zoomFadeMargin)) newZoomTarget = 1.0f - zoomFadeMargin;

            ZoomToward(newZoomTarget);
        }


        float ZoomSlowDownDelta(float delta)
        {
            // if we're at the lower level and delta is shrinking
            if (zoomTarget < zoomFadeMargin && delta < 0)
            {
                // normalize zoom value within the zoom margin
                float factor = ((zoomFadeMargin - zoomTarget) / zoomFadeMargin);
                // invert normalization
                factor = 1.0f - factor;
                // logarithmicize factor
                factor *= factor;
                // apply factor to delta
                return delta * factor;
            }

            if (zoomTarget > 1.0f - zoomFadeMargin && delta > 0)
            {
                // normalize zoom value within the zoom margin
                float factor = (1.0f - zoomTarget) / zoomFadeMargin;
                // logarithmicize factor
                factor *= factor;
                // apply factor to delta
                return delta * factor;
            }

            // return results
            return delta;
        }


        void ZoomToPct(float zoomPctValue)
        {
            // now scale up to zoom values
            float zoomCameraValue = ConvertPctToZoom(zoomPctValue);

            // if the camera is in orthographic mode
            if (currentCamera.orthographic)
            {
                // apply to camera
                currentCamera.orthographicSize = zoomCameraValue;
            }
            else // otherwise it's in perspective mode
            {
                // apply to camera
                currentCamera.fieldOfView = zoomCameraValue;
            }
            // apply these changes
            ApplyRendererOpacities();
        }


        void ZoomToward(float newZoomValue)
        {
            StopCoroutine("ZoomSnapTo");
            StopCoroutine("ZoomTowardRoutine");
            // start moving to new position
            StartCoroutine("ZoomTowardRoutine", newZoomValue);
        }


        IEnumerator ZoomTowardRoutine(float newZoomValue)
        {
            while (Mathf.Abs(newZoomValue - zoomTarget) > 0.01f)
            {
                // 
                zoomTarget = Mathf.Lerp(zoomTarget, newZoomValue, 0.25f);
                // snap to that new value
                ZoomToPct(zoomTarget);
                // 
                yield return new WaitForEndOfFrame();
            }
            // take on new value
            zoomTarget = newZoomValue;
            // snap to that new value
            ZoomToPct(zoomTarget);
            // clean up after new zoom position
            ZoomCleanup();
            // all done
            yield return null;
        }

        /// <summary>
        /// this method is called when we've finished or paused zooming
        /// </summary>

        void ZoomCleanup(bool useless = false)
        {
            // if we're snapping, it's the end of the scene. Stop all incoming processes
            if (zoomIsSnapping) return;

            // abort previous coroutines
            // if there was a snap forward routine
            StopCoroutine("ZoomSnapTo");
            StopCoroutine("ZoomTowardRoutine");

            // this is the target value we are going to snap to
            float snapTarget = zoomTarget;

            // if we're zoomed in beyond low margin and something is selected
            if (zoomTarget < zoomFadeMargin)
            {
                // if there is no target
                if (target == null)
                {
                    // back up to edge of fade
                    snapTarget = zoomFadeMargin;
                }
                // if we're beyond the point of no return and the manager is present
                else if (zoomTarget <= zoomPointOfNoReturnLow && managerIsPresent)
                {
                    snapTarget = 0.0f;
                }
                else // otherwise, we're inside the point of no return
                {
                    snapTarget = zoomFadeMargin;
                }
                // snap to this new value
                StartCoroutine("ZoomSnapTo", snapTarget);

            } // if (zoomTarget < zoomFadeMargin)


            // if were zoomed out beyond high margin
            if (zoomTarget > 1.0f - zoomFadeMargin)
            {

                // if we're beyond the point of no return and the manager is present
                if (zoomTarget >= zoomPointOfNoReturnHigh & managerIsPresent)
                {
                    // force zoom out
                    snapTarget = 1.0f;
                }
                else // otherwise, we're inside the point of no return
                {
                    // abort force zoom out
                    snapTarget = 1.0f - zoomFadeMargin;
                }

                // snap to this new value
                StartCoroutine("ZoomSnapTo", snapTarget);

            } // if zoomTarget > 1.0f - zoomFadeMargin

        } // ZoomCleanup()


        IEnumerator ZoomSnapTo(float newTargetValue)
        {
            // if this is a type of snap that will end the scene
            if (newTargetValue == 0.0f)
            {
                // set a flag to tell all incoming events to stop while we snap
                zoomIsSnapping = true;
                // signal that we're starting to transition
                StartSceneChangeForward();
            }
            else if (newTargetValue == 1.0f)
            {
                // set a flag to tell all incoming events to stop while we snap
                zoomIsSnapping = true;
                // signal that we're starting to transition
                StartSceneChangeBackward();
            }

            // how fast does this snapping work?
            float zoomSnapSpeed = 0.1f;
            // how close do we need to get to the target before we jump to that value?
            float snapTolerance = 0.01f;

            // cycle through which we're waiting for zoom target to snap into place
            while (Mathf.Abs(zoomTarget - newTargetValue) > snapTolerance)
            {
                // calculate the new value
                zoomTarget = Mathf.Lerp(zoomTarget, newTargetValue, zoomSnapSpeed);
                // apply that value
                ZoomToPct(zoomTarget);
                // wait for the next frame before moving again
                yield return new WaitForFixedUpdate();
            }
            // close enough, force that new value
            zoomTarget = newTargetValue;
            // now jump to that target
            ZoomToPct(newTargetValue);

            // if we were snapping forward
            if (newTargetValue == 1.0f)
            {
                // signal that we're done
                FinishSceneChangeBackward();
            }
            else if (newTargetValue == 0.0f)
            {
                // signal that we're done
                FinishSceneChangeForward();
            }
            // all done
            yield return null;
        }

        #endregion


        #region Snap

        void StartSceneChangeForward()
        {
            if (ZoomInStarted != null)
            {
                if (target == null)
                {
                    Debug.LogError("Starting snap into null");
                }
                else
                {
                    ZoomInStarted(target.name);
                }
            }
        }


        void StartSceneChangeBackward()
        {
            if (ZoomOutStarted != null)
            {
                ZoomOutStarted(null);
            }
        }


        void FinishSceneChangeForward()
        {
            if (ZoomInFinished != null)
            {
                if (target == null)
                {
                    Debug.LogError("Finishing snap into null");
                }
                else
                {
                    ZoomInFinished(target.name);
                }
            }
        }


        void FinishSceneChangeBackward()
        {
            if (ZoomOutFinished != null)
            {
                ZoomOutFinished(null);
            }
        }

        #endregion


        #region Rotate

        void Rotate(GameObject dragObject, Vector3 delta)
        {
            // if we're snapping, it's the end of the scene. Stop all incoming processes
            if (zoomIsSnapping) return;

            // if there is a co-routine running, stop it
            CancelResetPerspective();

            // get the current rotation in Euler Vector3 angle format
            Vector3 angles = this.transform.rotation.eulerAngles;

            // horizontal rotation around the object
            angles.y += delta.x * rotationSpeed;

            // vertical rotation
            angles.x -= delta.y * rotationSpeed;

            // if we want to clamp the vertical rotation
            if (clampVerticalAngle)
            {
                // clamp the vertical rotation to our two limits
                angles.x = Mathf.Clamp(angles.x, verticalMinimum, verticalMaximum);
            }

            // annul any incidential z rotations 
            angles.z = 0.0f;

            // apply results
            this.transform.rotation = Quaternion.Euler(angles);
        }


        void RotateToward(Quaternion newRotation)
        {
            // start a new routine
            StartCoroutine("RotateTowardRoutine", newRotation);
        }


        IEnumerator RotateTowardRoutine(Quaternion newRotation)
        {
            // loop while we're not yet at new rotation
            while (Quaternion.Angle(this.transform.rotation, newRotation) > 1.0f)
            {
                // Lerp to new rotation
                this.transform.rotation = Quaternion.Lerp(this.transform.rotation, newRotation, 0.25f);
                // wait for next frame
                yield return new WaitForEndOfFrame();
            }
            // snap to final rotation
            this.transform.rotation = startingRotation;
            // all done
            yield return null;
        }

        #endregion


        #region Fader

        /// <summary>
        /// Extracts the material from fader. There are two possible renderers:
        /// a simple Renderer or a SkinnedMeshRenderer. This will extract from either of the two
        /// </summary>
        /// <returns>The material found inside of the fader.</returns>
        /// <param name="fadeObject">Fade object.</param>

        Material[] ExtractMaterialsFromFader(GameObject fadeObject)
        {
            // try to extract a renderer
            Renderer renderer = fadeObject.GetComponent<Renderer>();
            if (renderer != null) 
            {
                return renderer.materials;
            }
            // try to extract a SkinnedMeshRenderer
            SkinnedMeshRenderer skinnedMeshRenderer = fadeObject.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                return skinnedMeshRenderer.materials;
            }
            // error
            Debug.LogError("No Renderer or SkinnedMeshRenderer");
            return null;
        }

        void HideEverything()
        {
            // go through all the focused objects
            foreach (KeyValuePair<GameObject, GameObject> focusedKeyValuePair in focusedFaders)
            {
                FadeMaterial(ExtractMaterialsFromFader(focusedKeyValuePair.Key), 0.0f);
            }

            // go through all the unfocused faders
            foreach (KeyValuePair<GameObject, GameObject> unfocusedKeyValuePair in unfocusedFaders)
            {
                // apply both fading values
                FadeMaterial(ExtractMaterialsFromFader(unfocusedKeyValuePair.Key), 0.0f);
            }
        }


        IEnumerator FadeInEverything()
        {
            float fadeSpeed = 0.05f;

            // fade everything in
            for (float opacity = 0.0f; opacity < 1.0f; opacity += fadeSpeed)
            {
                opacity = Mathf.Clamp01(opacity);

                // go through all the focused objects
                foreach (KeyValuePair<GameObject, GameObject> focusedKeyValuePair in focusedFaders)
                {
                    FadeMaterial(ExtractMaterialsFromFader(focusedKeyValuePair.Key), opacity);
                }

                // go through all the unfocused faders
                foreach (KeyValuePair<GameObject, GameObject> unfocusedKeyValuePair in unfocusedFaders)
                {
                    // apply both fading values
                    FadeMaterial(ExtractMaterialsFromFader(unfocusedKeyValuePair.Key), opacity);
                }

                yield return new WaitForEndOfFrame();
            }

        }


        void ApplyRendererOpacities()
        {
            float targetValue = targetCurve.Evaluate(zoomTarget);
            float focusedValue = focusableCurve.Evaluate(zoomTarget);
            float unfocusedValue = unfocusableCurve.Evaluate(zoomTarget) * xrayOpacityValue;
            float opaqueValue = 1.0f * xrayOpacityValue;

            // go through all the focused objects
            foreach (KeyValuePair<GameObject, GameObject> focusedKeyValuePair in focusedFaders)
            {
                GameObject childObject = focusedKeyValuePair.Key;
                Material[] childMaterials = ExtractMaterialsFromFader(childObject);
                GameObject parentObject = focusedKeyValuePair.Value;

                // if there is no target && the xray is not on && we're at the zoom-in point
                if (target == null && !xrayState && zoomTarget < zoomFadeMargin)
                {
                    FadeMaterial(childMaterials, opaqueValue);
                }
                // if this is the focused object, and we're zooming IN
                else if (parentObject == target && zoomTarget < zoomFadeMargin)
                {
                    //FadeMaterial(renderer.material, 1.0f);
                    FadeMaterial(childMaterials, targetValue);
                }
                // this is not the focused object but we're zooming in
                else if (parentObject != target && zoomTarget < zoomFadeMargin)
                {
                    FadeMaterial(childMaterials, unfocusedValue);
                }
                // otherwise this is either not the focused object or we're not zooming in
                else
                {
                    //FadeMaterial(renderer.material, focusedOpacity);
                    FadeMaterial(childMaterials, focusedValue);
                }
            }

            // go through all the unfocused faders
            foreach (KeyValuePair<GameObject, GameObject> unfocusedKeyValuePair in unfocusedFaders)
            {
                GameObject childObject = unfocusedKeyValuePair.Key;
                Material[] childMaterials = ExtractMaterialsFromFader(childObject);
                // if there is no target && the xray is not on && we're at the zoom-in point
                if (target == null && !xrayState && zoomTarget < zoomFadeMargin)
                {
                    FadeMaterial(childMaterials, opaqueValue);
                }
                else // there is a target, fade (if necessary) the unfocused renderer
                {
                    FadeMaterial(childMaterials, unfocusedValue);
                }
            }

        }

        void FadeMaterial(Material[] materials, float faderValue)
        {
            foreach(Material material in materials)
            {
                Color color = material.color;
                color.a = faderValue;
                material.color = color; 
            }
        }

        #endregion


        #region Perspective

        void ResetPerspective()
        {
            // make sure there aren't any previously running instances
            CancelResetPerspective();
            // rotate to original rotation
            RotateToward(startingRotation);
            // rotate to original position
            ZoomToward(zoomStartingPct);
        }


        void CancelResetPerspective()
        {
            StopCoroutine("RotateToward");
            StopCoroutine("ZoomToward");
        }

        #endregion


        #region Focus

        void FocusInit()
        {
            // memorize GameObjects (for targetting)
            MemorizeTargets();

            // if there is no assigned target and only one possible target, force this to the target
            if (target == null && focusableTargets.Count == 1)
            {
                target = focusableTargets[0];
            }
        }


        void ChangeFocus(GameObject newTarget)
        {
            // if we're snapping, it's the end of the scene. Stop all incoming processes
            if (zoomIsSnapping) return;

            // are we refocusing on an object
            if (newTarget != null)
            {
                // change focus to new object
                ChangeFocusToNewObject(newTarget);
            }
            else // refocusing on null
            {
                // change focus to null
                ChangeFocusToNull();
            }

        }


        void ChangeFocusToNewObject(GameObject newTarget)
        {
            // if the new target is not actually targetable
            if (!focusableTargets.Contains(newTarget)) return;

            // if we are already targeting this object
            if (target == newTarget)
            {
                // push in on this object
                //ZoomIn();
            }

            // remember target
            target = newTarget;
        }


        /// <summary>
        /// This is when we click in the void
        /// </summary>
        void ChangeFocusToNull()
        {
            // go back to original position
            ResetPerspective();

            // if we were previously focused on an object and there is more than one target
            if (focusableTargets.Count > 1)
            {
                // stop this selection
                target = null;
            }

            return;
        }


        void SelectPreviousFocus(bool useless = true)
        {
            // if we're snapping, it's the end of the scene. Stop all incoming processes
            if (zoomIsSnapping) return;

            // start with an index of none (null)
            int currentIndex = 0;

            // if there is a current target
            if (target != null)
            {
                // remember current index
                currentIndex = focusableTargets.IndexOf(target);
            }

            // wrap around
            if (currentIndex == -1 || (focusableTargets.Count == 1 || currentIndex <= 0))
            {
                currentIndex = focusableTargets.Count - 1;
            }
            else  // otherwise
            {
                // just decrement
                currentIndex -= 1;
            }

            // if we're null
            if (currentIndex == -1)
            {
                // turn off targetting
                target = null;
                // reset perspective to original position
                ResetPerspective();
            }
            else
            {
                // set the target
                target = focusableTargets[currentIndex];
            }

            // we've changed target state, so show currently available targetting options
            TurnOnXray();
        }


        void SelectNextFocus(bool useless = true)
        {
            // if we're snapping, it's the end of the scene. Stop all incoming processes
            if (zoomIsSnapping) return;

            // start with an index of none (null)
            int currentIndex = -1;
            // if there is a current target
            if (target != null)
            {
                // remember current index
                currentIndex = focusableTargets.IndexOf(target);
            }
            // wrap around
            if (currentIndex >= focusableTargets.Count - 1)
            {
                // only select null (-1) if there is more than one target
                if (focusableTargets.Count > 1)
                {
                    // set to null
                    currentIndex = -1;
                }
                else
                {
                    currentIndex = 0;
                }
            }
            else // otherwise
            {
                // incremement
                currentIndex += 1;
            }

            // if we're null
            if (currentIndex == -1)
            {
                target = null;
                // reset perspective to original position
                ResetPerspective();
            }
            else
            {
                // set the target using the index
                target = focusableTargets[currentIndex];
            }

            // we've changed target state, so show currently available targetting options
            TurnOnXray();
        }

        #endregion


        #region Xray

        void TouchChanged(int touchCount, int previousTouchCount)
        {
            // if we're touching down from touches off
            if (previousTouchCount == 0 && touchCount == 1 && !xrayState)
            {
                TurnOnXray();
            }
            // if we're releasing all touches 
            else if (touchCount > 1 && xrayState)
            {
                TurnOffXray();
            }
            // if xray was on and we're not longer touching
            else if (xrayState && touchCount == 0)
            {
                TurnOffXray();
            }

        }


        void TouchDragged(int touchCount)
        {
            if (xrayState)
            {
                TurnOffXray();
            }
        }


        /// <summary>
        /// Turns the X-Ray focus on, making clickable/zoomable objects visible
        /// </summary>

        void TurnOnXray()
        {
            xrayState = true;
            StartXray();
        }

        /// <summary>
        /// Turns the X-Ray focus off, making clickable/zoomable objects invisible
        /// </summary>

        void TurnOffXray()
        {
            xrayState = false;
            StartXray();
        }

        /// <summary>
        /// Turns the X-Ray focus on/off, making clickable/zoomable objects visible/invisible
        /// </summary>

        void ToggleXray()
        {
            xrayState = !xrayState;
            StartXray();
        }

        /// <summary>
        /// Starts the Xray routine
        /// </summary>

        void StartXray()
        {
            // turn off any other instances of this routine
            StopCoroutine("XrayRoutine");
            // start the routine to change xray
            StartCoroutine("XrayRoutine");
        }

        void ResetXrayCountdown()
        {
            //xrayCountdown = xrayDuration;
        }

        /// <summary>
        /// Animation co-routine to change opacity to new value
        /// </summary>

        IEnumerator XrayRoutine()
        {
            bool needToChangeOpacity = true;

            // if we're turning on xray
            if (xrayState)
            {
                // wait a little
                yield return new WaitForSeconds(0.1f);
            }

            while (needToChangeOpacity)
            {
                // calculate the xray values
                needToChangeOpacity = XrayChangeOpacity();
                // wait a cycle
                yield return new WaitForFixedUpdate();
            }

            //// so, did we just turn the xray on?
            //if (xrayState)
            //{
            //    // reset the xray timer to zero
            //    ResetXrayCountdown();

            //    // wait for xray countdown to finish
            //    while (xrayCountdown > 0.0f)
            //    {
            //        // countdown the timer
            //        xrayCountdown -= Time.deltaTime;
            //        // wait for frame
            //        yield return new WaitForEndOfFrame();
            //    }

            //    // now turn off
            //    xrayState = false;

            //    // reset requirement to change opacity
            //    needToChangeOpacity = true;

            //    while (needToChangeOpacity)
            //    {
            //        // calculate the xray values
            //        needToChangeOpacity = XrayChangeOpacity();
            //        // wait a cycle
            //        yield return new WaitForFixedUpdate();
            //    }
            //}

        }

        bool XrayChangeOpacity()
        {
            // if we're on, increase value
            if (xrayState == false) xrayOpacityValue += xraySpeed;
            else xrayOpacityValue -= xraySpeed; // or decrease

            // clamp values
            xrayOpacityValue = Mathf.Clamp(xrayOpacityValue, 0.1f, 1.0f);

            //Debug.Log(xrayOpacityValue);

            // apply these changes
            ApplyRendererOpacities();

            // calculate if we've reached the opacity goal
            if (xrayState == false && Mathf.Approximately(xrayOpacityValue, 1.0f))
            {   // ok, we're done
                return false;
            }
            else if (xrayState == true && Mathf.Approximately(xrayOpacityValue, 0.1f))
            {   // ok, we're done
                return false;
            }

            return true;
        }

        #endregion

    }

}

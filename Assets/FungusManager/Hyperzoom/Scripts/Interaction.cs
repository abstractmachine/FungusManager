using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
//using UnityEngine.UI;

using Fungus;

public class Interaction : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IScrollHandler
{
    #region Pointer class

    /// <summary>
    /// This Point class is used for tracking information on each pointer
    /// </summary>
    public class Pointer
    {
        public int id = -1;
        public Vector2 start = Vector2.zero;
        public Vector2 position = Vector2.zero;
        public Vector2 previous = Vector2.zero;
        public Vector2 delta = Vector2.zero;
        public float startTime = 0.0f;
    }

    #endregion


    #region Properties

    private float zoomSpeedPinch = 0.005f;
    private float zoomSpeedScroll = 0.1f;

    /// <summary>
    /// tracks whether we're dragging or not
    /// </summary>
    protected static bool didDrag = false;

    /// <summary>
    /// How many pixels do we have to drag to activate didDrag?
    /// </summary>
    protected static float dragActivationDistance = 40.0f;

    /// <summary>
    /// Tracks whether we're zooming or not
    /// </summary>
    protected static bool didZoom = false;

    /// <summary>
    /// Tracks whether the player held the click or not
    /// </summary>
    protected static bool didHold = false;

    /// <summary>
    /// How long it takes to declare a "hold"
    /// </summary>
    protected static float holdDelay = 0.6f;
    protected static float holdStart = 0.0f;

    /// <summary>
    /// Tracks whether we used multiple fingers
    /// </summary>
    protected static bool didMultitouch = false;

    /// <summary>
    /// The list of IDs for all the current fingers touching the screen
    /// </summary>
    protected static Dictionary<int, Pointer> pointerPositions = new Dictionary<int, Pointer>();

    /// <summary>
    /// how long we wait before considering that interaction has timed out.
    /// </summary>
    private float didInteractTimeoutDelay = 1.0f;

    /// <summary>
    /// The Camera attached to the gameObject using the Focus script
    /// </summary>
    private Camera currentCamera = null;

    #endregion


    #region Events

    /// <summary>
    /// Whenever we click & drag, send out this event
    /// </summary>
    public static event Action<GameObject, Vector3> DidRotate;

    /// <summary>
    /// Whenever we scroll || pinch, fire this event
    /// </summary>
    public static event Action<float> DidZoom;

    ///<summary>
    ///When we touch/click up — or scrolling time-outs —, fire this event
    ///</summary>
    ///<returns>The finish zoom.</returns>
    public static event Action<bool> DidFinishZoom;

    /// <summary>
    /// If scrollwheel is moving forward, and we're over a GameObject, send it as an event
    /// </summary>
    public static event Action<GameObject> DidScrollOverObject;

	public static event Action<int, int> TouchChanged;
	public static event Action<int> TouchDragged;

    #endregion


    #region Subclassed Events

    /// <summary>
    /// Whenever we've clicked on a focusable GameObject, fire this event
    /// </summary>
    public static event Action<GameObject> DidChangeFocus;

	/// <summary>
	/// This relays events from Interaction subclasses
	/// </summary>
	/// <param name="newFocusObject">New focus object.</param>
	protected virtual void ChangedFocus(GameObject newFocusObject)
    {
        Action<GameObject> handler = DidChangeFocus;
        if (handler != null) handler(newFocusObject);
    }

    #endregion


    #region Init

    void Start()
    {
        Hyperzoom hyperzoom = GameObject.FindObjectOfType<Hyperzoom>();
        if (hyperzoom == null)
        {
            Debug.LogError("Couldn't find Hyperzoom");    
        }
        else
		{
            currentCamera = hyperzoom.gameObject.GetComponentInChildren<Camera>(); 
            if (currentCamera == null)
			{
				Debug.LogError("Couldn't find Camera");
			}
		}
    }

    #endregion


    #region Drag

    public void OnBeginDrag(PointerEventData eventData)
    {
        //didDrag = true;
    }

    /// <summary>
    /// Handle drag events on this object
    /// </summary>
    /// <param name="eventData">The details of the user's interaction</param>

    public void OnDrag(PointerEventData eventData)
    {
        // avoid treating micro-movements as drag events
        // if we aren't dragging yet, and there is a starting point recorded for this pointer
        if (!didDrag && pointerPositions.ContainsKey(eventData.pointerId))
        {
            // calculate distance from starting point
            Vector2 startDelta = eventData.position - pointerPositions[eventData.pointerId].start;
            // if it's large enough
            if (startDelta.magnitude > dragActivationDistance)
            {   
                // activate this "yes we did drag" flag
                didDrag = true;
                // if there  are any listeners
                if (TouchDragged != null)
                {
                    // tell the listeners how many fingers there are
                    TouchDragged(pointerPositions.Count);
                }
            }
        }

        // if there are any listeners wanting to know if we've just dragged on this object
        if (didDrag)
        {
            // remember the position of this pointer
            // make sure this pointer ID doesn't already
            if (pointerPositions.ContainsKey(eventData.pointerId))
            {
                // remember previous position
                pointerPositions[eventData.pointerId].previous = pointerPositions[eventData.pointerId].position;
                // calculate delta using previous position
                pointerPositions[eventData.pointerId].delta = eventData.position - pointerPositions[eventData.pointerId].previous;
                // update new position
                pointerPositions[eventData.pointerId].position = eventData.position;
            }

			// if there are multitouches and this is #3, or there's only one touch
			if (pointerPositions.Count < 2 /*|| eventData.pointerId == 0*/)
            //if (pointerPositions.Count < 2)
            {
				if (DidRotate != null)
				{
					// rotate around focus pointusing delta data
					DidRotate(this.gameObject, eventData.delta);
                }
                // flag interaction
                DidInteract();
            }

            // if we're multitouch
            if (pointerPositions.Count == 2)
            {
                // calculate the delta changes to the pinch
                CalculatePinch();
                // flag interaction
                DidInteract();
            }

        } // if (DidDrag != null
    }
    // OnDrag()

    public void OnEndDrag(PointerEventData eventData)
    {
        //        // turn off the drag flag
        //        didDrag = false;
    }

    #endregion


    #region Scroll

    public void OnScroll(PointerEventData eventData)
    {
        // if there are any listeners wanting to know if we've just scrolled on this object
        if (DidZoom != null)
        {
            // extract delta from scroll
            Vector2 delta = eventData.scrollDelta;
            // extract the position
            Vector2 position = eventData.position;
            // re-calculate scroll speed
            delta.y *= zoomSpeedScroll;

            // if scrolling with the mouse
            if (!Mathf.Approximately(Mathf.Abs(delta.y), 0.0f))
            {
                // apply zoom action
                DidZoom(delta.y);
                // flag interaction
                DidInteract();

                // set the didZoom flag
                didZoom = true;
            }
        }
    }

    #endregion


    #region Multitouch

    void CalculatePinch()
    {
        // Find the magnitude of the vector (the distance) between the touches in each frame
        float previousTouchDeltaMagnitude = (pointerPositions[0].previous - pointerPositions[1].previous).magnitude;
        float currentTouchDeltaMagnitude = (pointerPositions[0].position - pointerPositions[1].position).magnitude;

        // Find the difference in the distances between each frame.
        float delta = previousTouchDeltaMagnitude - currentTouchDeltaMagnitude;

        // apply speed dampener
        delta *= zoomSpeedPinch;

        // if there are any listeners
        if (DidZoom != null)
        {
            // send out pinch event
            //DidPinch(delta);
            DidZoom(delta);
        }
        // set the didZoom flag
        didZoom = true;
        // flag interaction
        DidInteract();
    }

    #endregion


    #region PointerChange

    public void OnPointerDown(PointerEventData eventData)
	{
		// get the previous amount of pointers
		int previousCount = pointerPositions.Count;

		// if we need to reset flags
		if (previousCount == 0)
		{
			ResetFlags();
		}

        // create a new Pointer object
        // make sure this pointer ID doesn't already
        if (!pointerPositions.ContainsKey(eventData.pointerId))
        {
            Pointer pointer = new Pointer();
            pointer.id = eventData.pointerId;
            pointer.start = eventData.pressPosition;
            pointer.previous = eventData.position;
            pointer.position = eventData.position;
            pointer.startTime = Time.time;
            // remember this position
            pointerPositions.Add(eventData.pointerId, pointer);
        }

        // if we used multiple fingers
        if (pointerPositions.Count > 1)
        {
            // remember that we're multitouching
            didMultitouch = true;
        }

        // if there are any listeners
        if (TouchChanged != null)
        {
            // send out how many fingers/mouse are touching the screen
            TouchChanged(pointerPositions.Count, previousCount);
        }
    }
    // OnPointerDown

    public void OnPointerUp(PointerEventData eventData)
	{

		// get the previous amount of pointers
		int previousCount = pointerPositions.Count;

        // check to see if this key exists in the dictionary
        if (pointerPositions.ContainsKey(eventData.pointerId))
		{
            // get the start time of this click
            float timeLength = Time.time - pointerPositions[eventData.pointerId].startTime;
            // determine if this click was long enough to be considered a hold
            if (timeLength >= holdDelay)
            {
                // activate hold flag
                didHold = true;
                holdStart = Time.time;
            }

			// remove it
			pointerPositions.Remove(eventData.pointerId);
        }

        // if we neither zoomed nor dragged nor used any multitouch and now we're all released
        if (!didDrag && !didZoom && !didMultitouch && pointerPositions.Count == 0)
        {
            // activate the selection click
            PointerClicked();
        }

        // if we were zooming and now there are not longer any fingers
        if (didZoom && pointerPositions.Count == 0)
        {
            // if there are any listeners
            if (DidFinishZoom != null)
            {
                // send out a pinch done event
                DidFinishZoom(true);
            }
        }

        // if we need to reset flags (and since we've already used them in PointerClicked)
        if (pointerPositions.Count == 0)
        {
            ResetFlags();
		}

		// if there are any listeners
		if (TouchChanged != null)
		{
			// send out how many fingers/mouse are touching the screen
			TouchChanged(pointerPositions.Count, previousCount);
		}
    }
    // OnPointerDown


    void ResetFlags()
    {
		didZoom = false;
		didDrag = false;
		didHold = false;
		didMultitouch = false;
    }


    #endregion


    #region Click

    /// <summary>
    /// if is triggered, this is not a focus-able object
    /// </summary>

    public virtual void PointerClicked()
    {
        Debug.LogError("Unhandled PointerClicked() event");
    }

    #endregion


    #region Timeout

    void DidInteract()
    {
        // if there was a previous Timeout routine waiting to run
        StopCoroutine("DidInteractTimeoutRoutine");
        // start the routine to count down
        StartCoroutine("DidInteractTimeoutRoutine");
    }


    IEnumerator DidInteractTimeoutRoutine()
    {
        // wait for however long we need to wait
        yield return new WaitForSeconds(didInteractTimeoutDelay);
        // if there are any listeners
        if (DidFinishZoom != null)
        {
            // send out a zoom done event
            DidFinishZoom(true);
        }
        // all done
        yield return null;
    }

    #endregion

}

// class Interaction

//  IPointerEnterHandler - OnPointerEnter - Called when a pointer enters the object
//  IPointerExitHandler - OnPointerExit - Called when a pointer exits the object
//  IPointerDownHandler - OnPointerDown - Called when a pointer is pressed on the object
//  IPointerUpHandler - OnPointerUp - Called when a pointer is released (called on the original the pressed object)
//  IPointerClickHandler - OnPointerClick - Called when a pointer is pressed and released on the same object
//  IInitializePotentialDragHandler - OnInitializePotentialDrag - Called when a drag target is found, can be used to initialise values
//  IBeginDragHandler - OnBeginDrag - Called on the drag object when dragging is about to begin
//  IDragHandler - OnDrag - Called on the drag object when a drag is happening
//  IEndDragHandler - OnEndDrag - Called on the drag object when a drag finishes
//  IDropHandler - OnDrop - Called on the object where a drag finishes
//  IScrollHandler - OnScroll - Called when a mouse wheel scrolls
//  IUpdateSelectedHandler - OnUpdateSelected - Called on the selected object each tick
//  ISelectHandler - OnSelect - Called when the object becomes the selected object
//  IDeselectHandler - OnDeselect - Called on the selected object becomes deselected
//  IMoveHandler - OnMove - Called when a move event occurs (left, right, up, down, ect)
//  ISubmitHandler - OnSubmit - Called when the submit button is pressed
//  ICancelHandler - OnCancel - Called when the cancel button is pressed

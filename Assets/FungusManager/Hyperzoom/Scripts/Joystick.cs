using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joystick : MonoBehaviour
{
    public class Joypad
    {
        public bool left = false;
        public bool up = false;
        public bool right = false;
        public bool down = false;
        public float countdown = 0.0f;

        public void ResetCountdown() { countdown = 0.25f; }
        public bool CountdownExpired { get { countdown -= Time.deltaTime; return countdown <= 0.0f; } }
    }

    #region Properties

    public float horizontalRotationSpeedController = 10.0f;
    public float verticalRotationSpeedController = 10.0f;
    public float zoomSpeed = 0.333f;
    public float stickDeadZone = 0.005f;

    private Vector2 leftStickValue = Vector2.zero;
    private Vector2 rightStickValue = Vector2.zero;

    private Joypad leftJoypad = new Joypad();
    private Joypad shoulderButtons = new Joypad();
    private Joypad leftStick = new Joypad();

    #endregion


    #region Prototypes

    /// <summary>
    /// Right analog stick fires Rotation event
    /// </summary>
    public static event Action<GameObject, Vector3> DidRotate;
    /// <summary>
    /// Right analog stick fires Zoom event
    /// </summary>
    public static event Action<float> DidZoom;
    /// <summary>
    /// D-pad UP fires ZoomIn event
    /// </summary>
    public static event Action<bool> DidZoomIn;
    /// <summary>
    /// D-pad DOWN fire ZoomOut event
    /// </summary>
    public static event Action<bool> DidZoomOut;
    /// <summary>
    /// D-pad right + left stick RIGHT both fire SelectNextFocus event
    /// </summary>
    public static event Action<bool> DidSelectNextFocus;
	/// <summary>
	/// D-pad left + left stick LEFT both fire SelectPreviousFocus event
	/// </summary>
	public static event Action<bool> DidSelectPreviousFocus;
    /// <summary>
    /// whenever Analog stick returns to zero (for enough time), fire ZoomFinish event
    /// </summary>
    public static Action<bool> DidFinishZoom;

    #endregion


    #region Controller Polling

    void Update()
    {
        // check keyboard inpts
        UpdateKeyboard();
        // check controller button inputs
        UpdateDPad();
        UpdateButtons();
        // check analog stick inputs
        UpdateZoom();
        UpdateRotate();

    } // Update()


    void UpdateKeyboard()
    {
        // keyboard left arrow
        if (Input.GetKeyDown(KeyCode.LeftArrow)) SelectPreviousFocus();
        // keyboard right arrow
        if (Input.GetKeyDown(KeyCode.RightArrow)) SelectNextFocus();
        // keyboard up arrow
        if (Input.GetKeyDown(KeyCode.UpArrow)) ZoomIn();
        // keyboard down arrow
        if (Input.GetKeyDown(KeyCode.DownArrow)) ZoomOut();
    }


    void UpdateButtons()
    {
        // controller left shoulder button #1 && #2
        if (Input.GetKey(KeyCode.JoystickButton6) || Input.GetKey(KeyCode.JoystickButton8))
        {
            // send event
            if (!shoulderButtons.left) SelectPreviousFocus();
            // turn on current state
            shoulderButtons.left = true;
        }
        else // no left shoulder button press
        {
            // turn of current state
            shoulderButtons.left = false;
        }
        // controller right shoulder button #1 && #2
        if (Input.GetKey(KeyCode.JoystickButton7) || Input.GetKey(KeyCode.JoystickButton9))
        {
            // send event
            if (!shoulderButtons.right) SelectNextFocus();
            // turn on current state
            shoulderButtons.right = true;
        }
        else // no right should button press
        {
            // turn of current state
            shoulderButtons.right = false;
        }
    }


    void UpdateDPad()
    {
        // the left stick value
        leftStickValue = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        // the digital cross pad
        Vector2 dPad = Vector2.zero;
        //Vector2 dPad = new Vector2(Input.GetAxis("dPad_Horizontal"), Input.GetAxis("dPad_Vertical"));

        // if pushing all the way to the left
        if (dPad.y < -0.75f)
        {   // if we weren't previously pushing left
            if (!leftJoypad.up)
            {   // remember for next time
                leftJoypad.up = true;
                // Zoom In
                ZoomIn();
            }
        } // make sure we've crossed a threhold
        else if (dPad.y > -0.5f)
        {
            // reset left state
            leftJoypad.up = false;
        }

        // if pushing all the way to the right
        if (dPad.y > 0.75f)
        {   // if we weren't previously pushing right
            if (!leftJoypad.down)
            {   // remember for next time
                leftJoypad.down = true;
                // Zoom Out
                ZoomOut();
            }
        } // make sure we've crossed a threshold
        else if (dPad.y < 0.5f)
        {
            // reset right state
            leftJoypad.down = false;
        }

        // if pushing all the way to the left
        if (leftStickValue.x < -0.75f || dPad.x < -0.75f)
        {   // if we weren't previously pushing left
            if (!leftJoypad.left)
            {   // remember for next time
                leftJoypad.left = true;
                // Select Previous
                SelectPreviousFocus();
            }
        } // make sure we've crossed a threhold
        else if (leftStickValue.x > -0.5f && dPad.x > -0.5f)
        {
            // reset left state
            leftJoypad.left = false;
        }

        // if pushing all the way to the right
        if (leftStickValue.x > 0.75f || dPad.x > 0.75f)
        {   // if we weren't previously pushing right
            if (!leftJoypad.right)
            {   // remember for next time
                leftJoypad.right = true;
                // Select next
                SelectNextFocus();
            }
        } // make sure we've crossed a threshold
        else if (leftStickValue.x < 0.5f && dPad.x < 0.5f)
        {
            // reset right state
            leftJoypad.right = false;
        }
    }


    void UpdateZoom()
    {
        // the left stick value
        leftStickValue = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // if pushing all the way to down and not previously pushing down && the others are
        if (Mathf.Abs(leftStickValue.y) > stickDeadZone)
        {
            // reset countdown timer
            leftStick.ResetCountdown();

            // get current joystick axis value
            float verticalAxis = leftStickValue.y * zoomSpeed;
            // make sure there are listeners listening
            if (DidZoom != null)
            {
                // send DidZoom event with the delta change value
                DidZoom(verticalAxis);
            }

            // if this is a new direction
            if (leftStickValue.y < 0f && !leftStick.up)
            {
                // remember for next time
                leftStick.up = true;
            }
            else if (leftStickValue.y > 0f && !leftStick.down)
            {
                // remember for next time
                leftStick.down = true;
            }

        }

        // make sure we've crossed a threhold
        if (Mathf.Abs(leftStickValue.y) < stickDeadZone)
        {
            if (leftStick.CountdownExpired && (leftStick.down || leftStick.up))
            {
                // reset left state
                leftStick.up = false;
                leftStick.down = false;

                // send "finished zooming" event

                // make sure there are listeners
                if (DidFinishZoom != null)
                {
                    // send out a pinch done event
                    DidFinishZoom(true);
                }
            } // if (leftStick.CountdownExpired
        } // if (Mathf.Abs

    } // UpdateZoom


    void UpdateRotate()
    {
        Vector2 delta = Vector2.zero;

        // the right analog joysticks
        rightStickValue = new Vector2(Input.GetAxis("Horizontal-Right"), Input.GetAxis("Vertical-Right"));

        // if rotatation with left joystick around horizontal axis
        if (Mathf.Abs(rightStickValue.x) > stickDeadZone)
        {
            // get current joystick axis value
            delta.x = rightStickValue.x * horizontalRotationSpeedController;
            // make sure there are listeners listening
            if (DidRotate != null)
            {
                // send DidRotate event with the delta change value
                DidRotate(null, delta);
            }
        }

        // if rotatation with left joystick around horizontal axis
        if (Mathf.Abs(rightStickValue.y) > stickDeadZone)
        {
            // get current joystick axis value
            delta.y = rightStickValue.y * verticalRotationSpeedController;
            // make sure there are listeners listening
            if (DidRotate != null)
            {
                // send DidRotate event with the delta change value
                DidRotate(null, delta);
            }
        }

    } // UpdateRotate()

    #endregion


    #region Event Triggers

    void SelectNextFocus()
    {
        // make sure there is an event listener
        if (DidSelectNextFocus != null)
        {
            // send a "select previous focus" event
            DidSelectNextFocus(true);
        }
    } // SelectNextFocus()


	void SelectPreviousFocus()
    {
        // make sure there is an event listener
        if (DidSelectPreviousFocus != null)
        {
            // send a "select previous focus" event
            DidSelectPreviousFocus(true);
        }
    } // SelectPreviousFocus()


	void ZoomIn()
    {
        // make sure there is an event listener
        if (DidZoomIn != null)
        {
            // send a "select previous focus" event
            DidZoomIn(true);
        }
    } // ZoomIn()


	void ZoomOut()
    {
        // make sure there is an event listener
        if (DidZoomOut != null)
        {
            // send a "select previous focus" event
            DidZoomOut(true);
        }
    } // ZoomOut()

	#endregion
}

using UnityEngine;
using UnityEngine.EventSystems;

public class FungusCharacterManager : MonoBehaviour
{
    
    #region Init

    virtual protected void Awake()
    {
        EnableEventSystem();
    }

    void Start()
    {
        CheckForSceneManager();
    }


    void CheckForSceneManager()
    {

    }

    #endregion


    #region Activations

    protected void EnableEventSystem()
    {
        // get all the listeners
        EventSystem[] eventSystems = GameObject.FindObjectsOfType<EventSystem>();

        // if there are no EventSystems
        if (eventSystems.Length > 1)
        {
            // disable our own event system
            Debug.Log("Disable Our EventSystem");
        }

    } // EnableEventSystems()
    #endregion

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Zoomable : Interaction
{
    #region Click

    /// <summary>
    /// Whenever a pointer (finger/mouse) selects this GameObject
    /// </summary>

    public override void PointerClicked()
    {
        // if we didn't drag and we're not zooming
        if (!didDrag && !didZoom /* && !didHold */)
		{
			// send the new target object
			ChangedFocus(this.gameObject);
		}
    }

    #endregion
}

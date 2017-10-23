using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fungus
{
    public class Background : Interaction
    {
        #region Click

        /// <summary>
        /// Whenever a pointer (finger/mouse) selects the background
        /// </summary>

        public override void PointerClicked()
        {
            // if we didn't drag and we're not zooming
            if (!didDrag && !didZoom && !didHold)
            {
                // send null as the new target object
                ChangedFocus(null);
            }
        }

        #endregion
    }
}

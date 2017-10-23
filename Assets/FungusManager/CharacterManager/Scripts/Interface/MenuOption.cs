using UnityEngine;

namespace Fungus
{
    public class MenuOption : MonoBehaviour
    {
        [Tooltip("Should this menu item re-orient the parent menu to directly face the camera?")]
        public bool reorientMenuToCamera = false;

        static float staticOffset = 0.0f;
        float thisTimeOffset = 0.0f;
        float timeMultiplier = 1.0f;
        float amplitude = 20.0f;

        Camera currentCamera;
        Vector3 startingPosition;

        MenuDialog menuDialog;

        void Start()
        {
            RememberCamera();
            RememberMenu();

            staticOffset += Mathf.PI + Random.Range(-0.1f, 0.1f);
            timeMultiplier = Random.Range(-0.95f, 1.05f);
            thisTimeOffset = staticOffset;

            startingPosition = transform.localPosition;
        }


        void OnEnable()
        {
            if (reorientMenuToCamera)
            {
                ReorientMenuToFaceCamera();
            }
        }


        void OnDisable()
        {
        }


        void Update()
        {
            FloatAbout();
            LookAtCamera(this.gameObject);
        }


        void FloatAbout()
        {
            Vector3 movement = Vector3.zero;

            float t = ((Time.time * timeMultiplier) + thisTimeOffset);

            movement.x = Mathf.Cos(t);
            movement.x *= amplitude;
            movement.y = Mathf.Sin(2.0f * t) / 2.0f;
            movement.y *= amplitude;

            transform.localPosition = startingPosition + movement;
        }


        void LookAtCamera(GameObject gameObjectToOrient)
        {
            Vector3 relativePos = gameObjectToOrient.transform.position - currentCamera.transform.position;
            Quaternion rotation = Quaternion.LookRotation(relativePos);
            gameObjectToOrient.transform.rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, 0);
        }


        void RememberCamera()
        {
            if (currentCamera != null) return;

            currentCamera = Camera.main;

            if (currentCamera == null)
            {
                currentCamera = Camera.allCameras[0];
            }
        }


        void RememberMenu()
        {
            // if we've already found a reference, move on
            if (menuDialog != null) return;
            // find the root parent of this object

            // try to get a reference to the parent MenuDialog
            menuDialog = GetComponentInParent<MenuDialog>();
            // if we didn't find a MenuDialog then there is something wrong
            if (menuDialog == null)
            {
                // Bad problem
                Debug.LogError("Couldn't find parent MenuDialog in menu item " + this.name);
            }
        }


        void ReorientMenuToFaceCamera()
        {
            // make sure there is a camera
            if (currentCamera == null) RememberCamera();
            // make sure we have access to a parent MenuDialog
            if (menuDialog == null) RememberMenu();
            // get that MenuDialog's GameObject
            GameObject menuGameObject = menuDialog.gameObject;
            // re-orient that menu to the camera
            LookAtCamera(menuGameObject);
        }

    }

}

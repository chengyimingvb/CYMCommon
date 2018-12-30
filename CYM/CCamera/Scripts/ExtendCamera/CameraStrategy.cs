// StrategyCam v1.0
// 2014-03-04 (YYYY-MM-DD)
// by Fabian Hager


using UnityEngine;
using System.Collections;
namespace CYM.Cam
{
    public class CameraStrategy : MonoBehaviour
    {
        #region inspector
        // scroll, zoom, rotate and incline speeds
        public float scrollSpeed = 2.5f;
        public float zoomSpeed = 4f;
        public float rotateSpeed = 1f;
        public float inclineSpeed = 2.5f;

        // speed factors for key-controlled zoom, rotate and incline
        public float keyZoomSpeedFactor = 2f;
        public float keyRotateSpeedFactor = 1.5f;
        public float keyInclineSpeedFactor = 0.5f;

        // scroll, zoom, rotate and incline smoothnesses (higher values make the camera take more time to reach its intended position)
        public float scrollAndZoomSmooth = 0.08f;
        public float rotateAndInclineSmooth = 0.03f;

        // initital values for lookAt, zoom, rotation and inclination
        public float initialRotation = 0f; // 1 is one revolution
        public float initialInclination = 0.3f; // 0 is top-down, 1 is parallel to the x-z-plane
        public float initialZoom = 10f; // distance between camera position and the point on the x-z-plane the camera looks at
        public Vector3 initialLookAt = new Vector3(0f, 0f, 0f);

        // zoom to or from cursor modes
        public bool zoomOutFromCursor = false;
        public bool zoomInToCursor = true;

        // snapping rotation or inclination back to initial values after rotation or inclination keys have been released
        public bool snapBackRotation = false;
        public bool snapBackInclination = false;

        // snapping speed (for rotation and inclination)
        public float snapBackSpeed = 6f;

        // zooming in increases the inclination (from minInclination to maxInclination)
        public bool inclinationByZoom = false;

        // scroll, zoom, and inclination boundaries
        public float minX = -10f;
        public float maxX = 10f;
        public float minZ = -10f;
        public float maxZ = 10f;
        public float minZoom = 0.4f;
        public float maxZoom = 40f;
        public float minInclination = 0f;
        public float maxInclination = 0.9f;
        public float minRotation = -1f;
        public float maxRotation = 1f;


        // x-wrap and z-wrap (when reaching the boundaries the cam will jump to the other side, good for continuous maps)
        public bool xWrap = false;
        public bool zWrap = false;

        // keys for different controls
        public KeyCode keyScrollForward = KeyCode.W;
        public KeyCode keyScrollLeft = KeyCode.A;
        public KeyCode keyScrollBack = KeyCode.S;
        public KeyCode keyScrollRight = KeyCode.D;

        public KeyCode keyRotateAndIncline = KeyCode.LeftAlt;

        public KeyCode keyZoomIn = KeyCode.None;
        public KeyCode keyZoomOut = KeyCode.None;

        public KeyCode keyRotateLeft = KeyCode.None;
        public KeyCode keyRotateRight = KeyCode.None;

        public KeyCode keyInclineUp = KeyCode.None;
        public KeyCode keyInclineDown = KeyCode.None;
        #endregion

        #region prop
        Camera mainCamera;
        // width of the scroll sensitive area at the screen boundaries in px
        private int scrollBoundaries = 2;

        // target camera values (approached asymptotically for smoothing)
        private Vector3 targetLookAt = new Vector3(0f, 0f, 0f);
        private float targetZoom = 10f;
        private float targetRotation = 0f;
        private float targetInclination = 0f;

        // current camera values
        private Vector3 currentLookAt = new Vector3(0f, 0f, 0f);
        private float currentZoom = 10f;
        private float currentRotation = 0f;
        private float currentInclination = 0.3f;

        // snapping takes constant time (regardless of snapping distance), these values store where the snapping began (when the rotate and incline button was released)
        private float snapRotation = 0f;
        private float snapInclination = 0f;

        // for calculations
        private Vector3 viewDirection = new Vector3(0f, 0f, 0f);
        private float zoomRequest = 0f;
        private float rotateRequest = 0f;
        private float inclineRequest = 0f;
        private float targetSnap = 0f;
        private Vector3 xDirection = new Vector3(0f, 0f, 0f);
        private Vector3 yDirection = new Vector3(0f, 0f, 0f);
        private float oldTargetZoom = 0f;
        private float zoomPart = 0f;
        private Vector3 cursorPosition = new Vector3(0f, 0f, 0f);
        private Vector3 cursorDifference = new Vector3(0f, 0f, 0f);

        // for calculations (making camera behaviour framerate-independent)
        private float time = 0f;

        // to determine if camera is currently being rotated by the user
        private bool rotating = false;
        private Vector3 lastMousePosition = new Vector3(0f, 0f);

        // for trigonometry
        private static float TAU = Mathf.PI * 2;


        // access methods for camera values for "jumps"
        private float desiredZoom = 0f;
        private bool changeZoom = false;
        public void SetZoom(float zoom)
        {
            if (zoom < minZoom) zoom = minZoom;
            if (zoom > maxZoom) zoom = maxZoom;
            desiredZoom = zoom;
            changeZoom = true;
        }
        private float desiredRotation = 0f;
        private bool changeRotation = false;
        public void SetRotation(float rotation)
        {
            if (rotation < minRotation) rotation = minRotation;
            if (rotation > maxRotation) rotation = maxRotation;
            desiredRotation = rotation;
            changeRotation = true;
        }
        private float desiredInclination = 0f;
        private bool changeInclination = false;
        public void SetInclination(float inclination)
        {
            if (inclination < minInclination) inclination = minInclination;
            if (inclination > maxInclination) inclination = maxInclination;
            desiredInclination = inclination;
            changeInclination = true;
        }
        private Vector3 desiredLookAt = new Vector3(0f, 0f, 0f);
        private bool changeLookAt = false;
        public void setLookAt(Vector3 lookAt)
        {
            if (lookAt.x < minX) lookAt.x = minX;
            if (lookAt.x > maxX) lookAt.x = maxX;
            if (lookAt.z < minZ) lookAt.z = minZ;
            if (lookAt.z > maxZ) lookAt.z = maxZ;
            lookAt.y = 0f;
            desiredLookAt = lookAt;
            changeLookAt = true;
        }

        #endregion

        #region life
        void Awake()
        {
            mainCamera = GetComponent<Camera>();
        }
        void Update()
        {


            // handle the "jumps"
            if (changeZoom)
            {
                targetZoom = desiredZoom;
                changeZoom = false;
            }
            if (changeRotation)
            {
                targetRotation = desiredRotation;
                changeRotation = false;
            }
            if (changeLookAt)
            {
                targetLookAt = desiredLookAt;
                changeLookAt = false;
            }
            if (changeInclination)
            {
                targetInclination = desiredInclination;
                changeInclination = false;
            }


            time = Time.deltaTime;


            // determine directions the camera should move in when scrolling
            yDirection.x = transform.up.x;
            yDirection.y = 0;
            yDirection.z = transform.up.z;
            yDirection = yDirection.normalized;
            xDirection.x = transform.right.x;
            xDirection.y = 0;
            xDirection.z = transform.right.z;
            xDirection = xDirection.normalized;


            // scrolling when the cursor touches the screen boundaries or when WASD keys are used
            if (Input.mousePosition.x >= Screen.width - scrollBoundaries || Input.GetKey(keyScrollRight))
            {
                targetLookAt.x = targetLookAt.x + xDirection.x * scrollSpeed * time * targetZoom;
                targetLookAt.z = targetLookAt.z + xDirection.z * scrollSpeed * time * targetZoom;
            }
            if (Input.mousePosition.x <= scrollBoundaries || Input.GetKey(keyScrollLeft))
            {
                targetLookAt.x = targetLookAt.x - xDirection.x * scrollSpeed * time * targetZoom;
                targetLookAt.z = targetLookAt.z - xDirection.z * scrollSpeed * time * targetZoom;
            }
            if (Input.mousePosition.y >= Screen.height - scrollBoundaries || Input.GetKey(keyScrollForward))
            {
                targetLookAt.x = targetLookAt.x + yDirection.x * scrollSpeed * time * targetZoom;
                targetLookAt.z = targetLookAt.z + yDirection.z * scrollSpeed * time * targetZoom;
            }
            if (Input.mousePosition.y <= scrollBoundaries || Input.GetKey(keyScrollBack))
            {
                targetLookAt.x = targetLookAt.x - yDirection.x * scrollSpeed * time * targetZoom;
                targetLookAt.z = targetLookAt.z - yDirection.z * scrollSpeed * time * targetZoom;
            }


            // zooming when the mousewheel or the zoom keys are used
            zoomRequest = Input.GetAxis("Mouse ScrollWheel");
            if (Input.GetKey(keyZoomIn)) zoomRequest += keyZoomSpeedFactor * time;
            if (Input.GetKey(keyZoomOut)) zoomRequest -= keyZoomSpeedFactor * time;
            if (zoomRequest != 0)
            {
                // needed for zoom to cursor behaviour
                oldTargetZoom = targetZoom;
                // zoom
                targetZoom = targetZoom - zoomSpeed * targetZoom * zoomRequest;
                // enforce zoom boundaries
                if (targetZoom > maxZoom) targetZoom = maxZoom;
                if (targetZoom < minZoom) targetZoom = minZoom;
                // zoom-dependent inclination behaviour
                if (inclinationByZoom)
                {
                    // determine where the current targetZoom is in the range of the zoomBoundaries
                    zoomPart = 1f - ((targetZoom - minZoom) / maxZoom);
                    // make sure the inclination increase mostly happens at the lowest zoom levels (when the camera is closest to the x-z-plane)
                    zoomPart = zoomPart * zoomPart * zoomPart * zoomPart * zoomPart * zoomPart * zoomPart * zoomPart;
                    // apply the inclination
                    targetInclination = minInclination + zoomPart * (maxInclination - minInclination);
                }
                // zoom to and from cursor behaviour
                if ((zoomRequest > 0 && zoomInToCursor) || (zoomRequest < 0 && zoomOutFromCursor))
                {
                    // determine the point on the x-z-plane the cursor hovers over
                    cursorPosition = mainCamera.ScreenPointToRay(Input.mousePosition).direction;
                    cursorPosition = transform.position + cursorPosition * (transform.position.y / -cursorPosition.y);
                    // move closer to that point (in the same proportion the zoom has just changed)
                    targetLookAt = targetLookAt - ((oldTargetZoom - targetZoom) / (oldTargetZoom)) * (targetLookAt - cursorPosition);
                }
            }



            // rotating and inclining when the middle mouse button or Left ALT is pressed
            rotateRequest = 0f;
            inclineRequest = 0f;
            if (Input.GetMouseButton(2) || Input.GetKey(keyRotateAndIncline))
            {
                if (rotating)
                {
                    // how far the cursor has travelled on screen
                    cursorDifference = Input.mousePosition - lastMousePosition;
                    // determine the rotation
                    rotateRequest += rotateSpeed * 0.001f * cursorDifference.x;
                    // determine the inclination
                    inclineRequest -= inclineSpeed * 0.001f * cursorDifference.y;
                }
                else
                {
                    // rotation has just started
                    rotating = true;
                }
                // store cursor position
                lastMousePosition = Input.mousePosition;
            }
            else
            {
                rotating = false;
            }
            // key controlled rotation and inclination
            if (Input.GetKey(keyRotateLeft)) rotateRequest += keyRotateSpeedFactor * rotateSpeed * time;
            if (Input.GetKey(keyRotateRight)) rotateRequest -= keyRotateSpeedFactor * rotateSpeed * time;
            if (Input.GetKey(keyInclineUp)) inclineRequest += keyInclineSpeedFactor * inclineSpeed * time;
            if (Input.GetKey(keyInclineDown)) inclineRequest -= keyInclineSpeedFactor * inclineSpeed * time;

            if (rotateRequest != 0f)
            {
                // apply rotation
                targetRotation += rotateRequest;
                // enforce boundaries
                if (targetRotation > maxRotation) targetRotation = maxRotation;
                if (targetRotation < minRotation) targetRotation = minRotation;
                // make sure rotation stays in the interval between -0.5 and 0.5;
                if (targetRotation > 0.5f)
                {
                    targetRotation -= 1f;
                    currentRotation -= 1f;
                }
                else if (targetRotation < -0.5f)
                {
                    targetRotation += 1f;
                    currentRotation += 1f;
                }
                // in case inclination stops afterwards store the last inclination (for snapping)
                snapRotation = targetRotation;
                rotateRequest = 0f;
            }
            else if (!rotating)
            {
                // snap back
                if (snapBackRotation)
                {
                    // determine the next rotation value assuming constant snap speed
                    targetSnap = targetRotation + time * snapBackSpeed * (initialRotation - snapRotation);
                    // finish the snap when it would diverge again (this means it has reached or overshot the initial rotation)
                    if (Mathf.Abs(targetSnap - initialRotation) > Mathf.Abs(targetRotation - initialRotation)) targetSnap = initialRotation;
                    // apply the snap
                    targetRotation = targetSnap;
                }
            }

            if (!inclinationByZoom && inclineRequest != 0f)
            {
                // apply inclination
                targetInclination += inclineRequest;
                // enforce boundaries
                if (targetInclination > maxInclination) targetInclination = maxInclination;
                if (targetInclination < minInclination) targetInclination = minInclination;
                // in case inclination stops afterwards store the last inclination (for snapping)
                snapInclination = targetInclination;
                inclineRequest = 0f;
            }
            else if (!rotating)
            {
                // snap back
                if (snapBackInclination && !inclinationByZoom)
                {
                    // determine the next inclination value assuming constant snap speed
                    targetSnap = targetInclination + time * snapBackSpeed * (initialInclination - snapInclination);
                    // finish the snap when it would diverge again (this means it has reached or overshot the initial inclination)
                    if (Mathf.Abs(targetSnap - initialInclination) > Mathf.Abs(targetInclination - initialInclination)) targetSnap = initialInclination;
                    // apply the snap
                    targetInclination = targetSnap;
                }
            }



            // enforce scroll boundaries
            if (xWrap)
            {
                if (targetLookAt.x > maxX)
                {
                    targetLookAt.x -= maxX - minX;
                    currentLookAt.x -= maxX - minX;
                }
                if (targetLookAt.x < minX)
                {
                    targetLookAt.x += maxX - minX;
                    currentLookAt.x += maxX - minX;
                }
            }
            else
            {
                if (targetLookAt.x > maxX) targetLookAt.x = maxX;
                if (targetLookAt.x < minX) targetLookAt.x = minX;
            }
            if (zWrap)
            {
                if (targetLookAt.z > maxZ)
                {
                    targetLookAt.z -= maxZ - minZ;
                    currentLookAt.z -= maxZ - minZ;
                }
                if (targetLookAt.z < minZ)
                {
                    targetLookAt.z += maxZ - minZ;
                    currentLookAt.z += maxZ - minZ;
                }
            }
            else
            {
                if (targetLookAt.z > maxZ) targetLookAt.z = maxZ;
                if (targetLookAt.z < minZ) targetLookAt.z = minZ;
            }





            // calculate the current values (let them approach target values asymptotically - this is the actual smoothing)
            currentLookAt = currentLookAt - (time * (currentLookAt - targetLookAt)) / scrollAndZoomSmooth;
            currentZoom = currentZoom - (time * (currentZoom - targetZoom)) / scrollAndZoomSmooth;
            currentRotation = currentRotation - (time * (currentRotation - targetRotation)) / rotateAndInclineSmooth;
            if (inclinationByZoom)
            {
                // zoom-dependent inclination means the smoothing must be the smoothing when zooming
                currentInclination = currentInclination - (time * (currentInclination - targetInclination)) / scrollAndZoomSmooth;
            }
            else
            {
                currentInclination = currentInclination - (time * (currentInclination - targetInclination)) / rotateAndInclineSmooth;
            }


            // calculate the viewDirection of the camera from inclination and rotation
            viewDirection = (Vector3.down * (1.0f - currentInclination) + new Vector3(Mathf.Sin(currentRotation * TAU), 0, Mathf.Cos(currentRotation * TAU)) * (currentInclination)).normalized;
            // apply the current camera values to move and rotate the camera
            transform.position = currentLookAt + currentZoom * (-viewDirection);
            transform.LookAt(currentLookAt);


        }
        void Start()
        {

            // enforce sanity values (preventing weird effects or NaN results due to division by 0
            if (scrollAndZoomSmooth < 0.01f) scrollAndZoomSmooth = 0.01f;
            if (rotateAndInclineSmooth < 0.01f) rotateAndInclineSmooth = 0.01f;

            if (minInclination < 0.001f) minInclination = 0.001f;

            // enforce that initial values are within boundaries
            if (initialInclination < minInclination) initialInclination = minInclination;
            if (initialInclination > maxInclination) initialInclination = maxInclination;

            if (initialRotation < minRotation) initialRotation = minRotation;
            if (initialRotation > maxRotation) initialRotation = maxRotation;

            if (initialZoom < minZoom) initialZoom = minZoom;
            if (initialZoom > maxZoom) initialZoom = maxZoom;

            initialLookAt.y = 0f;
            if (initialLookAt.x > maxX) initialLookAt.x = maxX;
            if (initialLookAt.x < minX) initialLookAt.x = minX;
            if (initialLookAt.z > maxZ) initialLookAt.z = maxZ;
            if (initialLookAt.z < minZ) initialLookAt.z = minZ;

            // initialise current camera values
            currentZoom = initialZoom;
            currentInclination = initialInclination;
            currentRotation = initialRotation;
            currentLookAt = initialLookAt;

            // initialise target camera values (to current camera values)
            targetLookAt = currentLookAt;
            targetInclination = currentInclination;
            targetZoom = currentZoom;
            targetRotation = currentRotation;
        }
        #endregion
    }
}
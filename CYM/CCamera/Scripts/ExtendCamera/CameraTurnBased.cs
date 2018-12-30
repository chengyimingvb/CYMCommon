using UnityEngine;
using System;
using System.Collections;
using System.Threading;
namespace CYM.Cam
{
    public class CameraTurnBased : MonoBehaviour
    {
        #region public
        public Camera MainCamera;              // main camera
        public Transform MainTarget;           // main object
        public int DistanceY;                  // Camera Start Pos Y
        public int CameraZoomMin;              // Min Zoom 
        public int CameraZoomMax;              // Max Zoom
        public int CameraSpeed;                // Camera Speed 
        public int ScrollWheelSpeed;           // Scroll Speed
        public int CameraZoomSpeed;            // Zoom Speed With out Scroll Wheel
        public float XSpeed;                   // X Rotate Speed
        public float YSpeed;                   // Y Rotate Speed
        public float XMinLimit;                // X Min Angle
        public float XMaxLimit;                // Y Max Angle

        //public GUISkin WebLabel;
        #endregion

        #region private
        private bool IsActivated;              // When Camera Rotate she cant move.
        private StrategyCamera SCamera;        // Camera Class
        private float lastClickTime = 0;
        private float catchTime = 0.15f;

        private bool CheckBounds = true;
        #endregion

        private void Start()
        {
            #region Start
            CameraInit();
            #endregion
        }

        private void Update()
        {
            #region Update
            // ------------------------ Camera Rotate-----------------------
            #region CameraRotate
            if (Input.GetMouseButtonDown(1))
                IsActivated = true;


            if (Input.GetMouseButtonUp(1))
                IsActivated = false;


            if (MainTarget && IsActivated)
                SCamera.CameraRotate();
            #endregion
            // ------------------------ Camera Rotate-----------------------


            // ------------------------ Camera Zoom ------------------------
            #region CameraZoom
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
                SCamera.CameraScrollWheelZoom();            // Zoom With using Scroll Wheel

            if (Input.GetKey(KeyCode.Q))
                SCamera.CameraZoomIn();  // Zoom in With out using Scroll

            if (Input.GetKey(KeyCode.W))
                SCamera.CameraZoomOut(); // Zoom out With out using Scroll
            #endregion
            // ------------------------ Camera Zoom ------------------------





            // ------------------------ Double Click Set Focus On Object ---
            #region DoubleClickFocusOnObject
            if (Input.GetMouseButtonDown(0))
            {
                if (Time.time - lastClickTime < catchTime)
                {
                    SCamera.SetPosPickObject();
                    //SCamera.FocusOnPickingObject(5.0f);
                }
                lastClickTime = Time.time;
            }
            #endregion
            // ------------------------ Double Click Set Focus on Object ---


            // ------------------------ Check Bounds ----------------------- 
            #region CameraCheckBounds
            if (CheckBounds)
                SCamera.CameraCheckBound(0, 40, 40, 0, 3, 5);
            else
                SCamera.CameraCheckBoundReturn(0, 40, 40, 0, 3);

            #endregion
            // ------------------------ Check Bounds ----------------------- 


            // ------------------------ Focus on Object Lerp ---------------
            #region FocusOnPickingObjectLerp
            SCamera.FocusOnPickingObjectLerp(5.0f);
            #endregion
            // ------------------------ Focus on Object Lerp ---------------


            #endregion
        }

        private void FixedUpdate()
        {
            // ------------------------ Camera Move ------------------------
            #region CameraMove
            if (Input.GetMouseButton(0) && !IsActivated && Time.time - lastClickTime >= catchTime)
            {
                SCamera.CameraDragMove();
            }
            else if (!IsActivated)
            {
                if (20 > Input.mousePosition.x)
                    SCamera.CameraMoveLeft();

                if ((Screen.width - 20) < Input.mousePosition.x)
                    SCamera.CameraMoveRight();

                if (20 > Input.mousePosition.y)
                    SCamera.CameraMoveDown();

                if ((Screen.height - 20) < Input.mousePosition.y)
                    SCamera.CameraMoveUp();
            }
            #endregion
            // ------------------------ Camera Move ------------------------
        }

        private void CameraInit()
        {
            #region CameraInit
            if (MainCamera != null)
            {
                SCamera = gameObject.AddComponent<StrategyCamera>();

                SCamera.MainCamera = MainCamera;
                SCamera.Target = MainTarget;

                SCamera.ScrollWheelSpeed = ScrollWheelSpeed;
                SCamera.CameraZoomMin = CameraZoomMin;
                SCamera.CameraZoomMax = CameraZoomMax;
                SCamera.CameraZoomSpeed = CameraZoomSpeed;
                SCamera.CameraMoveSpeed = CameraSpeed;

                SCamera.DistanceY = DistanceY;
                SCamera.XSpeed = XSpeed;
                SCamera.YSpeed = YSpeed;
                SCamera.XMinLimit = XMinLimit;
                SCamera.XMaxLimit = XMaxLimit;

                SCamera.CameraStartPosition();

                SCamera.CameraLayers = Convert.ToInt32("0000" + "0000" + "0000" + "0000" + "0000" + "0000" + "0000" + "0001", 2);

                // the numbering Layers begins from the end.  1 set 0 disable
            }
            else
                print("Camera not found");
            #endregion
        }

        #region Camera core
        public class StrategyCamera : MonoBehaviour
        {
            #region properties
            public Camera MainCamera { get; set; }          // Main Camera
            public Transform Target { get; set; }           // main object X,Y,Z position

            public int ScrollWheelSpeed { get; set; }       // Scroll Speed
            public int CameraZoomMin { get; set; }          // Min Zoom 
            public int CameraZoomMax { get; set; }          // Max Zoom
            public int CameraZoomSpeed { get; set; }        // Zoom Speed With out Scroll Wheel
            public int CameraMoveSpeed { get; set; }        // Camera Speed Move 

            public float DistanceY { get; set; }    // Start Camera Y position
            public float XSpeed { get; set; }       // X Rotate Speed
            public float YSpeed { get; set; }       // Y Rotate Speed
            public float XMinLimit { get; set; }    // X Min Angle
            public float XMaxLimit { get; set; }    // X Max Angle

            public int CameraLayers { get; set; } // Set Camera Layers
            #endregion

            #region private
            private float x = 0.0f;
            private float y = 0.0f;
            private Vector3 Position;  // Main Object and Camera Position
            private RaycastHit Hit;

            private Vector3 CurrentPosition; // Current position main object and camera
            private bool StartLerp;
            #endregion

            #region utility
            public void CameraStartPosition()
            {
                #region CameraStartPosition
                // Init Camera Position
                x = MainCamera.transform.rotation.eulerAngles.x; // Start Camera Position Rotate Angle X if its not in bound he set default between Max X and Min X
                DistanceY *= 2; // DistanceY * 2 for set real position
                x = ClampAngle(x, XMinLimit, XMaxLimit);
                var rotation = Quaternion.Euler(x, y, 0);
                var position = rotation * new Vector3(0, 0, -DistanceY) + Target.position;

                MainCamera.transform.rotation = rotation;
                MainCamera.transform.position = position;
                // Init Camera Position
                #endregion
            }

            public void CameraRotate()
            {
                #region CameraRotate
                StartLerp = false;        // Check continue lerp or no

                y += Input.GetAxis("Mouse X") * XSpeed * 0.02f;
                x -= Input.GetAxis("Mouse Y") * YSpeed * 0.02f;
                x = ClampAngle(x, XMinLimit, XMaxLimit);
                var rotation = Quaternion.Euler(x, y, 0);
                Target.rotation = Quaternion.Euler(0, y, 0);
                var position = rotation * new Vector3(0, 0, -DistanceY) + Target.position;

                MainCamera.transform.rotation = rotation;
                MainCamera.transform.position = position;
                #endregion
            }

            private float ClampAngle(float angle, float min, float max)
            {
                #region ClampAngle
                // Check angles for rotation
                if (angle < -360)
                    angle += 360;
                if (angle > 360)
                    angle -= 360;
                return Mathf.Clamp(angle, min, max);
                // Check angles for rotation
                #endregion
            }

            public void CameraScrollWheelZoom()
            {
                #region CameraScrollWheelZoom
                StartLerp = false; // Check continue lerp or no

                DistanceY = Vector3.Distance(transform.position, Target.position);
                DistanceY = ZoomLimit(DistanceY - Input.GetAxis("Mouse ScrollWheel") * ScrollWheelSpeed, CameraZoomMin, CameraZoomMax);

                Position = -(transform.forward * DistanceY) + Target.position;
                MainCamera.transform.position = Position;
                #endregion
            }

            public void CameraZoomIn()
            {
                #region CameraZoomIn
                StartLerp = false; // Check continue lerp or no

                DistanceY = Vector3.Distance(transform.position, Target.position);
                DistanceY = ZoomLimit(DistanceY - CameraZoomSpeed * Time.deltaTime, CameraZoomMin, CameraZoomMax);

                Position = -(transform.forward * DistanceY) + Target.position;
                MainCamera.transform.position = Position;
                #endregion
            }

            public void CameraZoomOut()
            {
                #region CameraZoomOut
                StartLerp = false; // Check continue lerp or no

                DistanceY = Vector3.Distance(transform.position, Target.position);
                DistanceY = ZoomLimit(DistanceY + CameraZoomSpeed * Time.deltaTime, CameraZoomMin, CameraZoomMax);

                Position = -(transform.forward * DistanceY) + Target.position;
                MainCamera.transform.position = Position;
                #endregion
            }

            private float ZoomLimit(float dist, float min, float max)
            {
                #region ZoomLimit
                // Check Zoom limit
                if (dist < min)
                    dist = min;
                if (dist > max)
                    dist = max;
                return dist;
                // Check Zoom limit
                #endregion
            }

            public void CameraDragMove()
            {
                #region CameraDragMove 
                StartLerp = false;      // Check continue lerp or no
                Target.position -= Target.TransformDirection(new Vector3(
                        Input.GetAxis("Mouse X") * CameraMoveSpeed * Time.deltaTime,
                        0,
                        Input.GetAxis("Mouse Y") * CameraMoveSpeed * Time.deltaTime));
                #endregion
            }

            public void CameraMoveLeft()
            {
                #region CameraMoveLeft
                // Move Left
                StartLerp = false; // Check continue lerp or no
                Target.transform.position -= Target.transform.TransformDirection(new Vector3(CameraMoveSpeed * Time.deltaTime, 0, 0));
                #endregion
            }

            public void CameraMoveRight()
            {
                #region CameraMoveRight
                // Move right
                StartLerp = false; // Check continue lerp or no
                Target.transform.position += Target.transform.TransformDirection(new Vector3(CameraMoveSpeed * Time.deltaTime, 0, 0));
                #endregion
            }

            public void CameraMoveDown()
            {
                #region CameraMoveDown
                // Move down
                StartLerp = false;  // Check continue lerp or no
                Target.transform.position -= Target.transform.TransformDirection(new Vector3(0, 0, CameraMoveSpeed * Time.deltaTime));
                #endregion
            }

            public void CameraMoveUp()
            {
                #region CameraMoveUp
                // Move up
                StartLerp = false; // Check continue lerp or no
                Target.transform.position += Target.transform.TransformDirection(new Vector3(0, 0, CameraMoveSpeed * Time.deltaTime));
                #endregion
            }

            public void CameraCheckBound(
                int leftMaxBound,
                int rightMaxBound,
                int upMaxBound,
                int downMaxBound,
                int offsetBound,
                int speedReturn)
            {
                #region CameraCheckBounds
                // Check Left Map Bound
                if (Target.transform.position.x < -(leftMaxBound + offsetBound))
                {
                    Target.transform.position = Vector3.Lerp(
                        Target.transform.position,
                        new Vector3(leftMaxBound, 0, Target.transform.position.z),
                        speedReturn * Time.deltaTime);
                }
                // Check Left Map Bound


                // Check Down Map Bound
                if (Target.transform.position.z < -(downMaxBound + offsetBound))
                {
                    Target.transform.position = Vector3.Lerp(
                        Target.transform.position,
                        new Vector3(Target.transform.position.x, 0, downMaxBound),
                        speedReturn * Time.deltaTime);
                }
                // Check Down Map Bound


                // Check Right Map Bound
                if (Target.transform.position.x > rightMaxBound + offsetBound)
                {
                    Target.transform.position = Vector3.Lerp(
                        Target.transform.position,
                        new Vector3(rightMaxBound, 0, Target.transform.position.z),
                        speedReturn * Time.deltaTime);
                }
                // Check Right Map Bound


                // Check Up Map Bound
                if (Target.transform.position.z > upMaxBound + offsetBound)
                {
                    Target.transform.position = Vector3.Lerp(
                        Target.transform.position,
                        new Vector3(Target.transform.position.x, 0, upMaxBound),
                        speedReturn * Time.deltaTime);
                }
                // Check Up Map Bound
                #endregion
            }

            public void CameraCheckBoundReturn(
                int leftMaxBound,
                int rightMaxBound,
                int upMaxBound,
                int downMaxBound,
                int offsetBound)
            {
                #region CameraCheckBoundsSpherical
                if (Target.transform.position.x < -(leftMaxBound + offsetBound))
                {
                    Target.transform.position = new Vector3(rightMaxBound, 0, Target.transform.position.z);
                }

                if (Target.transform.position.z < -(downMaxBound + offsetBound))
                {
                    Target.transform.position = new Vector3(Target.transform.position.x, 0, upMaxBound);
                }

                if (Target.transform.position.x > rightMaxBound + offsetBound)
                {
                    Target.transform.position = new Vector3(leftMaxBound, 0, Target.transform.position.z);
                }

                if (Target.transform.position.z > upMaxBound + offsetBound)
                {
                    Target.transform.position = new Vector3(Target.transform.position.x, 0, downMaxBound);
                }
                #endregion
            }

            public void SetPosPickObject()
            {
                #region FocusOnPickingObject
                Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out Hit, Mathf.Infinity, CameraLayers))
                {
                    if (Hit.collider != null)
                    {
                        CurrentPosition = new Vector3(Hit.collider.transform.position.x, 0, Hit.collider.transform.position.z);
                        StartLerp = true;
                    }
                }
                #endregion
            }

            public void FocusOnPickingObject(float distance)
            {
                #region FocusOnPickingObject
                Target.transform.position = CurrentPosition;

                DistanceY = Vector3.Distance(transform.position, Target.position);
                DistanceY = ZoomLimit(distance, CameraZoomMin, CameraZoomMax);

                Position = -(transform.forward * DistanceY) + Target.position;
                MainCamera.transform.position = Position;
                #endregion
            }

            public void FocusOnPickingObjectLerp(float distance)
            {
                #region FocusOnPickingObjectLerp
                if (StartLerp)
                {
                    Target.transform.position = Vector3.Lerp(Target.transform.position, CurrentPosition, 10 * Time.deltaTime);
                    float CheckDist = Vector3.Distance(Target.transform.position, CurrentPosition);

                    DistanceY = Vector3.Distance(transform.position, Target.position);
                    DistanceY = ZoomLimit(distance, CameraZoomMin, CameraZoomMax);

                    Position = -(transform.forward * DistanceY) + Target.position;
                    MainCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, Position, 10 * Time.deltaTime);

                    float CheckDistCamera = Vector3.Distance(MainCamera.transform.position, Position);

                    if (CheckDist < 0.1f && CheckDistCamera < 0.1f)
                        StartLerp = false;
                }
                #endregion
            }
            #endregion
        }
        #endregion
    }

}
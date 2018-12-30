﻿using UnityEngine;

/// <summary>
/// ISRTS Camera
/// Created by SPINACH.
/// 
/// © 2013 - 2016 SPINACH All rights reserved.
/// </summary>
namespace CYM.Cam
{
    public class RTSCamera2D : MonoBehaviour
    {

        public float scrollValue;
        public bool unlockWhenMove;

        public Range zoomRange;

        public float movementLerpSpeed;

        public Rect bound;
        public Transform followingTarget;

        public float desktopScrollSpeed;
        public float desktopMoveSpeed;

        public float touchMoveSpeed;
        public float touchScrollSpeed;

        public string horizontalKeyboardAxis;
        public string verticalKeyboardAxis;

        public Vector2 objectPos;

        private Transform selfT;
        private Camera selfC;

        public bool keyBoardControl;
        public bool mouseControl;
        public bool touchControl;
        public bool allowFollow;

        static private RTSCamera2D self;

        #region runtime_control_switch
        public void KeyboardControl(bool enable)
        {
            keyBoardControl = enable;
        }

        public void MouseControl(bool enable)
        {

            if (Application.isMobilePlatform) return;

            mouseControl = enable;
        }

        public void TouchControl(bool enable)
        {
            touchControl = enable;
        }
        #endregion

        #region static_methods
        static public RTSCamera2D GetInstantiated() { return self; }

        static public void JumpToTargetForMain(Transform target)
        {
            self.objectPos.x = target.position.x;
            self.objectPos.y = target.position.y;
        }

        static public void FollowForMain(Transform target)
        {
            if (self.allowFollow)
            {
                self.followingTarget = target;
            }
        }

        static public void CancelFollowForMain()
        {
            self.followingTarget = null;
        }

        static public Transform GetFollowingTarget() { return self.followingTarget; }
        #endregion

        public void Follow(Transform target)
        {
            if (self.allowFollow)
            {
                self.followingTarget = target;
            }
        }

        public void CancelFollow()
        {
            self.followingTarget = null;
        }

        public Vector3 CalculateCurrentObjectPosition()
        {
            return transform.position;
        }

        /// <summary>
        /// Adjust to the attitude base on current setting.
        /// Editor use this method to generate preview.
        /// </summary>
        public void Adjust2AttitudeBaseOnCurrentSetting()
        {
            objectPos = CalculateCurrentObjectPosition();
            scrollValue = Mathf.Clamp01(scrollValue);

            GetComponent<Camera>().orthographicSize = zoomRange.min + (scrollValue * zoomRange.length);
        }

        void Awake()
        {
            self = this;
            selfT = transform;
            selfC = GetComponent<Camera>();
        }

        public void Start()
        {
            objectPos = CalculateCurrentObjectPosition();
            scrollValue = Mathf.Clamp01(scrollValue);

            KeyboardControl(keyBoardControl);
            MouseControl(mouseControl);
            TouchControl(touchControl);

        }

        private void Update()
        {
            //if (keyBoardControl)
            //{
            //    Move(Input.GetAxisRaw(horizontalKeyboardAxis) * Vector2.right, desktopMoveSpeed * Time.deltaTime);
            //    Move(Input.GetAxisRaw(verticalKeyboardAxis) * Vector2.up, desktopMoveSpeed * Time.deltaTime);
            //}

            if (mouseControl)
            {
                if (Input.mousePosition.y >= Screen.height) Move(Vector2.up, desktopMoveSpeed * Time.deltaTime);
                if (Input.mousePosition.y <= 0) Move(-Vector2.up, desktopMoveSpeed * Time.deltaTime);
                if (Input.mousePosition.x <= 0) Move(-Vector2.right, desktopMoveSpeed * Time.deltaTime);
                if (Input.mousePosition.x >= Screen.width) Move(Vector2.right, desktopMoveSpeed * Time.deltaTime);
                Scroll(Input.GetAxis("Mouse ScrollWheel") * desktopScrollSpeed);
            }

            UpdateTransform();
        }

        public void Move(Vector2 dir, float offset)
        {
            dir *= offset;
            if (unlockWhenMove && dir != Vector2.zero)
            {
                followingTarget = null;
            }
            objectPos += dir;

            objectPos.x = Mathf.Clamp(objectPos.x, bound.xMin, bound.xMax);
            objectPos.y = Mathf.Clamp(objectPos.y, bound.yMin, bound.yMax);
        }

        public void Scroll(float value)
        {
            scrollValue += value;
            scrollValue = Mathf.Clamp01(scrollValue);
        }

        void UpdateTransform()
        {
            Vector3 cameraPos;

            if (followingTarget)
            {
                objectPos.x = followingTarget.position.x;
                objectPos.y = followingTarget.position.y;
            }

            float wantedSize = zoomRange.min + (scrollValue * zoomRange.length);
            selfC.orthographicSize = Mathf.Lerp(selfC.orthographicSize, wantedSize, movementLerpSpeed * Time.deltaTime);

            cameraPos = objectPos;
            cameraPos.z = -1;
            selfT.position = Vector3.Lerp(selfT.position, cameraPos, movementLerpSpeed * Time.deltaTime);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawLine(new Vector3(bound.xMin, bound.yMin, 0), new Vector3(bound.xMin, bound.yMax, 0));
            Gizmos.DrawLine(new Vector3(bound.xMin, bound.yMax, 0), new Vector3(bound.xMax, bound.yMax, 0));
            Gizmos.DrawLine(new Vector3(bound.xMax, bound.yMax, 0), new Vector3(bound.xMax, bound.yMin, 0));
            Gizmos.DrawLine(new Vector3(bound.xMax, bound.yMin, 0), new Vector3(bound.xMin, bound.yMin, 0));
        }
    }
}
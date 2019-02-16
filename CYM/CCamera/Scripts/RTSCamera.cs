using UnityEngine;
using CYM;
/// <summary>
/// ISRTS Camera
/// Created by SPINACH.
/// 
/// © 2013 - 2016 SPINACH All rights reserved.
/// </summary>

public enum MouseButton
{
    Left, Right, Middle
}
namespace CYM.Cam
{

    public class RTSCamera : MonoBehaviour
    {

        public float DesktopMoveDragSpeed { get; set; } = 10.0f;
        public float DesktopMoveSpeed { get;  set; } = 300;
        public float DesktopKeyMoveSpeed { get; set; } = 2;
        public float DesktopScrollSpeed { get; set; } = 1;

        public CamScrollAnimationType scrollAnimationType;
        public AnimationCurve scrollXAngle = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve scrollHigh = AnimationCurve.Linear(0, 0, 1, 1);
        public float groundHigh;
        public float minHigh;
        public bool groundHighTest;
        public LayerMask groundMask;

        public float scrollValue;
        public bool unlockWhenMove;

        public float movementLerpSpeed;
        public float rotationLerpSpeed;

        public Rect bound;
        public Transform followingTarget;
        public Transform fixedPoint;

        public float desktopScrollSpeed;
        public float desktopMoveSpeed;
        public float desktopMoveDragSpeed;
        public float desktopRotateSpeed;

        public float touchMoveSpeed;
        public float touchScrollSpeed;
        public float touchRotateSpeed;

        public string horizontalKeyboardAxis;
        public string verticalKeyboardAxis;
        public string rotateAxis;

        public int mouseDragButton;
        public int mouseRotateButton;

        public Vector3 objectPos;

        private Transform selfT;

        public float wantYAngle;
        public float wantXAngle;

        public bool mouseRotateControl;
        public bool screenEdgeMovementControl;
        public bool mouseDragControl;
        public bool keyBoardControl;
        public bool mouseScrollControl;
        public bool allowFollow;

        public BoolState ControlDisabled { get; private set; } = new BoolState();

        static private RTSCamera self;

        #region runtime_control_switch
        public void ScreenEdgeMovementControl(bool enable)
        {

            if (Application.isMobilePlatform) return;
            screenEdgeMovementControl = enable;
        }

        public void MouseDragControl(bool enable)
        {

            if (Application.isMobilePlatform) return;
            mouseDragControl = enable;
        }

        public void MouseScrollControl(bool enable)
        {

            if (Application.isMobilePlatform) return;
            mouseScrollControl = enable;
        }
        #endregion

        #region static_methods
        public void LockFixedPointForMain(Transform pos)
        {
            if (self.allowFollow)
            {
                self.followingTarget = null;
                self.fixedPoint = pos;

                //Set the wantPos to make camera more smooth when leave fixed point.
                self.objectPos.x = pos.position.x;
                self.objectPos.z = pos.position.z;
            }
        }

        public void UnlockFixedPointForMain()
        {
            self.fixedPoint = null;
        }

        public void JumpToTargetForMain(Transform target)
        {
            self.objectPos.x = target.position.x;
            self.objectPos.z = target.position.z;
        }

        public void FollowForMain(Transform target)
        {
            if (self.allowFollow)
            {
                self.fixedPoint = null;
                self.followingTarget = target;
            }
        }

        public void CancelFollowForMain()
        {
            self.followingTarget = null;
        }

        public Transform GetFollowingTarget() { return self.followingTarget; }
        public Transform GetFixedPoint() { return self.fixedPoint; }
        #endregion

        public void LockFixedPoint(Transform pos)
        {
            if (self.allowFollow)
            {
                self.followingTarget = null;
                self.fixedPoint = pos;
            }
        }

        public void UnlockFixedPoint()
        {
            self.fixedPoint = null;
        }

        public void Follow(Transform target)
        {
            if (self.allowFollow)
            {
                self.fixedPoint = null;
                self.followingTarget = target;
            }
        }

        public void CancelFollow()
        {
            self.followingTarget = null;
        }

        public Vector3 CalculateCurrentObjectPosition()
        {

            float dist = objectPos.y * Mathf.Tan((90f - wantXAngle) * Mathf.Deg2Rad);

            Vector3 objectPosDir = -(transform.rotation * (-Vector3.forward * dist));
            return transform.position + objectPosDir;
        }

        void Awake()
        {
            self = this;
            selfT = transform;
        }

        public void Start()
        {
            DesktopMoveDragSpeed = desktopMoveDragSpeed;
            DesktopMoveSpeed = desktopMoveSpeed;

            objectPos = CalculateCurrentObjectPosition();
            scrollValue = Mathf.Clamp01(scrollValue);
            objectPos.y = scrollHigh.Evaluate(scrollValue);
            wantXAngle = scrollXAngle.Evaluate(scrollValue);

            Vector3 rot = selfT.eulerAngles;
            rot.x = BaseMathUtils.WrapAngle(rot.x);
            rot.y = BaseMathUtils.WrapAngle(rot.y);
            wantYAngle = rot.y;
            rot.x = scrollXAngle.Evaluate(scrollValue);
            wantXAngle = rot.x;
            selfT.eulerAngles = rot;

            ScreenEdgeMovementControl(screenEdgeMovementControl);
            MouseDragControl(mouseDragControl);
            MouseScrollControl(mouseScrollControl);
        }

        private void Update()
        {
            //if (keyBoardControl)
            //{
            //    Move(Input.GetAxisRaw(horizontalKeyboardAxis) * selfT.right, desktopMoveSpeed * Time.deltaTime);
            //    Move(Input.GetAxisRaw(verticalKeyboardAxis) * selfT.forward, desktopMoveSpeed * Time.deltaTime);
            //    Rotate(Input.GetAxisRaw(rotateAxis) * desktopMoveSpeed * Time.deltaTime);
            //}

            if (screenEdgeMovementControl)
            {
                if (Input.mousePosition.y >= Screen.height - 1f) _Move(selfT.forward, DesktopMoveSpeed * Time.deltaTime);
                if (Input.mousePosition.y <= 1) _Move(-selfT.forward, DesktopMoveSpeed * Time.deltaTime);
                if (Input.mousePosition.x <= 1) _Move(-selfT.right, DesktopMoveSpeed * Time.deltaTime);
                if (Input.mousePosition.x >= Screen.width - 1f) _Move(selfT.right, DesktopMoveSpeed * Time.deltaTime);
            }

            if (mouseScrollControl)
            {
                Scroll(Input.GetAxis("Mouse ScrollWheel") * -DesktopScrollSpeed);
            }

            if (mouseDragControl)
            {
                //if (Input.GetMouseButton(mouseRotateButton))
                //{
                //    Rotate(Input.GetAxis("Mouse X") * desktopRotateSpeed);
                //}

                if (Input.GetMouseButton(mouseDragButton))
                {
                    float mouseX = Input.GetAxis("Mouse X") / Screen.width * 10000f;
                    float mouseY = Input.GetAxis("Mouse Y") / Screen.height * 10000f;

                    Vector3 modifyRight = -Vector3.right;
                    //modifyRight = -selfT.right
                    _NormalizedMove(modifyRight * mouseX, DesktopMoveDragSpeed * Time.deltaTime);
                    Vector3 modifyForward = Vector3.forward;
                    //modifyForward = -(selfT.forward + selfT.up * 0.5f).normalized ;
                    _NormalizedMove(-modifyForward * mouseY, DesktopMoveDragSpeed * Time.deltaTime);
                }
            }

            UpdateTransform();
        }

        public void Move(Vector3 dir)
        {
            _Move(dir, DesktopMoveSpeed * Time.deltaTime);
        }

        void _Move(Vector3 dir, float offset)
        {
            if (ControlDisabled.IsIn()) return;

            dir.y = 0;
            dir.Normalize();
            dir *= offset;
            if (unlockWhenMove && dir != Vector3.zero)
            {
                followingTarget = null;
                fixedPoint = null;
            }
            objectPos += dir;

            objectPos.x = Mathf.Clamp(objectPos.x, bound.xMin, bound.xMax);
            objectPos.z = Mathf.Clamp(objectPos.z, bound.yMin, bound.yMax);
        }

        void _NormalizedMove(Vector3 dir, float offset)
        {
            if (ControlDisabled.IsIn()) return;

            dir.y = 0;
            dir *= offset;
            if (unlockWhenMove && dir != Vector3.zero)
            {
                followingTarget = null;
                fixedPoint = null;
            }
            objectPos += dir;

            objectPos.x = Mathf.Clamp(objectPos.x, bound.xMin, bound.xMax);
            objectPos.z = Mathf.Clamp(objectPos.z, bound.yMin, bound.yMax);
        }

        public void Rotate(float dir)
        {
            if (ControlDisabled.IsIn()) return;

            wantYAngle += dir;
            BaseMathUtils.WrapAngle(wantYAngle);
        }

        public void Scroll(float value)
        {
            if (ControlDisabled.IsIn()) return;

            scrollValue += value;
            scrollValue = Mathf.Clamp01(scrollValue);
            objectPos.y = scrollHigh.Evaluate(scrollValue);
            wantXAngle = scrollXAngle.Evaluate(scrollValue);
        }

        void UpdateTransform()
        {
            Vector3 cameraPosDir;
            Vector3 cameraPos;

            if (!fixedPoint)
            {
                float currentGroundHigh = groundHigh;

                //Set wanted position to target's position if we are following something.
                if (followingTarget)
                {
                    objectPos.x = followingTarget.position.x;
                    objectPos.z = followingTarget.position.z;
                }

                //Calculate vertical distance to ground to avoid intercepting ground.
                RaycastHit hit;
                Vector3 emitPos = objectPos;
                emitPos.y += 9999f;
                if (groundHighTest && Physics.Raycast(emitPos, -Vector3.up, out hit, Mathf.Infinity, groundMask))
                {
                    currentGroundHigh = hit.point.y;
                }

                emitPos = selfT.position;
                emitPos.y += 9999f;
                if (groundHighTest && Physics.Raycast(emitPos, -Vector3.up, out hit, Mathf.Infinity, groundMask))
                {
                    currentGroundHigh = Mathf.Max(currentGroundHigh, hit.point.y);
                }

                //Lerp actual rotation to wanted value.
                Quaternion targetRot = Quaternion.Euler(wantXAngle, wantYAngle, 0f);
                selfT.rotation = Quaternion.Slerp(selfT.rotation, targetRot, rotationLerpSpeed * Time.deltaTime);

                //Calculate a world position refers to the center of screen.
                float dist = objectPos.y * Mathf.Tan((90f - wantXAngle) * Mathf.Deg2Rad);

                //Use this vector to move camera back and rotate.
                Quaternion targetYRot = Quaternion.Euler(0f, wantYAngle, 0f);
                cameraPosDir = targetYRot * (Vector3.forward * dist);

                //Calculate the actual world position to prepare to move our camera object.
                cameraPos = objectPos - cameraPosDir;
                cameraPos.y = (objectPos.y + (followingTarget ? followingTarget.position.y : currentGroundHigh));

                //Lerp to wanted position.
                selfT.position = Vector3.Lerp(selfT.position, cameraPos, movementLerpSpeed * Time.deltaTime);
            }
            else
            {
                //If we are positioning to a fixed point, we simply move to it.
                selfT.rotation = Quaternion.Slerp(selfT.rotation, fixedPoint.rotation, rotationLerpSpeed * Time.deltaTime);
                selfT.position = Vector3.Lerp(selfT.position, fixedPoint.position, movementLerpSpeed * Time.deltaTime);

                //We also keep objectPos to fixedPoint to make a stable feeling while leave fixed point mode.
                objectPos.x = fixedPoint.position.x;
                objectPos.z = fixedPoint.position.z;
            }
        }

        void OnDrawGizmosSelected()
        {

            //Draw debug lines.
            Vector3 mp = transform.position;
            Gizmos.DrawLine(new Vector3(bound.xMin, mp.y, bound.yMin), new Vector3(bound.xMin, mp.y, bound.yMax));
            Gizmos.DrawLine(new Vector3(bound.xMin, mp.y, bound.yMax), new Vector3(bound.xMax, mp.y, bound.yMax));
            Gizmos.DrawLine(new Vector3(bound.xMax, mp.y, bound.yMax), new Vector3(bound.xMax, mp.y, bound.yMin));
            Gizmos.DrawLine(new Vector3(bound.xMax, mp.y, bound.yMin), new Vector3(bound.xMin, mp.y, bound.yMin));
        }
    }
}

public enum CamScrollAnimationType
{
    Simple, Advanced
}
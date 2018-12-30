using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
namespace CYM
{
    [RequireComponent(typeof(KinematicCharacterMotor))]
    public class BaseCharaCtrl : BaseCharacterController
    {
        #region Callback Val
        public event Callback Callback_OnJumpLanded;
        public event Callback Callback_OnLanded;
        public event Callback Callback_OnLeaveStableGround;
        public event Callback Callback_OnJump;
        public event Callback Callback_OnDoubbleJump;
        public event Callback Callback_OnFallDown;
        public event Callback<Collider, Vector3, Vector3, bool> Callback_OnGroundHit;
        public event Callback<Collider, Vector3, Vector3, bool> Callback_OnMovementHit;
        #endregion

        public float Radius { get; protected set; }
        public float Height { get; protected set; }
        public float HalfHeight { get { return Height * 0.5f; } }

        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f; // Max speed when stable on ground
        public float StableMovementSharpness = 15; // Sharpness of the acceleration when stable on ground
        public float OrientationSharpness = 10; // Sharpness of rotations when stable on ground

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 10f; // Max speed for air movement
        public float AirAccelerationSpeed = 5f; // Acceleration when in air
        public float Drag = 0.1f; // Air drag

        [Header("Jumping")]
        public bool AllowJumpingWhenSliding = false; // Is jumping allowed when we are sliding down a surface, even if we are not "stable" on it?
        public bool AllowDoubleJump = false;
        public bool AllowWallJump = false;
        public float JumpSpeed = 10f; // Strength of the jump impulse
        public float JumpPreGroundingGraceTime = 0f; // Time before landing that jump inputs will be remembered and applied at the moment of landing
        public float JumpPostGroundingGraceTime = 0f; // Time after getting un-grounded that jumping will still be allowed

        [Header("Misc")]
        public bool OrientTowardsGravity = true; // Should the character align its up direction with the gravity (used for the planet example)
        //public Vector3 Gravity = new Vector3(0, -9.81f, 0); // Gravity vector
        //public Transform MeshRoot; // This is the transform that will be scaled down while crouching

        private bool _canWallJump = false;
        private Vector3 _wallJumpNormal;

        [HideInInspector]
        public Collider[] IgnoredColliders = new Collider[] { };
        //忽略碰撞的层级
        int IgnoredLayer = 0;

        /// <summary>
        /// 是否在坠落
        /// </summary>\
        [HideInInspector]
        public bool IsFallDown = false;
        /// <summary>
        /// 上一次是否跳跃起
        /// </summary>
        [HideInInspector]
        public bool IsJumped = false;
        /// <summary>
        /// 上一次是否双联跳跃
        /// </summary>
        [HideInInspector]
        public bool IsDoubbleJumped = false;

        private Collider[] _probedColliders = new Collider[8];
        private Vector3 _moveInputVector = Vector3.zero;
        private Vector3 _lookInputVector = Vector3.zero;
        private Vector3 _smoothedLookInputDirection = Vector3.zero;
        private Vector3 _internalVelocityAdd = Vector3.zero;
        private bool _jumpRequested = false;
        private bool _jumpConsumed = false;
        private bool _doubleJumpConsumed = false;
        private bool _jumpedThisFrame = false;
        private Vector3 _jumpDirection = Vector3.up;
        private float _timeSinceJumpRequested = Mathf.Infinity;
        private float _timeSinceLastAbleToJump = 0f;
        private bool _isTryingToUncrouch = false;
        private Animator Animator;
        private GameObject GO;
        // RootMotion deltas
        public Vector3 RootMotionPositionDelta { get; private set; }
        public Quaternion RootMotionRotationDelta { get; private set; }

        private void OnEnable()
        {
            GO = gameObject;
            Animator = GO.GetComponentInChildren<Animator>();
            RootMotionPositionDelta = Vector3.zero;
            RootMotionRotationDelta = Quaternion.identity;
             _moveInputVector = Vector3.zero;
            //_lookInputVector = Vector3.zero;
        }
        private void Start()
        {

        }
        public void Move(Vector3 moveInput)
        {
            _moveInputVector = Vector3.ProjectOnPlane(moveInput, Motor.CharacterUp).normalized * moveInput.magnitude;
        }

        public void Look(Vector3 lookInput)
        {
            _lookInputVector = Vector3.ProjectOnPlane(lookInput, Motor.CharacterUp).normalized;
        }


        public override void BeforeCharacterUpdate(float deltaTime)
        {
        }

        public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (_lookInputVector != Vector3.zero && OrientationSharpness > 0f)
            {
                _smoothedLookInputDirection = Vector3.Slerp(_smoothedLookInputDirection, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;
                currentRotation = Quaternion.LookRotation(_smoothedLookInputDirection, Motor.CharacterUp);
            }
            if (OrientTowardsGravity)
            {
                currentRotation = Quaternion.FromToRotation((currentRotation * Vector3.up), -Physics.gravity) * currentRotation;
            }

            RootMotion_UpdateRotation(ref currentRotation,  deltaTime);
        }

        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            Vector3 targetMovementVelocity = Vector3.zero;
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                // Reorient velocity on slope
                currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

                // Calculate target velocity
                Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

                // Independant movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
                IsFallDown = false;
            }
            else
            {
                // Add move input
                if (_moveInputVector.sqrMagnitude > 0f)
                {
                    targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;
                    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Physics.gravity);
                    currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
                }

                Vector3 preVelocity = currentVelocity;
                // Gravity
                currentVelocity += Physics.gravity * deltaTime;
                // Drag
                currentVelocity *= (1f / (1f + (Drag * deltaTime)));

                //检测坠落
                if (currentVelocity.y < 0.0f)
                {
                    if (preVelocity.y >= 0.0f)
                    {
                        Callback_OnFallDown?.Invoke();
                    }
                    IsFallDown = true;
                }
                else
                {
                    IsFallDown = false;
                }
            }

            // Handle jumping
            _jumpedThisFrame = false;
            _timeSinceJumpRequested += deltaTime;
            if (_jumpRequested)
            {
                // Handle double jump
                if (AllowDoubleJump)
                {
                    if (_jumpConsumed && !_doubleJumpConsumed && (AllowJumpingWhenSliding ? !Motor.GroundingStatus.FoundAnyGround : !Motor.GroundingStatus.IsStableOnGround))
                    {
                        Motor.ForceUnground();

                        // Add to the return velocity and reset jump state
                        currentVelocity += (Motor.CharacterUp * JumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                        _jumpRequested = false;
                        _doubleJumpConsumed = true;
                        _jumpedThisFrame = true;

                        IsDoubbleJumped = true;
                        Callback_OnDoubbleJump?.Invoke();
                    }
                }


                // See if we actually are allowed to jump
                if (_canWallJump ||
                    !_jumpConsumed && ((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) || (JumpPostGroundingGraceTime > 0 && _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime)))
                {
                    // Calculate jump direction before ungrounding
                    Vector3 jumpDirection = Motor.CharacterUp;
                    if (_canWallJump)
                    {
                        jumpDirection = _wallJumpNormal;
                    }
                    else if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
                    {
                        jumpDirection = Motor.GroundingStatus.GroundNormal;
                    }

                    // Makes the character skip ground probing/snapping on its next update. 
                    // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                    Motor.ForceUnground();

                    // Add to the return velocity and reset jump state
                    currentVelocity += (jumpDirection * JumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                    _jumpRequested = false;
                    _jumpConsumed = true;
                    _jumpedThisFrame = true;

                    IsJumped = true;
                    Callback_OnJump?.Invoke();
                }
            }
            // Reset wall jump
            _canWallJump = false;

            // Take into account additive velocity
            if (_internalVelocityAdd.sqrMagnitude > 0f)
            {
                currentVelocity += _internalVelocityAdd;
                _internalVelocityAdd = Vector3.zero;
            }

            RootMotion_UpdateVelocity(ref currentVelocity,  deltaTime);
        }

        public override void AfterCharacterUpdate(float deltaTime)
        {
            // Handle jump-related values
            {
                // Handle jumping pre-ground grace period
                if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
                {
                    _jumpRequested = false;
                }

                if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
                {
                    // If we're on a ground surface, reset jumping values
                    if (!_jumpedThisFrame)
                    {
                        _doubleJumpConsumed = false;
                        _jumpConsumed = false;
                    }
                    _timeSinceLastAbleToJump = 0f;
                }
                else
                {
                    // Keep track of time since we were last able to jump (for grace period)
                    _timeSinceLastAbleToJump += deltaTime;
                }
            }

            // Grounding considerations
            if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
            {
                if(!IsPreFrameMustUnground)
                    Callback_OnLanded?.Invoke();
                IsPreFrameMustUnground = false;
                if (IsJumped)
                {
                    Callback_OnJumpLanded?.Invoke();
                }
                IsJumped = false;
                IsDoubbleJumped = false;
            }
            else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
            {
                Callback_OnLeaveStableGround?.Invoke();
            }

            if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
            {
                //_jumpConsumed = false;
                _timeSinceLastAbleToJump = 0f;
            }
            else
            {
                _timeSinceLastAbleToJump += deltaTime;
            }

            // Handle uncrouching
            if (_isTryingToUncrouch)
            {
                // Do an overlap test with the character's standing height to see if there are any obstructions
                SetDimensions(Radius, Height);
                if (Motor.CharacterOverlap(
                    Motor.TransientPosition,
                    Motor.TransientRotation,
                    _probedColliders,
                    Motor.CollidableLayers,
                    QueryTriggerInteraction.Ignore) > 0)
                {
                    // If obstructions, just stick to crouching dimensions
                    SetDimensions(Radius, HalfHeight);
                }
                else
                {
                    // If no obstructions, uncrouch
                    //MeshRoot.localScale = new Vector3(1f, 1f, 1f);

                    _isTryingToUncrouch = false;
                }
            }

            // Reset root motion deltas
            RootMotionPositionDelta = Vector3.zero;
            RootMotionRotationDelta = Quaternion.identity;
        }

        public override bool IsColliderValidForCollisions(Collider coll)
        {
            // Example of ignoring collisions with specific colliders
            for (int i = 0; i < IgnoredColliders.Length; i++)
            {
                if (coll == IgnoredColliders[i])
                {
                    return false;
                }
            }


            return true;
        }

        public override void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
           
        }

        public override void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            Callback_OnGroundHit?.Invoke(hitCollider,  hitNormal,  hitPoint,  true);
        }

        public override void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            Callback_OnMovementHit?.Invoke(hitCollider, hitNormal, hitPoint, true);

            // We can wall jump only if we are not stable on ground and are moving against an obstruction
            if (AllowWallJump && !Motor.GroundingStatus.IsStableOnGround && !hitStabilityReport.IsStable)//
            {
                _canWallJump = true;
                _wallJumpNormal = Vector3.up + hitNormal;
            }
        }

        public bool IsPreFrameMustUnground = false;
        public void AddVelocity(Vector3 velocity)
        {
            IsPreFrameMustUnground = true;
            Motor.ForceUnground();
            _internalVelocityAdd += velocity;
        }

        public void Teleport(Vector3 pos)
        {
            Motor.SetPosition(pos);
        }

        public void Jump()
        {
            _timeSinceJumpRequested = 0f;
            _jumpRequested = true;
        }

        public void Crouch(bool crouch)
        {
            if (crouch)
            {
                //float height = Motor.CapsuleHeight * 0.5f;
                //Motor.SetCapsuleDimensions(Motor.CapsuleRadius, height, height*0.5f);
                SetDimensions(Radius, HalfHeight);
                //MeshRoot.localScale = new Vector3(1f, 0.5f, 1f);
            }
            else
            {
                _isTryingToUncrouch = true;
            }
        }

        public void Creeping(bool crouch)
        {
            if (crouch)
            {
                SetDimensions(Radius, 0.3f);
                //MeshRoot.localScale = new Vector3(1f, 0.5f, 1f);
            }
            else
            {
                _isTryingToUncrouch = true;
            }
        }

        public void InitDimensions(float radius, float height)
        {
            Radius = radius;
            Height = height;
            RevertDimensions();
        }
        public void SetDimensions(float radius, float height)
        {
            Motor.SetCapsuleDimensions(radius, height, height*0.5f);
        }
        public void RevertDimensions()
        {
            SetDimensions(Radius,Height);
        }

        /// <summary>
        /// 操作主角对于其他人的碰撞层级
        /// e.x. 设置层级Chara 为 false,则此角色对其他所有的Char层级的对象都会忽略碰撞,但是其他角色依然会对此角色产生碰撞
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="b"></param>
        public void SetCollision(LayerData layer,bool b)
        {
            var val = (1 << (int)layer);
            if (b)
                Motor.CollidableLayers |= val;
            else
            {
                if((Motor.CollidableLayers & val) > 0)
                    Motor.CollidableLayers ^= val;
            }
        }
        /// <summary>
        /// 操作:主角对于其他人的碰撞层级
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="b"></param>
        public void RevertCollision()
        {
            Motor.CollidableLayers = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(this.gameObject.layer, i))
                {
                    Motor.CollidableLayers |= (1 << i);
                }
            }

        }

        private void OnAnimatorMove()
        {
            if (Animator && Animator.applyRootMotion)
            {
                // Accumulate rootMotion deltas between character updates 
                RootMotionPositionDelta += Animator.deltaPosition;
                RootMotionRotationDelta = Animator.deltaRotation * RootMotionRotationDelta;
            }
        }


        #region rootMotion
        public void RootMotion_UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (RootMotionRotationDelta != Quaternion.identity)
            {
                currentRotation = RootMotionRotationDelta * currentRotation;
            }
        }

        public void RootMotion_UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (RootMotionPositionDelta != Vector3.zero)
            {
                if (Motor.GroundingStatus.IsStableOnGround)
                {
                    if (deltaTime > 0)
                    {
                        // The final velocity is the velocity from root motion reoriented on the ground plane
                        currentVelocity = RootMotionPositionDelta / deltaTime;
                        currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
                    }
                    else
                    {
                        // Prevent division by zero
                        currentVelocity = Vector3.zero;
                    }
                }
                else
                {
                    //if (ForwardAxis > 0f)
                    //{
                    //    // If we want to move, add an acceleration to the velocity
                    //    Vector3 targetMovementVelocity = Motor.CharacterForward * ForwardAxis * MaxAirMoveSpeed;
                    //    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                    //    currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
                    //}

                    //// Gravity
                    //currentVelocity += Gravity * deltaTime;

                    // Drag
                    currentVelocity *= (1f / (1f + (Drag * deltaTime)));
                }
            }
        }
        #endregion

        /// <summary>
        /// 角色死亡的时候必须禁用 否则会影响对象池二次生成
        /// </summary>
        /// <param name="b"></param>
        public void EnableCtrl(bool b)
        {
            if(GO!=null)enabled = b;
            if (Motor == null) return;
            Motor.enabled = b;
            Motor.Capsule.enabled = b;
        }
        /// <summary>
        /// 打开 关闭 碰撞
        /// </summary>
        /// <param name="b"></param>
        public void EnableCollider(bool b)
        {
            Motor.Capsule.enabled = b;
        }

        public override void PostGroundingUpdate(float deltaTime)
        {
            //throw new System.NotImplementedException();
        }
    }
}
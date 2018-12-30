using UnityEngine;

namespace KinematicCharacterController
{
    public enum RigidbodyInteractionType
    {
        None,
        Kinematic,
        SimulatedDynamic
    }

    public enum StepHandlingMethod
    {
        None,
        Standard,
        Extra
    }

    public enum MovementSweepState
    {
        Initial,
        AfterFirstHit,
        FoundBlockingCrease,
        FoundBlockingCorner,
    }

    /// <summary>
    /// Represents the entire state of a character motor that is pertinent for simulation.
    /// Use this to save state or revert to past state
    /// </summary>
    [System.Serializable]
    public struct KinematicCharacterMotorState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 BaseVelocity;

        public bool MustUnground;
        public bool LastMovementIterationFoundAnyGround;
        public CharacterTransientGroundingReport GroundingStatus;

        public Rigidbody AttachedRigidbody; 
        public Vector3 AttachedRigidbodyVelocity; 
    }

    /// <summary>
    /// Describes an overlap between the character capsule and another collider
    /// </summary>
    public struct OverlapResult
    {
        public Vector3 Normal;
        public Collider Collider;

        public OverlapResult(Vector3 normal, Collider collider)
        {
            Normal = normal;
            Collider = collider;
        }
    }

    /// <summary>
    /// Contains all the information for the motor's grounding status
    /// </summary>
    public struct CharacterGroundingReport
    {
        public bool FoundAnyGround;
        public bool IsStableOnGround;
        public bool SnappingPrevented;
        public Vector3 GroundNormal;
        public Vector3 InnerGroundNormal;
        public Vector3 OuterGroundNormal;

        public Collider GroundCollider;
        public Vector3 GroundPoint;

        public void CopyFrom(CharacterTransientGroundingReport transientGroundingReport)
        {
            FoundAnyGround = transientGroundingReport.FoundAnyGround;
            IsStableOnGround = transientGroundingReport.IsStableOnGround;
            SnappingPrevented = transientGroundingReport.SnappingPrevented;
            GroundNormal = transientGroundingReport.GroundNormal;
            InnerGroundNormal = transientGroundingReport.InnerGroundNormal;
            OuterGroundNormal = transientGroundingReport.OuterGroundNormal;

            GroundCollider = null;
            GroundPoint = Vector3.zero;
        }
    }

    /// <summary>
    /// Contains the simulation-relevant information for the motor's grounding status
    /// </summary>
    public struct CharacterTransientGroundingReport
    {
        public bool FoundAnyGround;
        public bool IsStableOnGround;
        public bool SnappingPrevented;
        public Vector3 GroundNormal; 
        public Vector3 InnerGroundNormal;
        public Vector3 OuterGroundNormal;

        public void CopyFrom(CharacterGroundingReport groundingReport)
        {
            FoundAnyGround = groundingReport.FoundAnyGround;
            IsStableOnGround = groundingReport.IsStableOnGround;
            SnappingPrevented = groundingReport.SnappingPrevented;
            GroundNormal = groundingReport.GroundNormal;
            InnerGroundNormal = groundingReport.InnerGroundNormal;
            OuterGroundNormal = groundingReport.OuterGroundNormal;
        }
    }

    /// <summary>
    /// Contains all the information from a hit stability evaluation
    /// </summary>
    public struct HitStabilityReport
    {
        public bool IsStable;

        public Vector3 InnerNormal;
        public Vector3 OuterNormal;

        public bool ValidStepDetected;
        public Collider SteppedCollider;

        public bool LedgeDetected;
        public bool IsOnEmptySideOfLedge;
        public float DistanceFromLedge;
        public Vector3 LedgeGroundNormal;
        public Vector3 LedgeRightDirection;
        public Vector3 LedgeFacingDirection;
    }

    /// <summary>
    /// Contains the information of hit rigidbodies during the movement phase, so they can be processed afterwards
    /// </summary>
    public struct RigidbodyProjectionHit
    {
        public Rigidbody Rigidbody;
        public Vector3 HitPoint;
        public Vector3 EffectiveHitNormal;
        public Vector3 HitVelocity;
        public bool StableOnHit;
    }
    
    /// <summary>
    /// Component that manages character collisions and movement solving
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class KinematicCharacterMotor : MonoBehaviour
    {
#pragma warning disable 0414
        /// <summary>
        /// The BaseCharacterController that manages this motor
        /// </summary>
        [Header("Components")]
        public BaseCharacterController CharacterController;
        /// <summary>
        /// The capsule collider of this motor
        /// </summary>
        [ReadOnly]
        public CapsuleCollider Capsule;
        /// <summary>
        /// The rigidbody of this motor
        /// </summary>
        [ReadOnly]
        public Rigidbody Rigidbody;

        [Header("Capsule Settings")]
        /// <summary>
        /// Radius of the character's capsule
        /// </summary>
        [SerializeField]
        [Tooltip("Radius of the Character Capsule")]
        private float CapsuleRadius = 0.5f;
        /// <summary>
        /// Height of the character's capsule
        /// </summary>
        [SerializeField]
        [Tooltip("Height of the Character Capsule")]
        private float CapsuleHeight = 2f;
        /// <summary>
        /// Local y position of the character's capsule center
        /// </summary>
        [SerializeField]
        [Tooltip("Height of the Character Capsule")]
        private float CapsuleYOffset = 1f;
        /// <summary>
        /// Physics material of the character's capsule
        /// </summary>
        [SerializeField]
        [Tooltip("Physics material of the Character Capsule (Does not affect character movement. Only affects things colliding with it)")]
        private PhysicMaterial CapsulePhysicsMaterial;

        [Header("Misc Options")]

        /// <summary>
        /// Notifies the Character Controller when discrete collisions are detected
        /// </summary>    
        [Tooltip("Notifies the Character Controller when discrete collisions are detected")]
        public bool DetectDiscreteCollisions = false;
        /// <summary>
        /// Increases the range of ground detection, to allow snapping to ground at very high speeds
        /// </summary>    
        [Tooltip("Increases the range of ground detection, to allow snapping to ground at very high speeds")]
        public float GroundDetectionExtraDistance = 0f;
        /// <summary>
        /// Maximum height of a step which the character can climb
        /// </summary>    
        [Tooltip("Maximum height of a step which the character can climb")]
        public float MaxStepHeight = 0.5f;
        /// <summary>
        /// Minimum length of a step that the character can step on (used in Extra stepping method. Use this to let the character step on steps that are smaller that its radius
        /// </summary>    
        [Tooltip("Minimum length of a step that the character can step on (used in Extra stepping method). Use this to let the character step on steps that are smaller that its radius")]
        public float MinRequiredStepDepth = 0.1f;
        /// <summary>
        /// Maximum slope angle on which the character can be stable
        /// </summary>    
        [Range(0f, 89f)]
        [Tooltip("Maximum slope angle on which the character can be stable")]
        public float MaxStableSlopeAngle = 60f;
        /// <summary>
        /// The distance from the capsule central axis at which the character can stand on a ledge and still be stable
        /// </summary>    
        [Tooltip("The distance from the capsule central axis at which the character can stand on a ledge and still be stable")]
        public float MaxStableDistanceFromLedge = 0.5f;
        /// <summary>
        /// Prevents snapping to ground on ledges. Set this to true if you want more determinism when launching off slopes
        /// </summary>    
        [Tooltip("Prevents snapping to ground on ledges. Set this to true if you want more determinism when launching off slopes")]
        public bool PreventSnappingOnLedges = false;
        /// <summary>
        /// The maximun downward slope angle change that the character can be subjected to and still be snapping to the ground
        /// </summary>    
        [Tooltip("The maximun downward slope angle change that the character can be subjected to and still be snapping to the ground")]
        [Range(1f, 180f)]
        public float MaxStableDenivelationAngle = 180f;

        [Header("Rigidbody interactions")]
        /// <summary>
        /// How the character interacts with non-kinematic rigidbodies. \"Kinematic\" mode means the character pushes the rigidbodies with infinite force (as a kinematic body would). \"SimulatedDynamic\" pushes the rigidbodies with a simulated mass value.
        /// </summary>
        [Tooltip("How the character interacts with non-kinematic rigidbodies. \"Kinematic\" mode means the character pushes the rigidbodies with infinite force (as a kinematic body would). \"SimulatedDynamic\" pushes the rigidbodies with a simulated mass value.")]
        public RigidbodyInteractionType RigidbodyInteractionType;
        /// <summary>
        /// Determines if the character preserves moving platform velocities when de-grounding from them
        /// </summary>
        [Tooltip("Determines if the character preserves moving platform velocities when de-grounding from them")]
        public bool PreserveAttachedRigidbodyMomentum = true;
        
        [Header("Constraints")]
        /// <summary>
        /// Determines if the character's movement uses the planar constraint
        /// </summary>
        [Tooltip("Determines if the character's movement uses the planar constraint")]
        public bool HasPlanarConstraint = false;
        /// <summary>
        /// Defines the plane that the character's movement is constrained on, if HasMovementConstraintPlane is active
        /// </summary>
        [Tooltip("Defines the plane that the character's movement is constrained on, if HasMovementConstraintPlane is active")]
        public Vector3 PlanarConstraintAxis = Vector3.forward;

        [Header("Features & Optimizations")]
        /// <summary>
        /// Handles properly detecting grounding status on steps, but has a performance cost.
        /// </summary>
        [Tooltip("Handles properly detecting grounding status on steps, but has a performance cost.")]
        public StepHandlingMethod StepHandling = StepHandlingMethod.Standard;
        /// <summary>
        /// Handles properly detecting ledge information and grounding status, but has a performance cost.
        /// </summary>
        [Tooltip("Handles properly detecting ledge information and grounding status, but has a performance cost.")]
        public bool LedgeHandling = true;
        /// <summary>
        /// Handles properly being pushed by and standing on PhysicsMovers or dynamic rigidbodies. Also handles pushing dynamic rigidbodies
        /// </summary>
        [Tooltip("Handles properly being pushed by and standing on PhysicsMovers or dynamic rigidbodies. Also handles pushing dynamic rigidbodies")]
        public bool InteractiveRigidbodyHandling = true;
        /// <summary>
        /// Makes sure the character cannot perform a move at all if it would be overlapping with any collidable objects at its destination. Useful for preventing \"tunneling\"
        /// </summary>
        [Tooltip("(We suggest leaving this off. This has a pretty heavy performance cost, and is not necessary unless you start seeing situations where a fast-moving character moves through colliders) Makes sure the character cannot perform a move at all if it would be overlapping with any collidable objects at its destination. Useful for preventing \"tunneling\". ")]
        public bool SafeMovement = true;

        /// <summary>
        /// Contains the current grounding information
        /// </summary>
        [System.NonSerialized]
        public CharacterGroundingReport GroundingStatus = new CharacterGroundingReport();
        /// <summary>
        /// Contains the previous grounding information
        /// </summary>
        [System.NonSerialized]
        public CharacterTransientGroundingReport LastGroundingStatus = new CharacterTransientGroundingReport();
        /// <summary>
        /// Specifies the LayerMask that the character's movement algorithm can detect collisions with. By default, this uses the rigidbody's layer's collision matrix
        /// </summary>
        [System.NonSerialized]
        public LayerMask CollidableLayers = -1;

        /// <summary>
        /// The Transform of the character motor
        /// </summary>
        public Transform Transform { get; private set; }
        /// <summary>
        /// The character's up direction (always up-to-date during the character update phase)
        /// </summary>
        public Vector3 CharacterUp { get; private set; }
        /// <summary>
        /// The character's forward direction (always up-to-date during the character update phase)
        /// </summary>
        public Vector3 CharacterForward { get; private set; }
        /// <summary>
        /// The character's right direction (always up-to-date during the character update phase)
        /// </summary>
        public Vector3 CharacterRight { get; private set; }
        /// <summary>
        /// The character's position before the movement calculations began
        /// </summary>
        public Vector3 InitialSimulationPosition { get; private set; }
        /// <summary>
        /// The character's rotation before the movement calculations began
        /// </summary>
        public Quaternion InitialSimulationRotation { get; private set; }
        /// <summary>
        /// Represents the Rigidbody to stay attached to
        /// </summary>
        public Rigidbody AttachedRigidbody { get; private set; }
        /// <summary>
        /// Vector3 from the character transform position to the capsule center
        /// </summary>
        public Vector3 CharacterTransformToCapsuleCenter { get; private set; }
        /// <summary>
        /// Vector3 from the character transform position to the capsule bottom
        /// </summary>
        public Vector3 CharacterTransformToCapsuleBottom { get; private set; }
        /// <summary>
        /// Vector3 from the character transform position to the capsule top
        /// </summary>
        public Vector3 CharacterTransformToCapsuleTop { get; private set; }
        /// <summary>
        /// Vector3 from the character transform position to the capsule bottom hemi center
        /// </summary>
        public Vector3 CharacterTransformToCapsuleBottomHemi { get; private set; }
        /// <summary>
        /// Vector3 from the character transform position to the capsule top hemi center
        /// </summary>
        public Vector3 CharacterTransformToCapsuleTopHemi { get; private set; }

        /// <summary>
        /// Is the motor trying to force unground?
        /// </summary>
        public bool MustUnground { get; set; }
        /// <summary>
        /// Did the motor's last swept collision detection find a ground?
        /// </summary>
        public bool LastMovementIterationFoundAnyGround { get; set; }
        /// <summary>
        /// Index of this motor in KinematicCharacterSystem arrays
        /// </summary>
        public int IndexInCharacterSystem { get; set; }
        /// <summary>
        /// Remembers initial position before all simulation are done
        /// </summary>
        public Vector3 InitialTickPosition { get; set; }
        /// <summary>
        /// Remembers initial rotation before all simulation are done
        /// </summary>
        public Quaternion InitialTickRotation { get; set; }
        /// <summary>
        /// Specifies a Rigidbody to stay attached to
        /// </summary>
        public Rigidbody AttachedRigidbodyOverride { get; set; }

        private RaycastHit[] _internalCharacterHits = new RaycastHit[MaxHitsBudget];
        private Collider[] _internalProbedColliders = new Collider[MaxCollisionBudget];
        private Rigidbody[] _rigidbodiesPushedThisMove = new Rigidbody[MaxCollisionBudget];
        private RigidbodyProjectionHit[] _internalRigidbodyProjectionHits = new RigidbodyProjectionHit[MaxMovementSweepIterations];
        private Rigidbody _lastAttachedRigidbody;
        private bool _solveMovementCollisions = true;
        private bool _solveGrounding = true;
        private bool _movePositionDirty = false;
        private Vector3 _movePositionTarget = Vector3.zero;
        private bool _moveRotationDirty = false;
        private Quaternion _moveRotationTarget = Quaternion.identity;
        private bool _lastSolvedOverlapNormalDirty = false;
        private Vector3 _lastSolvedOverlapNormal = Vector3.forward;
        private int _rigidbodiesPushedCount = 0;
        private int _rigidbodyProjectionHitCount = 0;
        private float _internalResultingMovementMagnitude = 0f;
        private Vector3 _internalResultingMovementDirection = Vector3.zero;
        private bool _isMovingFromAttachedRigidbody = false;
        private Vector3 _cachedWorldUp = Vector3.up;
        private Vector3 _cachedWorldForward = Vector3.forward;
        private Vector3 _cachedWorldRight = Vector3.right;
        private Vector3 _cachedZeroVector = Vector3.zero;

        private Vector3 _internalTransientPosition;
        /// <summary>
        /// The character's goal position in its movement calculations (always up-to-date during the character update phase)
        /// </summary>
        public Vector3 TransientPosition
        {
            get
            {
                return _internalTransientPosition;
            }
            private set
            {
                _internalTransientPosition = value;
            }
        }

        private Quaternion _internalTransientRotation;
        /// <summary>
        /// The character's goal rotation in its movement calculations (always up-to-date during the character update phase)
        /// </summary>
        public Quaternion TransientRotation
        {
            get
            {
                return _internalTransientRotation;
            }
            private set
            {
                _internalTransientRotation = value;
                CharacterUp = _internalTransientRotation * _cachedWorldUp;
                CharacterForward = _internalTransientRotation * _cachedWorldForward;
                CharacterRight = _internalTransientRotation * _cachedWorldRight;
            }
        }

        /// <summary>
        /// The character's interpolated position
        /// </summary>
        public Vector3 InterpolatedPosition
        {
            get
            {
                return Transform.position;
            }
        }

        /// <summary>
        /// The character's interpolated rotation
        /// </summary>
        public Quaternion InterpolatedRotation
        {
            get
            {
                return Transform.rotation;
            }
        }

        /// <summary>
        /// The character's total velocity, including velocity from standing on rigidbodies or PhysicsMover
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                return _baseVelocity + _attachedRigidbodyVelocity;
            }
        }

        private Vector3 _baseVelocity;
        /// <summary>
        /// The character's velocity resulting from direct movement
        /// </summary>
        public Vector3 BaseVelocity
        {
            get
            {
                return _baseVelocity;
            }
            set
            {
                _baseVelocity = value;
            }
        }

        private Vector3 _attachedRigidbodyVelocity;
        /// <summary>
        /// The character's velocity resulting from standing on rigidbodies or PhysicsMover
        /// </summary>
        public Vector3 AttachedRigidbodyVelocity
        {
            get
            {
                return _attachedRigidbodyVelocity;
            }
            set
            {
                _attachedRigidbodyVelocity = value;
            }
        }

        /// <summary>
        /// The number of overlaps detected so far during character update (is reset at the beginning of the update)
        /// </summary>
        public int OverlapsCount { get; private set; }
        private OverlapResult[] _overlaps = new OverlapResult[MaxRigidbodyOverlapsCount];
        /// <summary>
        /// The overlaps detected so far during character update
        /// </summary>
        public OverlapResult[] Overlaps
        {
            get
            {
                return _overlaps;
            }
        }

        // Warning: Don't touch these constants unless you know exactly what you're doing!
        public const int MaxHitsBudget = 16;
        public const int MaxCollisionBudget = 16;
        public const int MaxGroundingSweepIterations = 2;
        public const int MaxMovementSweepIterations = 6;
        public const int MaxSteppingSweepIterations = 3;
        public const int MaxRigidbodyOverlapsCount = 16;
        public const int MaxDiscreteCollisionIterations = 3;
        public const float CollisionOffset = 0.001f;
        public const float GroundProbeReboundDistance = 0.02f;
        public const float MinimumGroundProbingDistance = 0.005f;
        public const float GroundProbingBackstepDistance = 0.1f;
        public const float SweepProbingBackstepDistance = 0.002f;
        public const float SecondaryProbesVertical = 0.02f;
        public const float SecondaryProbesHorizontal = 0.001f;
        public const float MinVelocityMagnitude = 0.01f;
        public const float SteppingForwardDistance = 0.03f;
        public const float MinDistanceForLedge = 0.05f;
        public const float CorrelationForVerticalObstruction = 0.01f;
        public const float ExtraSteppingForwardDistance = 0.01f;
        public const float ExtraStepHeightPadding = 0.01f;
#pragma warning restore 0414 

        private void OnEnable()
        {
            KinematicCharacterSystem.EnsureCreation();
            KinematicCharacterSystem.RegisterCharacterMotor(this);
        }

        private void OnDisable()
        {
            KinematicCharacterSystem.UnregisterCharacterMotor(this);
        }

        private void Reset()
        {
            ValidateData();
        }

        private void OnValidate()
        {
            ValidateData();
        }

        [ContextMenu("Remove Component")]
        private void HandleRemoveComponent()
        {
            Rigidbody tmpRigidbody = gameObject.GetComponent<Rigidbody>();
            CapsuleCollider tmpCapsule = gameObject.GetComponent<CapsuleCollider>();
            DestroyImmediate(this);
            DestroyImmediate(tmpRigidbody);
            DestroyImmediate(tmpCapsule);
        }

        /// <summary>
        /// Handle validating all required values
        /// </summary>
        public void ValidateData()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.centerOfMass = Vector3.zero;
            Rigidbody.useGravity = false;
            Rigidbody.drag = 0f;
            Rigidbody.angularDrag = 0f;
            Rigidbody.maxAngularVelocity = Mathf.Infinity;
            Rigidbody.maxDepenetrationVelocity = Mathf.Infinity;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            Rigidbody.isKinematic = true;
            Rigidbody.constraints = RigidbodyConstraints.None;
            Rigidbody.interpolation = KinematicCharacterSystem.InterpolationMethod == CharacterSystemInterpolationMethod.Unity ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;

            Capsule = GetComponent<CapsuleCollider>();
            CapsuleRadius = Mathf.Clamp(CapsuleRadius, 0f, CapsuleHeight * 0.5f);
            Capsule.isTrigger = false;
            Capsule.direction = 1;
            Capsule.sharedMaterial = CapsulePhysicsMaterial;
            SetCapsuleDimensions(CapsuleRadius, CapsuleHeight, CapsuleYOffset);

            MaxStepHeight = Mathf.Clamp(MaxStepHeight, 0f, Mathf.Infinity);
            MinRequiredStepDepth = Mathf.Clamp(MinRequiredStepDepth, 0f, CapsuleRadius);

            MaxStableDistanceFromLedge = Mathf.Clamp(MaxStableDistanceFromLedge, 0f, CapsuleRadius);

            transform.localScale = Vector3.one;

#if UNITY_EDITOR
            Capsule.hideFlags = HideFlags.NotEditable;
            Rigidbody.hideFlags = HideFlags.NotEditable;
            if (!Mathf.Approximately(transform.lossyScale.x, 1f) || !Mathf.Approximately(transform.lossyScale.y, 1f) || !Mathf.Approximately(transform.lossyScale.z, 1f))
            {
                Debug.LogError("Character's lossy scale is not (1,1,1). This is not allowed. Make sure the character's transform and all of its parents have a (1,1,1) scale.", this.gameObject);
            }
#endif
        }

        /// <summary>
        /// Sets whether or not the capsule collider will detect collisions
        /// </summary>
        public void SetCapsuleCollisionsActivation(bool kinematicCapsuleActive)
        {
            Rigidbody.detectCollisions = kinematicCapsuleActive;
        }

        /// <summary>
        /// Sets whether or not the motor will solve collisions when moving (or moved onto)
        /// </summary>
        public void SetMovementCollisionsSolvingActivation(bool movementCollisionsSolvingActive)
        {
            _solveMovementCollisions = movementCollisionsSolvingActive;
        }

        /// <summary>
        /// Sets whether or not grounding will be evaluated for all hits
        /// </summary>
        public void SetGroundSolvingActivation(bool stabilitySolvingActive)
        {
            _solveGrounding = stabilitySolvingActive;
        }

        /// <summary>
        /// Sets the character's position directly
        /// </summary>
        public void SetPosition(Vector3 position, bool bypassInterpolation = true)
        {
            Rigidbody.interpolation = RigidbodyInterpolation.None;
            Transform.position = position;
            Rigidbody.position = position;
            InitialSimulationPosition = position;
            TransientPosition = position;

            if (bypassInterpolation)
            {
                InitialTickPosition = position;
            }

            Rigidbody.interpolation = KinematicCharacterSystem.InterpolationMethod == CharacterSystemInterpolationMethod.Unity ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
        }

        /// <summary>
        /// Sets the character's rotation directly
        /// </summary>
        public void SetRotation(Quaternion rotation, bool bypassInterpolation = true)
        {
            Rigidbody.interpolation = RigidbodyInterpolation.None;
            Transform.rotation = rotation;
            Rigidbody.rotation = rotation;
            InitialSimulationRotation = rotation;
            TransientRotation = rotation;

            if (bypassInterpolation)
            {
                InitialTickRotation = rotation;
            }

            Rigidbody.interpolation = KinematicCharacterSystem.InterpolationMethod == CharacterSystemInterpolationMethod.Unity ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
        }

        /// <summary>
        /// Sets the character's position and rotation directly
        /// </summary>
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool bypassInterpolation = true)
        {
            Rigidbody.interpolation = RigidbodyInterpolation.None;
            Transform.SetPositionAndRotation(position, rotation);
            Rigidbody.position = position;
            Rigidbody.rotation = rotation;
            InitialSimulationPosition = position;
            InitialSimulationRotation = rotation;
            TransientPosition = position;
            TransientRotation = rotation;

            if (bypassInterpolation)
            {
                InitialTickPosition = position;
                InitialTickRotation = rotation;
            }

            Rigidbody.interpolation = KinematicCharacterSystem.InterpolationMethod == CharacterSystemInterpolationMethod.Unity ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
        }

        /// <summary>
        /// Moves the character position, taking all movement collision solving int account. The actual move is done the next time the motor updates are called
        /// </summary>
        public void MoveCharacter(Vector3 toPosition)
        {
            _movePositionDirty = true;
            _movePositionTarget = toPosition;
        }

        /// <summary>
        /// Moves the character rotation. The actual move is done the next time the motor updates are called
        /// </summary>
        public void RotateCharacter(Quaternion toRotation)
        {
            _moveRotationDirty = true;
            _moveRotationTarget = toRotation;
        }

        /// <summary>
        /// Returns all the state information of the motor that is pertinent for simulation
        /// </summary>
        public KinematicCharacterMotorState GetState()
        {
            KinematicCharacterMotorState state = new KinematicCharacterMotorState();

            state.Position = TransientPosition;
            state.Rotation = TransientRotation;

            state.BaseVelocity = _baseVelocity;
            state.AttachedRigidbodyVelocity = _attachedRigidbodyVelocity;

            state.MustUnground = MustUnground;
            state.LastMovementIterationFoundAnyGround = LastMovementIterationFoundAnyGround;
            state.GroundingStatus.CopyFrom(GroundingStatus);
            state.AttachedRigidbody = AttachedRigidbody;

            return state;
        }

        /// <summary>
        /// Applies a motor state instantly
        /// </summary>
        public void ApplyState(KinematicCharacterMotorState state, bool bypassInterpolation = true)
        {
            SetPositionAndRotation(state.Position, state.Rotation, bypassInterpolation);

            BaseVelocity = state.BaseVelocity;
            AttachedRigidbodyVelocity = state.AttachedRigidbodyVelocity;

            MustUnground = state.MustUnground;
            LastMovementIterationFoundAnyGround = state.LastMovementIterationFoundAnyGround;
            GroundingStatus.CopyFrom(state.GroundingStatus);
            AttachedRigidbody = state.AttachedRigidbody;
        }

        /// <summary>
        /// Resizes capsule. ALso caches importand capsule size data
        /// </summary>
        public void SetCapsuleDimensions(float radius, float height, float yOffset)
        {
            CapsuleRadius = radius;
            CapsuleHeight = height;
            CapsuleYOffset = yOffset;

            Capsule.radius = CapsuleRadius;
            Capsule.height = Mathf.Clamp(CapsuleHeight, CapsuleRadius * 2f, CapsuleHeight);
            Capsule.center = new Vector3(0f, CapsuleYOffset, 0f);

            CharacterTransformToCapsuleCenter = Capsule.center;
            CharacterTransformToCapsuleBottom = Capsule.center + (-_cachedWorldUp * (Capsule.height * 0.5f));
            CharacterTransformToCapsuleTop = Capsule.center + (_cachedWorldUp * (Capsule.height * 0.5f));
            CharacterTransformToCapsuleBottomHemi = Capsule.center + (-_cachedWorldUp * (Capsule.height * 0.5f)) + (_cachedWorldUp * Capsule.radius);
            CharacterTransformToCapsuleTopHemi = Capsule.center + (_cachedWorldUp * (Capsule.height * 0.5f)) + (-_cachedWorldUp * Capsule.radius);
        }

        private void Awake()
        {
            Transform = this.transform;
            ValidateData();

            TransientPosition = Transform.position;
            TransientRotation = Transform.rotation;

            // Build CollidableLayers mask
            CollidableLayers = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(this.gameObject.layer, i))
                {
                    CollidableLayers |= (1 << i);
                }
            }

            if(CharacterController)
            {
                CharacterController.SetupCharacterMotor(this);
            }

            SetCapsuleDimensions(CapsuleRadius, CapsuleHeight, CapsuleYOffset);
        }

        /// <summary>
        /// Update phase 1 is meant to be called after physics movers have calculated their velocities, but
        /// before they have simulated their goal positions/rotations. It is responsible for:
        /// - Initializing all values for update
        /// - Handling MovePosition calls
        /// - Solving initial collision overlaps
        /// - Ground probing
        /// - Handle detecting potential interactable rigidbodies
        /// </summary>
        public void UpdatePhase1(float deltaTime)
        {
            // NaN propagation safety stop
            if (float.IsNaN(_baseVelocity.x) || float.IsNaN(_baseVelocity.y) || float.IsNaN(_baseVelocity.z))
            {
                _baseVelocity = Vector3.zero;
            }
            if (float.IsNaN(_attachedRigidbodyVelocity.x) || float.IsNaN(_attachedRigidbodyVelocity.y) || float.IsNaN(_attachedRigidbodyVelocity.z))
            {
                _attachedRigidbodyVelocity = Vector3.zero;
            }

#if UNITY_EDITOR
            if (!Mathf.Approximately(Transform.lossyScale.x, 1f) || !Mathf.Approximately(Transform.lossyScale.y, 1f) || !Mathf.Approximately(Transform.lossyScale.z, 1f))
            {
                Debug.LogError("Character's lossy scale is not (1,1,1). This is not allowed. Make sure the character's transform and all of its parents have a (1,1,1) scale.", this.gameObject);
            }
#endif
            
            // Before update
            this.CharacterController.BeforeCharacterUpdate(deltaTime);

            TransientPosition = Transform.position;
            TransientRotation = Transform.rotation;
            InitialSimulationPosition = TransientPosition;
            InitialSimulationRotation = TransientRotation;
            _rigidbodyProjectionHitCount = 0;
            OverlapsCount = 0;
            _lastSolvedOverlapNormalDirty = false;

            #region Handle Move Position
            if (_movePositionDirty)
            {
                if (_solveMovementCollisions)
                {
                    if (InternalCharacterMove((_movePositionTarget - TransientPosition), deltaTime, out _internalResultingMovementMagnitude, out _internalResultingMovementDirection))
                    {
                        if (InteractiveRigidbodyHandling)
                        {
                            Vector3 tmpVelocity = Vector3.zero;
                            ProcessVelocityForRigidbodyHits(ref tmpVelocity, deltaTime);
                        }
                    }
                }
                else
                {
                    TransientPosition = _movePositionTarget;
                }

                _movePositionDirty = false;
            }
            #endregion

            LastGroundingStatus.CopyFrom(GroundingStatus);
            GroundingStatus = new CharacterGroundingReport();
            GroundingStatus.GroundNormal = CharacterUp;

            if (_solveMovementCollisions)
            {
                #region Resolve initial overlaps
                Vector3 resolutionDirection = _cachedWorldUp;
                float resolutionDistance = 0f;
                int iterationsMade = 0;
                bool overlapSolved = false;
                while(iterationsMade < MaxDiscreteCollisionIterations && !overlapSolved)
                {
                    int nbOverlaps = CharacterCollisionsOverlap(TransientPosition, TransientRotation, _internalProbedColliders);

                    if (nbOverlaps > 0)
                    {
                        // Solve overlaps that aren't against dynamic rigidbodies or physics movers
                        for (int i = 0; i < nbOverlaps; i++)
                        {
                            Rigidbody probedRigidbody = _internalProbedColliders[i].attachedRigidbody;
                            bool isPhysicsMoverOrDynamicRigidbody = probedRigidbody && (!probedRigidbody.isKinematic || probedRigidbody.GetComponent<PhysicsMover>());
                            if (!isPhysicsMoverOrDynamicRigidbody)
                            {
                                // Process overlap
                                Transform overlappedTransform = _internalProbedColliders[i].GetComponent<Transform>();
                                if (Physics.ComputePenetration(
                                        Capsule,
                                        TransientPosition,
                                        TransientRotation,
                                        _internalProbedColliders[i],
                                        overlappedTransform.position,
                                        overlappedTransform.rotation,
                                        out resolutionDirection,
                                        out resolutionDistance))
                                {
                                    // Resolve along obstruction direction
                                    Vector3 originalResolutionDirection = resolutionDirection;
                                    HitStabilityReport mockReport = new HitStabilityReport();
                                    mockReport.IsStable = IsStableOnNormal(resolutionDirection);
                                    resolutionDirection = GetObstructionNormal(resolutionDirection, mockReport);

                                    // Solve overlap
                                    Vector3 resolutionMovement = resolutionDirection * (resolutionDistance + CollisionOffset);
                                    TransientPosition += resolutionMovement;

                                    // Remember overlaps
                                    if (OverlapsCount < _overlaps.Length)
                                    {
                                        _overlaps[OverlapsCount] = new OverlapResult(resolutionDirection, _internalProbedColliders[i]);
                                        OverlapsCount++;
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        overlapSolved = true;
                    }

                    iterationsMade++;
                }
                #endregion
            }

            #region Ground Probing and Snapping
            // Handle ungrounding
            if (_solveGrounding)
            {
                if (MustUnground)
                {
                    TransientPosition += CharacterUp * (MinimumGroundProbingDistance * 1.5f);
                }
                else
                {
                    // Choose the appropriate ground probing distance
                    float selectedGroundProbingDistance = MinimumGroundProbingDistance; 
                    if (!LastGroundingStatus.SnappingPrevented && (LastGroundingStatus.IsStableOnGround || LastMovementIterationFoundAnyGround))
                    {
                        if (StepHandling != StepHandlingMethod.None)
                        {
                            selectedGroundProbingDistance = Mathf.Max(CapsuleRadius, MaxStepHeight);
                        }
                        else
                        {
                            selectedGroundProbingDistance = CapsuleRadius;
                        }

                        selectedGroundProbingDistance += GroundDetectionExtraDistance;
                    }

                    ProbeGround(ref _internalTransientPosition, TransientRotation, selectedGroundProbingDistance, ref GroundingStatus);
                }
            }

            LastMovementIterationFoundAnyGround = false;
            MustUnground = false;
            #endregion

            if (_solveGrounding)
            {
                CharacterController.PostGroundingUpdate(deltaTime);
            }

            if(InteractiveRigidbodyHandling)
            {
                #region Interactive Rigidbody Handling 
                _lastAttachedRigidbody = AttachedRigidbody;
                if (AttachedRigidbodyOverride)
                {
                    AttachedRigidbody = AttachedRigidbodyOverride;
                }
                else
                {
                    // Detect interactive rigidbodies from grounding
                    if (GroundingStatus.IsStableOnGround && GroundingStatus.GroundCollider.attachedRigidbody)
                    {
                        Rigidbody interactiveRigidbody = GetInteractiveRigidbody(GroundingStatus.GroundCollider);
                        if (interactiveRigidbody)
                        {
                            AttachedRigidbody = interactiveRigidbody;
                        }
                    }
                    else
                    {
                        AttachedRigidbody = null;
                    }
                }

                Vector3 tmpVelocityFromCurrentAttachedRigidbody = Vector3.zero;
                if(AttachedRigidbody)
                {
                    tmpVelocityFromCurrentAttachedRigidbody = GetVelocityFromRigidbodyMovement(AttachedRigidbody, TransientPosition, deltaTime);
                }

                // Conserve momentum when de-stabilized from an attached rigidbody
                if (PreserveAttachedRigidbodyMomentum && _lastAttachedRigidbody != null && AttachedRigidbody != _lastAttachedRigidbody)
                {
                    _baseVelocity += _attachedRigidbodyVelocity;
                    _baseVelocity -= tmpVelocityFromCurrentAttachedRigidbody;
                }

                // Process additionnal Velocity from attached rigidbody
                _attachedRigidbodyVelocity = _cachedZeroVector;
                if (AttachedRigidbody)
                {
                    _attachedRigidbodyVelocity = tmpVelocityFromCurrentAttachedRigidbody;

                    // Rotation from attached rigidbody
                    Vector3 newForward = Vector3.ProjectOnPlane(Quaternion.Euler(Mathf.Rad2Deg * AttachedRigidbody.angularVelocity * deltaTime) * CharacterForward, CharacterUp).normalized;
                    TransientRotation = Quaternion.LookRotation(newForward, CharacterUp);
                }

                // Cancel out horizontal velocity upon landing on an attached rigidbody
                if (GroundingStatus.GroundCollider &&
                    GroundingStatus.GroundCollider.attachedRigidbody && 
                    GroundingStatus.GroundCollider.attachedRigidbody == AttachedRigidbody && 
                    AttachedRigidbody != null && 
                    _lastAttachedRigidbody == null)
                {
                    _baseVelocity -= Vector3.ProjectOnPlane(_attachedRigidbodyVelocity, CharacterUp);
                }

                // Movement from Attached Rigidbody
                if (_attachedRigidbodyVelocity.sqrMagnitude > 0f)
                {
                    _isMovingFromAttachedRigidbody = true;

                    if (_solveMovementCollisions)
                    {
                        // Perform the move from rgdbdy velocity
                        if (InternalCharacterMove(_attachedRigidbodyVelocity * deltaTime, deltaTime, out _internalResultingMovementMagnitude, out _internalResultingMovementDirection))
                        {
                            _attachedRigidbodyVelocity = (_internalResultingMovementDirection * _internalResultingMovementMagnitude) / deltaTime;
                        }
                        else
                        {
                            _attachedRigidbodyVelocity = Vector3.zero;
                        }
                    }
                    else
                    {
                        TransientPosition += _attachedRigidbodyVelocity * deltaTime;
                    }
                    
                    _isMovingFromAttachedRigidbody = false;
                }
                #endregion
            }
        }

        /// <summary>
        /// Update phase 2 is meant to be called after physics movers have simulated their goal positions/rotations. 
        /// At the end of this, the TransientPosition/Rotation values will be up-to-date with where the motor should be at the end of its move. 
        /// It is responsible for:
        /// - Solving Rotation
        /// - Handle MoveRotation calls
        /// - Solving potential attached rigidbody overlaps
        /// - Solving Velocity
        /// - Applying planar constraint
        /// </summary>
        public void UpdatePhase2(float deltaTime)
        {
            // Handle rotation
            this.CharacterController.UpdateRotation(ref _internalTransientRotation, deltaTime);
            TransientRotation = _internalTransientRotation;

            // Handle move rotation
            if (_moveRotationDirty)
            {
                TransientRotation = _moveRotationTarget;
                _moveRotationDirty = false;
            }
            
            if (_solveMovementCollisions && InteractiveRigidbodyHandling)
            {
                if (InteractiveRigidbodyHandling)
                {
                    #region Solve potential attached rigidbody overlap
                    if (AttachedRigidbody)
                    {
                        float upwardsOffset = Capsule.radius;

                        RaycastHit closestHit;
                        if (CharacterGroundSweep(
                            TransientPosition + (CharacterUp * upwardsOffset),
                            TransientRotation,
                            -CharacterUp,
                            upwardsOffset,
                            out closestHit))
                        {
                            if (closestHit.collider.attachedRigidbody == AttachedRigidbody && IsStableOnNormal(closestHit.normal))
                            {
                                float distanceMovedUp = (upwardsOffset - closestHit.distance);
                                TransientPosition = TransientPosition + (CharacterUp * distanceMovedUp) + (CharacterUp * CollisionOffset);
                            }
                        }
                    }
                    #endregion
                }

                if (SafeMovement || InteractiveRigidbodyHandling)
                {
                    #region Resolve overlaps that could've been caused by rotation or physics movers simulation pushing the character
                    Vector3 resolutionDirection = _cachedWorldUp;
                    float resolutionDistance = 0f;
                    int iterationsMade = 0;
                    bool overlapSolved = false;
                    while (iterationsMade < MaxDiscreteCollisionIterations && !overlapSolved)
                    {
                        int nbOverlaps = CharacterCollisionsOverlap(TransientPosition, TransientRotation, _internalProbedColliders);
                        if (nbOverlaps > 0)
                        {
                            for (int i = 0; i < nbOverlaps; i++)
                            {
                                // Process overlap
                                Transform overlappedTransform = _internalProbedColliders[i].GetComponent<Transform>();
                                if (Physics.ComputePenetration(
                                        Capsule,
                                        TransientPosition,
                                        TransientRotation,
                                        _internalProbedColliders[i],
                                        overlappedTransform.position,
                                        overlappedTransform.rotation,
                                        out resolutionDirection,
                                        out resolutionDistance))
                                {
                                    // Resolve along obstruction direction
                                    Vector3 originalResolutionDirection = resolutionDirection;
                                    HitStabilityReport mockReport = new HitStabilityReport();
                                    mockReport.IsStable = IsStableOnNormal(resolutionDirection);
                                    resolutionDirection = GetObstructionNormal(resolutionDirection, mockReport);

                                    // Solve overlap
                                    Vector3 resolutionMovement = resolutionDirection * (resolutionDistance + CollisionOffset);
                                    TransientPosition += resolutionMovement;

                                    // If physicsMover, register as rigidbody hit for velocity
                                    if (InteractiveRigidbodyHandling)
                                    {
                                        Rigidbody probedRigidbody = _internalProbedColliders[i].attachedRigidbody;
                                        if (probedRigidbody)
                                        {
                                            PhysicsMover physicsMover = probedRigidbody.GetComponent<PhysicsMover>();
                                            if (physicsMover)
                                            {
                                                bool isPhysicsMoverOrDynamicRigidbody = probedRigidbody && (!probedRigidbody.isKinematic || physicsMover);
                                                if (isPhysicsMoverOrDynamicRigidbody)
                                                {
                                                    HitStabilityReport tmpReport = new HitStabilityReport();
                                                    tmpReport.IsStable = IsStableOnNormal(resolutionDirection);
                                                    if (tmpReport.IsStable)
                                                    {
                                                        LastMovementIterationFoundAnyGround = tmpReport.IsStable;
                                                    }
                                                    if (physicsMover.Rigidbody && physicsMover.Rigidbody != AttachedRigidbody)
                                                    {
                                                        Vector3 characterCenter = TransientPosition + (TransientRotation * CharacterTransformToCapsuleCenter);
                                                        Vector3 estimatedCollisionPoint = TransientPosition;

                                                        MeshCollider meshColl = _internalProbedColliders[i] as MeshCollider;
                                                        if (!(meshColl && !meshColl.convex))
                                                        {
                                                            Physics.ClosestPoint(characterCenter, _internalProbedColliders[i], overlappedTransform.position, overlappedTransform.rotation);
                                                        }

                                                        StoreRigidbodyHit(
                                                            physicsMover.Rigidbody,
                                                            Velocity,
                                                            estimatedCollisionPoint,
                                                            resolutionDirection,
                                                            tmpReport);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // Remember overlaps
                                    if (OverlapsCount < _overlaps.Length)
                                    {
                                        _overlaps[OverlapsCount] = new OverlapResult(resolutionDirection, _internalProbedColliders[i]);
                                        OverlapsCount++;
                                    }

                                    break;
                                }
                            }
                        }
                        else
                        {
                            overlapSolved = true;
                        }

                        iterationsMade++;
                    }
                    #endregion
                }
            }

            // Handle velocity
            this.CharacterController.UpdateVelocity(ref _baseVelocity, deltaTime);
            if (_baseVelocity.magnitude < MinVelocityMagnitude)
            {
                _baseVelocity = Vector3.zero;
            }

            #region Calculate Character movement from base velocity   
            // Perform the move from base velocity
            if (_baseVelocity.sqrMagnitude > 0f)
            {
                if (_solveMovementCollisions)
                {
                    if (InternalCharacterMove(_baseVelocity * deltaTime, deltaTime, out _internalResultingMovementMagnitude, out _internalResultingMovementDirection))
                    {
                        _baseVelocity = (_internalResultingMovementDirection * _internalResultingMovementMagnitude) / deltaTime;
                    }
                    else
                    {
                        _baseVelocity = Vector3.zero;
                    }
                }
                else
                {
                    TransientPosition += _baseVelocity * deltaTime;
                }
            }

            // Process rigidbody hits/overlaps to affect velocity
            if (InteractiveRigidbodyHandling)
            {
                ProcessVelocityForRigidbodyHits(ref _baseVelocity, deltaTime);
            }
            #endregion

            // Handle planar constraint
            if(HasPlanarConstraint)
            {
                TransientPosition = InitialSimulationPosition + Vector3.ProjectOnPlane(TransientPosition - InitialSimulationPosition, PlanarConstraintAxis.normalized);
            }

            // Discrete collision detection
            if(DetectDiscreteCollisions)
            {
                int nbOverlaps = CharacterCollisionsOverlap(TransientPosition, TransientRotation, _internalProbedColliders, CollisionOffset * 2f);
                for(int i = 0; i < nbOverlaps; i++)
                {
                    CharacterController.OnDiscreteCollisionDetected(_internalProbedColliders[i]);
                }
            }

            this.CharacterController.AfterCharacterUpdate(deltaTime);
        }

        /// <summary>
        /// Determines if motor can be considered stable on given slope normal
        /// </summary>
        private bool IsStableOnNormal(Vector3 normal)
        {
            return Vector3.Angle(CharacterUp, normal) <= MaxStableSlopeAngle;
        }

        /// <summary>
        /// Probes for valid ground and midifies the input transientPosition if ground snapping occurs
        /// </summary>
        public void ProbeGround(ref Vector3 probingPosition, Quaternion atRotation, float probingDistance, ref CharacterGroundingReport groundingReport)
        {
            if (probingDistance < MinimumGroundProbingDistance)
            {
                probingDistance = MinimumGroundProbingDistance;
            }

            int groundSweepsMade = 0;
            RaycastHit groundSweepHit = new RaycastHit();
            bool groundSweepingIsOver = false;
            Vector3 groundSweepPosition = probingPosition;
            Vector3 groundSweepDirection = (atRotation * -_cachedWorldUp);
            float groundProbeDistanceRemaining = probingDistance;
            while (groundProbeDistanceRemaining > 0 && (groundSweepsMade <= MaxGroundingSweepIterations) && !groundSweepingIsOver)
            {
                // Sweep for ground detection
                if (CharacterGroundSweep(
                        groundSweepPosition, // position
                        atRotation, // rotation
                        groundSweepDirection, // direction
                        groundProbeDistanceRemaining, // distance
                        out groundSweepHit)) // hit
                {
                    Vector3 targetPosition = groundSweepPosition + (groundSweepDirection * groundSweepHit.distance);
                    HitStabilityReport groundHitStabilityReport = new HitStabilityReport();
                    EvaluateHitStability(groundSweepHit.collider, groundSweepHit.normal, groundSweepHit.point, targetPosition, TransientRotation, ref groundHitStabilityReport);

                    // Handle ledge stability
                    if (groundHitStabilityReport.LedgeDetected)
                    {
                        if (groundHitStabilityReport.IsOnEmptySideOfLedge && groundHitStabilityReport.DistanceFromLedge > MaxStableDistanceFromLedge)
                        {
                            groundHitStabilityReport.IsStable = false;
                        }
                    }

                    groundingReport.FoundAnyGround = true;
                    groundingReport.GroundNormal = groundSweepHit.normal;
                    groundingReport.InnerGroundNormal = groundHitStabilityReport.InnerNormal;
                    groundingReport.OuterGroundNormal = groundHitStabilityReport.OuterNormal;
                    groundingReport.GroundCollider = groundSweepHit.collider;
                    groundingReport.GroundPoint = groundSweepHit.point;
                    groundingReport.SnappingPrevented = false;

                    // Found stable ground
                    if (groundHitStabilityReport.IsStable)
                    {
                        // Find all scenarios where ground snapping should be canceled
                        if (LedgeHandling)
                        {
                            // "Launching" off of slopes of a certain denivelation angle
                            if (LastGroundingStatus.FoundAnyGround && groundHitStabilityReport.InnerNormal.sqrMagnitude != 0f && groundHitStabilityReport.OuterNormal.sqrMagnitude != 0f)
                            {
                                float denivelationAngle = Vector3.Angle(groundHitStabilityReport.InnerNormal, groundHitStabilityReport.OuterNormal);
                                if (denivelationAngle > MaxStableDenivelationAngle)
                                {
                                    groundingReport.SnappingPrevented = true;
                                }
                                else
                                {
                                    denivelationAngle = Vector3.Angle(LastGroundingStatus.InnerGroundNormal, groundHitStabilityReport.OuterNormal);
                                    if (denivelationAngle > MaxStableDenivelationAngle)
                                    {
                                        groundingReport.SnappingPrevented = true;
                                    }
                                }
                            }

                            // Ledge stability
                            if (PreventSnappingOnLedges && groundHitStabilityReport.LedgeDetected)
                            {
                                groundingReport.SnappingPrevented = true;
                            }
                        }

                        groundingReport.IsStableOnGround = true;

                        // Ground snapping
                        if (!groundingReport.SnappingPrevented)
                        {
                            targetPosition += (-groundSweepDirection * CollisionOffset);
                            InternalMoveCharacterPosition(ref probingPosition, targetPosition, atRotation);
                        }

                        this.CharacterController.OnGroundHit(groundSweepHit.collider, groundSweepHit.normal, groundSweepHit.point, ref groundHitStabilityReport);
                        groundSweepingIsOver = true;
                    }
                    else
                    {
                        // Calculate movement from this iteration and advance position
                        Vector3 sweepMovement = (groundSweepDirection * groundSweepHit.distance) + ((atRotation * Vector3.up) * Mathf.Clamp(CollisionOffset, 0f, groundSweepHit.distance));
                        groundSweepPosition = groundSweepPosition + sweepMovement;

                        // Set remaining distance
                        groundProbeDistanceRemaining = Mathf.Min(GroundProbeReboundDistance, Mathf.Clamp(groundProbeDistanceRemaining - sweepMovement.magnitude, 0f, Mathf.Infinity));

                        // Reorient direction
                        groundSweepDirection = Vector3.ProjectOnPlane(groundSweepDirection, groundSweepHit.normal).normalized;
                    }
                }
                else
                {
                    groundSweepingIsOver = true;
                }

                groundSweepsMade++;
            }
        }

        /// <summary>
        /// Forces the character to unground itself on its next grounding update
        /// </summary>
        public void ForceUnground()
        {
            MustUnground = true;
        }

        /// <summary>
        /// Returns the direction adjusted to be tangent to a specified surface normal relatively to the character's up direction.
        /// Useful for reorienting a direction on a slope without any lateral deviation in trajectory
        /// </summary>
        public Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 surfaceNormal)
        {
            Vector3 directionRight = Vector3.Cross(direction, CharacterUp);
            return Vector3.Cross(surfaceNormal, directionRight).normalized;
        }

        /// <summary>
        /// Moves the character's position by given movement while taking into account all physics simulation, step-handling and 
        /// velocity projection rules that affect the character motor
        /// </summary>
        /// <returns> Returns false if movement could not be solved until the end </returns>
        private bool InternalCharacterMove(Vector3 movement, float deltaTime, out float resultingMovementMagnitude, out Vector3 resultingMovementDirection)
        {
            _rigidbodiesPushedCount = 0;
            bool wasCompleted = true;
            Vector3 remainingMovementDirection = movement.normalized;
            float remainingMovementMagnitude = movement.magnitude;
            resultingMovementDirection = remainingMovementDirection;
            resultingMovementMagnitude = remainingMovementMagnitude;
            int sweepsMade = 0;
            RaycastHit closestSweepHit;
            bool hitSomethingThisSweepIteration = true;
            Vector3 tmpMovedPosition = TransientPosition;
            Vector3 targetPositionAfterSweep = TransientPosition;
            Vector3 originalMoveDirection = movement.normalized;
            Vector3 previousMovementHitNormal = _cachedZeroVector;
            MovementSweepState sweepState = MovementSweepState.Initial;

            // Project movement against current overlaps
            for (int i = 0; i < OverlapsCount; i++)
            {
                if (Vector3.Dot(remainingMovementDirection, _overlaps[i].Normal) < 0f)
                {
                    InternalHandleMovementProjection(
                        IsStableOnNormal(
                            _overlaps[i].Normal) && !MustUnground,
                            _overlaps[i].Normal,
                            _overlaps[i].Normal,
                            originalMoveDirection,
                            ref sweepState,
                            ref previousMovementHitNormal,
                            ref resultingMovementMagnitude,
                            ref remainingMovementDirection,
                            ref remainingMovementMagnitude);
                }
            }

            // Sweep the desired movement to detect collisions
            while (remainingMovementMagnitude > 0f &&
                (sweepsMade <= MaxMovementSweepIterations) &&
                hitSomethingThisSweepIteration)
            {
                if (CharacterCollisionsSweep(
                        tmpMovedPosition, // position
                        TransientRotation, // rotation
                        remainingMovementDirection, // direction
                        remainingMovementMagnitude + CollisionOffset, // distance
                        out closestSweepHit, // closest hit
                        _internalCharacterHits) // all hits
                    > 0)
                {
                    // Calculate movement from this iteration
                    targetPositionAfterSweep = tmpMovedPosition + (remainingMovementDirection * closestSweepHit.distance) + (closestSweepHit.normal * CollisionOffset);
                    Vector3 sweepMovement = targetPositionAfterSweep - tmpMovedPosition;

                    // Evaluate if hit is stable
                    HitStabilityReport moveHitStabilityReport = new HitStabilityReport();
                    EvaluateHitStability(closestSweepHit.collider, closestSweepHit.normal, closestSweepHit.point, targetPositionAfterSweep, TransientRotation, ref moveHitStabilityReport);

                    // Handle stepping up perfectly vertical walls
                    bool foundValidStepHit = false;
                    if (_solveGrounding && StepHandling != StepHandlingMethod.None && moveHitStabilityReport.ValidStepDetected)
                    {
                        float obstructionCorrelation = Mathf.Abs(Vector3.Dot(closestSweepHit.normal, CharacterUp));
                        if (obstructionCorrelation <= CorrelationForVerticalObstruction)
                        {
                            RaycastHit closestStepHit;
                            Vector3 stepForwardDirection = Vector3.ProjectOnPlane(-closestSweepHit.normal, CharacterUp).normalized;
                            Vector3 stepCastStartPoint = (targetPositionAfterSweep + (stepForwardDirection * SteppingForwardDistance)) +
                                (CharacterUp * MaxStepHeight);

                            // Cast downward from the top of the stepping height
                            int nbStepHits = CharacterCollisionsSweep(
                                                stepCastStartPoint, // position
                                                TransientRotation, // rotation
                                                -CharacterUp, // direction
                                                MaxStepHeight, // distance
                                                out closestStepHit, // closest hit
                                                _internalCharacterHits); // all hitswwasa  

                            // Check for hit corresponding to stepped collider
                            for (int i = 0; i < nbStepHits; i++)
                            {
                                if (_internalCharacterHits[i].collider == moveHitStabilityReport.SteppedCollider)
                                {

                                    Vector3 endStepPosition = stepCastStartPoint + (-CharacterUp * (_internalCharacterHits[i].distance - CollisionOffset));
                                    tmpMovedPosition = endStepPosition;
                                    foundValidStepHit = true;

                                    // Consume magnitude for step
                                    remainingMovementMagnitude = Mathf.Clamp(remainingMovementMagnitude - sweepMovement.magnitude, 0f, Mathf.Infinity);
                                    break;
                                }
                            }
                        }
                    }

                    // Handle movement solving
                    if (!foundValidStepHit)
                    {
                        // Apply the actual movement
                        tmpMovedPosition = targetPositionAfterSweep;
                        remainingMovementMagnitude = Mathf.Clamp(remainingMovementMagnitude - sweepMovement.magnitude, 0f, Mathf.Infinity);
                        
                        // Movement hit callback
                        this.CharacterController.OnMovementHit(closestSweepHit.collider, closestSweepHit.normal, closestSweepHit.point, ref moveHitStabilityReport);
                        Vector3 obstructionNormal = GetObstructionNormal(closestSweepHit.normal, moveHitStabilityReport);

                        // Handle remembering rigidbody hits
                        if (InteractiveRigidbodyHandling && closestSweepHit.collider.attachedRigidbody)
                        {
                            StoreRigidbodyHit(
                                closestSweepHit.collider.attachedRigidbody, 
                                (remainingMovementDirection * resultingMovementMagnitude) / deltaTime,
                                closestSweepHit.point,
                                obstructionNormal,
                                moveHitStabilityReport);
                        }

                        // Project movement
                        InternalHandleMovementProjection(
                            moveHitStabilityReport.IsStable && !MustUnground,
                            closestSweepHit.normal,
                            obstructionNormal,
                            originalMoveDirection,
                            ref sweepState,
                            ref previousMovementHitNormal,
                            ref resultingMovementMagnitude,
                            ref remainingMovementDirection,
                            ref remainingMovementMagnitude);
                    }
                }
                // If we hit nothing...
                else
                {
                    hitSomethingThisSweepIteration = false;
                }

                // Safety for exceeding max sweeps allowed
                sweepsMade++;
                if (sweepsMade > MaxMovementSweepIterations)
                {
                    remainingMovementMagnitude = 0;
                    wasCompleted = false;
                }
            }

            // Move position for the remainder of the movement
            Vector3 targetFinalPosition = tmpMovedPosition + (remainingMovementDirection * remainingMovementMagnitude);
            InternalMoveCharacterPosition(ref _internalTransientPosition, targetFinalPosition, TransientRotation);
            resultingMovementDirection = remainingMovementDirection;

            return wasCompleted;
        }

        /// <summary>
        /// Gets the effective normal for movement obstruction depending on current grounding status
        /// </summary>
        private Vector3 GetObstructionNormal(Vector3 hitNormal, HitStabilityReport hitStabilityReport)
        {
            // Find hit/obstruction/offset normal
            Vector3 obstructionNormal = hitNormal;
            if (GroundingStatus.IsStableOnGround && !MustUnground && !hitStabilityReport.IsStable)
            {
                Vector3 obstructionLeftAlongGround = Vector3.Cross(GroundingStatus.GroundNormal, obstructionNormal).normalized;
                obstructionNormal = Vector3.Cross(obstructionLeftAlongGround, CharacterUp).normalized;
            }

            // Catch cases where cross product between parallel normals returned 0
            if(obstructionNormal == Vector3.zero)
            {
                obstructionNormal = hitNormal;
            }

            return obstructionNormal;
        }

        /// <summary>
        /// Remembers a rigidbody hit for processing later
        /// </summary>
        private void StoreRigidbodyHit(Rigidbody hitRigidbody, Vector3 hitVelocity, Vector3 hitPoint, Vector3 obstructionNormal, HitStabilityReport hitStabilityReport)
        {
            if (_rigidbodyProjectionHitCount < _internalRigidbodyProjectionHits.Length)
            {
                if (!hitRigidbody.GetComponent<KinematicCharacterMotor>())
                {
                    RigidbodyProjectionHit rph = new RigidbodyProjectionHit();
                    rph.Rigidbody = hitRigidbody;
                    rph.HitPoint = hitPoint;
                    rph.EffectiveHitNormal = obstructionNormal;
                    rph.HitVelocity = hitVelocity;
                    rph.StableOnHit = hitStabilityReport.IsStable;

                    _internalRigidbodyProjectionHits[_rigidbodyProjectionHitCount] = rph;
                    _rigidbodyProjectionHitCount++;
                }
            }
        }

        /// <summary>
        /// Processes movement projection upon detecting a hit
        /// </summary>
        private void InternalHandleMovementProjection(bool stableOnHit, Vector3 hitNormal, Vector3 obstructionNormal, Vector3 originalMoveDirection, ref MovementSweepState sweepState, 
            ref Vector3 previousObstructionNormal, ref float resultingMovementMagnitude, ref Vector3 remainingMovementDirection, ref float remainingMovementMagnitude)
        {
            if (remainingMovementMagnitude <= 0)
            {
                return;
            }
            
            Vector3 remainingMovement = originalMoveDirection * remainingMovementMagnitude;
            float remainingMagnitudeBeforeProj = remainingMovementMagnitude;
            if (stableOnHit)
            {
                LastMovementIterationFoundAnyGround = true;
            }

            // Blocking-corner handling
            if (sweepState == MovementSweepState.FoundBlockingCrease)
            {
                remainingMovementMagnitude = 0f;
                resultingMovementMagnitude = 0f;
                
                sweepState = MovementSweepState.FoundBlockingCorner;
            }
            // Handle projection
            else
            {
                CharacterController.HandleMovementProjection(ref remainingMovement, obstructionNormal, stableOnHit);

                remainingMovementDirection = remainingMovement.normalized;
                remainingMovementMagnitude = remainingMovement.magnitude;
                resultingMovementMagnitude = (remainingMovementMagnitude / remainingMagnitudeBeforeProj) * resultingMovementMagnitude;

                // Blocking corner handling
                if (sweepState == MovementSweepState.Initial)
                {
                    sweepState = MovementSweepState.AfterFirstHit;
                }
                else if (sweepState == MovementSweepState.AfterFirstHit)
                {
                    // Detect blocking corners
                    if (Vector3.Dot(previousObstructionNormal, remainingMovementDirection) < 0f)
                    {
                        Vector3 cornerVector = Vector3.Cross(previousObstructionNormal, obstructionNormal).normalized;
                        remainingMovement = Vector3.Project(remainingMovement, cornerVector);
                        remainingMovementDirection = remainingMovement.normalized;
                        remainingMovementMagnitude = remainingMovement.magnitude;
                        resultingMovementMagnitude = (remainingMovementMagnitude / remainingMagnitudeBeforeProj) * resultingMovementMagnitude;

                        sweepState = MovementSweepState.FoundBlockingCrease;
                    }
                }
            }

            previousObstructionNormal = obstructionNormal;
        }

        /// <summary>
        /// Moves the input position to the target. If SafeMovement is on, only move if we detect that the 
        /// character would not be overlapping with anything at the target position
        /// </summary>
        /// <returns> Returns true if no overlaps were found </returns>
        private bool InternalMoveCharacterPosition(ref Vector3 movedPosition, Vector3 targetPosition, Quaternion atRotation)
        {
            bool movementValid = true;
            if (SafeMovement)
            {
                int nbOverlaps = CharacterCollisionsOverlap(targetPosition, atRotation, _internalProbedColliders);
                if (nbOverlaps > 0)
                {
                    movementValid = false;
                }
            }

            if(movementValid)
            {
                movedPosition = targetPosition;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Takes into account rigidbody hits for adding to the velocity
        /// </summary>
        private void ProcessVelocityForRigidbodyHits(ref Vector3 processedVelocity, float deltaTime)
        {
            for (int i = 0; i < _rigidbodyProjectionHitCount; i++)
            {
                if (_internalRigidbodyProjectionHits[i].Rigidbody)
                {
                    // Keep track of the unique rigidbodies we pushed this update, to avoid doubling their effect
                    bool alreadyPushedThisRigidbody = false;
                    for (int j = 0; j < _rigidbodiesPushedCount; j++)
                    {
                        if (_rigidbodiesPushedThisMove[j] == _internalRigidbodyProjectionHits[j].Rigidbody)
                        {
                            alreadyPushedThisRigidbody = true;
                            break;
                        }
                    }

                    if (!alreadyPushedThisRigidbody && _internalRigidbodyProjectionHits[i].Rigidbody != AttachedRigidbody)
                    {
                        if (_rigidbodiesPushedCount < _rigidbodiesPushedThisMove.Length)
                        {
                            // Remember we hit this rigidbody
                            _rigidbodiesPushedThisMove[_rigidbodiesPushedCount] = _internalRigidbodyProjectionHits[i].Rigidbody;
                            _rigidbodiesPushedCount++;

                            if(RigidbodyInteractionType == RigidbodyInteractionType.SimulatedDynamic)
                            {
                                CharacterController.HandleSimulatedRigidbodyInteraction(ref processedVelocity, _internalRigidbodyProjectionHits[i], deltaTime);
                            }                            
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the input collider is valid for collision processing
        /// </summary>
        /// <returns> Returns true if the collider is valid </returns>
        private bool CheckIfColliderValidForCollisions(Collider coll)
        {
            // Ignore self
            if (coll == null ||
                coll == Capsule)
            {
                return false;
            }

            if (!IsColliderValidForCollisions(coll))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the input collider is valid for collision processing
        /// </summary>
        private bool IsColliderValidForCollisions(Collider coll)
        {
            // Ignore dynamic rigidbodies if the movement is made from AttachedRigidbody, or if RigidbodyInteractionType is kinematic
            if ((_isMovingFromAttachedRigidbody || RigidbodyInteractionType == RigidbodyInteractionType.Kinematic) && coll.attachedRigidbody && !coll.attachedRigidbody.isKinematic)
            {
                return false;
            }

            // If movement is made from AttachedRigidbody, ignore the AttachedRigidbody
            if (_isMovingFromAttachedRigidbody && coll.attachedRigidbody == AttachedRigidbody)
            {
                return false;
            }

            // Custom checks
            if (!this.CharacterController.IsColliderValidForCollisions(coll))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the motor is considered stable on a given hit
        /// </summary>
        public void EvaluateHitStability(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport stabilityReport)
        {
            if(!_solveGrounding)
            {
                stabilityReport.IsStable = false;
                return;
            }

            bool isStableOnNormal = false;
            Vector3 atCharacterUp = atCharacterRotation * Vector3.up;
            Vector3 innerHitDirection = Vector3.ProjectOnPlane(hitNormal, atCharacterUp).normalized;

            isStableOnNormal = this.IsStableOnNormal(hitNormal);
            stabilityReport.InnerNormal = hitNormal;
            stabilityReport.OuterNormal = hitNormal;
            
            // Step handling
            if (StepHandling != StepHandlingMethod.None && !isStableOnNormal)
            {
                // Stepping not supported on dynamic rigidbodies
                Rigidbody hitRigidbody = hitCollider.attachedRigidbody;
                if (!(hitRigidbody && !hitRigidbody.isKinematic))
                {
                    DetectSteps(atCharacterPosition, atCharacterRotation, hitPoint, innerHitDirection, ref stabilityReport);
                }
            }
            
            // Ledge handling
            if (LedgeHandling)
            {
                float ledgeCheckHeight = MinDistanceForLedge;
                if(StepHandling != StepHandlingMethod.None)
                {
                    ledgeCheckHeight = MaxStepHeight;
                }

                bool isStableLedgeInner = false;
                bool isStableLedgeOuter = false;

                RaycastHit innerLedgeHit;
                if (CharacterCollisionsRaycast(
                    hitPoint + (atCharacterUp * SecondaryProbesVertical) + (innerHitDirection * SecondaryProbesHorizontal), 
                    -atCharacterUp,
                    ledgeCheckHeight + SecondaryProbesVertical, 
                    out innerLedgeHit, 
                    _internalCharacterHits) > 0)
                {
                    stabilityReport.InnerNormal = innerLedgeHit.normal;
                    isStableLedgeInner = IsStableOnNormal(innerLedgeHit.normal);
                }

                RaycastHit outerLedgeHit;
                if (CharacterCollisionsRaycast(
                    hitPoint + (atCharacterUp * SecondaryProbesVertical) + (-innerHitDirection * SecondaryProbesHorizontal), 
                    -atCharacterUp,
                    ledgeCheckHeight + SecondaryProbesVertical, 
                    out outerLedgeHit, 
                    _internalCharacterHits) > 0)
                {
                    stabilityReport.OuterNormal = outerLedgeHit.normal;
                    isStableLedgeOuter = IsStableOnNormal(outerLedgeHit.normal);
                }
                
                stabilityReport.LedgeDetected = (isStableLedgeInner != isStableLedgeOuter);
                if (stabilityReport.LedgeDetected)
                {
                    stabilityReport.IsOnEmptySideOfLedge = isStableLedgeOuter && !isStableLedgeInner;
                    stabilityReport.LedgeGroundNormal = isStableLedgeOuter ? outerLedgeHit.normal : innerLedgeHit.normal;
                    stabilityReport.LedgeRightDirection = Vector3.Cross(hitNormal, outerLedgeHit.normal).normalized;
                    stabilityReport.LedgeFacingDirection = Vector3.Cross(stabilityReport.LedgeGroundNormal, stabilityReport.LedgeRightDirection).normalized;
                    stabilityReport.DistanceFromLedge = Vector3.ProjectOnPlane((hitPoint - (atCharacterPosition + (atCharacterRotation * CharacterTransformToCapsuleBottom))), atCharacterUp).magnitude;
                }
            }

            // Final stability evaluation
            if (isStableOnNormal || stabilityReport.ValidStepDetected)
            {
                stabilityReport.IsStable = true;
            }
            
            CharacterController.ProcessHitStabilityReport(hitCollider, hitNormal, hitPoint, atCharacterPosition, atCharacterRotation, ref stabilityReport);
        }

        private void DetectSteps(Vector3 characterPosition, Quaternion characterRotation, Vector3 hitPoint, Vector3 innerHitDirection, ref HitStabilityReport stabilityReport)
        {
            int nbStepHits = 0;
            Collider tmpCollider;
            RaycastHit outerStepHit;
            Vector3 characterUp = characterRotation * Vector3.up;
            Vector3 stepCheckStartPos = characterPosition;
            
            // Do outer step check with capsule cast on hit point
            stepCheckStartPos = characterPosition + (characterUp * MaxStepHeight) + (-innerHitDirection * CapsuleRadius);
            nbStepHits = CharacterCollisionsSweep(
                        stepCheckStartPos,
                        characterRotation,
                        -characterUp,
                        MaxStepHeight - CollisionOffset,
                        out outerStepHit,
                        _internalCharacterHits);

            // Check for overlaps and obstructions at the hit position
            if(CheckStepValidity(nbStepHits, characterPosition, characterRotation, innerHitDirection, stepCheckStartPos, out tmpCollider))
            {
                stabilityReport.ValidStepDetected = true;
                stabilityReport.SteppedCollider = tmpCollider;
            }

            if (StepHandling == StepHandlingMethod.Extra && !stabilityReport.ValidStepDetected)
            {
                // Do min reach step check with capsule cast on hit point
                stepCheckStartPos = characterPosition + (characterUp * MaxStepHeight) + (-innerHitDirection * MinRequiredStepDepth);
                nbStepHits = CharacterCollisionsSweep(
                            stepCheckStartPos,
                            characterRotation,
                            -characterUp,
                            MaxStepHeight - CollisionOffset,
                            out outerStepHit,
                            _internalCharacterHits);

                // Check for overlaps and obstructions at the hit position
                if (CheckStepValidity(nbStepHits, characterPosition, characterRotation, innerHitDirection, stepCheckStartPos, out tmpCollider))
                {
                    stabilityReport.ValidStepDetected = true;
                    stabilityReport.SteppedCollider = tmpCollider;
                }
            }
        }

        private bool CheckStepValidity(int nbStepHits, Vector3 characterPosition, Quaternion characterRotation, Vector3 innerHitDirection, Vector3 stepCheckStartPos, out Collider hitCollider)
        {
            hitCollider = null;
            Vector3 characterUp = characterRotation * Vector3.up;

            // Find the farthest valid hit for stepping
            bool foundValidStepPosition = false;
            while (nbStepHits > 0 && !foundValidStepPosition)
            {
                // Get farthest hit among the remaining hits
                RaycastHit farthestHit = new RaycastHit();
                float farthestDistance = 0f;
                int farthestIndex = 0;
                for (int i = 0; i < nbStepHits; i++)
                {
                    if (_internalCharacterHits[i].distance > farthestDistance)
                    {
                        farthestDistance = _internalCharacterHits[i].distance;
                        farthestHit = _internalCharacterHits[i];
                        farthestIndex = i;
                    }
                }

                Vector3 characterBottom = characterPosition + (characterRotation * CharacterTransformToCapsuleBottom);
                float hitHeight = Vector3.Project(farthestHit.point - characterBottom, characterUp).magnitude;

                Vector3 characterPositionAtHit = stepCheckStartPos + (-characterUp * (farthestHit.distance - CollisionOffset));

                if (hitHeight <= MaxStepHeight)
                {
                    int atStepOverlaps = CharacterCollisionsOverlap(characterPositionAtHit, characterRotation, _internalProbedColliders);
                    if (atStepOverlaps <= 0)
                    {
                        // Check for outer hit slope normal stability at the step position
                        RaycastHit outerSlopeHit;
                        if (CharacterCollisionsRaycast(
                                farthestHit.point + (characterUp * SecondaryProbesVertical) + (-innerHitDirection * SecondaryProbesHorizontal),
                                -characterUp,
                                MaxStepHeight + SecondaryProbesVertical,
                                out outerSlopeHit,
                                _internalCharacterHits) > 0)
                        {
                            if (IsStableOnNormal(outerSlopeHit.normal))
                            {
                                // Cast upward to detect any obstructions to moving there
                                RaycastHit tmpUpObstructionHit;
                                if (CharacterCollisionsSweep(
                                                    characterPosition, // position
                                                    characterRotation, // rotation
                                                    characterUp, // direction
                                                    MaxStepHeight - farthestHit.distance, // distance
                                                    out tmpUpObstructionHit, // closest hit
                                                    _internalCharacterHits) // all hits
                                        <= 0)
                                {
                                    // Do inner step check...
                                    bool innerStepValid = false;
                                    RaycastHit innerStepHit;

                                    // At the capsule center at the step height
                                    if (CharacterCollisionsRaycast(
                                        characterPosition + Vector3.Project((characterPositionAtHit - characterPosition), characterUp),
                                        -characterUp,
                                        MaxStepHeight,
                                        out innerStepHit,
                                        _internalCharacterHits) > 0)
                                    {
                                        if (IsStableOnNormal(innerStepHit.normal))
                                        {
                                            innerStepValid = true;
                                        }
                                    }

                                    if (!innerStepValid)
                                    {
                                        // At inner step of the step point
                                        if (CharacterCollisionsRaycast(
                                            farthestHit.point + (innerHitDirection * SecondaryProbesHorizontal),
                                            -characterUp,
                                            MaxStepHeight,
                                            out innerStepHit,
                                            _internalCharacterHits) > 0)
                                        {
                                            if (IsStableOnNormal(innerStepHit.normal))
                                            {
                                                innerStepValid = true;
                                            }
                                        }
                                    }

                                    if (!innerStepValid)
                                    {
                                        // At the current ground point at the step height
                                    }

                                    // Final validation of step
                                    if (innerStepValid)
                                    {
                                        hitCollider = farthestHit.collider;
                                        foundValidStepPosition = true;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }

                // Discard hit if not valid step
                if (!foundValidStepPosition)
                {
                    nbStepHits--;
                    if (farthestIndex < nbStepHits)
                    {
                        _internalCharacterHits[farthestIndex] = _internalCharacterHits[nbStepHits];
                    }
                }
            }
            
            return false;
        }

        /// <summary>
        /// Get true linear velocity (taking into account rotational velocity) on a given point of a rigidbody
        /// </summary>
        public Vector3 GetVelocityFromRigidbodyMovement(Rigidbody interactiveRigidbody, Vector3 atPoint, float deltaTime)
        {
            if (deltaTime > 0f)
            {
                Vector3 effectiveMoverVelocity = interactiveRigidbody.velocity;

                if (interactiveRigidbody.angularVelocity != Vector3.zero)
                {
                    Vector3 centerOfRotation = interactiveRigidbody.position + interactiveRigidbody.centerOfMass;

                    Vector3 centerOfRotationToPoint = atPoint - centerOfRotation;
                    Quaternion rotationFromInteractiveRigidbody = Quaternion.Euler(Mathf.Rad2Deg * interactiveRigidbody.angularVelocity * deltaTime);
                    Vector3 finalPointPosition = centerOfRotation + (rotationFromInteractiveRigidbody * centerOfRotationToPoint);
                    effectiveMoverVelocity += (finalPointPosition - atPoint) / deltaTime;
                }
                return effectiveMoverVelocity;
            }
            else
            {
                return Vector3.zero;
            }
        }

        /// <summary>
        /// Determines if a collider has an attached interactive rigidbody
        /// </summary>
        private Rigidbody GetInteractiveRigidbody(Collider onCollider)
        {
            if (onCollider.attachedRigidbody)
            {
                if (onCollider.attachedRigidbody.gameObject.GetComponent<PhysicsMover>())
                {
                    return onCollider.attachedRigidbody;
                }

                if (!onCollider.attachedRigidbody.isKinematic)
                {
                    return onCollider.attachedRigidbody;
                }
            }
            return null;
        }

        /// <summary>
        /// Calculates the velocity required to move the character to the target position over a specific deltaTime.
        /// Useful for when you wish to work with positions rather than velocities in the UpdateVelocity callback of BaseCharacterController
        /// </summary>
        public Vector3 GetVelocityForMovePosition(Vector3 fromPosition, Vector3 toPosition, float deltaTime)
        {
            if (deltaTime > 0)
            {
                return (toPosition - fromPosition) / deltaTime;
            }
            else
            {
                return Vector3.zero;
            }
        }

        /// <summary>
        /// Trims a vector to make it restricted against a plane
        /// </summary>
        private void RestrictVectorToPlane(ref Vector3 vector, Vector3 toPlane)
        {
            if (vector.x > 0 != toPlane.x > 0)
            {
                vector.x = 0;
            }
            if (vector.y > 0 != toPlane.y > 0)
            {
                vector.y = 0;
            }
            if (vector.z > 0 != toPlane.z > 0)
            {
                vector.z = 0;
            }
        }

        /// <summary>
        /// Detect if the character capsule is overlapping with anything collidable
        /// </summary>
        /// <returns> Returns number of overlaps </returns>
        public int CharacterCollisionsOverlap(Vector3 atPosition, Quaternion atRotation, Collider[] overlappedColliders, float radiusInflate = 0f)
        {
            int nbHits = 0;
            int nbUnfilteredHits = Physics.OverlapCapsuleNonAlloc(
                        atPosition + (atRotation * CharacterTransformToCapsuleBottomHemi),
                        atPosition + (atRotation * CharacterTransformToCapsuleTopHemi),
                        Capsule.radius + radiusInflate,
                        overlappedColliders,
                        CollidableLayers,
                        QueryTriggerInteraction.Ignore);

            // Filter out invalid colliders
            nbHits = nbUnfilteredHits;
            for (int i = nbUnfilteredHits - 1; i >= 0; i--)
            {
                if (!CheckIfColliderValidForCollisions(overlappedColliders[i]))
                {
                    nbHits--;
                    if (i < nbHits)
                    {
                        overlappedColliders[i] = overlappedColliders[nbHits];
                    }
                }
            }

            return nbHits;
        }

        /// <summary>
        /// Detect if the character capsule is overlapping with anything
        /// </summary>
        /// <returns> Returns number of overlaps </returns>
        public int CharacterOverlap(Vector3 atPosition, Quaternion atRotation, Collider[] overlappedColliders, LayerMask layers, QueryTriggerInteraction triggerInteraction, float radiusInflate = 0f)
        {
            int nbHits = 0;
            int nbUnfilteredHits = Physics.OverlapCapsuleNonAlloc(
                        atPosition + (atRotation * CharacterTransformToCapsuleBottomHemi),
                        atPosition + (atRotation * CharacterTransformToCapsuleTopHemi),
                        Capsule.radius + radiusInflate,
                        overlappedColliders,
                        layers,
                        triggerInteraction);

            // Filter out the character capsule itself
            nbHits = nbUnfilteredHits;
            for (int i = nbUnfilteredHits - 1; i >= 0; i--)
            {
                if (overlappedColliders[i] == Capsule)
                {
                    nbHits--;
                    if (i < nbHits)
                    {
                        overlappedColliders[i] = overlappedColliders[nbHits];
                    }
                }
            }

            return nbHits;
        }

        /// <summary>
        /// Sweeps the capsule's volume to detect collision hits
        /// </summary>
        /// <returns> Returns the number of hits </returns>
        public int CharacterCollisionsSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, float radiusInflate = 0f)
        {
            direction.Normalize();
            
            // Capsule cast
            int nbHits = 0;
            int nbUnfilteredHits = Physics.CapsuleCastNonAlloc(
                    position + (rotation * CharacterTransformToCapsuleBottomHemi) - (direction * SweepProbingBackstepDistance),
                    position + (rotation * CharacterTransformToCapsuleTopHemi) - (direction * SweepProbingBackstepDistance),
                    Capsule.radius + radiusInflate,
                    direction,
                    hits,
                    distance + SweepProbingBackstepDistance,
                    CollidableLayers,
                    QueryTriggerInteraction.Ignore);

            // Hits filter
            closestHit = new RaycastHit();
            float closestDistance = Mathf.Infinity;
            nbHits = nbUnfilteredHits;
            for (int i = nbUnfilteredHits - 1; i >= 0; i--)
            {
                hits[i].distance -= SweepProbingBackstepDistance;

                // Filter out the invalid hits
                if (hits[i].distance <= 0f ||
                    !CheckIfColliderValidForCollisions(hits[i].collider))
                {
                    nbHits--;
                    if (i < nbHits)
                    {
                        hits[i] = hits[nbHits];
                    }
                }
                else
                {
                    // Remember closest valid hit
                    if (hits[i].distance < closestDistance)
                    {
                        closestHit = hits[i];
                        closestDistance = hits[i].distance;
                    }
                }
            }

            return nbHits;
        }

        /// <summary>
        /// Sweeps the capsule's volume to detect hits
        /// </summary>
        /// <returns> Returns the number of hits </returns>
        public int CharacterSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, LayerMask layers, QueryTriggerInteraction triggerInteraction, float radiusInflate = 0f)
        {
            direction.Normalize();
            closestHit = new RaycastHit();

            // Capsule cast
            int nbHits = 0;
            int nbUnfilteredHits = Physics.CapsuleCastNonAlloc(
                position + (rotation * CharacterTransformToCapsuleBottomHemi),
                position + (rotation * CharacterTransformToCapsuleTopHemi),
                Capsule.radius + radiusInflate,
                direction,
                hits,
                distance,
                layers,
                triggerInteraction);

            // Hits filter
            float closestDistance = Mathf.Infinity;
            nbHits = nbUnfilteredHits;
            for (int i = nbUnfilteredHits - 1; i >= 0; i--)
            {
                // Filter out the character capsule
                if (hits[i].distance <= 0f || hits[i].collider == Capsule)
                {
                    nbHits--;
                    if (i < nbHits)
                    {
                        hits[i] = hits[nbHits];
                    }
                }
                else
                {
                    // Remember closest valid hit
                    if (hits[i].distance < closestDistance)
                    {
                        closestHit = hits[i];
                        closestDistance = hits[i].distance;
                    }
                }
            }

            return nbHits;
        }

        /// <summary>
        /// Casts the character volume in the character's downward direction to detect ground
        /// </summary>
        /// <returns> Returns the number of hits </returns>
        private bool CharacterGroundSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit)
        {
            direction.Normalize();
            closestHit = new RaycastHit();

            // Capsule cast
            int nbUnfilteredHits = Physics.CapsuleCastNonAlloc(
                position + (rotation * CharacterTransformToCapsuleBottomHemi) - (direction * GroundProbingBackstepDistance),
                position + (rotation * CharacterTransformToCapsuleTopHemi) - (direction * GroundProbingBackstepDistance),
                Capsule.radius,
                direction,
                _internalCharacterHits,
                distance + GroundProbingBackstepDistance,
                CollidableLayers,
                QueryTriggerInteraction.Ignore);

            // Hits filter
            bool foundValidHit = false;
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < nbUnfilteredHits; i++)
            {
                // Find the closest valid hit
                if (_internalCharacterHits[i].distance > 0f && CheckIfColliderValidForCollisions(_internalCharacterHits[i].collider))
                {
                    if (_internalCharacterHits[i].distance < closestDistance)
                    {
                        closestHit = _internalCharacterHits[i];
                        closestHit.distance -= GroundProbingBackstepDistance;
                        closestDistance = _internalCharacterHits[i].distance;

                        foundValidHit = true;
                    }
                }
            }

            return foundValidHit;
        }

        /// <summary>
        /// Raycasts to detect collision hits
        /// </summary>
        /// <returns> Returns the number of hits </returns>
        public int CharacterCollisionsRaycast(Vector3 position, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits)
        {
            direction.Normalize();

            // Raycast
            int nbHits = 0;
            int nbUnfilteredHits = Physics.RaycastNonAlloc(
                position,
                direction,
                hits,
                distance,
                CollidableLayers,
                QueryTriggerInteraction.Ignore);

            // Hits filter
            closestHit = new RaycastHit();
            float closestDistance = Mathf.Infinity;
            nbHits = nbUnfilteredHits;
            for (int i = nbUnfilteredHits - 1; i >= 0; i--)
            {
                // Filter out the invalid hits
                if (hits[i].distance <= 0f ||
                    !CheckIfColliderValidForCollisions(hits[i].collider))
                {
                    nbHits--;
                    if (i < nbHits)
                    {
                        hits[i] = hits[nbHits];
                    }
                }
                else
                {
                    // Remember closest valid hit
                    if (hits[i].distance < closestDistance)
                    {
                        closestHit = hits[i];
                        closestDistance = hits[i].distance;
                    }
                }
            }

            return nbHits;
        }
    }
}
using UnityEngine;
using Balla.Core;
using System.Collections;
using Unity.Netcode;
using System;
namespace Balla.Gameplay.Player
{
    public enum MovementState
    {
        None = 0,
        Walk = 1,
        Crouch = 2,
        Sprint = 4,
        Slide = 8,
        Air = 16,
        Ladder = 32,
        Mantle = 64,
        Special = 128,
    }


    /// <summary>
    /// A rigidbody-based character controller that floats the character above the ground.
    /// <br></br>By floating the character, it makes several things much simpler:
    /// <br></br>> Force against gravity on slopes does not need to be implemented, because the player does not contact slopes.
    /// <br></br>> The character controller feels slightly more responsive, and hitting the ground feels pretty good too.
    /// <br></br>> Stepping up low steps is simplified, meaning players can walk up stairs easily.
    /// <br></br> And additional information can be extracted from the ground the player walks on.
    /// <br></br> The Player Controller will handle almost all aspects of motion. It will also provde information for motion behaviours that are not handled by this script.
    /// <br></br> Examples include Ziplines and travelling via portals.
    /// </summary>
    public class PlayerMotor : BallaNetScript
    {
        [SerializeField] internal Rigidbody rb;
        [SerializeField] internal CapsuleCollider capsule;



        [SerializeField, ReadOnly, Tooltip("Obtained from Camera.Main if this player is the local authority.")] internal Camera cam;
        [SerializeField, Tooltip("Where to Move the camera to when updating the player.")] internal Transform camTargetPoint;
        protected Vector3 _camPosOld;
        protected Quaternion _camRotOld;
        public MovementState moveState;
        [SerializeField, Tooltip("The transform moved when the player crouches")] internal Transform crouchTransform;

        protected NetworkVariable<bool> crouched = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        bool _lastCrouch;
        #region Looking

        [SerializeField, Tooltip("The transform used to aim up and down with")] internal Transform aimTransform;
        [SerializeField, Tooltip("The transform rotated when the player looks left and right")] internal Transform rotationRoot;
        internal Vector2 lookDelta;
        internal float pitch;
        #endregion
        //--------------------------
        //--------------------------
        //--------------------------
        #region SimpleMove
        [Header("Ground Movement")]
        /// <summary>
        /// 
        /// </summary>
        [SerializeField, Tooltip("How much force is applied when moving forwards")]
        protected float forwardForce;
        /// <summary>
        /// 
        /// </summary>
        [SerializeField, Tooltip("How much force is applied while moving sideways")]
        protected float strafeForce;
        /// <summary>
        /// 
        /// </summary>
        [SerializeField, Tooltip("How much force is applied while moving backwards")]
        protected float backForce;
        /// <summary>
        /// 
        /// </summary>
        [SerializeField, Tooltip("Should we use the forward move force for all directions?")]
        protected bool unifiedMoveForce;
        /// <summary>
        /// 
        /// </summary>
        [SerializeField, Tooltip("How much damping is applied on the ground?")]
        protected float baseGroundDamping;

        [SerializeField, Tooltip("How much much more force should be applied while sprinting?")]
        protected float sprintForceMultiplier = 1.5f;

        [Header("Air Movement")]
        /// <summary>
        /// 
        /// </summary>
        [SerializeField, Tooltip("How much force is applied while moving in the air")]
        protected float airMoveForce;
        /// <summary>
        /// 
        /// </summary>
        [SerializeField, Tooltip("How much velocity damping is applied while in the air")]
        protected float baseAirDamping;
        #endregion
        //--------------------------
        //--------------------------
        //--------------------------
        #region Jump
        [SerializeField, Tooltip("How much force is applied while jumping")]
        protected float jumpSpeed;
        [SerializeField, Tooltip("How often we can jump?\nAlso controls the delay before we can be considered \"grounded\" again after jumping.")]
        protected float jumpCooldown;
        public int maxAirJumpCount;
        protected int airJumpCount;
        protected float currJumpCD;
        #endregion
        //--------------------------
        //--------------------------
        //--------------------------
        #region Crouch
        /// <summary>
        /// Is the player currently crouching?
        /// </summary>
        [Header("Crouching"), ReadOnly, SerializeField, Tooltip("Is the player currently crouched?")]
        internal bool isCrouching;
        /// <summary>
        /// How crouched the player is. This value moves between 0 and 1 when the player is crouching or uncrouching.
        /// </summary>
        [SerializeField, ReadOnly] internal float currentCrouch;

        [SerializeField, Tooltip("How much faster/slower the player moves while crouched")] protected float crouchMoveForceMult;
        /// <summary>
        /// The height of the player's head when standing, in local space. Crouching interpolates between this and <see cref="crouchHeadHeight"/>
        /// </summary>
        [SerializeField, Tooltip("The height of the player's aimTransform when standing, in local space.")] protected float standHeadHeight;
        /// <summary>
        /// How much shorter the player gets when crouching.
        /// <br></br>This is subtracted from <see cref="standHeadHeight"/>
        /// </summary>
        [SerializeField, Tooltip("The height of the player's aimTransform when crouched, in local space.")] protected float crouchShrinkFactor;
        /// <summary>
        /// The height of the player while crouching.
        /// </summary>
        [SerializeField, ReadOnly, Tooltip("The height of the player's aimTransform when crouched, in local space.")] protected float crouchHeadHeight;
        /// <summary>
        /// How long it takes for the player's head and capsule to lerp towards the target. Higher values here make you crouch slower.
        /// </summary>
        [SerializeField, Tooltip("How long crouching takes.")] protected float crouchTime;
        [SerializeField, ReadOnly] protected float crouchIncrement;
        /// <summary>
        /// How tall the player's capsule is when standing. Try to account for this in the stand height.
        /// </summary>
        [SerializeField, Tooltip("How tall the player's capsule is when standing.")] protected float standCapsuleHeight;
        /// <summary>
        /// How much the player's capsule shrinks when they crouch
        /// </summary>
        [SerializeField, Tooltip("How tall the player's capsule is when crouching.")] protected float capsuleCrouchShrink;
        /// <summary>
        /// How tall the player's capsule is when crouching
        /// </summary>
        [SerializeField, ReadOnly, Tooltip("How tall the player's capsule is when crouching.")] protected float crouchCapsuleHeight;
        /// <summary>
        /// How high the player's capsule is placed when standing. 
        /// <br></br>This uses <see cref="Vector3.up"/> * this value to determine the value.
        /// </summary>
        [SerializeField, Tooltip("How high the player's capsule is placed when standing")] protected float standCapsulePosition;
        /// <summary>
        /// How high the player's capsule is placed when crouching. 
        /// <br></br>This uses <see cref="Vector3.up"/> * this value to determine the value.
        /// </summary>
        [SerializeField, ReadOnly, Tooltip("How high the player's capsule is placed when crouching")] protected float crouchCapsulePosition;
        #endregion
        //--------------------------
        //--------------------------
        //--------------------------
        #region Sliding
        [Header("Sliding")]
        [SerializeField, ReadOnly] bool sliding;
        public float slideStartForce = 3f;
        public float slideDamp = .5f;
        public float slideMinSpeed = 2f;
        public float slideFOV = 5;
        #endregion Sliding
        //--------------------------
        //--------------------------
        //--------------------------
        #region Grounding
        /// <summary>
        /// 
        /// </summary>
        [Header("Ground Check"), SerializeField, Tooltip("How many rays are cast in the ground check")]
        protected int groundCheckRays;
        [SerializeField, Tooltip("The origin point of the ground check rays")]
        protected Transform groundCheckOrigin;
        [SerializeField, Tooltip("How wide the ground check rays should be cast")]
        protected float groundCheckRadius;
        [SerializeField, Tooltip("How far to cast the ground check rays")]
        protected float groundCheckDistance;
        int _gRayCount;
        float _gRayRad;

        [ReadOnly, SerializeField, Tooltip("The angle between each ray that is cast. Not editable directly.")]
        protected float groundCheckAngle;

        [SerializeField, Tooltip("")]
        protected float groundPositionOffset;
        [SerializeField, Tooltip("")]
        protected float targetHeight;
        [SerializeField, Tooltip("")]
        protected float groundSpringRestLength;
        [SerializeField, Tooltip("")]
        protected float groundSpringTravel;

        [SerializeField, Tooltip("")]
        protected LayerMask groundMask;
        protected float _maxSpringLength;
        protected float groundSpringForce;
        [SerializeField, Tooltip("")]
        protected float groundSpringDamper;
        [SerializeField, Tooltip("")]
        protected float groundSpringStrength;

        protected float[] _gSpringOffset, _lastSpringOffset, _springSpeed;
        protected Vector3 groundNormal;
        protected Vector3[] groundCheckPositions;
        protected Vector3[] _springPos, _springHitPos;
        protected float[] _springLengths;
        protected RaycastHit[] groundHits;


        Vector3 slopeForward, slopeForwardFull, slopeRight, slopeRightFull, slopeDirection;
        Vector3 dampRight, dampForward, moveForward, moveRight;
        /// <summary>
        /// The player's velocity projected against the surface they are standing on.
        /// <br></br>This will usually be similar to their velocity (with some acceptable variations, probably) whilst they are walking/running.
        /// <br></br>
        /// </summary>
        protected Vector2 slopeAlignedVelocity;


        [ReadOnly, SerializeField]
        internal bool isGrounded;
        #endregion
        //--------------------------
        //--------------------------
        //--------------------------
        #region Surface Motion
        [SerializeField] protected float surfaceMotionCastLength;
        [SerializeField]
        protected Rigidbody connectedBody, lastConnectedBody;
        Vector3 connectionVelocity, connectionWorldPos, connectionLocalPos;
        float connectionDeltaYaw, connectionYaw, connectionLastYaw;
        [SerializeField] protected float surfaceMotionCentripetalMultiplier = 1;
        #endregion
        //--------------------------
        //--------------------------
        //--------------------------
        #region Climbing
        [Header("Climbing")]
        public bool canClimb;
        public Transform climbCastOrigin;
        public float climbSpeed = 2;
        public float maxClimbDistance = .75f;
        public float maxClimbHeight = 1.5f;
        public float climbBlockHeightOffset = -0.1f;
        public bool canClimbToCrouch;
        public float climbSpaceToStand = 1.9f;
        public float climbSpaceToCrouch = 1f;
        public float climbToCrouchHeight = .6f;
        public float climbHeightOffset = 1;
        public float climbForwardOffset = 0.5f;
        public CapsuleCollider climbCastCapsule;
        public AnimationCurve climbVerticalCurve, climbLateralCurve;
        [ReadOnly, SerializeField] Vector3 climbEndPoint;
        [ReadOnly, SerializeField] bool isClimbing;
        [ReadOnly, SerializeField] float climbRate;


        #endregion
        //--------------------------
        //--------------------------
        //--------------------------
        #region Events
        /// <summary>
        /// Invoked when the player lands, passing the velocity when landing as the parameter
        /// </summary>
        public Action<float> OnLand;
        /// <summary>
        /// Invoked when the player jumps. Argument is true when double jumping. Ignore if not using double jump.
        /// </summary>
        public Action<bool> OnJump;
        /// <summary>
        /// Invoked when the player starts or stops crouching. Argument is crouch state
        /// </summary>
        public Action<bool> OnCrouchChanged;
        /// <summary>
        /// Invoked when the player starts sliding
        /// </summary>
        public Action OnSlideStart;
        /// <summary>
        /// Invoked when the player stops slidings
        /// </summary>
        public Action OnSlideEnd;
        /// <summary>
        /// Invoked when the player starts climbing on something
        /// </summary>
        public Action OnClimb;
        #endregion Events
        #region Unity Methods
        private void Start()
        {
            ConfigureGroundCheckPositions();
        }
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                cam = Camera.main;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnValidate()
        {
            if (_gRayCount != groundCheckRays || _gRayRad != groundCheckRadius)
            {
                if (groundCheckOrigin)
                {
                    groundCheckRays = Mathf.Max(groundCheckRays, 1);
                    groundCheckAngle = 360 / groundCheckRays;
                    ConfigureGroundCheckPositions();
                    _gRayCount = groundCheckRays;
                    _gRayRad = groundCheckRadius;
                }
                if (_springLengths.Length != _gRayCount)
                {
                    _springLengths = new float[groundCheckRays];
                }
            }
            _maxSpringLength = groundSpringRestLength + groundSpringTravel;
            if (capsule != null)
            {
                standCapsuleHeight = capsule.height;
                standCapsulePosition = capsule.center.y;

                crouchCapsuleHeight = standCapsuleHeight - capsuleCrouchShrink;
                crouchCapsulePosition = standCapsulePosition - capsuleCrouchShrink / 2;
            }
            if (aimTransform != null)
            {
                standHeadHeight = crouchTransform.localPosition.y;
                crouchHeadHeight = standHeadHeight - crouchShrinkFactor;
            }
            crouchIncrement = (1 / crouchTime);
        }
        void ConfigureGroundCheckPositions()
        {
            groundCheckPositions = new Vector3[groundCheckRays];
            _maxSpringLength = groundSpringRestLength + groundSpringTravel;
            for (int i = 0; i < groundCheckRays; i++)
            {
                groundCheckPositions[i] = Quaternion.Euler(0, groundCheckAngle * i, 0) * transform.forward * groundCheckRadius
                    + groundCheckOrigin.localPosition;
            }
            _springPos = new Vector3[groundCheckRays];
            _springLengths = new float[groundCheckRays];
            _springSpeed = new float[groundCheckRays];
            _lastSpringOffset = new float[groundCheckRays];
            _gSpringOffset = new float[groundCheckRays];
            groundHits = new RaycastHit[groundCheckRays];
        }
        private void OnDrawGizmos()
        {
            Gizmos.matrix = Matrix4x4.identity;
            if (groundCheckOrigin != null && groundCheckPositions != null)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < groundCheckRays; i++)
                {
                    Gizmos.DrawRay(transform.position + (transform.rotation * groundCheckPositions[i]),
                        Vector3.down * (_maxSpringLength + groundPositionOffset));
                }
            }
            if (climbCastOrigin != null)
            {
                Vector3 forwardPoint = Vector3.forward * maxClimbDistance;
                Gizmos.matrix = climbCastOrigin.localToWorldMatrix;
                //Forward cast Ray
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(Vector3.zero, Vector3.forward * maxClimbDistance);

                //Ground below cast
                Gizmos.color = Color.red;
                Gizmos.DrawRay(forwardPoint, Vector3.up * maxClimbHeight);

                //Height block ray
                Gizmos.color = Color.green;
                Gizmos.DrawRay((Vector3.forward * (maxClimbDistance - 0.02f)) - (Vector3.up * climbBlockHeightOffset), Vector3.up * (maxClimbHeight - climbBlockHeightOffset));


            }
        }
        #endregion

        #region LunarScript Overrides
        protected override void Timestep()
        {
            if (!IsOwner)
            {
                Crouch(crouched.Value);
                return;
            }
            CheckState();
            Crouch(isCrouching);
            if (!isClimbing && currJumpCD >= jumpCooldown)
            {
                CheckGround();
            }
            HandleMotion();
        }
        protected override void AfterFrame()
        {
            if (!IsOwner)
            {
                return;
            }
            base.AfterFrame();
            if (Input.Look != Vector2.zero)
            {
                RotatePlayer();
            }
            UpdateCamera();
        }
        #endregion



        #region Motion
        /// <summary>
        /// This method performs the ground check raycasts and gathers information about the surface they are on.<br></br>
        /// The calculations for the ground spring are also performed in this method.<br></br>
        /// It will return some helpful information from the surface such as:
        /// <br></br>* The friction of the surface the player is standing on
        /// <br></br>* The normal of the surface the player is standing on
        /// <br></br>This method will be ignored if a Special Movement type is to ignore the Ground Check, such as grappling (which implements its own simple ground/collision check)
        /// <br></br>or using a quick dash, where additional "friction" from the ground may be undesirable or where being on the ground has no impact on the dash.
        /// </summary>
        protected void CheckGround()
        {
            isGrounded = false;
            groundSpringForce = 0;
            groundNormal = Vector3.up;
            _springHitPos = new Vector3[groundCheckRays];
            for (int i = 0; i < groundCheckRays; i++)
            {
                _springLengths[i] = 0;
                bool hit = Physics.Raycast(transform.TransformPoint(groundCheckPositions[i]), -transform.up, out groundHits[i], _maxSpringLength + groundPositionOffset,
                    groundMask, QueryTriggerInteraction.Ignore) && Vector3.Dot(groundHits[i].normal, transform.up) >= 0.4f;

                Debug.DrawLine(transform.TransformPoint(groundCheckPositions[i]), hit ? groundHits[i].point : (transform.TransformPoint(groundCheckPositions[i]) + (transform.up * -_maxSpringLength)),
                    hit ? Color.green : Color.red, Time.fixedDeltaTime);
                if (hit)
                {
                    _springHitPos[i] = groundHits[i].point;
                    _springLengths[i] = groundHits[i].distance - groundPositionOffset;
                    _gSpringOffset[i] = (groundSpringRestLength - _springLengths[i]) / groundSpringTravel;

                    _springSpeed[i] = Vector3.Dot(transform.up, rb.GetPointVelocity(_springPos[i]));

                    _springPos[i] = Vector3.up * Mathf.Clamp(groundSpringRestLength - _springLengths[i], -groundSpringTravel, groundSpringTravel);
                    groundNormal += groundHits[i].normal;
                    groundSpringForce += (groundSpringStrength * _gSpringOffset[i]) - (groundSpringDamper * _springSpeed[i]);
                    isGrounded = true;
                }
            }
            if (isGrounded)
            {
                groundNormal.Normalize();
                rb.AddForce(transform.up * groundSpringForce);
                TransformSurfaceNormal();
                airJumpCount = maxAirJumpCount;
            }
            InheritSurfaceMotion();
        }

        /// <summary>
        /// Runs through a series of parameters to determine which state the player is currently in.
        /// </summary>
        protected void CheckMoveState()
        {
            //We need to figure out what our current move state is.
            //We can only override it if we don't currently have a special MoveState
            if (moveState == MovementState.Special)
                return;

            if (isClimbing)
            {
                //If we're climbing, this will override everything else.
                moveState = MovementState.Mantle;
                return;
            }
            if (isGrounded)
            {
                //If we are NOT sliding, we're moving above the min slide speed, we're currently sprinting AND we have pressed the crouch button, we should start our sprint.
                if (sliding)
                {
                    if (rb.linearVelocity.magnitude >= slideMinSpeed)
                    {
                        return;
                    }
                    else
                    {
                        EndSlide();
                        return;
                    }
                }
                else
                {
                    if (moveState == MovementState.Sprint && Input.Crouch)
                    {
                        StartSlide();
                        moveState = MovementState.Slide;
                        return;
                    }
                    if (isCrouching)
                    {
                        moveState = MovementState.Crouch;
                        return;
                    }
                    if (Input.Sprint && Input.Move.y >= 0.7f)
                    {
                        moveState = MovementState.Sprint;
                        return;
                    }
                    moveState = MovementState.Walk;
                }
            }
            else
            {
                moveState = MovementState.Air;
            }
        }


        /// <summary>
        /// Called within <see cref="CheckGround"/>, TransformSurfaceNormal calculates all the variations of the surface normal that the player may need.
        /// <br></br>It calculates the following:<br></br>
        /// * The player's velocity, both global and relative to the surface they are standing on<br></br>
        /// * The players forward and right direction relative to the surface they are standing on<br></br>
        /// </summary>
        protected void TransformSurfaceNormal()
        {
            Debug.DrawRay(transform.position, groundNormal, Color.green);
            slopeForwardFull = Vector3.Cross(rotationRoot.right, groundNormal);
            Debug.DrawRay(transform.position, slopeForwardFull, Color.blue);
            slopeRightFull = Vector3.Cross(groundNormal, slopeForwardFull);
            Debug.DrawRay(transform.position, slopeRightFull, Color.red);

            slopeDirection = (slopeRightFull + slopeForwardFull).normalized;
            slopeForward = slopeForwardFull * Vector3.Dot(slopeForwardFull, slopeDirection);
            slopeRight = slopeRightFull * Vector3.Dot(slopeRightFull, slopeDirection);

        }
        /// <summary>
        /// This method determines how the player should move using the ground check information and the player's inputs.
        /// <br></br>This will then call the method to be used for each type of movement. Some may be compounded into one using selection such as moving in the air or on the ground (simple motion)
        /// <br></br>This method handles the following:
        /// <br></br>* Check if the player is performing a "Special Movement" type, which is implemented via the SpecialMovement component.
        /// <br></br>*** If the player is performing a Special Movement, then the rest of the method will be skipped in favour of the SpecialMovement component. 
        /// <br></br>* Check if the player is on the ground.
        /// <br></br>* If on the ground...
        /// <br></br>*** Checking if the player is attempting to crouch, sprint or slide, and set the current move state based on this
        /// <br></br>*** Checking if the player should move simply (See <see cref="SimpleMove"/>)
        /// <br></br>* If in the air...
        /// <br></br>*** the player will most likely only move simply in the air, unless this is interrupted for some reason.
        /// </summary>
        protected void HandleMotion()
        {
            if (!isGrounded && !isClimbing && Input.Move.y > 0.3f)
            {
                CheckClimb();
            }
            CheckMoveState();

            switch (moveState)
            {
                case MovementState.None:
                    break;
                case MovementState.Walk or MovementState.Crouch or MovementState.Sprint or MovementState.Air:
                    SimpleMove();
                    break;
                case MovementState.Slide:
                    SlideUpdate();
                    break;
                case MovementState.Ladder:
                    //We'll later do the climb stuff
                    break;
                case MovementState.Mantle:
                    //Exclude movement logic if we're currently climbing
                    break;
                case MovementState.Special:
                    //Ignore movement logic if this is overridden. We'll look at doing something with this later on.
                    break;
                default:
                    break;
            }
            TryJump();
        }
        void CheckClimb()
        {
            //Cast forwards to see if there's a surface to climb.
            //We'll try using a capsule cast instead of a raycast to catch stuff at all heights
            //Something could be a climbable height, but cut off just above or just below the target point.
            Vector3 origin = climbCastOrigin.position + climbCastCapsule.center;
            Vector3 offset = (transform.up * climbCastCapsule.height / 2);
            Debug.DrawRay(origin, climbCastOrigin.forward, Color.powderBlue * maxClimbDistance, 2);
            if (!Physics.CapsuleCast(origin + offset, origin - offset, climbCastCapsule.radius, climbCastOrigin.forward, out RaycastHit hitFwd, maxClimbDistance, groundMask))
            {
                return;
            }
            //Leave early if we don't hit here.
            Vector3 fwdPoint = climbCastOrigin.position + (transform.forward * (capsule.radius + hitFwd.distance));

            //Return if this one does not hit OR if we don't have enough space to stand/crouch here.
            Debug.DrawRay(fwdPoint + (transform.up * (maxClimbHeight + 0.05f)), -transform.up * (maxClimbHeight + 0.05f), Color.orange, 2);
            if (!Physics.Raycast(fwdPoint + (transform.up * (maxClimbHeight + 0.05f)), -transform.up, out RaycastHit hitDown, maxClimbHeight + 0.05f, groundMask) || hitDown.distance <= 0.04f)
            {
                return;
            }

            //Now we should cast up to see if we have enough space to stand in the spot in front of us.
            Debug.DrawRay(hitDown.point - (transform.up * climbBlockHeightOffset), transform.up * (maxClimbHeight - climbBlockHeightOffset), Color.mediumPurple * maxClimbDistance, 2);
            if (Physics.Raycast(hitDown.point - (transform.up * climbBlockHeightOffset), transform.up, out RaycastHit hitUp, maxClimbHeight - climbBlockHeightOffset, groundMask) && (hitUp.distance - climbBlockHeightOffset) < climbSpaceToCrouch)
            {
                return;
            }
            //If all three are true, we should be able to vault up to this point.
            //We also need to check if there's a rigidbody attached to this component!
            if (hitDown.rigidbody != null)
            {
                if (!hitDown.rigidbody.isKinematic)
                {
                    //We cannot climb non-kinematic rigidbodies
                    return;
                }
                climbTargetRB = hitDown.rigidbody;
            }
                bool crouchOnClimb = Physics.Linecast(hitDown.point, hitDown.point + (transform.up * climbSpaceToStand), groundMask);
            climbEndPoint = hitDown.point + ((transform.up * climbHeightOffset) + (transform.forward * climbForwardOffset));
            Debug.DrawRay(climbEndPoint, Vector3.up, Color.red, 1f);
            StartCoroutine(ClimbToPoint(crouchOnClimb));
        }
        Rigidbody climbTargetRB;
        IEnumerator ClimbToPoint(bool crouchOnClimb)
        {
            //Start climbing!
            isClimbing = true;

            float t = 0;
            bool climbOnBody = climbTargetRB != null;
            rb.isKinematic = true;
            Vector3 start;
            if (climbOnBody)
            {
                start = climbTargetRB.transform.InverseTransformPoint(transform.position);
                climbEndPoint = climbTargetRB.transform.InverseTransformPoint(climbEndPoint);
            }
            else
            {
                start = transform.position;
            }
            isCrouching = crouchOnClimb;
            Vector3 climbPoint = start;
            float dist = Vector3.Distance(start, climbEndPoint);
            climbRate = climbSpeed / dist;
            float lastYaw = 0;
            var wff = new WaitForFixedUpdate();
            while (t < 1 && isClimbing)
            {
                t += climbRate * Time.fixedDeltaTime;
                float hLerp = climbLateralCurve.Evaluate(t);
                climbPoint = new Vector3()
                {
                    x = Mathf.LerpUnclamped(start.x, climbEndPoint.x, hLerp),
                    y = Mathf.LerpUnclamped(start.y, climbEndPoint.y, climbVerticalCurve.Evaluate(t)),
                    z = Mathf.LerpUnclamped(start.z, climbEndPoint.z, hLerp),
                };
                if (climbOnBody)
                {
                    transform.rotation *= Quaternion.Euler(0, climbTargetRB.transform.eulerAngles.y - lastYaw, 0);
                    transform.position = climbTargetRB.transform.TransformPoint(climbPoint);
                    lastYaw = climbTargetRB.transform.eulerAngles.y;
                }
                else
                {
                    transform.position = climbPoint;
                }
                if (crouchOnClimb)
                {
                    currentCrouch = hLerp;
                }
                yield return wff;
            }
            rb.isKinematic = false;
            climbTargetRB = null;
            isClimbing = false;
        }

        protected void CheckState()
        {
            isCrouching = Input.Crouch || sliding || Physics.SphereCast(groundCheckOrigin.position, capsule.radius * 0.9f, transform.up, out _, standHeadHeight + .5f, groundMask);
            if (_lastCrouch != isCrouching)
            {
                _lastCrouch = isCrouching;
                crouched.Value = _lastCrouch;
            }
        }

        /// <summary>
        /// Performs a variety of calculations to determine how the player should move when they are on a surface that also moves.
        /// <br></br>This includes rotating with surfaces the player stands on.
        /// <br></br>Motion Inheritance can be disabled for linear motion, angular motion or both if the developer does not desire for players to move with surfaces.
        /// <br></br>These calculations are applied in a physics-friendly way (for the most part) by casting in the player's direction of motion.
        /// <br></br>This cast will be done with a capsule slightly smaller than the player to ensure it does not start inside another collider.
        /// <br></br>Players will not be able to move through following a surface if they would penetrate another collider, but will continue to rotate with it if enabled.
        /// <br></br>Rotation if movement is not possible could be disabled, however.
        /// </summary>
        protected void InheritSurfaceMotion()
        {
            //Now check if we're on a surface that can move
            if (Physics.Raycast(groundCheckOrigin.position, -transform.up, out RaycastHit hit2, surfaceMotionCastLength, groundMask, QueryTriggerInteraction.Ignore))
            {
                //assign the hit rigidbody to ConnectedBody
                if (connectedBody == null && hit2.rigidbody != null)
                {
                    connectedBody = hit2.rigidbody;
                    connectionLastYaw = connectedBody.rotation.eulerAngles.y;
                    SetConnectionPosition();
                }
                connectedBody = hit2.rigidbody;
            }
            else
            {
                connectedBody = null;
            }
            if (connectedBody == null)
            {
                if (lastConnectedBody != null)
                {
                    rb.AddForce(connectionVelocity, ForceMode.VelocityChange);
                }
                lastConnectedBody = null;
                return;
            }
            DoSurfaceMotion();
        }
        void DoSurfaceMotion()
        {
            Vector3 connectDelta = connectedBody.transform.TransformPoint(connectionLocalPos) - connectionWorldPos;
            connectionVelocity = connectDelta / Delta;

            SetConnectionPosition();


            connectionYaw = connectedBody.rotation.eulerAngles.y;
            connectionDeltaYaw = connectionYaw - connectionLastYaw;
            connectionLastYaw = connectionYaw;

            transform.position += connectDelta;
            transform.rotation *= Quaternion.Euler(0, connectionDeltaYaw, 0);
            //We're also going to apply centripetal force based how fast we're rotating.
            if (connectionDeltaYaw > 0.1f)
            {
                CalculateCentripetalForce();
            }

            lastConnectedBody = connectedBody;
        }

        void SetConnectionPosition()
        {
            connectionWorldPos = rb.position;
            connectionLocalPos = connectedBody.transform.InverseTransformPoint(connectionWorldPos);
        }

        /// <summary>
        /// When standing on a rotating surface, the player slowly moves towards the outside edge of the rotating surface.
        /// <br></br>The cause for this is not known. However, I'm going to attempt (very important word) to solve it here.
        /// <br></br>We only enter this method if we're deemed to at a reasonable speed.
        /// <br></br>The assumption is made that this is not completely physically accurate.
        /// <br></br>It is also assumed that the speed at which the player moves away from the pivot point.
        /// <br></br>
        /// <br></br>The equation for centripetal force (centrifugal force is the reactionary force to centripetal. It's not real.) is
        /// <br></br> F = m * (w^2 / r) - and we have access to all (or most) of this information by some means or another.
        /// 
        /// </summary>
        protected void CalculateCentripetalForce()
        {
            //Lets think about this a little more abstract. We are, in essence, just moving point B around point A, at a distance R.
            //It doesn't matter what the various components of the equation are; its all the same maths.
            //What information do we have?
            //We know our linear velocity at point B, we know where point A is (though that's irrelevant here)

            //We can actually just calculate this bit in local space, avoidiing some extra maths.
            //The magnitude of the position would be the radius, surely?
            float r = new Vector2(connectionLocalPos.x, connectionLocalPos.z).magnitude;
            //We also have the linear velocity of point B. So we can use some maths to get from THAT to our Angular Velocity.
            //The nice thing about Unity is that it uses metres. So we don't have to worry about conversions.
            //Angular velocity is measured in radians per sec, so we may have to convert somewhere down the line, but that's easy too.
            //Angular velocity (W) is calculated by dividing the linear velocity by the radius. We know both of those.
            float v = connectionVelocity.magnitude;
            float w = v / r;
            //Then F (our centripetal force) is calculated by the equation I wrote above:
            //F = m * (w^2 / r)
            //So we can calculate THIS really easily too! wowza!
            float F = rb.mass * ((w * w) / r);
            //Then we calculate the force direction by subtracting our two points from each other, putting them on the same plane to avoid vertical force.
            Vector3 forceDir = (new Vector3(rb.position.x, 0, rb.position.z) - new Vector3(connectedBody.position.x, 0, connectedBody.position.z)).normalized;
            //then apply the force. I'll also add a multiplier in case it isn't *quite* physically accurate. 
            rb.AddForce(F * surfaceMotionCentripetalMultiplier * forceDir);
        }
        #region Movement Methods
        /// <summary>
        /// This method applies forces to the player when they are walking (or some variation thereof) or airborne.
        /// <br></br>If the player is on the ground, the movement direction will be adjusted to the surface they're standing on.
        /// </summary>
        protected void SimpleMove()
        {
            if (isGrounded)
            {
                slopeAlignedVelocity = new(Vector3.Dot(rb.linearVelocity, slopeRightFull), Vector3.Dot(rb.linearVelocity, slopeForwardFull));
                dampRight = slopeAlignedVelocity.x * -slopeRight;
                dampForward = slopeAlignedVelocity.y * -slopeForward;
                //combine the movement (the first half of the argument) and the damping (the second half) additively
                rb.AddForce((dampForward + dampRight) * baseGroundDamping, ForceMode.Acceleration);

                float forceMult = 1;
                switch (moveState)
                {
                    case MovementState.Crouch:
                        forceMult = crouchMoveForceMult;
                        break;
                    case MovementState.Sprint:
                        forceMult = sprintForceMultiplier;
                        break;
                    default:
                        break;
                }
                if (Input.Move != Vector2.zero)
                {
                    //Integrate walk/crouch/sprint later
                    moveRight = Input.Move.x * strafeForce * slopeRight;
                    moveForward = Input.Move.y * (Input.Move.y > 0 ? forwardForce : backForce) * slopeForward;
                    rb.AddForce((moveForward + moveRight) * forceMult);
                }

            }
            else
            {
                moveRight = Input.Move.x * airMoveForce * transform.right;
                moveForward = Input.Move.y * airMoveForce * transform.forward;
                rb.AddForce(moveForward + moveRight);
            }
        }
        protected void Crouch(bool crouchValue)
        {
            //If trying to crouch, we should move towards 1. If trying to stand, we should move towards 0.
            int _crouchtarget = crouchValue ? 1 : 0;
            if (currentCrouch != _crouchtarget)
            {
                currentCrouch = Mathf.MoveTowards(currentCrouch, _crouchtarget, crouchIncrement * Delta);
                capsule.height = Mathf.Lerp(standCapsuleHeight, crouchCapsuleHeight, currentCrouch);
                capsule.center = Vector3.up * Mathf.Lerp(standCapsulePosition, crouchCapsulePosition, currentCrouch);
                crouchTransform.localPosition = Vector3.up * Mathf.Lerp(standHeadHeight, crouchHeadHeight, currentCrouch);
            }
        }
        protected void TryJump()
        {
            if (isClimbing && Input.Jump)
            {
                //We want to jump away from the wall, which means we should go back and store the normal of the wall we're climbing, probably. Or we can just jump 
                isClimbing = false;
                isGrounded = false;
                Input.Jump = false;
                currJumpCD = 0;
            }
            else
            {
                if ((isGrounded || airJumpCount > 0) && currJumpCD >= jumpCooldown && Input.Jump)
                {
                    rb.AddForce(transform.up * (jumpSpeed + Mathf.Clamp(rb.linearVelocity.y, -4, 0)), ForceMode.VelocityChange);
                    if (!isGrounded)
                    {
                        airJumpCount--;
                    }
                    isGrounded = false;
                    Input.Jump = false;
                    currJumpCD = 0;
                }
                if (currJumpCD < jumpCooldown)
                {
                    currJumpCD += Delta;
                }
            }
        }

        void StartSlide()
        {
            sliding = true;
            Debug.Log("started slide");
            rb.AddForce(slopeForwardFull * slideStartForce, ForceMode.Impulse);
        }

        void SlideUpdate()
        {
            if (rb.linearVelocity.magnitude <= slideMinSpeed)
            {
                EndSlide();
            }
            else
            {
                rb.AddForce(-rb.linearVelocity * slideDamp);
            }
        }

        void EndSlide()
        {
            sliding = false;
        }

        #endregion


        #endregion Motion
        #region Non-motion
        //This block covers everything not relating to motion that also does not fit within another block. 

        /// <summary>
        /// Rotates the player based on their current look input (moving the mouse or pushing the right thumbstick, in most situations)
        /// <br></br>Both players AND servers will have control over their rotation, by allowing the player to rotate a child transform.
        /// <br></br>When sending move inputs, players will also send their current rotation to ensure the server has the most up-to-date information.
        /// </summary>
        protected void RotatePlayer()
        {

            float oldPitch = pitch;
            pitch = Mathf.Clamp(pitch + Input.Look.y, -89.5f, 89.5f);
            lookDelta = new(Input.Look.x, pitch - oldPitch);
            rotationRoot.localRotation *= Quaternion.Euler(0, lookDelta.x, 0);
            aimTransform.localRotation = Quaternion.Euler(-pitch, 0, 0);
        }
        protected void UpdateCamera()
        {
            cam.transform.SetPositionAndRotation(camTargetPoint.position, camTargetPoint.rotation);
        }
        #endregion Non-motion
    }
}
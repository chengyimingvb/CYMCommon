using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController
{
    public abstract class BaseCharacterController : MonoBehaviour
    {
        /// <summary>
        // The KinematicCharacterMotor that will be assigned to this CharacterController via the inspector
        /// </summary>
        public KinematicCharacterMotor Motor { get; private set; }

        /// <summary>
        /// This is called by the KinematicCharacterMotor in its Awake to setup references
        /// </summary>
        public void SetupCharacterMotor(KinematicCharacterMotor motor)
        {
            Motor = motor;
            motor.CharacterController = this;
        }
        
        /// <summary>
        /// Asks what the character's rotation should be on this character update. 
        /// Modify the "currentRotation" to change the character's rotation.
        /// </summary>
        public abstract void UpdateRotation(ref Quaternion currentRotation, float deltaTime);

        /// <summary>
        /// Asks what the character's velocity should be on this character update. 
        /// Modify the "currentVelocity" to change the character's velocity.
        /// </summary>
        public abstract void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime);

        /// <summary>
        /// Gives you a callback for before the character update begins, if you 
        /// want to do anything to start off the update.
        /// </summary>
        public abstract void BeforeCharacterUpdate(float deltaTime);

        /// <summary>
        /// Gives you a callback for when the character has finished evaluating its grounding status
        /// </summary>
        public abstract void PostGroundingUpdate(float deltaTime);

        /// <summary>
        /// Gives you a callback for when the character update has reached its end, if you 
        /// want to do anything to finalize the update.
        /// </summary>
        public abstract void AfterCharacterUpdate(float deltaTime);

        /// <summary>
        /// Asks if a given collider should be considered for character collisions.
        /// Useful for ignoring specific colliders in specific situations.
        /// </summary>
        public abstract bool IsColliderValidForCollisions(Collider coll);

        /// <summary>
        /// Gives you a callback for ground probing hits
        /// </summary>
        public abstract void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport);

        /// <summary>
        /// Gives you a callback for character movement hits
        /// </summary>
        public abstract void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport);

        /// <summary>
        /// Gives you a chance to modify the HitStabilityReport of every character movement hit before it is returned to the movement code.
        /// Use this for advanced customization of character hit stability
        /// </summary>
        public abstract void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport);

        /// <summary>
        /// Notifies you when the character is colliding against colliders, but that collision isn't the result of a character movement
        /// </summary>
        public virtual void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        /// <summary>
        /// Allows you to override the way velocity is projected on an obstruction
        /// </summary>
        public virtual void HandleMovementProjection(ref Vector3 movement, Vector3 obstructionNormal, bool stableOnHit)
        {
            if (Motor.GroundingStatus.IsStableOnGround && !Motor.MustUnground)
            {
                // On stable slopes, simply reorient the movement without any loss
                if (stableOnHit)
                {
                    movement = Motor.GetDirectionTangentToSurface(movement, obstructionNormal) * movement.magnitude;
                }
                // On blocking hits, project the movement on the obstruction while following the grounding plane
                else
                {
                    Vector3 obstructionRightAlongGround = Vector3.Cross(obstructionNormal, Motor.GroundingStatus.GroundNormal).normalized;
                    Vector3 obstructionUpAlongGround = Vector3.Cross(obstructionRightAlongGround, obstructionNormal).normalized;
                    movement = Motor.GetDirectionTangentToSurface(movement, obstructionUpAlongGround) * movement.magnitude;
                    movement = Vector3.ProjectOnPlane(movement, obstructionNormal);
                }
            }
            else
            {
                if (stableOnHit)
                {
                    // Handle stable landing
                    movement = Vector3.ProjectOnPlane(movement, Motor.CharacterUp);
                    movement = Motor.GetDirectionTangentToSurface(movement, obstructionNormal) * movement.magnitude;
                }
                // Handle generic obstruction
                else
                {
                    movement = Vector3.ProjectOnPlane(movement, obstructionNormal);
                }
            }
        }

        /// <summary>
        /// Allows you to override the way hit rigidbodies are pushed / interacted with. 
        /// ProcessedVelocity is what must be modified if this interaction affects the character's velocity.
        /// </summary>
        public virtual void HandleSimulatedRigidbodyInteraction(ref Vector3 processedVelocity, RigidbodyProjectionHit hit, float deltaTime)
        {
            float simulatedCharacterMass = 0.2f;

            // Handle pushing rigidbodies in SimulatedDynamic mode
            if (simulatedCharacterMass > 0f &&
                !hit.StableOnHit &&
                !hit.Rigidbody.isKinematic)
            {
                float massRatio = simulatedCharacterMass / hit.Rigidbody.mass;
                Vector3 effectiveHitRigidbodyVelocity = Motor.GetVelocityFromRigidbodyMovement(hit.Rigidbody, hit.HitPoint, deltaTime);
                Vector3 relativeVelocity = Vector3.Project(hit.HitVelocity, hit.EffectiveHitNormal) - effectiveHitRigidbodyVelocity;

                hit.Rigidbody.AddForceAtPosition(massRatio * relativeVelocity, hit.HitPoint, ForceMode.VelocityChange);
            }

            // Compensate character's own velocity against the moving rigidbodies
            if (!hit.StableOnHit)
            {
                Vector3 effectiveRigidbodyVelocity = Motor.GetVelocityFromRigidbodyMovement(hit.Rigidbody, hit.HitPoint, deltaTime);
                Vector3 projRigidbodyVelocity = Vector3.Project(effectiveRigidbodyVelocity, hit.EffectiveHitNormal);
                Vector3 projCharacterVelocity = Vector3.Project(processedVelocity, hit.EffectiveHitNormal);
                processedVelocity += projRigidbodyVelocity - projCharacterVelocity;
            }
        }
    }
}
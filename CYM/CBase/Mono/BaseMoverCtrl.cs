//------------------------------------------------------------------------------
// BaseMoverCtrl.cs
// Copyright 2018 2018/3/15 
// Created by CYM on 2018/3/15
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using KinematicCharacterController;
using DG.Tweening;

namespace CYM
{
    [RequireComponent(typeof(PhysicsMover))]
    public class BaseMoverCtrl : BaseMoverController
    {
        public Vector3 TargetPos = Vector3.zero;
        //public float TranslationPeriod = 10;
        public float TranslationSpeed = 1;
        public Vector3 RotationAxis = Vector3.up;
        public float RotSpeed = 10;
        public Vector3 OscillationAxis = Vector3.zero;
        public float OscillationPeriod = 10;
        public float OscillationSpeed = 10;

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        PhysicsMover physicsMover;
        bool isForward = true;
        float curTime = 0.0f;

        private void Awake()
        {
            physicsMover = gameObject.GetComponentInChildren<PhysicsMover>();
            physicsMover.MoverController = this;
            SetupMover(physicsMover);
        }

        private void Start()
        {
            _originalPosition = Mover.Rigidbody.position;
            _originalRotation = Mover.Rigidbody.rotation;
        }

        public override void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            curTime += Time.smoothDeltaTime * TranslationSpeed;
            if (isForward)
            {
                goalPosition = Vector3.LerpUnclamped(_originalPosition, TargetPos, curTime);
                if (curTime >= 1.0f)
                {
                    curTime = 0.0f;
                    isForward = false;
                }
            }
            else
            {
                goalPosition = Vector3.LerpUnclamped(TargetPos, _originalPosition, curTime);
                if (curTime >= 1.0f)
                {
                    curTime = 0.0f;
                    isForward = true;
                }
            }

            Quaternion targetRotForOscillation = Quaternion.Euler(OscillationAxis.normalized * (Mathf.Sin(Time.time * OscillationSpeed) * OscillationPeriod)) * _originalRotation;
            goalRotation = Quaternion.Euler(RotationAxis * RotSpeed * Time.time) * targetRotForOscillation;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position, TargetPos);
            Gizmos.DrawSphere(TargetPos,0.1f);
        }
    }
}
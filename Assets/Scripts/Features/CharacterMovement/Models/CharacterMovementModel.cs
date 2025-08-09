// Scripts/Feature/CharacterMovement/CharacterMovementModel.cs

using System;
using Game.Core.Framework;
using R3;
using UnityEngine;

namespace Game.Feature.CharacterMovement
{
    public class CharacterMovementModel : BaseModel
    {
        private float _moveSpeed;
        public float MoveSpeed
        {
            get => _moveSpeed;
            set => SetProperty(ref _moveSpeed, value, nameof(MoveSpeed));
        }
        private Vector3 _currentDirection;
        public Vector3 CurrentDirection
        {
            get => _currentDirection;
            set
            {
                SetProperty(ref _currentDirection, value, nameof(CurrentDirection));
            }
        }
        public bool canRotate=true;
        
        public readonly ReactiveProperty<bool> IsMoving = new(false);
    }
}
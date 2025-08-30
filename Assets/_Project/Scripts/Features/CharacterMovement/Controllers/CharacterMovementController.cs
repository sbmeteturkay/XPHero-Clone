using UnityEngine;
using Zenject;
using UniRx;
using Game.Core.Services;
using System;

namespace Game.Feature.CharacterMovement
{
    public class CharacterMovementController : IInitializable, ITickable, IDisposable
    {
        [Inject] private CharacterMovementModel _model;
        [Inject] private InputService _inputService; // Core'dan gelen bir input servisi
        [Inject] private PlayerService _playerService; 

        private CharacterController characterController;
        private Animator Animator;
        private GameObject Character;
        
        private CompositeDisposable _disposables = new ();
        private Vector3 input;
        private readonly string horizontal = "Horizontal";
        private readonly string vertical = "Vertical";
        private float movementSpeed;

        public CharacterMovementController(CharacterController characterController, Animator animator, GameObject character)
        {
            this.characterController = characterController;
            this.Animator = animator;
            this.Character = character;
        }
        
        public void Initialize()
        {
            _inputService.OnMoveInput
                .Subscribe(SetMovementInput)
                .AddTo(_disposables);

            _inputService.OnJumpInput
                .Where(isPressed => isPressed)
                .Subscribe(_ => Jump())
                .AddTo(_disposables);
            _playerService.PlayerUpgradeData.Subscribe(OnUpgradeChange).AddTo(_disposables);
            OnUpgradeChange(_playerService.PlayerUpgradeData.Value);
        }

        void OnUpgradeChange(UpgradeData upgradeData)
        {
            movementSpeed = upgradeData.MoveSpeed*0.06f;
        }

        void SetMovementInput(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude < 0.01f)
            {
                _model.MoveSpeed = 0;
                _model.IsMoving.Value = false;
                return;
            }
            _model.MoveSpeed=moveInput.magnitude*movementSpeed;
            _model.IsMoving.Value = true;
            _model.CurrentDirection=new Vector3(moveInput.x,0,moveInput.y);
        }
        public void Tick()
        {
            if(_model.MoveSpeed<=0)
                return;
            // Hareket mantığı
            characterController.Move(_model.CurrentDirection* _model.MoveSpeed * Time.deltaTime);
            if (_model.canRotate)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_model.CurrentDirection);
                Character.transform.rotation = Quaternion.RotateTowards(
                    Character.transform.rotation,
                    targetRotation,
                    500* Time.deltaTime
                );
            }
        
            // Bu input'u karakterin local space'ine çevir
            Vector3 localInput = Character.transform.InverseTransformDirection(_model.CurrentDirection);
        
            // Animatöre local değerleri ver

            Animator.SetFloat(horizontal, _model.CurrentDirection.magnitude > 0.1f?localInput.x:0);
            Animator.SetFloat(vertical, _model.CurrentDirection.magnitude > 0.1f?localInput.z:0);
        }

        private void Jump()
        {
            Debug.Log("Karakter zıpladı!");
            // Zıplama mantığı
        }

        // Diğer Controller metodları

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
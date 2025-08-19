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
        [Inject] private CharacterMovementView _view;
        [Inject] private InputService _inputService; // Core'dan gelen bir input servisi

        private CompositeDisposable _disposables = new ();
        private Vector3 input;
        string horizontal = "Horizontal";
        string vertical = "Vertical";
        public void Initialize()
        {
            _inputService.OnMoveInput
                .Subscribe(x=>SetMovementInput(x))
                .AddTo(_disposables);

            _inputService.OnJumpInput
                .Where(isPressed => isPressed)
                .Subscribe(_ => Jump())
                .AddTo(_disposables);
        }

        void SetMovementInput(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude < 0.01f)
            {
                _model.MoveSpeed = 0;
                _model.IsMoving.Value = false;
                return;
            }
            _model.MoveSpeed=moveInput.magnitude*3f;
            _model.IsMoving.Value = true;
            _model.CurrentDirection=new Vector3(moveInput.x,0,moveInput.y);
        }
        public void Tick()
        {
            if(_model.MoveSpeed<=0)
                return;
            // Hareket mantığı
            _view.characterController.Move(_model.CurrentDirection* _model.MoveSpeed * Time.deltaTime);
            if (_model.canRotate)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_model.CurrentDirection);
                _view.gameObject.transform.rotation = Quaternion.RotateTowards(
                    _view.gameObject.transform.rotation,
                    targetRotation,
                    500* Time.deltaTime
                );
            }
        
            // Bu input'u karakterin local space'ine çevir
            Vector3 localInput = _view.gameObject.transform.InverseTransformDirection(_model.CurrentDirection);
        
            // Animatöre local değerleri ver

            if (_model.CurrentDirection.magnitude > 0.1f)
            {
                _view.Animator.SetFloat(horizontal, localInput.x);
                _view.Animator.SetFloat(vertical, localInput.z);
            }
            else
            {
                // Idle
                _view.Animator.SetFloat(horizontal, 0);
                _view.Animator.SetFloat(vertical, 0);
            }
                
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
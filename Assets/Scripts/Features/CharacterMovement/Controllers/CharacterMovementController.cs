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
                return;
            }
            _model.MoveSpeed=moveInput.magnitude*3f;
            _model.CurrentDirection=new Vector3(moveInput.x,0,moveInput.y);
        }
        public void Tick()
        {
            // Model'deki verilere göre View'ı güncelle
            // _view.SetAnimation(GetAnimationState(_model.CurrentDirection));

            if(_model.MoveSpeed<=0)
                return;
            // Hareket mantığı
            _view.characterController.Move(_model.CurrentDirection* _model.MoveSpeed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(_model.CurrentDirection);
            _view.gameObject.transform.rotation = Quaternion.RotateTowards(
                    _view.gameObject.transform.rotation,
                targetRotation,
                500* Time.deltaTime
            );
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
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
            // View'ı başlat
            // _view.Initialize(GetComponent<Animator>()); // Eğer View bir MonoBehaviour ise

            Debug.Log("char movement controller initialized");
            _inputService.OnMoveInput
                .Subscribe(direction => Debug.Log("Move direction: " + direction))
                .AddTo(_disposables);

            _inputService.OnJumpInput
                .Where(isPressed => isPressed)
                .Subscribe(_ => Jump())
                .AddTo(_disposables);
        }

        public void Tick()
        {
            // Model'deki verilere göre View'ı güncelle
            // _view.SetAnimation(GetAnimationState(_model.CurrentDirection));

            // Hareket mantığı
            // Transform'u güncelleme gibi Unity bileşenleriyle etkileşim
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
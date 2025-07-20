// Scripts/Feature/CharacterMovement/CharacterMovementView.cs
using UnityEngine;
using Zenject;

namespace Game.Feature.CharacterMovement
{
    public class CharacterMovementView : MonoBehaviour, IInitializable
    {
        [Inject] private CharacterMovementModel _model;
        private Animator _animator;

        public void Initialize()
        {
            _model.PropertyChanged += OnModelPropertyChanged;

        }
        public void Initialize(Animator animator)
        {
            _animator = animator;
        }

        private void OnModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_model.MoveSpeed))
            {
                _animator.SetFloat("MoveSpeed", _model.MoveSpeed);
            }

            Debug.Log(_model.CurrentDirection);
            Debug.Log(_model.MoveSpeed);
            // Diğer görsel güncellemeler
        }

        public void SetAnimation(string animationName)
        {
            _animator.Play(animationName);
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.PropertyChanged -= OnModelPropertyChanged;
            }
        }


    }
}
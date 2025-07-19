// Scripts/Feature/CharacterMovement/CharacterMovementView.cs
using UnityEngine;
using Zenject;

namespace Game.Feature.CharacterMovement
{
    public class CharacterMovementView : MonoBehaviour
    {
        [Inject] private CharacterMovementModel _model;
        private Animator _animator;

        public void Initialize(Animator animator)
        {
            _animator = animator;
            _model.PropertyChanged += OnModelPropertyChanged;
        }

        private void OnModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_model.MoveSpeed))
            {
                _animator.SetFloat("MoveSpeed", _model.MoveSpeed);
            }
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
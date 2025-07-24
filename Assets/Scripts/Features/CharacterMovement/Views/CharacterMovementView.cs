// Scripts/Feature/CharacterMovement/CharacterMovementView.cs
using UnityEngine;
using Zenject;

namespace Game.Feature.CharacterMovement
{
    public class CharacterMovementView : MonoBehaviour
    {
        [Inject] private CharacterMovementModel _model;
        [SerializeField]private Animator _animator;
        public CharacterController characterController;

        public void Start()
        {
            _model.PropertyChanged += OnModelPropertyChanged;
        }

        private void OnModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_model.MoveSpeed))
            {
                if (_model.MoveSpeed > 0)
                {
                    _animator.CrossFade("Move",.2f);
                }
                else
                {
                    _animator.CrossFade("Idle",.2f);
                }
            }
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
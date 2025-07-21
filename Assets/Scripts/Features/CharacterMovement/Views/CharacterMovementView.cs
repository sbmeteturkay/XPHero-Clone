// Scripts/Feature/CharacterMovement/CharacterMovementView.cs
using UnityEngine;
using Zenject;

namespace Game.Feature.CharacterMovement
{
    public class CharacterMovementView : MonoBehaviour
    {
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
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
                _animator.SetFloat(MoveSpeed, _model.MoveSpeed);
            }
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
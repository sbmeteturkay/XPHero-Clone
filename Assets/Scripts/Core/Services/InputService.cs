// Scripts/Core/Services/InputService.cs
using UnityEngine;
using UniRx;
using UnityEngine.InputSystem;
using Zenject;
using System;

namespace Game.Core.Services
{
    public class InputService : IInitializable, IDisposable
    {
        public ReactiveProperty<Vector2> OnMoveInput { get; private set; }
        public ReactiveProperty<bool> OnJumpInput { get; private set; }

        private InputActionAsset _inputActionAsset;
        private InputActionMap _playerActionMap;
        private InputAction _moveAction;
        private InputAction _jumpAction;

        [Inject]
        public InputService(InputActionAsset inputActionAsset)
        {
            _inputActionAsset = inputActionAsset;
            OnMoveInput = new ReactiveProperty<Vector2>();
            OnJumpInput = new ReactiveProperty<bool>();
        }

        public void Initialize()
        {
            _playerActionMap = _inputActionAsset.FindActionMap("Player");
            
            if (_playerActionMap == null)
            {
                Debug.LogError("Player Action Map bulunamadı! Input Action Asset'inizde 'Player' adında bir Action Map olduğundan emin olun.");
                return;
            }

            _moveAction = _playerActionMap.FindAction("Move");
            _jumpAction = _playerActionMap.FindAction("Jump");

            if (_moveAction != null)
            {
                _moveAction.performed += OnMovePerformed;
                _moveAction.canceled += OnMoveCanceled;
            }

            if (_jumpAction != null)
            {
                _jumpAction.performed += OnJumpPerformed;
                _jumpAction.canceled += OnJumpCanceled;
            }

            _playerActionMap.Disable(); // Başlangıçta deaktif
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            OnMoveInput.Value = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            OnMoveInput.Value = Vector2.zero;
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            OnJumpInput.Value = true;
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            OnJumpInput.Value = false;
        }

        public void EnablePlayerInput()
        {
            if (_playerActionMap != null)
            {
                Debug.Log("player input enabled");
                _playerActionMap.Enable();
            }
        }

        public void DisablePlayerInput()
        {
            if (_playerActionMap != null)
            {
                _playerActionMap.Disable();
            }
        }

        public void Dispose()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMovePerformed;
                _moveAction.canceled -= OnMoveCanceled;
            }

            if (_jumpAction != null)
            {
                _jumpAction.performed -= OnJumpPerformed;
                _jumpAction.canceled -= OnJumpCanceled;
            }

            DisablePlayerInput();
        }
    }
}
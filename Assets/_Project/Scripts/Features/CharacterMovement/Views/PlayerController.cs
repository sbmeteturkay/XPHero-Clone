using System;
using System.Collections.Generic;
using Game.Feature.CharacterMovement;
using UniRx;
using Zenject;

namespace Game.Feature.Player
{
    public class PlayerController: IInitializable, IDisposable
    {
        [Inject] CharacterMovementModel characterMovementModel;
        [Inject] PlayerAttack playerAttack;
        [Inject] PlayerAnimationController playerAnimationController;
        private readonly List<IDisposable> disposables = new();
        public void Initialize()
        {
            disposables.Add(playerAttack.HasEnemyInRange.Subscribe(PlayerAttackOnOnAttackingChanged));
            disposables.Add(characterMovementModel.IsMoving.Subscribe(IsMovingVhange));
        }

        private void IsMovingVhange(bool isMoving)
        {
            playerAnimationController.CrossFade(isMoving ? "Move" : "Idle");
        }

        private void PlayerAttackOnOnAttackingChanged(Enemy.Enemy isAttacking)
        {
            characterMovementModel.canRotate = !isAttacking;
        }

        public void Dispose()
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
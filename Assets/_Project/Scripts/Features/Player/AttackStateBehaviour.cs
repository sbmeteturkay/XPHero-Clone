using UnityEngine;

namespace Game.Feature.Player
{
    public class AttackStateBehaviour : StateMachineBehaviour
    {
        private bool _hasDealtDamage;

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float normalizedTime = stateInfo.normalizedTime % 1f;

            if (!_hasDealtDamage && normalizedTime >= 0.5f)
            {
                _hasDealtDamage = true;
                Debug.Log("damage");
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _hasDealtDamage = false;
        }
    }
}
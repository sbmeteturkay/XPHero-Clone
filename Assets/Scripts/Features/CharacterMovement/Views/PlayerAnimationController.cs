using UnityEngine;

namespace Game.Feature.Player
{
    public class PlayerAnimationController: MonoBehaviour
    {
        [SerializeField] private Animator Animator;
        public void CrossFade(string stateName,float duration=0.2f)
        {
            Animator.CrossFade(stateName, 0.2f);
        }

        public void SetBool(string propertyName,bool value)
        {
            Animator.SetBool(propertyName,value);
        }
    }
}
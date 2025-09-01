using UnityEngine;

namespace Game.Feature.Player
{
    public class PlayerAnimationController: MonoBehaviour
    {
        [SerializeField] private Animator Animator;
        public void CrossFade(string stateName,float duration=0.2f)
        {
            Animator.CrossFade(stateName, 0.1f);
        }

        public void SetBool(string propertyName,bool value)
        {
            Animator.SetBool(propertyName,value);
        }
        public void SetFloat(string propertyName,float value)
        {
            Animator.SetFloat(propertyName,value);
        }
    }
}
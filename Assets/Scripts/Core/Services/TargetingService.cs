using Game.Core.Interfaces;
using UnityEngine;

namespace Game.Core.Services
{
    public class TargetingService
    {
        public IDamageable FindNearestTarget(Vector3 origin, float range, LayerMask targetLayer)
        {
            Collider[] hitColliders = new Collider[10];
            int numColliders=Physics.OverlapSphereNonAlloc(origin, range, hitColliders,targetLayer);
            for (int i = 0; i < numColliders; i++)
            {
                hitColliders[i].SendMessage("AddDamage");
            }
            // En yakın hedefi bulma mantığı
            IDamageable nearestTarget = null;
            float minDistance = float.MaxValue;

            foreach (var hitCollider in hitColliders)
            {
                IDamageable damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    float distance = Vector3.Distance(origin, hitCollider.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestTarget = damageable;
                    }
                }
            }
            return nearestTarget;
        }

        // Diğer hedefleme metodları (örneğin, tüm hedefleri bulma, belirli bir türdeki hedefleri bulma)
    }
}
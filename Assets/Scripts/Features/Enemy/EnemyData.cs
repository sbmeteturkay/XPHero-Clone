// Scripts/Feature/Enemy/EnemyData.cs
using UnityEngine;

namespace Game.Feature.Enemy
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string EnemyName;
        public float MaxHealth;
        public float MoveSpeed;
        public float AttackDamage;
        public float AttackRange;
        public float AttackCooldown;
        public GameObject EnemyPrefab; // Düşmanın görsel prefab'ı
        // Diğer AI veya davranış parametreleri buraya eklenebilir
    }
}

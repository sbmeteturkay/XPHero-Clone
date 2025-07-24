using UnityEngine;
using Zenject;
using Game.Core.Interfaces;
using Game.Core.Services;
using UniRx;

namespace Game.Feature.Player
{
    public class PlayerAttack : MonoBehaviour
    {
        [Inject] private DamageService _damageService;
        [Inject] private SignalBus _signalBus;
        [Inject] private PlayerService _playerService;
        [Inject] private PlayerTargetingService _playerTargetingService; // Yeni inject edilen servis

        [SerializeField] private float _attackDamage = 10f; // Oyuncunun saldırı hasarı
        [SerializeField] private float _attackRange = 2f; // Oyuncunun saldırı menzili
        [SerializeField] private float _attackCooldown = 0.5f; // Saldırı bekleme süresi

        private float _attackCooldownTimer;

        void Start()
        {
            // Hedefleme sinyallerine abone ol
            _signalBus.Subscribe<PlayerTargetFoundSignal>(OnTargetFound);
            _signalBus.Subscribe<PlayerTargetLostSignal>(OnTargetLost);
        }

        void Update()
        {
            if (_attackCooldownTimer > 0)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }

            // Eğer bir hedef varsa ve saldırı cooldown'ı bittiyse saldır
            if (_playerTargetingService.CurrentTarget != null && _attackCooldownTimer <= 0)
            {
                TryAttack();
            }
        }

        public void TryAttack()
        {
            if (_attackCooldownTimer <= 0 && _playerTargetingService.CurrentTarget != null)
            {
                // Saldırı animasyonunu tetikle
                // GetComponent<Animator>().SetTrigger("Attack");

                // Saldırı mantığını uygula
                PerformAttack(_playerTargetingService.CurrentTarget);

                _attackCooldownTimer = _attackCooldown;
            }
        }

        private void PerformAttack(Game.Feature.Enemy.Enemy targetEnemy)
        {
            // Hedefe doğru dön
            transform.LookAt(targetEnemy.transform.position);

            // Hasar uygula
            _damageService.ApplyDamage(targetEnemy, _attackDamage, gameObject);
            Debug.Log($"Oyuncu {targetEnemy.name} hedefine { _attackDamage} hasar verdi.");

            _signalBus.Fire(new PlayerAttackedSignal { Attacker = this, DamageDealt = _attackDamage });
        }

        private void OnTargetFound(PlayerTargetFoundSignal signal)
        {
            Debug.Log($"Oyuncu yeni hedef buldu: {signal.NewTarget.name}");
            // Hedef bulunduğunda hemen saldırıyı başlatabiliriz veya ilk cooldown'ı bekleyebiliriz.
            // TryAttack();
        }

        private void OnTargetLost(PlayerTargetLostSignal signal)
        {
            Debug.Log($"Oyuncu hedefi kaybetti: {signal.OldTarget.name}");
            // Hedef kaybedildiğinde saldırıyı durdur
            // StopAttackAnimation();
        }

        void OnDestroy()
        {
            _signalBus.Unsubscribe<PlayerTargetFoundSignal>(OnTargetFound);
            _signalBus.Unsubscribe<PlayerTargetLostSignal>(OnTargetLost);
        }
    }

    public class PlayerAttackedSignal { public PlayerAttack Attacker; public float DamageDealt; }
}
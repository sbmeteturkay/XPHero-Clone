// Scripts/Feature/Spawn/SpawnArea.cs
using UnityEngine;
using Zenject;
using System.Collections.Generic;

namespace Game.Feature.Spawn
{
    [RequireComponent(typeof(Collider))]
    public class SpawnArea : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        void Awake()
        {
            // Collider'ın trigger olduğundan emin ol
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) // Oyuncunun tag'i 'Player' olmalı
            {
                _signalBus.Fire(new PlayerEnteredSpawnAreaSignal { SpawnArea = this, PlayerObject = other.gameObject });
                Debug.Log($"Oyuncu {gameObject.name} spawn alanına girdi.");
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _signalBus.Fire(new PlayerExitedSpawnAreaSignal { SpawnArea = this, PlayerObject = other.gameObject });
                Debug.Log($"Oyuncu {gameObject.name} spawn alanından çıktı.");
            }
        }
    }

    // Zenject Signal Tanımları
    public class PlayerEnteredSpawnAreaSignal { public SpawnArea SpawnArea; public GameObject PlayerObject; }
    public class PlayerExitedSpawnAreaSignal { public SpawnArea SpawnArea; public GameObject PlayerObject; }
}
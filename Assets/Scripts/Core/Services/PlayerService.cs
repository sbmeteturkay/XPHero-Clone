using Game.Core.Interfaces;
using UnityEngine;
using Zenject;

namespace Game.Core.Services
{
    public class PlayerService : IInitializable
    {
        // Oyuncunun Transform'unu tutacak ReactiveProperty veya doğrudan Transform
        // ReactiveProperty kullanmak, oyuncu transform'u değiştiğinde (örneğin sahne değişimi) diğer sistemlerin haberdar olmasını sağlar.
        public Transform PlayerTransform { get; private set; }
        public IDamageable PlayerDamageable { get; private set; }

        // Oyuncunun GameObject'ini veya diğer bileşenlerini de tutabiliriz.
        // public GameObject PlayerGameObject { get; private set; }

        public void Initialize()
        {
            // Oyuncuyu sahneden bulma veya Zenject ile inject etme
            // Eğer oyuncu sahneye dinamik olarak ekleniyorsa, bu kısım farklı yönetilebilir.
            // Şimdilik basitçe tag ile bulalım.
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                PlayerTransform = playerObject.transform;
            }
            else
            {
                Debug.LogWarning("PlayerService: Oyuncu GameObject bulunamadı! Lütfen oyuncunuza 'Player' tag'ini ekleyin.");
            }
            PlayerDamageable = PlayerTransform.GetComponent<IDamageable>();
        }

        // Oyuncunun pozisyonunu, sağlığını vb. almak için metodlar eklenebilir.
        public Vector3 GetPlayerPosition()
        {
            return PlayerTransform != null ? PlayerTransform.position : Vector3.zero;
        }
    }
}


using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using R3;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI.PopupSystem
{
    public class PopupManager : IDisposable
    {
        private readonly PopupPool pool;
        private readonly MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private bool isDisposed = false;

        [Inject]
        public PopupManager(PopupPool pool)
        {
            this.pool = pool;
            Application.quitting += OnApplicationQuitting;
        }

        public void ShowDamage(Vector3 position, string text, Color color)
        {
            if (isDisposed) return;

            var popup = pool.Spawn();
            if (popup == null) return;

            popup.transform.position = position;
            popup.SetText(text);

            AnimatePopup(popup, color).Forget();
        }

        private async UniTask AnimatePopup(TextMeshPro popup, Color color)
        {
            if (isDisposed || popup == null) return;

            float duration = 0.5f;
            Vector3 endPos = popup.transform.position + Vector3.up * 1.5f;

            // Position animasyonu
            var positionTween = Tween.Position(popup.transform, endPos, duration, Ease.Linear);

            // Alpha animasyonu - renderer'ı cache'le
            var renderer = popup.GetComponent<Renderer>();
            if (renderer == null)
            {
                pool.Despawn(popup);
                return;
            }

            var alphaTween = Tween.Custom(1f, 0f, duration,
                onValueChange: (alpha) =>
                {
                    if (isDisposed || popup == null) return;

                    mpb.SetColor("_FaceColor", new Color(color.r, color.g, color.b, alpha));
                    renderer.SetPropertyBlock(mpb);
                }
                ,Ease.InQuad
            );

            // Animasyon bitince pool'a döndür
            positionTween.OnComplete(() =>
            {
                if (!isDisposed && popup != null)
                {
                    pool.Despawn(popup);
                }
            });

            // Sadece cancellation için delay - performans için minimal
            await UniTask.Delay(
                TimeSpan.FromSeconds(duration),
                cancellationToken: cancellationTokenSource.Token
            ).SuppressCancellationThrow();
        }

        private void OnApplicationQuitting()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (isDisposed) return;

            isDisposed = true;
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();

            Application.quitting -= OnApplicationQuitting;
        }
    }

}
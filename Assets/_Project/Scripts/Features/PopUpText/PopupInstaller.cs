using TMPro;
using Zenject;

namespace UI.PopupSystem
{
public class PopupInstaller : MonoInstaller
{
    public TextMeshPro popupPrefab;

    public override void InstallBindings()
    {
        // MemoryPool bind
        Container.BindMemoryPool<TextMeshPro, PopupPool>()
            .WithInitialSize(10)
            .FromComponentInNewPrefab(popupPrefab)
            .UnderTransformGroup("PopupPool");

        Container.Bind<PopupManager>().AsSingle();
    }
}
public class PopupPool : MonoMemoryPool<TextMeshPro>
{
    protected override void OnDespawned(TextMeshPro item)
    {
        base.OnDespawned(item);
        item.gameObject.SetActive(false);
    }

    protected override void OnSpawned(TextMeshPro item)
    {
        base.OnSpawned(item);
        item.gameObject.SetActive(true);
    }
}
}
namespace Game.Core.Interfaces
{
    public interface IAttackable
    {
        void Attack(IDamageable target);
        // Ek olarak, saldırı animasyonu, menzil kontrolü vb. parametreler eklenebilir.
    }
}
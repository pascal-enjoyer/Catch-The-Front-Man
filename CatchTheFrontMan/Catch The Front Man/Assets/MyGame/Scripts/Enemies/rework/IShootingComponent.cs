public interface IShootingComponent : IEnemyComponent
{
    bool IsAttacking { get; }
    void StartAttacking();
    void StopAttacking();
}
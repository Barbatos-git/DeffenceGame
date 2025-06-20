public interface IDamageable
{
    void TakeDamage(int amount);
    bool IsDead();
    float GetCurrentHealth();
    float GetMaxHealth();
}

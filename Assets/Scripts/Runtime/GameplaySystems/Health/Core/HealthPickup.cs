using UnityEngine;

public class HealthPickup : Pickup
{
    protected override bool ApplyPickup(GameObject player)
    {
        var damageable = player.GetComponentInParent<IDamageable>();
        if (damageable != null && damageable.IsAlive && damageable.CurrentHealth < damageable.MaxHealth)
        {
            damageable.Heal(amount);
            return true;
        }
        return false;
    }
}
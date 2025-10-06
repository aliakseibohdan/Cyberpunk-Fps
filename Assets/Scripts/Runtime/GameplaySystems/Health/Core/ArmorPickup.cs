using UnityEngine;

public class ArmorPickup : Pickup
{
    protected override bool ApplyPickup(GameObject player)
    {
        var playerHealth = player.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null && playerHealth.IsAlive)
        {
            playerHealth.AddArmor(amount);
            return true;
        }
        return false;
    }
}
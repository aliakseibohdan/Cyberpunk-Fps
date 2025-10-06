using UnityEngine;

public class HealthDebugGUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private bool showDebug = true;

    private void OnGUI()
    {
        if (!showDebug || playerHealth == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 400));

        GUILayout.Label($"HEALTH DEBUG", new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold });
        GUILayout.Space(10);

        GUILayout.Label($"Health: {playerHealth.CurrentHealth:F1}/{playerHealth.MaxHealth:F1}");
        GUILayout.Label($"Alive: {playerHealth.IsAlive}");

        GUILayout.Space(10);
        GUILayout.Label("QUICK ACTIONS", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });

        if (GUILayout.Button("Take 10 Kinetic Damage"))
            playerHealth.TakeDamage(10, DamageType.Kinetic);

        if (GUILayout.Button("Take 25 Explosive Damage"))
            playerHealth.TakeDamage(25, DamageType.Explosive);

        if (GUILayout.Button("Heal 25 Health"))
            playerHealth.Heal(25);

        if (GUILayout.Button("Add 25 Armor"))
            playerHealth.AddArmor(25);

        if (GUILayout.Button("Kill Player"))
            playerHealth.TakeDamage(1000, DamageType.Kinetic);

        GUILayout.Space(10);
        GUILayout.Label("DAMAGE TYPE TEST", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });

        foreach (DamageType damageType in System.Enum.GetValues(typeof(DamageType)))
        {
            if (GUILayout.Button($"10 {damageType} Damage"))
                playerHealth.TakeDamage(10, damageType);
        }

        GUILayout.EndArea();
    }
}
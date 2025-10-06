using UnityEngine;

[System.Serializable]
public struct DamageResistance
{
    [Range(0f, 1f)]
    public float kineticResistance;

    [Range(0f, 1f)]
    public float incendiaryResistance;

    [Range(0f, 1f)]
    public float explosiveResistance;

    [Range(0f, 1f)]
    public float electricalResistance;

    [Range(0f, 1f)]
    public float acidResistance;

    public readonly float GetResistance(DamageType damageType) => damageType switch
    {
        DamageType.Kinetic => kineticResistance,
        DamageType.Incendiary => incendiaryResistance,
        DamageType.Explosive => explosiveResistance,
        DamageType.Electrical => electricalResistance,
        DamageType.Acid => acidResistance,
        _ => 0f
    };
}
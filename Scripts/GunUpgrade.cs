
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
#endif

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class GunUpgrade : UdonSharpBehaviour
{
    [Header("If someone is holding two guns with upgrades, the ones with negative effects (multipliers less than 1.0) have precedence over positive effects (multipliers of 1.0 or more). If both are positive or both are negative, the more extreme one takes precedence.")]
    [Tooltip("Setting health to 0 or negative values will set the health to 1, so they will die in one hit.")]
    public float health_multiplier = 1f;
    [Tooltip("Setting shield to 0 or negative values disables the shield.")]
    public float shield_multiplier = 1f;
    [Tooltip("Setting regen speed to 0 or negative values disables the shield.")]
    public float shield_regen_amount_multiplier = 1f;
    [Tooltip("Setting speed to 0 or negative values makes it so you can't move lmao")]
    public float speed_multiplier = 1f;
    [Tooltip("Setting jump to 0 or negative values makes it so you can't jump")]
    public float jump_multiplier = 1f;
    void Start()
    {
        
    }
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    public void Reset()
    {
        if (GetComponent<P_Shooter>() == null)
        {
            P_Shooter shooter = GetComponent<P_Shooter>();
            if (shooter == null || shooter.gunUpgrades != null)
            {
                return;
            }
            SerializedObject s = new SerializedObject(shooter);
            s.FindProperty("gunUpgrades").objectReferenceValue = this;
            s.ApplyModifiedProperties();
        }
    }
#endif
}

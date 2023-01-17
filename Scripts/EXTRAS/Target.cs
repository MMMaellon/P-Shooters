
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Target : UdonSharpBehaviour
{
    public PlayerHandler playerHandler;
    public int starting_health = 100;
    public UdonBehaviour localShootUdon;
    public string localShootEvent;
    public UdonBehaviour globalShootUdon;
    public string globalShootEvent;

    public UdonBehaviour localDestroyUdon;
    public string localDestroyEvent;
    public UdonBehaviour globalDestroyUdon;
    public string globalDestroyEvent;
    
    [System.NonSerialized]
    public int health = 100;
    void Start()
    {
        ResetHealth();
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    public void Reset()
    {
        if (playerHandler == null)
        {
            playerHandler = FindObjectOfType<PlayerHandler>();
        }
    }
#endif

    public void ResetHealth()
    {
        health = starting_health;
    }

    public void OnParticleCollision(GameObject other)
    {
        OnHitDamageLayer(other);
    }
    public void OnTriggerEnter(Collider other)
    {
        OnHitDamageLayer(other.gameObject);
    }

    public void OnHitDamageLayer(GameObject other)
    {
        if (!Utilities.IsValid(other))
        {
            return;
        }
        P_Shooter leftShooter = playerHandler._localPlayer.GetLeftShooter();
        P_Shooter rightShooter = playerHandler._localPlayer.GetRightShooter();
        VRC_Pickup otherPickup = other.GetComponent<VRC_Pickup>();
        if (otherPickup == null)
        {
            otherPickup = other.GetComponentInParent<VRC_Pickup>();
        }
        if (otherPickup == null)
        {
            return;
        }
        if (leftShooter != null && leftShooter.smartPickup != null && leftShooter.smartPickup.pickup == otherPickup)
        {
            OnShot(leftShooter, true);
        }
        else if (rightShooter != null && rightShooter.smartPickup != null && rightShooter.smartPickup.pickup == otherPickup)
        {
            OnShot(rightShooter, false);
        }
    }

    public void OnShot(P_Shooter shooter, bool left_hand)
    {
        if (shooter == null)
        {
            return;
        }
        health -= shooter.manager.player_handler._localPlayer.CalcDamage(left_hand);

        if (localShootUdon)
        {
            localShootUdon.SendCustomEvent(localShootEvent);
        }
        if (globalShootUdon)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(GlobalShootEvent));
        }

        if (health <= 0)
        {
            Debug.LogWarning("no health");
            ResetHealth();

            if (localDestroyUdon)
            {
                localDestroyUdon.SendCustomEvent(localDestroyEvent);
            }
            if (globalDestroyUdon)
            {
                Debug.LogWarning("sending event");
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(GlobalDestroyEvent));
            }
        }
    }

    public void GlobalShootEvent()
    {
        if (globalShootUdon)
        {
            globalShootUdon.SendCustomEvent(globalShootEvent);
        }
    }
    public void GlobalDestroyEvent()
    {
        Debug.LogWarning("GlobalDestroyEvent");
        if (globalDestroyUdon)
        {
            Debug.LogWarning("GlobalDestroyEvent 2");
            globalDestroyUdon.SendCustomEvent(globalDestroyEvent);
        }
    }
}

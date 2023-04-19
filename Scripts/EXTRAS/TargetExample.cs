
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    public class TargetExample : UdonSharpBehaviour
    {
        public Target target;
        public float respawnDelay = 2f;
        public UnityEngine.UI.Image healthBar;
        void Start()
        {

        }

        public void UpdateHealthBar()
        {
            healthBar.fillAmount = ((float)target.health) / ((float)target.starting_health);
        }

        public void OnDestroyTarget()
        {
            Debug.LogWarning("OnDestroy");
            target.gameObject.SetActive(false);
            SendCustomEventDelayedSeconds(nameof(Respawn), respawnDelay);
        }

        public void Respawn()
        {
            Debug.LogWarning("Respawn");
            target.gameObject.SetActive(true);
            target.ResetHealth();
            UpdateHealthBar();
        }
    }
}
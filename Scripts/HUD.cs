
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    [RequireComponent(typeof(Animator)), UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class HUD : Cyan.PlayerObjectPool.CyanPlayerObjectPoolEventListener
    {
        [System.NonSerialized]
        public Player player = null;
        [System.NonSerialized]
        public VRCPlayerApi _localPlayer = null;
        [System.NonSerialized]
        public Animator animator = null;
        
        [Tooltip("At 0, the HUD will stay in the same position when you look up or down. At 1 it will be locked to your screen. I like keeping it somewhere in the middle because I don't like when UIs stick too closely to my screen in VR. Setting is ignored on desktop.")]
        public float screenFollow = 0.75f;

        public override void _OnLocalPlayerAssigned()
        {
            animator.SetBool("loaded", true);
        }

        public override void _OnPlayerAssigned(VRCPlayerApi vrcPlayer, int poolIndex, UdonBehaviour poolObject)
        {
            if (vrcPlayer.isLocal && player == null)
            {
                AssignPlayer(poolObject.GetComponent<Player>());
                isLocal = true;
            }
        }

        public override void _OnPlayerUnassigned(VRCPlayerApi vrcPlayer, int poolIndex, UdonBehaviour poolObject)
        {

        }

        [System.NonSerialized] public bool isLocal;
        [System.NonSerialized] public bool desktopUI;
        void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            animator = GetComponent<Animator>();
            if (Utilities.IsValid(player))
            {
                AssignPlayer(player);
                isLocal = Utilities.IsValid(player.Owner) && player.Owner.isLocal;
            } else
            {
                isLocal = true;
            }
            desktopUI = !_localPlayer.IsUserInVR();
        }

        public void AssignPlayer(Player newPlayer)
        {
            player = newPlayer;
            if (Utilities.IsValid(player.statsAnimator))
            {
                player.statsAnimator.gameObject.SetActive(false);
            }
            player.statsAnimator = animator;
            gameObject.SetActive(player.gameObject.activeSelf);
        }

        private VRCPlayerApi.TrackingData headData;
        public void LateUpdate()
        {
            headData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            transform.position = headData.position;
            if (desktopUI)
            {
                transform.rotation = headData.rotation;
            } else
            {
                transform.rotation = Quaternion.Slerp(_localPlayer.GetRotation(), headData.rotation, screenFollow);
            }
        }
    }
}
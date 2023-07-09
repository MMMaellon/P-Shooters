
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Cyan.PlayerObjectPool;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Immutable;
#endif

namespace MMMaellon.P_Shooters
{
    [RequireComponent(typeof(Animator)), UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class HUD : CyanPlayerObjectPoolEventListener
    {
        [System.NonSerialized]
        public Player player = null;
        [System.NonSerialized]
        public VRCPlayerApi _localPlayer = null;
        [System.NonSerialized]
        public Animator animator = null;
        
        [Tooltip("At 0, the HUD will stay in the same position when you look up or down. At 1 it will be locked to your screen. I like keeping it somewhere in the middle because I don't like when UIs stick too closely to my screen in VR. Setting is ignored on desktop.")]
        public float screenFollow = 0.75f;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset(){
            SetupHUD(this);
        }

        public static void SetupHUD(HUD hud){
            if (!Utilities.IsValid(hud))
            {
                return;
            }
            CyanPlayerObjectAssigner assigner = GameObject.FindObjectOfType<CyanPlayerObjectAssigner>();
            if(!Utilities.IsValid(assigner))
            {
                Helper.ErrorLog(hud, "No CyanPlayerObjectAssigner found in scene. Please add one to the scene");
            }
        }
#endif

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

        public virtual void Start()
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

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Immutable;
#endif

namespace MMMaellon
{
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(P_Shooter))]
    public class P_ShooterEditor : Editor
    {
        public static void SetupShooter(P_Shooter shooter)
        {
            if (!Utilities.IsValid(shooter))
            {
                Debug.LogError("<color=red>[P-Shooter AUTOSETUP]: FAILED</color> No P-Shooter Found");
                return;
            }
            Cyan.PlayerObjectPool.CyanPlayerObjectAssigner assigner = GameObject.FindObjectOfType<Cyan.PlayerObjectPool.CyanPlayerObjectAssigner>();
            if (!Utilities.IsValid(assigner))
            {
                Debug.LogError("<color=red>[P-Shooter AUTOSETUP]: FAILED</color> Could not find player object pool. Please set up a player object pool");
                return;
            }
            if (!Utilities.IsValid(assigner.GetComponentInChildren<Player>()))
            {
                Debug.LogError("<color=red>[P-Shooter AUTOSETUP]: FAILED</color> Could not find players in player object pool. Please make sure your player prefab uses a Player script on the root object");
                return;
            }
            SerializedObject serialized = new SerializedObject(shooter);
            serialized.FindProperty("assigner").objectReferenceValue = assigner;
            serialized.FindProperty("sync").objectReferenceValue = shooter.GetComponent<SmartObjectSync>();
            serialized.ApplyModifiedProperties();
        }
        public override void OnInspectorGUI()
        {
            P_Shooter shooter = target as P_Shooter;
            if (!Utilities.IsValid(shooter))
            {
                return;
            }
            if (!Utilities.IsValid(shooter.assigner))
            {
                EditorGUILayout.LabelField("Setup Required");
                EditorGUILayout.HelpBox(
@"Please set up a player object pool in the scene and then use the Setup button below
", MessageType.Info);

                EditorGUILayout.Space();

                if (GUILayout.Button(new GUIContent("Setup")))
                {
                    P_ShooterEditor.SetupShooter(target as P_Shooter);
                }
                EditorGUILayout.Space();
            }
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }
    }
#endif
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), RequireComponent(typeof(SmartObjectSync))]
    public class P_Shooter : SmartObjectSyncListener
    {
        public bool printDebugMessages = true;
        public void _print(string message)
        {
            if (printDebugMessages)
            {
                Debug.Log("<color=yellow>[P-Shooters P_Shooter.cs] " + name + ": </color>" + message);
            }
        }
        [System.NonSerialized, FieldChangeCallback(nameof(localPlayerObject))]
        public Player _localPlayerObject;

        public Player localPlayerObject
        {
            get
            {
                if (!Utilities.IsValid(_localPlayerObject))
                {
                    GameObject playerObj = assigner._GetPlayerPooledObject(_localPlayer);
                    if (!Utilities.IsValid(playerObj))
                    {
                        return _localPlayerObject;
                    }
                    _localPlayerObject = playerObj.GetComponent<Player>();
                }
                return _localPlayerObject;
            }
            set
            {
                _localPlayerObject = value;
            }
        }

        [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(state))]
        public int _state = STATE_IDLE;
        //negative values constitute custom states. mainly used for multi-step reloading
        [System.NonSerialized]
        public const int STATE_IDLE = 0;//gun is just sitting there
        [System.NonSerialized]
        public const int STATE_SHOOT = 1;//gun is shooting. Can only get here from the idle state if we had ammo
        [System.NonSerialized]
        public const int STATE_EMPTY = 2;//gun is trying to shoot, but is out of ammo.
        [System.NonSerialized]
        public const int STATE_RELOAD = 3;//gun is not able to shoot because you're stuck in a reload animation
        [System.NonSerialized]
        public const int STATE_DISABLED = 4;//gun jammed or overheated or something

        [Header("Required Components")]
        public ParticleSystem shootParticles;
        public Cyan.PlayerObjectPool.CyanPlayerObjectAssigner assigner;
        public SmartObjectSync sync;
        public Animator animator;
        public Transform gunParent;

        [Header("Optional Components")]
        public AmmoTracker ammo;
        public GameObject scope;
        public float zoomInTime = 0.25f;
        public float zoomOutTime = 0.1f;

        [Header("Sounds")]
        public AudioSource gunshotSource;
        public AudioClip[] gunshots;
        [Tooltip("There's a hard limit to how many audio clips can be playing at one time. If you have too many, background sounds and players might become muted until you rejoin the world.")]
        public bool overlapGunshotSounds = false;
        public int state{
            get => _state;
            set
            {
                _state = value;
                if (sync.IsLocalOwner())
                {
                    RequestSerialization();
                }
                animator.SetInteger("state", _state);
                _print("STATE: " + StateToString(state));
            }
        }

        public override void OnChangeState(SmartObjectSync s, int oldState, int newState)
        {
            _print("OnChangeState");
            if (s == sync)
            {
                animator.SetInteger("pickup_state", newState);
                if (newState == SmartObjectSync.STATE_TELEPORTING)
                {
                    animator.SetTrigger("teleport");
                }
            }
        } 

        public override void OnChangeOwner(SmartObjectSync s, VRCPlayerApi oldPlayer, VRCPlayerApi newPlayer)
        {
            if (s == sync)
            {
                animator.SetBool("local", sync.IsLocalOwner());
            }
            if (newPlayer.IsValid() && newPlayer.isLocal){
                if (Utilities.IsValid(scope))
                {
                    Networking.SetOwner(newPlayer, scope.gameObject);
                }
            }
        }


#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset(){
            P_ShooterEditor.SetupShooter(this);
        }
#endif
        [System.NonSerialized]
        public VRCPlayerApi _localPlayer;
        
        void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            sync = GetComponent<SmartObjectSync>();
            sync.AddListener(this);
            if (Utilities.IsValid(scope))
            {
                scope.gameObject.SetActive(false);
            }
            if (Utilities.IsValid(ammo))
            {
                ammo._Register(this);
            }
        }

        public int calcDamage()
        {
            return 1;
        }

        public void ShootFX()
        {
            if (Utilities.IsValid(ammo))
            {
                if (!ammo.ConsumeAmmo())
                {
                    state = STATE_EMPTY;
                    ammo.OutOfAmmoFX();
                }
            }
            if (state != STATE_SHOOT && state != STATE_IDLE)//no round was chambered during the shoot animation or we're not shooting
            {
                //might get disabled or run out of bullets during a burst fire
                return;
            }
            _print("ShootFX");
            shootParticles.Play();
            if (Utilities.IsValid(gunshotSource) && gunshots.Length > 0)
            {
                if (overlapGunshotSounds)
                {
                    gunshotSource.PlayOneShot(gunshots[Random.Range(0, gunshots.Length)]);
                } else
                {
                    gunshotSource.clip = gunshots[Random.Range(0, gunshots.Length)];
                    gunshotSource.Play();
                }
            }
        }
        private bool ADS = false;
        private Vector3 zoomStartPos;
        private Vector3 zoomStopPos;
        private Quaternion zoomStartRot;
        private Quaternion zoomStopRot;
        private float zoomStart;

        public void recordStartZoomTransforms()
        {
            zoomStartPos = zoomStopPos;
            zoomStartRot = zoomStopRot;
            float incompleteZoom = 0;
            float currentZoomTime = (Time.realtimeSinceStartup - zoomStart);
            if (!ADS)
            {
                if (zoomInTime == 0)
                {
                    zoomStart = Time.realtimeSinceStartup;
                    return;
                }
                incompleteZoom = 1 - (currentZoomTime / zoomInTime);
                incompleteZoom = Mathf.Lerp(0, Mathf.Lerp(0, incompleteZoom, incompleteZoom), incompleteZoom);
            }
            else
            {
                if (zoomOutTime == 0)
                {
                    zoomStart = Time.realtimeSinceStartup;
                    return;
                }
                incompleteZoom = 1 - (currentZoomTime / zoomOutTime);
                incompleteZoom = Mathf.Lerp(0, Mathf.Lerp(0, incompleteZoom, incompleteZoom), incompleteZoom);
            }
            if (incompleteZoom < 0)
            {
                zoomStart = Time.realtimeSinceStartup;
                return;
            }
            if (!ADS)
            {
                incompleteZoom *= zoomOutTime;
            }
            else
            {
                incompleteZoom *= zoomInTime;
            }
            zoomStart = Time.realtimeSinceStartup - incompleteZoom;
        }
        public void zoomIn()
        {
            VRCPlayerApi.TrackingData headData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            if (!Utilities.IsValid(scope))
            {
                zoomStopPos = Vector3.ProjectOnPlane((transform.position - headData.position), headData.rotation * Vector3.left) + headData.position;
                zoomStopPos = transform.InverseTransformPoint(zoomStopPos);
            } else
            {
                zoomStopPos = Quaternion.Inverse(transform.rotation) * (headData.position - scope.transform.position);
                zoomStopRot = scope.transform.localRotation;
            }

            if (zoomInTime <= 0)
            {
                gunParent.localPosition = zoomStopPos;
                gunParent.localRotation = zoomStopRot;
            }
            else
            {
                float lerp = (Time.realtimeSinceStartup - zoomStart) / zoomInTime;
                float smoothedLerp = Mathf.Lerp(lerp, 1, lerp);
                gunParent.localPosition = Vector3.Lerp(zoomStartPos, zoomStopPos, smoothedLerp);
                gunParent.localRotation = Quaternion.Lerp(zoomStartRot, zoomStopRot, smoothedLerp);
            }
        }

        public void zoomOut()
        {
            zoomStopPos = Vector3.zero;
            zoomStopRot = Quaternion.identity;
            
            if (zoomOutTime <= 0)
            {
                gunParent.localPosition = zoomStopPos;
                gunParent.localRotation = zoomStopRot;
            } else
            {
                float lerp = (Time.realtimeSinceStartup - zoomStart) / zoomOutTime;
                float smoothedLerp = Mathf.Lerp(lerp, 1,lerp);
                gunParent.localPosition = Vector3.Lerp(zoomStartPos, zoomStopPos, smoothedLerp);
                gunParent.localRotation = Quaternion.Lerp(zoomStartRot, zoomStopRot, smoothedLerp);
            }
        }

        public void Update()
        {
            if (!sync.pickup.IsHeld)
            {
                if (ADS)
                {
                    ADS = false;
                    recordStartZoomTransforms();
                    zoomOut();
                } else if (Time.realtimeSinceStartup - zoomStart < zoomOutTime)
                {
                    zoomOut();
                }
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                ADS = true;
                recordStartZoomTransforms();
            } else if (Input.GetKeyUp(KeyCode.LeftAlt))
            {
                ADS = false;
                recordStartZoomTransforms();
            }

            if (Input.GetKey(KeyCode.LeftAlt))
            {
                zoomIn();
            }
            else
            {
                zoomOut();
            }
        }

        public override void OnPickup()
        {
            if (Utilities.IsValid(scope))
            {
                scope.gameObject.SetActive(true);
            }
        }
        public override void OnPickupUseDown()
        {
            if (state != STATE_IDLE)
            {
                return;
            }
            
            if (Utilities.IsValid(ammo))
            {
                ammo.Shoot();
            }
            else 
            {
                state = STATE_SHOOT;
            }
        }
        public override void OnPickupUseUp()
        {
            if (state == STATE_SHOOT || state == STATE_EMPTY)
            {
                state = STATE_IDLE;
            }
        }
        public override void OnDrop()
        {
            if (Utilities.IsValid(scope))
            {
                scope.gameObject.SetActive(false);
            }
        }

        public string StateToString(int value)
        {
            switch (value)
            {
                case (STATE_IDLE):
                    {
                        return "STATE_ARMED";
                    }
                case (STATE_SHOOT):
                    {
                        return "STATE_SHOOT";
                    }
                case (STATE_EMPTY):
                    {
                        return "STATE_EMPTY";
                    }
                case (STATE_RELOAD):
                    {
                        return "STATE_RELOAD";
                    }
                case (STATE_DISABLED):
                    {
                        return "STATE_DISABLED";
                    }
                default:
                    {
                        return "INVALID STATE";
                    }
            }
        }
    }
}
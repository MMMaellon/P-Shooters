
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    [RequireComponent(typeof(CapsuleCollider)), UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Player : Cyan.PlayerObjectPool.CyanPlayerObjectPoolObject
    {
        public bool printDebugMessages = true;
        public void _print(string message)
        {
            if (printDebugMessages)
            {
                Debug.Log("<color=yellow>[P-Shooters Player.cs] " + name + ": </color>" + message);
            }
        }
        [System.NonSerialized]
        Transform parent;
        [System.NonSerialized]
        public VRCPlayerApi _localPlayer = null;
        [System.NonSerialized]
        public Player _localPlayerObject = null;
        [System.NonSerialized]
        public CapsuleCollider capsuleCollider = null;
        void Start()
        {
            id = transform.GetSiblingIndex();
            parent = transform.parent;
            _localPlayer = Networking.LocalPlayer;
            capsuleCollider = GetComponent<CapsuleCollider>();
            BuildResourceIdMap();
        }
        [System.NonSerialized]
        public int id = -1;
        [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(team))]
        public int _team = TEAM_NONE;
        [System.NonSerialized]
        public const int TEAM_NONE = -1001;
        public int team
        {
            get => _team;
            set
            {
                _team = value;
                if (Utilities.IsValid(statsAnimator))
                {
                    statsAnimator.SetInteger("team", value);
                }
                if(IsOwnerLocal()){
                    RequestSerialization();
                }
            }
        }
        [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(state))]
        public int _state = STATE_SPECTATING;
        public int state
        {
            get => _state;
            set
            {
                _state = value;
                if (Utilities.IsValid(statsAnimator))
                {
                    statsAnimator.SetInteger("state", value);
                }
                if(IsOwnerLocal()){
                    RequestSerialization();
                }
            }
        }
        [System.NonSerialized]
        public const int STATE_SPECTATING = -1001;//can't deal or take damage
        [System.NonSerialized]
        public const int STATE_NORMAL = 0;//can do everything
        [System.NonSerialized]
        public const int STATE_DISABLED = 1;//can't deal damage
        [System.NonSerialized]
        public const int STATE_INVINCIBLE = 2;//can't take damage
        [System.NonSerialized]
        public const int STATE_DOWNED = 3;//can't do anything, but can still be revived
        [System.NonSerialized]
        public const int STATE_DEAD = 3;//can't do anything, waiting for respawn
        [System.NonSerialized]
        public const int STATE_FROZEN = 5;//can't do anything and can't move, while resurrecting another player and stuff like that
        [System.NonSerialized, UdonSynced(UdonSyncMode.None)]
        //arbitrarily choose to keep the last 8 
        public int[] damageDealt = new int[8];
        [System.NonSerialized, UdonSynced(UdonSyncMode.None)]
        public int[] damageTargetId = { -1001, -1001, -1001, -1001, -1001, -1001, -1001, -1001};
        [System.NonSerialized]//we don't serialize this whole thing
        public int[] damageMatrix = new int[82];
        [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(shield))]
        public int _shield = 100;
        public int defaultShield = 100;
        public int maxShield = 100;
        public int shield
        {
            get => _shield;
            set
            {
                if (value > _shield)
                {
                    _shield = value;
                    if (value >= maxShield)
                    {
                        _shield = maxShield;
                        _OnIncreaseShield();
                        _OnMaxShield();
                    } else
                    {
                        _OnIncreaseShield();
                    }
                }
                else if (value < _shield)
                {
                    _shield = value;
                    if (value <= 0)
                    {
                        _shield = 0;
                        _OnDecreaseShield();
                        _OnMinShield();
                    } else
                    {
                        _OnDecreaseShield();
                    }
                }
                if (Utilities.IsValid(statsAnimator))
                {
                    statsAnimator.SetFloat("shield", Mathf.Clamp01((float)value / (float)maxShield));
                }
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
            }
        }
        [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(health))]
        public int _health = 100;
        public int defaultHealth = 100;
        public int maxHealth = 100;
        public int health
        {
            get => _health;
            set
            {
                if (value > _health)
                {
                    _health = value;
                    if (value >= maxHealth)
                    {
                        _health = maxHealth;
                        _OnIncreaseHealth();
                        _OnMaxHealth();
                    } else
                    {
                        _OnIncreaseHealth();
                    }
                }
                else if (value < _health)
                {
                    _health = value;
                    if (value <= 0)
                    {
                        _health = 0;
                        _OnDecreaseHealth();
                        _OnMinHealth();
                    } else
                    {
                        _OnDecreaseHealth();
                    }
                }
                if (Utilities.IsValid(statsAnimator))
                {
                    statsAnimator.SetFloat("health", Mathf.Clamp01((float)value / (float)maxHealth));
                }
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
            }
        }
        public LayerMask damageLayers;
        [Tooltip("Will automatically set \"health\" and \"shield\" float parameters and a \"team\" integer parameter on this animator")]
        [FieldChangeCallback(nameof(statsAnimator))]
        public Animator _statsAnimator = null;
        public Animator statsAnimator{
            get => _statsAnimator;
            set
            {
                _statsAnimator = value;
                _SetAnimatorValues();
            }
        }

        [Tooltip("Will automatically set event parameters on this animator for the following events: \"OnIncreaseHealth\", \"OnDecreaseHealth\", \"OnMaxHealth\", \"OnMinHealth\", \"OnIncreaseShield\", \"OnDecreaseShield\", \"OnMaxShield\", and \"OnMinShield\". Similar parameters will also be set for your resources, but with the resource name in place of \"Health\" and \"Shield\"")]
        public Animator eventAnimator = null;
        public PlayerListener[] listeners;

        [Tooltip("How tall do we assume a player is if they don't have a head bone on their avatar")]
        public float defaultHeight = 2f;
        [System.NonSerialized, FieldChangeCallback(nameof(syncedResources)), UdonSynced(UdonSyncMode.None)]
        public int[] _syncedResources = { };
        [System.NonSerialized, FieldChangeCallback(nameof(localResources))]
        public int[] _localResources = { };
        int oldValue;
        public int[] syncedResources{
            get => _syncedResources;
            set{
                _syncedResources = value;
                if (!Utilities.IsValid(value) || value.Length == 0)
                {
                    return;
                }
                for (int i = 0; i < value.Length; i++)
                {
                    ResourceManager resource = resources[reverseSyncResourceIdMap[i]];
                    oldValue = i < _syncedResources.Length ? _syncedResources[i] : resource.defaultValue;
                    if (value[i] > oldValue)
                    {
                        if (value[i] >= resource.maxValue)
                        {
                            if (!resource.allowOverflow)
                            {
                                _syncedResources[i] = resource.maxValue;
                            }
                        }
                    }
                    else if (value[i] < oldValue)
                    {
                        if (value[i] <= resource.minValue)
                        {
                            if (!resource.allowOverflow)
                            {
                                _syncedResources[i] = resource.minValue;
                            }
                        }
                    }
                    resource.OnChange(this, oldValue, value[i]);

                }
            }
        }
        public int[] localResources{
            get => _localResources;
            set{
                _localResources = value;
                for(int i = 0; i < value.Length; i++){
                    ResourceManager resource = resources[reverseLocalResourceIdMap[i]];
                    oldValue = i < _localResources.Length ? _localResources[i] : resource.defaultValue;
                    if(value[i] > oldValue){
                        if (value[i] >= resource.maxValue)
                        {
                            if(!resource.allowOverflow){
                                _localResources[i] = resource.maxValue;
                            }
                        }
                    } else if (value[i] < oldValue){
                        if (value[i] <= resource.minValue)
                        {
                            if(!resource.allowOverflow){
                                _localResources[i] = resource.minValue;
                            }
                        }
                    }
                    resource.OnChange(this, oldValue, value[i]);
                }
            }
        }
        [HideInInspector]
        public ResourceManager[] resources;
        [System.NonSerialized]
        public int[] resourceIdMap;
        [System.NonSerialized]
        public int[] reverseSyncResourceIdMap;
        [System.NonSerialized]
        public int[] reverseLocalResourceIdMap;

        public override void _OnOwnerSet()
        {
            ResetPlayer();
            if (Utilities.IsValid(statsAnimator))
            {
                statsAnimator.gameObject.SetActive(true);
            }
            if (Owner.isLocal)
            {
                capsuleCollider.radius = 0.1f;//make it skinny so it's hard to shoot yourself lol
                foreach (Player p in parent.GetComponentsInChildren<Player>(true))
                {
                    p._localPlayerObject = this;
                }
            }
        }

        public override void _OnCleanup()
        {
            if (Utilities.IsValid(statsAnimator))
            {
                statsAnimator.gameObject.SetActive(false);
            }
        }

        public void ResetPlayer()
        {
            ResetPlayerResources();
            if (IsOwnerLocal())
            {
                state = STATE_NORMAL;
                for (int i = 0; i < damageDealt.Length; i++)
                {
                    damageDealt[i] = 0;
                    damageTargetId[i] = -1001;
                }
                for (int i = 0; i < damageMatrix.Length; i++)
                {
                    damageMatrix[i] = 0;
                }
            } else if (Utilities.IsValid(_localPlayerObject))
            {
                damageMatrix[_localPlayerObject.id] = 0;
            }
        }

        public void BuildResourceIdMap()
        {
            if (resources.Length != syncedResources.Length + localResources.Length)
            {
                int syncedRCount = 0;
                int localRCount = 0;
                resourceIdMap = new int[resources.Length];
                for (int i = 0; i < resources.Length; i++)
                {
                    if (resources[i].synced)
                    {
                        resourceIdMap[i] = syncedRCount;
                        syncedRCount++;
                    }
                    else
                    {
                        resourceIdMap[i] = localRCount;
                        localRCount++;
                    }
                }
                reverseSyncResourceIdMap = new int[syncedRCount];
                syncedResources = new int[syncedRCount];
                reverseLocalResourceIdMap = new int[localRCount];
                localResources = new int[localRCount];

                syncedRCount = 0;
                localRCount = 0;
                for (int i = 0; i < resources.Length; i++)
                {
                    if (resources[i].synced)
                    {
                        reverseSyncResourceIdMap[syncedRCount] = i;
                        syncedRCount++;
                    }
                    else
                    {
                        reverseLocalResourceIdMap[localRCount] = i;
                        localRCount++;
                    }
                }
            }
        }

        public void ResetPlayerResources()
        {
            if (IsOwnerLocal())
            {
                shield = defaultShield;
                health = defaultHealth;
            }
            foreach (ResourceManager resource in resources)
            {
                if (!resource.synced || IsOwnerLocal())
                {
                    resource.ResetValue(this);
                }
            }
        }

        [System.NonSerialized] public Vector3 feetPos;
        [System.NonSerialized] public Vector3 headPos;
        private Vector3 leftFootPos;
        private Vector3 rightFootPos;

        public void Update()
        {
            if (!Utilities.IsValid(Owner))
            {
                return;
            }
            leftFootPos = Owner.GetBonePosition(HumanBodyBones.LeftFoot);
            rightFootPos = Owner.GetBonePosition(HumanBodyBones.RightFoot);
            feetPos = leftFootPos != Vector3.zero && rightFootPos != Vector3.zero ? Vector3.Lerp(leftFootPos, rightFootPos, 0.5f) : Owner.GetPosition();
            headPos = Owner.GetBonePosition(HumanBodyBones.Head);
            headPos = headPos != Vector3.zero ? headPos : feetPos + Vector3.up * defaultHeight;

            //assuming the model is 6 heads tall, headPos is at the 5th head, so we need to add 10% (1/5th) to the height
            transform.position = Vector3.Lerp(feetPos, headPos, 2.75f/5f); // this weird ratio because of the 7 heads thing
            capsuleCollider.height = Vector3.Distance(feetPos, headPos) * (6f/5f);
            transform.rotation = Owner.GetRotation() * Quaternion.FromToRotation(Vector3.up, headPos - feetPos);
            if (Utilities.IsValid(statsAnimator))
            {
                statsAnimator.transform.position = headPos;
            }
        }
        public void _SetAnimatorValues()
        {
            if (Utilities.IsValid(statsAnimator))
            {
                foreach (ResourceManager resource in resources)
                {
                    statsAnimator.SetInteger(resource.resourceName, GetResourceValueById(resource.id));
                }
            }
        }
        public int GetResourceId(string resourceName)
        {
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i].resourceName == resourceName)
                {
                    return i;
                }
            }
            return -1001;
        }
        public void SetResource(string resourceName, int value)
        {
            SetResourceValueById(GetResourceId(resourceName), value);
        }
        public void SetResourceValueById(int resourceId, int value)
        {
            if(resourceId < 0 || resourceId >= resources.Length){
                return;
            }
            if (resources[resourceId].synced)
            {
                syncedResources[resourceIdMap[resourceId]] = value;
                RequestSerialization();
            } else
            {
                localResources[resourceIdMap[resourceId]] = value;
            }
        }
        public void ChangeResource(string resourceName, int value)
        {
            ChangeResourceValueById(GetResourceId(resourceName), value);
        }
        public void ChangeResourceValueById(int resourceId, int change)
        {
            if (resourceId < 0 || resourceId >= resources.Length)
            {
                return;
            }
            if (resources[resourceId].synced)
            {
                syncedResources[resourceIdMap[resourceId]] += change;
                RequestSerialization();
            }
            else
            {
                localResources[resourceIdMap[resourceId]] += change;
            }
        }
        public int GetResource(string resourceName)
        {
            return GetResourceValueById(GetResourceId(resourceName));
        }
        public int GetResourceValueById(int resourceId)
        {
            if (resourceId < 0 || resourceId >= resources.Length)
            {
                return -1001;
            }
            if (resources[resourceId].synced)
            {
                return syncedResources[resourceIdMap[resourceId]];
            }
            else
            {
                return localResources[resourceIdMap[resourceId]];
            }
        }

        public void _OnIncreaseHealth()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnIncreaseHealth");
            }
            foreach (PlayerListener listener in listeners)
            {
                listener.OnIncreaseHealth(this, health);
            }
        }

        public void _OnDecreaseHealth()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnDecreaseHealth");
            }
            foreach (PlayerListener listener in listeners)
            {
                listener.OnDecreaseHealth(this, health);
            }
        }
        public void _OnMaxHealth()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnMaxHealth");
            }
            foreach (PlayerListener listener in listeners)
            {
                listener.OnMaxHealth(this, health);
            }
        }

        public void _OnMinHealth()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnMinHealth");
            }
            foreach (PlayerListener listener in listeners)
            {
                listener.OnMinHealth(this, health);
            }
        }
        public void _OnIncreaseShield()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnShieldIncrease");
            }
            foreach (PlayerListener listener in listeners)
            {
                listener.OnIncreaseShield(this, shield);
            }
        }

        public void _OnDecreaseShield()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnShieldDecrease");
            }
            foreach (PlayerListener listener in listeners)
            {
                listener.OnDecreaseShield(this, shield);
            }
        }
        public void _OnMaxShield()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnMaxShield");
            }
            foreach (PlayerListener listener in listeners)
            {
                listener.OnMaxShield(this, shield);
            }
        }

        public void _OnMinShield()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnMinShield");
            }
            foreach (PlayerListener listener in listeners)
            {
                listener.OnMinShield(this, shield);
            }
        }
        public void _SetStatAnimatorForResource(ResourceManager resource, int value)
        {
            if (resource.setAnimationParameterAsRatio)
            {
                if (resource.maxValue - resource.minValue == 0)
                {
                    statsAnimator.SetFloat(resource.resourceName, 0);
                }
                else
                {
                    statsAnimator.SetFloat(resource.resourceName, Mathf.Clamp01((float)(value - resource.minValue) / (float)(resource.maxValue - resource.minValue)));
                }
            }
            else
            {
                statsAnimator.SetInteger(resource.resourceName, resource.maxValue);
            }
        }
        public void _OnChangeResource(ResourceManager resource, int oldValue, int newValue)
        {
        }

        public void OnParticleCollision(GameObject other)
        {
            P_Shooter otherShooter = other.GetComponent<P_Shooter>();
            OnPShooterHit(Utilities.IsValid(otherShooter) ? otherShooter : other.GetComponentInParent<P_Shooter>());
        }
        public void OnTriggerEnter(Collider other)
        {
            if (damageLayers == (damageLayers | (1 << other.gameObject.layer)))
            {
                P_Shooter otherShooter = other.GetComponent<P_Shooter>();
                OnPShooterHit(Utilities.IsValid(otherShooter) ? otherShooter : otherShooter.GetComponentInParent<P_Shooter>());
            } else {
                //heal or something?
            }
        }

        public void OnPShooterHit(P_Shooter otherShooter)
        {
            if (!Utilities.IsValid(otherShooter))
            {
                return;
            }

            _localPlayerObject.SendDamage(otherShooter.calcDamage(), id);
        }

        private int matchingIndex = 0;
        public void SendDamage(int damage, int targetPlayerId)
        {
            //find match if there is one
            for (int i = 0; i < damageTargetId.Length; i++)
            {
                matchingIndex = i;
                if (damageTargetId[i] == targetPlayerId)
                {
                    break;
                }
            }
            //then work backwards from that point to push all elements one forward, overriding the match if there was one
            for (int i = matchingIndex; i > 0; i--)
            {
                damageDealt[i] = damageDealt[i - 1];
                damageTargetId[i] = damageTargetId[i - 1];
            }
            //write the newest damage entry at 0
            damageDealt[0] = damageMatrix[targetPlayerId] + damage;
            damageTargetId[0] = targetPlayerId;
            SyncDamageMatrix();
            RequestSerialization();
        }

        public void ReceiveOtherPlayerDamage(int damage, int attackerId)
        {
            _print("ReceiveOtherPlayerDamage " + damage);
            //here maybe we do a ping or something to show where the damage came from
            ReceiveDamage(damage, false);
        }

        public void ReceiveDamage(int damage, bool ignoreInvincibleAndSpectator)
        {
            if (damage == 0 || !IsOwnerLocal() || (!ignoreInvincibleAndSpectator && !CanTakeDamage()))
            {
                return;
            }
            if (shield <= 0)
            {
                health -= damage;
            } else if (damage > shield)
            {
                health = shield - damage;
                shield = 0;
            } else
            {
                shield -= damage;
            }
        }

        public override void OnDeserialization()
        {
            SyncDamageMatrix();
        }

        public void SyncDamageMatrix()
        {
            if (!Utilities.IsValid(_localPlayerObject))
            {
                return;
            }
            for (int i = 0; i < damageDealt.Length; i++)
            {
                if (damageTargetId[i] < 0)
                {
                    if (i == 0 && !IsOwnerLocal())
                    {
                        //this was a complete wipe, meaning we should reset the damage counter
                        damageMatrix[_localPlayerObject.id] = 0;
                    }
                    return;
                }
                if (damageTargetId[i] == _localPlayerObject.id)//don't send damage messages if we were just resetting our damage messages
                {
                    _localPlayerObject.ReceiveOtherPlayerDamage(damageDealt[i] - damageMatrix[_localPlayerObject.id], id);
                    damageMatrix[_localPlayerObject.id] = damageDealt[i];
                    if (!IsOwnerLocal())
                    {
                        return;
                    }
                } else if (IsOwnerLocal())
                {
                    damageMatrix[damageTargetId[i]] = damageDealt[i];
                }
            }
        }

        public bool IsOwnerLocal()
        {
            return Utilities.IsValid(Owner) && Owner.isLocal;
        }

        public bool CanTakeDamage()
        {
            return state == STATE_NORMAL || state == STATE_FROZEN;
        }
        public bool CanDealDamage()
        {
            return state == STATE_NORMAL || state == STATE_INVINCIBLE;
        }
    }
}
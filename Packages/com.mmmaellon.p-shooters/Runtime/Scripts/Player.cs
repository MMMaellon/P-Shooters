
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Serialization;

namespace MMMaellon.P_Shooters
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
        [HideInInspector]
        public CapsuleCollider capsuleCollider = null;
        public virtual void Start()
        {
            lastHealer = this;
            lastAttacker = this;
            if (id < 0)
            {
                id = transform.GetSiblingIndex();
            }
            parent = transform.parent;
            _localPlayer = Networking.LocalPlayer;
        }

        public virtual void OnEnable()
        {
            //reset all the animator stuff
            _SetAnimatorValues();
        }

        [System.NonSerialized]
        public int id = -1001;
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
                if (IsOwnerLocal())
                {
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
                if (IsOwnerLocal())
                {
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
        // [System.NonSerialized, UdonSynced(UdonSyncMode.None)]
        // public int[] damageDealt = new int[8];
        // [System.NonSerialized, UdonSynced(UdonSyncMode.None)]
        // public int[] damageTargetId = { -1001, -1001, -1001, -1001, -1001, -1001, -1001, -1001 };

        //two rightmost digits are the index, the rest are the damage.
        //if two rightmost are more then 82, then it's a wipe
        // //arbitrarily choose to keep the last 8 
        [System.NonSerialized, UdonSynced(UdonSyncMode.None)]
        public int[] damageSyncCache = { 99, 99, 99, 99, 99, 99, 99, 99 };
        [System.NonSerialized]//we don't serialize this whole thing
        public int[] damageMatrix = new int[82];
        [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(shield))]
        public int _shield = 100;
        [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(maxShield))]
        public int _maxShield = 100;
        public int maxShield
        {
            get => _maxShield;
            set
            {
                if (_maxHealth != value)
                {
                    _maxShield = value;
                    _SetShieldBar();
                    if (IsOwnerLocal())
                    {
                        RequestSerialization();
                    }
                }
            }
        }
        public int shield
        {
            get => _shield;
            set
            {
                _print("set shield from " + _shield + " to " + value);
                if (value > _shield)
                {
                    _shield = value;
                    if (value >= maxShield)
                    {
                        _shield = maxShield;
                        _OnIncreaseShield();
                        _OnMaxShield();
                    }
                    else
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
                    }
                    else
                    {
                        _OnDecreaseShield();
                    }
                }
                _SetShieldBar();
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
            }
        }
        [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(health))]
        public int _health = 100;
        [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(maxHealth))]
        public int _maxHealth = 100;
        public int maxHealth
        {
            get => _maxHealth;
            set
            {
                _maxHealth = value;
                _SetHealthBar();
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
            }
        }
        public int health
        {
            get => _health;
            set
            {
                _print("set health from " + _health + " to " + value);
                if (value > _health)
                {
                    _health = value;
                    if (value >= maxHealth)
                    {
                        _health = maxHealth;
                        _OnIncreaseHealth();
                        _OnMaxHealth();
                    }
                    else
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
                    }
                    else
                    {
                        _OnDecreaseHealth();
                    }
                }
                _SetHealthBar();
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
            }
        }
        [Tooltip("Will automatically set \"health\" and \"shield\" float parameters and a \"team\" integer parameter on this animator")]
        [FieldChangeCallback(nameof(statsAnimator))]
        public Animator _statsAnimator = null;
        public Animator statsAnimator
        {
            get => _statsAnimator;
            set
            {
                _statsAnimator = value;
                _SetAnimatorValues();
            }
        }

        [Tooltip("Will automatically set event parameters on this animator for the following events: \"OnIncreaseHealth\", \"OnDecreaseHealth\", \"OnMaxHealth\", \"OnMinHealth\", \"OnIncreaseShield\", \"OnDecreaseShield\", \"OnMaxShield\", and \"OnMinShield\". Similar parameters will also be set for your resources, but with the resource name in place of \"Health\" and \"Shield\"")]
        public Animator eventAnimator = null;
        public P_ShootersPlayerHandler playerHandler;

        [Tooltip("How tall do we assume a player is if they don't have a head bone on their avatar")]
        public float defaultHeight = 2f;
        [HideInInspector, FieldChangeCallback(nameof(syncedResources)), UdonSynced(UdonSyncMode.None)]
        public int[] _syncedResources = { };
        [HideInInspector, FieldChangeCallback(nameof(localResources))]
        public int[] _localResources = { };
        int oldValue;
        public int[] syncedResources
        {
            get => _syncedResources;
            set
            {
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
        public int[] localResources
        {
            get => _localResources;
            set
            {
                _localResources = value;
                for (int i = 0; i < value.Length; i++)
                {
                    ResourceManager resource = resources[reverseLocalResourceIdMap[i]];
                    oldValue = i < _localResources.Length ? _localResources[i] : resource.defaultValue;
                    if (value[i] > oldValue)
                    {
                        if (value[i] >= resource.maxValue)
                        {
                            if (!resource.allowOverflow)
                            {
                                _localResources[i] = resource.maxValue;
                            }
                        }
                    }
                    else if (value[i] < oldValue)
                    {
                        if (value[i] <= resource.minValue)
                        {
                            if (!resource.allowOverflow)
                            {
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
        [HideInInspector]
        public int[] resourceIdMap;
        [HideInInspector]
        public int[] reverseSyncResourceIdMap;
        [HideInInspector]
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
                if (Utilities.IsValid(playerHandler))
                {
                    playerHandler.localPlayer = this;
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

        public virtual void ResetPlayer()
        {
            ResetPlayerResources();
            if (IsOwnerLocal())
            {
                state = STATE_NORMAL;
                WipeDamageMatrix();
            }
            else if (Utilities.IsValid(_localPlayerObject))
            {
                damageMatrix[_localPlayerObject.id] = 0;
            }
        }

        public virtual void ResetPlayerResources()
        {
            _print("ResetPlayerResources");
            if (IsOwnerLocal())
            {
                shield = playerHandler.startingShield;
                health = playerHandler.startingHealth;
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

        public virtual void Update()
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
            transform.position = Vector3.Lerp(feetPos, headPos, 2.75f / 5f); // this weird ratio because of the 7 heads thing
            capsuleCollider.height = Vector3.Distance(feetPos, headPos) * (6f / 5f);
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
                    if (resource.setAnimationParameterAsRatio)
                    {
                        if (resource.maxValue - resource.minValue == 0)
                        {
                            statsAnimator.SetFloat(resource.resourceName, 0);
                        }
                        else
                        {
                            statsAnimator.SetFloat(resource.resourceName, Mathf.Clamp01((float)(GetResourceValueById(resource.id) - resource.minValue) / (float)(resource.maxValue - resource.minValue)));
                        }
                    }
                    else
                    {
                        statsAnimator.SetInteger(resource.resourceName, resource.maxValue);
                    }
                }

                statsAnimator.SetInteger("team", team);
                statsAnimator.SetInteger("state", state);
                statsAnimator.SetFloat("health", Mathf.Clamp01((float)health / (float)maxHealth));
                statsAnimator.SetFloat("shield", Mathf.Clamp01((float)shield / (float)maxShield));
            }
        }
        public void _SetHealthBar()
        {
            if (Utilities.IsValid(statsAnimator))
            {
                statsAnimator.SetFloat("health", Mathf.Clamp01((float)health / (float)maxHealth));
            }
        }
        public void _SetShieldBar()
        {
            if (Utilities.IsValid(statsAnimator))
            {
                statsAnimator.SetFloat("shield", Mathf.Clamp01((float)shield / (float)maxShield));
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
            if (resourceId < 0 || resourceId >= resources.Length)
            {
                return;
            }
            if (resources[resourceId].synced)
            {
                syncedResources[resourceIdMap[resourceId]] = value;
                RequestSerialization();
            }
            else
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

        [System.NonSerialized] public Player lastHealer = null;
        [System.NonSerialized] public Player lastAttacker = null;

        public virtual void _OnIncreaseHealth()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnIncreaseHealth");
            }
            foreach (PlayerListener listener in playerHandler.playerListeners)
            {
                listener.OnIncreaseHealth(lastHealer, this, health);
            }
        }

        public virtual void _OnDecreaseHealth()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnDecreaseHealth");
            }
            foreach (PlayerListener listener in playerHandler.playerListeners)
            {
                listener.OnDecreaseHealth(lastAttacker, this, health);
            }
        }
        public virtual void _OnMaxHealth()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnMaxHealth");
            }
            foreach (PlayerListener listener in playerHandler.playerListeners)
            {
                listener.OnMaxHealth(lastHealer, this, health);
            }
        }

        public virtual void _OnMinHealth()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnMinHealth");
            }
            foreach (PlayerListener listener in playerHandler.playerListeners)
            {
                listener.OnMinHealth(lastAttacker, this, health);
            }
        }
        public virtual void _OnIncreaseShield()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnShieldIncrease");
            }
            foreach (PlayerListener listener in playerHandler.playerListeners)
            {
                listener.OnIncreaseShield(lastHealer, this, shield);
            }
        }

        public virtual void _OnDecreaseShield()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnShieldDecrease");
            }
            foreach (PlayerListener listener in playerHandler.playerListeners)
            {
                listener.OnDecreaseShield(lastAttacker, this, shield);
            }
        }
        public virtual void _OnMaxShield()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnMaxShield");
            }
            foreach (PlayerListener listener in playerHandler.playerListeners)
            {
                listener.OnMaxShield(lastHealer, this, shield);
            }
        }

        public virtual void _OnMinShield()
        {
            if (Utilities.IsValid(eventAnimator))
            {
                eventAnimator.SetTrigger("OnMinShield");
            }
            foreach (PlayerListener listener in playerHandler.playerListeners)
            {
                listener.OnMinShield(lastAttacker, this, shield);
            }
        }
        public virtual void _SetStatAnimatorForResource(ResourceManager resource, int value)
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
        public virtual void _OnChangeResource(ResourceManager resource, int oldValue, int newValue)
        {
        }
        P_Shooter otherPShooter;

        public virtual void OnParticleCollision(GameObject other)
        {
            if (!Utilities.IsValid(other))
            {
                return;
            }
            otherPShooter = other.GetComponent<P_Shooter>();
            if (!Utilities.IsValid(otherPShooter))
            {
                otherPShooter = other.GetComponentInParent<P_Shooter>();
            }
            if (Utilities.IsValid(otherPShooter) && otherPShooter.damageOnParticleCollision)
            {
                OnPShooterHit(otherPShooter);
            }
        }
        public virtual void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other))
            {
                return;
            }
            if (playerHandler.meleeLayer == (playerHandler.meleeLayer | (1 << other.gameObject.layer)))
            {
                otherPShooter = other.GetComponent<P_Shooter>();
                if (!Utilities.IsValid(otherPShooter))
                {
                    otherPShooter = other.GetComponentInParent<P_Shooter>();
                }
                if (Utilities.IsValid(otherPShooter) && otherPShooter.damageOnTriggerEnter)
                {
                    OnPShooterHit(otherPShooter);
                }
            }
        }

        public virtual void OnPShooterHit(P_Shooter otherShooter)
        {
            if (!Utilities.IsValid(otherShooter))
            {
                return;
            }
            _print("OnPShooterHit");
            if (otherShooter.sync.owner == Owner && !otherShooter.selfDamage)
            {
                return;
            }

            if (!otherShooter.sync.IsLocalOwner())
            {
                return;
            }

            otherShooter.OnHitPlayerFX();

            foreach (PlayerListener listener in playerHandler.playerListeners)
            {
                if (!listener.CanDealDamage(_localPlayerObject, this))
                {
                    return;
                }
            }
            _localPlayerObject.SendDamage(AdjustDamage(otherShooter.CalcDamage()), id);
        }

        int newDamage;
        public virtual int AdjustDamage(int damage)
        {
            newDamage = damage;
            foreach (PlayerListener listener in playerHandler.playerListeners)
            {
                if (listener.ControlsDamage)
                {
                    newDamage = listener.AdjustDamage(_localPlayerObject, this, newDamage);
                }
            }
            return newDamage;
        }

        private int matchingIndex = 0;
        private int tempPlayerId = 99;
        public virtual void SendDamage(int damage, int targetPlayerId)
        {
            if (!CanDealDamage())
            {
                return;
            }
            _print("Sending " + damage + " to " + targetPlayerId);
            matchingIndex = damageSyncCache.Length - 1;
            //find match if there is one
            for (int i = 0; i < damageSyncCache.Length; i++)
            {
                if (Mathf.Abs(damageSyncCache[i]) % 100 == targetPlayerId)
                {
                    matchingIndex = i;
                    break;
                }
            }
            //then work backwards from that point to push all elements one forward, overriding the match if there was one
            for (int i = matchingIndex; i > 0; i--)
            {
                damageSyncCache[i] = damageSyncCache[i - 1];
            }
            //write the newest damage entry at 0
            tempDamage = (damageMatrix[targetPlayerId] + damage) * 100;
            if (tempDamage > 0)
            {
                damageSyncCache[0] = tempDamage + targetPlayerId;
            }
            else
            {
                damageSyncCache[0] = tempDamage - targetPlayerId;
            }
            SyncDamageMatrix();
            RequestSerialization();
        }

        public virtual void ReceiveOtherPlayerDamage(int damage, int attackerId)
        {
            _print("ReceiveOtherPlayerDamage " + damage);
            //here maybe we do a ping or something to show where the damage came from
            if (Utilities.IsValid(playerHandler))
            {
                Player attacker = playerHandler.players[attackerId];
                foreach (PlayerListener listener in playerHandler.playerListeners)
                {
                    if (!listener.CanDealDamage(attacker, this))
                    {
                        return;
                    }
                }
            }
            ReceiveDamage(damage, false);
        }

        int tempShield;
        public virtual void ReceiveDamage(int damage, bool ignoreInvincibleAndSpectator)
        {
            _print("ReceiveDamage " + damage);
            if (damage == 0 || !IsOwnerLocal() || (!ignoreInvincibleAndSpectator && !CanTakeDamage()))
            {
                return;
            }
            if (shield <= 0)
            {
                health -= damage;
            }
            else if (damage > shield)
            {
                _print("damage is more than shield " + damage);
                //we have to subtract shield first because if we subtract health first we'll respawn before shield can be set to zero
                tempShield = shield;
                shield = 0;
                health -= damage - tempShield;
            }
            else
            {
                shield -= damage;
            }
        }


        public virtual void ReceiveOtherPlayerHeal(int heal, int healerId)
        {
            _print("ReceiveOtherPlayerHeal " + heal);
            //here maybe we do a ping or something to show where the damage came from
            if (Utilities.IsValid(playerHandler))
            {
                Player healer = playerHandler.players[healerId];
                foreach (PlayerListener listener in playerHandler.playerListeners)
                {
                    if (!listener.CanDealHeal(healer, this))
                    {
                        return;
                    }
                }
                lastHealer = playerHandler.players[healerId];
            }
            else
            {
                lastHealer = this;
            }
            ReceiveHealth(heal, false);
        }

        public virtual void ReceiveHealth(int heal, bool ignoreInvincibleAndSpectator)
        {
            _print("ReceiveHealth " + heal);
            if (heal == 0 || !IsOwnerLocal() || (!ignoreInvincibleAndSpectator && !CanTakeDamage()))
            {
                return;
            }
            if (health >= maxHealth)
            {
                _print("ReceiveHealth 1");
                shield += heal;
            }
            else if (heal > maxHealth - health)
            {
                _print("ReceiveHealth 2");
                shield += heal - (maxHealth - health);
                health = maxHealth;
            }
            else
            {
                _print("ReceiveHealth 3");
                health += heal;
            }
        }

        public override void OnDeserialization()
        {
            Debug.LogWarning(name + " ondeserialization");
            SyncDamageMatrix();
        }

        int tempDamage;
        int tempDamageChange;
        public virtual void SyncDamageMatrix()
        {
            if (!Utilities.IsValid(_localPlayerObject))
            {
                return;
            }
            _print("SyncDamageMatrix");
            _print("full damage cache:");

            for (int i = 0; i < damageSyncCache.Length; i++)
            {
                _print("damageSyncCache[" + i + "] " + damageSyncCache[i]);
                //set this player to this much damage
                tempPlayerId = Mathf.Abs(damageSyncCache[i]) % 100;
                if (tempPlayerId < 0 || tempPlayerId >= damageMatrix.Length)
                {
                    if (i == 0)
                    {
                        //this was a complete wipe, meaning we should reset the damage counter
                        damageMatrix[_localPlayerObject.id] = 0;
                    }
                    return;
                }

                tempDamage = (damageSyncCache[i] - tempPlayerId) / 100;
                tempDamageChange = tempDamage - damageMatrix[tempPlayerId];
                damageMatrix[tempPlayerId] = tempDamage;
                if (tempDamageChange < 0)
                {
                    playerHandler.players[tempPlayerId].lastHealer = this;
                    if (tempPlayerId == _localPlayerObject.id)//don't send damage messages if we were just resetting our damage messages
                    {
                        _localPlayerObject.ReceiveOtherPlayerHeal(-tempDamageChange, id);
                    }
                }
                else
                {
                    playerHandler.players[tempPlayerId].lastAttacker = this;
                    if (tempPlayerId == _localPlayerObject.id)//don't send damage messages if we were just resetting our damage messages
                    {
                        _localPlayerObject.ReceiveOtherPlayerDamage(tempDamageChange, id);
                    }
                }
            }
        }

        public virtual void WipeDamageMatrix()
        {
            for (int i = 0; i < damageMatrix.Length; i++)
            {
                damageMatrix[i] = 0;
            }
            for (int i = 0; i < damageSyncCache.Length; i++)
            {
                damageSyncCache[i] = 99;
            }
            RequestSerialization();
        }

        public virtual void ConfirmNormalKill()
        {
            if (Utilities.IsValid(Owner))
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(OnNormalKillConfirmed));
            }
        }

        public virtual void ConfirmCriticalKill()
        {
            if (Utilities.IsValid(Owner))
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(OnCriticalKillConfirmed));
            }
        }

        public virtual void ConfirmTeamKill()
        {
            if (Utilities.IsValid(Owner))
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(OnTeamKillConfirmed));
            }
        }

        public virtual void OnNormalKillConfirmed()
        {
            if (Utilities.IsValid(playerHandler))
            {
                foreach (PlayerListener listener in playerHandler.playerListeners)
                {
                    listener.OnReceiveNormalKillConfirmation(this);
                }
            }
        }
        public virtual void OnCriticalKillConfirmed()
        {
            if (Utilities.IsValid(playerHandler))
            {
                foreach (PlayerListener listener in playerHandler.playerListeners)
                {
                    listener.OnReceiveCriticalKillConfirmation(this);
                }
            }
        }
        public virtual void OnTeamKillConfirmed()
        {
            if (Utilities.IsValid(playerHandler))
            {
                foreach (PlayerListener listener in playerHandler.playerListeners)
                {
                    listener.OnReceiveTeamKillConfirmation(this);
                }
            }
        }

        public bool IsOwnerLocal()
        {
            return Utilities.IsValid(Owner) && Owner.isLocal;
        }

        public virtual bool CanTakeDamage()
        {
            return state == STATE_NORMAL || state == STATE_FROZEN;
        }
        public virtual bool CanDealDamage()
        {
            return state == STATE_NORMAL || state == STATE_INVINCIBLE;
        }
    }
}
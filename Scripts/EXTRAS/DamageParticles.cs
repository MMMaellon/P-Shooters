
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace MMMaellon
{
    public class DamageParticles : UdonSharpBehaviour
    {
        public PlayerHandler player_handler;
        public int damage_amount = 10;
        public bool damage_ignores_shield = false;
        public bool self_damage = false;
        [Header("For Favor the shooter to work, either call the 'TakeOwnership' event or set the owner some other way")]
        public bool favor_the_shooter = true;

        void Start()
        {
            if (player_handler == null)
            {
                player_handler = GameObject.Find("__GUN MANAGER__").GetComponentInChildren<PlayerHandler>();
            }
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            if (player_handler == null)
            {
                player_handler = GameObject.FindObjectOfType<PlayerHandler>();
            }
        }
#endif

        public void OnParticleCollision(GameObject other)
        {
            if (player_handler == null || player_handler._localPlayer == null || player_handler._localPlayer.Owner == null || (player_handler.scores != null && (player_handler._localPlayer.team == 0 || !player_handler.scores.game_active)) || !Utilities.IsValid(other) || other == null)
            {
                return;
            }
            Player otherPlayer = other.GetComponent<Player>();
            if (otherPlayer == null || otherPlayer.Owner == null)
            {
                return;
            }

            if (favor_the_shooter && !Networking.LocalPlayer.IsOwner(gameObject))
            {
                return;
            }
            else if (!favor_the_shooter && player_handler._localPlayer.id != otherPlayer.id)
            {
                return;
            }

            if (favor_the_shooter)
            {
                if (otherPlayer.Owner.isLocal && !self_damage)
                {
                    return;
                }

                otherPlayer.TriggerHitFX(damage_amount);
            }

            if (otherPlayer.Owner.isLocal)
            {
                LowerHealth();
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(Shoot) + otherPlayer.id);
            }
        }

        public void LowerHealth()
        {
            if (player_handler._localPlayer.last_death + 3f > Time.timeSinceLevelLoad || player_handler._localPlayer.last_safe + 1f > Time.timeSinceLevelLoad)//invincible for 3 seconds
            {
                return;
            }
            player_handler.LowerHealth(damage_amount, damage_ignores_shield);
            if (favor_the_shooter && player_handler._localPlayer.health <= 0)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(OnKillPlayer));
            }
        }

        public void OnKillPlayer()
        {
            player_handler._localPlayer.GotKill();
        }

        public void TakeOwnership()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public void Shoot(int player_id)
        {
            if (player_id == player_handler._localPlayer.id)
            {
                LowerHealth();
            }
        }

        public void Shoot0()
        {
            Shoot(0);
        }
        public void Shoot1()
        {
            Shoot(1);
        }
        public void Shoot2()
        {
            Shoot(2);
        }
        public void Shoot3()
        {
            Shoot(3);
        }
        public void Shoot4()
        {
            Shoot(4);
        }
        public void Shoot5()
        {
            Shoot(5);
        }
        public void Shoot6()
        {
            Shoot(6);
        }
        public void Shoot7()
        {
            Shoot(7);
        }
        public void Shoot8()
        {
            Shoot(8);
        }
        public void Shoot9()
        {
            Shoot(9);
        }
        public void Shoot10()
        {
            Shoot(10);
        }
        public void Shoot11()
        {
            Shoot(11);
        }
        public void Shoot12()
        {
            Shoot(12);
        }
        public void Shoot13()
        {
            Shoot(13);
        }
        public void Shoot14()
        {
            Shoot(14);
        }
        public void Shoot15()
        {
            Shoot(15);
        }
        public void Shoot16()
        {
            Shoot(16);
        }
        public void Shoot17()
        {
            Shoot(17);
        }
        public void Shoot18()
        {
            Shoot(18);
        }
        public void Shoot19()
        {
            Shoot(19);
        }
        public void Shoot20()
        {
            Shoot(20);
        }
        public void Shoot21()
        {
            Shoot(21);
        }
        public void Shoot22()
        {
            Shoot(22);
        }
        public void Shoot23()
        {
            Shoot(23);
        }
        public void Shoot24()
        {
            Shoot(24);
        }
        public void Shoot25()
        {
            Shoot(25);
        }
        public void Shoot26()
        {
            Shoot(26);
        }
        public void Shoot27()
        {
            Shoot(27);
        }
        public void Shoot28()
        {
            Shoot(28);
        }
        public void Shoot29()
        {
            Shoot(29);
        }
        public void Shoot30()
        {
            Shoot(30);
        }
        public void Shoot31()
        {
            Shoot(31);
        }
        public void Shoot32()
        {
            Shoot(32);
        }
        public void Shoot33()
        {
            Shoot(33);
        }
        public void Shoot34()
        {
            Shoot(34);
        }
        public void Shoot35()
        {
            Shoot(35);
        }
        public void Shoot36()
        {
            Shoot(36);
        }
        public void Shoot37()
        {
            Shoot(37);
        }
        public void Shoot38()
        {
            Shoot(38);
        }
        public void Shoot39()
        {
            Shoot(39);
        }
        public void Shoot40()
        {
            Shoot(40);
        }
        public void Shoot41()
        {
            Shoot(41);
        }
        public void Shoot42()
        {
            Shoot(42);
        }
        public void Shoot43()
        {
            Shoot(43);
        }
        public void Shoot44()
        {
            Shoot(44);
        }
        public void Shoot45()
        {
            Shoot(45);
        }
        public void Shoot46()
        {
            Shoot(46);
        }
        public void Shoot47()
        {
            Shoot(47);
        }
        public void Shoot48()
        {
            Shoot(48);
        }
        public void Shoot49()
        {
            Shoot(49);
        }
        public void Shoot50()
        {
            Shoot(50);
        }
        public void Shoot51()
        {
            Shoot(51);
        }
        public void Shoot52()
        {
            Shoot(52);
        }
        public void Shoot53()
        {
            Shoot(53);
        }
        public void Shoot54()
        {
            Shoot(54);
        }
        public void Shoot55()
        {
            Shoot(55);
        }
        public void Shoot56()
        {
            Shoot(56);
        }
        public void Shoot57()
        {
            Shoot(57);
        }
        public void Shoot58()
        {
            Shoot(58);
        }
        public void Shoot59()
        {
            Shoot(59);
        }
        public void Shoot60()
        {
            Shoot(60);
        }
        public void Shoot61()
        {
            Shoot(61);
        }
        public void Shoot62()
        {
            Shoot(62);
        }
        public void Shoot63()
        {
            Shoot(63);
        }
        public void Shoot64()
        {
            Shoot(64);
        }
        public void Shoot65()
        {
            Shoot(65);
        }
        public void Shoot66()
        {
            Shoot(66);
        }
        public void Shoot67()
        {
            Shoot(67);
        }
        public void Shoot68()
        {
            Shoot(68);
        }
        public void Shoot69()
        {
            Shoot(69);
        }
        public void Shoot70()
        {
            Shoot(70);
        }
        public void Shoot71()
        {
            Shoot(71);
        }
        public void Shoot72()
        {
            Shoot(72);
        }
        public void Shoot73()
        {
            Shoot(73);
        }
        public void Shoot74()
        {
            Shoot(74);
        }
        public void Shoot75()
        {
            Shoot(75);
        }
        public void Shoot76()
        {
            Shoot(76);
        }
        public void Shoot77()
        {
            Shoot(77);
        }
        public void Shoot78()
        {
            Shoot(78);
        }
        public void Shoot79()
        {
            Shoot(79);
        }
        public void Shoot80()
        {
            Shoot(80);
        }
        public void Shoot81()
        {
            Shoot(81);
        }
    }
}
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.P_Shooters
{
    public abstract class PlayerListener : UdonSharpBehaviour
    {
        public P_ShootersPlayerHandler playerHandler;
        public bool ControlsDamage;
        public virtual void OnIncreaseShield(Player healer, Player receiver, int value){
            
        }
        public virtual void OnDecreaseShield(Player attacker, Player receiver, int value){
            
        }
        public virtual void OnMaxShield(Player healer, Player receiver, int value){
            
        }
        public virtual void OnMinShield(Player attacker, Player receiver, int value){
            
        }
        public virtual void OnIncreaseHealth(Player healer, Player receiver, int value){
            
        }
        public virtual void OnDecreaseHealth(Player attacker, Player receiver, int value){
            
        }
        public virtual void OnMaxHealth(Player healer, Player receiver, int value){
            
        }
        public virtual void OnMinHealth(Player attacker, Player receiver, int value){

        }
        public virtual bool CanDealHeal(Player healer, Player receiver)
        {
            return true;
        }
        public virtual bool CanDealDamage(Player attacker, Player receiver)
        {
            return true;
        }

        public virtual int AdjustDamage(Player attacker, Player receiver, int damage)
        {
            return damage;
        }

        public virtual void OnReceiveNormalKillConfirmation(Player attacker)
        {

        }
        public virtual void OnReceiveCriticalKillConfirmation(Player attacker)
        {

        }
        public virtual void OnReceiveTeamKillConfirmation(Player attacker)
        {

        }

    }
}
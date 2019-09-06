using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    public class PowerAttackBehavior : MonoBehaviour, ISpecialAbility
    {
        PowerAttackConfig config;

        public void SetConfig(PowerAttackConfig configToSet)
        {
            this.config = configToSet;
        }

        private void Start()
        {
            print("Power Attack behavior attached to: " + gameObject.name);
        }
        
        public void Use(AbilityUseParams useParams)
        {
            DealDamage(useParams);
            PlayParticleEffect();
        }

        private void DealDamage(AbilityUseParams useParams)
        {
            print("Power Attack used by: " + gameObject.name);
            float damageToDeal = useParams.baseDamage + config.GetExtraDamage();
            useParams.target.AdjustHealth(damageToDeal);
        }

        private void PlayParticleEffect()
        {
            var prefab = Instantiate(config.GetParticlePrefab(), transform.position, Quaternion.identity);
            ParticleSystem myParticleSystem = prefab.GetComponent<ParticleSystem>();
            myParticleSystem.Play();
            Destroy(prefab, myParticleSystem.main.duration);
        }
    }
}



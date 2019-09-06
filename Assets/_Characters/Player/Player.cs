using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using RPG.CameraUI; //TODO Rewire?
using RPG.Weapons;
using RPG.Core;
using System;
using System.Collections;

namespace RPG.Characters
{
    public class Player : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHealthPoints = 100f;
        [SerializeField] float baseDamage = 10f;        
        [SerializeField] Weapon weaponInUse = null;
        [SerializeField] AnimatorOverrideController animatorOverrideController = null;
        [SerializeField] AudioClip[] damageSounds;
        [SerializeField] AudioClip[] deathSounds;

        [SerializeField] SpecialAbility[] abilities = null;

        const string DEATH_TRIGGER = "Death";
        const string ATTACK_TRIGGER = "Attack";

        AudioSource audioSource;
        Animator animator;    
        float currentHealthPoints;
        CameraRaycaster cameraRaycaster;
        float lastHitTime = 0f;

        public float healthAsPercentage { get { return currentHealthPoints / maxHealthPoints; } }

        void Start()
        {
            RegisterForMouseClick();
            SetCurrentMaxHealth();
            PutWeaponInHand();
            SetupRuntimeAnimator();
            abilities[0].AttachComponentTo(gameObject);
            audioSource = GetComponent<AudioSource>();
        }

        public void TakeDamage(float damage)
        {
            bool playerDies = (currentHealthPoints - damage <= 0); // must ask before reducing heatlh
            ReduceHealth(damage);              
            if (playerDies)
            {                
                StartCoroutine(KillPlayer());
            }            
        }

        IEnumerator KillPlayer()
        {
            animator.SetTrigger(DEATH_TRIGGER);

            audioSource.clip = deathSounds[UnityEngine.Random.Range(0, deathSounds.Length)];
            audioSource.Play();
            yield return new WaitForSecondsRealtime(audioSource.clip.length);
            
            SceneManager.LoadScene(0);
        }

        private void ReduceHealth(float damage)
        {
            currentHealthPoints = Mathf.Clamp(currentHealthPoints - damage, 0f, maxHealthPoints);
            audioSource.clip = damageSounds[UnityEngine.Random.Range(0, damageSounds.Length)];
            audioSource.Play();
        }

        private void SetCurrentMaxHealth()
        {
            currentHealthPoints = maxHealthPoints;
        }

        private void SetupRuntimeAnimator()
        {
            animator = GetComponent<Animator>();
            animator.runtimeAnimatorController = animatorOverrideController;
            animatorOverrideController["DEFAULT ATTACK"] = weaponInUse.GetAttackAnimClip();
        }

        private void PutWeaponInHand()
        {
            GameObject dominantHand = RequestDominantHand();
            var weaponPrefab = weaponInUse.GetWeaponPrefab();
            var weapon = Instantiate(weaponPrefab, dominantHand.transform);
            weapon.transform.localPosition = weaponInUse.gripTransform.localPosition;
            weapon.transform.localRotation = weaponInUse.gripTransform.localRotation;
        }

        private GameObject RequestDominantHand()
        {
            var dominantHands = GetComponentsInChildren<DominantHand>();
            int numberOfDominantHands = dominantHands.Length;
            Assert.IsFalse(numberOfDominantHands <= 0, "No dominantHand found on player.");
            Assert.IsFalse(numberOfDominantHands > 1, "Multiple dominantHands found");
            return dominantHands[0].gameObject;
        }

        private void RegisterForMouseClick()
        {
            cameraRaycaster = FindObjectOfType<CameraRaycaster>();
            cameraRaycaster.onMouseOverEnemy += OnMouseOverEnemy;
        }

        private void OnMouseOverEnemy(Enemy enemy)
        {
            if (Input.GetMouseButton(0) && IsTargetInRange(enemy.gameObject))
            {
                AttackTarget(enemy);
            }

            else if (Input.GetMouseButtonDown(1))
            {
                AttemptSpecialAbillity(0, enemy);
            }
        }

        private void AttemptSpecialAbillity(int abilityIndex, Enemy enemy)
        {
            var energyComponent = GetComponent<Energy>();
            var energyCost = abilities[abilityIndex].GetEnergyCost();

            if (energyComponent.IsEnergyAvailable(energyCost))
            {
                energyComponent.ConsumeEnergy(energyCost);
                var abilityParams = new AbilityUseParams(enemy, baseDamage);
                abilities[abilityIndex].Use(abilityParams);
            }
        }

        private void AttackTarget(Enemy enemy)
        {
            if (Time.time - lastHitTime > weaponInUse.GetMinTimeBetweenHits())
            {
                animator.SetTrigger(ATTACK_TRIGGER);
                enemy.TakeDamage(baseDamage);
                lastHitTime = Time.time;
            }
        }

        private bool IsTargetInRange(GameObject target)
        {
            float distanceToTarget = (target.transform.position - transform.position).magnitude;
            return distanceToTarget <= weaponInUse.GetMaxAttackRange();
        }

        
    }

}


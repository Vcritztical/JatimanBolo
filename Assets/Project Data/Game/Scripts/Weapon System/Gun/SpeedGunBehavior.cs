using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class SpeedGunBehavior : BaseGunBehavior
    {
        [LineSpacer]
        [SerializeField] ParticleSystem shootParticleSystem;
        [SerializeField] LayerMask targetLayers;
        [SerializeField] float bulletDisableTime = 2f;

        private float attackDelay;
        private DuoFloat bulletSpeed;
        private float bulletSpreadAngle;

        private float nextShootTime;
        private float lastShootTime;

        private Pool bulletPool;
        private TweenCase shootTweenCase;

        // Pakai BaseWeaponUpgrade biar fleksibel (Bisa Rgb atau Rifle upgrade)
        private BaseWeaponUpgrade upgrade;

        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            base.Initialise(characterBehaviour, data);

            // 1. Ambil Upgrade secara umum (Base)
            upgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(data.UpgradeType);

            if (upgrade == null)
            {
                Debug.LogError($"[SpeedGun] Upgrade '{data.UpgradeType}' tidak ditemukan! Cek nama Upgrade di Weapons Database.");
                return;
            }

            // 2. Setup Pool Peluru
            var stage = upgrade.GetCurrentStage() as BaseWeaponUpgradeStage;
            if (stage != null && stage.BulletPrefab != null)
            {
                bulletPool = new Pool(new PoolSettings(stage.BulletPrefab.name + "_Rifle", stage.BulletPrefab, 15, true));
                Debug.Log("[SpeedGun] Pool peluru SIAP!");
            }
            else
            {
                Debug.LogError("[SpeedGun] Bullet Prefab di ScriptableObject Upgrade masih KOSONG!");
            }

            RecalculateDamage();
        }

        public override void GunUpdate()
        {
            if (upgrade == null || bulletPool == null || characterBehaviour == null || shootPoint == null) return;

            // Update UI Reload
            float reloadProgress = (Time.timeSinceLevelLoad - lastShootTime) / attackDelay;
            AttackButtonBehavior.SetReloadFill(1 - Mathf.Clamp01(reloadProgress));

            if (!characterBehaviour.IsCloseEnemyFound) return;

            Vector3 shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;


            if (Physics.Raycast(shootPoint.position, shootDirection, out var hitInfo, 100f, targetLayers))
            {
                if (hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
                {

                    if (Vector3.Angle(shootDirection, transform.forward) < 45f)
                    {
                        characterBehaviour.SetTargetActive();

                        if (nextShootTime < Time.timeSinceLevelLoad && characterBehaviour.IsAttackingAllowed)
                        {
                            Shoot();
                        }
                        return;
                    }
                }
            }

            characterBehaviour.SetTargetUnreachable();
        }

        private void Shoot()
        {
            nextShootTime = Time.timeSinceLevelLoad + attackDelay;
            lastShootTime = Time.timeSinceLevelLoad;

            AttackButtonBehavior.SetReloadFill(0);

            // Efek Recoil
            shootTweenCase.KillActive();
            shootTweenCase = transform.DOLocalMoveZ(-0.15f, 0.05f).OnComplete(() => transform.DOLocalMoveZ(0, 0.1f));

            if (shootParticleSystem != null) shootParticleSystem.Play();


            var currentStage = upgrade.GetCurrentStage() as BaseWeaponUpgradeStage;
            int bulletsCount = currentStage.BulletsPerShot.Random();

            for (int i = 0; i < bulletsCount; i++)
            {
                GameObject bulletObj = bulletPool.GetPooledObject(new PooledObjectSettings()
                    .SetPosition(shootPoint.position)
                    .SetEulerRotation(characterBehaviour.transform.eulerAngles));

                PlayerBulletBehavior bulletScript = bulletObj.GetComponent<PlayerBulletBehavior>();
                if (bulletScript != null)
                {
                    bulletScript.Initialise(damage.Random() * characterBehaviour.Stats.BulletDamageMultiplier, bulletSpeed.Random(), characterBehaviour.ClosestEnemyBehaviour, bulletDisableTime);

                    if (bulletSpreadAngle > 0)
                        bulletObj.transform.Rotate(0, Random.Range(-bulletSpreadAngle * 0.5f, bulletSpreadAngle * 0.5f), 0);
                }
            }

            characterBehaviour.OnGunShooted();
            if (characterBehaviour.MainCameraCase != null) characterBehaviour.MainCameraCase.Shake(0.04f, 0.04f, 0.2f, 0.7f);
            AudioController.PlaySound(AudioController.Sounds.shotShotgun);
        }

        public override void RecalculateDamage()
        {
            if (upgrade == null) return;
            var stage = upgrade.GetCurrentStage() as BaseWeaponUpgradeStage;
            if (stage == null) return;

            damage = stage.Damage;
            bulletSpreadAngle = stage.Spread;
            attackDelay = 1f / Mathf.Max(0.1f, stage.FireRate);
            bulletSpeed = stage.BulletSpeed;
        }

        public override void OnLevelLoaded() => RecalculateDamage();

        public override void OnGunUnloaded()
        {
            if (bulletPool != null) { bulletPool.Clear(); bulletPool = null; }
        }

        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {

            transform.SetParent(characterGraphics.MinigunHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload() => bulletPool?.ReturnToPoolEverything();
    }
}
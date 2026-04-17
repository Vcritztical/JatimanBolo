using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class RgbGunBehavior : BaseGunBehavior
    {
        [LineSpacer]
        [SerializeField] ParticleSystem shootParticleSystem;

        [SerializeField] LayerMask targetLayers;
        [SerializeField] float bulletDisableTime;

        private float attackDelay;
        private DuoFloat bulletSpeed;
        private float bulletSpreadAngle;

        private float nextShootTime;
        private float lastShootTime;

        private Pool bulletPool;

        private TweenCase shootTweenCase;
        private Vector3 shootDirection;

        private RgbUpgrade upgrade;

        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            base.Initialise(characterBehaviour, data);

            upgrade = UpgradesController.GetUpgrade<RgbUpgrade>(data.UpgradeType);

            if (upgrade == null) return;

            var stage = upgrade.CurrentStage as BaseWeaponUpgradeStage;
            if (stage == null || stage.BulletPrefab == null) return;

            GameObject bulletObj = stage.BulletPrefab;
            bulletPool = new Pool(new PoolSettings(bulletObj.name, bulletObj, 5, true));

            RecalculateDamage();
        }

        public override void OnLevelLoaded()
        {
            RecalculateDamage();
        }

        public override void RecalculateDamage()
        {
            if (upgrade == null) return;

            var stage = upgrade.GetCurrentStage();
            if (stage == null) return;

            damage = stage.Damage;
            bulletSpreadAngle = stage.Spread;
            attackDelay = 1f / stage.FireRate;
            bulletSpeed = stage.BulletSpeed;
        }

        public override void GunUpdate()
        {
            if (upgrade == null || bulletPool == null) return;

            // Update UI reload fill
            AttackButtonBehavior.SetReloadFill(1 - (Time.timeSinceLevelLoad - lastShootTime) / (nextShootTime - lastShootTime));

            // Jika musuh tidak terdeteksi oleh EnemyDetector, hentikan logika
            if (!characterBehaviour.IsCloseEnemyFound)
                return;

            // Kalkulasi arah dari moncong ke musuh
            shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;

            // PENTING: Raycast dimulai dari shootPoint agar tidak menabrak collider player sendiri
            if (Physics.Raycast(shootPoint.position, shootDirection, out var hitInfo, 300f, targetLayers))
            {
                if (hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
                {
                    // Cek sudut bidikan
                    if (Vector3.Angle(shootDirection, transform.forward) < 40f)
                    {
                        // MEMUNCULKAN LINGKARAN PUTIH
                        characterBehaviour.SetTargetActive();

                        // Cek jeda tembak dan input serangan
                        if (nextShootTime >= Time.timeSinceLevelLoad) return;
                        if (!characterBehaviour.IsAttackingAllowed) return;

                        AttackButtonBehavior.SetReloadFill(0);

                        // Efek recoil senjata
                        shootTweenCase.KillActive();
                        shootTweenCase = transform.DOLocalMoveZ(-0.15f, 0.1f).OnComplete(delegate
                        {
                            shootTweenCase = transform.DOLocalMoveZ(0, 0.15f);
                        });

                        if (shootParticleSystem != null) shootParticleSystem.Play();

                        nextShootTime = Time.timeSinceLevelLoad + attackDelay;
                        lastShootTime = Time.timeSinceLevelLoad;

                        // Spawn Peluru
                        int bulletsNumber = upgrade.GetCurrentStage().BulletsPerShot.Random();
                        for (int i = 0; i < bulletsNumber; i++)
                        {
                            PlayerBulletBehavior bullet = bulletPool.GetPooledObject(new PooledObjectSettings().SetPosition(shootPoint.position).SetEulerRotation(characterBehaviour.transform.eulerAngles)).GetComponent<PlayerBulletBehavior>();
                            bullet.Initialise(damage.Random() * characterBehaviour.Stats.BulletDamageMultiplier, bulletSpeed.Random(), characterBehaviour.ClosestEnemyBehaviour, bulletDisableTime);

                            // Spread peluru
                            bullet.transform.Rotate(new Vector3(0f, i == 0 ? 0f : Random.Range(bulletSpreadAngle * -0.5f, bulletSpreadAngle * 0.5f), 0f));
                        }

                        characterBehaviour.OnGunShooted();
                        characterBehaviour.MainCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);

                        AudioController.PlaySound(AudioController.Sounds.shotShotgun);
                    }
                    else
                    {
                        characterBehaviour.SetTargetUnreachable();
                    }
                }
                else
                {
                    characterBehaviour.SetTargetUnreachable();
                }
            }
            else
            {
                characterBehaviour.SetTargetUnreachable();
            }
        }

        private void OnDrawGizmos()
        {
            if (characterBehaviour == null || characterBehaviour.ClosestEnemyBehaviour == null || shootPoint == null)
                return;

            Color defCol = Gizmos.color;
            Gizmos.color = Color.magenta;

            Vector3 dir = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;
            Gizmos.DrawRay(shootPoint.position, dir.normalized * 10f);

            Gizmos.color = defCol;
        }

        public override void OnGunUnloaded()
        {
            if (bulletPool != null)
            {
                bulletPool.Clear();
                bulletPool = null;
            }
        }

        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.ShootGunHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            bulletPool?.ReturnToPoolEverything();
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Game.Managers;
using Game.Utility;
using UnityEngine;

namespace Game.Systems.Spirit.Internal
{
    [Serializable]
    public class ShootSystem
    {
        public event Action<BulletSystem> BulletHit;
        public event Action PrepareToShoot;
        public event Action<BulletSystem> Shooting;

        public int ShotCount { get => shotCount; set => shotCount = value > 0 ? value : 0; }
        public bool isHaveChainTargets;

        List<BulletSystem> bullets = new List<BulletSystem>();
        ICombatComponent owner;
        ObjectPool bulletPool;
        WaitForSeconds delayBetweenAttacks;
        WaitForSeconds bulletLifetime;
        Vector3 bulletSpawnPosition;
        bool canShoot = true;
        double currentAttackCooldown;
        int shotCount;
        double attackSpeed;
        double attackDelay;

        public ShootSystem(ICombatComponent owner, Vector3 bulletSpawnPosition)
        {
            this.owner = owner;
            this.bulletSpawnPosition = bulletSpawnPosition;
        }

        void OnDestroy() => bulletPool.DestroyPool();

        public void Set(GameObject bullet)
        {
            bulletPool = new ObjectPool(bullet, owner.Prefab.transform, 2);
        }

        public void UpdateSystem(double attackSpeed, double attackDelay)
        {
            if (canShoot)
            {
                RefreshAttackCooldown();
                ShootBullet();
                GameLoop.Instance.StartCoroutine(CooldownAttack());
            }

            UpdateBullets();

            IEnumerator CooldownAttack()
            {
                canShoot = false;
                yield return delayBetweenAttacks;
                canShoot = true;
            }

            void RefreshAttackCooldown()
            {
                var newAttackCooldown = GetAttackCooldown();

                if (currentAttackCooldown != newAttackCooldown)
                {
                    currentAttackCooldown = newAttackCooldown;
                    delayBetweenAttacks = new WaitForSeconds((float)newAttackCooldown);
                }

                double GetAttackCooldown()
                {
                    var modifiedAttackDelay = attackDelay.GetPercent(attackSpeed);

                    return attackSpeed < 100 ?
                        attackDelay + (attackDelay - modifiedAttackDelay) :
                        attackDelay - (modifiedAttackDelay - attackDelay);
                }
            }

            void ShootBullet()
            {
                PrepareToShoot?.Invoke();

                for (int i = 0; i < shotCount; i++)
                {
                    CreateBullet(owner.Targets[i]);
                }

                void CreateBullet(IHealthComponent target)
                {
                    var newBullet = bullets.Find(bullet => bullet.Prefab.activeSelf);

                    if (newBullet == null)
                    {
                        newBullet = new BulletSystem(bulletPool.PopObject(), bulletSpawnPosition);
                        bullets.Add(newBullet);
                    }

                    if (target != null)
                    {
                        newBullet.Target = target;
                    }
                    else
                    {
                        newBullet.SetActive(false);
                    }

                    if (bulletLifetime == null)
                    {
                        bulletLifetime = new WaitForSeconds(newBullet.Lifetime);
                    }

                    Shooting?.Invoke(newBullet);
                }
            }
        }

        public void SetTargetReached(BulletSystem bullet)
        {
            if (!bullet.IsTargetReached)
            {
                bullet.IsTargetReached = true;
                GameLoop.Instance.StartCoroutine(RemoveBullet());
            }

            IEnumerator RemoveBullet()
            {
                yield return bulletLifetime;
                bullet.SetActive(false);
            }
        }

        public void UpdateBullets()
        {
            bullets.ForEach(bullet =>
            {
                if (!bullet.IsTargetReached)
                {
                    if (bullet.Target == null || bullet.Target.Prefab == null)
                    {
                        SetTargetReached(bullet);
                    }
                    else
                    {
                        if (bullet.DistanceToTarget > 30)
                        {
                            bullet.Update();
                        }
                        else
                        {
                            SetTargetReached(bullet);
                            BulletHit?.Invoke(bullet);
                            return;
                        }
                    }
                }
            });
        }
    }
}
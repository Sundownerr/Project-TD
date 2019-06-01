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
        List<float> removeTimers = new List<float>();
        SpiritSystem ownerSpirit;
        ObjectPool bulletPool;
        WaitForSeconds delayBetweenAttacks;
        bool canShoot = true;
        double currentAttackCooldown;
        int shotCount;

        public ShootSystem(SpiritSystem spirit) => ownerSpirit = spirit;

        void OnDestroy() => bulletPool.DestroyPool();

        public void Set(GameObject bullet)
        {
            bulletPool = new ObjectPool(bullet, ownerSpirit.Prefab.transform, 2);
        }

        public void UpdateSystem()
        {
            if (canShoot)
            {
                var newAttackCooldown = GetAttackCooldown();

                if (currentAttackCooldown != newAttackCooldown)
                {
                    currentAttackCooldown = newAttackCooldown;
                    delayBetweenAttacks = new WaitForSeconds((float)newAttackCooldown);
                }

                canShoot = false;
                ShotBullet();
                GameLoop.Instance.StartCoroutine(CooldownAttack());
            }

            Shoot();

            for (int i = 0; i < removeTimers.Count; i++)
            {
                if (removeTimers[i] > 0)
                {
                    removeTimers[i] -= Time.deltaTime;
                }
                else
                {
                    RemoveBullet(bullets[0]);
                    removeTimers.RemoveAt(i);
                }
            }

            IEnumerator CooldownAttack()
            {
                yield return delayBetweenAttacks;
                canShoot = true;
            }

            double GetAttackCooldown()
            {
                var attackDelay = ownerSpirit.Data.Get(Enums.Spirit.AttackDelay).Sum;
                var attackSpeed = ownerSpirit.Data.Get(Enums.Spirit.AttackSpeed).Sum;

                var modifiedAttackDelay = attackDelay.GetPercent(attackSpeed);

                return attackSpeed < 100 ?
                    attackDelay + (attackDelay - modifiedAttackDelay) :
                    attackDelay - (modifiedAttackDelay - attackDelay);
            }

            void ShotBullet()
            {
                PrepareToShoot?.Invoke();

                for (int i = 0; i < shotCount; i++)
                {
                    bullets.Add(CreateBullet(ownerSpirit.Targets[i]));
                }

                BulletSystem CreateBullet(IHealthComponent target)
                {
                    var newBullet = new BulletSystem(bulletPool.PopObject());

                    SetBulletData();

                    Shooting?.Invoke(newBullet);
                    newBullet.Prefab.SetActive(true);

                    return newBullet;

                    void SetBulletData()
                    {
                        newBullet.Prefab.transform.position = ownerSpirit.ShootPoint.position;
                        newBullet.Prefab.transform.rotation = ownerSpirit.MovingPart.rotation;
                        newBullet.IsTargetReached = false;

                        if (target != null)
                        {
                            newBullet.Target = target;
                        }
                        else
                        {
                            RemoveBullet(newBullet);
                        }
                    }
                }
            }

            void RemoveBullet(BulletSystem bullet)
            {
                bullet.Prefab.SetActive(false);
                bullets.Remove(bullet);
            }
        }

        public void SetTargetReached(BulletSystem bullet)
        {
            if (!bullet.IsTargetReached)
            {
                bullet.IsTargetReached = true;
                removeTimers.Add(bullet.Lifetime);
            }
        }

        public void Shoot()
        {
            for (int i = 0; i < bullets.Count; i++)
            {
                var bullet = bullets[i];

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
            }
        }
    }
}
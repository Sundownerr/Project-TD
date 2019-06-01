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
        List<GameObject> bulletGOs = new List<GameObject>();
        List<float> removeTimers = new List<float>();
        SpiritSystem ownerSpirit;
        ObjectPool bulletPool;
        WaitForSeconds delayBetweenAttacks;
        bool canShoot = true;
        double previousAttackCooldown;
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
                var attackCooldown = CalculateAttackCooldown();

                if (previousAttackCooldown != attackCooldown)
                {
                    previousAttackCooldown = attackCooldown;
                    delayBetweenAttacks = new WaitForSeconds((float)attackCooldown);
                }

                canShoot = false;
                ShotBullet();
                GameLoop.Instance.StartCoroutine(AttackCooldown());
            }

            Shoot();

            for (int i = 0; i < removeTimers.Count; i++)
                if (removeTimers[i] > 0)
                    removeTimers[i] -= Time.deltaTime;
                else
                {
                    RemoveBullet(bullets[0]);
                    removeTimers.RemoveAt(i);
                }

            IEnumerator AttackCooldown()
            {
                yield return delayBetweenAttacks;
                canShoot = true;
            }

            double CalculateAttackCooldown()
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
                    CreateBullet(ownerSpirit.Targets[i]);

                void CreateBullet(IHealthComponent target)
                {
                    var bulletGO = bulletPool.PopObject();

                    bulletGOs.Add(bulletGO);
                    bullets.Add(new BulletSystem(bulletGO));

                    SetBulletData(bullets[bullets.Count - 1]);

                    Shooting?.Invoke(bullets[bullets.Count - 1]);
                    bulletGOs[bulletGOs.Count - 1].SetActive(true);

                    void SetBulletData(BulletSystem bullet)
                    {
                        bulletGO.transform.position = ownerSpirit.ShootPoint.position;
                        bulletGO.transform.rotation = ownerSpirit.MovingPart.rotation;
                        bullet.Show(true);
                        bullet.IsTargetReached = false;

                        if (target != null)
                            bullet.Target = target;
                        else
                            RemoveBullet(bullet);
                    }
                }
            }

            void RemoveBullet(BulletSystem bullet)
            {
                bullet.Show(false);
                bullet.Prefab.SetActive(false);
                bullets.Remove(bullet);
                bulletGOs.Remove(bullet.Prefab);
            }
        }

        public void SetTargetReached(BulletSystem bullet)
        {
            if (!bullet.IsTargetReached)
            {
                bullet.IsTargetReached = true;
                bullet.Show(false);
                removeTimers.Add(bullets[bulletGOs.Count - 1].Lifetime);
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
                        var offset = new Vector3(0, 40, 0);
                        var distance = bullet.Prefab.transform.position.GetDistanceTo(bullet.Target.Prefab.transform.position + offset);

                        if (distance < 30)
                        {
                            BulletHit?.Invoke(bullet);
                            return;
                        }
                        else
                        {
                            var randVec = new Vector3(
                                UnityEngine.Random.Range(-10, 10),
                                UnityEngine.Random.Range(-10, 10),
                                UnityEngine.Random.Range(-10, 10));

                            var distanceModifier = Mathf.Lerp(1f, distance, Time.deltaTime / 12);

                            bullet.Prefab.transform.LookAt(bullet.Target.Prefab.transform.position + offset);
                            bullet.Prefab.transform.Translate(Vector3.forward * bullet.Speed * distanceModifier + randVec, Space.Self);
                        }
                    }
                }
            }
        }
    }
}
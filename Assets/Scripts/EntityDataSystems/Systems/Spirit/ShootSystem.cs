using System;
using System.Collections;
using System.Collections.Generic;
using Game.Enemy;
using Game.Enums;
using Game.Systems;
using UnityEngine;

namespace Game.Spirit.System
{
    [Serializable]
    public class ShootSystem
    {
        public int ShotCount { get => shotCount; set => shotCount = value > 0 ? value : 0; }
        public bool isHaveChainTargets;
        public event EventHandler<BulletSystem> BulletHit;
        public event EventHandler PrepareToShoot;
        public event EventHandler<BulletSystem> Shooting;

        List<BulletSystem> bullets = new List<BulletSystem>();
        List<GameObject> bulletGOs = new List<GameObject>();
        List<float> removeTimers = new List<float>();
        SpiritSystem ownerSpirit;
        ObjectPool bulletPool;
        double attackDelay;
        int shotCount;

        public ShootSystem(SpiritSystem spirit) => ownerSpirit = spirit;

        void OnDestroy() => bulletPool.DestroyPool();

        public void Set(GameObject bullet)
        {
            attackDelay = ownerSpirit.Data.Get(Enums.Spirit.AttackDelay).Value
                .GetPercent(ownerSpirit.Data.Get(Enums.Spirit.AttackSpeed).Value);

            bulletPool = new ObjectPool(bullet, ownerSpirit.Prefab.transform, 2);
           
        }

        public void UpdateSystem()
        {
            var modifiedAttackSpeed =
                ownerSpirit.Data.Get(Enums.Spirit.AttackDelay).Value.GetPercent(
                    ownerSpirit.Data.Get(Enums.Spirit.AttackSpeed).Value);

            var attackCooldown = ownerSpirit.Data.Get(Enums.Spirit.AttackSpeed).Value < 100 ?
                    ownerSpirit.Data.Get(Enums.Spirit.AttackDelay).Value + (ownerSpirit.Data.Get(Enums.Spirit.AttackDelay).Value - modifiedAttackSpeed) :
                    ownerSpirit.Data.Get(Enums.Spirit.AttackDelay).Value - (modifiedAttackSpeed - ownerSpirit.Data.Get(Enums.Spirit.AttackDelay).Value);

            attackDelay = attackDelay > attackCooldown ? 0 : attackDelay + Time.deltaTime * 0.5f;
            MoveBullet();

            if (attackDelay > attackCooldown)
                ShotBullet();

            for (int i = 0; i < removeTimers.Count; i++)
                if (removeTimers[i] > 0)
                    removeTimers[i] -= Time.deltaTime;
                else
                {
                    RemoveBullet(bullets[0]);
                    removeTimers.RemoveAt(i);
                }

            #region Helper functions

            void ShotBullet()
            {
                PrepareToShoot?.Invoke(null, null);

                for (int i = 0; i < shotCount; i++)
                    CreateBullet(ownerSpirit.Targets[i]);

                void CreateBullet(IHealthComponent target)
                {
                    var bulletGO = bulletPool.PopObject();

                    bulletGOs.Add(bulletGO);
                    bullets.Add(new BulletSystem(bulletGO));

                    SetBulletData(bullets[bullets.Count - 1]);

                    Shooting?.Invoke(null, bullets[bullets.Count - 1]);
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

            #endregion
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

        public void MoveBullet()
        {

            for (int i = 0; i < bullets.Count; i++)
            {
                if (!bullets[i].IsTargetReached)
                {
                    var bullet = bullets[i];
                    if (bullet.Prefab.activeSelf)
                        if (!bullet.IsTargetReached)
                            if (bullet.Target == null || bullet.Target.Prefab == null)
                                SetTargetReached(bullet);
                            else
                            {
                                var bulletGO = bullet.Prefab;
                                var offset = new Vector3(0, 40, 0);
                                var distance = bulletGO.transform.position.GetDistanceTo(bullet.Target.Prefab.transform.position + offset);

                                if (distance < 30)
                                {
                                    HitTarget(bullet);
                                    return;
                                }
                                else
                                {
                                    var randVec = new Vector3(
                                        UnityEngine.Random.Range(-10, 10),
                                        UnityEngine.Random.Range(-10, 10),
                                        UnityEngine.Random.Range(-10, 10));

                                    bulletGO.transform.LookAt(bullet.Target.Prefab.transform.position + offset);
                                    bulletGO.transform.Translate(Vector3.forward * bullet.Speed + randVec, Space.Self);
                                }
                            }
                }
            }
        }

        void HitTarget(BulletSystem bullet) => BulletHit?.Invoke(null, bullet);
    }
}
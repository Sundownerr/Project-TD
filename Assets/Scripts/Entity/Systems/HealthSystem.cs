using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Game.Enemy;
using Game.Data;
using Game.Systems;
using Game.Spirit;
using UnityEngine;

public class HealthSystem 
{
    public List<Effect> AppliedEffects { get; private set; }
    public event EventHandler<IHealthComponent> Died = delegate { };
    public bool IsVulnerable { get; set; }

    private IHealthComponent owner;
	private double maxHealth, healthRegen, regenTimer;

	public HealthSystem(IHealthComponent owner)
	{
		this.owner = owner;

        AppliedEffects = new List<Effect>();

        if (owner is EnemySystem enemy)
			maxHealth = enemy.Data.GetValue(Numeral.MaxHealth);
	}

	public void UpdateSystem()
	{
		if (owner is EnemySystem enemy)
		{
			var health = enemy.Data.GetValue(Numeral.Health);
			healthRegen = enemy.Data.GetValue(Numeral.HealthRegen);				
				
			if (health < maxHealth)
			{
				regenTimer = regenTimer > 1 ? 0 : regenTimer += Time.deltaTime;

				if (regenTimer == 1)
					health += healthRegen;
			}				
			else
				if (health > maxHealth)
					health = maxHealth;			
		}		
	}

    public void ChangeHealth(IDamageDealer changer, double damage)
	{
        if (!IsVulnerable)
            return;

        if (owner is EnemySystem enemy)
        {
            var remainingHealth =
                enemy.Data.Get(Numeral.Health, From.Applied).Value > 0 ?
                enemy.Data.Get(Numeral.Health, From.Applied) :
                enemy.Data.Get(Numeral.Health, From.Base);

            enemy.LastDamageDealer = changer;
            remainingHealth.Value -= damage;

            if (remainingHealth.Value <= 0)
            {
                GiveResources();
                IsVulnerable = false;

                Died?.Invoke(null, owner);
            }
        }

		#region  Helper functions
		
		void GiveResources()
        {
            if (enemy.LastDamageDealer is SpiritSystem spirit)           
                spirit.AddExp((int)enemy.Data.GetValue(Numeral.Exp));
        }
		
		#endregion
	}
}

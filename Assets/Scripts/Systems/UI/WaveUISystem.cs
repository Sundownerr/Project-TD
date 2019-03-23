﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Game.Enemy.Data;
using System.Text;
using UnityEngine.UI;

namespace Game.Systems
{
    public class WaveUISystem : ExtendedMonoBehaviour
    {
        public PlayerSystem Owner { get; set; }
        public Button StartWaveButton;
        public TextMeshProUGUI EnemyTypes, Race, Armor, Traits, WaveNumber;
        public event EventHandler WaveStarted = delegate { };

        private Animator animator;

        protected override void Awake()
        {
            base.Awake();

            StartWaveButton.onClick.AddListener(StartWave);
            animator = GetComponent<Animator>();
        }

        public void SetSystem(PlayerSystem player)
        {
            Owner = player;
            Owner.WaveSystem.WaveEnded += OnWaveEnded;
            Owner.WaveSystem.WaveStarted += OnWaveStarted;
            Owner.WaveSystem.WavesGenerated += OnWavesGenerated;
            if (Owner.WaveSystem.WaveNumber == 1)
                ActivateUI(true);
        }

        private void OnWavesGenerated(object _, EventArgs e) { ActivateUI(true); Debug.Log("generated"); }
        private void OnWaveStarted(object _, EventArgs e) => ActivateUI(false);
        private void OnWaveEnded(object _, EventArgs e) => ActivateUI(true);

        private void StartWave()
        {
            if (Owner.WaveSystem.WaveNumber <= Owner.WaveAmount)
            {
                StartWaveButton.gameObject.SetActive(false);
                WaveStarted?.Invoke(null, null);
                GC.Collect();
            }
        }

        private void ActivateUI(bool activate)
        {
            if (activate)
            {
                UpdateUI();
                animator.SetBool("isOpen", true);
                StartWaveButton.gameObject.SetActive(true);
            }
            else
            {
                animator.SetBool("isOpen", false);
                StartWaveButton.gameObject.SetActive(false);
            }
        }

        private void UpdateUI()
        {
            var wave = Owner.WaveSystem.CurrentWave;
       

            Race.text = wave.EnemyTypes[0].Race.ToString();
            Armor.text = wave.EnemyTypes[0].ArmorType.ToString();
            EnemyTypes.text = CalculateTypes();
            Traits.text = GetTraitsAndAbilities();
            WaveNumber.text = $"wave {Owner.WaveSystem.WaveNumber}";

            #region  Helper functions

            string GetTraitsAndAbilities()
            {
                var traitsAndAbilities = new StringBuilder();

                for (int i = 0; i < wave.EnemyTypes[0].Traits.Count; i++)
                    traitsAndAbilities.Append($"{wave.EnemyTypes[0].Traits[i].Name}     ");

                for (int i = 0; i < wave.EnemyTypes.Count; i++)
                    if (wave.EnemyTypes[i].Type == EnemyType.Commander || wave.EnemyTypes[i].Type == EnemyType.Boss)
                    {
                        for (int j = 0; j < wave.EnemyTypes[i].Abilities.Count; j++)
                            traitsAndAbilities.Append($"{wave.EnemyTypes[i].Abilities[j].Name} ");
                        break;
                    }
                return traitsAndAbilities.ToString();
            }

            string CalculateTypes()
            {
                var enemyTypes = new StringBuilder();
                var smallCount = 0;
                var normalCount = 0;
                var commanterCount = 0;
                var flyingCount = 0;
                var bossCount = 0;

                for (int i = 0; i < wave.EnemyTypes.Count; i++)
                {
                    if (wave.EnemyTypes[i].Type == EnemyType.Small) { smallCount++; continue; }
                    if (wave.EnemyTypes[i].Type == EnemyType.Normal) { normalCount++; continue; }
                    if (wave.EnemyTypes[i].Type == EnemyType.Commander) { commanterCount++; continue; }
                    if (wave.EnemyTypes[i].Type == EnemyType.Flying) { flyingCount++; continue; }
                    if (wave.EnemyTypes[i].Type == EnemyType.Boss) { bossCount++; continue; }
                }

                enemyTypes
                    .Append(smallCount > 0 ? $"{smallCount} small " : string.Empty)
                    .Append(normalCount > 0 ? $"{normalCount} normal " : string.Empty)
                    .Append(commanterCount > 0 ? $"{commanterCount} commander " : string.Empty)
                    .Append(flyingCount > 0 ? $"{flyingCount} flying " : string.Empty)
                    .Append(bossCount > 0 ? $"{bossCount} boss " : string.Empty);
                return enemyTypes.ToString();
            }

            #endregion
        }
    }
}

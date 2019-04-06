﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Game.Enemy.Data;
using System.Text;
using UnityEngine.UI;
using Game.Enums;
using Lean.Localization;

namespace Game.Systems
{
    public class WaveUISystem : ExtendedMonoBehaviour
    {
        public PlayerSystem Owner { get; set; }
        public Button StartWaveButton;
        public TextMeshProUGUI EnemyTypes, Race, Armor, Traits, WaveNumber;
        public event EventHandler WaveStarted = delegate { };

        private Animator animator;
        private StringBuilder enemyTypes = new StringBuilder();
        private StringBuilder traitsAndAbilities = new StringBuilder();

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
            var wave = Owner.WaveSystem.Waves.Peek();

            Race.text = wave.EnemyTypes[0].Race.GetLocalized();
            Armor.text = wave.EnemyTypes[0].ArmorType.GetLocalized();
            EnemyTypes.text = CalculateTypes();
            Traits.text = GetTraitsAndAbilities();
            WaveNumber.text = $"{LeanLocalization.GetTranslationText(LocaleKeys.UIWave)} {Owner.WaveSystem.WaveNumber + 1}";

            #region  Helper functions

            string GetTraitsAndAbilities()
            {
                wave.EnemyTypes[0].Traits.ForEach(trait => traitsAndAbilities.Append($"{trait.Name}     "));
                wave.EnemyTypes.Find(enemy => enemy.IsBossOrCommander())?.Abilities.ForEach(ability => traitsAndAbilities.Append($"{ability.Name} "));

                var result = traitsAndAbilities.ToString();
                traitsAndAbilities.Clear();
                return result;
            }

            string CalculateTypes()
            {
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
                    .Append(smallCount > 0 ? $"{smallCount} {EnemyType.Small.GetLocalized()} " : string.Empty)
                    .Append(normalCount > 0 ? $"{normalCount} {EnemyType.Normal.GetLocalized()} " : string.Empty)
                    .Append(commanterCount > 0 ? $"{commanterCount} {EnemyType.Commander.GetLocalized()} " : string.Empty)
                    .Append(flyingCount > 0 ? $"{flyingCount} {EnemyType.Flying.GetLocalized()} " : string.Empty)
                    .Append(bossCount > 0 ? $"{bossCount} {EnemyType.Boss.GetLocalized()} " : string.Empty);

                var result = enemyTypes.ToString();
                enemyTypes.Clear();
                return result;
            }

            #endregion
        }
    }
}

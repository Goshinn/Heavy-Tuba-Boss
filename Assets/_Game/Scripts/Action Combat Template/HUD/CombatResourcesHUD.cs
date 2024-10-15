using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatResourcesHUD : MonoBehaviour
{
    [Header("References")]
    private PlayerCombatResources combatResources;

    [Header("Setup - Resources")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image blockResourceBar;
    [SerializeField] private Animator blockResourceDepletedIndicator;
    [SerializeField] private Image staminaBar;

    [Header("Settings")]
    [SerializeField] private float barLerpSpeed = 30f;

    [Header("Members")]
    private Coroutine updateReiatsuBarCorout;
    private Coroutine updateStaminaBarCorout;

    private void Awake()
    {
        // Hook up references
        combatResources = GetComponentInParent<CanvasManager>().CombatResources;
    }

    private void OnEnable()
    {
        // Subscribe to events
        Application.quitting += UnsubscribeFromEvents;
        combatResources.HealthChanged += UpdateHealthBar;
        combatResources.BlockResourceChanged += OnBlockResourceChanged;
        combatResources.BlockResourceDepleted += OnBlockResourceDepleted;
        combatResources.StaminaChanged += OnStaminaChanged;
    }

    private void UpdateHealthBar()
    {
        healthBar.fillAmount = combatResources.CurrentHealth / combatResources.MaxHealth;
    }

    private void OnStaminaChanged()
    {
        if (updateStaminaBarCorout != null)
        {
            StopCoroutine(updateStaminaBarCorout);
        }
        float targetValue = combatResources.Stamina / combatResources.MaxStamina;
        updateStaminaBarCorout = StartCoroutine(LerpBarToTargetValue(staminaBar, targetValue));
    }

    private void OnBlockResourceChanged()
    {
        if (updateReiatsuBarCorout != null)
        {
            StopCoroutine(updateReiatsuBarCorout);
        }
        float targetValue = combatResources.BlockResource / combatResources.MaxBlockResource;
        updateReiatsuBarCorout = StartCoroutine(LerpBarToTargetValue(blockResourceBar, targetValue));
    }

    private void OnBlockResourceDepleted()
    {
        blockResourceDepletedIndicator.SetTrigger("BlockResourceDepleted");
    }

    private IEnumerator LerpBarToTargetValue(Image bar, float targetValue)
    {
        while (Mathf.Abs(bar.fillAmount - targetValue) > 0.01f)
        {
            bar.fillAmount = Mathf.Lerp(bar.fillAmount, targetValue, Time.deltaTime * barLerpSpeed);
            yield return null;
        }
        bar.fillAmount = targetValue;

        // Set the appropriate corout var to null
        if (bar == blockResourceBar)
        {
            updateReiatsuBarCorout = null;
        }
        else if (bar == staminaBar)
        {
            updateStaminaBarCorout = null;
        }
        else
        {
            Debug.LogError("Lerped value of unidentified bar");
        }
    }

    private void UnsubscribeFromEvents()
    {
        Application.quitting -= UnsubscribeFromEvents;
        combatResources.HealthChanged -= UpdateHealthBar;
        combatResources.StaminaChanged -= OnStaminaChanged;
        combatResources.BlockResourceChanged -= OnBlockResourceChanged;
    }
}

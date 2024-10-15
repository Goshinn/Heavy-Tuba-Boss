using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public abstract class EnemyHUD : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] protected Text entityNameTextField;
    [SerializeField] protected Image healthBar;
    [SerializeField] protected CanvasGroup canvasGroup;   // Alpha set to 0 when offscreen

    [Header("References")]
    protected IKillable assignedKillable;
    protected Camera cam;
    protected Animator anim;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public virtual void Initialize(IKillable killable, Camera camera)
    {
        // Hook up references
        cam = camera;

        // Initialize & subscribe to evnts
        assignedKillable = killable;

        // Initialize this healthbar
        entityNameTextField.text = assignedKillable.KillableName;
        healthBar.fillAmount = assignedKillable.CurrentHealth / assignedKillable.MaxHealth;

        // Setup evnt listeners
        assignedKillable.HealthChanged += OnHealthModified;
    }

    protected virtual void OnHealthModified()
    {
        healthBar.fillAmount = assignedKillable.CurrentHealth / assignedKillable.MaxHealth;
    }

    // Called before this.gameObject is destroyed by EnemyHealthBarController OnDespawnHealthBar
    public virtual void UnregisterFromEvents()
    {
        if (assignedKillable != null)
        {
            assignedKillable.HealthChanged -= OnHealthModified;
        }
        else
        {
            Debug.LogError("assignedKillable is null");
        }
    }
}

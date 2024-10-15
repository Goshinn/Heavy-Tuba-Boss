using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class MobEnemyHUD : EnemyHUD
{
    [Header("References")]
    private Transform healthbarTrackerObj;

    [Header("Settings")]
    [SerializeField] private Vector3 healthBarOffset;
    [SerializeField] private float showForSecondsOnTakeDamage;

    [Header("Members")]
    private Coroutine hideHealthBarCorout;

    private void Awake()
    {
        canvasGroup.alpha = 0;
    }

    private void Update()
    {
        UpdateHealthBarPosition();
    }

    public override void Initialize(IKillable killable, Camera camera)
    {
        base.Initialize(killable, camera);

        // Additional setup of this healthbar
        assignedKillable.HealthLost += OnHealthLost;
        healthbarTrackerObj = assignedKillable.HealthbarPositionTrackerObject;
    }

    // Update healthbar position - if behind camera, hide, else position healthbar to baseAI.healthBarPosition
    private void UpdateHealthBarPosition()
    {
        if (assignedKillable == null || healthbarTrackerObj == null)
        {
            Debug.Log($"who dat boi, {assignedKillable == null} || {healthbarTrackerObj == null}");
            return;
        }

        Vector3 healthbarPos = cam.WorldToScreenPoint(healthbarTrackerObj.position) + healthBarOffset;
        if (healthbarPos.z > 0)
        {
            canvasGroup.alpha = 1;
            transform.position = healthbarPos;
        }
        else
        {
            // Hide this healthbar when its behind the screen
            canvasGroup.alpha = 0;
        }
    }

    private void OnHealthLost()
    {
        // Show this enemy's healthbar if it was hidden for x seconds upon taking dmg
        anim.SetBool("ShowHealthBar", true);
        if (hideHealthBarCorout != null)
        {
            StopCoroutine(hideHealthBarCorout);
        }
        hideHealthBarCorout = StartCoroutine(HideHealthBarAfterSeconds(showForSecondsOnTakeDamage));
    }

    private IEnumerator HideHealthBarAfterSeconds(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        anim.SetBool("ShowHealthBar", false);
        hideHealthBarCorout = null;
    }

    public override void UnregisterFromEvents()
    {
        base.UnregisterFromEvents();
        assignedKillable.HealthLost -= OnHealthLost;
    }
}

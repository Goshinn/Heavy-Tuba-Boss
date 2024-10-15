using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossEnemyHUD : EnemyHUD
{
    [Header("Additional References")]
    [SerializeField] private Image healthLostBar;

    [Header("Members")]
    [SerializeField] private float healthLostLerpDelay = 1f;
    [SerializeField] private float healthLostBarMoveRate = 1f;
    private Coroutine healthLostCorout;

    protected override void Awake()
    {
        base.Awake();
        anim.SetBool("ShowHealthBar", true);
    }

    protected override void OnHealthModified()
    {
        base.OnHealthModified();

        if (healthLostCorout == null)
        {
            healthLostCorout = StartCoroutine(IndicateHealthLost());
        }
    }

    private IEnumerator IndicateHealthLost()
    {
        yield return new WaitForSeconds(healthLostLerpDelay);

        while (healthLostBar.fillAmount > healthBar.fillAmount)
        {
            healthLostBar.fillAmount = Mathf.MoveTowards(healthLostBar.fillAmount, healthBar.fillAmount, healthLostBarMoveRate * Time.deltaTime);
            yield return null;
        }

        healthLostCorout = null;
    }
}

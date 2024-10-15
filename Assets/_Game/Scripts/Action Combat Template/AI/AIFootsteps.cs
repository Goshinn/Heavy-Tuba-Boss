using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFootsteps : MonoBehaviour
{
    [Header("References")]
    private Animator anim;

    [Header("Setup")]
    [SerializeField, FMODUnity.EventRef] private string footstepEventRef;

    [Header("Members")]
    private float previousFrameFootstepLeftValue = 0;
    private float previousFrameFootstepRightValue = 0;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        float currentFrameFootstepLeftValue = anim.GetFloat("FootstepLeft");
        float currentFrameFootstepRightValue = anim.GetFloat("FootstepRight");

        // Check if should play footstep sound
        if (previousFrameFootstepLeftValue > 0 && currentFrameFootstepLeftValue <= 0)
        {
            FMODUnity.RuntimeManager.PlayOneShot(footstepEventRef, transform.position);
        }

        if (previousFrameFootstepRightValue > 0 && currentFrameFootstepRightValue <= 0)
        {
            FMODUnity.RuntimeManager.PlayOneShot(footstepEventRef, transform.position);
        }

        previousFrameFootstepLeftValue = currentFrameFootstepLeftValue;
        previousFrameFootstepRightValue = currentFrameFootstepRightValue;
    }
}

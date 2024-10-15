using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use tags to check for surface type to determine which footstep sound to play?
/// For terrains, perhaps we can shoot a raycast down, get the splatmap and check which texture is currently active beneath our foot to determine footstep sound?
/// </summary>
public class CharacterFootsteps : MonoBehaviour
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
            FMODUnity.RuntimeManager.PlayOneShot(footstepEventRef);
        }

        if (previousFrameFootstepRightValue > 0 && currentFrameFootstepRightValue <= 0)
        {
            FMODUnity.RuntimeManager.PlayOneShot(footstepEventRef);
        }

        previousFrameFootstepLeftValue = currentFrameFootstepLeftValue;
        previousFrameFootstepRightValue = currentFrameFootstepRightValue;
    }
}

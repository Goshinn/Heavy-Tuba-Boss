using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class AdvanceTimeline : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private PlayableDirector director;
    [SerializeField] private float scrubToTime;

    private void Start()
    {
        // Create action & setup callback
        var skipCutsceneAction = new InputAction("SkipCutscene", binding: "<Keyboard>/enter");
        skipCutsceneAction.performed += context => SkipCutscene();
        skipCutsceneAction.Enable();
    }

    public void SkipCutscene()
    {
        Debug.Log("Skipping");
        director.time = scrubToTime;
    }
}

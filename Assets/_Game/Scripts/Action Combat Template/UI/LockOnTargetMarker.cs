using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockOnTargetMarker : MonoBehaviour
{
    [Header("References")]
    private Camera cam;
    private RectTransform rectTransform;
    private Image lockOnMarker; 

    [Header("Members")]
    private Transform lockOnTarget;

    private void Awake()
    {
        // Hook up references
        cam = GetComponentInParent<CanvasManager>().Cam;
        rectTransform = GetComponent<RectTransform>();
        lockOnMarker = GetComponent<Image>();

        // Setup callbacks
        CameraBase.TargetLockedOn += OnTargetLockedOn;
        CameraBase.TargetLockOnEnded += OnTargetLockOnEnded;
    }

    private void Update()
    {
        if (lockOnTarget != null)
        {
            Vector3 targetScreenPosition = cam.WorldToScreenPoint(lockOnTarget.position);
            rectTransform.position = targetScreenPosition;
        }
    }

    private void OnTargetLockedOn(Transform lockOnObj)
    {
        lockOnTarget = lockOnObj;
        lockOnMarker.enabled = true;
    }

    private void OnTargetLockOnEnded()
    {
        lockOnTarget = null;
        lockOnMarker.enabled = false;
    }
}

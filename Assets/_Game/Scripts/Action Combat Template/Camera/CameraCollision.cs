using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraCollision : MonoBehaviour
{
    [Header("References")]
    private CameraBase cameraBase;

    [Header("Settings")]
    [SerializeField] private LayerMask environmentCollisionLayer;   // Layers in which camera collides with; exclude players
    [SerializeField] private float minDistance = 1f;                // Min distance camera is from player
    [SerializeField] private float maxDistance = 4f;                // Max distance camera is from cameraBase
    [SerializeField] private float smooth = 30f;                    // Smooth value applied when lerping camera due to collisions
    [SerializeField] private float spherecastRadius = 0.1f;         // The bigger, the more easily the camera will push into player upon cam collision

    [Header("Player Collision")]
    [SerializeField] private LayerMask playerLayer;             // When overlapSphere collides with an object from this layer, collidingWithPlayer is set to true which fades the player character out
    [SerializeField] private float playerCollisionRadius = 0.1f;

    [Header("Members")]
    private Vector3 dollyDir;                                   // Global direction to camera from player - used for spherecast
    private Vector3 localDollyDir;                              // Local direction to player - used to set distance
    private float desiredDistance;
    private float distance;                                     // Current dist from player
    private float desiredMaxDistance;

    private void Awake()
    {
        cameraBase = GetComponentInParent<CameraBase>();

        // Init vars
        localDollyDir = transform.localPosition.normalized;
        desiredMaxDistance = maxDistance;
        desiredDistance = desiredMaxDistance;
    }

    private void Update()
    {
        SetDesiredDistance();
        CheckForCollision();
    }

    private void SetDesiredDistance()
    {
        desiredDistance = desiredMaxDistance;
    }

    public void SetDesiredMaxDistance(InputAction.CallbackContext context)
    {
        desiredMaxDistance = Mathf.Clamp(desiredMaxDistance + context.ReadValue<float>() * Time.deltaTime, 0, maxDistance);
    }

    // Pulls the camera closer to player if collision is detected
    private void CheckForCollision()
    {
        dollyDir = (transform.position - cameraBase.transform.position).normalized;

        // If spherecast hits something that is not the player, move closer to player, else stay at desired distance
        RaycastHit hit;
        if (Physics.SphereCast(cameraBase.transform.position, spherecastRadius, dollyDir, out hit, desiredDistance, environmentCollisionLayer, QueryTriggerInteraction.Ignore))
        {
            if (cameraBase.AssignedPlayer != null && hit.transform.root.gameObject != cameraBase.AssignedPlayer)
            {
                distance = Mathf.Clamp(hit.distance * 0.9f, minDistance, desiredDistance);
                Debug.DrawLine(cameraBase.transform.position, hit.point, Color.red);
            }
            else
            {
                Debug.DrawRay(cameraBase.transform.position, dollyDir * desiredDistance, Color.green);
                distance = desiredDistance;
            }
        }
        else
        {
            Debug.DrawRay(cameraBase.transform.position, dollyDir * desiredDistance, Color.green);
            distance = desiredDistance;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, localDollyDir * distance, Time.deltaTime * smooth);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, spherecastRadius);
    }
}

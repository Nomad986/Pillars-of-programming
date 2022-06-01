using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;

    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float lookDistance;
    [SerializeField] private float angle;
    private bool canSeePlayer;
    private Vector3 basePosition;

    private void Awake()
    {
        canSeePlayer = false;
        basePosition = transform.position;
        StartCoroutine(FOVRoutine());
    }

    private IEnumerator FOVRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.2f);
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        Collider[] proximityCheck = Physics.OverlapSphere(transform.position, lookDistance, playerMask);

        if(proximityCheck.Length > 0)
        {
            Transform target = proximityCheck[0].transform;
            Vector3 directionToTargetBody = (target.position - transform.position).normalized;

            Vector3 targetHeadPosition = target.position;
            targetHeadPosition.y += 1f;
            Vector3 directionToTargetHead = (targetHeadPosition - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTargetBody) < angle / 2 ||
                Vector3.Angle(transform.forward, directionToTargetHead) < angle / 2)
            {
                float distanceToTargetBody = Vector3.Distance(transform.position, target.position);
                float distanceToTargetHead = Vector3.Distance(transform.position, targetHeadPosition);

                if (!Physics.Raycast(transform.position, directionToTargetBody, distanceToTargetBody, groundMask) ||
                    !Physics.Raycast(transform.position, directionToTargetHead, distanceToTargetHead, groundMask))
                {
                    canSeePlayer = true;
                    agent.SetDestination(target.position);
                }
                else
                {
                    canSeePlayer = false;
                    agent.SetDestination(basePosition);
                }

            }
            else
            {
                canSeePlayer = false;
                agent.SetDestination(basePosition);
            }
        }
        else 
        {
            canSeePlayer = false;
            agent.SetDestination(basePosition);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private NavMeshAgent agent;

    [SerializeField] private float waitTime = 0.2f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float lookDistance;
    [SerializeField] private float angle;
    [SerializeField] private GameObject eyes;
    private bool canSeePlayer;
    private Vector3 basePosition;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        canSeePlayer = false;
        basePosition = transform.position;
    }

    private void Update()
    {
        FieldOfViewCheck();
    }

    private IEnumerator FOVRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(waitTime);
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        Collider[] proximityCheck = Physics.OverlapSphere(eyes.transform.position, lookDistance, playerMask);

        if(proximityCheck.Length > 0)
        {
            Transform target = proximityCheck[0].transform;
            Vector3 directionToTargetBody = (target.position - eyes.transform.position).normalized;

            Vector3 targetHeadPosition = target.position;
            targetHeadPosition.y += 1f;
            Vector3 directionToTargetHead = (targetHeadPosition - eyes.transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTargetBody) < angle / 2 ||
                Vector3.Angle(transform.forward, directionToTargetHead) < angle / 2)
            {
                float distanceToTargetBody = Vector3.Distance(eyes.transform.position, target.position);
                float distanceToTargetHead = Vector3.Distance(eyes.transform.position, targetHeadPosition);

                if (!Physics.Raycast(eyes.transform.position, directionToTargetBody, distanceToTargetBody, groundMask) ||
                    !Physics.Raycast(eyes.transform.position, directionToTargetHead, distanceToTargetHead, groundMask))
                {
                    canSeePlayer = true;
                    agent.SetDestination(target.position);
                    Debug.DrawRay(eyes.transform.position, directionToTargetHead * 10f, Color.yellow);
                    Debug.DrawRay(eyes.transform.position, directionToTargetBody * 10f, Color.yellow);
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

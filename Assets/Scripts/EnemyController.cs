using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float waitTime = 0.2f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float lookDistance;
    [SerializeField] private float angle;
    [SerializeField] private GameObject eyes;
    private Vector3 basePosition;

    private NavMeshAgent agent;
    private Animator animator;

    private bool awareOfPlayer;
    private bool canSeePlayer;
    private Vector3 destination;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        basePosition = transform.position;
        awareOfPlayer = false;
    }

    private void Update()
    {
        canSeePlayer = FieldOfViewCheck();

        if (!awareOfPlayer && !canSeePlayer)
        {
            destination = basePosition;
        }
        else if (awareOfPlayer && canSeePlayer)
        {
            destination = PlayerManager.instance.player.transform.position;
        }
        else if (!awareOfPlayer && canSeePlayer)
        {
            awareOfPlayer = true;
            destination = PlayerManager.instance.player.transform.position;
        }
        else if (awareOfPlayer && !canSeePlayer)
        {
            //chase for some time, then stop being aware and return to base position
        }

        animator.SetBool("awareOfPlayer", awareOfPlayer);
        animator.SetBool("canSeePlayer", canSeePlayer);
        agent.SetDestination(destination);
    }

    private bool FieldOfViewCheck()
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
                    //can see player
                    Debug.DrawRay(eyes.transform.position, directionToTargetHead * 10f, Color.yellow);
                    Debug.DrawRay(eyes.transform.position, directionToTargetBody * 10f, Color.yellow);
                    return true;
                }
                else
                {
                    //cannot see player
                    return false;
                }

            }
            else
            {
                //can see player
                return false;
            }
        }
        else 
        {
            //cannot see player
            return false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float runningSpeed;
    [SerializeField] private float walkingSpeed;

    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float lookDistance;
    [SerializeField] private float angle;
    [SerializeField] private GameObject eyes;
    private Vector3 basePosition;

    private NavMeshAgent agent;
    private Animator animator;
    private string currentState;

    private bool awareOfPlayer;
    private bool canSeePlayer;
    private bool inPlace;
    private bool animationLocked;
    private IEnumerator lockingFunction;
    private IEnumerator awareFunction;
    private IEnumerator attackFunction;
    private bool awareFunctionRunning;
    [SerializeField] private float searchTime;
    [SerializeField] private float attackDistance;
    [SerializeField] private float basePositionOffset;

    const string ZOMBIE_IDLE = "Zombie Idle";
    const string ZOMBIE_PUNCHING = "Zombie Punching";
    const string ZOMBIE_KICKING = "Zombie Kicking";
    const string ZOMBIE_RUNNING = "Zombie Running";
    const string ZOMBIE_WALKING = "Walking";
    const string ZOMBIE_SCREAM = "Zombie Scream";
    const string ZOMBIE_AGONIZING = "Zombie Agonizing";
    [SerializeField] private float shortenAgonizing;
    [SerializeField] private float shortenScream;
    [SerializeField] private float waitForPunchCast;
    [SerializeField] private float waitForKickCast;
    private int losingSightPhase;

    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        basePosition = transform.position;
        awareOfPlayer = false;
        inPlace = true;
        animationLocked = false;
        losingSightPhase = 0;
        awareFunctionRunning = false;
    }

    private void Update()
    {
        canSeePlayer = FieldOfViewCheck();
        float distance = Vector3.Distance(PlayerManager.instance.player.transform.position,
            transform.position);
        inPlace = Vector3.Distance(basePosition, transform.position) < basePositionOffset;

        if (!animationLocked)
        {
            if (distance > attackDistance)
            {
                if (!awareOfPlayer && !canSeePlayer)
                {
                    //Debug.Log("State 1");
                    if (inPlace)
                    {
                        agent.SetDestination(transform.position);
                        ChangeAnimationState(ZOMBIE_IDLE);
                    }
                    else
                    {
                        agent.speed = walkingSpeed;
                        agent.SetDestination(basePosition);
                        ChangeAnimationState(ZOMBIE_WALKING);
                    }
                }
                else if (awareOfPlayer && canSeePlayer)
                {
                    if (losingSightPhase == 1)
                    {
                        losingSightPhase = 0;
                    }
                    if (awareFunctionRunning)
                    {
                        StopCoroutine(awareFunction);
                        awareFunctionRunning = false;
                    }

                    //Debug.Log("State 2");
                    agent.speed = runningSpeed;
                    agent.SetDestination(PlayerManager.instance.player.transform.position);
                    ChangeAnimationState(ZOMBIE_RUNNING);
                }
                else if (!awareOfPlayer && canSeePlayer)
                {
                    //Debug.Log("State 3");
                    awareOfPlayer = true;
                    ChangeAnimationState(ZOMBIE_SCREAM);
                    lockingFunction = LockAnimation(false);
                    StartCoroutine(lockingFunction);
                }
                else if (awareOfPlayer && !canSeePlayer && losingSightPhase == 0)
                {
                    //Debug.Log("State 4");
                    agent.SetDestination(transform.position);
                    ChangeAnimationState(ZOMBIE_AGONIZING);
                    lockingFunction = LockAnimation(false);
                    StartCoroutine(lockingFunction);
                    losingSightPhase = 1;
                }
                else if (awareOfPlayer && !canSeePlayer && losingSightPhase == 1)
                {
                    awareFunction = SearchCountdown();
                    StartCoroutine(awareFunction);
                    agent.speed = walkingSpeed;
                    agent.SetDestination(PlayerManager.instance.player.transform.position);
                    ChangeAnimationState(ZOMBIE_WALKING);
                }
            }
            else
            {
                agent.SetDestination(transform.position);
                int randomAttack = Random.Range(0, 2);
                if (randomAttack == 0)
                {
                    ChangeAnimationState(ZOMBIE_PUNCHING);
                    lockingFunction = LockAnimation(true);
                    attackFunction = CastAttack(0);
                }
                else if (randomAttack == 1)
                {
                    ChangeAnimationState(ZOMBIE_KICKING);
                    lockingFunction = LockAnimation(true);
                    attackFunction = CastAttack(1);
                }
                StartCoroutine(lockingFunction);
                StartCoroutine(attackFunction);
            }
        }

        bool playerWithinDistance =
                Vector3.Distance(transform.position, PlayerManager.instance.player.transform.position)
                < lookDistance;

        if (awareOfPlayer && playerWithinDistance)
        {
            FacePlayer();
        }

        //Debug.Log(inPlace);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    private void FacePlayer()
    {
        Vector3 direction = 
            (PlayerManager.instance.player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = lookRotation;
    }

    private void CheckAttack(int randomAttack)
    {
        Collider[] hit = Physics.OverlapSphere(attackPoint.position, attackRange, playerMask);

        if (hit.Length > 0)
        {
            if (randomAttack == 0)
            {
                Debug.Log("HIT");
                hit[0].GetComponentInParent<PlayerController>().ReceiveDamage(10);
            }
            else if (randomAttack == 1)
            {
                hit[0].GetComponentInParent<PlayerController>().ReceiveDamage(25);
                Debug.Log("HIT");
            }
        }
    }

    private IEnumerator CastAttack(int randomAttack)
    {
        if (randomAttack == 0)
        {
            yield return new WaitForSeconds(waitForPunchCast);
            CheckAttack(0);
        }
        else if (randomAttack == 1)
        {
            yield return new WaitForSeconds(waitForKickCast);
            CheckAttack(1);
        }
    }

    private IEnumerator SearchCountdown()
    {
        awareFunctionRunning = true;
        yield return new WaitForSeconds(searchTime);
        awareOfPlayer = false;
        losingSightPhase = 0;
        awareFunctionRunning = false;
    }

    private IEnumerator LockAnimation(bool returnToIdle)
    {
        animationLocked = true;
        if(currentState == ZOMBIE_AGONIZING)
        {
            yield return new WaitForSeconds(11.633f - shortenAgonizing);
        }
        else if (currentState == ZOMBIE_SCREAM)
        {
            yield return new WaitForSeconds(2.8f - shortenScream);
        }
        else if (currentState == ZOMBIE_PUNCHING)
        {
            yield return new WaitForSeconds(3.833f);
        }
        else if (currentState == ZOMBIE_KICKING)
        {
            yield return new WaitForSeconds(3.367f);
        }
        animationLocked = false;

        if (returnToIdle)
        {
            ChangeAnimationState(ZOMBIE_IDLE);
        }
    }

    private void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        animator.Play(newState);

        currentState = newState;
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

using UnityEngine;
using UnityEngine.AI;

public class SimpleCharacterMotor : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Animator Params")]
    public string walkBool = "Walk";
    public string sitTrigger = "Sit";
    public string bigJumpTrigger = "BigJump";
    public string smallJumpTrigger = "SmallJump";

    [Header("Arrival")]
    public float arrivalThreshold = 0.03f;

    private Transform currentTarget;
    private bool destinationSet = false;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void WarpTo(Transform point)
    {
        if (agent == null || point == null) return;

        agent.Warp(point.position);
        transform.rotation = point.rotation;

        currentTarget = null;
        destinationSet = false;

        SetWalk(false);
    }

    public void MoveTo(Transform point)
    {
        MoveToInternal(point, true);
    }

    public void MoveToNoWalk(Transform point)
    {
        MoveToInternal(point, false);
    }

    private void MoveToInternal(Transform point, bool useWalkAnim)
    {
        if (agent == null || point == null) return;

        currentTarget = point;
        agent.isStopped = false;
        destinationSet = agent.SetDestination(point.position);

        SetWalk(useWalkAnim && destinationSet);
    }

    public void StopMoving()
    {
        if (agent == null) return;

        agent.isStopped = true;
        agent.ResetPath();

        currentTarget = null;
        destinationSet = false;

        SetWalk(false);
    }

    public bool HasReachedDestination()
    {
        if (agent == null || currentTarget == null || !destinationSet)
            return false;

        if (agent.pathPending)
            return false;

        float navDist = agent.remainingDistance;
        float directDist = Vector3.Distance(transform.position, currentTarget.position);

        bool reachedByNav = navDist != Mathf.Infinity && navDist <= Mathf.Max(agent.stoppingDistance, arrivalThreshold);
        bool reachedByDirect = directDist <= arrivalThreshold;

        if (reachedByNav || reachedByDirect)
        {
            StopMoving();
            return true;
        }

        return false;
    }

    public void FaceTarget(Transform point)
    {
        if (point == null) return;

        Vector3 dir = point.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(dir);
    }

    public void PlayBigJump()
    {
        SetWalk(false);

        if (animator != null && !string.IsNullOrEmpty(bigJumpTrigger))
            animator.SetTrigger(bigJumpTrigger);
    }

    public void PlaySmallJump()
    {
        SetWalk(false);

        if (animator != null && !string.IsNullOrEmpty(smallJumpTrigger))
            animator.SetTrigger(smallJumpTrigger);
    }

    public void PlaySit()
    {
        SetWalk(false);

        if (animator != null && !string.IsNullOrEmpty(sitTrigger))
            animator.SetTrigger(sitTrigger);
    }

    private void SetWalk(bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(walkBool))
            animator.SetBool(walkBool, value);
    }
}
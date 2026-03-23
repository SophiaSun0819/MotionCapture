using UnityEngine;
using UnityEngine.AI;

public class SimpleCharacterMotor : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform visualRoot;

    [Header("Animator Params")]
    public string walkBool = "Walk";
    public string sitTrigger = "Sit";
    public string bigJumpTrigger = "BigJump";
    public string smallJumpTrigger = "SmallJump";

    [Header("Arrival")]
    public float arrivalThreshold = 0.03f;

    [Header("Turning")]
    public float turnSpeed = 10f;
    public bool rotateVisualToMovement = true;

    private Transform currentTarget;
    private bool destinationSet = false;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (visualRoot == null && animator != null)
            visualRoot = animator.transform;
    }

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (visualRoot == null && animator != null)
            visualRoot = animator.transform;
    }

    private void Update()
    {
        RotateVisualTowardMovement();
    }

    public void WarpTo(Transform point)
    {
        if (agent == null || point == null) return;

        agent.Warp(point.position);
        transform.position = point.position;
        transform.rotation = point.rotation;

        if (visualRoot != null)
            visualRoot.rotation = point.rotation;

        currentTarget = null;
        destinationSet = false;

        SetWalk(false);
    }

    public bool MoveTo(Transform point)
    {
        return MoveToInternal(point, true);
    }

    public bool MoveToNoWalk(Transform point)
    {
        return MoveToInternal(point, false);
    }

    private bool MoveToInternal(Transform point, bool useWalkAnim)
    {
        if (agent == null || point == null) return false;

        currentTarget = point;
        agent.isStopped = false;
        destinationSet = agent.SetDestination(point.position);

        SetWalk(useWalkAnim && destinationSet);
        return destinationSet;
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

        Quaternion lookRot = Quaternion.LookRotation(dir.normalized);
        transform.rotation = lookRot;

        if (visualRoot != null)
            visualRoot.rotation = lookRot;
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

    private void RotateVisualTowardMovement()
    {
        if (!rotateVisualToMovement || visualRoot == null || agent == null)
            return;

        Vector3 vel = agent.velocity;
        vel.y = 0f;

        if (vel.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(vel.normalized);
        visualRoot.rotation = Quaternion.Slerp(
            visualRoot.rotation,
            targetRot,
            Time.deltaTime * turnSpeed
        );
    }

    private void SetWalk(bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(walkBool))
            animator.SetBool(walkBool, value);
    }
}
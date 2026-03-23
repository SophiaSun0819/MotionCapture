using UnityEngine;
using UnityEngine.AI;

public class Page2CharacterMotor : MonoBehaviour
{
    public enum MoveAnimMode
    {
        None,
        Walk,
        CoughWalk
    }

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform visualRoot;

    [Header("Animator Params")]
    public string walkBool = "Walk";
    public string coughWalkBool = "CoughWalk";
    public string bigJumpTrigger = "BigJump";
    public string sitTrigger = "Sit";

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
        SetCoughWalk(false);
    }

    public bool MoveTo(Transform point, MoveAnimMode mode = MoveAnimMode.Walk)
    {
        if (agent == null || point == null) return false;

        currentTarget = point;
        agent.isStopped = false;
        destinationSet = agent.SetDestination(point.position);

        switch (mode)
        {
            case MoveAnimMode.Walk:
                SetWalk(destinationSet);
                SetCoughWalk(false);
                break;

            case MoveAnimMode.CoughWalk:
                SetWalk(false);
                SetCoughWalk(destinationSet);
                break;

            case MoveAnimMode.None:
                SetWalk(false);
                SetCoughWalk(false);
                break;
        }

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

        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = lookRot;

        if (visualRoot != null)
            visualRoot.rotation = lookRot;
    }

    public void PlayBigJump()
    {
        SetWalk(false);
        SetCoughWalk(false);

        if (animator != null && !string.IsNullOrEmpty(bigJumpTrigger))
            animator.SetTrigger(bigJumpTrigger);
    }

    public void PlaySit()
    {
        SetWalk(false);
        SetCoughWalk(false);

        if (animator != null && !string.IsNullOrEmpty(sitTrigger))
            animator.SetTrigger(sitTrigger);
    }

    public void HoldCoughWalkIdle()
    {
        StopMoving();
        SetWalk(false);
        SetCoughWalk(true);
    }

    public void ClearCoughWalk()
    {
        SetCoughWalk(false);
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

    private void SetCoughWalk(bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(coughWalkBool))
            animator.SetBool(coughWalkBool, value);
    }
}
using UnityEngine;
using UnityEngine.AI;

public class OldCharacterMotor : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Animator Params")]
    public string walkBool = "Walk";
    public string tripTrigger = "Trip";
    public string drawBowTrigger = "DrawBow";
    public string lookUpTrigger = "LookUp";
    public string sitTrigger = "Sit";

    [Header("Arrival")]
    public float arrivalThreshold = 0.5f;

    private bool isMoving = false;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void WarpTo(Transform point)
    {
        if (agent == null || point == null) return;
        agent.Warp(point.position);
        transform.rotation = point.rotation;
        isMoving = false;
        animator.SetBool(walkBool, false);
    }

    public void MoveTo(Transform point)
    {
        if (agent == null || point == null) return;
        agent.isStopped = false;
        agent.SetDestination(point.position);
        isMoving = true;
        animator.SetBool(walkBool, true);
    }

    public void StopMoving()
    {
        if (agent == null) return;
        agent.isStopped = true;
        agent.ResetPath();
        isMoving = false;
        animator.SetBool(walkBool, false);
    }

    public bool HasReachedDestination()
    {
        if (!isMoving) return false;
        if (agent.pathPending) return false;
        if (agent.remainingDistance > agent.stoppingDistance + arrivalThreshold) return false;
        return true;
    }

    public void FaceTarget(Transform point)
    {
        if (point == null) return;
        Vector3 dir = point.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    public void PlayTrip()
    {
        animator.SetBool(walkBool, false);
        animator.SetTrigger(tripTrigger);
    }

    public void PlayDrawBow()
    {
        animator.SetBool(walkBool, false);
        animator.SetTrigger(drawBowTrigger);
    }

    public void PlayLookUp()
    {
        animator.SetBool(walkBool, false);
        animator.SetTrigger(lookUpTrigger);
    }

    public void PlaySit()
    {
        animator.SetBool(walkBool, false);
        animator.SetTrigger(sitTrigger);
    }
}

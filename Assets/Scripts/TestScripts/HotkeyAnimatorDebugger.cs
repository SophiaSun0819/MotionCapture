using UnityEngine;

public class HotkeyAnimatorDebugger : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Child
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            animator.SetInteger("Child", 0);
            animator.SetInteger("Adult", 0);
            animator.SetInteger("Old", 0);

            animator.Play("Child_walk");
            Debug.Log("Switch to Child Walk");
        }

        // Adult
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            animator.SetInteger("Child", 0);
            animator.SetInteger("Adult", 1);
            animator.SetInteger("Old", 0);

            animator.Play("Adult_Walk");
            Debug.Log("Switch to Adult Walk");
        }

        // Old
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            animator.SetInteger("Child", 0);
            animator.SetInteger("Adult", 0);
            animator.SetInteger("Old", 2);

            animator.Play("Old_Walk");
            Debug.Log("Switch to Old Walk");
        }
    }
}
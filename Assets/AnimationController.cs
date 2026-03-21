using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public enum AnimParamType
{
    Trigger,
    Bool,
   
}

    public struct AnimEvent
{
    public Animator animator;
    public AnimParamType paramType;
    
    
    public string paraName; // triggerName or subStateName
   
    public bool boolValue;

    public AnimEvent(Animator anim, string name,AnimParamType type, bool b)
    {
        animator = anim;
        paraName=name;
        paramType=type;
        boolValue=b;

    }
}
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        EventCenter.AddListener<AnimEvent>(OnPlayAnim);
    }

    void OnDisable()
    {
        EventCenter.RemoveListener<AnimEvent>(OnPlayAnim);
    }

    private void OnPlayAnim(AnimEvent e)
    {   if(e.animator!=animator) return;

        switch (e.paramType)
        {
            case AnimParamType.Trigger:
                animator.SetTrigger(e.paraName);
                break;
            case AnimParamType.Bool:
                animator.SetBool(e.paraName,e.boolValue);
                break;
       
        }
        
    }
    
}
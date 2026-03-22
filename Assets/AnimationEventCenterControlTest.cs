using UnityEngine;

public class AnimationEventCenterControlTest : MonoBehaviour
{
    public Animator animator;

    public string paramName = "BigJump";
    public AnimationController.AnimParamType paramType;

    public bool boolValue;
   

    public bool triggerNow = false; // 👈 用这个当按钮

    void Update()
    {
        if (triggerNow)
        {
            TriggerEvent();
            triggerNow = false; // 👈 关键：触发一次就关掉
        }
    }

    void TriggerEvent()
    {
        EventCenter.Trigger(
            new AnimationController.AnimEvent(
                animator,
                paramName,
                paramType,
                boolValue
                
            )
        );

        Debug.Log("触发动画参数: " + paramName);
    }
}
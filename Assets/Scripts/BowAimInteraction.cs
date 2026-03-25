using UnityEngine;
using UnityEngine.InputSystem;

public class BowAimInteraction : MonoBehaviour
{
    [Header("Hold Settings")]
    public float requiredHoldTime = 2.5f;

    [Header("Input")]
    public InputActionReference triggerAction;

    [Header("Half VO")]
    public AudioSource voSource;
    public AudioClip oldHalfClip;

    [Header("UI")]
    public GameObject aimUI;
    public GameObject failVFX;

    public bool IsLocked { get; private set; }
    public bool IsAiming { get; private set; }
    public bool IsTriggerStarted { get; private set; }

    private float holdTimer = 0f;
    private bool wasTriggerHeld = false;
    private bool halfPlayed = false;

    public void StartAiming()
    {
        IsLocked = false;
        IsAiming = true;
        IsTriggerStarted = false;
        holdTimer = 0f;
        wasTriggerHeld = false;
        halfPlayed = false;
        if (aimUI != null) aimUI.SetActive(true);
    }

    public void StopAiming()
    {
        IsAiming = false;
        holdTimer = 0f;
        IsTriggerStarted = false;
        wasTriggerHeld = false;
        halfPlayed = false;
        if (aimUI != null) aimUI.SetActive(false);
    }

    private void Update()
    {
        if (!IsAiming || IsLocked) return;

        bool triggerHeld = false;
        if (triggerAction != null)
            triggerHeld = triggerAction.action.ReadValue<float>() > 0.5f;

        if (triggerHeld)
        {
            if (!IsTriggerStarted) IsTriggerStarted = true;
            holdTimer += Time.deltaTime;

            if (!halfPlayed && holdTimer >= requiredHoldTime * 0.5f)
            {
                halfPlayed = true;
                if (voSource != null && oldHalfClip != null)
                    voSource.PlayOneShot(oldHalfClip);
            }

            if (holdTimer >= requiredHoldTime) LockSuccess();
        }
        else
        {
            if (wasTriggerHeld && !IsLocked) ReleaseFail();
        }

        wasTriggerHeld = triggerHeld;
    }

    private void LockSuccess()
    {
        IsLocked = true;
        IsAiming = false;
        holdTimer = 0f;
        if (aimUI != null) aimUI.SetActive(false);
        Debug.Log("[BowAim] Success!");
    }

    private void ReleaseFail()
    {
        holdTimer = 0f;
        IsTriggerStarted = false;
        wasTriggerHeld = false;
        halfPlayed = false;
        if (failVFX != null)
        {
            failVFX.SetActive(true);
            Invoke(nameof(HideFailVFX), 1f);
        }
        Debug.Log("[BowAim] Fail - released too early!");
    }

    private void HideFailVFX()
    {
        if (failVFX != null) failVFX.SetActive(false);
    }
}
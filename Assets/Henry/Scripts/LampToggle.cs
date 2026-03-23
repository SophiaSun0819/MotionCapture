using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSimpleInteractable))]
public class LampToggle : MonoBehaviour
{
    [Header("Lamp")]
    public GameObject lightObject; // the child object that represents the light

    [Header("Optional")]
    public AudioSource audioSource;
    public AudioClip toggleOnClip;
    public AudioClip toggleOffClip;

    [Header("Haptics")]
    public float hapticAmplitude = 0.6f;
    public float hapticDuration = 0.1f;

    private XRSimpleInteractable interactable;
    private bool isOn = false;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();

        if (lightObject != null)
            isOn = lightObject.activeSelf;
    }

    private void OnEnable()
    {
        interactable.selectExited.AddListener(OnToggle);
    }

    private void OnDisable()
    {
        interactable.selectExited.RemoveListener(OnToggle);
    }

    private void OnToggle(SelectExitEventArgs args)
    {
        ToggleLight();
        SendHaptics(args.interactorObject);
    }

    private void ToggleLight()
    {
        if (lightObject == null) return;

        isOn = !isOn;
        lightObject.SetActive(isOn);

        // Play sound
        if (audioSource != null)
        {
            if (isOn && toggleOnClip != null)
                audioSource.PlayOneShot(toggleOnClip);
            else if (!isOn && toggleOffClip != null)
                audioSource.PlayOneShot(toggleOffClip);
        }
    }

    private void SendHaptics(object interactorObj)
    {
        if (interactorObj is XRBaseInputInteractor inputInteractor)
        {
            if (inputInteractor.TryGetComponent<XRBaseController>(out var controller))
            {
                controller.SendHapticImpulse(hapticAmplitude, hapticDuration);
            }
        }
    }
}
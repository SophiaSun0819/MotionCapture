using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSimpleInteractable))]
public class MapClickableObject : MonoBehaviour
{
    [Header("Main Object")]
    public Renderer targetRenderer;

    [Header("Glow Object")]
    public Renderer glowRenderer;
    public string emissionProperty = "_EmissionColor";

    [Header("Glow Settings")]
    public Color glowColor = Color.yellow;
    public float pulseSpeed = 2f;
    public float pulseMin = 0.5f;
    public float pulseMax = 2.5f;

    [Header("Hover Glow")]
    public float hoverIntensity = 4f;

    [Header("Click")]
    public float clickScaleMultiplier = 1.12f;
    public float clickAnimDuration = 0.12f;
    public bool disableAfterUse = true;
    public bool disableGameObjectAfterUse = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;

    [Header("Haptics")]
    public float hoverHapticAmplitude = 0.3f;
    public float hoverHapticDuration = 0.05f;
    public float clickHapticAmplitude = 0.9f;
    public float clickHapticDuration = 0.18f;

    [Header("Page Trigger")]
    public MapFlowController flowController;
    public int pageIndex = 1;

    private XRSimpleInteractable interactable;
    private MaterialPropertyBlock mpb;
    private bool isHovered = false;
    private bool hasBeenUsed = false;
    private bool hoverPlayed = false;
    private Vector3 originalScale;
    private Coroutine scaleRoutine;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        mpb = new MaterialPropertyBlock();
        originalScale = transform.localScale;

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();
    }

    private void OnEnable()
    {
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
        interactable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        interactable.hoverEntered.RemoveListener(OnHoverEntered);
        interactable.hoverExited.RemoveListener(OnHoverExited);
        interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void Update()
    {
        UpdateGlow();
    }

    private void UpdateGlow()
    {
        if (glowRenderer == null) return;

        glowRenderer.GetPropertyBlock(mpb);

        float intensity;

        if (isHovered)
        {
            // Hover = steady bright glow
            intensity = hoverIntensity;
        }
        else
        {
            // Idle = pulsing glow
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            intensity = Mathf.Lerp(pulseMin, pulseMax, pulse);
        }

        Color emission = glowColor * intensity;

        if (glowRenderer.sharedMaterial != null &&
            glowRenderer.sharedMaterial.HasProperty(emissionProperty))
        {
            mpb.SetColor(emissionProperty, emission);
        }

        glowRenderer.SetPropertyBlock(mpb);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (hasBeenUsed) return;

        isHovered = true;

        if (!hoverPlayed)
        {
            PlayClip(hoverClip);
            hoverPlayed = true;
        }

        SendHaptics(args.interactorObject, hoverHapticAmplitude, hoverHapticDuration);
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (hasBeenUsed) return;

        isHovered = false;
        hoverPlayed = false;
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (hasBeenUsed) return;

        hasBeenUsed = true;
        isHovered = false;

        PlayClip(clickClip);
        SendHaptics(args.interactorObject, clickHapticAmplitude, clickHapticDuration);
        PunchScale();

        if (flowController != null)
        {
            if (pageIndex == 1)
                flowController.StartPage1();
            else if (pageIndex == 2)
                flowController.StartPage2();
        }

        if (disableAfterUse)
        {
            if (disableGameObjectAfterUse)
                StartCoroutine(DisableGameObjectNextFrame());
            else
                StartCoroutine(DisableInteractableNextFrame());
        }
    }

    private void PunchScale()
    {
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(PunchScaleRoutine());
    }

    private IEnumerator PunchScaleRoutine()
    {
        Vector3 target = originalScale * clickScaleMultiplier;

        float t = 0f;
        while (t < clickAnimDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / clickAnimDuration);
            transform.localScale = Vector3.Lerp(originalScale, target, lerp);
            yield return null;
        }

        t = 0f;
        while (t < clickAnimDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / clickAnimDuration);
            transform.localScale = Vector3.Lerp(target, originalScale, lerp);
            yield return null;
        }

        transform.localScale = originalScale;
        scaleRoutine = null;
    }

    private IEnumerator DisableInteractableNextFrame()
    {
        yield return null;
        if (interactable != null)
            interactable.enabled = false;
    }

    private IEnumerator DisableGameObjectNextFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void SendHaptics(object interactorObj, float amplitude, float duration)
    {
        if (interactorObj is XRBaseInputInteractor inputInteractor)
        {
            var device = inputInteractor.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.XRBaseController>();

            if (device != null)
            {
                device.SendHapticImpulse(amplitude, duration);
            }
        }
    }
}
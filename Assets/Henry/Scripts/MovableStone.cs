using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class MovableStone : MonoBehaviour
{
    [Header("Clear")]
    public string clearZoneTag = "StoneClearZone";

    [Header("Hover Visual")]
    public GameObject hoverVisual;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverClip;
    public AudioClip pickupClip;
    public AudioClip clearedClip;

    public event Action<MovableStone> OnCleared;

    private XRGrabInteractable grabInteractable;
    private bool hoverPlayed = false;
    private bool isCleared = false;

    public bool IsCleared => isCleared;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (hoverVisual != null)
            hoverVisual.SetActive(false);
    }

    private void OnEnable()
    {
        grabInteractable.hoverEntered.AddListener(OnHoverEntered);
        grabInteractable.hoverExited.AddListener(OnHoverExited);
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        grabInteractable.hoverEntered.RemoveListener(OnHoverEntered);
        grabInteractable.hoverExited.RemoveListener(OnHoverExited);
        grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (isCleared) return;

        if (hoverVisual != null)
            hoverVisual.SetActive(true);

        if (!hoverPlayed)
        {
            PlayClip(hoverClip);
            hoverPlayed = true;
        }
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (isCleared) return;

        if (hoverVisual != null)
            hoverVisual.SetActive(false);

        hoverPlayed = false;
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (isCleared) return;

        PlayClip(pickupClip);

        if (hoverVisual != null)
            hoverVisual.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCleared) return;
        if (!other.CompareTag(clearZoneTag)) return;

        isCleared = true;

        if (hoverVisual != null)
            hoverVisual.SetActive(false);

        PlayClip(clearedClip);
        OnCleared?.Invoke(this);
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
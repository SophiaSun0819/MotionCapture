using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class SnapPlank : MonoBehaviour
{
    [Header("Snap")]
    public Transform snapPoint;
    public float snapDistance = 0.35f;
    public string snapZoneTag = "PlankSnapZone";

    [Header("Hover Visual")]
    public GameObject hoverVisual;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverClip;
    public AudioClip pickupClip;
    public AudioClip snapClip;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private bool isSnapped = false;
    private bool hoverSoundPlayed = false;
    private bool isHeld = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (hoverVisual != null)
            hoverVisual.SetActive(false);
    }

    private void OnEnable()
    {
        grabInteractable.hoverEntered.AddListener(OnHoverEntered);
        grabInteractable.hoverExited.AddListener(OnHoverExited);
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        grabInteractable.hoverEntered.RemoveListener(OnHoverEntered);
        grabInteractable.hoverExited.RemoveListener(OnHoverExited);
        grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
        grabInteractable.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (isSnapped) return;

        if (hoverVisual != null)
            hoverVisual.SetActive(true);

        if (!hoverSoundPlayed)
        {
            PlayClip(hoverClip);
            hoverSoundPlayed = true;
        }
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (isSnapped) return;

        if (hoverVisual != null)
            hoverVisual.SetActive(false);

        hoverSoundPlayed = false;
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (isSnapped) return;

        isHeld = true;

        if (hoverVisual != null)
            hoverVisual.SetActive(false);

        PlayClip(pickupClip);
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (isSnapped) return;

        isHeld = false;
        TrySnap();
    }

    private void OnTriggerStay(Collider other)
    {
        if (isSnapped) return;
        if (!other.CompareTag(snapZoneTag)) return;

        // Optional: allow snap while still held if close enough
        TrySnap();
    }

    private void TrySnap()
    {
        if (snapPoint == null || isSnapped) return;

        float dist = Vector3.Distance(transform.position, snapPoint.position);
        if (dist > snapDistance) return;

        ForceSnap();
    }

    private void ForceSnap()
    {
        isSnapped = true;
        isHeld = false;

        // If currently selected, force deselect by disabling interactable briefly
        grabInteractable.enabled = false;

        transform.position = snapPoint.position;
        transform.rotation = snapPoint.rotation;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        if (hoverVisual != null)
            hoverVisual.SetActive(false);

        PlayClip(snapClip);
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
using System.Collections;
using UnityEngine;

public class TurnPage : MonoBehaviour
{
    public LocalPortalController portal;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip pageTurnClip;

    private void Awake()
    {
        if (portal == null)
            portal = GetComponent<LocalPortalController>();
    }

    public IEnumerator Switch(GameObject currentPage, GameObject nextPage)
    {
        PlaySFX(pageTurnClip);
        yield return new WaitForSeconds(0.05f);

        yield return StartCoroutine(Shrink());

        if (currentPage != null) currentPage.SetActive(false);
        if (nextPage != null) nextPage.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(Expand());
    }

    private IEnumerator Shrink()
    {
        if (portal == null)
            yield break;

        portal.ShrinkPortal();

        while (portal.radius > 0.01f)
            yield return null;
    }

    private IEnumerator Expand()
    {
        if (portal == null)
            yield break;

        portal.ExpandPortal();

        while (portal.radius < 2.99f)
            yield return null;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }
}
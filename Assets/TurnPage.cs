using System.Collections;
using UnityEngine;

public class TurnPage : MonoBehaviour
{
    public LocalPortalController portal;

    private void Awake()
    {
        if (portal == null)
            portal = GetComponent<LocalPortalController>();
    }

    public IEnumerator Switch(GameObject from, GameObject to)
    {
        yield return StartCoroutine(Shrink());

        if (from != null) from.SetActive(false);
        if (to != null) to.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(Expand());
    }

    IEnumerator Shrink()
    {
        portal.ShrinkPortal();

        while (portal.radius > 0.01f)
            yield return null;
    }

    IEnumerator Expand()
    {
        portal.ExpandPortal();

        while (portal.radius < 2.99f)
            yield return null;
    }
}
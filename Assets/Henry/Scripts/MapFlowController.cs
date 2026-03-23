using System.Collections;
using UnityEngine;

public class MapFlowController : MonoBehaviour
{
    [Header("Portal")]
    public TurnPage turnPage;

    [Header("Maps")]
    public GameObject map1;
    public GameObject map2;
    public GameObject map3;

    [Header("Pages")]
    public GameObject page1;
    public GameObject page2;
    public GameObject page3;

    [Header("Page Controllers")]
    public Page1Controller page1Controller;
    public Page2Controller page2Controller;
    // public Page3Controller page3Controller;

    private int currentStage = 0;

    private void Start()
    {
        map1.SetActive(true);
        map2.SetActive(false);
        map3.SetActive(false);

        page1.SetActive(false);
        page2.SetActive(false);
        page3.SetActive(false);
    }

    // 🔥 Called when player clicks object on map
    public void StartPage1()
    {
        StartCoroutine(MapToPage(map1, page1, page1Controller));
    }

    public void StartPage2()
    {
        StartCoroutine(MapToPage(map2, page2, page2Controller));
    }

    // 🔥 Called when page finishes
    public void GoToMap2()
    {
        StartCoroutine(PageToMap(page1, map2));
    }

    public void GoToMap3()
    {
        StartCoroutine(PageToMap(page2, map3));
    }

    IEnumerator MapToPage(GameObject map, GameObject page, MonoBehaviour controller)
    {
        yield return StartCoroutine(turnPage.Switch(map, page));

        if (controller != null)
        {
            controller.Invoke("StartPage", 0f);
        }
    }

    IEnumerator PageToMap(GameObject page, GameObject map)
    {
        yield return StartCoroutine(turnPage.Switch(page, map));
    }
}
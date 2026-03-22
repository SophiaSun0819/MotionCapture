using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class TurnPage : MonoBehaviour
{
    LocalPortalController localPortalController;
    public GameObject page1;
    public GameObject page2;
    public GameObject page3;
    [Header("Debug Triggers")]
    public bool testPage1To2 = false;
    public bool testPage2To3 = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        localPortalController=GetComponent<LocalPortalController>();
    }

    // Update is called once per frame
    void Update()
    {
        // 在 Inspector 里勾选这个 bool 就能触发测试
        if (testPage1To2)
        {
            testPage1To2 = false; // 自动重置，防止重复触发
            StartCoroutine(SwitchPageSequence(page1, page2));
            // SwicthPage(page1,page2);
        }

        if (testPage2To3)
        {
            testPage2To3 = false;
            StartCoroutine(SwitchPageSequence(page2, page3));
            //  SwicthPage(page2,page3);
        }
    }
    // void SwicthPage(GameObject currentPage,GameObject nextPage)
    // {
    //     StartCoroutine(Shrink());
    //     currentPage.SetActive(false);
    //     nextPage.SetActive(true);


    // }
    // 核心修改：将 SwitchPage 逻辑封装进一个大的协程序列
    IEnumerator SwitchPageSequence(GameObject currentPage, GameObject nextPage)
    {
        // 1. 先让当前页面缩小
        yield return StartCoroutine(Shrink());

        // 2. 缩小动画彻底播完后，再切换显示状态
        if (currentPage != null) currentPage.SetActive(false);
        if (nextPage != null) nextPage.SetActive(true);

        // 3. 让新页面展开
        yield return StartCoroutine(Expand());
    }
    IEnumerator Shrink()
    {
        localPortalController.ShrinkPortal();
        while (localPortalController.radius > 0.01f)
    {
        yield return null; // 每帧等待，直到半径接近 0
    }
    }

    IEnumerator Expand()
    {
        localPortalController.ExpandPortal();
        while (localPortalController.radius < 2.99f)
    {
        // 告诉 Unity：这一帧先停这，下一帧再回来检查条件
        yield return null; 
    }
    }
}

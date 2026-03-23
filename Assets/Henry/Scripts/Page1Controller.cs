using System.Collections;
using UnityEngine;

public class Page1Controller : MonoBehaviour
{
    public MapFlowController flowController;
    [Header("Debug")]
    public bool autoStartOnEnable = false;
    public bool skipIntroSequence = false;
    public bool verboseLogs = true;

    [Header("Page Objects")]
    public GameObject pageRoot;

    [Header("Audio")]
    public AudioSource voSource;

    [Header("Voice Over")]
    public AudioClip introClip;
    public AudioClip walkingToBridgeClip;
    public AudioClip idleAtBridgeClip;
    public AudioClip childHintClip;
    public AudioClip firstPlankClip;
    public AudioClip secondPlankClip;
    public AudioClip bridgeFinishedClip;
    public AudioClip diaryClip;

    [Header("Character")]
    public SimpleCharacterMotor childMotor;
    public Transform startPoint;
    public Transform bridgeWaitPoint;
    public Transform bridgeStartPoint;
    public Transform bridgeEndPoint;
    public Transform diaryPoint;

    [Header("Puzzle")]
    public BridgePuzzleTracker bridgePuzzle;

    [Header("Timing")]
    public float pageMagicDuration = 1.25f;
    public float jumpOntoBridgeDuration = 1.0f;
    public float jumpOffBridgeDuration = 0.75f;
    public float afterSitDelay = 1.0f;

    private bool pageStarted = false;
    private bool firstPlankVOPlayed = false;
    private bool secondPlankVOPlayed = false;

    private void OnEnable()
    {
        if (autoStartOnEnable)
            StartPage();
    }

    public void StartPage()
    {
        if (pageStarted) return;
        pageStarted = true;
        StartCoroutine(Page1Sequence());
    }

    private IEnumerator Page1Sequence()
    {
        if (pageRoot != null && !pageRoot.activeSelf)
            pageRoot.SetActive(true);

        if (childMotor == null || startPoint == null || bridgeWaitPoint == null || bridgeStartPoint == null || bridgeEndPoint == null || diaryPoint == null)
        {
            LogError("Missing one or more required references.");
            yield break;
        }

        if (bridgePuzzle == null)
        {
            LogError("BridgePuzzleTracker is not assigned.");
            yield break;
        }

        childMotor.WarpTo(startPoint);
        yield return null;
        yield return null;

        childMotor.MoveTo(bridgeWaitPoint);

        if (!skipIntroSequence)
            yield return PlayVO(walkingToBridgeClip);

        yield return new WaitUntil(() => childMotor.HasReachedDestination());

        yield return PlayVO(idleAtBridgeClip);
        yield return PlayVO(childHintClip);

        while (!bridgePuzzle.IsBridgeComplete())
        {
            int snappedCount = bridgePuzzle.GetPlacedCount();

            if (!firstPlankVOPlayed && snappedCount >= 1)
            {
                firstPlankVOPlayed = true;
                yield return PlayVO(firstPlankClip);
            }

            if (!secondPlankVOPlayed && snappedCount >= 2)
            {
                secondPlankVOPlayed = true;
                yield return PlayVO(secondPlankClip);
            }

            yield return null;
        }

        yield return PlayVO(bridgeFinishedClip);

        childMotor.MoveTo(bridgeStartPoint);
        yield return new WaitUntil(() => childMotor.HasReachedDestination());

        childMotor.FaceTarget(bridgeEndPoint);
        childMotor.PlayBigJump();
        yield return new WaitForSeconds(jumpOntoBridgeDuration);

        childMotor.MoveToNoWalk(bridgeEndPoint);
        yield return new WaitUntil(() => childMotor.HasReachedDestination());

        childMotor.FaceTarget(diaryPoint);
        childMotor.PlaySmallJump();
        yield return new WaitForSeconds(jumpOffBridgeDuration);

        childMotor.MoveTo(diaryPoint);
        yield return new WaitUntil(() => childMotor.HasReachedDestination());

        childMotor.PlaySit();
        yield return new WaitForSeconds(afterSitDelay);

        yield return PlayVO(diaryClip);

        Log("Page 1 complete.");

        // GO BACK TO MAP
        if (flowController != null)
        {
            flowController.GoToMap2();
        }
    }

    private IEnumerator PlayVO(AudioClip clip)
    {
        if (clip == null || voSource == null)
            yield break;

        voSource.clip = clip;
        voSource.Play();

        while (voSource.isPlaying)
            yield return null;
    }

    private void Log(string msg)
    {
        if (verboseLogs)
            Debug.Log("[Page1Controller] " + msg, this);
    }

    private void LogError(string msg)
    {
        Debug.LogError("[Page1Controller] " + msg, this);
    }
    
}
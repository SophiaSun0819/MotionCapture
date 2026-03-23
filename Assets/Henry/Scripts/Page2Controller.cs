using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Page2Controller : MonoBehaviour
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
    public AudioClip walkingToCliffClip;   // NVO
    public AudioClip cliffIdleClip;        // NVO
    public AudioClip adultCliffIdleClip;   // AVO after landing / before moving
    public AudioClip firstStoneClip;       // AVO
    public AudioClip secondStoneClip;      // AVO
    public AudioClip lastStoneClip;        // AVO
    public AudioClip diaryClip;            // NVO / diary

    [Header("Character")]
    public Page2CharacterMotor adultMotor;
    public Transform startPoint;
    public Transform cliffEdgePoint;
    public Transform bottomLandPoint;
    public Transform rockWaitPoint;
    public Transform rampTopPoint;
    public Transform diaryPoint;

    [Header("Puzzle")]
    public StonePuzzleTracker stonePuzzle;

    [Header("Wall Fade")]
    public MaterialFadeGroup transparentWallGroup;

    [Header("Timing")]
    public float pageMagicDuration = 1.25f;
    public float afterSitDelay = 1.0f;
    public float afterLandPause = 0.2f;

    [Header("Jump Arc")]
    public float jumpHeight = 1.5f;
    public float jumpDuration = 1.1f;

    private bool pageStarted = false;
    private bool firstStonePlayed = false;
    private bool secondStonePlayed = false;
    private bool lastStonePlayed = false;

    private void OnEnable()
    {
        if (autoStartOnEnable)
            StartPage();
    }

    public void StartPage()
    {
        if (pageStarted) return;
        pageStarted = true;
        StartCoroutine(Page2Sequence());
    }

    private IEnumerator MoveAndPlayVO(Transform target, AudioClip clip, Page2CharacterMotor.MoveAnimMode moveMode)
    {
        bool startedMove = adultMotor.MoveTo(target, moveMode);
        if (!startedMove)
        {
            LogError("Failed to path to " + target.name);
            yield break;
        }

        bool voWasStarted = false;

        if (clip != null && voSource != null)
        {
            voSource.clip = clip;
            voSource.Play();
            voWasStarted = true;
        }

        // Character reaches point first and should idle immediately
        yield return new WaitUntil(() => adultMotor.HasReachedDestination());
        adultMotor.StopMoving();

        // But sequence should wait until VO finishes before continuing
        if (voWasStarted)
        {
            while (voSource.isPlaying)
                yield return null;
        }
    }

    private IEnumerator ArcJumpToPoint(Vector3 targetPosition, float height, float duration)
    {
        if (adultMotor == null || adultMotor.agent == null)
            yield break;

        NavMeshAgent agent = adultMotor.agent;

        Vector3 startPos = adultMotor.transform.position;
        Vector3 endPos = targetPosition;

        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            Vector3 pos = Vector3.Lerp(startPos, endPos, t);

            float arc = 4f * height * t * (1f - t);
            pos.y += arc;

            adultMotor.transform.position = pos;

            Vector3 nextFlat = endPos - adultMotor.transform.position;
            nextFlat.y = 0f;
            if (nextFlat.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(nextFlat.normalized);
                adultMotor.transform.rotation = lookRot;

                if (adultMotor.visualRoot != null)
                    adultMotor.visualRoot.rotation = lookRot;
            }

            yield return null;
        }

        adultMotor.transform.position = endPos;

        // Reattach to lower navmesh
        agent.Warp(endPos);
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.isStopped = false;

        yield return null;
    }

    private IEnumerator Page2Sequence()
    {
        if (pageRoot != null && !pageRoot.activeSelf)
            pageRoot.SetActive(true);

        if (adultMotor == null || startPoint == null || cliffEdgePoint == null || bottomLandPoint == null || rockWaitPoint == null || rampTopPoint == null || diaryPoint == null)
        {
            LogError("Missing one or more required references.");
            yield break;
        }

        if (stonePuzzle == null)
        {
            LogError("StonePuzzleTracker is not assigned.");
            yield break;
        }

        adultMotor.WarpTo(startPoint);
        yield return null;
        yield return null;

        // Walk to cliff edge while VO plays
        yield return MoveAndPlayVO(cliffEdgePoint, walkingToCliffClip, Page2CharacterMotor.MoveAnimMode.Walk);

        // Jump / fall
        adultMotor.FaceTarget(bottomLandPoint);
        adultMotor.PlayBigJump();

        yield return StartCoroutine(
            ArcJumpToPoint(bottomLandPoint.position, jumpHeight, jumpDuration)
        );

        yield return new WaitForSeconds(afterLandPause);

        // Reveal rocks by fading walls transparent
        if (transparentWallGroup != null)
            transparentWallGroup.FadeTransparent();

        // NVO idle in cliff
        yield return PlayVO(cliffIdleClip);

        // Hold cough-walk in place first
        adultMotor.HoldCoughWalkIdle();

        // AVO plays, then character starts moving
        yield return PlayVO(adultCliffIdleClip);

        bool startedCoughMove = adultMotor.MoveTo(rockWaitPoint, Page2CharacterMotor.MoveAnimMode.CoughWalk);
        if (!startedCoughMove)
        {
            LogError("Failed to path to rockWaitPoint. Check that rockWaitPoint is on the lower NavMesh.");
            yield break;
        }

        yield return new WaitUntil(() => adultMotor.HasReachedDestination());

        // Keep cough-walk looping while waiting for player
        adultMotor.HoldCoughWalkIdle();

        bool puzzleDone = false;

        while (!puzzleDone)
        {
            int count = stonePuzzle.GetClearedCount();

            if (!firstStonePlayed && count >= 1)
            {
                firstStonePlayed = true;
                yield return PlayVO(firstStoneClip);
            }

            if (!secondStonePlayed && count >= 2)
            {
                secondStonePlayed = true;
                yield return PlayVO(secondStoneClip);
            }

            if (!lastStonePlayed && count >= 3)
            {
                lastStonePlayed = true;
                yield return PlayVO(lastStoneClip);
                puzzleDone = true;
            }

            yield return null;
        }

        // Stay in cough-walk until reaching the top point
        bool startedRampMove = adultMotor.MoveTo(rampTopPoint, Page2CharacterMotor.MoveAnimMode.CoughWalk);
        if (!startedRampMove)
        {
            LogError("Failed to path to rampTopPoint. Check that rampTopPoint is on the ramp NavMesh.");
            yield break;
        }

        yield return new WaitUntil(() => adultMotor.HasReachedDestination());

        // Fade walls back in when character reaches the top
        if (transparentWallGroup != null)
            transparentWallGroup.FadeOpaque();

        // Reset before normal walk
        adultMotor.StopMoving();
        adultMotor.ClearCoughWalk();
        adultMotor.FaceTarget(diaryPoint);

        yield return null;

        bool startedDiaryMove = adultMotor.MoveTo(diaryPoint, Page2CharacterMotor.MoveAnimMode.Walk);
        if (!startedDiaryMove)
        {
            LogError("Failed to path to diaryPoint. Check that diaryPoint is on walkable NavMesh.");
            yield break;
        }

        yield return new WaitUntil(() => adultMotor.HasReachedDestination());

        adultMotor.StopMoving();
        adultMotor.PlaySit();
        yield return new WaitForSeconds(afterSitDelay);

        yield return PlayVO(diaryClip);

        Log("Page 2 complete.");
        
        // GO BACK TO MAP
        if (flowController != null)
        {
            flowController.GoToMap3();
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
            Debug.Log("[Page2Controller] " + msg, this);
    }

    private void LogError(string msg)
    {
        Debug.LogError("[Page2Controller] " + msg, this);
    }
}
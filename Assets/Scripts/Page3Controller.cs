using System.Collections;
using UnityEngine;

public class Page3Controller : MonoBehaviour
{
    [Header("Debug")]
    public bool autoStartOnEnable = false;
    public bool verboseLogs = true;

    [Header("Character")]
    public OldCharacterMotor oldMotor;
    public BowAimInteraction bowAimInteraction;

    [Header("Waypoints")]
    public Transform startPoint;
    public Transform climbingPoint;
    public Transform tripPoint;
    public Transform shootPoint;
    public Transform diaryPoint;

    [Header("Scene Objects")]
    public GameObject sunObject;
    public GameObject moonObject;

    [Header("Audio")]
    public AudioSource voSource;
    public AudioSource sfxSource;

    [Header("Voice Over Clips")]
    public AudioClip oldWalkingClip;
    public AudioClip oldTopClip;
    public AudioClip oldLookUpClip;
    public AudioClip oldAskBowClip;
    public AudioClip oldShootClip;
    public AudioClip oldThanksClip;
    public AudioClip oldDiaryClip;

    [Header("SFX Clips")]
    public AudioClip pageTurnClip;
    public AudioClip sunBreakClip;

    [Header("Timing")]
    public float tripRecoverDuration = 3.0f;
    public float drawBowDuration = 4.0f;
    public float arrowFlightDuration = 2.0f;
    public float lookUpDuration = 3.0f;
    public float skyTransitionDuration = 3.0f;

    private bool pageStarted = false;

    private void OnEnable()
    {
        if (autoStartOnEnable) StartPage();
    }

    public void StartPage()
    {
        if (pageStarted) return;
        pageStarted = true;
        StartCoroutine(Page3Sequence());
    }

    private IEnumerator Page3Sequence()
    {
        if (oldMotor == null || startPoint == null || climbingPoint == null ||
            tripPoint == null || shootPoint == null || diaryPoint == null)
        {
            Debug.LogError("[Page3Controller] Missing required references.", this);
            yield break;
        }

        if (moonObject != null) moonObject.SetActive(false);

        // 1. Warp 到山腳
        oldMotor.WarpTo(startPoint);
        yield return null;
        PlaySFX(pageTurnClip);

        // 2. 爬山到中繼點
        oldMotor.MoveTo(climbingPoint);
        yield return PlayVO(oldWalkingClip);
        yield return new WaitUntil(() => oldMotor.HasReachedDestination());
        oldMotor.StopMoving();

        // 3. 繼續到滑倒點
        oldMotor.MoveTo(tripPoint);
        yield return new WaitUntil(() => oldMotor.HasReachedDestination());
        oldMotor.StopMoving();
        oldMotor.PlayTrip();
        yield return new WaitForSeconds(tripRecoverDuration);

        // 4. 爬到山頂
        oldMotor.MoveTo(shootPoint);
        yield return new WaitUntil(() => oldMotor.HasReachedDestination());

        // 6. 仰望太陽
        oldMotor.PlayLookUp();
        yield return new WaitForSeconds(lookUpDuration);

        // 7. 請玩家幫忙拉弓
        yield return PlayVO(oldAskBowClip);
        bowAimInteraction.StartAiming();

        // 等玩家按下 Trigger 才觸發拉弓動畫
        yield return new WaitUntil(() => bowAimInteraction.IsTriggerStarted);
        oldMotor.PlayDrawBow();

        // 等玩家 Hold 滿放箭
        yield return new WaitUntil(() => bowAimInteraction.IsLocked);

        // 8. 放箭
        PlaySFX(oldShootClip);
        yield return new WaitForSeconds(arrowFlightDuration);

        // 9. 太陽碎裂 + 天空漸暗 + 月亮出現
        PlaySFX(sunBreakClip);
        yield return StartCoroutine(SunBreakEffect());
        yield return StartCoroutine(FadeToNight());
        if (moonObject != null) moonObject.SetActive(true);

        // 9. 仰望天空（看太陽變月亮）
        oldMotor.PlayLookUp();
        yield return PlayVO(oldLookUpClip);
        yield return new WaitForSeconds(lookUpDuration);

        // 10. 走到日記位置坐下
        oldMotor.MoveTo(diaryPoint);
        yield return new WaitUntil(() => oldMotor.HasReachedDestination());
        oldMotor.StopMoving();
        oldMotor.PlaySit();
        yield return PlayVO(oldDiaryClip);

        // 11. 最後感謝玩家
        yield return PlayVO(oldThanksClip);

        if (verboseLogs) Debug.Log("[Page3Controller] Page 3 完成", this);
    }

    private IEnumerator FadeToNight()
    {
        Color fromAmbient = RenderSettings.ambientLight;
        Color toAmbient = new Color(0.05f, 0.05f, 0.15f);
        Color fromFog = RenderSettings.fogColor;
        Color toFog = new Color(0.02f, 0.02f, 0.08f);
        float elapsed = 0f;
        while (elapsed < skyTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float ratio = elapsed / skyTransitionDuration;
            RenderSettings.ambientLight = Color.Lerp(fromAmbient, toAmbient, ratio);
            RenderSettings.fogColor = Color.Lerp(fromFog, toFog, ratio);
            yield return null;
        }
        RenderSettings.ambientLight = toAmbient;
        RenderSettings.fogColor = toFog;
    }

    private IEnumerator SunBreakEffect()
    {
        if (sunObject == null) yield break;

        // 先嘗試用 ShaderPosition
        ShaderPosition sp = sunObject.GetComponent<ShaderPosition>();
        if (sp != null)
        {
            sp.PlayRadiusFade();
            yield return new WaitForSeconds(sp.duration);
        }
        else
        {
            // 沒有 ShaderPosition 就直接縮放消失
            float elapsed = 0f;
            float duration = 1.5f;
            Vector3 originalScale = sunObject.transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                sunObject.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                yield return null;
            }
        }

        sunObject.SetActive(false);
    }

    private IEnumerator PlayVO(AudioClip clip)
    {
        if (clip == null || voSource == null) yield break;
        voSource.clip = clip;
        voSource.Play();
        while (voSource.isPlaying) yield return null;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
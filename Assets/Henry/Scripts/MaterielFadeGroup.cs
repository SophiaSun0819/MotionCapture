using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialFadeGroup : MonoBehaviour
{
    [System.Serializable]
    public class FadeTarget
    {
        public Renderer renderer;
        public int materialIndex = 0;
    }

    public List<FadeTarget> targets = new List<FadeTarget>();
    public float fadeDuration = 0.5f;
    public float transparentAlpha = 0.2f;

    private readonly List<Material> runtimeMaterials = new List<Material>();
    private Coroutine fadeRoutine;

    private void Awake()
    {
        runtimeMaterials.Clear();

        foreach (var t in targets)
        {
            if (t.renderer == null) continue;

            Material[] mats = t.renderer.materials;
            if (t.materialIndex < 0 || t.materialIndex >= mats.Length) continue;

            runtimeMaterials.Add(mats[t.materialIndex]);
        }
    }

    public void FadeTransparent()
    {
        StartFade(transparentAlpha);
    }

    public void FadeOpaque()
    {
        StartFade(1f);
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeTo(targetAlpha));
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (runtimeMaterials.Count == 0)
            yield break;

        List<float> startAlphas = new List<float>();

        foreach (var mat in runtimeMaterials)
        {
            Color c = GetColor(mat);
            startAlphas.Add(c.a);
        }

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / fadeDuration);

            for (int i = 0; i < runtimeMaterials.Count; i++)
            {
                Material mat = runtimeMaterials[i];
                Color c = GetColor(mat);
                c.a = Mathf.Lerp(startAlphas[i], targetAlpha, lerp);
                SetColor(mat, c);
            }

            yield return null;
        }

        for (int i = 0; i < runtimeMaterials.Count; i++)
        {
            Material mat = runtimeMaterials[i];
            Color c = GetColor(mat);
            c.a = targetAlpha;
            SetColor(mat, c);
        }

        fadeRoutine = null;
    }

    private Color GetColor(Material mat)
    {
        if (mat.HasProperty("_BaseColor"))
            return mat.GetColor("_BaseColor");

        if (mat.HasProperty("_Color"))
            return mat.color;

        return Color.white;
    }

    private void SetColor(Material mat, Color c)
    {
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", c);
        else if (mat.HasProperty("_Color"))
            mat.color = c;
    }
}
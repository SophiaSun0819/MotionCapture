using UnityEngine;

[ExecuteInEditMode]
public class ShaderPosition : MonoBehaviour
{
    public float radius = 1f;
    
    [Header("Animation Settings")]
    public float startRadius = 1.5f;
    public float endRadius = 0.0f;
    public float duration = 1.0f;

    [Header("Debug Trigger")]
    public bool triggerAnimation = false; // 在Inspector勾选此项触发

    private float _timer = 0f;
    private bool _isAnimating = false;

    void Update()
    {
        // 处理动画逻辑
        HandleAnimation();

        // 实时同步给所有使用该全局变量的 Shader
        Shader.SetGlobalVector("_Position", transform.position);
        Shader.SetGlobalFloat("_Radius", radius);

        // Inspector 调试触发器
        if (triggerAnimation)
        {
            triggerAnimation = false;
            PlayRadiusFade();
        }
    }

    /// <summary>
    /// 公共方法：启动半径递减动画
    /// </summary>
    [ContextMenu("Play Radius Fade")] // 可以在组件右上角三个点菜单里点击执行
    public void PlayRadiusFade()
    {
        radius = startRadius;
        _timer = 0f;
        _isAnimating = true;
    }

    private void HandleAnimation()
    {
        if (!_isAnimating) return;

        if (_timer < duration)
        {
            _timer += Time.deltaTime;
            // 使用 Lerp 进行平滑线性插值
            radius = Mathf.Lerp(startRadius, endRadius, _timer / duration);
        }
        else
        {
            radius = endRadius;
            _isAnimating = false;
        }
    }
}
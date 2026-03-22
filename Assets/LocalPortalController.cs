using UnityEngine;

[ExecuteInEditMode]
public class LocalPortalController : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("控制当前物体 Shader 中的半径")]
    public float radius = 0f;
    
    [Tooltip("洞的中心点（如果不指定，默认使用当前物体中心）")]
    public Transform interactor;

    [Header("Animation Settings")]
    public float duration = 1.0f; // 动画持续时间

    [Header("Debug Triggers")]
    [Tooltip("点击触发：半径 0 -> 3 (展开)")]
    public bool triggerExpand = false;
    [Tooltip("点击触发：半径 3 -> 0 (收缩)")]
    public bool triggerShrink = false;

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    // 内部动画变量
    private float _animTimer = 0f;
    private bool _isAnimating = false;
    private float _startValue;
    private float _targetValue;

    void OnEnable()
    {
        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        if (_renderer == null) return;

        // 1. 处理动画插值逻辑
        HandleAnimation();

        // 2. 更新材质属性
        _renderer.GetPropertyBlock(_propBlock);

        Vector3 pos = interactor != null ? interactor.position : transform.position;
        _propBlock.SetVector("_Position", pos);
        _propBlock.SetFloat("_Radius", radius);

        _renderer.SetPropertyBlock(_propBlock);

        // 3. 调试触发器检测
        if (triggerExpand)
        {
            triggerExpand = false;
            StartRadiusAnimation(0f, 3f, duration);
        }

        if (triggerShrink)
        {
            triggerShrink = false;
            StartRadiusAnimation(3f, 0f, duration);
        }
    }

    /// <summary>
    /// 通用动画启动方法
    /// </summary>
    /// <param name="from">起始半径</param>
    /// <param name="to">目标半径</param>
    /// <param name="time">持续时间</param>
    public void StartRadiusAnimation(float from, float to, float time)
    {
        _startValue = from;
        _targetValue = to;
        radius = from;
        duration = time;
        _animTimer = 0f;
        _isAnimating = true;
    }

    private void HandleAnimation()
    {
        if (!_isAnimating) return;

        if (_animTimer < duration)
        {
            _animTimer += Time.deltaTime;
            // 计算归一化的进度 (0 到 1)
            float progress = _animTimer / duration;
            // 使用 Lerp 进行平滑过渡
            radius = Mathf.Lerp(_startValue, _targetValue, progress);
        }
        else
        {
            radius = _targetValue;
            _isAnimating = false;
        }
    }

    // 提供两个简单的公共接口方便外部调用
    public void ExpandPortal() => StartRadiusAnimation(0f, 3f, duration);
    public void ShrinkPortal() => StartRadiusAnimation(3f, 0f, duration);
}
using Godot;
using MyProject;

/// <summary>
/// 浮动提示（向上飘 + 渐隐）。
///
/// 用法：UIManager.Instance.ShowFloatTips(id, text)
///   id > 10000：纯文字横条，文字 = effectType 的 type 字段 + text
///   id < 10000：左图右文，左图为 itemDic[id].Icon 对应图片，右文为 text
///
/// 每次调用 UIManager.ShowFloatTips 都会新建一个实例，
/// 所以多条提示可以同时存在、各自独立播放动画，互不影响。
/// 连续调用（比如在循环里）时，UIManager 会自动让
/// 后一条比前一条晚 0.3 秒出现，避免都挤在同一时刻。
/// </summary>
public partial class FloatTips : Control
{
    // ── 外部绑定（在 .tscn 里通过 node_paths 连好）──
    [Export] public ColorRect bar;        // 横条本身（350x85 的黑色背景），动画作用在它上面
    [Export] public TextureRect icon;     // 左侧物品图标（id<10000 时显示）
    [Export] public Label tipsLabel;      // 横条文字

    // ── 动画参数（想调整效果改这里就行）──
    private const float Duration = 2.0f;        // 动画总时长（秒）：移动 + 渐隐同时进行
    private const float MoveUpDistance = 450f;  // 向上移动的距离（像素）

    // 图标布局常量（和场景里 Icon 节点的 offset 保持一致）
    private const float IconLeft = 10f;    // 图标距横条左边的边距
    private const float IconSize = 65f;    // 图标尺寸（正方形 65x65）
    private const float TextLeftGap = 10f; // 图标与文字之间的间距
    private const float TextSidePadding = 15f; // 纯文字模式下文字左右的留白

    private Tween _tween;

    public override void _Ready()
    {
        // 默认完全可见：出现即满透明度，随后一边上移一边渐隐
        Modulate = new Color(1, 1, 1, 1);

        // ── 关键：让鼠标点击"穿透"整条提示 ──
        // 根节点是全屏锚点的 Control，MouseFilter 默认为 Stop，
        // 会挡住屏幕上所有按钮的点击。这里把自身和所有子 Control
        // 都设为 Ignore（完全忽略鼠标），点击就能落到下面的按钮上了。
        // 用递归写法的好处：以后往场景里加新子节点也不用再手动设置。
        SetMouseFilterRecursive(this);
    }

    // 递归把 target 及其所有 Control 子节点的鼠标过滤模式设为 Ignore
    private void SetMouseFilterRecursive(Control target)
    {
        target.MouseFilter = MouseFilterEnum.Ignore;
        foreach (Node child in target.GetChildren())
        {
            if (child is Control controlChild)
            {
                SetMouseFilterRecursive(controlChild);
            }
        }
    }

    // ─────────────────────────────────────────────────────────
    //  对外入口：设置内容 + 定位 + 播放动画
    //  delay：延迟多少秒才出现（连续调用时用于错开显示），0 = 立即出现
    // ─────────────────────────────────────────────────────────
    public void ShowTips(int id, string text, float delay = 0f)
    {
        SetupContent(id, text);
        SetupPosition();
        PlayAnimation(delay);
    }

    // ─────────────────────────────────────────────────────────
    //  根据 id 设置横条内容（纯文字 / 左图右文）
    // ─────────────────────────────────────────────────────────
    private void SetupContent(int id, string text)
    {
        if (id>100000) 
        {
            switch (id)
            {
                case 100001:
                    icon.Visible = true;
                    icon.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/noiseIcon.png");
                    break;
                case 100002:
                    icon.Visible = true;
                    icon.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/mapIcon.png");
                    break;
                default:
                    break;
            }
            tipsLabel.OffsetLeft = IconLeft + IconSize + TextLeftGap; // = 85
            tipsLabel.OffsetRight = -TextSidePadding;
            tipsLabel.Text = text;
        }
        else if (id > 10000 && id<100000)
        {
            // ── 纯文字模式 ──
            // 隐藏图标，文字铺满整条（左右各留 15px），居中显示
            icon.Visible = false;
            tipsLabel.OffsetLeft = TextSidePadding;
            tipsLabel.OffsetRight = -TextSidePadding;

            // 读取 effectType 配置里的 type 字段，与传入文字拼接
            // 例如 type="生命值"、text="+10" → 显示 "生命值+10"
            if (ConfigManager.Instance.effectTypeDic.ContainsKey(id))
            {
                string typeText = ConfigManager.Instance.effectTypeDic[id].Type;
                tipsLabel.Text = typeText + text;
            }
            else
            {
                GD.PrintErr("[FloatTips] effectTypeDic 里找不到 ID：" + id);
                tipsLabel.Text = text;
            }
        }
        else if( id<10000 ) 
        {
            // ── 左图右文模式 ──
            // 显示图标，并加载物品图标：res://Assets/Images/Items/{Icon}.png
            icon.Visible = true;
            if (ConfigManager.Instance.itemDic.ContainsKey(id))
            {
                string iconPath = "res://Assets/Images/Items/"
                                  + ConfigManager.Instance.itemDic[id].Icon + ".png";
                icon.Texture = ResourceLoader.Load<Texture2D>(iconPath);
            }
            else
            {
                GD.PrintErr("[FloatTips] itemDic 里找不到 ID：" + id);
            }

            // 文字从图标右边开始：左偏移 = 图标左边距 + 图标宽 + 间距
            tipsLabel.OffsetLeft = IconLeft + IconSize + TextLeftGap; // = 85
            tipsLabel.OffsetRight = -TextSidePadding;
            tipsLabel.Text = text;
        }
    }

    // ─────────────────────────────────────────────────────────
    //  把横条放到屏幕水平居中、垂直 1/3 处（横条中心对齐 1/3 线）
    // ─────────────────────────────────────────────────────────
    private void SetupPosition()
    {
        Vector2 screenSize = GetViewport().GetVisibleRect().Size;
        // 横条尺寸在场景里定为 350x85，这里读实际尺寸以便兼容后续修改
        Vector2 barSize = bar.Size;
        if (barSize == Vector2.Zero)
        {
            barSize = new Vector2(350, 85);
        }

        float startX = (screenSize.X - barSize.X) / 2f;   // 水平居中
        float startY = screenSize.Y / 3f - barSize.Y / 2f; // 垂直 1/3 处（中心对齐）
        bar.Position = new Vector2(startX, startY);
    }

    // ─────────────────────────────────────────────────────────
    //  播放动画：向上移动 + 渐隐（同时进行），结束后自我销毁
    //  delay > 0 时：延迟期间横条先隐藏，到时间再显示并开始播放
    // ─────────────────────────────────────────────────────────
    private void PlayAnimation(float delay)
    {
        // 理论上每次都是新实例，这里以防万一先杀掉旧动画
        _tween?.Kill();
        _tween = CreateTween();

        // ── 延迟阶段（连续调用错开出现时用到）──
        if (delay > 0f)
        {
            // 延迟期间先隐藏横条，等时间到了再显示出来
            bar.Visible = false;
            _tween.TweenInterval(delay);
            _tween.TweenCallback(Callable.From(() => bar.Visible = true));
        }

        // 并行模式：下面两个 TweenProperty 同时执行
        _tween.SetParallel(true);
        // 1) 向上移动（保持匀速）
        _tween.TweenProperty(bar, "position:y", bar.Position.Y - MoveUpDistance, Duration);
        // 2) 同时渐隐：先慢后快（Cubic 缓入曲线）
        //    alpha 随时间的变化是 1 - t³（t 为 0~1 的进度）：
        //    第 1 秒结束(t=0.5)时 alpha ≈ 87.5%，满足"前 1 秒不低于 80%"；
        //    第 2 秒从 87.5% 一路快速掉到 0。
        //    想要前段更"坚挺"可把 Cubic 改成 Quart（1 - t⁴，1 秒处约 94%）。
        _tween.TweenProperty(bar, "modulate:a", 0f, Duration)
              .SetTrans(Tween.TransitionType.Cubic)
              .SetEase(Tween.EaseType.In);

        // 结束并行块，动画播完后销毁自己
        _tween.Chain();
        _tween.TweenCallback(Callable.From(QueueFree));
    }
}

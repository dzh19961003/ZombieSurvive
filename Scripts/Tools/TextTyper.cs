using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// 逐字打印文本的静态工具类。
/// 打印过程中，点击鼠标左键或按空格键会立即展示全部文本。
///
/// 使用示例：
///   // 每秒3个字（默认速度）
///   TextTyper.TypeText(myLabel, "这是要逐字打印的文本");
///   // 自定义速度，每秒5个字
///   TextTyper.TypeText(myLabel, "这是要逐字打印的文本", 5);
/// </summary>
public static class TextTyper
{
    public static event Action OnTypeEnd;
    // ========== 默认速度 ==========
    // 每秒打印3个字
    private const int DEFAULT_CHARS_PER_SECOND = 36;

    // ========== 公开方法 ==========

    /// <summary>
    /// 逐字打印文本（默认速度：每秒3个字）。
    /// 调用后立即返回，文本在后台逐字显示。
    /// 打印过程中点击鼠标左键或按空格键可立即展示全部文本。
    /// </summary>
    /// <param name="label">要显示文本的 Label 控件</param>
    /// <param name="fullText">要打印的完整文本</param>
    public static async void TypeText(Label label, string fullText)
    {
        // 内部调用带速度参数的重载
        await TypeTextAsync(label, fullText, DEFAULT_CHARS_PER_SECOND);
    }

    /// <summary>
    /// 逐字打印文本（自定义速度）。
    /// 调用后立即返回，文本在后台逐字显示。
    /// 打印过程中点击鼠标左键或按空格键可立即展示全部文本。
    /// </summary>
    /// <param name="label">要显示文本的 Label 控件</param>
    /// <param name="fullText">要打印的完整文本</param>
    /// <param name="charsPerSecond">每秒打印的字数，值越大越快，最小为1</param>
    public static async void TypeText(Label label, string fullText, int charsPerSecond)
    {
        await TypeTextAsync(label, fullText, charsPerSecond);
    }

    // ========== 段落渐出打印 ==========

    // 段落渐出的固定淡入时间（秒）
    private const float PARAGRAPH_FADE_SECONDS = 1.6f;
    // 默认"阅读速度"：每秒多少字，用来算每段停留多久
    private const int PARAGRAPH_DEFAULT_READ_SPEED = 30;
    // 段首统一补两个中文字符（全角空格）的缩进
    private const string PARAGRAPH_INDENT = "　　";

    /// <summary>
    /// 按段落渐出打印。把 fullText 按换行切成若干段，
    /// 每段做成一个 Label 放进 container（VBoxContainer），逐段淡入：
    ///   1. 当前段 0.3s 从透明渐显到不透明
    ///   2. 淡入完后，按"该段字数 ÷ readSpeedPerSecond"停留一段时间
    ///   3. 再淡入下一段，直到全部完成
    /// 每段段首会统一加上两个中文字符（全角空格）的缩进。
    /// 点击鼠标左键或按空格键可立即显示全部段落。
    /// </summary>
    /// <param name="container">承载各段 Label 的 VBoxContainer（每段自动 new 一个 Label 加进去）</param>
    /// <param name="fullText">完整文本，按换行（\n 或 \r\n）分段，空行忽略</param>
    /// <param name="readSpeedPerSecond">阅读速度（每秒多少字），越小停留越久，默认 30</param>
    public static void TypeParagraphs(Node container, string fullText, int readSpeedPerSecond = PARAGRAPH_DEFAULT_READ_SPEED)
    {
        TypeParagraphsAsync(container, fullText, readSpeedPerSecond <= 0 ? 1 : readSpeedPerSecond);
    }

    private static async void TypeParagraphsAsync(Node container, string fullText, int readSpeedPerSecond)
    {
        if (container == null) return;

        // ===== 按非空行切段 =====
        List<string> paras = SplitParagraphs(fullText);

        // 每段先建好 Label，初始透明（Modulate.A=0），放进 container
        // 段首统一加两个中文字符（全角空格）缩进
        var labels = new List<Label>();
        foreach (var text in paras)
        {
            var lb = new Label { Text = PARAGRAPH_INDENT + text, AutowrapMode = TextServer.AutowrapMode.WordSmart };
            lb.AddThemeColorOverride("font_color", new Color(0.2f, 0.16f, 0.1f, 1));
            lb.AddThemeFontSizeOverride("font_size", 48);
            lb.AddThemeConstantOverride("line_spacing", 16);
            lb.AddThemeConstantOverride("paragraph_spacing", 24);
            lb.Modulate = new Color(1, 1, 1, 0); // 先透明
            container.AddChild(lb);
            labels.Add(lb);
        }

        // 逐段渐出
        bool skipped = false;
        for (int i = 0; i < labels.Count; i++)
        {
            Label lb = labels[i];

            // 跳过输入：把剩余所有段一次性显示并结束
            if (CheckSkipInput())
            {
                skipped = true;
            }

            if (skipped)
            {
                // 当前段直接显示
                lb.Modulate = new Color(1, 1, 1, 1);
                continue;
            }

            // ===== 淡入当前段（0.3s）=====
            // 0.3s / 16ms ≈ 19 步，每步 alpha 增量 0.05，19*0.05≈0.95，最后一帧补满
            for (float a = 0f; a < 1f; a += 0.05f)
            {
                lb.Modulate = new Color(1, 1, 1, Mathf.Min(a, 1f));
                // 淡入期间也响应跳过
                if (CheckSkipInput())
                {
                    lb.Modulate = new Color(1, 1, 1, 1);
                    skipped = true;
                    break;
                }
                await Task.Delay(16);
            }

            if (skipped)
            {
                continue;
            }

            // ===== 停留：字数 ÷ 阅读速度（秒）=====
            int len = paras[i].Length;
            float staySeconds = len / (float)readSpeedPerSecond;
            int stayMs = Mathf.Max(0, (int)Mathf.Round(staySeconds * 1000f));

            int waited = 0;
            while (waited < stayMs)
            {
                if (CheckSkipInput())
                {
                    skipped = true;
                    break;
                }
                await Task.Delay(16);
                waited += 16;
            }
        }

        // 无论正常结束还是跳过，保证所有段可见
        foreach (var lb in labels)
        {
            lb.Modulate = new Color(1, 1, 1, 1);
        }

        OnTypeEnd?.Invoke();
    }

    /// <summary>按换行切段，忽略空行。</summary>
    private static List<string> SplitParagraphs(string fullText)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(fullText)) return result;

        // 统一换行符后按行切
        string[] lines = fullText.Replace("\r\n", "\n").Split('\n');
        foreach (var raw in lines)
        {
            // 去掉首尾空白（diary 段落开头带缩进空格）
            string t = raw.Trim();
            if (t.Length > 0)
            {
                result.Add(t);
            }
        }
        return result;
    }

    // ========== 点按续写（日记专用） ==========

    // 每个"点按续写"任务的世代号。新任务开始时 ++，旧任务发现自己过期就静默退出，
    // 避免面板关闭/重开后旧协程继续访问已释放的节点（ObjectDisposedException）。
    private static int _clickRunId = 0;

    /// <summary>
    /// 作废当前正在跑的点按续写任务（比如日记面板关闭时调用）。
    /// 旧任务下一帧就会静默退出，不再访问任何节点。
    /// </summary>
    public static void CancelClickTyping()
    {
        _clickRunId++;
    }

    /// <summary>
    /// 日记专用：第一段自动出现，第二段起要玩家点屏幕任意处才出下一段。
    /// 每段 0.3s 淡入；全部出完触发 OnTypeEnd。
    /// hintLabel（"点击续写日记"）会被紧跟在当前最后一段下方居中，
    /// 跟随段落往下移；最后一段出完就隐藏。
    /// 同一时间只跑一个点按续写任务，后启动的会作废前面的。
    /// </summary>
    /// <param name="container">承载各段 Label 的 VBoxContainer（每段自动 new 一个 Label）</param>
    /// <param name="fullText">完整文本，按换行分段，空行忽略，段首加两个全角空格缩进</param>
    /// <param name="hintLabel">跟随最后一段下方显示的"点击续写"提示 Label（可为 null）</param>
    public static void TypeParagraphsByClick(Node container, string fullText, Label hintLabel = null)
    {
        int runId = ++_clickRunId;
        TypeParagraphsByClickAsync(container, fullText, hintLabel, runId);
    }

    private static async void TypeParagraphsByClickAsync(Node container, string fullText, Label hintLabel, int runId)
    {
        if (container == null) return;

        List<string> paras = SplitParagraphs(fullText);
        if (paras.Count == 0)
        {
            OnTypeEnd?.Invoke();
            return;
        }

        const int paragraphSpacing = 24;
        // 段落淡入时长（秒），需要更快/更慢直接改这个值
        const float paragraphFadeSeconds = 0.6f;

        // 关键：段落 Label 是"点一下才 AddChild 下一段"，不是一次全建。
        // 这样 VBoxContainer 只撑到"已出的段"，hint 由调用方（DiaryUI._Process）
        // 每帧跟随到最后一段下方，视觉上才能"提示跟着段落走"。

        // 第一段自动出（直接 AddChild + 淡入）
        Label firstLb = MakeParagraphLabel(paras[0], paragraphSpacing);
        container.AddChild(firstLb);

        // 第一段淡入
        await FadeIn(firstLb, runId, paragraphFadeSeconds);
        if (runId != _clickRunId) return;

        // 段完全出现后，hint 才显示（位置由调用方 DiaryUI._Process 每帧跟随）
        // 只有"还有下一段"才显示提示，最后一段不出
        if (hintLabel != null && IsValidNode(hintLabel))
        {
            hintLabel.Visible = paras.Count > 1;
        }

        for (int i = 1; i < paras.Count; i++)
        {
            if (runId != _clickRunId) return;
            if (!IsValidNode(container)) return;

            // 等玩家点屏幕才出这一段
            await WaitOneClick(runId);
            if (runId != _clickRunId) return;
            if (!IsValidNode(container)) return;

            // 点击后立即收起提示（等下一段完全出现后再出现）
            if (hintLabel != null && IsValidNode(hintLabel))
            {
                hintLabel.Visible = false;
            }

            // 现在才创建并 AddChild 这一段
            Label lb = MakeParagraphLabel(paras[i], paragraphSpacing);
            container.AddChild(lb);

            // 淡入
            await FadeIn(lb, runId, paragraphFadeSeconds);
            if (runId != _clickRunId) return;

            // 段落完全出现后，若还有下一段，再让提示出现
            if (hintLabel != null && IsValidNode(hintLabel))
            {
                hintLabel.Visible = i < paras.Count - 1;
            }
        }

        if (runId != _clickRunId) return;
        OnTypeEnd?.Invoke();
    }

    /// <summary>创建一段的 Label（字号48、段首两全角空格、段间24px 间距）。</summary>
    private static Label MakeParagraphLabel(string text, int paragraphSpacing)
    {
        var lb = new Label { Text = PARAGRAPH_INDENT + text, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        lb.AddThemeColorOverride("font_color", new Color(0.2f, 0.16f, 0.1f, 1));
        lb.AddThemeFontSizeOverride("font_size", 48);
        lb.AddThemeConstantOverride("line_spacing", 16);
        lb.AddThemeConstantOverride("paragraph_spacing", paragraphSpacing);
        lb.Modulate = new Color(1, 1, 1, 0);
        return lb;
    }

    /// <summary>按指定秒数淡入（每帧 16ms 步进）。支持世代号取消。</summary>
    private static async Task FadeIn(Label lb, int runId, float seconds)
    {
        if (lb == null) return;
        if (seconds <= 0f) seconds = 0.1f; // 兜底最小 0.1s，避免除零
        // 16ms/帧，alpha 增量 = 16ms / seconds
        float step = 0.016f / seconds;
        for (float a = 0f; a < 1f; a += step)
        {
            if (runId != _clickRunId || !IsValidNode(lb)) return;
            lb.Modulate = new Color(1, 1, 1, Mathf.Min(a, 1f));
            await Task.Delay(16);
        }
        if (IsValidNode(lb)) lb.Modulate = new Color(1, 1, 1, 1);
    }

    /// <summary>节点是否仍然有效（没被释放、且在场景树里）。</summary>
    private static bool IsValidNode(Node node)
    {
        return node != null && GodotObject.IsInstanceValid(node) && node.IsInsideTree();
    }

    /// <summary>等一次"鼠标左键按下"（按下沿检测，不会因按住不动反复触发）。
    /// 如果 runId 已过期（被新任务/面板关闭作废），会立即返回。</summary>
    private static async Task WaitOneClick(int runId)
    {
        // 先吃掉可能残留的"按下"状态，等"松手"后再等"再次按下"，
        // 避免连续两段共用同一次点击
        while (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (runId != _clickRunId) return;
            await Task.Delay(16);
        }
        while (!Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (runId != _clickRunId) return;
            await Task.Delay(16);
        }
    }

    // ========== 核心逻辑 ==========

    /// <summary>
    /// 异步逐字打印文本的核心逻辑。
    /// 每帧检查是否有跳过输入（鼠标左键 / 空格键），有则立即展示全部文本。
    /// </summary>
    /// <param name="label">要显示文本的 Label 控件</param>
    /// <param name="fullText">要打印的完整文本</param>
    /// <param name="charsPerSecond">每秒打印的字数</param>
    private static async Task TypeTextAsync(Label label, string fullText, int charsPerSecond)
    {
        // ===== 空文本保护 =====
        if (string.IsNullOrEmpty(fullText))
        {
            label.Text = "";
            return;
        }

        // ===== 速度保护：最小速度为1 =====
        if (charsPerSecond <= 0)
        {
            charsPerSecond = 1;
        }

        // 计算每个字的间隔时间（毫秒）
        int intervalMs = 1000 / charsPerSecond;
        int totalChars = fullText.Length;

        // ===== 逐字打印 =====
        // 从第1个字开始，到最后一个字结束
        for (int i = 1; i <= totalChars; i++)
        {
            // 更新 Label 显示当前已打印的部分
            label.Text = fullText.Substring(0, i);

            // 等待一个字的间隔时间
            // 将等待拆分为每帧检查一次（约16ms），以便及时响应跳过输入
            int waitedMs = 0;
            while (waitedMs < intervalMs)
            {
                // 每帧检查一次是否有跳过输入
                if (CheckSkipInput())
                {
                    // 有跳过输入 → 立即显示全部文本并结束
                    label.Text = fullText;
                    OnTypeEnd?.Invoke();
                    return;
                }

                // 等待一帧（约16ms）
                await Task.Delay(16);
                waitedMs += 16;
            }
        }

        // ===== 打印完毕，确保显示完整文本 =====
        label.Text = fullText;
        OnTypeEnd?.Invoke();
    }

    // ========== 输入检测 ==========

    /// <summary>
    /// 检查用户是否触发了"跳过打印"的操作。
    /// 条件：按下鼠标左键 或 按下空格键。
    /// </summary>
    /// <returns>true 表示应该跳过打印，直接显示全部文本</returns>
    private static bool CheckSkipInput()
    {
        // 检测鼠标左键是否被按下
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            return true;
        }

        // 检测空格键是否被按下
        if (Input.IsKeyPressed(Key.Space))
        {
            return true;
        }

        return false;
    }
}

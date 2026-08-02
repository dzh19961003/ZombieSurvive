using System;
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
    // ========== 默认速度 ==========
    // 每秒打印3个字
    private const int DEFAULT_CHARS_PER_SECOND = 24;

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
                    return;
                }

                // 等待一帧（约16ms）
                await Task.Delay(16);
                waitedMs += 16;
            }
        }

        // ===== 打印完毕，确保显示完整文本 =====
        label.Text = fullText;
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

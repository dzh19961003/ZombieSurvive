using Godot;
using MyProject;

/// <summary>
/// 日记面板：用纯色图临时替代笔记本贴图，顶部显示"X日"，正文逐字打印。
/// 由 StartMenu.OpenDiary() 创建并调用 Show()。
/// </summary>
public partial class DiaryUI : Control
{
    private Label dateLabel;
    private Label contentLabel;
    private Button closeBtn;

    public override void _Ready()
    {
        // 显式取节点，不用 Export，避免实例化时解析不到导致 null
        dateLabel = GetNode<Label>("Panel/TitleRow/dateLabel");
        contentLabel = GetNode<Label>("Panel/ContentLabel");
        closeBtn = GetNode<Button>("Panel/CloseBtn");

        closeBtn.Pressed += OnClosePressed;
    }

    /// <summary>
    /// 显示日记。date 为 1~22，text 为要逐字打印的正文（空字符串则只显示日期）。
    /// </summary>
    public void Show(int date, string text)
    {
        // 面板在场景里默认 visible=false，显示时才打开
        Visible = true;
        dateLabel.Text = date + "日";
        contentLabel.Text = "";

        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        TextTyper.TypeText(contentLabel, text, 24);
    }

    private void OnClosePressed()
    {
        // 关闭时释放自身（用 CreateUI 创建的，不在 ShowUI 缓存里）
        UIManager.Instance.DeleteUI(this);
    }
}

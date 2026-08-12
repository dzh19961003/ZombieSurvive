using Godot;
using System;
using MyProject;

namespace MyProject
{
    public partial class MainUI : Control
    {
        [Export] public Button propertyBtn;
        [Export] public TextureButton sleepBtn;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            propertyBtn.Pressed += EnterPro;
            sleepBtn.Pressed += () =>
            {
                CommonTips tips = new CommonTips();
                if (PlayerManager.Instance.Hunger != 1)
                {
                    tips = UIManager.Instance.ShowCommonTips("上床休息", "是否确定上床休息并结束今天？");
                }
                else
                {
                    tips = UIManager.Instance.ShowCommonTips("上床休息", "你非常饥饿！现在睡觉可能就见不到明天的太阳了，是否确定睡觉？");
                }
                tips.OnConfirm = () =>
                    {
                        do
                        {
                            GameManager.Instance.AdvanceTime();
                        }
                        while (GameManager.Instance.CurrentTimePeriod != 0);

                    };
            };
        }

        private void EnterPro()
        {
            UIManager.Instance?.ShowUI(Paths.PropertyUI);
        }

    }
}

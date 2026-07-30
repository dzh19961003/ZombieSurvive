// ============================================================
//  Trait.cs — 天赋图标 UI 组件
//  功能：显示天赋图标，点击显示详细说明
// ============================================================

using Godot;
using MyProject;
using Godot.Collections;
using System;


namespace MyProject
{
    public partial class Trait : Control
    {
        // 天赋唯一 ID
        public int TalentID { get; set; }


        [Export] public TextureButton Bg;
        [Export] public Label TalentName;
        [Export] public TextureRect[] Rarity;

        public override void _EnterTree()
        {
            base._EnterTree();

            // 连接按钮点击信号
            if (Bg != null)
            {
                Bg.Pressed += OnTalentClick;
            }
        }

        public override void _ExitTree()
        {
            // 断开信号，防止内存泄漏
            if (Bg != null)
            {
                Bg.Pressed -= OnTalentClick;
            }

            base._ExitTree();
        }

        // ============================================================
        //  外部初始化接口
        // ============================================================

        /// <summary>
        /// 设置天赋 ID 并初始化显示
        /// </summary>
        public void Setup(int id)
        {
            this.TalentID = id;
            InitialState();
        }

        // ============================================================
        //  显示初始化
        // ============================================================

        /// <summary>
        /// 根据 TalentID 从配置表加载数据并显示
        /// </summary>
        public void InitialState()
        {
            // ID 为 0 表示空槽位，不显示
            if (TalentID == 0)
            {
                SetEmpty();
                return;
            }

            // 防御性检查：ConfigManager 是否就绪
            if (ConfigManager.Instance == null)
            {
                GD.PrintErr($"[Trait] ConfigManager 为 null！ID={TalentID}");
                SetEmpty();
                return;
            }

            // 防御性检查：配置表中是否存在该 ID
            if (!ConfigManager.Instance.talentDic.ContainsKey(TalentID))
            {
                GD.PrintErr($"[Trait] talentDic 中找不到 ID={TalentID}！");
                SetEmpty();
                return;
            }

            // 从配置表获取天赋数据
            Talent talent = ConfigManager.Instance.talentDic[TalentID];

            //读取稀有度，默认为隐藏所有稀有度图标
            for (int i = 0; i < Rarity.Length; i++)
            {
                Rarity[i].Visible = false;
            }
            Rarity[ConfigManager.Instance.talentDic[TalentID].Rarity - 1].Visible = true;

            // 设置显示文本
            if (TalentName != null)
            {
                TalentName.Text = talent.Name;
            }



            // 显示所有节点
            if (Bg != null) Bg.Visible = true;
            if (TalentName != null) TalentName.Visible = true;

            GD.Print($"[Trait] 初始化完成：ID={TalentID}, Name={talent.Name}");
        }

        // ============================================================
        //  设置为空槽位
        // ============================================================

        /// <summary>
        /// 隐藏所有显示节点（空槽位状态）
        /// </summary>
        private void SetEmpty()
        {
            if (Bg != null) Bg.Visible = false;
            if (TalentName != null) TalentName.Visible = false;
        }

        // ============================================================
        //  点击事件
        // ============================================================

        /// <summary>
        /// 点击天赋图标时显示详细说明
        /// </summary>
        private void OnTalentClick()
        {
            // 空槽位不响应
            if (TalentID == 0) return;

            // 防御性检查
            if (ConfigManager.Instance == null) return;
            if (!ConfigManager.Instance.talentDic.ContainsKey(TalentID)) return;
            
            // 获取天赋数据并显示提示
            Talent talent = ConfigManager.Instance.talentDic[TalentID];

            if (UIManager.Instance != null)
            {
                
                DetailsTalent talentTips = (DetailsTalent)UIManager.Instance.ShowUI("res://UI/DetailsTag/DetailsTalent.tscn");
                talentTips.InitialTips(TalentID);
                UIManager.Instance.SetSpwanPosition(this, talentTips);


            }
        }
    }
}
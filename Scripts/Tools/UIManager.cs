using Godot;
using MyProject;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

public partial class UIManager : Node
{
    // 全局单例
    public static UIManager Instance { get; private set; }

    // 记录已经加载过的面板，避免重复创建
    // key = 面板场景的路径，value = 面板节点本身
    private Dictionary<string, Control> _panels = new();

    public override void _Ready()
    {
        Instance = this;
        ShowUI(Paths.MainUI);
    }

    // ─────────────────────────────────────────────────────────
    //  ShowPanel：打开一个面板
    //  第一次调用时会加载场景并创建节点；
    //  之后再调用同一路径，直接显示已有节点（不重复创建）。
    // ─────────────────────────────────────────────────────────
    public Control ShowUI(string scenePath)
    {
        // 如果这个面板从来没打开过，就创建它
        if (!_panels.ContainsKey(scenePath))
        {
            var scene = GD.Load<PackedScene>(scenePath);
            if (scene == null)
            {
                GD.PrintErr("找不到这个面板场景：" + scenePath);
                return null;
            }
            var panel = scene.Instantiate<Control>();
            AddChild(panel);
            // 加到 UIManager 节点下面
            _panels[scenePath] = panel;
        }

        // 把面板显示出来
        _panels[scenePath].Visible = true;
        this.MoveChild(_panels[scenePath], this.GetChildCount() - 1);
        return _panels[scenePath];

    }

    // ─────────────────────────────────────────────────────────
    //  HidePanel：隐藏一个面板（节点还在，只是看不见）
    //
    //  用法示例：
    //    UIManager.Instance.HidePanel(UIPaths.Shop);
    // ─────────────────────────────────────────────────────────
    public void HideUI(string scenePath)
    {
        if (_panels.ContainsKey(scenePath))
        {
            _panels[scenePath].Visible = false;
        }
    }

    // ─────────────────────────────────────────────────────────
    //  CreateUI：每次创建全新实例（不走缓存），
    //  适用于需要每次打开都是全新状态的界面（如 ExploreUI）。
    //  关闭时用 DeleteUI 销毁。
    // ─────────────────────────────────────────────────────────
    public Control CreateUI(string scenePath)
    {
        var scene = GD.Load<PackedScene>(scenePath);
        if (scene == null)
        {
            GD.PrintErr("[UIManager] 找不到场景：" + scenePath);
            return null;
        }
        var panel = scene.Instantiate<Control>();
        AddChild(panel);
        return panel;
    }

    // ─────────────────────────────────────────────────────────
    //  DeleteUI：销毁一个 UI 面板节点，同时清理 ShowUI 缓存
    //  （防止缓存里留下已释放节点的悬空引用）。
    //
    //  重载 1：传节点引用
    //    UIManager.Instance.DeleteUI(exploreUI);
    //  重载 2：传场景路径
    //    UIManager.Instance.DeleteUI("res://UI/Explore/ExploreUI.tscn");
    // ─────────────────────────────────────────────────────────
    public void DeleteUI(Control panel)
    {
        if (panel == null) return;

        // 从 ShowUI 缓存中移除（如果有）
        string keyToRemove = null;
        foreach (var kv in _panels)
        {
            if (kv.Value == panel)
            {
                keyToRemove = kv.Key;
                break;
            }
        }
        if (keyToRemove != null)
        {
            _panels.Remove(keyToRemove);
        }

        panel.QueueFree();
    }

    public void DeleteUI(string scenePath)
    {
        if (_panels.TryGetValue(scenePath, out var panel))
        {
            _panels.Remove(scenePath);
            panel.QueueFree();
        }
    }

    // ─────────────────────────────────────────────────────────
    //  HideAll：关闭所有面板（切换场景前可以调用）
    // ─────────────────────────────────────────────────────────
    public void HideAll()
    {
        foreach (var panel in _panels.Values)
        {
            panel.Visible = false;
        }
    }

    //输入物品ID，返回对应稀有度的物品类型图标
    public Texture2D SetItemRarityType(int ID)
    {
        Texture2D texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/material_1.png");

        switch (ConfigManager.Instance.itemDic[ID].Type)
        {
            case 1:
                switch (ConfigManager.Instance.itemDic[ID].Rarity)
                {
                    case 1:
                        texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/food_1.png");
                        break;
                    case 2:
                        texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/food_2.png");
                        break;
                    case 3:
                        texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/food_3.png");
                        break;
                    case 4:
                        texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/food_4.png");
                        break;
                    default:
                        break;
                }
                break;
            case 2:
                switch (ConfigManager.Instance.itemDic[ID].Rarity)
                {
                    case 1:
                        texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/medic_1.png");
                        break;
                    case 2:
                        texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/medic_2.png");
                        break;
                    case 3:
                        texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/medic_3.png");
                        break;
                    case 4:
                        texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/medic_4.png");
                        break;
                    default:
                        break;
                }
                break;
            case 3:
                int type = ConfigManager.Instance.equipDic[ConfigManager.Instance.itemDic[ID].EquipID].Type;

                if (type == 1)
                {
                    switch (ConfigManager.Instance.itemDic[ID].Rarity)
                    {
                        case 1:
                            texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/weapon_1.png");
                            break;
                        case 2:
                            texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/weapon_2.png");
                            break;
                        case 3:
                            texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/weapon_3.png");
                            break;
                        case 4:
                            texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/weapon_4.png");
                            break;
                        default:
                            break;
                    }
                }
                else if (type == 2 || type == 3)
                {
                    switch (ConfigManager.Instance.itemDic[ID].Rarity)
                    {
                        case 1:
                            texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/armer_1.png");
                            break;
                        case 2:
                            texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/armer_2.png");
                            break;
                        case 3:
                            texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/armer_3.png");
                            break;
                        case 4:
                            texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/armer_4.png");
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (ConfigManager.Instance.itemDic[ID].Rarity)
                    {
                        case 1:
                            texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/treasure_1.png");
                            break;
                        case 2:
                            texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/treasure_2.png");
                            break;
                        case 3:
                            texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/treasure_3.png");
                            break;
                        case 4:
                            texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/treasure_4.png");
                            break;
                        default:
                            break;
                    }
                }
                break;
            case 4:
                switch (ConfigManager.Instance.itemDic[ID].Rarity)
                {
                    case 1:
                        texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/material_1.png");
                        break;
                    case 2:
                        texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/material_2.png");
                        break;
                    case 3:
                        texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/material_3.png");
                        break;
                    case 4:
                        texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/ItemType/material_4.png");
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        return texture;

    }

    //创建一个通用提示弹窗
    public CommonTips ShowCommonTips(string title, string tips)
    {
        CommonTips commonTips = (CommonTips)ShowUI("res://UI/CommonTips.tscn");
        commonTips.SpawnTips(title, tips);
        return commonTips;
    }

    //自动设置标签位置
    //前面的Control参数一般就是脚本挂在的地方的Control，一般在调用时直接写"this"即可
    //后面的Control参数是标签，需要生成后在脚本里获取到标签并传入

    /*示例代码：
     DetailsFood foodTips = (DetailsFood)UIManager.Instance.ShowUI("res://UI/DetailsTag/DetailsFood.tscn");
     foodTips.InitialTips(ID);
     UIManager.Instance.SetSpwanPosition(this, foodTips);
    */
    public void SetSpwanPosition(Control item, Control tag)
    {

        Vector2 itemGlobalPos = item.GlobalPosition;

        float itemRight = itemGlobalPos.X + item.Size.X;
        float itemTop = itemGlobalPos.Y;


        Vector2 controlSize = tag.Size;
        if (controlSize == Vector2.Zero)
        {
            controlSize = new Vector2(400, 500);
        }


        float controlX, controlY;

        // 屏幕宽度
        float screenWidth = GetViewport().GetVisibleRect().Size.X;

        // 右侧放得下
        if (itemRight + controlSize.X <= screenWidth)
        {
            controlX = itemRight;
        }
        //放不下
        else
        {
            controlX = itemGlobalPos.X - controlSize.X;
        }

        // 竖直方向：
        controlY = itemTop;
        if (controlY < 0)
        {
            controlY = 0;
        }
        if (controlY + controlSize.Y > GetViewport().GetVisibleRect().Size.Y)
        {
            controlY = GetViewport().GetVisibleRect().Size.Y - controlSize.Y;
        }

        // 4) 赋值
        tag.Position = new Vector2(controlX, controlY);
    }

    //通过id找到对应图标，大于10000的是属性，小于10000的是物品
    public Texture2D GetItemIcon(int ID) 
    {
        Texture2D texture2D = null;
        
        if (ID<10000)
        {
            texture2D = ResourceLoader.Load<Texture2D>("res://Assets/Images/Items/" + ConfigManager.Instance.itemDic[ID].Icon + ".png");
        }
        else
        {
            texture2D = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/"+ConfigManager.Instance.effectTypeDic[ID].ImageIcon);
        }
        return texture2D;
    }
}

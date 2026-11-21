using Godot;
using MyProject;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BattleScreen : Control
{
    public const string ScenePath = "res://UI/Battle/BattleMain.tscn";
    private const string ArtPath = "res://Assets/Images/UI/Battle/";
    private static readonly Vector2 DesignSize = new(1080, 1920);
    private static readonly BattlePart[] Parts = Enum.GetValues<BattlePart>();
    private static readonly string[] PartNames = { "头部", "身体", "手臂" };
    private static readonly string[] PartArt = { "head", "body", "arm" };
    private static readonly Color Gold = new("#ffd377");
    private static readonly Color Red = new("#ff6664");
    private static readonly Color Pale = new("#dce5ed");

    private enum Phase { Advancing, Player, Targeting, Enemy, Closed }

    private sealed record Limb(BattlePart Part, string DisplayName, double Hp, double MaxHp, Button Card);

    private readonly List<Limb> limbs = new();
    private readonly Label[] chanceLabels = new Label[3];
    private readonly Label[] chargeLabels = new Label[3];
    private readonly Label[] weightLabels = new Label[3];
    private readonly Label[] cooldownLabels = new Label[3];
    private readonly Texture2D[] partIcons = new Texture2D[3];
    private readonly Texture2D[] missingIcons = new Texture2D[3];
    private readonly double[] weights = new double[3];
    private readonly int[] baseWeights = new int[3];
    private Enemy enemy;
    private BattleSession session;
    private Action onExit;
    private PlayerManager player;
    private Control stage;
    private Control partContainer;
    private Control track;
    private Control playerMarker;
    private Control enemyMarker;
    private Button randomButton;
    private Button precisionButton;
    private Button speedTwo;
    private Button speedFour;
    private Button detailsToggle;
    private BagUI bag;
    private Tween targetingTween;
    private Phase phase;
    private int speed = 1;
    private bool details;
    private double playerProgress;
    private double enemyProgress;
    private double enemyDelay;

    public void Initialize(Enemy enemyData, BattleSession battleSession, Action returnToExplore)
    {
        enemy = enemyData;
        session = battleSession;
        onExit = returnToExplore;
    }

    public override void _Ready()
    {
        player = PlayerManager.Instance;
        bool preview = enemy == null;
        enemy ??= new Enemy
        {
            Name = "普通丧尸", Level = 3, Speed = 9,
            HeadNum = 1, BodyNum = 1, ArmNum = 2,
            HeadHP = new() { 30 }, BodyHP = new() { 50 }, ArmHP = new() { 18, 18 }
        };
        session ??= new BattleSession(player);

        stage = new Control { Name = "Stage", MouseFilter = MouseFilterEnum.Ignore, Size = DesignSize };
        var scenery = GetChildren();
        AddChild(stage);
        foreach (Node child in scenery)
        {
            if (child.Name != "Background" && child.Name != "BackgroundTint")
                child.Reparent(stage, false);
        }
        Resized += FitStage;
        FitStage();

        track = stage.GetNode<Control>("Timeline");
        playerMarker = stage.GetNode<Control>("PlayerMarker");
        enemyMarker = stage.GetNode<Control>("EnemyMarker");
        partContainer = stage.GetNode<Control>("EnemyParts");
        randomButton = stage.GetNode<Button>("RandomAttack");
        precisionButton = stage.GetNode<Button>("PrecisionAttack");
        speedTwo = stage.GetNode<Button>("Speed1");
        speedFour = stage.GetNode<Button>("Speed2");
        detailsToggle = stage.GetNode<Button>("DetailsToggle");
        randomButton.Pressed += RandomAttack;
        precisionButton.Pressed += ToggleTargeting;
        speedTwo.Pressed += () => SelectSpeed(2);
        speedFour.Pressed += () => SelectSpeed(4);
        detailsToggle.Pressed += ToggleDetails;
        stage.GetNode<TextureButton>("PlayerHUD/Backpack").Pressed += OpenBag;

        CreateReadouts();
        CreateDebugButton();
        ConfigureEnemyHeader();
        var cardScene = GD.Load<PackedScene>("res://UI/Battle/Components/BattleLimbCard.tscn");
        AddLimbs(cardScene, BattlePart.Head, enemy.HeadNum, enemy.HeadHP, false);
        AddLimbs(cardScene, BattlePart.Body, enemy.BodyNum, enemy.BodyHP, false);
        AddLimbs(cardScene, BattlePart.Arm, enemy.ArmNum, enemy.ArmHP, preview);
        if (player != null)
            player.GetItem += RefreshPlayerData;
        RefreshPlayerData();
        RefreshTurnControls();
        UpdateMarkers();
    }

    public override void _Process(double delta)
    {
        if (bag != null || phase is Phase.Player or Phase.Targeting or Phase.Closed)
            return;

        double elapsed = delta * speed;
        if (phase == Phase.Enemy)
        {
            enemyDelay -= elapsed;
            if (enemyDelay <= 0)
            {
                GD.Print("攻击：敌人");
                enemyProgress = 0;
                phase = Phase.Advancing;
                UpdateMarkers();
            }
            return;
        }

        double playerSpeed = Math.Max(1, player?.Agility ?? 10);
        double enemySpeed = Math.Max(1, enemy.Speed);
        double fastest = Math.Max(playerSpeed, enemySpeed);
        double playerRate = playerSpeed / fastest / 3.2;
        double enemyRate = enemySpeed / fastest / 3.2;
        double playerSeconds = (1 - playerProgress) / playerRate;
        double enemySeconds = (1 - enemyProgress) / enemyRate;
        double step = Math.Min(elapsed, Math.Min(playerSeconds, enemySeconds));
        playerProgress = Math.Clamp(playerProgress + step * playerRate, 0, 1);
        enemyProgress = Math.Clamp(enemyProgress + step * enemyRate, 0, 1);
        if (step >= playerSeconds && playerSeconds <= enemySeconds)
        {
            playerProgress = 1;
            phase = Phase.Player;
            session.PlayerTurnStarted(player);
            RefreshTurnControls();
        }
        else if (step >= enemySeconds)
        {
            enemyProgress = 1;
            phase = Phase.Enemy;
            enemyDelay = 0.28;
        }
        UpdateMarkers();
    }

    public override void _ExitTree()
    {
        if (player != null)
            player.GetItem -= RefreshPlayerData;
    }

    private void FitStage()
    {
        float fit = Math.Min(Size.X / DesignSize.X, Size.Y / DesignSize.Y);
        if (fit <= 0)
            return;
        stage.Scale = Vector2.One * fit;
        stage.Position = (Size - DesignSize * fit) / 2;
    }

    private void CreateReadouts()
    {
        for (int index = 0; index < Parts.Length; index++)
        {
            int number = index + 1;
            chanceLabels[index] = stage.GetNode<Label>($"ChanceValue{number}");
            chargeLabels[index] = stage.GetNode<Label>($"UsesValue{number}");
            weightLabels[index] = CreateDetailLabel(chanceLabels[index]);
            cooldownLabels[index] = CreateDetailLabel(chargeLabels[index]);
            partIcons[index] = GD.Load<Texture2D>($"{ArtPath}battle_main_part_{PartArt[index]}_icon.tres");
            missingIcons[index] = GD.Load<Texture2D>($"{ArtPath}battle_main_part_{PartArt[index]}_missing_icon.tres");
        }
        foreach (var button in new[] { randomButton, precisionButton })
            button.GetNode<Label>("DamageValue").Text = "—";
    }

    private Label CreateDetailLabel(Label above)
    {
        var label = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore, Position = above.Position + new Vector2(-12, 40),
            Size = new Vector2(above.Size.X + 24, 26), Visible = false
        };
        label.AddThemeFontSizeOverride("font_size", 23);
        stage.AddChild(label);
        return label;
    }

    private void CreateDebugButton()
    {
        if (!OS.IsDebugBuild())
            return;

        var button = new Button
        {
            Name = "DebugExit",
            Text = "调试退出",
            Position = new Vector2(27, 29),
            Size = new Vector2(151, 48),
            FocusMode = FocusModeEnum.None,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };
        var white = new StyleBoxFlat { BgColor = Colors.White, CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6 };
        foreach (string state in new[] { "normal", "hover", "pressed" })
            button.AddThemeStyleboxOverride(state, white);
        foreach (string state in new[] { "font_color", "font_hover_color", "font_pressed_color" })
            button.AddThemeColorOverride(state, new Color("#292929"));
        button.AddThemeFontSizeOverride("font_size", 23);
        button.Pressed += FinishPreview;
        stage.AddChild(button);
    }

    private void ConfigureEnemyHeader()
    {
        var header = stage.GetNode<Control>("EnemyHeader");
        var nameLabel = header.GetNode<Label>("EnemyName");
        nameLabel.Text = string.IsNullOrWhiteSpace(enemy.Name) ? "未知丧尸" : enemy.Name;
        if (nameLabel.Text.Length >= 6)
            nameLabel.AddThemeFontSizeOverride("font_size", 31);
        var skull = GD.Load<Texture2D>($"{ArtPath}battle_main_danger.tres");
        for (int index = 0; index < Math.Clamp(enemy.Level, 0, 5); index++)
        {
            var badge = new TextureRect
            {
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, Texture = skull,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter = MouseFilterEnum.Ignore
            };
            header.AddChild(badge);
            badge.Position = new Vector2(249 + index * 53, 20);
            badge.Size = new Vector2(43, 50);
        }
        string imagePath = $"{ArtPath}{enemy.Image}.png";
        if (!string.IsNullOrWhiteSpace(enemy.Image) && ResourceLoader.Exists(imagePath))
            stage.GetNode<TextureRect>("EnemyPresentation/EnemySprite").Texture = GD.Load<Texture2D>(imagePath);
    }

    private void AddLimbs(PackedScene cardScene, BattlePart part, int count, List<double> hitPoints, bool previewMissingArm)
    {
        int cols = Math.Clamp(count, 1, 6);
        int rows = Math.Max(1, (count + cols - 1) / cols);
        int partIndex = (int)part;
        float laneY = part switch { BattlePart.Head => 326, BattlePart.Body => 585, _ => 806 };
        float fit = Math.Min(1, Math.Min(820f / (cols * 204), 205f / (rows * 200)));
        for (int index = 0; index < count; index++)
        {
            double configuredHp = hitPoints != null && index < hitPoints.Count ? Math.Max(0, hitPoints[index]) : 0;
            var card = cardScene.Instantiate<Button>();
            card.Name = $"{part}{index + 1}";
            partContainer.AddChild(card);
            card.Size = new Vector2(192, 200);
            int row = index / cols;
            int itemsInRow = Math.Min(cols, count - row * cols);
            float separation = part == BattlePart.Arm && count == 2 ? 560 : 204 * fit;
            card.Position = new Vector2(540 + (index % cols - (itemsInRow - 1) / 2f) * separation - 96, laneY + (row * 200 + 100) * fit - 100);
            string displayName = part == BattlePart.Arm && count == 2
                ? (index == 0 ? "左臂" : "右臂")
                : count > 1 ? $"{PartNames[partIndex]} {index + 1}" : PartNames[partIndex];
            card.SetMeta("fit", fit);
            var limb = new Limb(part, displayName, previewMissingArm && index == 1 ? 0 : configuredHp, configuredHp, card);
            limbs.Add(limb);
            card.Pressed += () => PrecisionAttack(limb);
        }
    }

    private void RefreshPlayerData()
    {
        baseWeights[0] = player?.Attack_head_weight ?? 100;
        baseWeights[1] = player?.Attack_body_weight ?? 250;
        baseWeights[2] = player?.Attack_limb_weight ?? 150;
        for (int index = 0; index < Parts.Length; index++)
            weights[index] = limbs.Any(limb => limb.Part == Parts[index] && limb.Hp > 0) ? Math.Max(0, baseWeights[index]) : 0;
        if (weights.Sum() == 0)
            foreach (var limb in limbs.Where(limb => limb.Hp > 0))
                weights[(int)limb.Part] = 1;

        int[] shown = new int[3];
        double total = weights.Sum();
        if (total > 0)
        {
            var ratios = weights.Select(weight => 100 * weight / total).ToArray();
            shown = ratios.Select(ratio => (int)Math.Floor(ratio)).ToArray();
            foreach (int index in Enumerable.Range(0, 3).OrderByDescending(index => ratios[index] - shown[index]).Take(100 - shown.Sum()))
                shown[index]++;
        }
        for (int index = 0; index < Parts.Length; index++)
        {
            chanceLabels[index].Text = $"{shown[index]}%";
            weightLabels[index].Text = $"权重 {(int)weights[index]}";
            weightLabels[index].AddThemeColorOverride("font_color", weights[index] < baseWeights[index] ? Red : weights[index] > baseWeights[index] ? new Color("#9ce78e") : Pale);
        }
        SetVital("Health", player?.Hp ?? 100, player?.MaxHp ?? 100);
        SetVital("Armor", player?.Armor ?? 100, player?.MaxArmor ?? 100);
    }

    private void SetVital(string kind, double current, double maximum)
    {
        string frame = $"PlayerHUD/{kind}Frame";
        var bar = stage.GetNode<ProgressBar>($"{frame}/{kind}Bar");
        bar.MaxValue = Math.Max(1, maximum);
        bar.Value = current;
        stage.GetNode<Label>($"{frame}/Value").Text = $"{current:0.#}/{maximum:0.#}";
    }

    private void RefreshTurnControls()
    {
        targetingTween?.Kill();
        targetingTween = null;
        bool picking = phase == Phase.Targeting;
        SetAttackAvailability(randomButton, phase == Phase.Player && weights.Sum() > 0);
        SetAttackAvailability(precisionButton, picking || (phase == Phase.Player && limbs.Any(CanTarget)));
        precisionButton.GetNode<Label>("ActionName").Text = picking ? "选择攻击部位" : "精准攻击";
        precisionButton.GetNode<Control>("CancelHint").Visible = picking;
        precisionButton.GetNode<Control>("DamageIcon").Visible = !picking;
        precisionButton.GetNode<Control>("DamageValue").Visible = !picking;
        foreach (var limb in limbs)
            RenderLimb(limb, picking && CanTarget(limb));
        if (picking)
            AnimateTargets();

        int capacity = BattleSession.Capacity(player);
        for (int index = 0; index < Parts.Length; index++)
        {
            int count = session.Charges(Parts[index]);
            chargeLabels[index].Text = $"{count}/{capacity}";
            chargeLabels[index].AddThemeColorOverride("font_color", count == 0 ? Red : Pale);
            cooldownLabels[index].Text = count >= capacity ? "已满" : $"◷ {session.Remaining(Parts[index])}回合";
        }
    }

    private void SetAttackAvailability(Button button, bool available)
    {
        button.Disabled = !available;
        string suffix = available ? "" : "_disabled";
        button.GetNode<TextureRect>("Symbol").SelfModulate = available ? Colors.White : new Color("#92959c");
        button.GetNode<TextureRect>("DamageIcon").Texture = GD.Load<Texture2D>($"{ArtPath}battle_main_swords{suffix}.png");
        Color text = available ? Colors.White : new Color("#939da6");
        button.GetNode<Label>("ActionName").AddThemeColorOverride("font_color", text);
        button.GetNode<Label>("DamageValue").AddThemeColorOverride("font_color", text);
    }

    private void RenderLimb(Limb limb, bool selectable)
    {
        var button = limb.Card;
        bool missing = limb.Hp <= 0;
        float fit = (float)button.GetMeta("fit");
        button.Disabled = !selectable;
        button.ThemeTypeVariation = missing ? "BattlePartMissing" : selectable ? "BattlePartSelectable" : "BattlePartUnavailable";
        button.Scale = Vector2.One * fit * (selectable ? 1.04f : 1);
        button.RotationDegrees = 0;
        button.PivotOffset = button.Size / 2;
        button.GetNode<Control>("MissingBadge").Visible = missing;
        const string column = "Content/Column/";
        button.GetNode<TextureRect>($"{column}Symbol").Texture = (missing ? missingIcons : partIcons)[(int)limb.Part];
        button.GetNode<Label>($"{column}PartName").Text = limb.DisplayName;
        button.GetNode<Label>($"{column}Health/Value").Text = $"{limb.Hp:0.#}/{limb.MaxHp:0.#}";
        var health = button.GetNode<ProgressBar>($"{column}Health/Bar");
        health.MaxValue = Math.Max(1, limb.MaxHp);
        health.Value = limb.Hp;
    }

    private bool CanTarget(Limb limb) => limb.Hp > 0 && session.Charges(limb.Part) > 0;

    private void AnimateTargets()
    {
        targetingTween = CreateTween().SetLoops();
        foreach (float angle in new[] { -5f, 5f, -5f, 5f, 0f })
        {
            bool first = true;
            foreach (var limb in limbs.Where(CanTarget))
            {
                (first ? targetingTween : targetingTween.Parallel()).TweenProperty(limb.Card, "rotation_degrees", angle, 0.12);
                first = false;
            }
        }
        targetingTween.TweenInterval(0.19);
    }

    private void SelectSpeed(int requested)
    {
        speed = speed == requested ? 1 : requested;
        speedTwo.Modulate = speed == 2 ? Gold : Colors.White;
        speedFour.Modulate = speed == 4 ? Gold : Colors.White;
    }

    private void ToggleDetails()
    {
        details = !details;
        detailsToggle.Text = details ? "▲ 详情" : "▼ 详情";
        var backdrop = stage.GetNode<Control>("DetailBackdrop");
        backdrop.Size = new Vector2(backdrop.Size.X, details ? 309 : 293);
        foreach (var label in weightLabels.Concat(cooldownLabels))
            label.Visible = details;
    }

    private void ToggleTargeting()
    {
        if (bag != null || phase is not (Phase.Player or Phase.Targeting) || (phase == Phase.Player && !limbs.Any(CanTarget)))
            return;
        phase = phase == Phase.Targeting ? Phase.Player : Phase.Targeting;
        RefreshTurnControls();
    }

    public override void _UnhandledKeyInput(InputEvent input)
    {
        if (phase == Phase.Targeting && bag == null && input is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape })
        {
            ToggleTargeting();
            GetViewport().SetInputAsHandled();
        }
    }

    private void RandomAttack()
    {
        double total = weights.Sum();
        if (phase != Phase.Player || bag != null || total <= 0)
            return;
        double ticket = Random.Shared.NextDouble() * total;
        for (int index = 0; index < Parts.Length; index++)
        {
            ticket -= weights[index];
            if (ticket >= 0)
                continue;
            var targets = limbs.Where(limb => limb.Part == Parts[index] && limb.Hp > 0).ToArray();
            CommitAttack("随机", targets[Random.Shared.Next(targets.Length)]);
            return;
        }
    }

    private void PrecisionAttack(Limb limb)
    {
        if (phase == Phase.Targeting && bag == null && CanTarget(limb) && session.TrySpend(limb.Part))
            CommitAttack("精准", limb);
    }

    private void CommitAttack(string mode, Limb limb)
    {
        GD.Print($"攻击：玩家/{mode}/{limb.DisplayName}");
        playerProgress = 0;
        phase = Phase.Advancing;
        RefreshTurnControls();
        UpdateMarkers();
    }

    private void UpdateMarkers()
    {
        float left = track.Position.X + 13;
        float distance = track.Size.X - 26;
        playerMarker.Position = new Vector2(left + distance * (float)playerProgress - playerMarker.Size.X / 2, playerMarker.Position.Y);
        enemyMarker.Position = new Vector2(left + distance * (float)enemyProgress - enemyMarker.Size.X / 2, enemyMarker.Position.Y);
    }

    private void OpenBag()
    {
        if (bag != null || phase == Phase.Closed || player == null || GameManager.Instance == null)
            return;
        bag = GD.Load<PackedScene>("res://UI/Explore/BagUI.tscn").Instantiate<BagUI>();
        bag.EnableBattleMedicine = true;
        bag.TreeExited += () =>
        {
            bag = null;
            if (phase != Phase.Closed)
            {
                targetingTween?.Play();
                RefreshPlayerData();
            }
        };
        targetingTween?.Pause();
        stage.AddChild(bag);
    }

    private void FinishPreview()
    {
        if (phase == Phase.Closed)
            return;
        phase = Phase.Closed;
        Hide();
        QueueFree();
        var resume = onExit;
        onExit = null;
        resume?.Invoke();
    }
}

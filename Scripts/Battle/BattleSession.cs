using System;

public enum BattlePart
{
    Head,
    Body,
    Arm
}

public sealed class BattleSession
{
    private readonly int[] remaining = new int[3];
    private readonly int[] charges = new int[3];

    public BattleSession(PlayerManager player)
    {
        foreach (BattlePart part in Enum.GetValues<BattlePart>())
            remaining[(int)part] = Cooldown(player, part);
    }

    public static int Capacity(PlayerManager player) => Math.Max(1, player?.PrecisionChargeCapacity ?? 2);

    public int Remaining(BattlePart part) => remaining[(int)part];

    public int Charges(BattlePart part) => charges[(int)part];

    public void PlayerTurnStarted(PlayerManager player)
    {
        foreach (BattlePart part in Enum.GetValues<BattlePart>())
        {
            int index = (int)part;
            int capacity = Capacity(player);
            charges[index] = Math.Min(charges[index], capacity);
            if (charges[index] == capacity)
                continue;

            if (--remaining[index] > 0)
                continue;

            charges[index]++;
            remaining[index] = Cooldown(player, part);
        }
    }

    public bool TrySpend(BattlePart part)
    {
        int index = (int)part;
        if (charges[index] == 0)
            return false;

        charges[index]--;
        return true;
    }

    private static int Cooldown(PlayerManager player, BattlePart part) => Math.Max(1, part switch
    {
        BattlePart.Head => player?.HeadTime ?? 6,
        BattlePart.Body => player?.BodyTime ?? 2,
        _ => player?.ArmTime ?? 3,
    });
}


using System.Collections.Generic;

public static class Consts
{
    public static readonly List<int> carefulExploreProgress = new List<int> { 7, 8 };
    public static readonly List<int> quickExploreProgress = new List<int> { 15, 16, 17 };
    public static readonly List<int> carefulNoiseProgress = new List<int> { 16, 17, 18, 19, 20 };
    public static readonly List<int> quickNoiseProgress = new List<int> { 10, 11 };
    public static readonly List<int> leaveDanger = new List<int> { 1, 2, 3 };
    public static readonly List<int> leaveDangerWeight = new List<int> { 100, 200, 300 };

    //每次训练基础经验值
    public static readonly int trainExp = 50;
    //每点智力增加的百分比经验获取效率
    public static readonly int expPerIntellect = 2;
    //每项训练次数的最大值
    public static readonly int maxTrainTimes = 3;

}

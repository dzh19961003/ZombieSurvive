
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public static class Tools
{
    //给出一个List或者Array返回一个随机数   
    public static int GetRandomNumber(Array<int> numArray)
    {
        if (numArray == null || numArray.Count == 0) return -1;
        int r = GD.RandRange(0, numArray.Count - 1);        
        return numArray[r];
    }
    public static int GetRandomNumber(List<int> numList)
    {
        if (numList == null || numList.Count == 0) return -1;
        int r = GD.RandRange(0, numList.Count - 1);
        return numList[r];
    }
    //给出一个List或者Array，根据权重返回一个随机数
    public static int GetRandomNumber(Array<int> numArray,Array<int> weight)
    {
        int result = 0;
        int sum = 0;
        for (int i = 0; i < weight.Count; i++)
        {
            sum += weight[i];
        }
        int r=  GD.RandRange(0, sum);

        sum = 0;
        for (int i = 0; i < weight.Count; i++)
        {
            sum += weight[i];
            if (sum>r)
            {
                result = i;
                break;
            }
        }
        return numArray[result];
    }
    public static int GetRandomNumber(List<int> numArray, List<int> weight)
    {
        int result = 0;
        int sum = 0;
        for (int i = 0; i < weight.Count; i++)
        {
            sum += weight[i];
        }
        int r = GD.RandRange(0, sum);

        sum = 0;
        for (int i = 0; i < weight.Count; i++)
        {
            sum += weight[i];
            if (sum > r)
            {
                result = i;
                break;
            }
        }
        return numArray[result];
    }
    //从 Godot 的 Dictionary 里安全地读小数（存档读档用）
    //注意：Dictionary 取出来的是 Godot.Variant，不能用 Convert.ToDouble（会抛异常），
    //要按 Variant 的实际类型分开取值
    public static double LoadDouble(Dictionary data, string key, double defaultValue)
    {
        if (data == null || !data.ContainsKey(key))
        {
            return defaultValue;
        }
        Variant value = data[key];
        if (value.VariantType == Variant.Type.Int)
        {
            return value.AsInt32();
        }
        if (value.VariantType == Variant.Type.Float)
        {
            return value.AsDouble();
        }
        return defaultValue;
    }
}

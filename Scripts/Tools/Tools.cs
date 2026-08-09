
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
}


using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public static class Tools
{
    //给出一个List或者Array返回一个随机数   
    public static int GetRandomNumber(Array<int> numArray)
    {
        numArray = new Array<int>();
        int r = GD.RandRange(1, numArray.Count);        
        return numArray[r];
    }
    public static int GetRandomNumber(List<int> numList)
    {
        numList = new List<int>();
        int r = GD.RandRange(1, numList.Count);
        return numList[r];
    }
    //给出一个List或者Array，根据权重返回一个随机数
    public static int GetRandomNumber(Array<int> numArray,Array<int> weight)
    {
        return 1;
    }
    public static int GetRandomNumber(List<int> numArray, Array<int> weight)
    {
        return 1;
    }

}

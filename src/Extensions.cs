using Godot;
using System;
using System.Collections.Generic;

public static class Extensions
{
    private static Random rnd = new Random();

    public static T GetRandom<T>(this Godot.Collections.Array<T> source)
    {
        int randIndex = rnd.Next(source.Count);
        return source[randIndex];
    }

    public static Godot.Collections.Array<T> As<T>(this Godot.Collections.Array originalArray) where T : class
    {
        Godot.Collections.Array<T> newArray = new Godot.Collections.Array<T>();
        for (int i = 0; i < originalArray.Count; i++)
        {
            newArray.Add(originalArray[i] as T);
        }
        return newArray;
    }
  
}

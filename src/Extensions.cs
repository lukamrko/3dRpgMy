using Godot;
using System;

public static class Extensions
{

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

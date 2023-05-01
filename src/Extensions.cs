using Godot;
using System;
using System.Collections.Generic;

public static class Extensions
{
    private static Random rnd = new Random();

    public static T GetRandom<[MustBeVariant] T>(this Godot.Collections.Array<T> source)
    {
        int randIndex = rnd.Next(source.Count);
        return source[randIndex];
    }


    public static Godot.Collections.Array<T> As<[MustBeVariant] T>(this Godot.Collections.Array<Node> originalArray)
    where T : class
    {
        Godot.Collections.Array<T> newArray = new Godot.Collections.Array<T>();
        for (int i = 0; i < originalArray.Count; i++)
        {
            newArray.Add(originalArray[i] as T);
        }
        return newArray;
    }

    public static Vector3 Rounded(this Vector3 originalVector)
    {
        // return new Vector3
        // (
        //     Convert.ToInt32(originalVector.x),
        //     Convert.ToInt32(originalVector.y),
        //     Convert.ToInt32(originalVector.z)
        // );

        var x = (float)Math.Round(originalVector.X * 2, MidpointRounding.AwayFromZero) / 2;
        var y = (float)Math.Round(originalVector.Y * 2, MidpointRounding.AwayFromZero) / 2;
        var z = (float)Math.Round(originalVector.Z * 2, MidpointRounding.AwayFromZero) / 2;

        return new Vector3(x, y, z);
    }

    public static void AddRangeAs<[MustBeVariant] T, [MustBeVariant] A>(this Godot.Collections.Array<A> originalArray, Godot.Collections.Array<T> newArray)
    where T : class
    where A : class
    {
        foreach (var element in newArray)
        {
            originalArray.Add(element as A);
        }
    }

    public static bool EqualsAnyOf<T>(this T source, params T[] list)
    {
        if (source is null)
        {
            throw new ArgumentNullException("Source is null!");
        }
        for (int i = 0; i < list.Length; i++)
        {
            if (source.Equals(list[i]))
            {
                return true;
            }
        }
        return false;
    }

}

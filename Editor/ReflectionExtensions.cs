using System;
using System.Reflection;

public static class ReflectionExtensions
{
    public static T GetFieldValue<T>(this object obj, string name)
    {
        // Set the flags so that private and public fields from instances will be found
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var field = obj.GetType().GetField(name, bindingFlags);
        return (T)field?.GetValue(obj);
    }


    public static T GetFirst<T>( this T[] obj, Predicate<T> predicate) where T : class
    {
        for(int i = 0; i < obj.Length;i++)
        {
            if (predicate(obj[i])) return obj[i];
        }
        return null;
    }

    public static int Find<T>(this T[] obj, Predicate<T> predicate) where T : class
    {
        for (int i = 0; i < obj.Length; i++)
        {
            if (predicate(obj[i])) return i;
        }
        return -1;
    }
}
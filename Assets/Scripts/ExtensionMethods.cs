using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DarienEngine;

public static class ExtensionMethods
{
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (item, index));
    }
}

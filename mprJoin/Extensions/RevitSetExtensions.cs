namespace mprJoin.Extensions;

using System.Collections.Generic;
using Autodesk.Revit.DB;

/// <summary>
/// Расширения для массивов ревита
/// </summary>
public static class RevitSetExtensions
{
    /// <summary>
    /// Получение списка из ElementArray.
    /// </summary>
    /// <param name="elementArray">ElementArray.</param>
    public static List<Element> ToList(this ElementArray elementArray)
    {
        var result = new List<Element>();
        foreach (Element element in elementArray)
        {
            result.Add(element);
        }

        return result;
    }
}
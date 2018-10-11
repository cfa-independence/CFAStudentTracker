using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// Partial class for helper methods
/// </summary>
public static partial class SelectHelpers
{
    /// <summary>
    /// Creates an IEnumerable&lt;SelectListItem&gt; from an IEnumerable&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">The type of the elements of enumerable.</typeparam>
    /// <param name="enumerable">The IEnumerable&lt;T&gt; to create an IEnumerable&lt;SelectListItem&gt; from.</param>
    /// <param name="value">A function to extract a value from each element.</param>
    /// <param name="selectAll">Whether all values are selected.</param>
    /// <returns>An IEnumerable&lt;SelectListItem&gt; that contains elements from the IEnumerable&lt;T&gt;.</returns>
    public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> enumerable, Func<T, object> value, bool selectAll = false)
    {
        return enumerable.ToSelectList(value, value, selectAll);
    }

    /// <summary>
    /// Creates an IEnumerable&lt;SelectListItem&gt; from an IEnumerable&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">The type of the elements of enumerable.</typeparam>
    /// <param name="enumerable">The IEnumerable&lt;T&gt; to create a IEnumerable&lt;SelectListItem&gt; from.</param>
    /// <param name="value">A function to extract a value from each element.</param>
    /// <param name="selectedValue">Items matching this value will be initially selected.</param>
    /// <returns>An IEnumerable&lt;SelectListItem&gt; that contains elements from the IEnumerable&lt;T&gt;.</returns>
    public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> enumerable, Func<T, object> value, object selectedValue)
    {
        return enumerable.ToSelectList(value, value, new List<object>() { selectedValue });
    }

    /// <summary>
    /// Creates an IEnumerable&lt;SelectListItem&gt; from an IEnumerable&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">The type of the elements of enumerable.</typeparam>
    /// <param name="enumerable">The IEnumerable&lt;T&gt; to create a IEnumerable&lt;SelectListItem&gt; from.</param>
    /// <param name="value">A function to extract a value from each element.</param>
    /// <param name="selectedValues">Items matching these values will be initially selected.</param>
    /// <returns>An IEnumerable&lt;SelectListItem&gt; that contains elements from the IEnumerable&lt;T&gt;.</returns>
    public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> enumerable, Func<T, object> value, IEnumerable<object> selectedValues)
    {
        return enumerable.ToSelectList(value, value, selectedValues);
    }

    /// <summary>
    /// Creates an IEnumerable&lt;SelectListItem&gt; from an IEnumerable&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">The type of the elements of enumerable.</typeparam>
    /// <param name="enumerable">The IEnumerable&lt;T&gt; to create a IEnumerable&lt;SelectListItem&gt; from.</param>
    /// <param name="value">A function to extract a value from each element.</param>
    /// <param name="text">A function to extract display text from each element.</param>
    /// <param name="selectAll">Whether all values are selected.</param>
    /// <returns>An IEnumerable&lt;SelectListItem&gt; that contains elements from the IEnumerable&lt;T&gt;.</returns>
    public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> enumerable, Func<T, object> value, Func<T, object> text, bool selectAll = false)
    {
        foreach (var f in enumerable.Where(x => x != null))
        {
            yield return new SelectListItem()
            {
                Value = value(f).ToString(),
                Text = text(f).ToString(),
                Selected = selectAll
            };
        }
    }

    /// <summary>
    /// Creates an IEnumerable&lt;SelectListItem&gt; from an IEnumerable&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">The type of the elements of enumerable.</typeparam>
    /// <param name="enumerable">The IEnumerable&lt;T&gt; to create a IEnumerable&lt;SelectListItem&gt; from.</param>
    /// <param name="value">A function to extract a value from each element.</param>
    /// <param name="text">A function to extract display text from each element.</param>
    /// <param name="selectedValue">Items matching this value will be initially selected.</param>
    /// <returns>An IEnumerable&lt;SelectListItem&gt; that contains elements from the IEnumerable&lt;T&gt;.</returns>
    public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> enumerable, Func<T, object> value, Func<T, object> text, object selectedValue)
    {
        return enumerable.ToSelectList(value, text, new List<object>() { selectedValue });
    }

    /// <summary>
    /// Creates an IEnumerable&lt;SelectListItem&gt; from an IEnumerable&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">The type of the elements of enumerable.</typeparam>
    /// <param name="enumerable">The IEnumerable&lt;T&gt; to create a IEnumerable&lt;SelectListItem&gt; from.</param>
    /// <param name="value">A function to extract a value from each element.</param>
    /// <param name="text">A function to extract display text from each element.</param>
    /// <param name="selectedValues">Items matching these values will be initially selected.</param>
    /// <returns>An IEnumerable&lt;SelectListItem&gt; that contains elements from the IEnumerable&lt;T&gt;.</returns>
    public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> enumerable, Func<T, object> value, Func<T, object> text, IEnumerable<object> selectedValues)
    {
        var sel = selectedValues != null
            ? selectedValues.Where(x => x != null).ToList().ConvertAll<string>(x => x.ToString())
            : new List<string>();

        foreach (var f in enumerable.Where(x => x != null))
        {
            yield return new SelectListItem()
            {
                Value = value(f).ToString(),
                Text = text(f).ToString(),
                Selected = sel.Contains(value(f).ToString())
            };
        }
    }
}
namespace HourScript;

using System.Text;

public static class Calc
{
    public static string Join<T>(IEnumerable<T> list, string separator)
    {
        StringBuilder sb = new();
        bool first = true;

        foreach (T item in list)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                sb.Append(separator);
            }

            sb.Append(item);
        }

        return sb.ToString();
    }

    public static List<E> Map<T, E>(List<T> list, Func<T, E> converter)
    {
        List<E> result = new();

        foreach (T item in list)
        {
            result.Add(converter(item));
        }

        return result;
    } 

    public static int? DoubleToIndex(double value, int size)
    {
        if (!double.IsRealNumber(value)) return null;

        int intIdx = (int)value;

        if (intIdx < 0 || intIdx >= size) return null;

        return intIdx;
    }
}
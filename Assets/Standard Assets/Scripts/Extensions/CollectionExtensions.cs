using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Extensions
{
	public static class CollectionExtensions 
	{
		public static List<T> ToList_class<T> (this T[] array) where T : class
		{
			List<T> output = new List<T>();
			output.AddRange(array);
			return output;
		}

		public static bool Contains_class<T> (this T[] array, T element) where T : class
		{
			foreach (T obj in array)
			{
				if (obj == element)
					return true;
			}
			return false;
		}

		public static int IndexOf_class<T> (this T[] array, T element) where T : class
		{
			for (int i = 0; i < array.Length; i ++)
			{
				if (array[i] == element)
					return i;
			}
			return -1;
		}

		public static T[] Add_class<T> (this T[] array, T element) where T : class
		{
			List<T> output = array.ToList_class();
			output.Add(element);
			return output.ToArray();
		}

		public static T[] Insert_class<T> (this T[] array, int index, T element) where T : class
		{
			List<T> output = array.ToList_class();
			output.Insert(index, element);
			return output.ToArray();
		}

		public static T[] Remove_class<T> (this T[] array, T element) where T : class
		{
			List<T> output = array.ToList_class();
			output.Remove(element);
			return output.ToArray();
		}

		public static T[] RemoveAt_class<T> (this T[] array, int index) where T : class
		{
			List<T> output = array.ToList_class();
			output.RemoveAt(index);
			return output.ToArray();
		}

		public static T[] RemoveRange_class<T> (this T[] array, IEnumerable<T> array2) where T : class
		{
			List<T> output = array.ToList_class();
			foreach (T element in array2)
				output.Remove(element);
			return output.ToArray();
		}

		public static T[] AddRange_class<T> (this T[] array, IEnumerable<T> array2) where T : class
		{
			List<T> output = array.ToList_class();
			output.AddRange(array2);
			return output.ToArray();
		}
		
		public static List<T> ToList<T> (this T[] array)
		{
			List<T> output = new List<T>();
			output.AddRange(array);
			return output;
		}

		public static T[] Add<T> (this T[] array, T element)
		{
			List<T> output = array.ToList();
			output.Add(element);
			return output.ToArray();
		}

		public static T[] Remove<T> (this T[] array, T element)
		{
			List<T> output = array.ToList();
			output.Remove(element);
			return output.ToArray();
		}

		public static T[] RemoveAt<T> (this T[] array, int index)
		{
			List<T> output = array.ToList();
			output.RemoveAt(index);
			return output.ToArray();
		}

		public static T[] AddRange<T> (this T[] array, IEnumerable<T> array2)
		{
			List<T> output = array.ToList();
			output.AddRange(array2);
			return output.ToArray();
		}

		public static bool Contains<T> (this T[] array, T element)
		{
			foreach (T obj in array)
			{
				if (obj.Equals(element))
					return true;
			}
			return false;
		}

		public static int IndexOf<T> (this T[] array, T element)
		{
			for (int i = 0; i < array.Length; i ++)
			{
				if (array[i].Equals(element))
					return i;
			}
			return -1;
		}
		
		public static T[] Reverse<T> (this T[] array)
		{
			List<T> output = array.ToList();
			output.Reverse();
			return output.ToArray();
		}

		public static string ToString<T> (this T[] array, string elementSeparator)
		{
			string output = "";
			if (array != null)
			{
				foreach (T element in array)
					output += element.ToString() + elementSeparator;
			}
			return output;
		}

		public static T[] AddArray<T> (this T[] array, Array array2)
		{
			List<T> output = array.ToList();
			for (int i = 0; i < array2.Length; i ++)
				output.Add((T) array2.GetValue(i));
			return output.ToArray();
		}

		public static List<T> ToList_struct<T> (this T[] array) where T : struct
		{
			List<T> output = new List<T>();
			output.AddRange(array);
			return output;
		}

		public static int IndexOf_struct<T> (this T[] array, T element) where T : struct
		{
			for (int i = 0; i < array.Length; i ++)
			{
				if (array[i].Equals(element))
					return i;
			}
			return -1;
		}

		public static T[] AddRange_struct<T> (this T[] array, IEnumerable<T> array2) where T : struct
		{
			List<T> output = array.ToList_struct();
			output.AddRange(array2);
			return output.ToArray();
		}

		public static int IndexOf_Array<T> (this Array array, T element)
		{
			for (int i = 0; i < array.Length; i ++)
			{
				if (array.GetValue(i).Equals(element))
					return i;
			}
			return -1;
		}

		public static int IndexOf_ICollection<T> (this ICollection collection, T element)
		{
			int i = 0;
			IEnumerator enumerator = collection.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Equals(element))
					return i;
				i ++;
			}
			return -1;
		}

		public static T GetValue_ICollection<T> (this ICollection collection, int index)
		{
			IEnumerator enumerator = collection.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (index == 0)
					return (T) enumerator.Current;
				index --;
			}
			return default(T);
		}

		public static bool Contains_IList<T> (this IList<T> list, T element)
		{
			IEnumerator enumerator = list.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (((T) enumerator.Current).Equals(element))
					return true;
			}
			return false;
		}
	}
}
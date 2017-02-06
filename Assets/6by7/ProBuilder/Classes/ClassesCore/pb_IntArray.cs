/*	This exists because Unity can't
 *	serialize jaggaed arrays.  Also, it 
 *	has a couple of handy methods that make
 *	dealing with shared vertex indices easier.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[System.Serializable]
/**
 *	\brief Used as a substitute for a jagged int array.  
 *	Also contains some ProBuilder specific extensions for 
 *	dealing with jagged int arrays.  Note that this class
 *	exists because Unity does not serialize jagged arrays.
 */
public class pb_IntArray
{
#region Members

	public int[] array;
#endregion

#region Constructor / Operators

	public List<int> ToList()
	{
		return new List<int>(array);
	}

	public pb_IntArray(int[] intArray)
	{
		array = intArray;
	}

	// Copy constructor
	public pb_IntArray(pb_IntArray intArray)
	{
		array = intArray.array;
	}

	public int this[int i]
	{	
		get { return array[i]; }
		set { array[i] = value; }
	}

	public int Length
	{
		get { return array.Length; }
	}

	public static implicit operator int[](pb_IntArray intArr)
	{
		return intArr.array;
	}
#endregion

	public override string ToString()
	{
		string str = "";
		for(int i = 0; i < array.Length - 1; i++)
			str += array[i] + ", ";
		str += array[array.Length-1];

		return str;
	}
	
	public bool IsEmpty()
	{
		return (array == null || array.Length < 1);
	}

	public static void RemoveEmptyOrNull(ref pb_IntArray[] val)
	{
		List<pb_IntArray> valid = new List<pb_IntArray>();
		foreach(pb_IntArray par in val)
		{
			if(par != null && !par.IsEmpty())
				valid.Add(par);
		}
		val = valid.ToArray();
	}
}	

public static class IntArrayExtensions
{
	// Returns a jagged int array
	public static int[][] ToArray(this pb_IntArray[] val)
	{
		int[][] arr = new int[val.Length][];
		for(int i = 0; i < arr.Length; i++)
			arr[i] = val[i].array;
		return arr;
	}

	public static List<List<int>> ToList(this pb_IntArray[] val)
	{
		List<List<int>> l = new List<List<int>>();
		for(int i = 0; i < val.Length; i++)
			l.Add( val[i].ToList() );
		return l;
	}

	public static string ToFormattedString(this pb_IntArray[] arr)
	{
		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < arr.Length; i++)
			sb.Append( "[" + arr[i].array.ToFormattedString(", ") + "] " );
		
		return sb.ToString();
	}

	public static bool Contains(this pb_IntArray[] pbIntArr, int[] arr)
	{
		List<int> a = (List<int>)arr.OrderBy(s => s);
		for(int i = 0; i < pbIntArr.Length; i++)
		{
			if(a.SequenceEqual( pbIntArr[i].array.OrderBy(s => s) ))
				return true;
		}
		return false;
	}

	public static bool IsEqual(this pb_IntArray pbIntArr, int[] arr)
	{
		return arr.OrderBy(s => s).SequenceEqual( pbIntArr.array.OrderBy(s => s) );
	}

	public static int IndexOf(this int[] array, int val, pb_IntArray[] sharedIndices)
	{
		int indInShared = sharedIndices.IndexOf(val);
		if(indInShared < 0) return -1;

		int[] allValues = sharedIndices[indInShared];

		for(int i = 0; i < array.Length; i++)
			if(System.Array.IndexOf(allValues, array[i]) > -1)
				return i;

		return -1;
	}

	// Scans an array of pb_IntArray and returns the index of that int[] that holds the index.
	// Aids in removing duplicate vertex indices.
	public static int IndexOf(this pb_IntArray[] intArray, int index)
	{
		for(int i = 0; i < intArray.Length; i++)
		{
			// for some reason, this is about 2x faster than System.Array.IndexOf
			for(int n = 0; n < intArray[i].Length; n++)	
				if(intArray[i][n] == index)
					return i;
		}
		return -1;
	}

	public static int IndexOf(this pb_IntArray[] pbIntArr, int[] arr)
	{
		IOrderedEnumerable<int> a = arr.OrderBy(s => s);
		for(int i = 0; i < pbIntArr.Length; i++)
		{
			IOrderedEnumerable<int> b = pbIntArr[i].array.OrderBy(s => s);

			if(a.SequenceEqual(b))
				return i;
		}
		return -1;
	}

	// Returns all shared vertices with input of index array
	public static int[] AllIndicesWithValues(this pb_IntArray[] pbIntArr, int[] indices)
	{
		List<int> used = new List<int>();
		List<int> shared = new List<int>();
		for(int i = 0; i < indices.Length; i++)
		{
			int indx = pbIntArr.IndexOf(indices[i]);
			if(used.Contains(indx))
				continue;
			shared.AddRange(pbIntArr[indx].array);
			used.Add(indx);
		}

		return shared.ToArray();
	}

	/**
	 *	Given triangles, this returns a distinct array containing the first value of each sharedIndex array entry.
	 */
	public static int[] UniqueIndicesWithValues(this pb_IntArray[] pbIntArr, int[] values)
	{
		List<int> unique = new List<int>(values);
		
		for(int i = 0; i < unique.Count; i++)
			unique[i] = pbIntArr[pbIntArr.IndexOf(values[i])][0];

		return unique.Distinct().ToArray();
	}
}
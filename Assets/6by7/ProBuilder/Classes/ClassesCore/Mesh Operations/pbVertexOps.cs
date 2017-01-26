using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.Math;
using ProBuilder2.Triangulator;
using ProBuilder2.Triangulator.Geometry;

namespace ProBuilder2.MeshOperations
{
	public static class pbVertexOps
	{
#region Merge / Split

		/**
		 *	\brief Collapses all passed indices to a single shared index.
		 *	
		 */
		public static bool MergeVertices(this pb_Object pb, int[] indices)
		{
			Vector3[] verts = pb.vertices;
			Vector3 cen = Vector3.zero;

			foreach(int i in indices)
				cen += verts[i];
				
			cen /= (float)indices.Length;

			int newIndex = pb.MergeSharedIndices(indices);

			int firstTriInSharedIndexArr = pb.sharedIndices[newIndex][0];

			pb.SetSharedVertexPosition(firstTriInSharedIndexArr, cen);

			int[] mergedSharedIndex = pb.GetSharedIndices()[newIndex].array;
			
			int[] removedIndices = pb.RemoveDegenerateTriangles();

			// get a non-deleted index to work with
			int ind = -1;
			for(int i = 0; i < mergedSharedIndex.Length; i++)
				if(!removedIndices.Contains(mergedSharedIndex[i]))
					ind = mergedSharedIndex[i];


			int t = ind;
			for(int i = 0; i < removedIndices.Length; i++)
				if(ind > removedIndices[i])	
					t--;

			pb.ClearSelection();

			if(t > -1)
				pb.SetSelectedTriangles(new int[1] { t });

			return true;	
		}

		/**
		 *	Similar to Merge vertices, expect that this method only collapses vertices within
		 *	a specified distance of one another (typically epsilon).
		 */
		public static bool WeldVertices(this pb_Object pb, int[] indices, float delta)
		{
			int[] si = new int[indices.Length];
			Vector3[] v = pb.vertices;

			// set the shared indices cache to a unique non-used index
			for(int i = 0; i < indices.Length; i++)
				si[i] = -(i-1);
			
			for(int i = 0; i < indices.Length-1; i++)
			{
				for(int n = i+1; n < indices.Length; n++)
				{
					if(si[i] == si[n])
						continue;	// they already share a vertex

					// Note that this will not take into account 
					if(Vector3.Distance(v[indices[i]], v[indices[n]]) < delta)
					{
						Vector3 cen = (v[indices[i]] + v[indices[n]]) / 2f;
						v[indices[i]] = cen;
						v[indices[n]] = cen;
						int newIndex = pb.MergeSharedIndices(new int[2] {indices[i], indices[n]});
						si[i] = newIndex;
						si[n] = newIndex;
					}
				}
			}

			pb.SetVertices(v);

			return true;	
		}

		public static bool SplitVertices(this pb_Object pb, int[] indices)
		{
			pb_IntArray[] sharedIndices = pb.sharedIndices;

			List<int> usedIndex = new List<int>();
			List<int> splits = new List<int>();

			for(int i = 0; i < indices.Length; i++)
			{
				int index = sharedIndices.IndexOf(indices[i]);

				if(!usedIndex.Contains(index))
				{
					usedIndex.Add(index);
					splits.AddRange(sharedIndices[index].array);
				}
			}

			pb.RemoveIndicesFromSharedArray(splits.ToArray());

			foreach(int i in splits)
				pb.AddIndexToSharedIndexArray(-1, i);

			return true;
		}
#endregion

#region Add / Subtract


	/**
	 *	Given a face and a point, this will add a vertex to the pb_Object and retriangulate the face.
	 */
	public static bool AppendVertexToFace(this pb_Object pb, pb_Face face, Vector3 point, ref pb_Face newFace)
	{
		if(!face.isValid()) return false;

		// First order of business - project face to 2d
		int[] distinctIndices = face.distinctIndices;
		Vector3[] verts = pb.GetVertices(distinctIndices);

		// Get the face normal before modifying the vertex array
		Vector3 nrm = pb_Math.PlaneNormal(verts);
		Vector3 projAxis = pb_Math.GetProjectionAxis(nrm).ToVector3();
		
		// Add the new point
		verts = verts.Add(point);

		// Project
		List<Vector2> plane = new List<Vector2>(pb_Math.VerticesTo2DPoints(verts, projAxis));

		// Save the sharedIndices index for each distinct vertex
		pb_IntArray[] sharedIndices = pb.sharedIndices;
		int[] sharedIndex = new int[distinctIndices.Length+1];
		for(int i = 0; i < distinctIndices.Length; i++)
			sharedIndex[i] = sharedIndices.IndexOf(distinctIndices[i]);
		sharedIndex[distinctIndices.Length] = -1;	// add the new vertex to it's own sharedIndex

		// Triangulate the face with the new point appended
		int[] tris = Delauney.Triangulate(plane).ToIntArray();

		// Check to make sure the triangulated face is facing the same direction, and flip if not
		Vector3 del = Vector3.Cross( verts[tris[2]] - verts[tris[0]], verts[tris[1]]-verts[tris[0]]).normalized;
		if(Vector3.Dot(nrm, del) > 0) System.Array.Reverse(tris);

		// Compose new face
		newFace = pb.AppendFace(verts, new pb_Face(tris, face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, face.color), sharedIndex);

		// And delete the old
		pb.DeleteFace(face);

		return true;
	}

	/**
	 *	Given a face and a point, this will add a vertex to the pb_Object and retriangulate the face.
	 */
	public static bool AppendVerticesToFace(this pb_Object pb, pb_Face face, List<Vector3> points, out pb_Face newFace)
	{
		if(!face.isValid())
		{
			newFace = face;
			return false;
		}

		// First order of business - project face to 2d
		int[] distinctIndices = face.distinctIndices;
		Vector3[] verts = pb.GetVertices(distinctIndices);

		// Get the face normal before modifying the vertex array
		Vector3 nrm = pb_Math.PlaneNormal(verts);
		Vector3 projAxis = pb_Math.GetProjectionAxis(nrm).ToVector3();
		
		// Add the new point
		Vector3[] t_verts = new Vector3[verts.Length + points.Count];
		System.Array.Copy(verts, 0, t_verts, 0, verts.Length);
		System.Array.Copy(points.ToArray(), 0, t_verts, verts.Length, points.Count);

		verts = t_verts;

		// Project
		List<Vector2> plane = new List<Vector2>(pb_Math.VerticesTo2DPoints(verts, projAxis));

		// Save the sharedIndices index for each distinct vertex
		pb_IntArray[] sharedIndices = pb.sharedIndices;
		int[] sharedIndex = new int[distinctIndices.Length+points.Count];
		for(int i = 0; i < distinctIndices.Length; i++)
			sharedIndex[i] = sharedIndices.IndexOf(distinctIndices[i]);
		
		for(int i = distinctIndices.Length; i < distinctIndices.Length+points.Count; i++)
			sharedIndex[i] = -1;	// add the new vertex to it's own sharedIndex

		// Triangulate the face with the new point appended
		int[] tris = Delauney.Triangulate(plane).ToIntArray();

		// Check to make sure the triangulated face is facing the same direction, and flip if not
		Vector3 del = Vector3.Cross( verts[tris[2]] - verts[tris[0]], verts[tris[1]]-verts[tris[0]]).normalized;
		if(Vector3.Dot(nrm, del) > 0) System.Array.Reverse(tris);

		// Compose new face
		newFace = pb.AppendFace(verts, new pb_Face(tris, face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, face.color), sharedIndex);

		// And delete the old
		pb.DeleteFace(face);	

		return true;
	}
#endregion
	}
}
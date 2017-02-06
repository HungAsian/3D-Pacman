using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/**
 *	Used to query pb_Objects for more detailed information than what would belong in the pbObejct class
 */
namespace ProBuilder2.MeshOperations
{
	public class pbMeshUtils
	{
		/**
		 *	Returns all faces connected to the passed edge.
		 */
		public static List<pb_Face> GetConnectedFaces(pb_Object pb, pb_Edge edge)
		{
			List<pb_Face> faces = new List<pb_Face>();
			pb_IntArray[] sharedIndices = pb.sharedIndices;

			foreach(pb_Face f in pb.faces)
			{
				if(f.edges.IndexOf(edge, sharedIndices) > -1)
					faces.Add(f);
			}
			return faces;
		}

		// todo update this and ^ this with faster variation below
		public static List<pb_Face> GetConnectedFaces(pb_Object pb, pb_Edge[] edges)
		{
			List<pb_Face> faces = new List<pb_Face>();
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			foreach(pb_Face f in pb.faces)
			{
				foreach(pb_Edge e in edges)
					if(!faces.Contains(f) && f.edges.IndexOf(e, sharedIndices) > -1)
						faces.Add(f);
			}
			return faces;
		}

		// todo - make this slightly less jagged
		public static List<pb_Face>[][] GetConnectedFacesJagged(pb_Object pb, pb_Face[] selFaces)
		{
			int len = selFaces.Length;

			List<pb_Face>[][] faces = new List<pb_Face>[len][];
			for(int j = 0; j < len; j++)
			{
				faces[j] = new List<pb_Face>[selFaces[j].edges.Length];
				for(int i = 0; i < selFaces[j].edges.Length; i++)
					faces[j][i] = new List<pb_Face>();
			}

			pb_IntArray[] sharedIndices = pb.sharedIndices;
				
			pb_Edge[][] sharedEdges = new pb_Edge[len][];
			for(int i = 0; i < len; i++)
				sharedEdges[i] = pb_Edge.GetUniversalEdges(selFaces[i].edges, sharedIndices);

			for(int i = 0; i < pb.faces.Length; i++)
			{
				pb_Edge[] faceEdges = pb_Edge.GetUniversalEdges(pb.faces[i].edges, sharedIndices);
				
				for(int j = 0; j < len; j++)
				{
					int ind = faceEdges.ContainsMatch(sharedEdges[j]);
					if(ind > -1)
						faces[j][ind].Add(pb.faces[i]);
				}
			}

			return faces;
		}

		public static List<pb_Face>[][] GetConnectedFacesJagged(pb_Object pb, pb_Edge[][] selEdges)
		{
			int len = selEdges.Length;

			List<pb_Face>[][] faces = new List<pb_Face>[len][];
			for(int j = 0; j < len; j++)
			{
				faces[j] = new List<pb_Face>[selEdges[j].Length];
				for(int i = 0; i < selEdges[j].Length; i++)
					faces[j][i] = new List<pb_Face>();
			}

			pb_IntArray[] sharedIndices = pb.sharedIndices;
				
			pb_Edge[][] sharedEdges = new pb_Edge[len][];
			for(int i = 0; i < len; i++)
				sharedEdges[i] = pb_Edge.GetUniversalEdges(selEdges[i], sharedIndices);

			for(int i = 0; i < pb.faces.Length; i++)
			{
				pb_Edge[] faceEdges = pb_Edge.GetUniversalEdges(pb.faces[i].edges, sharedIndices);
				
				for(int j = 0; j < len; j++)
				{
					int ind = faceEdges.ContainsMatch(sharedEdges[j]);
					if(ind > -1)
						faces[j][ind].Add(pb.faces[i]);
				}
			}

			return faces;
		}

		/**
		 *	Returns all faces connected to the passed vertex index.
		 */
		public static List<pb_Face> GetConnectedFaces(pb_Object pb, int index)
		{
			List<pb_Face> faces = new List<pb_Face>();
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			int i = sharedIndices.IndexOf(index);

			foreach(pb_Face f in pb.faces)
			{
				if(f.distinctIndices.ContainsMatch((int[])sharedIndices[i]) > -1)
					faces.Add(f);
			}
			return faces;
		}

		public static pb_Face[] GetConnectedFaces(pb_Object pb, int[] indices)
		{
			List<pb_Face> faces = new List<pb_Face>();
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			int[] i = new int[indices.Length];
			for(int j = 0; j < indices.Length; j++)
				i[j] = sharedIndices.IndexOf(indices[j]);

			foreach(pb_Face f in pb.faces)
			{
				foreach(int n in i)
					if(f.distinctIndices.ContainsMatch((int[])sharedIndices[n]) > -1)
						faces.Add(f);
			}

			return faces.Distinct().ToArray();
		}

		// Todo - This does not return unique edges - it contains duplicates
		public static pb_Edge[] GetConnectedEdges(pb_Object pb, int[] indices)
		{
			List<pb_Edge> edges = new List<pb_Edge>();
			pb_IntArray[] sharedIndices = pb.sharedIndices;

			foreach(pb_Edge edge in pb_Edge.AllEdges(pb.faces))
			{
				for(int i = 0; i < indices.Length; i++)
					if(edge.Contains(indices[i], sharedIndices))
						edges.Add(edge);
			}
			return edges.ToArray();
		}

		public static pb_Edge[] GetEdgeRing(pb_Object pb, pb_Edge[] edges)
		{
			List<pb_Edge> usedEdges = new List<pb_Edge>();
			foreach(pb_Edge e in edges)
			{	
				pb_Face origFace;
				pb_Edge origEdge;

				if( !pb.ValidFaceAndEdgeWithEdge(e, out origFace, out origEdge) )
					continue;

				usedEdges.Add(origEdge);

				pb_Face curFace = origFace;
				pb_Edge curEdge = origEdge;
				
				pb_Face opFace;
				pb_Edge opEdge;


				while( GetOppositeEdge(pb, curFace, curEdge, out opFace, out opEdge) )
				{
					curFace = opFace;
					curEdge = opEdge;

					usedEdges.Add(curEdge);
					
					if(opFace == origFace) 
						break;	
				}
			}

			return usedEdges.Distinct().ToArray();
		}

		// public static pb_Edge[] GetEdgeLoop(pb_Object pb, pb_Edge[] edges)
		// {
		// 	List<pb_Edge> ring = new List<pb_Edge>();

		// 	pb_Edge[] orig_uni_edges = pb_Edge.GetUniversalEdges(edges, pb.sharedIndices);
		// 	pb_Edge[] all_uni_edges = pb_Edge.GetUniversalEdges(pb_Edge.AllEdges(pb.faces), pb.sharedIndices);

		// 	Bugger.Log(orig_uni_edges.ToFormattedString("\n"));
		// 	Bugger.Log(all_uni_edges.ToFormattedString("\n"));

		// 	foreach(pb_Edge e in orig_uni_edges)
		// 	{
		// 		ring.Add(e);
		// 		int lasty = e.y;
		// 		bool foundNeighbor = true;

		// 		int n = 0;
		// 		while(foundNeighbor && n < 32)
		// 		{
		// 			foundNeighbor = false;
					
		// 			foreach(pb_Edge ne in all_uni_edges)	
		// 			{
						
		// 				n++;
		// 				if(ne.Equals(e)) continue;

		// 				if(ne.x == lasty)
		// 				{
		// 					Bugger.Log(e + " = " + ne);
							
		// 					lasty = ne.y;
		// 					ring.Add(ne);
		// 					foundNeighbor = true;
		// 					break;
		// 				}
		// 			}
		// 		}
		// 	}
		
		// 	pb_Edge[] tri_ring = ring.Distinct().ToArray();

		// 	for(int i = 0; i < tri_ring.Length; i++)
		// 	{
		// 		tri_ring[i].x = pb.sharedIndices[tri_ring[i].x][0];
		// 		tri_ring[i].y = pb.sharedIndices[tri_ring[i].y][0];
		// 	}

		// 	return tri_ring;
		// }

		public static bool GetOppositeEdge(pb_Object pb, pb_Face face, pb_Edge edge, out pb_Face opposite_face, out pb_Edge opposite_edge)
		{
			opposite_face = null;
			opposite_edge = null;
 
			// Construct a list of all edges starting at vertex edge.y and going around the face.  Then grab the middle edge.
			pb_Edge[] ordered_edges = new pb_Edge[face.edges.Length];
			ordered_edges[0] = edge;

			for(int i = 1; i < face.edges.Length; i++)
			{
				foreach(pb_Edge e in face.edges)
				{
					if(e.x == ordered_edges[i-1].y)
					{
						ordered_edges[i] = e;
						break;
					}
				}
			}

			pb_Edge opEdgeLocal = ordered_edges[face.edges.Length/2];

			List<pb_Face> connectedFaces = pbMeshUtils.GetConnectedFaces(pb, opEdgeLocal);
			connectedFaces.Remove(face);
			opposite_face = connectedFaces[0];
			
			for(int i = 0; i < opposite_face.edges.Length; i++)
			{
				if(opposite_face.edges[i].Equals(opEdgeLocal, pb.sharedIndices))
				{
					opposite_edge = opposite_face.edges[i];
					break;
				}
			}

			return true;
		}
	}
}
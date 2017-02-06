using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class pb_Mesh : MonoBehaviour
{

#region Members

	public Vector3[] points;
	public pb_Face[] faces;
	public pb_Edge[] edges;
#endregion

#region Cached Members

	public Mesh mesh {
		get {
			return GetComponent<MeshFilter>().sharedMesh == null ? new Mesh() : GetComponent<MeshFilter>().sharedMesh;
		}
		set {
			GetComponent<MeshFilter>().sharedMesh = value;
		}
	}
#endregion

	public void ToMesh()
	{
		// materials 
			// hard / soft edges
				// build
		// Dictionary<Material, List<pb_Face>> submeshes = new Dictionary<Material, List<pb_Face>>();
		// foreach(pb_Face face in faces)
		// {
		// 	if(submeshes.ContainsKey(face.material))
		// 		submeshes[face.material].Add(face);
		// 	else
		// 		submeshes.Add(face.material, new List<pb_Face>(1){face});
		// }

		// List<Vector3> v = new List<Vector3>();
		// List<int> t = new List<int>();

		// int[] vertexIncrementCount = new int[points.Length];
		
		// foreach(KeyValuePair<Material, List<pb_Face>> kvp in submeshes)
		// {
			
		// }

		// Mesh m = mesh;
		// m.Clear();
		// m.vertices = points;
		// m.triangles = pb_Face.AllTriangles(faces);
		// m.uv = new Vector2[6] {
		// 	new Vector2(0f, 0f),
		// 	new Vector2(.4f, 0f),
		// 	new Vector2(0f, 1f),
		// 	new Vector2(.4f, 1f),
		// 	new Vector2(1f, 0f),
		// 	new Vector2(1f, 1f)
		// };
		// m.RecalculateNormals();
		// mesh = m;
	}
}
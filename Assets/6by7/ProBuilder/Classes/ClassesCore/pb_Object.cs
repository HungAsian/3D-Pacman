/*
 *	ProBuilder Object
 *	@Karl Henkel, @Gabriel Williams
 *
 */

#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1
#define UNITY_4_3
#elif UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define UNITY_4
#elif UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define UNITY_3
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SixBySeven;
using System.Linq;
using System.Reflection;
using ProBuilder2.Common;
using ProBuilder2.Math;
using ProBuilder2.MeshOperations;

[AddComponentMenu("")]	// Don't let the user add this to any object.
[System.Serializable]

/**
 *	\brief Object class for all ProBuilder geometry.
 */
public class pb_Object : MonoBehaviour
{

#region INITIALIZATION

	/**
	 *	\brief Duplicates and returns the passed pb_Object.
	 *	@param pb The pb_Object to duplicate.
	 *	\returns A unique copy of the passed pb_Object.
	 */
	public static pb_Object InitWithObject(pb_Object pb)
	{
		Vector3[] v = pb.vertices;

		pb_Face[] f = new pb_Face[pb.faces.Length];
		
		for(int i = 0; i < f.Length; i++)
			f[i] = pb.faces[i].DeepCopy();

		pb_Object p = CreateInstanceWithVerticesFaces(v, f);

		p.SetName(pb.GetName());

		return p;
	}

	/**
	 *	\brief Creates a new #pb_Object using passed vertices to construct geometry.
	 *	Typically you would not call this directly, as the #ProBuilder class contains
	 *	a wrapper for this purpose.  In fact, I'm not sure why this is public...
	 *	@param vertices A vertex array (Vector3[]) containing the points to be used in 
	 *	the construction of the #pb_Object.  Vertices must be wound in counter-clockise
	 *	order.  Triangles will be wound in vertex groups of 4, with the winding order
	 *	0,1,2 1,3,2.  Ex: 
	 *	\code{.cs}
	 *	// Creates a pb_Object plane
	 *	pb_Object.CreateInstanceWithPoints(new Vector3[4]{
	 *		new Vector3(-.5f, -.5f, 0f),
	 *		new Vector3(.5f, -.5f, 0f),
	 *		new Vector3(-.5f, .5f, 0f),
	 *		new Vector3(.5f, .5f, 0f)
	 *		});
	 *
	 *	\endcode
	 *	\returns The resulting #pb_Object.
	 */
	public static pb_Object CreateInstanceWithPoints(Vector3[] vertices)
	{
		if(vertices.Length % 4 != 0) {
			Debug.LogWarning("Invalid Geometry.  Make sure vertices in are pairs of 4 (faces).");
			return null;
		}
			
		GameObject _gameObject = new GameObject();	
		pb_Object pb_obj = _gameObject.AddComponent<pb_Object>();
		pb_obj.SetName("MergedObject");

		pb_obj.GeometryWithPoints(vertices);

		pb_obj.gameObject.AddComponent<pb_Entity>().SetEntity(ProBuilder.EntityType.Detail);

		return pb_obj;
	}

	/**
	 *	\brief Creates a new pb_Object with passed vertex array and pb_Face array.  Allows for a great deal of control when constructing geometry.
	 *	@param _vertices The vertex array to use in construction of mesh.
	 *	@param _faces A pb_Face array containing triangle, material per face, and pb_UV parameters for each face.
	 *	\sa pb_Face pb_UV
	 *	\returns The newly created pb_Object.
	 */
	public static pb_Object CreateInstanceWithVerticesFaces(Vector3[] v, pb_Face[] f)
	{
		GameObject _gameObject = new GameObject();	
		pb_Object pb_obj = _gameObject.AddComponent<pb_Object>();
		pb_obj.SetName("MergedObject");

		pb_obj.GeometryWithVerticesFaces(v, f);


		return pb_obj;
	}

	public static pb_Object CreateInstanceWithVerticesFacesSharedIndices(Vector3[] v, pb_Face[] f, pb_IntArray[] si)
	{
		GameObject _gameObject = new GameObject();
		pb_Object pb = _gameObject.AddComponent<pb_Object>();
		pb.SetName("MergedObject");

		pb.gameObject.AddComponent<MeshFilter>();

		pb.GeometryWithVerticesFacesIndices(v, f, si);
		
		pb.gameObject.AddComponent<pb_Entity>().SetEntity(ProBuilder.EntityType.Detail);

		return pb;
	}
#endregion

#region EVENTS
	
	public delegate void OnVertexMovementEventHandler(pb_Object pb);
	public static event OnVertexMovementEventHandler OnVertexMovement;

	public delegate void OnRefreshEventHandler(pb_Object pb);
	public static event OnRefreshEventHandler OnRefresh;
#endregion

#region INTERNAL MEMBERS

	[SerializeField]
	private pb_Face[]		 			_quads;
	private pb_Face[]					_faces { get { return _quads; } }// set { _quads = value; } }
	[SerializeField]
	private ProBuilder.Shape		 	_shape;
	[SerializeField]
	private pb_IntArray[] 				_sharedIndices;
	[SerializeField]
	private int[]						_uniqueIndices; 	// A collection of unique indices
	[SerializeField]
	private Vector3[] 					_vertices;

	// UV2 generation params
	public float angleError = 8f;
	public float areaError = 15f;
	public float hardAngle = 88f;
	public float packMargin = 20f;

	// User settings
	static bool							snapEnabled { get { return SixBySeven.Shared.snapEnabled; } }	///< Use ProGrids snap values?
	static float						snapValue { get { return SixBySeven.Shared.snapValue; } }	///< Snap value for vertices and faces.
	
	public pb_Face[]					SelectedFaces { get { return m_selectedFaces; } }
	public int[]						SelectedTriangles { get { return m_selectedTriangles; } }
	public pb_Edge[]					SelectedEdges { get { return m_SelectedEdges; } }

	[SerializeField] private pb_Face[]	m_selectedFaces 		= new pb_Face[]{};
	[SerializeField] private pb_Edge[]	m_SelectedEdges 		= new pb_Edge[]{};
	[SerializeField] private int[]		m_selectedTriangles 	= new int[]{};

	public Vector3 						previousTransform = new Vector3(0f, 0f, 0f);

	public bool 						isSelectable = true;	///< Optional flag - if true editor should ignore clicks on this object.

	[SerializeField]
	private string _name = "Object";
#endregion

#region ACCESS
	
	public Mesh msh
	{
		get
		{
			if(!GetComponent<MeshFilter>())
				return null;

			return GetComponent<MeshFilter>().sharedMesh;
		}
		set 
		{ 	
			if(gameObject.GetComponent<MeshFilter>() != null)
					gameObject.GetComponent<MeshFilter>().sharedMesh = value;
				else
					gameObject.AddComponent<MeshFilter>().sharedMesh = value;
		}
	}

	public pb_Face[] faces { get { return _quads; } }// == null ? Extractfaces(msh) : _faces; } }
	public pb_Face[] quads {get { Debug.LogWarning("pb_Quad is deprecated.  Please use pb_Face instead."); return _quads; } }

	public pb_IntArray[] 	sharedIndices { get { return _sharedIndices; } }

	public int[] uniqueIndices { get { return _uniqueIndices; } }
	public int id { get { return gameObject.GetInstanceID(); } }
	public void SetShapeType(ProBuilder.Shape shape) { _shape = shape;	}
	public ProBuilder.EntityType entityType { get { return GetComponent<pb_Entity>() == null ? 
		ProBuilder.EntityType.Detail : GetComponent<pb_Entity>().entityType; } }

	public Vector3[] vertices { get { return _vertices; } }
	public int vertexCount { get { return _vertices.Length; } }

	/**
	 *	\brief Refreshes the gameObject name with most recent information.
	 *	\sa SetName
	 */
	public void RefreshName()
	{
		SetName(_name);
	}

	/**
	 *	\returns Returns the human readable object name.
	 */
	public string GetName()
	{
		return _name;
	}

	// TODO
	/**
	 *	\brief Gets all vertices in local space from face.
	 *	@param _face The #pb_Face to draw vertices from.
	 *	\returns A Vector3[] array containing all vertices contained within a #pb_Face.
	 *	\sa #pb_Face #GetTriangles #GetNormals #GetTriangle #GetMaterial
	 */
	public Vector3[] GetVertices(pb_Face face)
	{
		return face.GetVertices(_vertices);
	}

	// /**
	//  * \brief Gets triangles for specified pb_Face. 
	//  * @param face The face to retrieve triangles for.
	//  * \returns All triangles contained within face.  Use #DistinctTriangles to retrieve distinct tris.
	//  */
	public int[] GetTriangles(pb_Face face)
	{
		return face.indices;
	}

	/**
	 * \brief Gets vertex normals for the selected face. 
	 * @param face
	 * \returns Vector3[] containing all normals for a face.
	 */
	public Vector3[] GetNormals(pb_Face face)
	{
		// muhahaha
		return face.GetVertices(msh.normals);
	}

	/**
	 * \brief Returns the triangle at the specified index within the #pb_Face index array.
	 * @param index The index that the triangle beings at.
	 * @param face The #pb_Face to query.
	 * \returns A vertex array in local space.
	 * \notes ...I'm not sure why this exists...
	 */
	public Vector3[] GetTriangle(int index, pb_Face quad)
	{
		return quad.GetTriangle(index, _vertices);//msh.vertices);
	}

	/**
	 * \brief Returns the material property of the specified #pb_Face. 
	 * \returns Returns the material property of the specified #pb_Face. 
	 * @param face The face to extract material data from.
	 */
	public Material GetMaterial(pb_Face face)
	{
		return face.material;
	}

	/**
	 *	\brief Returns a copy of the sharedIndices array.
	 */
	public pb_IntArray[] GetSharedIndices()
	{
		int sil = _sharedIndices.Length;
		pb_IntArray[] sharedIndicesCopy = new pb_IntArray[sil];
		for(int i = 0; i < sil; i++)
		{
			int[] arr = new int[_sharedIndices[i].Length];
			System.Array.Copy(_sharedIndices[i].array, arr, arr.Length);
			sharedIndicesCopy[i] = new pb_IntArray(arr);
		}

		return sharedIndicesCopy;
	}
#endregion

#region SELECTION

	/**
	 *	Adds a face to this pb_Object's selected array.  Also updates the SelectedEdges and SelectedTriangles arrays.
	 */
	public void AddToFaceSelection(pb_Face face)
	{
		SetSelectedFaces(m_selectedFaces.Add(face));
	}

	/**
	 *	Sets this pb_Object's SelectedFaces array, as well as SelectedEdges and SelectedTriangles.
	 */
	public void SetSelectedFaces(pb_Face[] t_faces)
	{
		this.m_selectedFaces = t_faces;
		this.m_SelectedEdges = pb_Edge.AllEdges(m_selectedFaces);
		this.m_selectedTriangles = sharedIndices.UniqueIndicesWithValues(pb_Face.AllTrianglesDistinct(m_selectedFaces));
	}

	public void SetSelectedEdges(pb_Edge[] edges)
	{
		this.m_selectedFaces = new pb_Face[0];
		this.m_SelectedEdges = edges;
		this.m_selectedTriangles = sharedIndices.UniqueIndicesWithValues(m_SelectedEdges.ToIntArray());				
	}

	/**
	 *	Sets this pb_Object's SelectedTriangles array.  Clears SelectedFaces and SelectedEdges arrays.
	 */
	public void SetSelectedTriangles(int[] tris)
	{
		m_selectedFaces = new pb_Face[0];
		m_SelectedEdges = new pb_Edge[0];
		m_selectedTriangles = sharedIndices.UniqueIndicesWithValues(tris);
	}

	/**
	 *	Removes face at index in SelectedFaces array, and updates the SelectedTriangles and SelectedEdges arrays to match.
	 */
	public void RemoveFromFaceSelectionAtIndex(int index)
	{
		SetSelectedFaces(m_selectedFaces.RemoveAt(index));
	}

	/**
	 *	Removes face from SelectedFaces array, and updates the SelectedTriangles and SelectedEdges arrays to match.
	 */
	public void RemoveFromFaceSelection(pb_Face face)
	{		
		SetSelectedFaces(m_selectedFaces.Remove(face));
	}

	/**
	 *	Clears SelectedFaces, SelectedEdges, and SelectedTriangle arrays.  You do not need to call this when setting an individual array, as the setter methods will handle updating the associated caches.
	 */
	public void ClearSelection()
	{
		m_selectedFaces = new pb_Face[0];
		m_SelectedEdges = new pb_Edge[0];
		m_selectedTriangles = new int[0];
	}
#endregion

#region SET

	/**
	 *	\brief Sets the #pb_Object name that is shown in the hierarchy.
	 *	@param __name The name to apply.  Format is pb-Name[#pb_EntityType]-id
	 *	\sa RefreshName
	 */
	public void SetName(string __name)
	{
		_name = __name;
		gameObject.name = "pb-" + _name + "[" + entityType + "]" + id;
	}

	public void SetVertices(Vector3[] v)
	{
		_vertices = v;
		msh.vertices = v;
	}

	public void SetSharedIndices(pb_IntArray[] si)
	{
		_sharedIndices = si;
		RefreshUniqueIndices();
	}

	/**
	 *	\brief Given a triangle index, locate buddy indices and move all vertices to this new position
	 */
	public void SetSharedVertexPosition(int index, Vector3 position)
	{
		int sharedIndicesIndex = _sharedIndices.IndexOf(index);
		foreach(int n in _sharedIndices[sharedIndicesIndex].array)
		{
			_vertices[n] = position;
		}	
		msh.vertices = _vertices;
	}
#endregion

#region SHARED INDEX OPERATIONS

	/**
	 *	Associates all passed indices with a single shared index.  Does not perfrom any additional operations 
	 *	to repair triangle structure or vertex placement.
	 */
	public int MergeSharedIndices(int[] indices)
	{
		if(indices.Length < 2) return -1;

		List<int> used = new List<int>();
		List<int> newSharedIndex = new List<int>();

		// Create a new int[] composed of all indices in shared selection
		for(int i = 0; i < indices.Length; i++)
		{
			int si = _sharedIndices.IndexOf(indices[i]);
			if(!used.Contains(si))
			{
				newSharedIndex.AddRange(_sharedIndices[si].array);
				used.Add(si);
			}
		}

		// Now remove the old entries
		int rebuiltSharedIndexLength = _sharedIndices.Length - used.Count;
		pb_IntArray[] rebuild = new pb_IntArray[rebuiltSharedIndexLength];
		
		int n = 0;
		for(int i = 0; i < _sharedIndices.Length; i++)
		{
			if(!used.Contains(i))
				rebuild[n++] = _sharedIndices[i];
		}

		SetSharedIndices( rebuild.Add( new pb_IntArray(newSharedIndex.ToArray()) ) );

		return _sharedIndices.Length-1;
	}

	/**
	 *	Associates indices with a single shared index.  Does not perfrom any additional operations 
	 *	to repair triangle structure or vertex placement.
	 */
	public void MergeSharedIndices(int a, int b)
	{
		int aIndex = _sharedIndices.IndexOf(a);
		int oldBIndex = _sharedIndices.IndexOf(b);
		AddIndexToSharedIndexArray(aIndex, b);

		int[] arr = _sharedIndices[oldBIndex].array;
		_sharedIndices[oldBIndex].array = arr.RemoveAt(System.Array.IndexOf(arr, b));
		pb_IntArray.RemoveEmptyOrNull(ref _sharedIndices);
		RefreshUniqueIndices();
	}

	// Remove the index and put it in it's own entry
	// public void SplitIndex(int a)
	// {
	// 	RemoveIndexFromSharedArray(a);
	// 	AddIndexToSharedIndexArray(-1, a);
	// }

	/**
	 *	\brief Cycles through the vertices and places near vertices in
	 *	a shared index group.  Writes directly to the internal _sharedIndices
	 *	property.  This is different from ExtractSharedIndices in that it allows
	 *	for a delta to be set, and applies it's changes automatically.
	 */
	public void MergeVertices(float delta)
	{
		Dictionary<Vector3, List<int>> pointIndex = new Dictionary<Vector3, List<int>>();

		for(int i = 0; i < _vertices.Length; i++)
		{
			bool foundMatch = false;
			foreach(KeyValuePair<Vector3, List<int>> kvp in pointIndex)
			{
				if( _vertices[i].EqualWithError(kvp.Key, delta) )
				{
					foundMatch = true;
					kvp.Value.Add(i);
					break;
				}
			}

			if(!foundMatch)
			{
				pointIndex.Add(_vertices[i], new List<int>() { i });
			}
		}

		int n = pointIndex.Count;
		pb_IntArray[] rebuiltSharedIndex = new pb_IntArray[n];
		n = 0;
	
		foreach(KeyValuePair<Vector3, List<int>> kvp in pointIndex)
			rebuiltSharedIndex[n++] = new pb_IntArray(kvp.Value.ToArray());

		SetSharedIndices(rebuiltSharedIndex);
	}

	public int AddIndexToSharedIndexArray(int sharedIndex, int triangleIndexToAdd)
	{
		if(sharedIndex > -1)
			_sharedIndices[sharedIndex].array = _sharedIndices[sharedIndex].array.Add(triangleIndexToAdd);
		else
			_sharedIndices = (pb_IntArray[])_sharedIndices.Add( new pb_IntArray(new int[]{triangleIndexToAdd}) );

		RefreshUniqueIndices();
		
		return sharedIndex > -1 ? sharedIndex : _sharedIndices.Length-1;
	}

	private int AddRangeToSharedIndexArray(int sharedIndex, int[] indices)
	{
		if(sharedIndex > -1)
			_sharedIndices[sharedIndex].array = _sharedIndices[sharedIndex].array.AddRange(indices);
		else
			_sharedIndices = (pb_IntArray[])_sharedIndices.Add( new pb_IntArray(indices) );

		RefreshUniqueIndices();
		
		return sharedIndex > -1 ? sharedIndex : _sharedIndices.Length-1;
	}

	private void RemoveIndexFromSharedArray(int remove)
	{
		int index = _sharedIndices.IndexOf(remove);
		Debug.Log("Removing: " + remove + " Index: " + index);
		if(index < 0) return;
		
		int[] arr = _sharedIndices[index].array;
		_sharedIndices[index].array = arr.RemoveAt(System.Array.IndexOf(arr, remove));
		
		pb_IntArray.RemoveEmptyOrNull(ref _sharedIndices);
		RefreshUniqueIndices();
	}

	public void RemoveIndicesFromSharedArray(int[] remove)
	{
		// remove face indices from all shared indices caches
		for(int i = 0; i < _sharedIndices.Length; i++)
		{
			for(int n = 0; n < remove.Length; n++)
			{
				int ind = System.Array.IndexOf(_sharedIndices[i], remove[n]);

				if(ind > -1)
					_sharedIndices[i].array = _sharedIndices[i].array.RemoveAt(ind);
			}
		}

		// Remove empty or null entries caused by shifting around all them indices
		pb_IntArray.RemoveEmptyOrNull(ref _sharedIndices);
		RefreshUniqueIndices();
	}

	/**
	 *	\brief Removes the specified indices from the array, and shifts all values 
	 *	down to account for removal in the vertex array.  Only use when deleting
	 *	faces or vertices.  For general moving around and modification of shared 
	 *	index array, use RemvoeIndicesFromSharedArray()
	 */
	private void RemoveAndShiftIndicesFromSharedArray(int[] remove)
	{
		// MUST BE DISTINCT
		remove = remove.ToDistinctArray();

		// remove face indices from all shared indices caches
		for(int i = 0; i < _sharedIndices.Length; i++)
		{
			for(int n = 0; n < remove.Length; n++)
			{
				int ind = System.Array.IndexOf(_sharedIndices[i], remove[n]);

				if(ind > -1)
					_sharedIndices[i].array = _sharedIndices[i].array.RemoveAt(ind);
			}
		}

		// Remove empty or null entries caused by shifting around all them indices
		pb_IntArray.RemoveEmptyOrNull(ref _sharedIndices);
		
		// now cycle through and shift indices
		for(int i = 0; i < _sharedIndices.Length; i++)
		{
			for(int n = 0; n < _sharedIndices[i].Length; n++)
			{
				int ind = _sharedIndices[i][n];
				int sub = 0;

				// use a count and subtract at end because indices aren't guaranteed to be in order.
				// ex, 9, 8, 7 would only sub 1 if we just did rm < ind; ind--
				foreach(int rm in remove)
				{
					if(rm < ind)
						sub++;
				}
				_sharedIndices[i][n] -= sub;
			}
		}

		RefreshUniqueIndices();
	}

	/**
	 *	\brief Removes the specified index from the array, and shifts all values 
	 *	down to account for removal in the vertex array.  Only use when deleting
	 *	faces or vertices.  For general moving around and modification of shared 
	 *	index array, use RemoveIndexFromSharedArray()
	 */
	public void RemoveAndShiftIndexFromSharedArray(int index)
	{
		int si = _sharedIndices.IndexOf(index);
		if(si > -1)
			_sharedIndices[si].array = _sharedIndices[si].array.Remove(index);

		pb_IntArray.RemoveEmptyOrNull(ref _sharedIndices);

		for(int i = 0; i < _sharedIndices.Length; i++)
		{
			for(int n = 0; n < _sharedIndices[i].Length; n++)
			{
				if(index < _sharedIndices[i][n])
					_sharedIndices[i][n]--;
			}
		}
		RefreshUniqueIndices();
	}
#endregion

#region FACE WRANGLING

	/**
	 *	\brief Given a pb_Face, return the center point in object space.  See also #VerticesCenter.
	 *	@param face The face to extract center point from.
	 *	\return A point in object space.
	 */
	public Vector3 FaceCenter(pb_Face face)
	{
		Vector3 cen = Vector3.zero;
		foreach(int i in face.distinctIndices)
			cen += _vertices[i];
		return cen / (float)face.distinctIndices.Length;
	}

	public Vector3 FaceCenter(pb_Face[] faces)
	{
		Vector3 cen = Vector3.zero;
		int[] allTris = pb_Face.AllTriangles(faces);
		foreach(int i in allTris)
			cen += _vertices[i];
		return cen / (float) allTris.Length;
	}

	public Vector3 FaceNormal(pb_Face q)
	{
		return pb_Math.PlaneNormal(q.GetVertices(_vertices));
	}

	/**
	 * \brief Returns a #pb_Face which contains the passed triangle. 
	 * @param tri int[] composed of three indices.
	 * \returns #pb_Face if match is found, null if not.
	 */
	public pb_Face FaceWithTriangle(int[] tri)
	{
		for(int i = 0; i < faces.Length; i++)
		{
			if(	faces[i].Contains(tri) )
				return faces[i];
		}

		return null;
	}

	/**
	 * The SelectedEdges array contains Edges made up of indices that aren't guaranteed to be 'valid' - that is, they
	 * may not belong to the same face.  This method extracts an edge and face combo from the face independent edge
	 * selection.
	 */
	public bool ValidFaceAndEdgeWithEdge(pb_Edge faceIndependentEdge, out pb_Face face, out pb_Edge edge)
	{
		edge = null;
		face = null;
		foreach(pb_Face f in faces)
		{
			int ind = f.edges.IndexOf(faceIndependentEdge, _sharedIndices);
			if(ind > -1)
			{
				face = f;
				edge = f.edges[ind];
				return true;
			}
		}

		return false;
	}

	/**
	 * \brief Get the passed pb_Face index in the #faces array. 
	 * @param face The face to search against.
	 * \returns The index of the face in the #faces array.  -1 if not found.
	 */
	public int FaceIndex(pb_Face face)
	{
		for(int i = 0; i < faces.Length; i++)
		{
			if(faces[i] == face)
				return i;
		}
		return -1;
	}

	/**
	 * \brief Accepts a List<pb_Face> and returns a jagged int array containing the shared indices, composed exclusively of the selected faces.  For the opposite (inclusive), #SharedTrianglesWithTriangles will return one dimensional int array with every shared index for that pb_Object.
	 * @param faces The faces to extract triangle from.
	 * \return Returns a jagged int array.  Unique Vertex[ Shared Triangles[] ]
	 */
	public int[][] SharedTrianglesExclusive(List<pb_Face> faces)
	{
		List<int> distinct = DistinctTriangles(faces);
		Dictionary<int, List<int>> shared = new Dictionary<int, List<int>>();
		int i = 0;
		for(i = 0; i < distinct.Count; i++)
		{
			int sharedIndex = sharedIndices.IndexOf(distinct[i]);
			
			if(shared.ContainsKey(sharedIndex))
				shared[sharedIndex].Add(distinct[i]);
			else
				shared.Add(sharedIndex, new List<int>(){distinct[i]});
		}

		int[][] jarr = new int[shared.Count][];
		i = 0;
		foreach(KeyValuePair<int, List<int>> kvp in shared)
		{
			jarr[i++] = kvp.Value.ToArray();
		}

		return jarr;
	}

	public int[][] SharedTrianglesExclusive(pb_Edge[] edges)
	{
		int[] distinct = edges.ToIntArray();
		Dictionary<int, List<int>> shared = new Dictionary<int, List<int>>();
		int i = 0;
		for(i = 0; i < distinct.Length; i++)
		{
			int sharedIndex = sharedIndices.IndexOf(distinct[i]);
			
			if(shared.ContainsKey(sharedIndex))
				shared[sharedIndex].Add(distinct[i]);
			else
				shared.Add(sharedIndex, new List<int>(){distinct[i]});
		}

		int[][] jarr = new int[shared.Count][];
		i = 0;
		foreach(KeyValuePair<int, List<int>> kvp in shared)
		{
			jarr[i++] = kvp.Value.ToArray();
		}

		return jarr;
	}

	/**
	 *	\brief Returns an int[] containing 'unique' triangles.  Unique triangles means that if two triangles point to two different vertex indices, but those vertices share a world point, only on triangle will be returned.
	 *	@param faces #pb_Face array to extract triangle data from.
	 *	\returns An int[] of 'distinct' triangles.
	 */
	// todo - this is actually returning indices
	public int[] DistinctTriangles(pb_Face[] faces)
	{
		List<int> di = new List<int>();
		for(int i = 0; i < faces.Length; i++)
			di.AddRange(faces[i].distinctIndices);

		return di.ToArray();
	}

	/**
	 *	\brief Returns a List<int> containing 'unique' triangles.  Unique triangles means that if two triangles point to two different vertex indices, but those vertices share a world point, only on triangle will be returned.
	 *	@param faces #pb_Face array to extract triangle data from.
	 *	\returns A List<int> of 'distinct' triangles.
	 */
	public List<int> DistinctTriangles(List<pb_Face> faces)
	{
		List<int> di = new List<int>();
		for(int i = 0; i < faces.Count; i++)
		{
			di.AddRange(faces[i].distinctIndices);
		}
		return di;
	}

	/**
	 *	\brief Set the internal face array with the passed pb_Face array.
	 *	@param faces New pb_Face[] containing face data.  Mesh triangle data is extracted from the internal #pb_Face array, so be sure to account for all triangles.
	 */
	public void SetFaces(pb_Face[] _qds)
	{
		_quads = _qds;
	}
#endregion

#region CONSTANT

	public static Vector3[] VERTICES_CUBE = new Vector3[] {
		// bottom 4 verts
		new Vector3(-.5f, -.5f, .5f),		// 0	
		new Vector3(.5f, -.5f, .5f),		// 1
		new Vector3(.5f, -.5f, -.5f),		// 2
		new Vector3(-.5f, -.5f, -.5f),		// 3

		// top 4 verts
		new Vector3(-.5f, .5f, .5f),		// 4
		new Vector3(.5f, .5f, .5f),			// 5
		new Vector3(.5f, .5f, -.5f),		// 6
		new Vector3(-.5f, .5f, -.5f)		// 7
	};

	public static int[] TRIANGLES_CUBE = new int[] {
		0, 1, 4, 5, 1, 2, 5, 6, 2, 3, 6, 7, 3, 0, 7, 4, 4, 5, 7, 6, 3, 2, 0, 1
	};
#endregion

#region MESH INITIALIZATION

	private void GeometryWithPoints(Vector3[] v)
	{
		// Wrap in faces		
		int[] t = new int[ (v.Length/4) * 6];
		int c = 0, i = 0;
		for(i = 0; i < v.Length; i+=4)
		{
			t[c+0] = i+0;
			t[c+1] = i+1;
			t[c+2] = i+2;

			t[c+3] = i+1;
			t[c+4] = i+3;
			t[c+5] = i+2;

			c+=6;
		}
		Mesh m = new Mesh();
		m.name = "pb_Mesh" + gameObject.GetInstanceID();

		m.vertices = v;
		m.triangles = t;

		m.uv = new Vector2[v.Length];
		m.uv2 = new Vector2[v.Length];
		;
		/* Create Mesh Filter, add mesh */
		gameObject.AddComponent<MeshFilter>().sharedMesh = m;

		/* Extract Quad Data */
		_quads = ExtractFaces(m);
		_vertices = v;
		RefreshUV();		RefreshNormals();

		/* Generate Tangents *after* normals, uvs, and vertices */		
		RefreshTangent();
		
		_sharedIndices = ExtractSharedIndices();	
		RefreshUniqueIndices();

		/* Add Materials */
		gameObject.AddComponent<MeshRenderer>().sharedMaterial = ProBuilder.DefaultMaterial;

		/* Set probuilder stuff type */
		_shape = ProBuilder.Shape.Cube;
	}

	/**
	 *	\brief Rebuilds the sharedIndex array and uniqueIndex array each time
	 *	called.
	 */
	private void GeometryWithVerticesFaces(Vector3[] v, pb_Face[] f)
	{
		Mesh m = new Mesh();
		if(msh != null)
			m.name = msh.name;
		m.vertices = v;
		m.triangles = pb_Face.AllTriangles(f);
		m.uv = new Vector2[v.Length];
		m.uv2 = new Vector2[v.Length];

		;
		m.RecalculateBounds();

		if(msh != null)
			DestroyImmediate(msh);

		msh = m;

		if(!gameObject.GetComponent<MeshRenderer>())
			gameObject.AddComponent<MeshRenderer>().sharedMaterial = ProBuilder.DefaultMaterial;
		else
			gameObject.GetComponent<MeshRenderer>().sharedMaterial = ProBuilder.DefaultMaterial;
		
		SetFaces( f );

		_vertices = v;
		_sharedIndices = ExtractSharedIndices();
		RefreshUniqueIndices();
		

		ToMesh();

		_shape = ProBuilder.Shape.Custom;
	}

	private void GeometryWithVerticesFacesIndices(Vector3[] v, pb_Face[] f, pb_IntArray[] s)
	{
		Mesh m = new Mesh();
		if(msh != null)
			m.name = msh.name;

		m.vertices = v;
		m.triangles = pb_Face.AllTriangles(f);
		m.uv = new Vector2[v.Length];
		m.uv2 = new Vector2[v.Length];

		;
		m.RecalculateBounds();

		if(msh != null)
			DestroyImmediate(msh);

		msh = m;

		if(!gameObject.GetComponent<MeshRenderer>())
			gameObject.AddComponent<MeshRenderer>().sharedMaterial = ProBuilder.DefaultMaterial;
		else
			gameObject.GetComponent<MeshRenderer>().sharedMaterial = ProBuilder.DefaultMaterial;
		
		SetFaces( f );

		_vertices = v;
		_sharedIndices = s;
		RefreshUniqueIndices();

		ToMesh();

		_shape = ProBuilder.Shape.Custom;
	}

	public bool ReconstructMesh()
	{
		if(msh != null)
			DestroyImmediate(msh);

		if(_vertices == null)
		{
			msh = null;
			return false;
		}

		Mesh m = new Mesh();
		m.name = "pb_Mesh" + id;
		m.vertices = _vertices;
		m.triangles = pb_Face.AllTriangles(_faces);
		m.uv = new Vector2[_vertices.Length];
		m.uv2 = new Vector2[_vertices.Length];

		;
		m.RecalculateBounds();
		
		msh = m;

		ToMesh();
		
		return true;
	}
#endregion

#region MESH MODIFY
	
	/**
	 *	\brief Use this for moving vertices.  Arguments are selected indices (distinct), and the offset to apply.
	 *	@param selectedTrianglesDistinct Triangles to apply the offset to.  Must be distinct.
	 *	@param offset Offset in meters to apply.
	 *	\notes This method also applies a snap value if one is set.  Snaps vertices in world space, not locally.
	 * 	\sa #SharedTrianglesWithTriangles #RemoveDuplicateVertexIndices
	 */
	public void TranslateVertices(int[] selectedTrianglesDistinct, Vector3 offset)
	{
		TranslateVertices(selectedTrianglesDistinct, offset, false);
	}

	public void TranslateVertices(int[] selectedTrianglesDistinct, Vector3 offset, bool forceDisableSnap)
	{	
		Vector3 orig = offset;
		if(transform.localScale != Vector3.one)
			FreezeScaleTransform();
			
		int i = 0;
		int[] indices = SharedTrianglesWithTriangles( selectedTrianglesDistinct );

		offset = gameObject.transform.InverseTransformDirection(offset);

		Vector3[] verts = vertices;
		for(i = 0; i < indices.Length; i++)
			verts[indices[i]] += offset;
		
		// Snaps to world grid
		if(SixBySeven.Shared.snapEnabled && !forceDisableSnap)
			for(i = 0; i < indices.Length; i++)
			{
				if(SixBySeven.Shared.useAxisConstraints)
					verts[indices[i]] = transform.InverseTransformPoint(pbUtil.SnapValue(transform.TransformPoint(verts[indices[i]]), orig /*mask*/, snapValue));
				else
					verts[indices[i]] = transform.InverseTransformPoint(pbUtil.SnapValue(transform.TransformPoint(verts[indices[i]]), Vector3.one, snapValue));
			}

		SetVertices(verts);

		if(OnVertexMovement != null)
			OnVertexMovement(this);
	}

	/**
	 *	\brief Scale vertices and set transform.localScale to Vector3.one.
	 */
	public void FreezeScaleTransform()
	{
		Vector3[] v = msh.vertices;
		for(int i = 0; i < v.Length; i++)
			v[i] = Vector3.Scale(v[i], transform.localScale);

		SetVertices(v);
		transform.localScale = new Vector3(1f, 1f, 1f);
		Refresh();
	}

	/**
	 * \brief Flips the winding order for the entire mesh. 
	 */
	public void ReverseWindingOrder()
	{
		for(int i = 0; i < faces.Length; i++)
			faces[i].ReverseIndices();
	
		ToMesh();
		Refresh();
	}	

	/**
	 *	\brief Reverse the winding order for each passed #pb_Face.
	 *	@param faces The faces to apply normal flippin' to.
	 *	\returns Nothing.  No soup for you.
	 *	\sa SelectedFaces pb_Face
	 */
	public void ReverseWindingOrder(pb_Face[] faces)
	{
		for(int i = 0; i < faces.Length; i++)
			faces[i].ReverseIndices();

		ToMesh();
		Refresh();
	}	

	public void CenterPivot(int[] indices)
	{	
		Vector3[] verts = VerticesInWorldSpace(indices == null ? uniqueIndices : indices);

		Vector3 center = Vector3.zero;
		foreach (Vector3 v in verts)
			center += v;
	
		center /= (float)verts.Length;

		Vector3 dir = (transform.position - center);

		transform.position = center;

		// the last bool param force disables snapping vertices
		TranslateVertices(uniqueIndices, dir, true);

		Refresh();
	}

	public void DetachFace(pb_Face face)
	{
		RemoveIndicesFromSharedArray(face.indices);

		// Add these vertices back into the sharedIndices array under it's own entry
		for(int i = 0; i < face.distinctIndices.Length; i++)
		{			
			int[] arr = new int[1] { face.distinctIndices[i] };
			_sharedIndices = pbUtil.Add(_sharedIndices, new pb_IntArray(arr));
		}
		RefreshUniqueIndices();
	}

	/**
	 *	Removes the passed face from this pb_Object.  Handles shifting vertices and triangles, as well as messing with the sharedIndices cache.
	 */
	public void DeleteFace(pb_Face face)
	{	
		int f_ind = System.Array.IndexOf(_quads, face);
		int[] distInd = face.distinctIndices;
		
		Vector3[] verts = _vertices.RemoveAt(distInd);
		pb_Face[] nFaces = _faces.RemoveAt(f_ind);

		// shift all other face indices down to account for moved vertex positions
		for(int i = 0; i < nFaces.Length; i++)
		{
			int[] tris = nFaces[i].indices;
			for(int n = 0; n < tris.Length; n++)
			{
				int sub = 0;
				for(int d = 0; d < distInd.Length; d++)
				{
					if(tris[n] > distInd[d])
						sub++;
				}
				tris[n] -= sub;
			}
			nFaces[i].SetIndices(tris);
		}

		// shift all other face indices in the shared index array down to account for moved vertex positions
		RemoveAndShiftIndicesFromSharedArray(distInd);
		
		_vertices = verts;
		SetFaces(nFaces);
		RebuildFaceCaches();
		ToMesh();
	}

	public void DeleteFaces(pb_Face[] faces)
	{	
		int[] f_ind = new int[faces.Length];
		for(int i = 0; i < faces.Length; i++)
			f_ind[i] = System.Array.IndexOf(_quads, faces[i]);
		
		int[] distInd = pb_Face.AllTrianglesDistinct(faces);

		Vector3[] verts = _vertices.RemoveAt(distInd);
		pb_Face[] nFaces = _faces.RemoveAt(f_ind);

		// shift all other face indices down to account for moved vertex positions
		for(int i = 0; i < nFaces.Length; i++)
		{
			int[] tris = nFaces[i].indices;
			for(int n = 0; n < tris.Length; n++)
			{
				int sub = 0;
				for(int d = 0; d < distInd.Length; d++)
				{
					if(tris[n] > distInd[d])
						sub++;
				}
				tris[n] -= sub;
			}
			nFaces[i].SetIndices(tris);
		}

		// shift all other face indices in the shared index array down to account for moved vertex positions
		RemoveAndShiftIndicesFromSharedArray(distInd);
		
		_vertices = verts;
		SetFaces(nFaces);
		RebuildFaceCaches();
		ToMesh();
	}

	public int[] RemoveUnusedVertices()
	{
		List<int> del = new List<int>();
		int[] tris = pb_Face.AllTriangles(faces);

		for(int i = 0; i < _vertices.Length; i++)
			if(!tris.Contains(i))
				del.Add(i);
		
		DeleteIndices(del.ToArray());
		return del.ToArray();
	}

	/**
	 *	Deletes the vertcies from the passed index array.  Does not modify face index caches.
	 */
	private void DeleteIndices(int[] distInd)
	{
		Vector3[] verts = msh.vertices;

		verts = verts.RemoveAt(distInd);
		pb_Face[] nFaces = faces;

		// shift all other face indices down to account for moved vertex positions
		for(int i = 0; i < nFaces.Length; i++)
		{
			int[] tris = nFaces[i].indices;
			for(int n = 0; n < tris.Length; n++)
			{
				int sub = 0;
				for(int d = 0; d < distInd.Length; d++)
				{
					if(tris[n] > distInd[d])
						sub++;
				}
				tris[n] -= sub;
			}
			nFaces[i].SetIndices(tris);
		}

		// shift all other face indices in the shared index array down to account for moved vertex positions
		RemoveAndShiftIndicesFromSharedArray(distInd);
		
		int vc = verts.Length;

		Mesh m = new Mesh();

		m.vertices = verts;
		m.triangles = pb_Face.AllTriangles(nFaces);
		m.normals = new Vector3[vc];
		m.tangents = new Vector4[vc];
		m.uv = new Vector2[vc];
		m.uv2 = new Vector2[vc];
		
		;
		m.RecalculateBounds();

		msh = m;

		_vertices = verts;

		RebuildFaceCaches();

		ToMesh();	
	}

	/**
	 *	\brief
	 *	param sharedIndex An optional array that sets the new pb_Face indices to use the _sharedIndices array.
	 *	\returns The newly appended pb_Face.
	 */
	public pb_Face AppendFace(Vector3[] v, pb_Face face)
	{
		int[] shared = new int[v.Length];
		for(int i = 0; i < v.Length; i++)
			shared[i] = -1;
		return AppendFace(v, face, shared);
	}
	
	public pb_Face AppendFace(Vector3[] v, pb_Face face, int[] sharedIndex)
	{
		List<Vector3> _verts = new List<Vector3>(_vertices);
		List<pb_Face> _faces = new List<pb_Face>(faces);

		_verts.AddRange(v);
		face.ShiftIndicesToZero();
		face.ShiftIndices(vertexCount);
		_faces.Add(face);

		// Dictionary<int, int> grp = new Dictionary<int, int>();	// this allows append face to add new vertices to a new shared index group
		// 														// if the sharedIndex is negative and less than -1, it will create new gorup
		// 														// that other sharedIndex members can then append themselves to.
		for(int i = 0; i < sharedIndex.Length; i++)
		{
			// if(sharedIndex[i] < -1)
			// {
			// 	if(grp.ContainsKey(sharedIndex[i]))
			// 		AddIndexToSharedIndexArray(grp[sharedIndex[i]], i+vertexCount);
			// 	else
			// 		grp.Add(sharedIndex[i], AddIndexToSharedIndexArray(sharedIndex[i], i+vertexCount));
			// }
			// else
				AddIndexToSharedIndexArray(sharedIndex[i], i+vertexCount);
		}

		///* reconstruct mesh
		int vc = _verts.Count;

		if(msh == null) msh = new Mesh();

		msh.vertices = _verts.ToArray();
		msh.triangles = pb_Face.AllTriangles(_faces);
		msh.normals = new Vector3[vc];
		msh.tangents = new Vector4[vc];
		msh.colors32 = pb_Face.Color32ArrayWithFaces(_faces.ToArray(), vc);
		
		;
		msh.RecalculateBounds();

		_vertices = _verts.ToArray();
		SetFaces(_faces.ToArray());

		ToMesh();

		return face;
	}

	public pb_Face[] AppendFaces(Vector3[][] new_Vertices, pb_Face[] new_Faces, int[][] new_SharedIndices)
	{
		List<Vector3> _verts = new List<Vector3>(this._vertices);
		List<pb_Face> _faces = new List<pb_Face>(this.faces);
		int vc = vertexCount;

		// Dictionary<int, int> grp = new Dictionary<int, int>();	// this allows append face to add new vertices to a new shared index group
		// 														// if the sharedIndex is negative and less than -1, it will create new gorup
		// 														// that other sharedIndex members can then append themselves to.
		for(int i = 0; i < new_Faces.Length; i++)
		{
			_verts.AddRange(new_Vertices[i]);
			new_Faces[i].ShiftIndicesToZero();
			new_Faces[i].ShiftIndices(vc);
			_faces.Add(new_Faces[i]);

			if(new_SharedIndices != null && new_Vertices[i].Length != new_SharedIndices[i].Length)
			{
				Debug.LogError("Append Face failed because sharedIndex array does not match new vertex array.");
				return null;
			}

			if(new_SharedIndices != null)
				for(int j = 0; j < new_SharedIndices[i].Length; j++)
				{
					// TODO - FIX ME
					// if(new_SharedIndices[i][j] < -1)
					// {
					// 	if(grp.ContainsKey(new_SharedIndices[i][j]))
					// 		AddIndexToSharedIndexArray(grp[new_SharedIndices[i][j]], j+vc);
					// 	else
					// 		grp.Add(new_SharedIndices[i][j], AddIndexToSharedIndexArray(new_SharedIndices[i][j], j+vc));
					// }
					// else
						AddIndexToSharedIndexArray(new_SharedIndices[i][j], j+vc);
				}
			else
				for(int j = 0; j < new_Vertices[i].Length; j++)
				{
					AddIndexToSharedIndexArray(-1, j+vc);
				}
			vc = _verts.Count;
		}

		///* reconstruct mesh
		if(msh == null) msh = new Mesh();

		msh.vertices = _verts.ToArray();
		msh.triangles = pb_Face.AllTriangles(_faces);
		msh.normals = new Vector3[vc];
		msh.tangents = new Vector4[vc];
		msh.colors32 = pb_Face.Color32ArrayWithFaces(_faces.ToArray(), vc);
		msh.uv = new Vector2[vc];
		msh.uv2 = new Vector2[vc];
		
		;
		msh.RecalculateBounds();

		msh.name = msh.name;

		_vertices = _verts.ToArray();
		SetFaces(_faces.ToArray());

		ToMesh();

		return new_Faces;
	}
#endregion

#region INFORMATION SEEKING

	// Returns a jagged array of faces, split into int[face,triangles]
	// note that this method assumes triangle faces are next to one another
	private int[][] TrianglesByFace(int[] tris)
	{		
		int faceCount = tris.Length / 6;

		int[][] j_arr = new int[tris.Length / 6][];

		int curIndex = 0;
		for(int i = 0; i < faceCount; i++) {
			j_arr[i] = new int[6] {
				tris[curIndex+0],
				tris[curIndex+1],
				tris[curIndex+2],
				tris[curIndex+3],
				tris[curIndex+4],
				tris[curIndex+5]
			};
			curIndex += 6;
		}
		return j_arr;
	}

	private pb_Face[] ExtractFaces(Mesh m)
	{
		int[][] faces = TrianglesByFace(m.triangles);
		pb_Face[] q = new pb_Face[faces.Length];

		for(int i = 0; i < faces.Length; i++)
			q[i] = new pb_Face(faces[i], ProBuilder.DefaultMaterial, new pb_UV(), 0, -1, (Color32)Color.white);

		return q;
	}

	/**
	 *	\brief Cycles through a mesh and returns a pb_IntArray[] of 
	 *	triangles that point to the same point in world space.
	 *	@param _mesh The mesh to examine.
	 *	\sa pb_IntArray
	 *	\notes pbIntArray exists because Unity cannot serialize jagged arrays.
	 *	\returns A pb_IntArray[] (basically just an int[][] with some added functionality).
	 */
	private pb_IntArray[] ExtractSharedIndices()
	{
		int len = vertexCount;
		Vector3[] v = _vertices;
		bool[] assigned = pbUtil.FilledArray(false, len);

		List<pb_IntArray> shared = new List<pb_IntArray>();
		for(int i = 0; i < len-1; i++)
		{
			if(assigned[i])	// already assigned this vertex to a sharedIndex
				continue;

			List<int> indices = new List<int>(1) {i};
			for(int n = i+1; n < len; n++)
			{
				if( v[i] == v[n] )
				// if(Vector3.Distance(v[i], v[n]) < Mathf.Epsilon)
				{
					indices.Add(n);
					assigned[n] = true;
				}
			}

			shared.Add(new pb_IntArray(indices.ToArray()));
		}

		if(!assigned[len-1])
			shared.Add(new pb_IntArray(new int[1]{len-1}));

		return shared.ToArray();
	}

	/**
	 *	\brief Given a pb_IntArray array, returns the first value of each 
	 *	contained array.  Useful for grabbing a series of triangles that 
	 *	are guaranteed to not share a reference to the same world point.  
	 *	This is necessary because TranslateVertices requires that only one 
	 *	vertex per shared world point be passed.
	 *	@param _sharedTriangleArray A pb_IntArray[] containing arrays of 
	 *	triangles that point to vertices that share a world point.
	 *	\returns An int array of triangles that only contain one vertex per model point.
	 */
	private void RefreshUniqueIndices()
	{
		_uniqueIndices = new int[_sharedIndices.Length];
		for(int i = 0; i < _uniqueIndices.Length; i++)
			_uniqueIndices[i] = _sharedIndices[i][0];
	}

	/**
	 *	\brief Given a face, this method returns all other triangles that 
	 *	point to a vertex in the same points contained by the face index 
	 *	array.
	 *	@param face The face to extract triangle data from.
	 *	\return An int array of triangles.
	 */
	public int[] SharedTrianglesWithQuad(pb_Face face)
	{
		return _sharedIndices.AllIndicesWithValues(face.distinctIndices);
	}

	/**
	 *	\brief Given a single vertex index, this method returns all other 
	 *	triangles that point to a vertex in the same point.
	 *	@param index The vertex index to extract triangle data from.
	 *	\return An int array of shared triangles.
	 */
	public int[] SharedTrianglesWithVertexIndex(int index)
	{
		return SharedTrianglesWithTriangles(new int[]{index});
	}

	/**
	 *	\brief Given a #pb_Face array, return all shared triangles.  Returned 
	 *	array is guaranteed to contain an index for every vertex that shares 
	 *	a point with any point in the pb_Face array.
	 *	@param faces The array of faces to extract data from.
	 *	\return An int array of shared triangles.
	 */
	public int[] SharedTrianglesWithFaces(pb_Face[] faces)
	{
		return SharedTrianglesWithTriangles(DistinctTriangles(faces));
	}

	public int[] SharedTrianglesWithTriangles(int[] indices)
	{
		return _sharedIndices.AllIndicesWithValues(indices.ToDistinctArray());		
	}

	public int[] RemoveDuplicateVertexIndices(int[] indices)
	{
		List<int> distinct = new List<int>();
		List<int> used = new List<int>();
		for(int i = 0; i < indices.Length; i++)
		{			
			int intArrayIndex = sharedIndices.IndexOf(indices[i]);
			
			if(!used.Contains(intArrayIndex)) {
				used.Add( intArrayIndex );
				distinct.Add(indices[i]);
			}
		}

		return distinct.ToArray();
	}

	/**
	 *	\brief Gets vertices in world space
	 *	\returns A Vector3[] arry containing all vertex points in world space.
	 */
	public Vector3[] VerticesInWorldSpace()
	{
		Vector3[] worldPoints = new Vector3[_vertices.Length];

		System.Array.Copy(_vertices, worldPoints, worldPoints.Length);

		for(int i = 0; i < worldPoints.Length; i++)
			worldPoints[i] = transform.TransformPoint(worldPoints[i]);

		return worldPoints;
	}

	public Vector3[] VerticesInWorldSpace(int[] indices)
	{
		if(indices == null)
			Debug.LogWarning("indices == null");

		Vector3[] worldPoints = GetVertices(indices);

		for(int i = 0; i < worldPoints.Length; i++)
			worldPoints[i] = transform.TransformPoint(worldPoints[i]);

		return worldPoints;
	}

	public List<Vector3> VerticesInWorldSpace(List<int> indices)
	{
		if(indices == null)
			Debug.LogWarning("indices == null");

		List<Vector3> worldPoints = GetVertices(indices);

		for(int i = 0; i < worldPoints.Count; i++)
			worldPoints[i] = transform.TransformPoint(worldPoints[i]);

		return worldPoints;
	}

	public Vector3[] VerticesInWorldSpace(pb_Face face)
	{
		if(face == null) return null;

		Vector3[] worldPoints = GetVertices(face);

		for(int i = 0; i < worldPoints.Length; i++)
			worldPoints[i] = transform.TransformPoint(worldPoints[i]);

		return worldPoints;
	}

	// Overloads!
	public Vector3[] VerticesInWorldSpace(pb_Face[] _qds)
	{
		List<Vector3> worldPoints = new List<Vector3>();
		foreach(pb_Face quad in _qds)
			worldPoints.AddRange(VerticesInWorldSpace(quad));
		return worldPoints.ToArray();
	}

	/**
	 *	\brief Returns vertices in local space.
	 *	\returns Vector3[] Vertices for passed indices in local space.
	 */
	public Vector3[] GetVertices(int[] indices)
	{
		Vector3[] v = new Vector3[indices.Length];
		
		for(int i = 0; i < v.Length; i++)
			v[i] = _vertices[indices[i]];

		return v;
	}

	/**
	 *	\brief Returns vertices in local space.
	 *	\returns List<Vector3> Vertices for passed indices in local space.
	 */
	public List<Vector3> GetVertices(List<int> indices)
	{
		List<Vector3> v = new List<Vector3>(indices.Count);
		
		for(int i = 0; i < indices.Count; i++)
			v.Add( _vertices[indices[i]] );

		return v;
	}
#endregion

#region MESH CONSTRUCTION

	// Only copies vertex, triangle, and uv(1/2) data
	/**
	 *	\brief Performs a deep copy of a mesh and returns a new mesh object.
	 *	@param _mesh The mesh to copy.
	 *	\returns Copied mesh object.
	 */
	public static Mesh MeshWithMesh(Mesh _mesh)
	{
		Vector3[] v = new Vector3[_mesh.vertices.Length];
		int[][]   t = new int[_mesh.subMeshCount][];
		Vector2[] u = new Vector2[_mesh.uv.Length];
		Vector2[] u2 = new Vector2[_mesh.uv2.Length];
		Vector4[] tan = new Vector4[_mesh.tangents.Length];
		Vector3[] n = new Vector3[_mesh.normals.Length];
		Color32[] c = new Color32[_mesh.colors32.Length];

		System.Array.Copy(_mesh.vertices, v, v.Length);

		for(int i = 0; i < t.Length; i++)
			t[i] = _mesh.GetTriangles(i);

		System.Array.Copy(_mesh.uv, u, u.Length);
		System.Array.Copy(_mesh.uv2, u2, u2.Length);
		System.Array.Copy(_mesh.normals, n, n.Length);
		System.Array.Copy(_mesh.tangents, tan, tan.Length);
		System.Array.Copy(_mesh.colors32, c, c.Length);

		Mesh m = new Mesh();
		m.Clear();

		m.vertices = v;
		
		m.subMeshCount = t.Length;
		for(int i = 0; i < t.Length; i++)
			m.SetTriangles(t[i], i);

		m.uv = u;
		m.uv2 = u2;
	
		m.tangents = tan;
		m.normals = n;

		return m;
	}

	/**
	 *	\brief Force regenerate geometry.  Also responsible for sorting faces with shared materials into the same submeshes.
	 *	@param removeNoDraw If true, NoDraw faces will not be added to the mesh.
	 */
	public void ToMesh(bool removeNoDraw)
	{

		// Sort the faces into groups of like materials
		Dictionary<Material, List<pb_Face>> matDic = new Dictionary<Material, List<pb_Face>>();
		foreach(pb_Face quad in faces)
		{
			if( (quad.material == null) || (removeNoDraw && quad.material.name == "NoDraw") )
				continue;
				
			if(matDic.ContainsKey(quad.material))
				matDic[quad.material].Add(quad);
			else
				matDic.Add(quad.material, new List<pb_Face>(1){quad});
		}

		// dont clear the mesh, cause we want to save everything except triangle data
		Mesh m = msh;
		
		Material[] mats = new Material[matDic.Count];
		
		m.triangles = null;
		m.vertices = _vertices;
		m.subMeshCount = matDic.Count;

		int i = 0;

		foreach( KeyValuePair<Material, List<pb_Face>> kvp in matDic )
		{
			m.SetTriangles(pb_Face.AllTriangles(kvp.Value), i);
			mats[i] = kvp.Key;
			i++;
		}

		m.RecalculateBounds();
		;

		GetComponent<MeshFilter>().sharedMesh = m;
		GetComponent<MeshRenderer>().sharedMaterials = mats;
	}

	/**
	 *	\brief Call this to ensure that the mesh is unique.  Basically performs a DeepCopy and assigns back to self.
	 */
	public void MakeUnique()
	{
		Mesh m = MeshWithMesh(msh);
		m.name = "pb_Mesh" + gameObject.GetInstanceID();

		gameObject.GetComponent<MeshFilter>().sharedMesh = m;

		pb_Face[] q = new pb_Face[_quads.Length];

		for(int i = 0; i < q.Length; i++) {
			q[i] = _quads[i].DeepCopy();
		}

		_quads = q;

		pb_IntArray[] sv = new pb_IntArray[_sharedIndices.Length];
		System.Array.Copy(_sharedIndices, sv, sv.Length);
		_sharedIndices = sv;

		_vertices = m.vertices;

		Refresh();
	}

	// Uses local mesh and quad info to generate triangle sets per material
	public void ToMesh()
	{
		ToMesh(false);
	}

	public void HideNodraw()
	{
		ToMesh(true);
	}

	public void ShowNodraw()
	{
		ToMesh(false);
	}

	/**
	 *	\brief Recalculates standard mesh properties - normals, bounds, collisions, UVs, tangents, and colors.
	 */
	public void Refresh()
	{
		// Mesh
		RefreshNormals();

		msh.RecalculateBounds();
		
		if(GetComponent<Collider>())
		{
			foreach(Collider c in gameObject.GetComponents<Collider>())
			{
				System.Type t = c.GetType();

				if(t == typeof(BoxCollider))
				{
					((BoxCollider)c).center = msh.bounds.center;
					((BoxCollider)c).size = msh.bounds.size;
				} else
				if(t == typeof(SphereCollider))
				{
					((SphereCollider)c).center = msh.bounds.center;
					((SphereCollider)c).radius = pb_Math.LargestValue(msh.bounds.extents);
				} else
				if(t == typeof(CapsuleCollider))
				{
					((CapsuleCollider)c).center = msh.bounds.center;
					Vector2 xy = new Vector2(msh.bounds.extents.x, msh.bounds.extents.z);
					((CapsuleCollider)c).radius = pb_Math.LargestValue(xy);
					((CapsuleCollider)c).height = msh.bounds.size.y;
				} else
				if(t == typeof(WheelCollider))
				{
					((WheelCollider)c).center = msh.bounds.center;
					((WheelCollider)c).radius = pb_Math.LargestValue(msh.bounds.extents);
				} else
				if(t == typeof(MeshCollider))
				{
					gameObject.GetComponent<MeshCollider>().sharedMesh = null;	// this is stupid.
					gameObject.GetComponent<MeshCollider>().sharedMesh = msh;
				} 
			}
		}

		;

		RefreshUV();

		RefreshTangent();

		RefreshColor();

		if(OnRefresh != null)
			OnRefresh(this);
	}	
#endregion

#region UV

	/**
	 *	Returns a new unused texture group id.
	 */
	public int UnusedTextureGroup(int i)
	{
		int[] used = new int[faces.Length];
		for(int j = 0; j < faces.Length; j++)	
			used[j] = faces[j].textureGroup;
		while(System.Array.IndexOf(used, i) > -1)
			i++;
		return i;
	}

	public int UnusedTextureGroup()
	{
		int i = 1;
	
		int[] used = new int[faces.Length];
		for(int j = 0; j < faces.Length; j++)	
			used[j] = faces[j].textureGroup;
	
		while(System.Array.IndexOf(used, i) > -1)
			i++;

		return i;
	}

	public void RefreshUV()
	{
		RefreshUV(faces);
	}

	public void RefreshUV(pb_Face[] faces)
	{
		Dictionary<int, List<pb_Face>> tex_groups = new Dictionary<int, List<pb_Face>>();
		
		int n = -2;
		foreach(pb_Face f in faces)
		{
			if(f.textureGroup > 0 && tex_groups.ContainsKey(f.textureGroup))
				tex_groups[f.textureGroup].Add(f);
			else
				tex_groups.Add( f.textureGroup > 0 ? f.textureGroup : n--, new List<pb_Face>(1) { f });
		}

		// Add any non-selected faces to the update list
		if(this.faces.Length != faces.Length)
			foreach(pb_Face f in this.faces)
				if(tex_groups.ContainsKey(f.textureGroup) && !tex_groups[f.textureGroup].Contains(f))
					tex_groups[f.textureGroup].Add(f);

		Vector2[] newUVs = msh.uv;
		n = 0;
		foreach(KeyValuePair<int, List<pb_Face>> kvp in tex_groups)
		{
			Vector2[] uvs;
			Vector3 nrm = Vector3.zero;

			foreach(pb_Face face in kvp.Value)
				nrm += this.FaceNormal(face);

			// Bugger.Log("uv-ing vertices:\n" + GetVertices(pb_Face.AllTrianglesDistinct(kvp.Value).ToArray()).ToFormattedString("\n"));
			nrm /= (float)kvp.Value.Count;

			if(kvp.Value[0].uv.useWorldSpace)
				uvs = pb_UV_Utility.PlanarMap( VerticesInWorldSpace( pb_Face.AllTrianglesDistinct(kvp.Value).ToArray()), kvp.Value[0].uv, nrm);
			else
				uvs = pb_UV_Utility.PlanarMap( GetVertices(pb_Face.AllTrianglesDistinct(kvp.Value).ToArray()), kvp.Value[0].uv, nrm);
			
			int j = 0;
			foreach(pb_Face f in kvp.Value)
				foreach(int i in f.distinctIndices)
					newUVs[i] = uvs[j++];
		}
		msh.uv = newUVs;
	}

	public Vector2[] GetUVs(pb_Face quad)
	{
		int[] ind = quad.distinctIndices;
		Vector2[] uvs = new Vector2[ind.Length];
		for(int i = 0; i < ind.Length; i++)
			uvs[i] = msh.uv[ind[i]];
		return uvs;
	}

	public void SetFaceMaterial(pb_Face quad, Material mat)
	{
		quad.SetMaterial(mat);
		ToMesh();
	}

	public void SetFaceMaterial(pb_Face[] quad, Material mat)
	{
		for(int i = 0; i < quad.Length; i++)
			quad[i].SetMaterial(mat);

		ToMesh();
	}

	public void SetObjectMaterial(Material mat)
	{
		foreach(pb_Face quad in faces)
			quad.SetMaterial(mat);
		ToMesh();
	}

	/**
	 *	\brief True if any face in this #pb_Object is marked NoDraw.
	 */
	public bool containsNodraw
	{
		get 
		{
			foreach(Material mat in gameObject.GetComponent<MeshRenderer>().sharedMaterials)
				if(mat != null && mat.name == "NoDraw")
					return true;
			return false;
		}
	}

	/**
	 *	\brief True if object only consists of NoDraw faces.
	 */
	public bool onlyNodraw
	{
		get
		{
			foreach(pb_Face f in faces)
				if(f.material != null && f.material.name != "NoDraw")
					return false;
			return true;
		}
	}

	/**
	 *	\brief Sets the pb_Face uvSettings param to match the passed #pv_UV _uv 
	 */
	public void SetFaceUV(pb_Face quad, pb_UV _uv)
	{
		quad.SetUV(_uv);

		if(quad.uv.useWorldSpace)
			SetUVs(quad, pb_UV_Utility.PlanarMap(VerticesInWorldSpace(quad), quad.uv) );
		else
			SetUVs(quad, pb_UV_Utility.PlanarMap( quad.GetDistinctVertices(_vertices), quad.uv) );
	}

	private void SetUVs(pb_Face face, Vector2[] uvs)
	{
		int[] vertIndices = face.distinctIndices;
		Vector2[] newUV = new Vector2[msh.uv.Length];
		System.Array.Copy(msh.uv, newUV, msh.uv.Length);
		
		for(int i = 0; i < vertIndices.Length; i++) {
			newUV[vertIndices[i]] = uvs[i];
		}

		gameObject.GetComponent<MeshFilter>().sharedMesh.uv = newUV;		
	}

	private void SetUVs(pb_Face[] quad, Vector2[][] uvs)
	{
		Vector2[] newUV = new Vector2[msh.uv.Length];
		System.Array.Copy(msh.uv, newUV, msh.uv.Length);
		
		for(int i = 0; i < quad.Length; i++) {

			int[] vertIndices = quad[i].distinctIndices;
			for(int n = 0; n < vertIndices.Length; n++)
				newUV[vertIndices[n]] = uvs[i][n];
		
		}

		gameObject.GetComponent<MeshFilter>().sharedMesh.uv = newUV;
	}

	public Vector2[] GetFaceUV(pb_Face q)
	{
		int[] dv = q.distinctIndices;
		Vector2[] uvs = new Vector2[dv.Length];
		for(int i = 0; i < uvs.Length; i++)
		{
			uvs[i] = msh.uv[dv[i]];
		}
		return uvs;
	}

	public void SetUV2(Vector2[] v)
	{
		GetComponent<MeshFilter>().sharedMesh.uv2 = v;
	}

	// public void GenerateUV2()
	// {
	// 	GenerateUV2(ProBuilder.UV2Method.BinPack);
	// }

	// public void GenerateUV2(ProBuilder.UV2Method uv2Method)
	// {
	// 	switch(uv2Method)
	// 	{
	// 		case ProBuilder.UV2Method.BinPack:
	// 			// This code generates normalized UVs for every plane, then packs them into
	// 			// a 0-1 coordinate system.  Pretty neat, but unfortunately Unity's lightmapping
	// 			// system doesn't like the way these UVs are laid out.  Maybe some day I'll figure
	// 			// this out.
	// 			//
	// 			// Therefore, this code is not called in Editor at all.  It exists because it still
	// 			// provides a good example of generating bin-packed uvs that may be desirable for
	// 			// someone.

	// 			Vector2[][] perQuadUVs = new Vector2[faces.Length][];
	// 			for(int i = 0 ; i < faces.Length; i++) {
	// 				perQuadUVs[i] = pb_UV_Utility.PlanarMap(GetVertices(faces[i]), pb_UV.LightmapUVSettings);
	// 			}

	// 			perQuadUVs = pb_UV_Utility.BinPackUVs(perQuadUVs, .1f);

	// 			Vector2[] uvJoined = new Vector2[perQuadUVs.Length*4];

	// 			if(uvJoined.Length != msh.uv2.Length)	Debug.LogWarning("UV2 Channel Corrupt.");

	// 			int step = 0;
	// 			for(int i = 0; i < perQuadUVs.Length; i++) {
	// 				uvJoined[step+0] = perQuadUVs[i][0];
	// 				uvJoined[step+1] = perQuadUVs[i][1];
	// 				uvJoined[step+2] = perQuadUVs[i][2];
	// 				uvJoined[step+3] = perQuadUVs[i][3];
	// 				step+=4;
	// 			}

	// 			GetComponent<MeshFilter>().sharedMesh.uv2 = pb_UV_Utility.NormalizeUVs(uvJoined);
	// 			break;

	// 		case ProBuilder.UV2Method.Unity:
	// 			break;
	// 	}
	// }
#endregion

#region COLORS

	private void RefreshColor()
	{
		msh.colors32 = pb_Face.Color32ArrayWithFaces(faces, vertexCount);
	}

	private void SetColors32(Color32[] c32)
	{
		if(c32.Length != _vertices.Length)
			return;

		msh.colors32 = c32;
	}

	public void SetFaceColor(pb_Face face, Color32 c32)
	{
		Color32[] clrs = GetColors32();

		int[] indices = face.distinctIndices;

		for(int i = 0; i < indices.Length; i++)
			clrs[indices[i]] = c32;

		face.SetColor(c32);

		SetColors32(clrs);
	}

	public Color32[] GetColors32()
	{
		Mesh m = msh;
		
		if(m.colors32 == null || m.colors32.Length < 1)
			m.colors32 = pbUtil.FilledArray((Color32)Color.white, _vertices.Length);

		return m.colors32;
	}	
#endregion

#region EDGES

	public Vector3[] GetEdgeVertices(pb_Edge edge)
	{
		return new Vector3[]
		{
			_vertices[edge.x],
			_vertices[edge.y]
		};
	}
#endregion

#region TANGENTS

	public void RefreshTangent()
	{
		// implementation found here (no sense re-inventing the wheel, eh?)
		// http://answers.unity3d.com/questions/7789/calculating-tangents-vector4.html

		//speed up math by copying the mesh arrays
		int[] triangles = msh.triangles;
		Vector3[] vertices = msh.vertices;
		Vector2[] uv = msh.uv;
		Vector3[] normals = msh.normals;

		//variable definitions
		int triangleCount = triangles.Length;
		int vertexCount = vertices.Length;

		Vector3[] tan1 = new Vector3[vertexCount];
		Vector3[] tan2 = new Vector3[vertexCount];

		Vector4[] tangents = new Vector4[vertexCount];

		for (long a = 0; a < triangleCount; a += 3)
		{
			long i1 = triangles[a + 0];
			long i2 = triangles[a + 1];
			long i3 = triangles[a + 2];

			Vector3 v1 = vertices[i1];
			Vector3 v2 = vertices[i2];
			Vector3 v3 = vertices[i3];

			Vector2 w1 = uv[i1];
			Vector2 w2 = uv[i2];
			Vector2 w3 = uv[i3];

			float x1 = v2.x - v1.x;
			float x2 = v3.x - v1.x;
			float y1 = v2.y - v1.y;
			float y2 = v3.y - v1.y;
			float z1 = v2.z - v1.z;
			float z2 = v3.z - v1.z;

			float s1 = w2.x - w1.x;
			float s2 = w3.x - w1.x;
			float t1 = w2.y - w1.y;
			float t2 = w3.y - w1.y;

			float r = 1.0f / (s1 * t2 - s2 * t1);

			Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
			Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

			tan1[i1] += sdir;
			tan1[i2] += sdir;
			tan1[i3] += sdir;

			tan2[i1] += tdir;
			tan2[i2] += tdir;
			tan2[i3] += tdir;
		}


		for (long a = 0; a < vertexCount; ++a)
		{
			Vector3 n = normals[a];
			Vector3 t = tan1[a];

			//Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
			//tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
			Vector3.OrthoNormalize(ref n, ref t);
			tangents[a].x = t.x;
			tangents[a].y = t.y;
			tangents[a].z = t.z;

			tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
		}

		gameObject.GetComponent<MeshFilter>().sharedMesh.tangents = tangents;
	}
#endregion

#region NORMALS

	// groups are per-face
	private void SmoothPerGroups()
	{
		// it might make sense to cache this...
		Dictionary<int, List<pb_Face>> groups = new Dictionary<int, List<pb_Face>>();
		for(int i = 0; i < faces.Length; i++) {
			// smoothing groups 
			// 0 		= none
			// 1 - 24 	= smooth
			// 25 - 42	= hard
			if(faces[i].smoothingGroup > 0 && faces[i].smoothingGroup < 25)
			{
				if(groups.ContainsKey(faces[i].smoothingGroup))
					groups[faces[i].smoothingGroup].Add(faces[i]);
				else
					groups.Add(faces[i].smoothingGroup, new List<pb_Face>(){faces[i]});
			}
		}

		Vector3[] nrmls = msh.normals;
		foreach(KeyValuePair<int, List<pb_Face>> kvp in groups)
		{
			int[][] smoothed = SharedTrianglesExclusive(kvp.Value);

			for(int i = 0; i < smoothed.Length; i++)
			{
				Vector3[] vN = new Vector3[smoothed[i].Length];
				int n = 0;
				for(n = 0; n < vN.Length; n++)
					vN[n] = nrmls[smoothed[i][n]];

				Vector3 nrml = pb_Math.Average(vN);

				for(n = 0; n < smoothed[i].Length; n++)
					nrmls[smoothed[i][n]] = nrml.normalized;
			}
		}
		SetNormals(nrmls);
	}

	public bool SetNormals(Vector3[] nrmls)
	{
		if(nrmls.Length != msh.vertices.Length) {
			return false;
		}
		
		GetComponent<MeshFilter>().sharedMesh.normals = nrmls;
		return true;
	}

	public void RefreshNormals()
	{
		Mesh m = msh;

		// All hard edges
		m.RecalculateNormals();
			
		// Per-vertex (not ideal unless you've got a sphere)
		// SmoothPerVertexNormals();

		SmoothPerGroups();
	}
#endregion

#region CLEANUP

	// This won't run unless ExecuteInEditMode is set.  If we destroy the mesh, there's no going back,
	// so unless people really bitch about that mesh leak when deleting objects, we'll just let it be.
	public void OnDestroy()
	{
		DestroyImmediate(gameObject.GetComponent<MeshFilter>().sharedMesh);
		// EditorUtility.UnloadUnusedAssets();
	}

	public void Destroy()
	{
		Mesh m = pb_Object.MeshWithMesh(msh);
		m.name = msh.name;

		GameObject go = gameObject;


		if(go.GetComponent<pb_Entity>())
			Object.DestroyImmediate(go.GetComponent<pb_Entity>());

		go.GetComponent<MeshFilter>().sharedMesh = m;
		if(go.GetComponent<MeshCollider>())
			go.GetComponent<MeshCollider>().sharedMesh = m;
		
		Object.DestroyImmediate(this);
	}
#endregion

#region OVERRIDES

	public override string ToString()
	{
		string str =  
			"Name: " + gameObject.name + "\n" +
			"ID: " + id + "\n" +
			"Shape: " + _shape.ToString() + "\n" +
			"Entity Type: " + GetComponent<pb_Entity>().entityType + "\n" +
			"Shared / Total Vertices: " + sharedIndices.Length + " , " + msh.vertices.Length + "\n" +
			"faces: " + faces.Length;
		return str;
	}

	public string ToStringDetailed()
	{
		string str =  
			"Name: " + gameObject.name + "\n" +
			"ID: " + id + "\n" +
			"Shape: " + _shape.ToString() + "\n" +
			"Entity Type: " + GetComponent<pb_Entity>().entityType + "\n" +
			"Shared Vertices: " + sharedIndices.Length + "\n" +
			"Faces: " + faces.Length + "\n" +
			"Submesh: " + msh.subMeshCount + "\n" +
			"Vertices: " + msh.vertices.Length + "\n" +
			_vertices.ToFormattedString("\n") + "\n" +
			"Triangles: " + msh.triangles.Length + "\n" + 
			_faces.ToFormattedString("\n") + "\n";

		return str;
	}
#endregion

#region REBUILDING / INSTANTIATION


	/**
	 *	\brief Forces each pb_Face in the object to rebuild it's edge arrays.
	 *	Recommended to be done after adding or removing vertices / triangles
	 */
	public void RebuildFaceCaches()
	{
		foreach(pb_Face f in faces)
			f.RebuildCaches();
	}

	public void Verify()
	{
		if(msh == null)
		{
			// attempt reconstruction...
			if( !ReconstructMesh() )	
			{
				// fuck.  reconstruct failed.
				Debug.LogError("ProBuilder Object " + id + " contains null geometry.  Self destruct in 5...4...3...");
				DestroyImmediate(this.gameObject);
				return;
			}
		}
		else
		{			
			// default to cached vertices
			if(_vertices.Length != msh.vertices.Length)
			{
				ReconstructMesh();
				return;
			}

			// check to make sure that faces and vertex data from mesh match
			// pb_Object cached values.  Can change when applying/reverting
			// prefabs
			if(!msh.vertices.IsEqual(_vertices))
			{
				ReconstructMesh();
				return;
			}
		}

		int meshNo;
		int.TryParse(msh.name.Replace("pb_Mesh", ""), out meshNo);
		if(meshNo != id)
		{
			MakeUnique();
		}
	}

	/**
	 *	\brief Checks #face array for materials, and cycles through current 
	 *	submesh index to make sure that appropriate triangles are assigning 
	 *	to each submesh.
	 */
	private bool FacesMatchTriangles()
	{
		Dictionary<Material, int> matTriCount = new Dictionary<Material, int>();
		for(int i = 0; i < faces.Length; i++)
			if(!matTriCount.ContainsKey(faces[i].material))
				matTriCount.Add(faces[i].material, faces[i].indices.Length);
			else
				matTriCount[faces[i].material] += faces[i].indices.Length;

		Material[] sm = GetComponent<MeshRenderer>().sharedMaterials;
		Mesh m = msh;

		if(matTriCount.Count != sm.Length)
		{
			return false;
		}

		foreach(KeyValuePair<Material, int> kvp in matTriCount)
		{
			int i = System.Array.IndexOf(sm, kvp.Key);
	
			if(i < 0)
			{
				return false;
			}
			else
			{
				if(m.GetTriangles(i).Length != kvp.Value)
				{
					return false;
				}
			}
		}

		return true;
	}
#endregion

#region SNAPPING
	// for backwards compatability
	public static void OnProGridsChange(bool _snapEnabled, float _snapValue)//, bool snapAllVertices)
	{
		SixBySeven.Shared.snapEnabled = _snapEnabled;
		SixBySeven.Shared.snapValue = _snapValue;
	}
#endregion

}

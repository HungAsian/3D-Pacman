using UnityEngine;
using System.Collections;

[AddComponentMenu("")]	// Don't let the user add this to any object.
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
/**
 *	\brief Class to aid in the organization of pb_Object gameObjects.  Runs a check on Start() 
 *	to determine whether or not group should auto-collapse.
 */
public class pb_Group : MonoBehaviour {

	[HideInInspector]
	public bool acg = false;

	void Start ()
	{
		if(!acg) return;

		MeshFilter[] mf = gameObject.GetComponentsInChildren<MeshFilter>() as MeshFilter[];
		foreach(Transform t in transform)
			t.gameObject.pbSetActive(false);
			
		pb_MeshCombineUtility.CombineMeshFilters(mf, transform, false);

		gameObject.AddComponent<MeshCollider>();
	}

	public void ChildCheck()
	{
		if(transform.childCount < 1 && GetComponent<MeshFilter>().sharedMesh == null)
			DestroyImmediate(gameObject);
	}
}

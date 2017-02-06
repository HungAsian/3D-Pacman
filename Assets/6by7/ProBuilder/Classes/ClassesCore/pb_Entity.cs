// #define DEBUG

using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;
using ProBuilder2.Common;

[AddComponentMenu("")]	// Don't let the user add this to any object.
/**
 *	\brief Determines how this #pb_Object should behave in game.
 */
public class pb_Entity : MonoBehaviour
{

	pb_Object pb;

	/**
	 *	\brief 
	 *	@param 
	 *	\returns 
	 */
	/***** DEPRECATED ****/
	public enum ColliderType
	{
		MeshCollider,
		BoxCollider,
		None,
		Upgraded
	}

	public ColliderType colliderType = ColliderType.Upgraded;
	public bool isTrigger = false;
	public PhysicMaterial physicMaterial;
	public bool forceConvex = false;
	public bool smoothSphereCollisions = false;
	public Vector3 center = Vector3.zero;
	public Vector3 size = Vector3.one;
	public bool userSetDimensions = false;

	public void GenerateCollisions()
	{
		// start with a fresh canvas
		foreach(Component c in transform.GetComponents<Component>())
		{
			if(c.GetType().BaseType == typeof(Collider))
				DestroyImmediate(c);
		}
		
		switch(colliderType)
		{
			case ColliderType.None:
				break;
		
			case ColliderType.MeshCollider:
				MeshCollider mc = gameObject.AddComponent<MeshCollider>();
				mc.isTrigger = isTrigger;
				mc.convex = forceConvex;
				mc.smoothSphereCollisions = smoothSphereCollisions;
				mc.sharedMaterial = physicMaterial;
				break;
		
			case ColliderType.BoxCollider:
				BoxCollider bc = gameObject.AddComponent<BoxCollider>();
				bc.isTrigger = isTrigger;
				bc.sharedMaterial = physicMaterial;
		
				if(userSetDimensions)
				{
					bc.center = center;
					bc.size = size;
				}
				break;
		}

		foreach(Component c in transform.GetComponents<Component>())
		{
			if(c.GetType().BaseType == typeof(Collider))
			{
				c.hideFlags = (HideFlags)0;
			}
		}

		colliderType = ColliderType.Upgraded;
	}
	/***			***/

	[HideInInspector]
	public ProBuilder.EntityType entityType { get { return _entityType; } }
	[SerializeField]
	[HideInInspector]
	private ProBuilder.EntityType _entityType;

	/**
	 *	\brief Unity Awake method.
	 */
	public void Awake()
	{
		pb = GetComponent<pb_Object>();
		if(pb == null) 
		{
			Debug.LogError("pb is null");
			return;
		}

		if(pb.containsNodraw)
			pb.HideNodraw();

		switch(entityType)
		{
			case ProBuilder.EntityType.Occluder:
				// Destroy(gameObject);
			break;

			case ProBuilder.EntityType.Detail:
			break;

			case ProBuilder.EntityType.Trigger:
				#if !DEBUG
				GetComponent<MeshRenderer>().enabled = false;
				// Destroy(GetComponent<MeshRenderer>());
				// Destroy(this);
				#endif
			break;

			case ProBuilder.EntityType.Collider:
				// Destroy(GetComponent<pb_Object>());
				#if !DEBUG
				GetComponent<MeshRenderer>().enabled = false;
				// Destroy(GetComponent<MeshRenderer>());
				// Destroy(this);
				#endif
			break;
		}
	}

	public void SetEntity(ProBuilder.EntityType t)
	{
		_entityType = t;
	}
}
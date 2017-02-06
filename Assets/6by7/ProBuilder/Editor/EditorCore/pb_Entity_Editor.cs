using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;

[CustomEditor(typeof(pb_Entity))]
public class pb_Entity_Editor : Editor
{
	pb_Entity ent;
	public enum ColType
	{
		MeshCollider,
		BoxCollider,
		SphereCollider
	}

	public void OnEnable()
	{
		ent = (pb_Entity)target;
		if(ent.colliderType != pb_Entity.ColliderType.Upgraded) ent.GenerateCollisions();
	}

	public override void OnInspectorGUI()
	{
		GUI.changed = false;

		ProBuilder.EntityType et = ent.entityType;
		et = (ProBuilder.EntityType)EditorGUILayout.EnumPopup("Entity Type", et);
		if(et != ent.entityType) { ent.SetEntityType(et); GUI.changed = false; EditorUtility.SetDirty(ent); }

		// Convience
		GUILayout.Label("Add Collider", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal();

			if(GUILayout.Button("Mesh Collider", EditorStyles.miniButtonLeft))
				AddCollider( ColType.MeshCollider );

			if(GUILayout.Button("Box Collider", EditorStyles.miniButtonMid))
				AddCollider( ColType.BoxCollider );

			if(GUILayout.Button("Remove Collider", EditorStyles.miniButtonRight))
				RemoveColliders();

		GUILayout.EndHorizontal();

		GUILayout.Space(4);

		if(GUI.changed)
			EditorUtility.SetDirty(ent);
	}

	private void AddCollider(ColType c)
	{
		GameObject go = ((pb_Entity)target).gameObject;

		Collider[] existingCollisions = go.GetComponents<Collider>();
		
		if( existingCollisions != null && existingCollisions.Length > 0 )
		{
			if( EditorUtility.DisplayDialog("Prior Collider", go.name + " already has a collider: \n" + existingCollisions.ToFormattedString("\n") + "\n\nWould you like to replace it?", "Replace", "Cancel"))
					RemoveColliders();
			else
				return;
		}

		switch(c)
		{
			case ColType.MeshCollider:
				go.AddComponent<MeshCollider>().convex = true;
				break;

			case ColType.BoxCollider:	
				go.AddComponent<BoxCollider>();
				break;

			case ColType.SphereCollider:	
				go.AddComponent<SphereCollider>();
				break;

			default:
				break;
		}
	}

	private void RemoveColliders()
	{
		foreach(Collider c in ((pb_Entity)target).gameObject.GetComponents<Collider>())
			DestroyImmediate(c);
	}
}

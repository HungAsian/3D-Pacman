#pragma warning disable 0162 // TODO - FIX
#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5
#define UNITY_4_3
#endif

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5
#define UNITY_4
#endif

#undef UNITY_4

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.EditorEnum;
using System.Collections.Generic;

[CustomEditor(typeof(pb_Object))]
[CanEditMultipleObjects]
public class pb_Object_Editor : Editor
{

	pb_Object pb;
	
	// RectOffset buttonPadding = new RectOffset(2, 2, 2, 2);
	bool info = false;
	Renderer ren;
	Vector3 offset = Vector3.zero;

	public void OnEnable()
	{	
		if(EditorApplication.isPlayingOrWillChangePlaymode)
			return;
		
		pb = (pb_Object)target;
		ren = pb.gameObject.GetComponent<Renderer>();

		#if UNITY_4
		EditorUtility.SetSelectedWireframeHidden(ren, true);
		#else
		EditorUtility.SetSelectedWireframeHidden(ren, false);
		#endif

		pb.Verify();
	}

	// bool pbInspectorFoldout = false;
	public override void OnInspectorGUI()
	{
		GUI.backgroundColor = Color.green;

		if(GUILayout.Button("Open " + pb_Constant.PRODUCT_NAME))
			if (EditorPrefs.HasKey(pb_Constant.pbDefaultOpenInDockableWindow) && 
				!EditorPrefs.GetBool(pb_Constant.pbDefaultOpenInDockableWindow))
				EditorWindow.GetWindow(typeof(pb_Editor), true, pb_Constant.PRODUCT_NAME, true);			// open as floating window
			else
				EditorWindow.GetWindow(typeof(pb_Editor), false, pb_Constant.PRODUCT_NAME, true);			// open as dockable window

		GUI.backgroundColor = Color.white;

		info = EditorGUILayout.Foldout(info, "Info");

		if(info)
		{
			Vector3 sz = ren.bounds.size;
			EditorGUILayout.Vector3Field("Object Size (read only)", sz);
		}

		if(pb.SelectedTriangles.Length > 0)
		{
			offset = EditorGUILayout.Vector3Field("Quick Offset", offset);
			if(GUILayout.Button("Apply Offset"))
			{
				pbUndo.RecordObject(pb, "Offset Vertices");
				pb.TranslateVertices(pb.SelectedTriangles, offset);
				pb.Refresh();
				if(pb_Editor.instanceIfExists != null)
					pb_Editor.instance.UpdateSelection();
			}
		}
	}

	void OnSceneGUI()
	{
		if(GUIUtility.hotControl < 1 && pb.transform.localScale != Vector3.one)
			pb.FreezeScaleTransform();
	}

	bool HasFrameBounds() 
	{
		return pb.SelectedTriangles.Length > 0;
	}

	Bounds OnGetFrameBounds()
	{
		Vector3[] verts = pb.VerticesInWorldSpace();
		
		if(pb.SelectedTriangles.Length < 2)
			return new Bounds(verts[pb.SelectedTriangles[0]], Vector3.one * .2f);

		Vector3 min = verts[pb.SelectedTriangles[0]], max = min;
		
		for(int i = 1; i < pb.SelectedTriangles.Length; i++)
		{
			int j = pb.SelectedTriangles[i];

			min.x = Mathf.Min(verts[j].x, min.x);
			max.x = Mathf.Max(verts[j].x, max.x);
			min.y = Mathf.Min(verts[j].y, min.y);
			max.y = Mathf.Max(verts[j].y, max.y);
			min.z = Mathf.Min(verts[j].z, min.z);
			max.z = Mathf.Max(verts[j].z, max.z);

		}

		return new Bounds( (min+max)/2f, max-min );
	}
}
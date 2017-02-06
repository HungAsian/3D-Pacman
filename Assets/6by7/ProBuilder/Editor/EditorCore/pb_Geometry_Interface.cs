// #define FREE

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

/**
 *	\internal -- todo Implement a 'preview' mesh mode, allowing pb to skip some of the lengthy pb-specific calculations and making preview supa fast.
 */
public class pb_Geometry_Interface : EditorWindow
{	
	static Color PREVIEW_COLOR = new Color(.5f, .9f, 1f, .56f);
	public ProBuilder.Shape shape = ProBuilder.Shape.Cube;

	private pb_Object previewObject;
	private bool showPreview = true;
	private Material _prevMat;

	public Material previewMat
	{
		get
		{
			if(_prevMat == null)
			{
				_prevMat = new Material(Shader.Find("Diffuse"));
				// _prevMat = new Material(Shader.Find("Hidden/ProBuilder/UnlitColor"));
				_prevMat.mainTexture = (Texture2D)Resources.Load("Textures/GridBox_Default");
				_prevMat.SetColor("_Color", PREVIEW_COLOR);
			}
			return _prevMat;
		}
	}
	private bool initPreview = false; // used to toggle preview on and off from class OnGUI

	Material userMaterial = null;
	public void OnEnable()
	{
		#if !PROTOTYPE
			userMaterial = pb_Preferences_Internal.GetMaterial(pb_Constant.pbDefaultMaterial);
		#endif

		initPreview = true;
	}

	public void OnDisable()
	{
		DestroyPreviewObject();
	}


	[MenuItem("GameObject/Create Other/" + pb_Constant.PRODUCT_NAME + " Cube _%k")]
	public static void MenuCreateCube()
	{
		pb_Object pb = ProBuilder.CreatePrimitive(ProBuilder.Shape.Cube);
		
		#if !PROTOTYPE
		Material mat = null;
		if(EditorPrefs.HasKey(pb_Constant.pbDefaultMaterial))
			mat = (Material)AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString(pb_Constant.pbDefaultMaterial), typeof(Material));

		if(mat != null) pb.SetObjectMaterial(mat);
		#endif

		pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ProBuilder.ColliderType>(pb_Constant.pbDefaultCollider));
	}

	int dip = 82;
	public void OnGUI()
	{	
		GUILayout.BeginHorizontal();
			bool sp = showPreview;
			showPreview = GUILayout.Toggle(showPreview, "Show Preview");
			if(sp != showPreview && !showPreview) DestroyPreviewObject();

			if(GUILayout.Button("Center Preview"))
			{
				if(previewObject == null) return;

				pb_Editor_Utility.ScreenCenter(previewObject.gameObject);
				Selection.activeTransform = previewObject.transform;
				Selection.activeObject = previewObject;
				RegisterPreviewObjectTransform();
			}
		GUILayout.EndHorizontal();

		GUILayout.Space(7);

		GUILayout.Label("Shape Selector", EditorStyles.boldLabel);
		
		ProBuilder.Shape oldShape = shape;
		shape = (ProBuilder.Shape)EditorGUILayout.EnumPopup(shape);
			
		GUILayout.Space(14);

		GUI.Box(new Rect(6, dip, Screen.width-12, Screen.height-dip-6), "");

		if(shape != oldShape) initPreview = true;

		switch(shape)
		{
			case ProBuilder.Shape.Cube:
				CubeGUI();
				break;
			case ProBuilder.Shape.Prism:
				PrismGUI();
				break;
			case ProBuilder.Shape.Stair:
				StairGUI();
				break;
			case ProBuilder.Shape.Cylinder:
				CylinderGUI();
				break;
			case ProBuilder.Shape.Plane:
				PlaneGUI();
				break;
			case ProBuilder.Shape.Door:
				DoorGUI();
				break;
			case ProBuilder.Shape.Pipe:
				PipeGUI();
				break;
			case ProBuilder.Shape.Cone:
				ConeGUI();
				break;
			case ProBuilder.Shape.Sprite:
				SpriteGUI();
				break;
			case ProBuilder.Shape.Custom:
				CustomGUI();
				break;
		}
	}

	/**
	 *	\brief Creates a cube.
	 *	\returns The cube.
	 */
	static Vector3 cubeSize = Vector3.one;
	public void CubeGUI()
	{
		cubeSize = EditorGUILayout.Vector3Field("Dimensions", cubeSize);
		
		if(cubeSize.x <= 0) cubeSize.x = .01f;
		if(cubeSize.y <= 0) cubeSize.y = .01f;
		if(cubeSize.z <= 0) cubeSize.z = .01f;

		if( showPreview && (GUI.changed || initPreview) ) SetPreviewObject(pb_Shape.CubeGenerator(cubeSize));

		if(GUILayout.Button("New Cube")) 
		{
			pb_Object pb = pb_Shape.CubeGenerator(cubeSize);
			if( userMaterial ) pb.SetObjectMaterial( userMaterial );
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ProBuilder.ColliderType>(pb_Constant.pbDefaultCollider));

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}
	}
	
	/**
	 *	\brief Creates a sprite.
	 *	\returns The sprite.
	 */
	public void SpriteGUI()
	{
		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Facing Direction");
		plane_axis = (ProBuilder.Axis)EditorGUILayout.EnumPopup(plane_axis);
		GUILayout.EndHorizontal();

		if( showPreview && (GUI.changed || initPreview) ) 
			SetPreviewObject(
				 pb_Shape.PlaneGenerator(
				 	1,
				 	1,
				 	0,
				 	0,
				 	plane_axis,
				 	false));

		if(GUILayout.Button("New Sprite")) 
		{
			pb_Object pb = pb_Shape.PlaneGenerator(
				 	1,
				 	1,
				 	0,
				 	0,
				 	plane_axis,
				 	false);
			if( userMaterial ) pb.SetObjectMaterial( userMaterial );
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ProBuilder.ColliderType>(pb_Constant.pbDefaultCollider));

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}
	}

	/**
	 *	\brief Creates a prism.
	 *	...that's it.
	 *	\returns The prism.
	 */
	static Vector3 prismSize = Vector3.one;
	public void PrismGUI()
	{
		prismSize = EditorGUILayout.Vector3Field("Dimensions", prismSize);
		
		if(prismSize.x < 0) prismSize.x = 0.01f;
		if(prismSize.y < 0) prismSize.y = 0.01f;
		if(prismSize.z < 0) prismSize.z = 0.01f;

		if( showPreview && (GUI.changed || initPreview) ) SetPreviewObject(pb_Shape.PrismGenerator(prismSize));

		if(GUILayout.Button("New Prism")) 
		{
			pb_Object pb = pb_Shape.PrismGenerator(prismSize);
			if( userMaterial ) pb.SetObjectMaterial( userMaterial );
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ProBuilder.ColliderType>(pb_Constant.pbDefaultCollider));

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}
	}

	/**** Stair Generator ***/
	static bool extendSidesToFloor = true;
	static bool generateBack = true;
	static int stair_steps = 6;
	static float stair_width = 4f, stair_height = 5f, stair_depth = 8f;
	static bool stair_platformsOnly = false;
	public void StairGUI()
	{
		stair_steps = EditorGUILayout.IntField("Steps", stair_steps);
		stair_steps = Clamp(stair_steps, 2, 50);

		stair_width = EditorGUILayout.FloatField("Width", stair_width);
		stair_width = Mathf.Clamp(stair_width, 0.01f, 500f);

		stair_height = EditorGUILayout.FloatField("Height", stair_height);
		stair_height = Mathf.Clamp(stair_height, .01f, 500f);

		stair_depth = EditorGUILayout.FloatField("Depth", stair_depth);
		stair_depth = Mathf.Clamp(stair_depth, .01f, 500f);

		stair_platformsOnly = EditorGUILayout.Toggle("Platforms Only", stair_platformsOnly);
		if(stair_platformsOnly) { GUI.enabled = false; extendSidesToFloor = false; generateBack = false; }
		extendSidesToFloor = EditorGUILayout.Toggle("Extend sides to floor", extendSidesToFloor);
		generateBack = EditorGUILayout.Toggle("Generate Back", generateBack);
		GUI.enabled = true;

		if( showPreview && (GUI.changed || initPreview) ) 
			SetPreviewObject(pb_Shape.StairGenerator(
				stair_steps, 
				stair_width,
				stair_height,
				stair_depth,
				extendSidesToFloor,
				generateBack,
				stair_platformsOnly));

		if(GUILayout.Button("Build Stairs"))
		{
			pb_Object pb = pb_Shape.StairGenerator(stair_steps, stair_width, stair_height, stair_depth, extendSidesToFloor, generateBack, stair_platformsOnly);

			if( userMaterial ) pb.SetObjectMaterial( userMaterial );
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ProBuilder.ColliderType>(pb_Constant.pbDefaultCollider));

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;		
		}
	}

	/**** Cylinder Generator ***/
	static int cyl_axisCuts = 6;
	static float cyl_radius = 1.5f;
	static float cyl_height = 4f;
	static int cyl_heightCuts = 2;
	public void CylinderGUI()
	{
		#if FREE || TORNADO_TWINS
		GUI.enabled = false;
		#endif

		// Store old values	
		cyl_radius = EditorGUILayout.FloatField("Radius", cyl_radius);
		cyl_radius = Mathf.Clamp(cyl_radius, .01f, Mathf.Infinity);

		cyl_axisCuts = EditorGUILayout.IntField("Axis Divisions", cyl_axisCuts);
		cyl_axisCuts = Clamp(cyl_axisCuts, 2, 48);

		cyl_height = EditorGUILayout.FloatField("Height", cyl_height);

		cyl_heightCuts = EditorGUILayout.IntField("Height Cuts", cyl_heightCuts);
		cyl_heightCuts = Clamp(cyl_heightCuts, 0, 48);

		if(cyl_axisCuts % 2 != 0)
			cyl_axisCuts++;

		if(cyl_heightCuts < 0)
			cyl_heightCuts = 0;

		if( showPreview && (GUI.changed || initPreview) ) 
		{
			SetPreviewObject(
				pb_Shape.CylinderGenerator(
				cyl_axisCuts,
				cyl_radius,
				cyl_height,
				cyl_heightCuts),
				new int[1] { (cyl_axisCuts*(cyl_heightCuts+1)*4)+1 } );
		}

		if(GUILayout.Button("Build Cylinder")) 
		{
			pb_Object pb = pb_Shape.CylinderGenerator(cyl_axisCuts, cyl_radius, cyl_height, cyl_heightCuts);
			
			int centerIndex = (cyl_axisCuts*(cyl_heightCuts+1)*4)+1;
			
			if( userMaterial ) pb.SetObjectMaterial( userMaterial );
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ProBuilder.ColliderType>(pb_Constant.pbDefaultCollider), new int[1] {centerIndex} );

			AlignWithPreviewObject(pb.gameObject);
			
			DestroyPreviewObject();
			showPreview = false;			
		}

		#if FREE || TORNADO_TWINS
			GUI.enabled = true;
		#endif
	}

	static Vector3 doorSize = Vector3.one;
	public void DoorGUI()
	{
		doorSize = EditorGUILayout.Vector3Field("Door Size", doorSize);
		if( showPreview && (GUI.changed || initPreview) ) 
			SetPreviewObject(pb_Shape.DoorGenerator(doorSize));

		if(GUILayout.Button("Build Door")) {
			pb_Object pb = pb_Shape.DoorGenerator(doorSize);
			
			if( userMaterial ) pb.SetObjectMaterial( userMaterial );
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ProBuilder.ColliderType>(pb_Constant.pbDefaultCollider));

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;			
		}
	}

	static float plane_width = 10, plane_height = 10;
	static int plane_widthCuts = 3, plane_heightCuts = 3;
	static ProBuilder.Axis plane_axis = ProBuilder.Axis.Up;
	static bool plane_smooth = false;
	public void PlaneGUI()
	{
		#if FREE || TORNADO_TWINS
			GUI.enabled = false;
		#endif

		plane_axis = (ProBuilder.Axis)EditorGUILayout.EnumPopup("Normal Axis", plane_axis);

		plane_width = EditorGUILayout.FloatField("Width", plane_width);
		plane_height = EditorGUILayout.FloatField("Height", plane_height);

		if(plane_width < 1f)
			plane_width = 1f;

		if(plane_height < 1f)
			plane_height = 1f;

		plane_widthCuts = EditorGUILayout.IntField("Cuts Width", plane_widthCuts);
		
		if(plane_widthCuts < 0)
			plane_widthCuts = 0;

		plane_heightCuts = EditorGUILayout.IntField("Cuts Height", plane_heightCuts);
		
		if(plane_heightCuts < 0)
			plane_heightCuts = 0;

		if( showPreview && (GUI.changed || initPreview) ) 
			SetPreviewObject(
				 pb_Shape.PlaneGenerator(
				 	plane_width,
				 	plane_height,
				 	plane_widthCuts,
				 	plane_heightCuts,
				 	plane_axis,
				 	plane_smooth));

		if(GUILayout.Button("Build Plane"))
		{
			pb_Object pb = pb_Shape.PlaneGenerator(plane_width, plane_height, plane_widthCuts, plane_heightCuts, plane_axis, plane_smooth);
			
			if( userMaterial ) pb.SetObjectMaterial( userMaterial );
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ProBuilder.ColliderType>(pb_Constant.pbDefaultCollider));

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}

		#if FREE || TORNADO_TWINS
			GUI.enabled = true;
		#endif
	}

	static float pipe_radius = 1f;
	static float pipe_height = 2f;
	static float pipe_thickness = .2f;
	static int pipe_subdivAxis = 6;
	static int pipe_subdivHeight = 1;
	void PipeGUI()
	{
		#if FREE || TORNADO_TWINS
			GUI.enabled = false;
		#endif
		pipe_radius = EditorGUILayout.FloatField("Radius", pipe_radius);
		pipe_height = EditorGUILayout.FloatField("Height", pipe_height);
		pipe_thickness = EditorGUILayout.FloatField("Thickness", pipe_thickness);
		pipe_subdivAxis = EditorGUILayout.IntField("Subdivisions Axis", pipe_subdivAxis);
		pipe_subdivHeight = EditorGUILayout.IntField("Subdivisions Height", pipe_subdivHeight);
		
		if(pipe_radius < .1f)
			pipe_radius = .1f;

		if(pipe_height < .1f)
			pipe_height = .1f;

		pipe_subdivHeight = (int)Mathf.Clamp(pipe_subdivHeight, 0f, 32f);
		pipe_thickness = Mathf.Clamp(pipe_thickness, .01f, pipe_radius-.01f);
		pipe_subdivAxis = (int)Mathf.Clamp(pipe_subdivAxis, 3f, 32f);		

		if( showPreview && (GUI.changed || initPreview) ) 
			SetPreviewObject(
				 pb_Shape.PipeGenerator(	
				 	pipe_radius,
					pipe_height,
					pipe_thickness,
					pipe_subdivAxis,
					pipe_subdivHeight
				 	));	 	

		if(GUILayout.Button("Build Pipe"))
		{
			pb_Object pb = pb_Shape.PipeGenerator(	
				 	pipe_radius,
					pipe_height,
					pipe_thickness,
					pipe_subdivAxis,
					pipe_subdivHeight
				 	);

			if( userMaterial ) pb.SetObjectMaterial( userMaterial );
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ProBuilder.ColliderType>(pb_Constant.pbDefaultCollider));

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}

		#if FREE || TORNADO_TWINS
			GUI.enabled = true;
		#endif
	}

	static float 	cone_radius = 1f;
	static float 	cone_height = 2f;
	static int 		cone_subdivAxis = 6;
	void ConeGUI()
	{
		#if FREE || TORNADO_TWINS
			GUI.enabled = false;
		#endif
		cone_radius = EditorGUILayout.FloatField("Radius", cone_radius);
		cone_height = EditorGUILayout.FloatField("Height", cone_height);
		cone_subdivAxis = EditorGUILayout.IntField("Subdivisions Axis", cone_subdivAxis);
		
		if(cone_radius < .1f)
			cone_radius = .1f;

		if(cone_height < .1f)
			cone_height = .1f;

		pipe_subdivHeight = (int)Mathf.Clamp(pipe_subdivHeight, 1f, 32f);
		pipe_thickness = Mathf.Clamp(pipe_thickness, .01f, cone_radius-.01f);
		cone_subdivAxis = (int)Mathf.Clamp(cone_subdivAxis, 3f, 32f);		

		if( showPreview && (GUI.changed || initPreview) ) 
			SetPreviewObject(
				 pb_Shape.ConeGenerator(	
				 	cone_radius,
					cone_height,
					cone_subdivAxis
				 	));	 	

		if(GUILayout.Button("Build Cone"))
		{
			pb_Object pb = pb_Shape.ConeGenerator(	
				 	pipe_radius,
					cone_height,
					cone_subdivAxis
				 	);

			if( userMaterial ) pb.SetObjectMaterial( userMaterial );
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ProBuilder.ColliderType>(pb_Constant.pbDefaultCollider));

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}

		#if FREE || TORNADO_TWINS
			GUI.enabled = true;
		#endif
	}

	static string verts = "//Vertical Plane\n0, 0, 0\n1, 0, 0\n0, 1, 0\n1, 1, 0\n";
	static Vector2 scrollbar = new Vector2(0f, 0f);
	public void CustomGUI()
	{
		#if FREE || TORNADO_TWINS
			GUI.enabled = false;
		#endif

		GUILayout.Label("Custom Geometry", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox("Vertices must be wound in faces, and counter-clockwise.\n(Think horizontally reversed Z)", MessageType.Info);
			
		scrollbar = GUILayout.BeginScrollView(scrollbar);
			verts = EditorGUILayout.TextArea(verts, GUILayout.MinHeight(160));
		GUILayout.EndScrollView();

		if( showPreview && (GUI.changed || initPreview) ) 
			SetPreviewObject(
				ProBuilder.CreateObjectWithPoints(pbUtil.StringToVector3Array(verts)));

		if(GUILayout.Button("Build Geometry")) 
		{
			if(verts.Length > 256)
				Debug.Log("Whoa!  Did you seriously type all those points!?");
			pb_Object pb = ProBuilder.CreateObjectWithPoints(pbUtil.StringToVector3Array(verts));
			
			if( userMaterial ) pb.SetObjectMaterial( userMaterial );
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ProBuilder.ColliderType>(pb_Constant.pbDefaultCollider));

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}

		#if FREE || TORNADO_TWINS
			GUI.enabled = true;
		#endif
	}

	public int Clamp(int val, int min, int max)
	{
		if(val > max) val = max;
		if(val < min) val = min;
		return val;
	}

#region PREVIEW OBJECT

	public void DestroyPreviewObject()
	{
		if(previewObject != null) GameObject.DestroyImmediate(previewObject.gameObject);
		if(_prevMat != null) DestroyImmediate(_prevMat);
	}

	private void SetPreviewObject(pb_Object pb)
	{
		SetPreviewObject(pb, null);
	}

	private void SetPreviewObject(pb_Object pb, int[] indicesToCenterPivotOn)
	{
		pb.isSelectable = false;

		initPreview = false;
		bool prevTransform = false;

		if(previewObject != null)
		{
			prevTransform = true;
			RegisterPreviewObjectTransform();
		}
		
		DestroyPreviewObject();

		previewObject = pb;
		previewObject.SetName("Preview");
		previewObject.SetObjectMaterial(previewMat);

		if(pb_Preferences_Internal.GetBool(pb_Constant.pbForceGridPivot))
			previewObject.CenterPivot(indicesToCenterPivotOn == null ? new int[1]{0} : indicesToCenterPivotOn);

		if(prevTransform)
		{
			previewObject.transform.position = m_pos;
			previewObject.transform.rotation = m_rot;
			previewObject.transform.localScale = m_scale;
		}
		else
		{
			pb_Editor_Utility.ScreenCenter(previewObject.gameObject);
		}

		if(SixBySeven.Shared.snapEnabled)
			pb.transform.position = pbUtil.SnapValue(pb.transform.position, SixBySeven.Shared.snapValue);
		else
		if(pb_Preferences_Internal.GetBool(pb_Constant.pbForceVertexPivot))
			pb.transform.position = pbUtil.SnapValue(pb.transform.position, 1f);
			
		Selection.activeTransform = pb.transform;
	}

	Vector3 m_pos = Vector3.zero;
	Quaternion m_rot = Quaternion.identity;
	Vector3 m_scale = Vector3.zero;
	private void RegisterPreviewObjectTransform()
	{
		m_pos 	= previewObject.transform.position;
		m_rot 	= previewObject.transform.rotation;
		m_scale = previewObject.transform.localScale;
	}	

	private bool PreviewObjectHasMoved()
	{
		if(m_pos != previewObject.transform.position)
			return true;
		if(m_rot != previewObject.transform.rotation)
			return true;
		if(m_scale != previewObject.transform.localScale)
			return true;	
		return false;
	}

	private void AlignWithPreviewObject(GameObject go)
	{
		if(go == null || previewObject == null) return;
		go.transform.position 	= previewObject.transform.position;
		go.transform.rotation 	= previewObject.transform.rotation;
		go.transform.localScale = previewObject.transform.localScale;
		go.GetComponent<pb_Object>().FreezeScaleTransform();
	}
#endregion
}

#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5
#define UNITY_4_3
#elif UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define UNITY_4
#elif UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define UNITY_3
#endif

using UnityEngine;
using System.Collections;

public static class pb_Constant
{
#if PROTOTYPE
	public const string PRODUCT_NAME = "Prototype";
#else
	public const string PRODUCT_NAME = "ProBuilder";
#endif

	// make preference keys constant
	public const string pbDefaultEditMode 				= "pbDefaultEditMode";
	public const string pbDefaultEditLevel 				= "pbDefaultEditLevel";
	public const string pbDefaultSelectionMode 			= "pbDefaultSelectionMode";
	public const string pbDefaultFaceColor 				= "pbDefaultFaceColor";
	public const string pbDefaultSelectedVertexColor	= "pbDefaultSelectedVertexColor";
	public const string pbDefaultVertexColor			= "pbDefaultVertexColor";
	public const string pbDefaultOpenInDockableWindow 	= "pbDefaultOpenInDockableWindow";
	public const string pbDefaultShortcuts 				= "pbDefaultShortcuts";
	public const string pbDefaultMaterial 				= "pbDefaultMaterial";
	public const string pbDefaultHideFaceMask 			= "pbDefaultHideFaceMask";
	public const string pbEditorPrefVersion 			= "pbEditorPrefVersion";
	public const string pbHandleAlignment	 			= "pbHandleAlignment";
	public const string pbDefaultCollider 				= "pbDefaultCollider";
	public const string pbForceConvex 					= "pbForceConvex";
	public const string pbVertexColorPrefs 				= "pbVertexColorPrefs";
	public const string pbShowEditorNotifications 		= "pbShowEditorNotifications";
	public const string pbDragCheckLimit		 		= "pbDragCheckLimit";
	public const string pbForceVertexPivot		 		= "pbForceVertexPivot";
	public const string pbForceGridPivot		 		= "pbForceGridPivot";
	public const string pbPerimeterEdgeExtrusionOnly	= "pbPerimeterEdgeExtrusionOnly";
	public const string pbPerimeterEdgeBridgeOnly		= "pbPerimeterEdgeBridgeOnly";
	public const string pbVertexHandleSize 				= "pbVertexHandleSize";

	public static Rect RectZero = new Rect(0,0,0,0);

	// First Tier
	public const int MENU_ABOUT = 0;
	public const int MENU_WINDOW = 100;
	public const int MENU_EDITOR = 200;
	public const int MENU_SELECTION = 300;
	public const int MENU_GEOMETRY = 400;
	public const int MENU_ACTIONS = 500;
	public const int MENU_REPAIR = 600;
	public const int MENU_TOOLS = 700;
	public const int MENU_VERTEX_COLORS = 800;

	// Second Tier
	public const int MENU_GEOMETRY_FACE = 0;
	public const int MENU_GEOMETRY_EDGE = 20;
	public const int MENU_GEOMETRY_VERTEX = 40;
	public const int MENU_GEOMETRY_USEINFERRED = 80;
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Math;

/**
 *	\brief Responsible for mapping UV coordinates.  
 *	Generally should only be called by #pb_Object 
 *	after setting #pb_UV parameters.
 */
public class pb_UV_Utility {

	public static Vector2[] PlanarMap(Vector3[] verts, pb_UV uvSettings) { return PlanarMap(verts, uvSettings, Vector3.zero); }
	public static Vector2[] PlanarMap(Vector3[] verts, pb_UV uvSettings, Vector3 nrm)
	{
		if(verts.Length < 3)
		{
			Debug.LogWarning("Attempting to project UVs on a face with < 3 vertices.  This is most often caused by removing or creating Geometry and Undo-ing without selecting a new face.  Try deselecting this object then performing your edits.");
			return new Vector2[verts.Length];
		}

		Vector2[] uvs = new Vector2[verts.Length];
		Vector3 planeNormal = nrm == Vector3.zero ? pb_Math.PlaneNormal(verts[0], verts[1], verts[2]) : nrm;
		Vector3 vec = new Vector3();

		pb_UV.ProjectionAxis project = (uvSettings.projectionAxis == pb_UV.ProjectionAxis.AUTO) ? pb_Math.GetProjectionAxis(planeNormal) : uvSettings.projectionAxis;	

		switch(project)
		{
			case pb_UV.ProjectionAxis.Planar_X:
				vec = Vector3.up;
				break;

			case pb_UV.ProjectionAxis.Planar_Y:
				vec = Vector3.forward;
				break;
			
			case pb_UV.ProjectionAxis.Planar_Y_Negative:
				vec = -Vector3.forward;
				break;
			
			case pb_UV.ProjectionAxis.Planar_Z:
				vec = Vector3.up;
				break;

			default:
				vec = Vector3.forward;
				break;
		}
		
		/**
		 *	Assign vertices to UV coordinates
		 */
		for(int i = 0; i < verts.Length; i++)
		{
			float u, v;
			Vector3 uAxis, vAxis;
			
			// get U axis
			uAxis = Vector3.Cross(planeNormal, vec);
			uAxis.Normalize();

			// calculate V axis relative to U
			vAxis = Vector3.Cross(uAxis, planeNormal);
			vAxis.Normalize();

			u = Vector3.Dot(uAxis, verts[i]);// / uvSettings.scale.x);
			v = Vector3.Dot(vAxis, verts[i]);// / uvSettings.scale.y);

			uvs[i] = new Vector2(u, v);
		}

		if(!uvSettings.useWorldSpace)
			uvs = ShiftToPositive(uvs);

		uvs = ApplyUVSettings(uvs, uvSettings);

		return uvs;
	}

	private static Vector2[] ApplyUVSettings(Vector2[] uvs, pb_UV uvSettings)
	{
		Vector2 cen = Vector2Center(uvs);
		int len = uvs.Length;

		switch(uvSettings.fill)
		{
			case pb_UV.Fill.Tile:
				break;
			case pb_UV.Fill.Normalize:
				uvs = NormalizeUVs(uvs);
				break;
			case pb_UV.Fill.Stretch:
				uvs = StretchUVs(uvs);
				break;
		}

		if(uvSettings.justify != pb_UV.Justify.None)
			uvs = JustifyUVs(uvs, uvSettings.justify);

		// Apply offset last, so that fill and justify don't override it.
		uvs = OffsetUVs(uvs, -uvSettings.offset);

		Vector2 cen2 = Vector2.zero;

		for(int i = 0; i < len; i++)
		{
			Vector2 zeroed = uvs[i]-cen;
			
			if(uvSettings.useWorldSpace)
				uvs[i] = new Vector2(uvs[i].x / uvSettings.scale.x, uvs[i].y / uvSettings.scale.y);
			else
				uvs[i] = new Vector2(zeroed.x / uvSettings.scale.x, zeroed.y / uvSettings.scale.y) + cen;
			
			cen2 += uvs[i];
		}

		cen = cen2/(float)len;
		
		for(int i = 0; i < len; i++)
			uvs[i] = uvs[i].RotateAroundPoint(cen, uvSettings.rotation);
		
		for(int i = 0; i < len; i++)
		{
			float u = uvs[i].x, v = uvs[i].y;
			
			if(uvSettings.flipU)
				u = -u;

			if(uvSettings.flipV)
				v = -v;

			if(!uvSettings.swapUV)
				uvs[i] = new Vector2(u, v);
			else
				uvs[i] = new Vector2(v, u);
		}

		return uvs;
	}

#region UTILITY

	private static Vector2[] StretchUVs(Vector2[] uvs)
	{
		Vector2 smallest = SmallestVector2(uvs);
		Vector2 mag = LargestVector2(uvs);

		for(int i = 0; i < uvs.Length; i++)
		{
			uvs[i] -= smallest;	// zero off
			uvs[i] = new Vector2(uvs[i].x/mag.x, uvs[i].y/mag.y);
		}
		return uvs;
	}

	/**
	 *	A super basic implementation of a Next Fit 2d bin packing algorithm.
	 *	This could be improved by checking remaining rectangles for a fit before
	 *	creating a new shelf... but it's late and this works decently well for
	 *	our purposes.  Not to mention that this will be run fairly frequently,
	 *	so the easier the math the better.
	 */
	public static Vector2[][] BinPackUVs(Vector2[][] uvs, float pad)
	{
		Vector2[][] packed = new Vector2[uvs.Length][];

		List<UVsWithContainingBox> u = new List<UVsWithContainingBox>();
		for(int i = 0; i < uvs.Length; i++) 
			u.Add(new UVsWithContainingBox(uvs[i]));

		u = UVsWithContainingBox.SortByWidthDescending(u);

		float totalArea = 0f;
		for(int i = 0; i < u.Count; i++)
			totalArea += u[i].area;
		float sqrSize = Mathf.Pow(totalArea, .5f); 	// starting square size of bounding box.  best guess.
		float incSize = sqrSize/10f;				// increase subsequent boxes by 1/10th of original guess.
		List<Shelf> shelf = new List<Shelf>();		// store shelfs
		int n = 0; 									// current shelf				
		shelf.Add(new Shelf(pad, pad, pad, pad));

		for(int i = 0; i < u.Count; i++)
		{
			// if the next rect won't fit on this shelf, start a new shelf.
			if( shelf[n].xMax + u[i].width+pad > sqrSize ) 
			{
				shelf.Add(new Shelf(pad, pad, shelf[n].yMax+pad, 0f));
				n++;
			}

			// shift uvs into place
			ShiftToPoint(u[i].uvs, new Vector2(shelf[n].xMax+pad, shelf[n].yMin+pad));

			// update the current shelf's dimensions.
			shelf[n].xMax += u[i].width+pad;	// add new object's dimensions to shelf width
			if(shelf[n].yMax < u[i].height+pad)	// resize the shelf height
				shelf[n].yMax = u[i].height+pad + ( (n-1 >=0) ? shelf[n-1].yMax : 0);

			// if we're over the allowed size, clear everything and start again with a larger
			// sqrSize
			if(shelf[n].yMax+pad > sqrSize) {
				shelf.Clear();
				shelf.Add(new Shelf(pad, pad, pad, pad));
				sqrSize += incSize;
				i = -1;
				n = 0;
			}
		}

		for(int i = 0; i < u.Count; i++) {
			packed[i] = u[i].uvs;
		}

		return packed;
	}

	private static Vector2[] OffsetUVs(Vector2[] uvs, Vector2 offset)
	{
		for(int i = 0; i < uvs.Length; i++)
			uvs[i] += offset;
		return uvs;
	}

	/*
	 *	Returns normalized UV values for a mesh uvs (0,0) - (1,1)
	 */
	private static Vector2[] NormalizeUVs(Vector2[] uvs)
	{
		/*
		 *	how this works -
		 *		- shift uv coordinates such that the lowest value x and y coordinates are zero
		 *		- scale non-zeroed coordinates uniformly to normalized values (0,0) - (1,1)
		 */

		// shift UVs to zeroed coordinates
		Vector2 smallestVector2 = SmallestVector2(uvs);

		int i;
		for(i = 0; i < uvs.Length; i++)
		{
			uvs[i] -= smallestVector2;
		}

		float scale = LargestFloatInVector2Array(uvs);

		for(i = 0; i < uvs.Length; i++)
		{
			uvs[i] /= scale;
		}

		return uvs;
	}

	private static Vector2[] JustifyUVs(Vector2[] uvs, pb_UV.Justify j)
	{
		Vector2 amt = new Vector2(0f, 0f);
		switch(j)
		{
			case pb_UV.Justify.Left:
				amt = new Vector2(SmallestVector2(uvs).x, 0f);
				break;
			case pb_UV.Justify.Right:
				amt = new Vector2(LargestVector2(uvs).x - 1f, 0f);
				break;
			case pb_UV.Justify.Top:
				amt = new Vector2(0f, LargestVector2(uvs).y - 1f);
				break;
			case pb_UV.Justify.Bottom:
				amt = new Vector2(0f, SmallestVector2(uvs).y);
				break;
			case pb_UV.Justify.Center:
				amt = Vector2Center(uvs) - (new Vector2(.5f, .5f));
				break;
		}

		for(int i = 0; i < uvs.Length; i++)
			uvs[i] -= amt;
	
		return uvs;
	}

	private static Vector2[] ShiftToPoint(Vector2[] uvs, Vector2 point)
	{
		Vector2 offset = point - SmallestVector2(uvs);
		return OffsetUVs(uvs, offset);
	}

	private static Vector2[] ShiftToPositive(Vector2[] uvs)
	{
		// shift UVs to zeroed coordinates
		Vector2 smallestVector2 = SmallestVector2(uvs);

		int i;
		for(i = 0; i < uvs.Length; i++)
		{
			uvs[i] -= smallestVector2;
		}

		return uvs;
	}

	private static Vector2 Vector2Center(Vector2[] v)
	{
		Vector2 sum = new Vector2(0f, 0f);
		for(int i = 0; i < v.Length; i++)
			sum += v[i];
		return sum/(float)v.Length;
	}

	private static Vector2 SmallestVector2(Vector2[] v)
	{
		Vector2 s = v[0];
		for(int i = 0; i < v.Length; i++)
		{
			if(v[i].x < s.x)
				s.x = v[i].x;
			if(v[i].y < s.y)
				s.y = v[i].y;
		}
		return s;
	}

	private static Vector2 RotateUVs(Vector2 originalUVRotation, float angleChange)
	{
		float c = Mathf.Cos(angleChange*Mathf.Deg2Rad);
		float s = Mathf.Sin(angleChange*Mathf.Deg2Rad);
		Vector2 finalUVRotation = new Vector2(originalUVRotation.x*c - originalUVRotation.y*s, originalUVRotation.x*s + originalUVRotation.y*c);
		return finalUVRotation;
	}

	public static Vector2 LargestVector2(Vector2[] v)
	{
		Vector2 l = v[0];
		for(int i = 0; i < v.Length; i++)
		{
			if(v[i].x > l.x)
				l.x = v[i].x;
			if(v[i].y > l.y)
				l.y = v[i].y;
		}
		return l;
	}

	public static float Area(Vector2[] v)
	{
		Vector2 dim = Dimensions(v);
		return dim.x * dim.y;
	}

	public static Vector2 Dimensions(Vector2[] v)
	{
		return LargestVector2(v) - SmallestVector2(v);
	}

	public static float LargestFloatInVector2Array(Vector2[] v)
	{
		float l = v[0].x;
		for(int i = 0; i < v.Length; i++)
		{
			if(v[i].x > l)
				l = v[i].x;
			if(v[i].y > l)
				l = v[i].y;
		}
		return l;
	}
#endregion
}

#region BIN PACKING CLASSES

public class UVsWithContainingBox {

	public Vector2[] uvs;
	public float area;
	public float width;
	public float height;

	public UVsWithContainingBox(Vector2[] _uv)
	{
		uvs = _uv;
		area = pb_UV_Utility.Area(uvs);
		width = pb_UV_Utility.Dimensions(uvs).x;
		height = pb_UV_Utility.Dimensions(uvs).y;
	}

	public static List<UVsWithContainingBox> SortByAreaDescending(List<UVsWithContainingBox> uvs)
	{
		List<UVsWithContainingBox> sorted = new List<UVsWithContainingBox>();
		for(int i = 0; i < uvs.Count; i++)
		{
			int insertPosition = sorted.Count;
			for(int n = 0; n < sorted.Count; n++)
			{
				if(uvs[i].area > sorted[n].area && n < insertPosition)
					insertPosition = n;
			}
			sorted.Insert(insertPosition, uvs[i]);
		}
		return sorted;
	}

	public static List<UVsWithContainingBox> SortByWidthDescending(List<UVsWithContainingBox> uvs)
	{
		List<UVsWithContainingBox> sorted = new List<UVsWithContainingBox>();
		for(int i = 0; i < uvs.Count; i++)
		{
			int insertPosition = sorted.Count;
			for(int n = 0; n < sorted.Count; n++)
			{
				if(uvs[i].width > sorted[n].width && n <= insertPosition)
					insertPosition = n;
			}
			sorted.Insert(insertPosition, uvs[i]);
		}
		return sorted;
	}

	public static List<UVsWithContainingBox> SortByHeightDescending(List<UVsWithContainingBox> uvs)
	{
		List<UVsWithContainingBox> sorted = new List<UVsWithContainingBox>();
		for(int i = 0; i < uvs.Count; i++)
		{
			int insertPosition = sorted.Count;
			for(int n = 0; n < sorted.Count; n++)
			{
				if(uvs[i].height > sorted[n].height && n < insertPosition)
					insertPosition = n;
			}
			sorted.Insert(insertPosition, uvs[i]);
		}
		return sorted;
	}

	public override string ToString()
	{
		return ("Area: " + area + "  " + "Width / Height: " + width + ", " + height);
	}
}

public class Shelf {
	public Shelf() {
		xMin = 0f;
		xMax = 0f;
		yMin = 0f;
		yMax = 0f;
	}

	public Shelf(float xmin, float xmax, float ymin, float ymax) {
		xMin = xmin;
		xMax = xmax;
		yMin = ymin;
		yMax = ymax;
	}

	public float xMin = 0f;
	public float xMax = 0f;
	public float yMin = 0f;
	public float yMax = 0f;	
}


#endregion
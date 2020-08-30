//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Simple class that holds texture atlas uvs.
/// </summary>

public class NJGAtlas : MonoBehaviour
{
	[System.Serializable]
	public class Sprite
	{
		public int id;
		public string name;
		public Rect uvs;
		public Vector2 position { get { if (mPos == Vector2.zero) mPos = new Vector2(uvs.x, uvs.y); return mPos; } }
		public Vector2 size { get { if (mSize == Vector2.zero) mSize = new Vector2(width, height); return mSize; } }
		public float width { get { return uvs.width; } }
		public float height { get { return uvs.height; } }
		public bool initialized;

		Vector2 mPos;
		Vector2 mSize;
	}

	public Shader shader;
	public int size = 2048;
	public int padding = 1;
	public Sprite[] sprites;
	public Texture2D texture;
	public Material spriteMaterial;

	public Sprite GetSprite(int id) { return sprites[id]; }

	public Sprite GetSprite(string spriteName) 
	{
		for (int i = 0, imax = sprites.Length; i < imax; i++)
		{
			if (sprites[i] != null)
			{
				if (!string.IsNullOrEmpty(sprites[i].name))
				{
					if (sprites[i].name == spriteName)
						return sprites[i];
				}
			}
		}
		return null; 
	}

	List<string> mNames = new List<string>();

	public List<string> GetListOfSprites()
	{
		mNames.Clear();

		for (int i = 0, imax = sprites.Length; i < imax; i++)				
			mNames.Add(sprites[i].name);				
		
		return mNames;
	}

	/// <summary>
	/// Convenience function that retrieves a list of all sprite names that contain the specified phrase
	/// </summary>

	public List<string> GetListOfSprites(string match)
	{
		if (string.IsNullOrEmpty(match)) return GetListOfSprites();
		List<string> list = new List<string>();

		// First try to find an exact match
		for (int i = 0, imax = sprites.Length; i < imax; ++i)
		{
			Sprite s = sprites[i];

			if (s != null && !string.IsNullOrEmpty(s.name) && string.Equals(match, s.name, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(s.name);
				return list;
			}
		}

		// No exact match found? Split up the search into space-separated components.
		string[] keywords = match.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < keywords.Length; ++i) keywords[i] = keywords[i].ToLower();

		// Try to find all sprites where all keywords are present
		for (int i = 0, imax = sprites.Length; i < imax; ++i)
		{
			Sprite s = sprites[i];

			if (s != null && !string.IsNullOrEmpty(s.name))
			{
				string tl = s.name.ToLower();
				int matches = 0;

				for (int b = 0; b < keywords.Length; ++b)
				{
					if (tl.Contains(keywords[b])) ++matches;
				}
				if (matches == keywords.Length) list.Add(s.name);
			}
		}
		//list.Sort(CompareString);
		return list;
	}

	/// <summary>
	/// Create a 2D Plane.
	/// </summary>

	public void CreateSprite(GameObject go, Rect uvRect, Color color)
	{
		// create meshFilter if new
		MeshFilter meshFilter = (MeshFilter)go.GetComponent(typeof(MeshFilter));
		if (meshFilter == null)
			meshFilter = (MeshFilter)go.AddComponent(typeof(MeshFilter));

		// create mesh if new
		Mesh mesh = meshFilter.sharedMesh;

		if (mesh == null)
			mesh = new Mesh();

		mesh.Clear();

		// setup rendering
		MeshRenderer meshRenderer = (MeshRenderer)go.GetComponent(typeof(MeshRenderer));
		if (meshRenderer == null)
			meshRenderer = (MeshRenderer)go.AddComponent(typeof(MeshRenderer));

		meshRenderer.GetComponent<Renderer>().sharedMaterial = spriteMaterial;
		// create the mesh geometry
		// Unity winding order is counter-clockwise when viewed
		// from behind and facing forward (away)
		// Unity winding order is clockwise when viewed
		// from behind and facing behind
		// 1---2
		// | / |
		// | / |
		// 0---3
		Vector3[] newVertices;
		int[] newTriangles;
		Vector2[] uvs;

		float hExtent = texture.width * 0.5f;
		float vExtent = texture.height * 0.5f;

		newVertices = new Vector3[4];
		newVertices[0] = new Vector3(-hExtent, -vExtent, 0);
		newVertices[1] = new Vector3(-hExtent, vExtent, 0);
		newVertices[2] = new Vector3(hExtent, vExtent, 0);
		newVertices[3] = new Vector3(hExtent, -vExtent, 0);

		newTriangles = new int[] { 0, 1, 2, 0, 2, 3 };
		uvs = new Vector2[4];
		uvs[0] = new Vector2(uvRect.x, uvRect.y);
		uvs[1] = new Vector2(uvRect.x, uvRect.y + uvRect.height);
		uvs[2] = new Vector2(uvRect.x + uvRect.width, uvRect.y + uvRect.height);
		uvs[3] = new Vector2(uvRect.x + uvRect.width, uvRect.y);

		Color[] vertColors = new Color[4];
		vertColors[0] = color;
		vertColors[1] = color;
		vertColors[2] = color;
		vertColors[3] = color;

		// update the mesh
		mesh.vertices = newVertices;
		mesh.colors = vertColors;
		mesh.uv = uvs;
		mesh.triangles = newTriangles;

		// generate some some normals for the mesh
		//mesh.normals = new Vector3[4];
		//mesh.RecalculateNormals();

		/*if (File.Exists(meshPath) == true)
		{
			File.Delete(meshPath);
			AssetDatabase.Refresh();
		}
		AssetDatabase.CreateAsset(mesh, meshPath);
		AssetDatabase.Refresh();*/
		meshFilter.sharedMesh = mesh;
		// add collider
		//go.AddComponent(typeof(MeshCollider));
	}

	/// <summary>
	/// Change the sprite of 2D Mesh.
	/// </summary>

	public void ChangeSprite(Mesh mesh, Rect uvRect)
	{
		Vector2[] uvs = new Vector2[4];
		uvs[0] = new Vector2(uvRect.x, uvRect.y);
		uvs[1] = new Vector2(uvRect.x, uvRect.y + uvRect.height);
		uvs[2] = new Vector2(uvRect.x + uvRect.width, uvRect.y + uvRect.height);
		uvs[3] = new Vector2(uvRect.x + uvRect.width, uvRect.y);
		mesh.uv = uvs;
	}

	/// <summary>
	/// Change the color of 2D Mesh
	/// </summary>

	public void ChangeColor(Mesh mesh, Color color)
	{
		Color[] vertColors = new Color[4];
		vertColors[0] = color;
		vertColors[1] = color;
		vertColors[2] = color;
		vertColors[3] = color;
		mesh.colors = vertColors;
	}
}

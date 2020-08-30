/// <summary>
/// This is Radar System. using to detection an objects and showing on minimap by Tags[]
/// </summary>
using UnityEngine;
using System.Collections;
namespace RadarSystem{
	
public enum Alignment
{
	None,
	LeftTop,
	RightTop,
	LeftBot,
	RightBot ,
	MiddleTop ,
	MiddleBot,
	LeftMiddle,
	RightMiddle,
}

public class RadarSystem : MonoBehaviour
{
	 public bool showOut = false;
	private Vector2 inposition;
		public float heightRate = 0.2f;
		public float Size {
			get{
				return Screen.height * heightRate;
			}
		} // size of minimap
	public float Distance = 100;// maximum distance of objects
	public Texture2D[] Navtexture;// textutes list
	public Texture2D NavCompass;// compass texture
	public Texture2D NavBG;// background texture
	public Vector2 PositionOffset = new Vector2 (0, 0);// minimap position offset
	public Alignment PositionAlignment = Alignment.None;// position alignment
	public float Scale = 1;// mini map scale ( Scale < 1 = zoom in , Scale > 1 = zoom out)
	public float ScaleIndicator = 1;
	public bool MapRotation;
	public GameObject Player;
	static public bool Show = true;
	public Color ColorMult = Color.white;
	
	void Start ()
	{
	
	}

	void Update ()
	{
		if (!Player) {
			Player = this.gameObject;
		}
		
		if (Scale <= 0) {
			Scale = 1;
		}
		Vector2 offset = PositionOffset;
			offset.x *= Screen.width;
			offset.y *= Screen.height;
		switch (PositionAlignment) {
		case Alignment.None:
				inposition = offset;
			break;
		case Alignment.LeftTop:
				inposition = Vector2.zero + offset;
			break;
		case Alignment.RightTop:
				inposition = new Vector2 (Screen.width - Size, 0) + offset;
			break;
		case Alignment.LeftBot:
				inposition = new Vector2 (0, Screen.height - Size) + offset;
			break;
		case Alignment.RightBot:
				inposition = new Vector2 (Screen.width - Size, Screen.height - Size) + offset;
			break;
		case Alignment.MiddleTop:
				inposition = new Vector2 ((Screen.width / 2) - (Size / 2), Size) + offset;
			break;
		case Alignment.MiddleBot:
				inposition = new Vector2 ((Screen.width / 2) - (Size / 2), Screen.height - Size) + offset;
			break;
		case Alignment.RightMiddle:
				inposition = new Vector2 ((Screen.width) - (Size), (Screen.height/2 )- (Size/2)) + offset;
			break;
		case Alignment.LeftMiddle:
				inposition = new Vector2 ( 0, (Screen.height/2 )- (Size/2)) + offset;
			break;
		}
		
	}
	
	Vector2 ConvertToNavPosition (Vector3 pos)
	{
		Vector2 res = Vector2.zero;
		if (Player) {
			res.x = inposition.x + (((pos.x - Player.transform.position.x) + (Size * Scale) / 2f) / Scale);
			res.y = inposition.y + ((-(pos.z - Player.transform.position.z) + (Size * Scale) / 2f) / Scale);
		}
		return res;
	}
	void DrawNav (RaderItem[] items)
	{
		if (Player!=null && items!=null) {
			for (int i=0; i<items.Length; i++) {
				if (items [i] == null) {
					continue;
				}
				if (Distance > 0f && (items [i].m_trans.position - Player.transform.position).magnitude > Distance) {
					continue;
				}
				Texture2D navtexture = Navtexture [items [i].index];
				Vector2 pos = ConvertToNavPosition (items [i].m_trans.position);
				Vector2 pcenter = (inposition + new Vector2 (Size / 2f, Size / 2f));
				if (Vector2.Distance (pos, pcenter ) + (navtexture.width / 2) > (Size / 2f)) {
					if (showOut) {
						pos = (pos-pcenter).normalized * (Size * 0.5f-navtexture.width*0.5f) +pcenter;
					} else {
						continue;
					}
				}
				float navscale = Scale;
				if (navscale < 1) {
					navscale = 1;
				}
				GUI.DrawTexture (new Rect (pos.x - (navtexture.width *Size /navtexture.height/ navscale * ScaleIndicator) / 2, pos.y - (navtexture.height*Size /navtexture.height/ navscale * ScaleIndicator) / 2, navtexture.width*Size /navtexture.height/ navscale * ScaleIndicator, navtexture.height*Size /navtexture.height / navscale * ScaleIndicator), navtexture);
			}
		}
	}
	RaderItem[] _items = null;
	RaderItem[] m_Items {
		get{
			if (_items == null) {
				_items = GameObject.FindObjectsOfType<RaderItem> ();
			}
			return _items;
		}
		set{
			_items = GameObject.FindObjectsOfType<RaderItem> ();
		}
	}
	float udpateTime = 0f;
	void OnGUI ()
	{
		if (!Show)
			return;
			if (Time.time>udpateTime) {
				m_Items = null;
				udpateTime = Time.time+3f;
			}
		
		//	GUI.color = Color.Lerp(ColorMult,Color.black,Mathf.PingPong( Time.time,1f));
			GUI.color = ColorMult;
		if (MapRotation) {
			GUIUtility.RotateAroundPivot (-(this.transform.eulerAngles.y), inposition + new Vector2 (Size / 2f, Size / 2f)); 
		}
		if (NavBG)
			GUI.DrawTexture (new Rect (inposition.x, inposition.y, Size, Size), NavBG);
		GUIUtility.RotateAroundPivot ((this.transform.eulerAngles.y), inposition + new Vector2 (Size / 2f, Size / 2f)); 
			if (NavCompass) {
				GUI.DrawTexture (new Rect (inposition.x + (Size / 2f) - (Size/2), inposition.y + (Size / 2f) - (Size/2),Size,Size), NavCompass);
			}
		GUIUtility.RotateAroundPivot (-(this.transform.eulerAngles.y), inposition + new Vector2 (Size / 2f, Size / 2f)); 
		DrawNav (m_Items);

	}
}
}




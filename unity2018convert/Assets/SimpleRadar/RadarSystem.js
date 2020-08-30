enum Alignment
{
	None,
	LeftTop,
	RightTop,
	LeftBot,
	RightBot ,
	MiddleTop ,
	MiddleBot
}

	private var inposition:Vector2;
	public var Size:float = 400; // size of minimap
	public var Distance:float = 100;// maximum distance of objects
	public var Navtexture:Texture2D[];// textutes list
	public var EnemyTag:String[];// object tags list
	public var NavCompass:Texture2D ;// compass texture
	public var NavBG:Texture2D;// background texture
	public var PositionOffset:Vector2  = new Vector2 (0, 0);// minimap position offset
	public var PositionAlignment:Alignment  = Alignment.None;// position alignment
	public var Scale:float = 1;// mini map scale ( Scale < 1 = zoom in , Scale > 1 = zoom out)
	public var ScaleIndicator:float = 1;
	public var MapRotation:boolean;
	public var Player:GameObject;
	public var Show:boolean = true;
	public var ColorMult:Color = Color.white;
	
	function Start ()
	{
	
	}

	function Update ()
	{
		if (!Player) {
			Player = this.gameObject;
		}
		
		if (Scale <= 0) {
			Scale = 1;
		}
	
		switch (PositionAlignment) {
		case Alignment.None:
			inposition = PositionOffset;
			break;
		case Alignment.LeftTop:
			inposition = Vector2.zero + PositionOffset;
			break;
		case Alignment.RightTop:
			inposition = new Vector2 (Screen.width - Size, 0) + PositionOffset;
			break;
		case Alignment.LeftBot:
			inposition = new Vector2 (0, Screen.height - Size) + PositionOffset;
			break;
		case Alignment.RightBot:
			inposition = new Vector2 (Screen.width - Size, Screen.height - Size) + PositionOffset;
			break;
		case Alignment.MiddleTop:
			inposition = new Vector2 ((Screen.width / 2) - (Size / 2), Size) + PositionOffset;
			break;
		case Alignment.MiddleBot:
			inposition = new Vector2 ((Screen.width / 2) - (Size / 2), Screen.height - Size) + PositionOffset;
			break;
		}
		
	}
	
	function ConvertToNavPosition (pos:Vector3 ):Vector2
	{
		var res:Vector2  = Vector2.zero;
		if (Player) {
			res.x = inposition.x + (((pos.x - Player.transform.position.x) + (Size * Scale) / 2f) / Scale);
			res.y = inposition.y + ((-(pos.z - Player.transform.position.z) + (Size * Scale) / 2f) / Scale);
		}
		return res;
	}

	function DrawNav (enemylists:GameObject[] , navtexture:Texture2D )
	{
		if (Player) {
			for (var i:int=0; i<enemylists.Length; i++) {
				if (Vector3.Distance (Player.transform.position, enemylists [i].transform.position) <= (Distance * Scale)) {
					var pos:Vector2  = ConvertToNavPosition (enemylists [i].transform.position);
				
					if (Vector2.Distance (pos, (inposition + new Vector2 (Size / 2f, Size / 2f))) + (navtexture.width / 2) < (Size / 2f)) {
						 var navscale:float = Scale;
						if (navscale < 1) {
							navscale = 1;
						}
						GUI.DrawTexture (new Rect (pos.x - (navtexture.width / navscale * ScaleIndicator) / 2, pos.y - (navtexture.height / navscale * ScaleIndicator) / 2, navtexture.width / navscale * ScaleIndicator, navtexture.height / navscale * ScaleIndicator), navtexture);
					}
				}
			}
		}
	}

	function OnGUI ()
	{
		if (!Show)
			return;
		
		GUI.color = ColorMult;
		if (MapRotation) {
			GUIUtility.RotateAroundPivot (-(this.transform.eulerAngles.y), inposition + new Vector2 (Size / 2f, Size / 2f)); 
		}
	
		for (var i:int=0; i<EnemyTag.Length; i++) {
			DrawNav (GameObject.FindGameObjectsWithTag (EnemyTag [i]), Navtexture [i]);
		}
		if (NavBG)
			GUI.DrawTexture (new Rect (inposition.x, inposition.y, Size, Size), NavBG);
		GUIUtility.RotateAroundPivot ((this.transform.eulerAngles.y), inposition + new Vector2 (Size / 2f, Size / 2f)); 
		if (NavCompass)
			GUI.DrawTexture (new Rect (inposition.x + (Size / 2f) - (NavCompass.width / 2f), inposition.y + (Size / 2f) - (NavCompass.height / 2f), NavCompass.width, NavCompass.height), NavCompass);

	}



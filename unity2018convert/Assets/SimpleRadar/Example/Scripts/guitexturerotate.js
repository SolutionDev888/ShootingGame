public var testTexture: Texture2D = null;
var		tacho_texture 		: Texture;

function OnGUI(){
//This is the tachotexture
GUI.Label( Rect( 16, 16, 128, 128 ), tacho_texture );	// x position, y position, size x, size y

//Here comes the tachoneedle rotation
var matrixBackup:Matrix4x4  = GUI.matrix;
var thisAngle:float = Time.frameCount * 2;
var pos:Vector2 = Vector2(80,80); // rotatepoint in texture plus x/y coordinates. our needle is at 16/16. Texture is 128/128. Makes middle 64 plus 16 = 80

GUIUtility.RotateAroundPivot(thisAngle, pos);

var thisRect:Rect = Rect(16,16,128,128); //x position, y position, size x, size y

GUI.DrawTexture(thisRect, testTexture);  
GUI.matrix = matrixBackup; 
}
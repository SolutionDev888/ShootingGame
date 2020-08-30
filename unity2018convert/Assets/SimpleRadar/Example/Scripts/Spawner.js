public var ObjectSpawn : GameObject;
private var timetemp = 0;
public var timeSpawn : float = 3;
public var enemyCount = 0;
public var radiun :int;


function Start () {
   if(GetComponent.<Renderer>()){
   	   GetComponent.<Renderer>().enabled = false;
   }
   timetemp = Time.time;
}

function Update () {
   var gos : GameObject[];
   gos = GameObject.FindGameObjectsWithTag(ObjectSpawn.tag);

   if(gos.length < enemyCount){
  
   if(Time.time > timetemp+timeSpawn){
   	  timetemp = Time.time;
   	  
   	  var positionSpawn = transform.position+ new Vector3(Random.Range(-radiun,radiun),0,Random.Range(-radiun,radiun));
   	  
   	  if(ObjectSpawn){
   	  	  var enemyCreated:GameObject = Instantiate(ObjectSpawn,positionSpawn, Quaternion.identity);
   	  }
   	  
   	  }
   }
}

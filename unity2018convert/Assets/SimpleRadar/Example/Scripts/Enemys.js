var Target:Transform;
var TargetTag:String = "Player";
var PositionTarget:Vector3;
var Speed:int = 5;
var reconLangth:int = 300;

private var aiTime:int;
private var lastPos:Vector3;

function Start () {

	PositionTarget = this.transform.position + new Vector3(Random.Range(-reconLangth,reconLangth),0,Random.Range(-reconLangth,reconLangth));
	
	if(!Target){
		if(GameObject.FindGameObjectWithTag(TargetTag)){
			Target = GameObject.FindGameObjectWithTag(TargetTag).gameObject.transform;
		}
	}
	
	if(Target){
		PositionTarget = Target.transform.position;
	}
}


function RandomDir(){
	PositionTarget = this.transform.position + new Vector3(Random.Range(-reconLangth,reconLangth),0,Random.Range(-reconLangth,reconLangth));
}
function AIState(){
	aiTime--;
	if(aiTime<=0){
		if(!Target){
			RandomDir();
		}else{
			if(Random.Range(0,10)>5){
				PositionTarget = Target.transform.position;
			}else{
				RandomDir();
			}
		}
		aiTime = Random.Range(50,300);
	}
}


function Update () {
	
	AIState();
	var deltaspeed = Vector3.Distance(transform.position,lastPos);
	PositionTarget.y = this.transform.position.y;
	var rotation = Quaternion.LookRotation(PositionTarget - transform.position);
    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 20);
	var distance = Vector3.Distance(PositionTarget,this.transform.position);
	
	
	if(distance > 20){
		// do run animation
		this.transform.position += (this.transform.forward * Speed) * Time.deltaTime;
	}
	
	if(distance <= 20 || deltaspeed< 0.1f){
		if(Target){
			// do attack animation
		}else{ 
			// do idle animation
		}
	}
	
	
	lastPos = transform.position;
	
}

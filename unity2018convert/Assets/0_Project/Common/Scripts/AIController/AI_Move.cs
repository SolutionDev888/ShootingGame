using UnityEngine;
using System.Collections;

public class AI_Move : MonoBehaviour {


    public GameObject goAIGun;
	public static bool isFire{
		get{
			return LevelSetting.isStartedFight;
		}
		set{
			LevelSetting.isStartedFight = value;
		}
	}
    protected Transform m_Trans;
    public SWS.PathManager normal_patrolRoot;
    public SWS.PathManager attrak_patrolRoot;
	public Vector3 norStatsRate = new Vector3(1f,1f,1f);
	public Vector3 norStatsTime = new Vector3(5f,8f,3f);
	public Vector3 attStatsRate = new Vector3(1f,1f,1f);
	public Vector3 attStatsTime = new Vector3(5f,6f,3f);
	public Vector2 retreatTimeRange = new Vector2 (5f,10f);
	public Transform firstAttackPosition;
	public Vector3 moveSpeed = new Vector3 (0f, 1.5f, 3f);
	public Vector3 moveRotate = new Vector3 (0f, 90f, 160f);
    public LayerMask dontWalkLayer;
    public float targetCheckRadius = 1f;
    protected CharacterController m_ChaCtrl;
    protected Rigidbody m_Rigid;
    public float moveSlowdownDistance;
	public float moveEndReachedDistance = 0.6f;
    public float curMoveSpeed {
        get
        {
			return currentMode== AIMode.Walk? moveSpeed.y :(currentMode== AIMode.Run?moveSpeed.z:moveSpeed.x);
        }
    }
    public float curTurnSpeed
    {
        get
        {
			return currentMode== AIMode.Walk? moveRotate.y :(currentMode== AIMode.Run?moveRotate.z:moveRotate.x);
        }
    }
    public float health;

	public float castRadius = 0.25f;

    public int weaponType = 0;

    public float attackOffsetAngle = -10f;
    public float findPlayerDistance = 10;


    protected Vector3 m_TargetPos;
    protected Vector3 m_StepTargetPos;
    protected Vector3 m_StepTargetDir;


    protected AI_Shooter m_shooter;

    protected Animator m_Animator;


    protected Seeker m_Seeker;                  // Cached Seeker component
    Pathfinding.Path path = null;
    private int curStepPathIndex = 0;


    private Vector3 m_footPos;
    int h_weaponType = Animator.StringToHash("WeaponType");
    int h_IsReloading = Animator.StringToHash("IsReloading");
    int h_StartReload = Animator.StringToHash("StartReload");
    int h_HitType = Animator.StringToHash("HitType");
    int h_IsRunning = Animator.StringToHash("IsRunning");
    int h_IsMoving = Animator.StringToHash("IsMoving");
    int h_Forward = Animator.StringToHash("Forward");
    int h_IsAttacking = Animator.StringToHash("IsAttacking");
    int h_Pitch = Animator.StringToHash("Pitch");
	int h_IsCrouching = Animator.StringToHash("IsCrouching");

    
    private void Awake()
    {
        m_Animator = GetComponentInChildren<Animator>();
        m_Trans = transform;
        m_ChaCtrl = GetComponentInChildren<CharacterController>();
        m_Rigid = GetComponentInChildren<Rigidbody>();
        m_Seeker = GetComponentInChildren<Seeker>();
        m_shooter = GetComponentInChildren<AI_Shooter>();
        m_Seeker.pathCallback += OnPathComplete;
        m_Animator.SetInteger(h_weaponType, weaponType);
		if (m_shooter != null)
		{
			m_Animator.SetInteger(h_weaponType, weaponType);
		}
		else
		{
			m_Animator.SetInteger(h_weaponType, 0);
		}
    }
    public virtual void OnDestroy()
    {

        if (path != null)
            path.Release(this);

    }
    protected virtual void OnEnable()
    {
    }

    protected virtual void OnDisable()
    {
    }
    public virtual void TryReload()
    {
        if (m_shooter.maxAmmo == 0 || m_Animator.GetBool(h_IsReloading))
        {
            return;
        }
		if (m_Animator.GetBool (h_IsReloading))
			return;
		AIProcessForce (Random.Range (0f, 1f) > 0.8f ? AIMode.Crouch : AIMode.Run);
		if(m_Animator.GetBool(h_IsCrouching))
			m_Animator.Play("ReloadCrouching",4,0f);
		else
			m_Animator.Play("Reload",4,0f);
		m_Animator.SetLayerWeight(4,1f);
        vp_Timer.In(2f, delegate ()
        {
				if(m_shooter==null)return;
            if (m_shooter.maxAmmo < m_shooter.clipSize)
            {
                if (m_shooter.maxAmmo == -1)
                {
                    m_shooter.currentAmmo = m_shooter.clipSize;
                } else { 
                    m_shooter.currentAmmo = m_shooter.maxAmmo;
                    m_shooter.maxAmmo = 0;
                }
            }
            else
			{
                m_shooter.currentAmmo = m_shooter.clipSize;
                m_shooter.maxAmmo -= m_shooter.clipSize;
            }
            m_Animator.SetBool(h_IsReloading, false);
			m_Animator.SetLayerWeight(4,0f);
        });
    }
	vp_Timer.Handle crouchHandle = new vp_Timer.Handle();
	vp_Timer.Handle damageHandle = new vp_Timer.Handle();
	bool isDamage =false;
    public void Damage(float damage)
    {
        if (health <= 0 || m_Animator.enabled == false)
        {
            return;
        }
        health -= damage;
		isFire = true;
        if (health <= 0)
        {
            this.CancelInvoke();
            LevelSetting.ME.remainEnemyCount--;
            m_Animator.enabled = false;
            m_ChaCtrl.enabled = false;
            this.enabled = false;
            if(m_shooter!=null) m_shooter.weaponThrow();
            foreach (DamageController child in this.GetComponentsInChildren<DamageController>())
                child.Die();
			RadarSystem.RaderItem raderItme = GetComponent<RadarSystem.RaderItem> ();
			if(raderItme!=null)Component.Destroy (raderItme);
            GameObject.Destroy(this.gameObject, 5f);
       }
        else
        {
            m_Animator.SetLayerWeight(3, 1f);
            m_Animator.SetFloat(h_HitType, Random.Range(0f, 1f));
            m_Animator.Play("Hit",3);
			if (damageHandle.Active)
				damageHandle.Cancel ();
			isDamage = true;
			vp_Timer.In(1.5f, delegate ()
				{
					if(m_Animator==null)return;
					m_Animator.SetLayerWeight(3, 0f);
					isDamage =false;
					AIProcessForce (Random.Range (0f, 1f) > 0.8f ? AIMode.Crouch : AIMode.Run);
				},damageHandle);
        }
    }
    public virtual void OnPathComplete(Pathfinding.Path _p)
    {
        Pathfinding.ABPath p = _p as Pathfinding.ABPath;
        if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");
        if (path != null) path.Release(this);
        p.Claim(this);
        path = p;
        curStepPathIndex = 0;
        //If it couldn't (error == true), then a message has probably been logged to the console
        //however it can also be got using p.errorLog
        //if (p.error)
    }
    private Vector3 getTargetPos()
    {
        Vector3 targetPos = Vector3.zero;
		if (normal_patrolRoot != null && normal_patrolRoot.waypoints.Length>0 && isFire==false)
        {
            targetPos = normal_patrolRoot.waypoints[Random.Range(0, normal_patrolRoot.waypoints.Length * 3) % normal_patrolRoot.waypoints.Length].position;
        }
		if (isFirstTarget==false && isFire == true && firstAttackPosition!=null) {
			isFirstTarget = true;
			targetPos = firstAttackPosition.position;
		}else if (isFire == true)
        {
			if (attrak_patrolRoot != null && attrak_patrolRoot.waypoints.Length > 0)
				targetPos = attrak_patrolRoot.waypoints [Random.Range (0, attrak_patrolRoot.waypoints.Length * 3) % attrak_patrolRoot.waypoints.Length].position;
			else if (normal_patrolRoot != null && normal_patrolRoot.waypoints.Length > 0 ) {
				targetPos = normal_patrolRoot.waypoints[Random.Range(0, normal_patrolRoot.waypoints.Length * 3) % normal_patrolRoot.waypoints.Length].position;
			}
		}
		if (targetPos != Vector3.zero) {
			m_Seeker.StartPath (m_footPos, targetPos);
			isTarget = true;
		}
        return targetPos;
    }

    // Use this for initialization
    void Start () {
        //if (gameObject.name == "Soldier (5)")
        //{
        //    goAIGun.transform.localPosition = new Vector3(0.042f, 0.09f, 0.034f);
        //    goAIGun.transform.localRotation = Quaternion.Euler(new Vector3(-72.43719f, -13.00189f, 149.4372f));
        //}
        AIProcess ();
    }
    // Update is called once per frame
    private void OnAnimatorMove()
    {

    }
	private void FixedUpdate(){
		if (currentMode == AIMode.Attack) {
			float xAngle = m_Trans.forward.deltaXAngle(PlayerMark.instance.m_Trans.position - m_shooter.m_Trans.position);
//			m_Animator.SetFloat(h_Pitch, 0f,0.1f,Time.fixedDeltaTime);
			m_Animator.SetFloat(h_Pitch, xAngle/85f ,0.3f,Time.fixedDeltaTime);
		} else {
			m_Animator.SetFloat(h_Pitch, 0f,0.1f,Time.fixedDeltaTime);
		}
	}

	bool canAttack = false;
	float scanCanAttack = 1f;
	vp_Timer.Handle aiShootHandle = new vp_Timer.Handle();
    private void LateUpdate()
    {
		Vector3 targetDir = PlayerMark.instance.m_Trans.position - m_Trans.position;
		if (isFire == false && targetDir.magnitude < findPlayerDistance) {
			isFire = true;
		}
		if (health <= 0f || m_shooter==null || currentMode!= AIMode.Attack)
			return;
		targetDir = PlayerMark.instance.m_Trans.position - m_shooter.m_Trans.position;
		if (Time.time>scanCanAttack) {
			scanCanAttack = Time.time + 1f;
			RaycastHit rhit;
			if (Physics.SphereCast (m_shooter.m_Trans.position+targetDir.normalized*1f,castRadius, targetDir.normalized, out rhit, targetDir.magnitude,LevelSetting.ME.enemybulletMask)) {
				canAttack = rhit.collider.gameObject.layer == PlayerMark.instance.gameObject.layer;
			} else {
				canAttack = false;
			}
		}
		if (canAttack) {
			float yAngle = m_Trans.forward.deltaYAngle (PlayerMark.instance.m_Trans.position - m_shooter.m_Trans.position);
			if (Mathf.Abs (yAngle) < Mathf.Abs (attackOffsetAngle) + 1f) {
				if (aiShootHandle.Active == true)
					aiShootHandle.Cancel ();
				m_shooter.TryFire ();
			}
		} else {
			if(aiShootHandle.Active==false)
				vp_Timer.In(3f, delegate (){
				AIProcessForce (AIMode.Run);
			} ,aiShootHandle );
		}
	}
	bool isTarget=false;
	bool isFirstTarget = false;
	float aiTime = 1f;
	vp_Timer.Handle aiHandle = new vp_Timer.Handle();
	public enum AIMode{
		None,
		Idle,
		Walk,
		Run,
		Crouch,
		Attack,
	}
	public AIMode p_currentMode = AIMode.None;
	public AIMode currentMode{
		get{
			return p_currentMode;
		}
		set{
			switch (p_currentMode) {
			case AIMode.Idle:
                if (goAIGun != null)
                {
                    goAIGun.transform.localPosition = new Vector3(0.042f, 0.09f, 0.034f);
                    goAIGun.transform.localRotation = Quaternion.Euler(new Vector3(-72.43719f, -13.00189f, 149.4372f));
                }
                break;
			case AIMode.Walk:
				break;
			case AIMode.Run:
				break;
			case AIMode.Crouch:
				m_Animator.SetBool (h_IsCrouching, false);
				break;
			case AIMode.Attack:
				m_Animator.SetBool (h_IsAttacking, false);
                if (goAIGun != null)
                {
                    goAIGun.transform.localPosition = new Vector3(0.042f, 0.09f, 0.034f);
                    goAIGun.transform.localRotation = Quaternion.Euler(new Vector3(-72.43719f, -13.00189f, 149.4372f));
                }

                break;
			}

			switch (value) {
			case AIMode.Idle:
                if (goAIGun != null)
                {
                    goAIGun.transform.localPosition = new Vector3(0.042f, 0.09f, 0.034f);
                    goAIGun.transform.localRotation = Quaternion.Euler(new Vector3(-72.43719f, -13.00189f, 149.4372f));
                }
                break;
			case AIMode.Walk: 
				if(isTarget==false)
				m_TargetPos = getTargetPos ();
				break;
			case AIMode.Run:
				if(isTarget==false)
				m_TargetPos = getTargetPos ();
				break;
			case AIMode.Crouch:
				m_Animator.SetBool (h_IsCrouching, true);
				break;
			case AIMode.Attack:
				canAttack = true;
				m_Animator.SetBool (h_IsAttacking, true);
                if (goAIGun != null)
                {
                    goAIGun.transform.localPosition = new Vector3(0.04555245f, 0.08925228f, 0.02942923f);
                    goAIGun.transform.localRotation = Quaternion.Euler(new Vector3(-72.38306f, 107.1372f, 32.17234f));
                }

                break;
			}
			p_currentMode = value;
		}
	}
	void AIProcessForce(AIMode mode){
		if (aiHandle.Active)
			aiHandle.Cancel ();
		currentMode = mode;
		Vector3 rate = isFire ? norStatsRate : attStatsRate;
		if ((mode == AIMode.Walk && rate.y<=0f)||(mode == AIMode.Run && rate.z<=0f)|| ((mode == AIMode.Walk||mode == AIMode.Run)&&isTarget == false)) {
			if (isFire) {
				currentMode = AIMode.Crouch;
			} else {
				currentMode = AIMode.Idle;
			}
		}
		vp_Timer.In (Random.Range(3f,6f), AIProcess, aiHandle);
	}
	void AIProcessNext(){
		if (aiHandle.Active)
			aiHandle.Cancel ();
		AIProcess ();
	}
	void AIProcess(){
		if (this == null)
			return;
		Vector3 rate = isFire ? norStatsRate : attStatsRate;
		Vector3 time = isFire ? norStatsTime : attStatsTime;
		switch (currentMode) {
		case AIMode.None:
			currentMode = AIMode.Idle;
			vp_Timer.In (Random.Range(1f,rate.x), AIProcess, aiHandle);
			break;
		case AIMode.Idle:
			float r = Random.Range (0f, rate.y + rate.z);
			if (r <= rate.y && rate.y > 0f) {
				currentMode = AIMode.Walk;
				vp_Timer.In (Random.Range (3f, rate.y), AIProcess, aiHandle);
			} else if (r >= rate.y && rate.z > 0f) {
				currentMode = AIMode.Run;
				vp_Timer.In (Random.Range (3f, rate.z), AIProcess, aiHandle);
			} else {
				if (isFire) {
					currentMode = AIMode.Attack;
					vp_Timer.In (Random.Range (3f, 7f), AIProcess, aiHandle);
				} else {
					currentMode = AIMode.Idle;
					vp_Timer.In (Random.Range(1f,rate.x), AIProcess, aiHandle);
				}
			}
			break;
		case AIMode.Walk:
			if (isTarget) {
				currentMode = AIMode.Run;
				vp_Timer.In (Random.Range (3f, rate.z), AIProcess, aiHandle);
			} else {
				if (isFire) {
					currentMode = AIMode.Crouch;
					vp_Timer.In (Random.Range (3f, 7f), AIProcess, aiHandle);
				} else {
					currentMode = AIMode.Idle;
					vp_Timer.In (Random.Range (3f, rate.x), AIProcess, aiHandle);
				}
			}
			break;
		case AIMode.Run:
			if (isTarget) {
				currentMode = AIMode.Walk;
				vp_Timer.In (Random.Range (3f, rate.x), AIProcess, aiHandle);
			} else {
				if (isFire) {
					currentMode = AIMode.Crouch;
					vp_Timer.In (Random.Range (3f, 7f), AIProcess, aiHandle);
				} else {
					currentMode = AIMode.Idle;
					vp_Timer.In (Random.Range (3f, rate.x), AIProcess, aiHandle);
				}
			}
			break;
		case AIMode.Crouch:
			if (isFire && m_shooter!=null) {
				currentMode = AIMode.Attack;
				vp_Timer.In (Random.Range (3f, 7f), AIProcess, aiHandle);
			} else {
				currentMode = AIMode.Run;
				vp_Timer.In (Random.Range (3f, rate.z), AIProcess, aiHandle);
			}
			break;
		case AIMode.Attack:
			currentMode = AIMode.Crouch;
			vp_Timer.In (Random.Range (3f, 7f), AIProcess, aiHandle);
			break;
		}
	}
    void Update () {
		if (health <= 0f)
			return;
		m_footPos = m_Trans.position;
		if (isDamage==false &&(currentMode== AIMode.Walk || currentMode == AIMode.Run)) {
			if (path != null && curStepPathIndex < path.path.Count) {
				Vector3 dir = Vector3.zero;
				float targetDist = 0f;
				while (curStepPathIndex < path.path.Count) {
					dir = (Vector3)path.path [curStepPathIndex].position - m_footPos;
					dir.y = 0;
					targetDist = dir.magnitude;
					if (targetDist > moveEndReachedDistance)
						break;
					curStepPathIndex++;
				}
				if (targetDist > moveEndReachedDistance) {
					float slowdown = Mathf.Clamp01 (targetDist / moveSlowdownDistance);
					Vector3 forward = m_Trans.forward;
					float dot = Vector3.Dot (dir.normalized, forward);
					float sp = curMoveSpeed * Mathf.Max (dot, .05f) * slowdown;
					m_StepTargetPos = forward * sp;
					m_StepTargetDir = dir;
					m_Animator.SetFloat (h_Forward, 1f, 0.15f, Time.deltaTime);
				} else {
					if (curStepPathIndex >= path.path.Count - 1) {
						isTarget = false;
						AIProcessNext ();
					}
					m_StepTargetDir = Vector3.zero;
					m_Animator.SetFloat (h_Forward, 0f, 0.3f, Time.deltaTime);
				}
			} else //if (m_Seeker.IsDone ()) 
			{
				if (m_Seeker.IsDone ()) 
					isTarget = false;
				m_StepTargetPos = Vector3.zero;
				m_StepTargetDir = Vector3.zero;
				m_TargetPos = Vector3.zero;
				m_Animator.SetFloat (h_Forward, 0f, 0.3f, Time.deltaTime);
			}
			Move (m_StepTargetPos);
		} else {
			m_StepTargetPos = Vector3.zero;
			m_StepTargetDir = Vector3.zero;
			m_Animator.SetFloat (h_Forward, 0f, 0.3f, Time.deltaTime);
			Move (Vector3.zero);
		}
		//Rotate towards targetDirection
		if (m_ChaCtrl != null) {
			if (m_ChaCtrl.velocity.magnitude > 1)
				return;
			else {
			}
		}
    }
	private void Move(Vector3 moveDir){
		if (currentMode == AIMode.Walk){// && moveDir != Vector3.zero && m_Animator.GetFloat (h_Forward) > 0.2f) {
			if (m_Animator.GetBool (h_IsRunning) != false)
				m_Animator.SetBool (h_IsRunning, false);
			if (m_Animator.GetBool (h_IsMoving) == false)
				m_Animator.SetBool (h_IsMoving, true);
		} else if (currentMode == AIMode.Run){// && moveDir != Vector3.zero && m_Animator.GetFloat (h_Forward) > 0.2f) {
			if (m_Animator.GetBool (h_IsRunning) == false)
				m_Animator.SetBool (h_IsRunning, true);
			if (m_Animator.GetBool (h_IsMoving) == false)
				m_Animator.SetBool (h_IsMoving, true);
		} else {
			if (m_Animator.GetBool (h_IsRunning) != false)
				m_Animator.SetBool (h_IsRunning, false);
			if (m_Animator.GetBool (h_IsMoving) != false)
				m_Animator.SetBool (h_IsMoving, false);
		}
		if (m_ChaCtrl != null)
			m_ChaCtrl.SimpleMove (moveDir);
		else if (m_Rigid != null)
			m_Rigid.AddForce (moveDir);
		else
			m_Trans.Translate (moveDir * Time.deltaTime, Space.World);

		if (currentMode == AIMode.Attack || currentMode == AIMode.Crouch) {
			float sign = (m_Trans.forward.deltaYAngle (PlayerMark.instance.m_Trans.position - (m_shooter==null?m_Trans.position:m_shooter.m_Trans.position)) - attackOffsetAngle);
			//sign = (sign < 0f ? -1 : (sign > 0f ? 1f : 0f));
			Vector3 euler = m_Trans.rotation.eulerAngles;
			euler.z = 0;
			euler.x = 0;
			euler.y = euler.y + moveRotate.z * Time.deltaTime * sign / 40f;
			m_Trans.rotation = Quaternion.Euler (euler);
		} else if(m_StepTargetDir != Vector3.zero){
			Quaternion rot = m_Trans.rotation;
			Quaternion toTarget = Quaternion.LookRotation(m_StepTargetDir);

			rot = Quaternion.Slerp(rot, toTarget, curMoveSpeed * Time.deltaTime);
			Vector3 euler = rot.eulerAngles;
			euler.z = 0;
			euler.x = 0;
			m_Trans.rotation = Quaternion.Euler(euler);
		}
	}
}

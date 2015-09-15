using UnityEngine;
using System.Collections;
using System;
using System.IO;
public class plSerenSword :Fighter {

	const float PS_SCALE = 1.2f;
	const int S_IDLE = 0;
	const int S_NB = 4;
	const int S_SB = 5;
	const int S_DB = 6;
	const int S_UB = 7;
	const int S_NC = 8;
	const int S_SC = 9;
	const int S_DC = 10;
	const int S_UC = 11;
	const int SAIR_DIFF = 6;
	const int S_NAIR = 21;
	const int S_FAIR = 12;
	const int S_BAIR = 23;
	const int S_DAIR = 24;
	const int S_UAIR = 25;
	//public const string MOVE_FILE = "Assets/Standard Assets/Fighters/Serenity/psysword.movescript";
	public ParticleSystem swordFlare;
	public Transform sBone;
		
		
		public SPoint psMove;
		SPoint vel;
		SPoint accl;
		public float hovHgt, hovDst, trackWgt,spdLmt, idleRotSpd;
		Quaternion oRotQuat;
		Vector3 oRotVec;
		public int atkMode;
	public int curMove=-1;
		AttkTrack[] aBox;
	Color startCol, readyCol;
		public SPoint lastMove;
		SPoint animOffset;
		STimer stunTmr;
		int cAtk;
	float[] cRespDist;
		float[] cRespTime;
	float cRespTimer;
	float[] airRespTime;
		SPoint cStartPos;//record the starting position for tweening
		Quaternion cStartOri;
		bool isFacingRight;
		bool psReady=false;
	public SPoint swordTarget;
	// Use this for initialization
	void Awake () {
		swordTarget = new SPoint (0,0);
		aBox=null;
		lastMove = new SPoint ();
		thwTmr = new STimer (throwTime);
		psMove = new SPoint ();
		accl = new SPoint ();
		vel = new SPoint ();
		stunTmr = new STimer ();
		atkMode=0;
		hovHgt = 5;
		hovDst = 7;
		trackWgt = 0.04f;
		spdLmt = 0.5f;
		idleRotSpd = 30;
		oRotQuat = new Quaternion();
		oRotVec = new Vector3(0, -1, 0);
		cRespTimer=0;
		isFacingRight=true;
		//C psMove info here
		cRespDist=new float[5];
		cRespTime=new float[5];
		cRespDist[0]=30;
		cRespDist[1]=60;
		cRespDist[2]=30;
		cRespDist[3]=30;
		cRespDist[4]=30;
		for(int i=0;i<5;i++)//square this for efficiency 
			cRespDist[i]=cRespDist[i]*cRespDist[i];
		cRespTime[0]=0.3f;
		cRespTime[1]=0.5f;
		cRespTime[2]=0.3f;
		cRespTime[3]=0.3f;
		cRespTime[4] = 0.2f;
		cStartPos=new SPoint(0, 0);
		cAtk=0;
		LoadPlayer ();
		stats.id.fighter=SERENITYSWORD;
		stats.id.num=plNum*100+11;
		stats.size.y=9;
		stats.size.x=9;

		SetPlayer ();
		startCol = new Color32 (230, 42, 84, 255);
		readyCol = new Color32 (24, 142, 230, 255);
        
    }
	public void Start(){
		fHelper.Animate ("Idle", true, 0);
		Vector3 oRotFoc = new Vector3(0, -1, 0);
		oRotFoc.Normalize();
		Quaternion q = Quaternion.FromToRotation( oRotVec,oRotFoc)*GetRotation();
		Rotate(q);
		grbTmr = new STimer (0.2f);
		debugFadeTmr = new STimer (0.5f);
    }

    void LoadPlayer(){
		attackBox = new AttkBox ();
		hitbox = new HitBox();
		fHelper = new FighterHelper ();
		LoadMoveScript (MOVE_FILE);

	}
	// Update is called once per frame
	void Update () {
		if (Time.deltaTime != 0) {
			if(debugModeOn)
				FindConsole();
			float timeLapsed = Time.deltaTime;
			if(!debugFadeTmr.IsReady()){
				if(debugFadeTmr.RunTimer (timeLapsed))
					Destroy (attkHitbox);
			}
			float cDist = new SPoint (GetPos ().x - swordTarget.x, GetPos ().y - swordTarget.y).SqDistFromOrigin ();//measure from 
			if (cDist < cRespDist [0])
					psReady = true;
			else
					psReady = false;
			if(psReady)
				swordFlare.startColor=readyCol;
			else
				swordFlare.startColor=startCol;
			float cItrp = 0;//used for C-psMove positioning
			if(curMove>0)
				curMove=-1;
            if (!atkTmr.IsReady ()) {
			
					fHelper.frame = 1.0f - atkTmr.tmr / atkTmr.GetLen ();
					if (fHelper.atkType == SPC) {
							float cTP = fHelper.aniTmr.tmr;
							float cL = fHelper.aniTmr.GetLen ();
							fHelper.frame = cTP / cL;
					} else {
							//sprAnimHolder.UpdateAnim(timeLapsed);
							fHelper.frame = 1.0f - atkTmr.tmr / atkTmr.GetLen ();
					}
					if (cRespTimer > 0) {
							cItrp = cRespTimer / cRespTime [cAtk];
							cRespTimer -= timeLapsed;
							if (cRespTimer < 0)
									cRespTimer = 0;
					}

					//fHelper.TR.rotation = Quaternion.identity;//reset orientation
					if (atkTmr.RunTimer (timeLapsed)) {
							stats.motion.pos = new SPoint (sBone.position.x, sBone.position.y);
							transform.position = sBone.position;
							atkMode = 0;//reset rotations
							Vector3 oRotFoc = new Vector3 (0, -1, 0);
							oRotFoc.Normalize ();
							Quaternion q = Quaternion.FromToRotation (oRotVec, oRotFoc) * GetRotation ();
							Rotate (q);
							if (InAttack (SPC))
									transform.rotation = Quaternion.identity;
							//fHelper.SetWeightedAnim(0);
					}

			}
			if ((atkMode > 0)) {//using a b attack
				RunMoveIns(timeLapsed);
					if ((is_C (atkMode)) || (atkMode == SPB) || (is_SwordAir (atkMode))) {
							SPoint test = new SPoint (sBone.position.x, sBone.position.y);
							if (cItrp > 0) {
									stats.motion.pos.x = cItrp * cStartPos.x + (1.0f - cItrp) * swordTarget.x;
									stats.motion.pos.y = cItrp * cStartPos.y + (1.0f - cItrp) * swordTarget.y;

							} else if(atkMode!=SPC) {
									stats.motion.pos = new SPoint (swordTarget.x, swordTarget.y);		
							}
							transform.position = new Vector3 (GetPos ().x, GetPos ().y, 0);
					}		

					Quaternion wSQqA = Quaternion.identity;//sBone.rotation;
					//if(atkMode!=SPC)
					//	Rotate(sBone.rotation);
					/*psMove.x=sBone.position.x;
psMove.y=sBone.position.y;
if(is_B(atkMode))
	psMove.y-=stats.size.y;
if((is_C(atkMode))||(atkMode==SPB)||(is_SwordAir(atkMode))){
	if(cItrp>0){
		stats.motion.pos.x = cItrp*cStartPos.x + (1.0f-cItrp)*swordTarget.x;
		stats.motion.pos.y = cItrp*cStartPos.y + (1.0f-cItrp)*swordTarget.y;
		//rotation as well
		oRotQuat = fHelper.TR.rotation;
		Quaternion q = sBone.rotation;
		q = Quaternion.Slerp( oRotQuat, q, 0.05f);
		fHelper.TR.rotation=q;
	}else{
		stats.motion.pos=new SPoint(swordTarget.x, swordTarget.y);		
	}
}		
if(!isFacingRight)
	psMove.x=-psMove.x;*/
			} else {
					Idle ();
					float swordAng = Mathf.Atan2 (swordTarget.y - GetPos ().y, swordTarget.x - GetPos ().x); 
					float swordDist = Mathf.Sqrt (Mathf.Pow (swordTarget.y - GetPos ().y, 2) + Mathf.Pow (swordTarget.x - GetPos ().x, 2)) / 30;
					swordDist = Mathf.Sqrt (swordDist);
					if (swordDist > spdLmt)
							swordDist = spdLmt;
					stunTmr.RunTimer (timeLapsed);
					if (stunTmr.IsReady ()) {//no motion during stun
							stats.motion.vel.x = swordDist * Mathf.Cos (swordAng);
							stats.motion.vel.y = swordDist * Mathf.Sin (swordAng);
					}
					
					IdleRotate ();
			}
			stats.motion.vel.x += accl.x;
			stats.motion.vel.y += accl.y;
			stats.motion.pos.x += stats.motion.vel.x;
			stats.motion.pos.y += stats.motion.vel.y;
		
			float speedRat = 0.9f;

			stats.motion.vel.x = stats.motion.vel.x*speedRat;
			stats.motion.vel.y = stats.motion.vel.y*speedRat;
			transform.position = new Vector3 (GetPos ().x, GetPos ().y, 0);
            
        }
    }
    
    
    void Load(string file, SPoint p){
		hitbox = new HitBox();
		hitbox.AllocateFirstIndices(1);//for a 1 dimensional array of vertices
		hitbox.AllocateSecondIndices(0, 4);
		hitbox.SetVertex(-stats.size.x/2, 0, 0, 3);
		hitbox.SetVertex(stats.size.x/2, 0, 0, 2);
		hitbox.SetVertex(stats.size.x/2, stats.size.y, 0, 1);
		hitbox.SetVertex(-stats.size.x/2, stats.size.y, 0, 0);	
		//fHelper.Load();
		int pNum=5;//stats.id.num;
		oRotQuat = GetRotation();
		//pPtcl.LoadSerenitySwordParticles(fHelper.spriteNode, eng,  pNum);
		//pPtcl.Emit(0, SPoint(0, 0), 0);
	}
	public void GetHit(AttkData hit){
		float scV = 1;
		stats.motion.vel.x=Mathf.Cos(hit.dir)*scV;
		stats.motion.vel.y=Mathf.Sin(hit.dir)*scV;
		atkTmr.ResetTimer();
		float stunT = hit.mag/2;
		if(stunT>1)
			stunT=1;
		stunTmr.SetTimer(stunT);
		Idle();
		
	}

	/*void LoadAnimHolder(string file, SPoint p){
		sprAnimHolder.name = file;
		sprAnimHolder.spriteNode = eng->mSceneMgr->getRootSceneNode()->createChildSceneNode(Ogre::Vector3( p.x, p.y, 0 ));
		sprAnimHolder.spriteEntity = eng->mSceneMgr->createEntity(file);
		sprAnimHolder.spriteEntity->setMaterialName("Hidden");
		sprAnimHolder.spriteNode->attachObject(sprAnimHolder.spriteEntity);
		//sprAnimHolder.spriteNode->yaw(Ogre::Degree(90));
		sprAnimHolder.spriteNode->translate(Ogre::Vector3(0, 10, 0));
		string animName;
		Ogre::AnimationatkMode* atkMode;
		Ogre::AnimationatkModeSet *anSet = sprAnimHolder.spriteEntity->getAllAnimationatkModes();
		if (false) {
			Ogre::AnimationatkModeIterator iter = anSet->getAnimationatkModeIterator();
			while (iter.hasMoreElements()) {
				animName = iter.peekNextKey();
				atkMode = iter.peekNextValue();
				atkMode->setEnabled(true);
				iter.moveNext();
			}
			eng->Render();//load into GPU
			iter = anSet->getAnimationatkModeIterator();//reset iterator
			while (iter.hasMoreElements()) {//disable all again
				animName = iter.peekNextKey();
				atkMode = iter.peekNextValue();
				atkMode->setEnabled(false);
				iter.moveNext();
			}
		}
		sprAnimHolder.animatkMode = sprAnimHolder.spriteEntity->getAnimationatkMode("NB");
		atkTmr.SetTimer(sprAnimHolder.Animate("NB", false, 0));
		sprAnimHolder.UpdateAnim(atkTmr.tmr);
	}*/

	public bool IsSwordAirReady(){
		if(!atkTmr.IsReady())
			return false;
		float cDist = new SPoint(GetPos().x-swordTarget.x, GetPos().y-swordTarget.y).SqDistFromOrigin();
		if(cDist<cRespDist[4])
			return true;
		return false;
	}
	
	public override void StartAttack(int aInd, string aS){

		int lastatkMode=atkMode;// in case of a need to go back
		if(atkTmr.IsReady()){
			curMove=aInd;
			state = 4;
			if(aInd==S_NB)//NB
				atkMode = S_NB;
			else if (aInd==S_SB)//SB
				atkMode = S_SB;
			else if (aInd==S_DB)//DB
				atkMode = S_DB;
			else if (aInd==S_UB)//UB
				atkMode = S_UB;
			else if(aInd==SPC)
				atkMode = SPC;
			else if((is_C(aInd))||(aInd==SPB)||(is_SwordAir(aInd))){
				//swordTarget.y-=hovHgt;
				float cDist = new SPoint(GetPos().x-swordTarget.x, GetPos().y-swordTarget.y).SqDistFromOrigin();//measure from 
				//sword position to target position.
				cStartPos = GetPos();
				atkMode=aInd;
				if((aInd==S_NC)&&(cDist<cRespDist[0]))//NB
					cAtk=0;
				else if((aInd==S_SC)&&(cDist<cRespDist[1]))//SB
					cAtk=1;
				else if((aInd==S_DC)&&(cDist<cRespDist[2]))//DB
					cAtk=2;
				else if((aInd==S_UC)&&(cDist<cRespDist[3]))//UB
					cAtk=3;
				else if((aInd>=S_NAIR)&&(cDist<cRespDist[4])){
					cAtk=4;
				}
				else if(atkMode!=SPB){//cannot make a C attack
					RevertState();//so undo
					atkMode=lastatkMode;
					cStartPos = new SPoint(0, 0);
				}
				if((is_C(atkMode))||(atkMode==SPB)||(is_SwordAir(atkMode))){//make sure the attack went through
					cRespTimer=cRespTime[cAtk];
					cStartOri=GetRotation();
					//cStartOri = Quaternion.Normalise(cStartOri);
				}

			}
			if(atkMode>0){

				if(is_SwordAir(atkMode))
					attackBox.ResetHits(aInd-SAIR_DIFF);
				else
					attackBox.ResetHits(aInd);
				fHelper.atkType=aInd;
				atkMode=atkMode;
				atkTmr.SetTimer(fHelper.Animate(aS, false, 0));
				if(is_B (atkMode)){
					if (moveCont [aInd] != null)
						moveCont [aInd].StartAttack ();}
			}
		}
	}
	

	public void SetFacingRight(bool b){

		if (isFacingRight!= b) {
				isFacingRight=b;
			fHelper.Pivot(180);
			ConsoleLog ("SwordPivot");
			if(b)
				ConsoleLog ("true");
			else
				ConsoleLog("false");
				
			}
	}
	public void Translate(SPoint p){
		transform.Translate (p.x, p.y, 0, Space.World);
	}
	/*string RenderEffects(GrEngine* eng){
		if(psReady)
			pPtcl.SetParticleColour(0, 0, 0.4, 0.4, 1);
		else
			pPtcl.SetParticleColour(0, 0, 1, 0.3, 0.3);
		
		return S_OK;
	}*/
	void UpdateFrame(AttkTrack[] aPtr){

		aBox=aPtr;
	}
	
	void Idle(){
		atkMode=S_IDLE;
		atkTmr.ResetTimer();
		stats.motion.pos.x+=psMove.x; //apply any psMove to pos
		stats.motion.pos.y+=psMove.y;
		psMove = new SPoint(0, 0);
		state = 0;
	}
	void Rotate(Quaternion q){
		Vector3 r = new Vector3();
		float a=0;
		q.ToAngleAxis(out a,out r);
		transform.Rotate(r, a);
		}
	void Rotate(Vector3 r){

		transform.Rotate (r);
	}
	void SetRotate(Quaternion q){
	//GameObject ggo = this.gameObject;
	//transform.Rotate (0, -90.0f, 0);
		transform.rotation = q;
	//	fHelper.spriteNode->pitch(Ogre::Degree(90));
	//	fHelper.spriteNode->yaw(Ogre::Degree(90), Ogre::Node::TransformSpace::TS_WORLD);
	}
	
	public void IdleRotate(){
		if (!fHelper.anim.GetCurrentAnimatorStateInfo (0).IsName ("Idle"))
						fHelper.Animate ("Idle", true, 0);
		if((Mathf.Abs(vel.x)<1000)||(Mathf.Abs(vel.y)<1000)){
			Vector3 oRotFoc = new Vector3(-vel.x, -vel.y-0.7f, 0);
			oRotFoc.Normalize();
			oRotQuat = GetRotation();
			Quaternion q = Quaternion.FromToRotation( oRotVec,oRotFoc)*oRotQuat;
			q= Quaternion.Slerp(oRotQuat, q,0.5f);
			//Rotate(q);
			oRotVec = new Vector3(0, -1, 0);
			oRotVec = q*oRotVec;
		}
	}
	public Quaternion GetRotation(){
		return transform.rotation;
	}
	public void Spin(float rads){
		Rotate (new Vector3((rads*180.0f)/Mathf.PI,0,0));
	}
	
	bool is_SwordAir(int aAtk){
		if((aAtk==S_NAIR)||(aAtk==S_FAIR)||(aAtk==S_BAIR)||(aAtk==S_DAIR)||(aAtk==S_UAIR))
			return true;
		else
			return false;
	}
	public void RunMoveIns(float timeLapsed){
		
		bool test1 = (fHelper.atkType != Fighter.GRAB);
		bool test2 = InState( ATTK);
		if((fHelper.atkType!=Fighter.GRAB)&&(atkMode>0)) {
			if(moveCont[fHelper.atkType]!=null){
				moveCont[fHelper.atkType].RunTimers(timeLapsed, fHelper, stats);
            }
        }
    }
	
}

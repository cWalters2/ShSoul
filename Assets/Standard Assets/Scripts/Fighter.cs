using UnityEngine;
using System.Collections;

using System;
using System.IO;

public class Fighter : MonoBehaviour {
	//unity linker vars
	public  TextAsset SCRIPT_FILE;
	public TextAsset STATS_FILE;
	public TextAsset MOVE_FILE;
	public TextAsset MOVESCRIPT;

	public const float TUMBLE_FRICRAT=0.3f;
	public const float GUARDREC = 20.0f;
	public const int LAGCANCEL = 3;
	//editable physics vars
	public Stats stats=new Stats();
	protected int state;
	public AudioClip[] audClips;
	public ParticleSystem[] effects;
	 
	public AudioSource audMain;
	public AudioClip[] audGen;
	protected float rightLip;
	protected float leftLip;
	protected float tu_vRoThresh;//the threshold to roll instead of bounce on tumble landing
	protected float tu_vReThresh;//threshold for maximum reflection vs shallow reflection
	protected float tu_bncRat;//variable to determine bounce off tumble reflection
	protected float reduceDI;
	protected float scaleDI;
	protected MoveInstruct[] moveCont;
	protected int moveInd=0;

	//player defines
	public const int SERENITY = 1;
	public const int SERENITYSWORD = 111;
	public const int YARA = 2;
	public const int KIROCH = 3;
	bool hasConsole;
	MatchConsole cons;
	protected const int AXIS_LIMIT = 100;
	public int  PROJ_LMT;
	protected const float EPS = 0.00001f;
	public const float LOWTHRESH = 0.2f;
	public const float WALK_THRESH=0.5f;
	protected const float VERT_THRESH = 0.7f;
	protected const int UP = 0;
	protected const int DOWN = 1;
	protected const int LEFT = 2;
	protected const int RIGHT = 3;

	public const int IDLE = 0;
	public const int WALK = 1;
	public const int CROUCH = 2;
	public const int DASH = 3;
	public const int ATTK = 4;
	//public const int TUMBLE = 5;
	public const int GUARD = 6;
	public const int DODGE = 7;
	public const int GRABBED = 8;
	public const int ROLLING = 9;
	public const int EDGECLIMB = 102;
	public const int FALL = 10;
	public const int AIRATK = 11;
	public const int EDGEGRAB = 101;
	public const int EDGEROLL = 103;
	public const int EDGEATTK = 104;

	public const int NA = 0;
	public const int SA = 1;
	public const int DA = 2;
	public 	const int UA = 3;
	public const int NB = 4;
	public  const int SB = 5;
	public const int DB = 6;
	public const int UB = 7;
	public const int NC = 8;
	public const int SC = 9;
	public const int DC = 10;
	public const int UC = 11;
	public const int NAIR = 12;
	public const int FAIR = 13;
	public const int BAIR = 14;
	public const int DAIR = 15;
	public const int UAIR = 16;
	public const int DATK = 17;
	public const int  LATK = 18;
	public const int GRAB = 101;
	public const int SPA = 19;
	public const int SPB = 20;
	public const int SPC = 21;
	public const int ASPA = 22;
	public const int ASPB = 23;
	public const int ASPC = 24;
	public const int MOVENUM = 25;
	public bool active=true;
	protected const int EF_HIT = 0;
	protected const int EF_DCLOUD = 1;
	public bool tPortFlag=false;
	protected const int UNIQUE_LMT = 8;//limite for number of unique movements in moveController.cs
	protected  Vector3 NORM_ORIENT;
	public int plNum;
	protected float magScale;
	protected float lastFlash=0;
	bool ledgeJumpFlag=false;
	public FighterHelper fHelper;
	public GameController gCont=new GameController();
	public HitBox hitbox;
	public Material hitboxMat;
	public Material attkBoxMat, attkBoxFade;
	public AttkBox attackBox;
	public Fighter[] fighterList;
	protected float stunMult = 1;
	public STimer atkTmr=new STimer();
	public Transform thwBone;
	public int recUsed; //keeps record of #times air recovery used
	bool dmgFlag;
	public bool debugModeOn;

	bool dmgDebugFlag;
	protected string scFile;
	protected string stFile;
	protected Fighter grabbedPlr;
	public STimer grbTmr;
	public STimer thwTmr;
	public float rollInvulnStart;
	public float rollInvulnEnd;
	public float rollBurstSpeed;
	private STimer rollInvOn;
	private STimer rollInvOff;
	protected STimer debugFadeTmr;
	protected STimer stunTimer;

	protected AttkData hitHolder;
	public float throwTime;
	public float grabTime;
    public plProjectile[] projectile;
	public GameObject debugHitbox;
	public GameObject attkHitbox;
	public GameObject guardDisplay;
	public GameObject edgeGrbBox;
	// Use this for initialization

	//these functions are intended to be callable within Unity's animator
	//These are based on, but have NOTHING to do with the MoveController
	//This does much the same thing, but takes advantage of Unity's GUI
	public void _MoveBurst(float b){
		//Applies an instant burst of speed based on parameter
		stats.walk.gndSpeed=b*fHelper.IntFacingRight();
	}
	public void _Brake(float b){
		//Multiplies the parameter by character speed.
		stats.walk.gndSpeed = stats.walk.gndSpeed * b;
	}
	public void _AirBrake(float a){
		//same as above, but stops aerial velocity
		stats.motion.vel.x = a*stats.motion.vel.x;
		stats.motion.vel.y =a*stats.motion.vel.y;
	}
	public void _AirBurstX(float x){
		stats.motion.vel.x = x;
	}
	public void _AirBurstY(float y){
		stats.motion.vel.y = y;
	}
	protected void Awake () {




	}
	public void SetRightLip(float rl){
		rightLip = rl;

	}
	public void SetLeftLip(float ll){
		leftLip = ll;
	}
	public virtual void ReloadStats(){
		//should not be called from here
    }
	public virtual float GetSpecMeter(){
		return 0;
	}
	protected void ConsoleLog(string s){
		if((debugModeOn)&&(cons != null)){
			cons.PostToConsole(s);		
		
		}
	}
	protected bool FindConsole(){
		//due to a lack of build order solutions in my project (thanks UNITY!)
		//this function exists to search for and attach the console as needed.
		//must be added to update to be effective.
		if (!debugModeOn)
						return false;
		if (cons == null) {
			GameObject tm = GameObject.FindGameObjectWithTag("Console");
			if(tm!=null){
				cons = tm.GetComponent("MatchConsole") as MatchConsole;
				return true;
			}
        }
		return false;

	
	}
	public void Start(){
		//stats=new Stats();
		FindConsole ();
		rollInvOn = new STimer ();
		rollInvOff = new STimer ();
		debugFadeTmr = new STimer (0.5f);
		atkTmr = new STimer ();
		NORM_ORIENT = new Vector3 (0, 90.0f, 0);
		if (debugModeOn)
			CreateColBox ();
		GameObject go;
		int pCt = 0;
		for (int i=1; i<5; i++) {
			go = GameObject.FindGameObjectWithTag ("Player" + i);
			if (go != null)
				pCt++;
			}
		hitHolder = null;
		stunTimer = new STimer ();
		fighterList = new Fighter[pCt-1];
		int plC=0;
		int otC = 0;
		go=new GameObject();
		for(int i=1;i<pCt+1;i++){
			go =  GameObject.FindGameObjectWithTag("Player"+i);
			if(go!=null){

				if(i!=plNum){
					fighterList[otC]=go.GetComponent<Fighter>();
					otC++;
				}
			plC++;
			}
        }
    }
	public  void PlayAnimSound(int sI){
		if (audClips.Length > sI) {
			audMain.clip = audClips [sI];
			audMain.PlayOneShot(audClips [sI]);
			
		}
	}public  void PlayGeneralSound(String s){
		if (s.CompareTo("hit")==0) {
			audMain.clip = audGen [0];
			audMain.PlayOneShot(audGen [0]);
            
        }
    }
	public  void PlayEffect(int sI){
		if (effects.Length > sI) {
			effects[sI].Emit(1);
			
		}
	}
	public void FindPNum(){
		GameObject go;
		int plCounter=1;
		for(int i=1;i<5;i++){
			go =  GameObject.FindGameObjectWithTag("Player"+i);
			if(go!=null)
				plCounter++;
		}
		plNum = plCounter;
        string tag = "Player" + plNum;
        this.tag = tag;
	}public void SetPNum(int num){

		plNum = num;
		stats.id.num = plNum;
		string tag = "Player" + num;
        this.tag = tag;
    }
   public virtual void LoadPlayer(){
		FindPNum ();
		attackBox = new AttkBox ();
		attackBox.grabRange = stats.grabRange;
		hitbox = new HitBox();
		fHelper = new FighterHelper ();
		hitbox.AllocateFirstIndices(1);//for a 1 dimensional array of vertices
		hitbox.AllocateSecondIndices(0, 4);
		hitbox.SetVertex(-stats.size.x/2, 0, 0, 3);
		hitbox.SetVertex(stats.size.x/2, 0, 0, 2);
		hitbox.SetVertex(stats.size.x/2, stats.size.y, 0, 1);
		hitbox.SetVertex(-stats.size.x/2, stats.size.y, 0, 0);
		fHelper.grabBox = new HitBox ();
		fHelper.grabBox.AllocateFirstIndices (1);
		fHelper.grabBox.AllocateSecondIndices (0, 4);
		fHelper.grabBox.SetVertex (-stats.size.x / 2, stats.size.y + stats.edgegrab.hgt, 0, 0);
		fHelper.grabBox.SetVertex (-stats.size.x / 2, stats.size.y, 0, 3);
		fHelper.grabBox.SetVertex (-stats.size.x / 2 - stats.edgegrab.wid, stats.size.y, 0, 2);
		fHelper.grabBox.SetVertex (-stats.size.x / 2 - stats.edgegrab.wid, stats.size.y + stats.edgegrab.hgt, 0, 16);
		fHelper.grabBox.isActive = false;
		fHelper.sScale=1;

        //pPtcl.LoadParticles(eng, stats.id.num, "Yara");

		//LoadMoveScript(MOVESCRIPT);
		
	}
	virtual protected void  SpecA(){}//meant to be called from descendant classes
	virtual protected void  SpecB(){}
	virtual protected void  SpecC(){}
	// Use this for initialization
	protected  void Update (){
		gCont.FrameUpdate (stats.id.num);
		FrameUpdate ();
	}
	public void Idle(){
		if( state == CROUCH)
						return;
		if (!stats.tumble.tmr.IsReady ()) {
			if(!fHelper.anim.GetCurrentAnimatorStateInfo (0).IsName ("Slam"))
				fHelper.Animate("Slam", true, 0);
			return;
        }
		if((fHelper.actionTmr.IsReady())&&(!fHelper.airborne)&&(fHelper.actionTmr.IsReady())){
			float lStart=stats.walk.loopStart;
			if((!fHelper.anim.GetCurrentAnimatorStateInfo(0).IsName("GetUp"))&& 
			   (!stats.flags.mBusy)&&(!stats.flags.aBusy)&&(Mathf.Abs(stats.walk.gndSpeed)<LOWTHRESH)){
                if ((!fHelper.InAnim("EdgeBalance")) && (leftLip != 0) && (stats.motion.pos.x + stats.size.x / 2 > leftLip))
                    fHelper.Animate("EdgeBalance", true, 0);
                else if ( (!fHelper.InAnim("EdgeBalance"))&& (rightLip != 0) && (stats.motion.pos.x - stats.size.x / 2 < rightLip))
                    fHelper.Animate("EdgeBalance", true, 0);
                else if (!fHelper.anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")&&(!fHelper.anim.GetCurrentAnimatorStateInfo(0).IsName("EdgeBalance"))&&(state!=CROUCH)){
					fHelper.Animate("Idle", true, 0);
					fHelper.animLoopStart=0;
					state=IDLE;
				}else if(state==CROUCH){
					fHelper.Animate("Crouch", true, 0);
					fHelper.animLoopStart=0;
				}


			}else if(((fHelper.anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch")))&&(Mathf.Abs(stats.walk.gndSpeed)>LOWTHRESH)){
				//crouch into walk handling
				fHelper.Animate("Walk", true, lStart);
				fHelper.animLoopStart=0;
			}
		}
	}
	public float[][] GetClashAngs(float[][] oAngs){
		if (!atkTmr.IsReady ()) {
			AttkTrack aBox = new AttkTrack (); 
			int t = 0;
			attackBox.FetchFrame (fHelper.atkType, atkTmr.GetLen (), fHelper.frame, t, new SPoint (GetPos ().x, GetPos ().y), fHelper.IsFacingRight (), aBox);//fetches frame into aBox			
			
			oAngs = new float[aBox.iLength][];
			for (int i=0; i<aBox.iLength; i++)
				oAngs [i] = new float[aBox.jLength [i]];
			//aBox.FetchSPArr (oBox);
            oAngs=aBox.atkAng;
			return oAngs;
		} else
            return null;
    }
    public SPoint[][] GetClashBox(SPoint[][] oBox){
		if (!atkTmr.IsReady ()) {
				AttkTrack aBox = new AttkTrack (); 
				int t = 0;
				attackBox.FetchFrame (fHelper.atkType, atkTmr.GetLen (), fHelper.frame, t, new SPoint (GetPos ().x, GetPos ().y), fHelper.IsFacingRight (), aBox);//fetches frame into aBox			

				oBox = new SPoint[aBox.iLength][];
				for (int i=0; i<aBox.iLength; i++)
						oBox [i] = new SPoint[aBox.jLength [i]];
				aBox.FetchSPArr (oBox);
			//oAngs=aBox.atkAng;
			if(oBox.Length==0)
				return null;
			return oBox;
		} else
				return null;
	}protected void Recoil(float t){
		//no time dependant implimented yet but soon
		fHelper.Animate ("Hit",false,0);
		state = IDLE;
		atkTmr.ResetTimer ();
	}
	protected bool ClashCheck(Fighter opp, SPoint[][] pBox, float[][] pAng){
		if (opp.atkTmr.IsReady ())
			return false;
		SPoint[][] oBox=null;
		float [][] oAngs=null;

		oBox = opp.GetClashBox (oBox);
		int pLen = pBox.Length;
		//abox = obox
		//plBox = pBox;
		if (oBox == null)
						return false;
		oAngs = opp.GetClashAngs (oAngs);
		int numPoly = pBox.Length;
		bool hitCheck = false;
		bool hitflag = false;
		int atkLen = 0;
		int pInd = 0;

		for(int oInd=0;oInd<numPoly;oInd++){//iterate per poly
			hitflag=true;
			atkLen =  pBox[oInd].Length;
			for(int j = 0; j < atkLen; j++)
				if(!CheckAxis(oBox[oInd][j], oAngs[oInd][j], pBox[pInd], oBox[oInd], pLen, oBox[oInd].Length) ) //no axis intersection
					hitflag = false;
			
			if(hitflag)//test on the axis of the player hit box to confirm
				for(int j = 0; j < pLen; j++)
					if(!CheckAxis(pBox[pInd][j], pAng[pInd][j], pBox[pInd], oBox[oInd], pLen, oBox[oInd].Length)) //no axis intersection
						hitflag = false;
			if(hitflag)
				hitCheck=true;
			
		}
		hitflag=hitCheck;

		if (hitCheck) {


			if(GetCurPriority()>=opp.GetCurPriority())
				opp.Recoil (1);
			if(GetCurPriority()<=opp.GetCurPriority()){
				Recoil(1);
				return true;
			}else
				return false;
        }else
						return false;
	}
	 public float GetCurPriority(){
		float pr = 10.0f;
		if(!atkTmr.IsReady()){
			float fr = fHelper.frame;

			AttkVtx a = attackBox.trackList[fHelper.atkType].centre;
			for(AttkVtx c = a;c!=null;c=c.next){
				if(c.frame<fr)
					pr=c.pri;
			}
			return pr;
		}
				return 1;
			}
	public virtual void AttackDetect(Fighter plr){
		bool multiHitFlag = false;
		string hitText;
		//std::stringstream log;
		bool hitflag = true;	
		int atkLen;
		int spI=0;
		int spJ=0;
		int hbLen=4;
		SPoint[] plBox;
		SPoint[][] spBox;
		float[] plAng ={0, Mathf.PI/2, Mathf.PI, -Mathf.PI/2};
		int il = 0;
		if (fHelper.atkType  == GRAB)
		     il = 1;
		else
			il = attackBox.trackList[fHelper.atkType].iLength;//num of tracks
		AttkTrack aBox = new AttkTrack(); 

		int polyHit=-1;
		plBox = plr.GetHitBox();//loads victim hitbox into plBox
		//each side is made from the vertices [i] and [+1]
		int numPoly = 0; //number of polygons in a track;
		int pInd = 0;//polygon index
		SPoint[] pHdr = new SPoint[8];
		
		//projectile checks
		for(int p=0;p<PROJ_LMT;p++){
			if(projectile[p].active){
				for(int i=0;i<8;i++)
					pHdr[i]=new SPoint(projectile[p].v[i].x+projectile[p].pos.x,projectile[p].v[i].y+projectile[p].pos.y);
				atkLen =  projectile[p].GetVNum();
				for(int j = 0; j < atkLen; j++)
					if(!CheckAxis(pHdr[0], projectile[p].ang[j], plBox, pHdr, hbLen, projectile[p].GetVNum()) ) //no axis intersection
						hitflag = false;
				
				if(hitflag)//test on the axis of the player hit box to confirm
					for(int j = 0; j < 4; j++)
						if(!CheckAxis(plBox[j], plAng[j], plBox, pHdr, hbLen, projectile[p].GetVNum())) //no axis intersection
							hitflag = false;
				if(hitflag){
					projectile[p].active=false;
					plr.GetHit(projectile[p].hitdata);
				}
				
			}
		}
		if(!atkTmr.IsReady()){//only continue if an attack is out
			if(fHelper.atkType==GRAB){
				aBox= new AttkTrack();
				aBox.noHit=false;
				float gCheck = grabTime/fHelper.actionTmr.GetLen();
				float fCheck = fHelper.frame/fHelper.actionTmr.GetLen();
				if(Mathf.Abs(fCheck-gCheck)<Time.deltaTime){
					
					aBox.SetGrabBox(GetPos (), stats.grabRange);

					float[] atAng = new float[4];
					atAng[0]=0;
					atAng[1]=Mathf.PI/2;
					atAng[2]=Mathf.PI;
					atAng[3]=-Mathf.PI/2;
					//for(pInd=0;pInd<numPoly;pInd++)//iterate per poly
					atkLen =  4;
					hitflag=true;
					attackBox.grabRange= stats.grabRange;
					SPoint[] spArr = attackBox.FetchGrabRenderFrame(fHelper.frame, GetPos (), fHelper.IsFacingRight());

					for(int j = 0; j < atkLen; j++)
						if(!CheckAxis(spArr[j], atAng[j], plBox, spArr, hbLen, atkLen) ) //no axis intersection
							hitflag = false;
					
					if(hitflag)//test on the axis of the player hit box to confirm
						for(int j = 0; j < 4; j++)
							if(!CheckAxis(plBox[j], plAng[j], plBox, spArr, hbLen, atkLen)) //no axis intersection
								hitflag = false;
					
					if(hitflag){//hit
						if(plr.grabbedPlr!=null){
							plr.grabbedPlr. state=IDLE;
							plr.grabbedPlr.Idle();
							plr.grabbedPlr.fHelper.Animate ("Idle", true, 0);
							plr.fHelper.Animate ("Idle", true, 0);
							plr.grabbedPlr.stats.flags.mBusy=false;
							plr.grabbedPlr.stats.flags.aBusy=false;
							plr.grabbedPlr=null;
							Idle();
							fHelper.Animate ("Idle", true, 0);


						}
						else if(plr.stats.dodge.IsReady()){
						grabbedPlr=plr;
							plr.Grabbed ();}
					}
				}
				
			}else{
			//attack checks

			for(int t=0;t<il;t++){//iterate through all sub tr

				//if(plr.CharName().CompareTo("PsySword")==0)
				//	int ds=2;//checkpt
				if((fHelper.seqInd!=0)&&((fHelper.seqInd-1)!=t)){
					hitflag=false;//do nothing, for now
				}else {
					aBox= new AttkTrack();
					aBox.noHit=false;
						if(attackBox.trackList[fHelper.atkType].trackType==AttkBox.FLASH){
							if(fHelper.frame>lastFlash){
								
								for(AttkVtx av = attackBox.trackList[fHelper.atkType].aVtx[0][0]; av!=null;av=av.next){
									if((av.frame==lastFlash)&&(av.next!=null)){
										lastFlash=av.next.frame;
										attackBox.flashAtkRdy=true;
                                        attackBox.ResetHits (fHelper.atkType);
                                        while(av.next!=null)
                                            av=av.next;
                                        //av=null;//force exit
                                    }
                                }
							}
						}
					attackBox.FetchFrame(fHelper.atkType, atkTmr.GetLen(), fHelper.frame, t, new SPoint(GetPos().x, GetPos().y), fHelper.IsFacingRight(), aBox);//fetches frame into aBox
					
						//aBox has its polygons separated into different tracks for efficiency
					//make a new SPoint array, spBox, to use checkAxis.
					
					hitflag=true;
					if(aBox.noHit==false){

						//if(plr.CharName().CompareTo("PsySword")==0)
						//	int d2s=2;//checkpt
						spBox=new SPoint[aBox.iLength][];
						for(int i=0;i<aBox.iLength;i++)
							spBox[i]=new SPoint[aBox.jLength[i]];
						aBox.FetchSPArr(spBox);
							if(ClashCheck(plr, spBox, aBox.atkAng)){

								return;
							}
						
						numPoly=aBox.iLength;
							bool hitCheck = false;
						for(pInd=0;pInd<numPoly;pInd++){//iterate per poly
								hitflag=true;
							atkLen =  aBox.GetJlength(pInd);
							for(int j = 0; j < atkLen; j++)
								if(!CheckAxis(aBox.aVtx[pInd][j].pos, aBox.atkAng[pInd][j], plBox, spBox[pInd], hbLen, aBox.GetJlength(pInd)) ) //no axis intersection
									hitflag = false;
							
							if(hitflag)//test on the axis of the player hit box to confirm
								for(int j = 0; j < 4; j++)
									if(!CheckAxis(plBox[j], plAng[j], plBox, spBox[pInd], hbLen, aBox.GetJlength(pInd))) //no axis intersection
										hitflag = false;
							if(hitflag)
									hitCheck=true;
							
						}
							hitflag=hitCheck;
						//log.str(std::string());
						/*	for(int i=0;i<aBox.iLength;i++){//tidy up
							if(!aBox.noHit)
								delete [] spBox[i];
						}
						if(aBox.iLength==0){
							int ee = 2;
							//log.str(std::string());
						}
						else if(!aBox.noHit){//make sure it was hit
							if(aBox.iLength>1)
								delete [] spBox;
							else
								delete spBox;
							spBox=NULL;
						}*/
					}else
						hitflag=false;//abandon
					if(hitflag){
						AttkData retOb  = new AttkData(aBox.GetJlength(t));//initilize for holding vals
						Attack(plr, t, aBox, retOb);
						if(!retOb.noHit){
								if(debugModeOn){
								if(multiHitFlag)
									hitText = CharName() + " multi-hit\r\n" + plr.CharName() + "!";
								else
									hitText =  CharName() + " hit \r\n" + plr.CharName() + "!";
								ConsoleLog(hitText);
								}
							plr.GetHit(retOb);
							if((cons!=null)&&(!multiHitFlag)&&(retOb!=null))
								cons.PostHit(retOb.GetSPArr(), retOb.curInd, retOb.aBds[0], retOb.attLen, retOb.pBds, retOb.GetConOrigin(), retOb.GetConInter());
							multiHitFlag=true;
							t=il;//force exit; one track hit
						}	
					}
					
					}
					
				}
			}
		}
		//seems the ideal place to determine pushback
		//if the difference in z is less than their average width
		if((Mathf.Abs(plr.GetPos().x-GetPos().x) < (plr.stats.size.x+stats.size.x)/2  )
		   &&(Mathf.Abs(plr.GetPos().y-GetPos().y) < (plr.stats.size.y+stats.size.y)/2) 
		   &&(!fHelper.airborne)&&(!plr.fHelper.airborne))
		if(plr.GetPos().x-GetPos().x>0){
			stats.motion.move.x-=0.1f;
			plr.stats.motion.move.x+=0.1f;}
		else{
			stats.motion.move.x+=0.1f;
			plr.stats.motion.move.x-=0.1f;}
		
		
		//prob at a second case here for seren sword
	//	if(plr.CharName().CompareTo("Serenity")==0){
	//		AttackDetect(plr.GetFamiliar());
	//	}
		if (fHelper.atkType == DATK) {
			if ((rightLip != 0) && (stats.motion.pos.x < rightLip)) {
				stats.motion.vel.x = 0;
				stats.walk.gndSpeed = 0;

			} else if ((leftLip != 0) && (stats.motion.pos.x > leftLip)) {
				stats.motion.vel.x = 0;
				stats.walk.gndSpeed = 0;
			}
		}
	}
	protected virtual void Attack(Fighter plr,int tr, AttkTrack aBox, AttkData data){
		//pHit is a bool array of flags that tell whether a sub-poly held collision
		//aBox is a modified track taken from current frame
		//plr is the player hit
		//check pnum else we cut Serenuity's sword
		if((plr.stats.id.num<100)&&(attackBox.trackList[fHelper.atkType].hit[tr][plr.stats.id.num])){//if this attack was recorded already
			data.noHit=true; //flag so this does not trigger impact
			return;//don't hit again
		}
		attackBox.trackList[fHelper.atkType].hit[tr][plr.stats.id.num]=true;

						
		bool isInBox=false;
		bool vNextInBox=false;
		AttkVtx iVt;	
		bool[] cornerFlag = new bool[4];
		SPoint[] hb = plr.GetHitBox();// double[4]
		for(int i=0;i<4;i++){
			cornerFlag[i]=true;
			hb[i].x = hb[i].x-GetPos().x;//translate to origin
			hb[i].y = hb[i].y-GetPos().y;
		}
		bool emptyFlag = true;
		bool fullFlag =true;//assume true, disprove by counterexample
		SPoint vPos = new SPoint ();
		SPoint opp = new SPoint();
		int vNext,vLast,jl, il;
		float nX, nY, lY=0, lX=0, vX, vY, ppX, ppY, ph2, pw2, diDist,vDir, vDmg, vMag, vWgt = 0;
		//add poitional value (for wgt=0) into data
		opp.x=plr.GetPos().x-GetPos().x;//used to store the centerpoint
		if(fHelper.IsFacingRight())      //for input into data
			opp.x-=(aBox.centre.pos.x);
		else
			opp.x+=(aBox.centre.pos.x);
		opp.y=plr.GetPos().y-GetPos().y+aBox.centre.pos.y;	
		data.SetAVals(opp);
		il = aBox.iLength;
		//set data.abds as an sPoint holding the vertices for aBox.vtx
		//this is done purely for debug rendering
		//which will later handle 'data'
		data.aBds = new SPoint[il][];
		for(int j=0;j<il;j++){
			data.aBds[j] = new SPoint[aBox.GetJlength(j)];
			for(int i=0;i<aBox.GetJlength(j);i++)
				data.aBds[j][i] = new SPoint(aBox.aVtx[j][i].pos.x-GetPos().x, aBox.aVtx[j][i].pos.y-GetPos().y);
		}
		
		
		emptyFlag=true;
		fullFlag=true;
		ph2 = plr.stats.size.y/2;
		pw2 = plr.stats.size.x/2;
		ppX = plr.GetPos().x;
		ppY = plr.GetPos().y+ph2;
		for(int i=0;i<4;i++)//assume true then prove false
			cornerFlag[i]=true;
		for(int p=0; p<aBox.iLength;p++){
			jl = aBox.GetJlength(p);
			for(int v = 0; v < jl;v++){//loop through the second index
				vPos = aBox.aVtx[p][v].pos;//use this vertex to check whether it falls
				if((Mathf.Abs(vPos.x-ppX)<pw2)&&(Mathf.Abs(vPos.y-ppY)<ph2))// within hitbox
					emptyFlag = false;//can't be empty
				else
					fullFlag = false;//can't be full
			}
			if(fullFlag){
				for(int v = 0; v < jl;v++){
					int before = v-1;
					if(before<0)
						before=jl-1;
					diDist = Mathf.Sqrt(Mathf.Pow(aBox.aVtx[p][before].pos.x-aBox.aVtx[p][v].pos.x, 2) + Mathf.Pow(aBox.aVtx[p][before].pos.y-aBox.aVtx[p][v].pos.y, 2));
					vMag = aBox.aVtx[p][v].mag;
					vDir = aBox.aVtx[p][v].dir;
					vDmg = aBox.aVtx[p][v].dmg;
					vWgt = aBox.aVtx[p][v].wgt;
					data.addVal(aBox.aVtx[p][v].pos, vMag, vDir, diDist, vDmg, vWgt);
					for(int i=0;i<4;i++)
						cornerFlag[i]=false;//none possible to be included
				}
			}
			else if(emptyFlag){
				for(int j = 0;j<jl;j++){
					diDist=1;
					vMag = aBox.aVtx[p][j].mag;
					vDir = aBox.aVtx[p][j].dir;
					vDmg = aBox.aVtx[p][j].dmg;
					vWgt = aBox.aVtx[p][j].wgt;
					vPos = aBox.aVtx[p][j].pos;
					vPos.x-=GetPos().x;
					vPos.y-=GetPos().y;
					data.addVal(vPos, vMag, vDir, diDist, vDmg, vWgt);
				}
			}
			else 
			for(int v = 0; v < jl;v++){
				vNext = (v+1)%jl;//circular array
				vPos = aBox.aVtx[p][vNext].pos;
				vNextInBox = ((Mathf.Abs(vPos.x-ppX)<pw2)&&(Mathf.Abs(vPos.y-ppY)<ph2));
				vLast=v-1;
				if(vLast<0)
					vLast=jl-1;
				vMag = aBox.aVtx[p][v].mag;
				vDir = aBox.aVtx[p][v].dir;
				vDmg = aBox.aVtx[p][v].dmg;
				vWgt = aBox.aVtx[p][v].wgt;
				vPos = aBox.aVtx[p][v].pos;
				vX = aBox.aVtx[p][v].pos.x-GetPos().x;//all values translated to origin
				vY = aBox.aVtx[p][v].pos.y-GetPos().y;
				if(v==0){
					lX=vX;//mustdefine these ahead of time
					lY=vY;//so the first dir is properly averaged
				}     //in case the loop below is not entered
				if((Mathf.Abs(vPos.x-ppX)<pw2)&&(Mathf.Abs(vPos.y-ppY)<ph2)){//within the victim's hitbox
					diDist = Mathf.Sqrt( (lX-vX)*(lX-vX) + (lY-vY)*(lY-vY));
					data.addVal(new SPoint(vX, vY), vMag, vDir, diDist, vDmg, vWgt);
					lX=vX;
					lY=vY;
					if(!vNextInBox){
						iVt = InterpVert(aBox.aVtx[p][v], aBox.aVtx[p][vNext], plr);
						iVt.pos.x-=GetPos().x;//all values translated to origin
						iVt.pos.y-=GetPos().y;
						diDist = Mathf.Sqrt(Mathf.Pow(iVt.pos.x-vX, 2)+Mathf.Pow(iVt.pos.y-vY, 2));
						data.addVal(iVt.pos, iVt.mag, iVt.dir, diDist, iVt.dmg, iVt.wgt);
						lX=iVt.pos.x;
						lY=iVt.pos.y;
					}
				}
				else {
					if(vNextInBox){
						iVt = InterpVert(aBox.aVtx[p][vNext], aBox.aVtx[p][v], plr);
						iVt.pos.x-=GetPos().x;//all values translated to origin
						iVt.pos.y-=GetPos().y;
						diDist = Mathf.Sqrt(Mathf.Pow(iVt.pos.x-lX, 2)+Mathf.Pow(iVt.pos.y-lY, 2));
						data.addVal(iVt.pos, iVt.mag, iVt.dir, diDist, iVt.dmg, iVt.wgt);
						lX=iVt.pos.x;
						lY=iVt.pos.y;
					}
				}
				for(int i = 0; i<4;i++){//check player corner axis
					vX = aBox.aVtx[p][v].pos.x-GetPos().x-hb[i].x;//all values translated to origin
					vY = aBox.aVtx[p][v].pos.y-GetPos().y-hb[i].y;
					nX = aBox.aVtx[p][vNext].pos.x-GetPos().x-hb[i].x;
					nY = aBox.aVtx[p][vNext].pos.y-GetPos().y-hb[i].y;
					if((fHelper.IsFacingRight())&&(nX*vY-vX*nY<0))//second part is a shortcut formula
						cornerFlag[i]=false;
					else if((!fHelper.IsFacingRight())&&(nX*vY-vX*nY>0))
						cornerFlag[i]=false;
				}
			}
			//last step; add corners that retained their flag
			if(emptyFlag){//for now ; TODO: add case later
				for(int i=0;i<4;i++)
				if(cornerFlag[i]){
					iVt = InterpCorner(aBox.aVtx[p], aBox.GetJlength(p), plr.GetPos());
					iVt.pos.x-=GetPos().x;//all values translated to origin
					iVt.pos.y-=GetPos().y;
					diDist = Mathf.Sqrt(Mathf.Pow(iVt.pos.x-lX, 2)+Mathf.Pow(iVt.pos.y-lY, 2));
					data.addVal(iVt.pos, iVt.mag, iVt.dir, diDist, iVt.dmg,iVt.wgt);
					lX=iVt.pos.x;
					lY=iVt.pos.y;
				}
			}
		}

		data.rollIntoAvg();
		if (attackBox.trackList [fHelper.atkType].isVacuum) {
			data.isVacuum=true;
		}

		data.SetConOrigin(aBox.centre.pos);
		data.PlayerHitBoxInput(hb);
	}
	
	protected AttkVtx InterpCorner(AttkVtx[] arrV, int arrLen, SPoint oPos){
		AttkVtx rtnVtx = new AttkVtx();
		float dist = 0;
		float distSum=0;
		SPoint tPos = new SPoint();
		for(int i = 0; i< arrLen;i++){
			tPos.x = arrV[i].pos.x-oPos.x;//tranlated for ease of calculations
			tPos.y = arrV[i].pos.y-oPos.y;
			dist=Mathf.Sqrt(tPos.SqDistFromOrigin());
			distSum+=dist;
		}//now average out verts
		for(int i = 0; i< arrLen;i++){
			tPos.x = arrV[i].pos.x-oPos.x;//tranlated for ease of calculations
			tPos.y = arrV[i].pos.y-oPos.y;
			dist=Mathf.Sqrt(tPos.SqDistFromOrigin());
			rtnVtx.mag+=(dist/distSum)*arrV[i].mag;
			rtnVtx.dir+=(dist/distSum)*arrV[i].dir;
			rtnVtx.dmg+=(dist/distSum)*arrV[i].dmg;
			rtnVtx.wgt+=(dist/distSum)*arrV[i].wgt;
		}
		return rtnVtx;
	}
	
	protected AttkVtx InterpVert(AttkVtx vI, AttkVtx vO, Fighter plr){
		//returns an interpolated vertec between vI (vertex inside player hitbox),
		//vO(vertex outside player hitbox), where plr is the player
		AttkVtx retVtx = new AttkVtx();
		float itpWgt = 1.1f;
		if(vO.pos.x > plr.Right()){
			retVtx.pos.x=plr.Right();
			itpWgt = (plr.Right() - vI.pos.x)/(vO.pos.x-vI.pos.x);
			retVtx.pos.y = itpWgt*vI.pos.y + (1-itpWgt)*vO.pos.y;//solve rest
		}else if(vO.pos.x < plr.Left()){
			retVtx.pos.x=plr.Left();
			itpWgt = (plr.Left() - vI.pos.x)/(vO.pos.x-vI.pos.x);
			retVtx.pos.y = itpWgt*vI.pos.y + (1-itpWgt)*vO.pos.y;//solve rest		
		}
		if(itpWgt>1){
			if(vO.pos.y > plr.Top()){
				retVtx.pos.y=plr.Top();
				itpWgt = (plr.Top() - vI.pos.y)/(vO.pos.y-vI.pos.y);
				retVtx.pos.x = itpWgt*vI.pos.x + (1-itpWgt)*vO.pos.x;//solve rest		
			}
			else if(vO.pos.y > plr.Bottom()){
				retVtx.pos.y=plr.Bottom();
				itpWgt = (plr.Bottom() - vI.pos.y)/(vO.pos.y-vI.pos.y);
				retVtx.pos.x = itpWgt*vI.pos.x + (1-itpWgt)*vO.pos.x;//solve rest		
			}
		}
		retVtx.mag = itpWgt*vI.mag + (1-itpWgt)*vO.mag;
		retVtx.dir = itpWgt*vI.dir + (1-itpWgt)*vO.dir;
		retVtx.dmg = itpWgt*vI.dmg + (1-itpWgt)*vO.dmg;
		retVtx.wgt = itpWgt*vI.wgt + (1-itpWgt)*vO.wgt;
		return retVtx;
	}
	public Fighter GetFamiliar(){
		return null;
	}

	public 	void Aerial(){
		if(fHelper.airborne){
			int atk = 0;
			string name = "";
			if(gCont.Held(UP))
				StartAttack(UAIR, "UAIR");
			else if(gCont.Held(DOWN))
				StartAttack(DAIR, "DAIR");
			else if((fHelper.IsFacingRight()&&gCont.Held(RIGHT))||(!fHelper.IsFacingRight()&&gCont.Held(LEFT)))
				StartAttack(FAIR, "FAIR");
			else if((fHelper.IsFacingRight()&&gCont.Held(LEFT))||(!fHelper.IsFacingRight()&&gCont.Held(RIGHT)))
				StartAttack(BAIR, "BAIR");
			else
				StartAttack(NAIR, "NAIR");
		}
	}
	
	protected bool is_A(int but){
		if((but==NA)||(but==SA)||(but==UA)||(but==DA))
			return true;
		else
			return false;
	}
	
	protected bool is_B(int but){
		if((but==NB)||(but==SB)||(but==UB)||(but==DB))
			return true;
		else
			return false;
	}
	
	protected bool is_C(int but){
		if((but==NC)||(but==SC)||(but==UC)||(but==DC))
			return true;
		else
			return false;
	}
	public bool PerAttk(){
		if (InState ( DASH))
						StartAttack (DATK, "DATK");
				else if (fHelper.airborne)
						Aerial ();
				else
						return false;
		return true;
    }
	public void  A(){
		if (PerAttk ())
		    return;
		else if ( state < 3) {//able to attack while idle,running or crouching
				if (gCont.lStick.y > VERT_THRESH)
						StartAttack (UA, "UA");
				else if (gCont.Held (DOWN))
						StartAttack (DA, "DA");
				else if ((gCont.Held (LEFT)) || (gCont.Held (RIGHT)))
						StartAttack (SA, "SA");
				else
						StartAttack (NA, "NA");
		} else if ( state ==  ATTK) {
			if(gCont.lStick.y>VERT_THRESH)
				SequenceCheck(UA, "UA");
			else if(gCont.Held(DOWN))
				SequenceCheck(DA, "DA");
			else if((gCont.Held(LEFT))||(gCont.Held(RIGHT)))
				SequenceCheck(SA, "SA");
			else
				SequenceCheck(NA, "NA");
		}
			
	}
	
	public void B(){
		if (PerAttk ())
			return;
        else if( state < 3){
			if(gCont.lStick.y>VERT_THRESH)
				StartAttack(UB, "UB");
			else if(gCont.Held(DOWN))
				StartAttack(DB, "DB");
			else if((gCont.Held(LEFT))||(gCont.Held(RIGHT)))
				StartAttack(SB, "SB");
			else
				StartAttack(NB, "NB");
		}else if ( state ==  ATTK) {
			if(gCont.lStick.y>VERT_THRESH)
				SequenceCheck(UB, "UB");
			else if(gCont.Held(DOWN))
				SequenceCheck(DB, "DB");
			else if((gCont.Held(LEFT))||(gCont.Held(RIGHT)))
				SequenceCheck(SB, "SB");
			else
				SequenceCheck(NB, "NB");
		}	
	}
	
	public void C(){
		if (PerAttk ())
			return;
        else if( state < 3){
			if(gCont.lStick.y>VERT_THRESH)
				StartAttack(UC, "UC");
			else if(gCont.Held(DOWN))
				StartAttack(DC, "DC");
			else if((gCont.Held(LEFT))||(gCont.Held (RIGHT)))
				StartAttack(SC, "SC");
			else
				StartAttack(NC, "NC");
		}else if ( state ==  ATTK) {
			if(gCont.lStick.y>VERT_THRESH)
				SequenceCheck(UC, "UC");
			else if(gCont.Held(DOWN))
				SequenceCheck(DC, "DC");
			else if((gCont.Held(LEFT))||(gCont.Held(RIGHT)))
				SequenceCheck(SC, "SC");
			else
				SequenceCheck(NC, "NC");
		}	
	}
	public bool GrabCheck(){
		if(stats.flags.aBusy)
			return false;
		if (fHelper.actionTmr.IsReady ()) {
			stats.jump.tmr.ResetTimer();
			 state =  ATTK;
			fHelper.animLoopStart=0;
			float actTime = fHelper.Animate("Grab", false, 0);
			fHelper.frame=0;
			fHelper.actionTmr.SetTimer(actTime);
			fHelper.atkType=Fighter.GRAB;
        }else
            return false;
        return true;
    }
	public void Grab(){
		if (!fHelper.airborne) {
			if (GrabCheck ()) {//if attack possible
				atkTmr.SetTimer (fHelper.aniTmr.GetLen ());	
				grbTmr.SetTimer (grabTime);
			}		

		}
	}
	public void SetPlayer(){
		fHelper.anim = GetComponent<Animator>();
		fHelper.animation = GetComponent<Animation>();
		fHelper.TR = transform;
		stats.motion.pos.x = fHelper.TR.position.x;
		stats.motion.pos.y = fHelper.TR.position.y;
	}
	public bool AttackCheck(int aI, string aS, int sI){
		bool retFlag= AttackCheck(aI, aS);
		fHelper.seqInd = sI;
		return retFlag;
	}
	public bool AttackCheck(int aI, string aS){
		if (!stats.tumble.tmr.IsReady ())
			return false;
		fHelper.seqInd = 0;
		if ((stats.flags.aBusy) && (aI != Fighter.LATK))
			return false;
		else if (aI == Fighter.LATK) {
			fHelper.actionTmr.ResetTimer ();
			aS="LedgeAttack";
		}
		if(fHelper.actionTmr.IsReady()){
			stats.jump.tmr.ResetTimer();
			 state =  ATTK;
			fHelper.animLoopStart=0;
			float actTime = fHelper.Animate(aS, false, 0);
			fHelper.frame=0;
			fHelper.actionTmr.SetTimer(actTime);
			//animName = aS;
			fHelper.atkType=aI;
		}else
			return false;
		return true;
	}
	public void SequenceCheck(int aI, string aS){
		if ((attackBox.trackList [aI].trackType == 3) && (aI < NAIR)) {
			if (InAttack (aI)) {
				fHelper.seqInd++;
				if(fHelper.seqInd>=attackBox.trackList[aI].iLength)
					fHelper.seqInd=0;
				fHelper.actionTmr.ResetTimer ();
				if (AttackCheck(aI, aS, fHelper.seqInd)) {//if attack possible
					atkTmr.SetTimer (fHelper.aniTmr.GetLen ());
					attackBox.ResetHits (aI);


					if (moveCont [aI] != null)
						moveCont [aI].StartAttack ();
				}	
			}	
		} 
	}
	public virtual void StartAttack(int aI, string aS){


			if (AttackCheck (aI, aS)) {//if attack possible
				atkTmr.SetTimer (fHelper.aniTmr.GetLen ());
				stats.jump.tmr.ResetTimer();
				attackBox.ResetHits (aI);
				if (moveCont [aI] != null)
					moveCont [aI].StartAttack ();
			if(attackBox.trackList[aI].trackType==AttkBox.FLASH){
				attackBox.flashAtkRdy=true;
				lastFlash=attackBox.trackList[aI].aVtx[0][0].frame;
				}
			else if(attackBox.trackList[aI].trackType==AttkBox.SEQUENCE){
				if(debugModeOn){
					if(attkHitbox!=null){
						Destroy (attkHitbox);
						debugFadeTmr.ResetTimer();
					}

				}
			}
			else
				lastFlash=1;
			}

	}
	public virtual void PostUpdate(){
		stats.motion.accl = new SPoint();
		SPoint t = new SPoint();
		t.x = stats.motion.pos.x - stats.motion.lastPos.x;
		t.y = stats.motion.pos.y - stats.motion.lastPos.y;
		FighterTranslate (t);
		if(InState( FALL))
			Fall();
		if(fHelper.airborne)
			stats.walk.gndSpeed=0;//reset for safety
	}


	// Update is called once per frame
	protected virtual void GrabUpdate(float timeLapsed){
		if(grbTmr.RunTimer (timeLapsed))  {
			if(grabbedPlr!=null) {
				fHelper.actionTmr.SetTimer (fHelper.Animate ("Throw", false, 0.0f));
				thwTmr.SetTimer (throwTime);
			}
		}
		if (thwTmr.RunTimer (timeLapsed)) {
			ThrowMotion (0, stats.thr.dmg, stats.thr.dir, stats.thr.mag);
			grabbedPlr=null;
			
			fHelper.atkType=0;
		} if((grabbedPlr!=null)){

			
			grabbedPlr.SetPos (new SPoint(thwBone.position.x, thwBone.position.y));
			grabbedPlr.transform.eulerAngles = thwBone.eulerAngles;
			grabbedPlr.transform.Rotate(0, 0, 90.0f);

		}
		
	}
	protected virtual void FrameUpdate() {
		if(debugModeOn)
			FindConsole ();//always check in case console 
		float timeLapsed = Time.deltaTime;

		//double timeFr=timeLapsed*30;//simplified define for current development
		//ignore for now cause this is kind of a hack...
		if( StateChange())
			if((fHelper.lastState== EDGEATTK)||(fHelper.lastState== EDGEROLL)||(fHelper.lastState== DODGE))//kills invulnerabilty after a dodge
				stats.flags.invuln=false;//just in case it was not reset
		if((fHelper.lastState!= state))//end of edgegrab
			if((fHelper.lastState== EDGEROLL))
				stats.flags.mBusy=false;
		if ( state ==  EDGEROLL) {
			
			if (fHelper.IsFacingRight ())
				stats.motion.vel.x = stats.edgegrab.rollSpd;
			else
				stats.motion.vel.x = -stats.edgegrab.rollSpd;
		}
		if (state == ROLLING)
			CheckForLip ();
		fHelper.lastState= state;
		bool actionFlag=false;
		if(fHelper.actionTmr.RunTimer(timeLapsed)){
			actionFlag=true;
			stats.flags.aBusy=false;
			stats.flags.mBusy=false;
			stats.flags.invuln=false;
			fHelper.UnTranslate();
			if( state== ATTK){		
				if((fHelper.atkType==2)||(fHelper.atkType==6)||(fHelper.atkType==10)){//down attacks
					fHelper.Animate("Crouch", true, 0.15f);
					fHelper.animLoopStart=0.15f;
					 state= CROUCH;}
				else if(fHelper.atkType!=Fighter.GRAB){
					 state=IDLE;
					//Idle();
				}
			}
		}
		if (fHelper.actionTmr.IsReady ()) {
						if (InState (EDGECLIMB)) {

								if (fHelper.airborne)
										Fall ();
								
						}
						//else
						//Idle();
						fHelper.actionTmr.ResetTimer ();
		} else if (InState (EDGECLIMB)){
			float tDist = fHelper.IntFacingRight()*(timeLapsed/fHelper.actionTmr.GetLen())*stats.size.x*2;
			if(fHelper.frame>0.5f)
			   tDist=0;
			stats.motion.pos.x +=tDist;
				fHelper.UnTranslate ();
				FighterTranslate (new SPoint (tDist, 0));
			 
		
		}

		if ( state ==  GUARD) {
			float gDir = stats.guard.dir;
			bool face = true;
			if(Mathf.Abs (gDir)>Mathf.PI/2){
				if(gDir>0)
					gDir = -(gDir-Mathf.PI);
				else
					gDir = -(gDir+Mathf.PI);
				face=false;
			}
			gDir = gDir+Mathf.PI/2;
			gDir = gDir/Mathf.PI;
			fHelper.SetWeightedAnim (gDir);
			if(fHelper.IsFacingRight()!=face){
				FaceRightIm(face);

			}
			
		}
		else{
			fHelper.UpdateAnim(timeLapsed);
			if(stats.guard.arc>stats.guard.max)
				stats.guard.arc=stats.guard.max;
			else if(stats.guard.arc<stats.guard.max)
				stats.guard.arc+=timeLapsed*GUARDREC;
		}
		if(!fHelper.aniTmr.IsReady()){
			fHelper.aniTmr.RunTimer(timeLapsed);
			fHelper.frame = 1.0f-fHelper.aniTmr.tmr/fHelper.aniTmr.GetLen();
			if(fHelper.aniTmr.IsReady()){
				if(fHelper.nextAnim.CompareTo("")!=0){
					fHelper.Animate(fHelper.nextAnim, fHelper.NextLoop(), 0);
					fHelper.nextAnim="";
				}
				/*else if(animState.getLoop()){
					aniTmr.SetTimer(animState->getLength()-animLoopStart);
					animState->setTimePosition(animLoopStart);//reset time position to loop}
				}*/
			}	
		}else
			fHelper.frame=0;


		gCont.Process(timeLapsed);
		ProcessInput ();
		GrabUpdate (timeLapsed);
		stats.motion.lastPos.x = GetPos().x;
		stats.motion.lastPos.y = GetPos().y;
		stats.motion.pos.x+=stats.motion.move.x;
		stats.motion.pos.y+=stats.motion.move.y;
		stats.motion.move = new SPoint(0,0);

		//guard updates
		if((Mathf.Abs(gCont.lStick.x)+Mathf.Abs(gCont.lStick.y)<0.2)&&(!fHelper.IsFacingRight()))
			stats.guard.dir = -Mathf.PI;	
		else
			stats.guard.dir=gCont.lStick.GetDir();
		
		for(int i=0;i<PROJ_LMT;i++)
			projectile[i].FrameUpdate(timeLapsed);
		 
		//edgegrabbing
		if((stats.tumble.tmr.IsReady())&&(!InState( ATTK))&&(GetVel().y < 0.8)&&(fHelper.airborne)&&(stats.edgegrab.delayTmr.IsReady()))
			fHelper.grabBox.isActive = true;
		else
			fHelper.grabBox.isActive = false;

		//move scripts are run here
		RunMoveIns (timeLapsed);
		//walking
		Walk(timeLapsed);
		//timers
		if (state == EDGEGRAB) {
			stats.motion.vel.x=fHelper.IntFacingRight();		
		}
		RunTimers(timeLapsed);
		//idlechecking
		if((stats.jump.tmr.IsReady())&&(atkTmr.IsReady())&&(!InState( GRABBED))&&(!InState( DASH))&&(!InState( GUARD)))
			Idle();
	
						
		//falling
		if((fHelper.airborne)&&( state!= GRABBED)){		
			stats.motion.accl.y -= stats.grav;	
			if(gCont.lStick.y<0){
				if(stats.motion.vel.y<0)
				stats.motion.accl.y += (gCont.lStick.y*0.1f)*stats.grav;	 //will range between 0 and 20%
				else
					stats.motion.accl.y += (gCont.lStick.y-0.3f)*stats.grav;	 //will range between 0 and 100%
			}
		}
		if (!stunTimer.IsReady ()) {
			stats.motion.vel.x = 0;
			stats.motion.vel.y = 0;
			}
		if(dmgFlag){
			dmgFlag=false;//reset
			dmgDebugFlag=true;//to render debug info
		}
		//motion update
		stats.motion.vel.x += stats.motion.accl.x;
		stats.motion.vel.y += stats.motion.accl.y;
		//edgeroll
       if(InState( EDGEROLL)){
			if (stats.edgegrab.rollMax > 0) {
				stats.edgegrab.rollMax -=Mathf.Abs(GetVel().x);
				if(stats.edgegrab.rollMax<0)
				if(GetVel().x > 0){
					stats.motion.vel.x -=stats.edgegrab.rollMax;
					//stats.motion.pos.x-=stats.edgegrab.rollMax;
				}else if(GetVel().x<0){
					stats.motion.vel.x+=stats.edgegrab.rollMax;
					//stats.motion.pos.x+=stats.edgegrab.rollMax;
				}
			}else if(stats.edgegrab.rollMax < 0){
				stats.motion.vel.x=0;
				//stats.motion.pos.x=stats.motion.lastPos.x;
                
            }
            
        }
		if (InState ( GRABBED)) {
			stats.motion.vel.y=0;


		}
		if (InState (ROLLING))
			CheckForLip ();
		stats.motion.pos.x += stats.motion.vel.x;
		stats.motion.pos.y += stats.motion.vel.y;

		UpdateGrabBox();
		//frame check
		if(atkTmr.GetLen() != 0)
			fHelper.frame = (1 - atkTmr.tmr/atkTmr.GetLen());

		if (grabbedPlr != null) {
		 
			grabbedPlr.grabbedPlr=null;
			grabbedPlr.fHelper.actionTmr.ResetTimer ();
			grabbedPlr.atkTmr.ResetTimer ();
			 
        }
        //attack detection
		for (int i=0; i<fighterList.Length; i++)
				AttackDetect (fighterList[i]);
		if (debugModeOn) {
			RenderHitbox ();

		}
		if(stats.flags.landed){
			
			PlayEffect(EF_DCLOUD);

			stats.flags.landed=false;
		}
	}
	public void PivotEnd(){
		//call this to cancel the animation or clear pivot vars
		if(fHelper.IsFacingRight())
			fHelper.Orient(0);
		else
			fHelper.Orient(180);
		stats.pivot.slowPivot=false;
		stats.flags.mBusy=false;
		stats.flags.aBusy=false;
		//Walk(0, gCont.lStick.x);
	}
	public void FighterPivot(){
		float pTimer = stats.pivot.pTmr.tmr;
		float pTime = stats.pivot.fastTime;
		if((!stats.flags.mBusy)&&(stats.jump.tmr.IsReady())){
			if(stats.flags.running){
				stats.pivot.slowPivot=true;
				if( state== DASH)
					state= WALK;
				fHelper.Animate("Walk", true, 0);
			}else if(state==DASH)
				fHelper.Animate("Run", true, 0);

			if(stats.pivot.pTmr.IsReady()){
				if(!fHelper.IsFacingRight())
					stats.pivot.pTmr.SetTimer(-pTime);
			else
				stats.pivot.pTmr.SetTimer(pTime);
			}
			else{ //sprite is already pivoting
				if(!fHelper.IsFacingRight())
					stats.pivot.pTmr.SetTimer( pTimer-pTime);
			else
				stats.pivot.pTmr.SetTimer(pTime + pTimer); 
			}
			stats.flags.aBusy=true;
			
			if(fHelper.IsFacingRight())
				fHelper.FaceRight(false);
			else
				fHelper.FaceRight(true);

			fHelper.animLoopStart=0;
		}
	}
	void CheckForLip(){
		if ((rightLip != 0) && (stats.motion.pos.x <= rightLip)) {
			stats.motion.vel.x=0;
			stats.walk.gndSpeed=0;
			stats.motion.pos.x=rightLip;
			
		}
		else if((leftLip!=0)&&(stats.motion.pos.x>=leftLip)){
			stats.motion.vel.x=0;
			stats.walk.gndSpeed=0;
			stats.motion.pos.x=leftLip;
        }
        
    }
    public void RunMoveIns(float timeLapsed){
	 
		bool test1 = (fHelper.atkType != Fighter.GRAB);
		bool test2 = InState( ATTK);
		if((fHelper.atkType!=Fighter.GRAB)&&(InState( ATTK))) {

			if(moveCont[fHelper.atkType]!=null){
				//for(int i=0;i<moveCont[fHelper.atkType].
			moveCont[fHelper.atkType].RunTimers(timeLapsed, fHelper, stats);
				if ((rightLip != 0) && (stats.motion.pos.x < rightLip)) {
					stats.motion.vel.x=0;
					stats.walk.gndSpeed=0;

                }
				else if((leftLip!=0)&&(stats.motion.pos.x>leftLip)){
					stats.motion.vel.x=0;
					stats.walk.gndSpeed=0;
                }
            }
        }
    }
    public bool ProcessInput( ){
		//process keyboard/controller input here
		//once this function has been run, input will have been aquired
		//and the player's fighter will move according to input
		
		//gCont.lStick -1000 to 1000: - is up,+ down
		//gCont.rStick -1000 to 1000: - is left, + is right
		//gCont.a F3150: 'X' <
		//gCont.b F3150: 'Y' ^
		//gCont.c F3150: 'B' >
		//gCont.c F3150: 'D' v
	
		
		if(gCont.lStick.y<-VERT_THRESH)
			Crouch();
		else if(InState( CROUCH)){//crouch released
			 state=IDLE;
			Idle();
		}
		int ifr = fHelper.IntFacingRight ();
		//simple transformations
		
		if(!fHelper.airborne&&gCont.StickDblTapped()&&(gCont.lStick.x==ifr)){
			if((atkTmr.IsReady())&&(!stats.flags.mBusy)&&(!stats.flags.aBusy)&&(stats.tumble.tmr.IsReady())){
				Dash();
				stats.walk.gndSpeed=gCont.lStick.x;//mult by stuck
				//because this is the quickest way to change the sign for not facing right 
			}
		}
		
		if((gCont.TappedThisFrame(GameController.UP))||(gCont.TappedThisFrame(GameController.D))){//UP
			if(!EdgeClimbCheck(stats.motion.pos, gCont.TappedThisFrame(GameController.D))){ //check for edgeclimb
				if(JumpCheck(stats.jump.tmr.GetLen())){//will jump, if true
					if((!fHelper.airborne)&&(stats.jump.tmr.IsReady())){//delay on non airborne jumps
						stats.jump.tmr.SetTimer();
						stats.edgegrab.edgeTmr.SetTimer();
					}else{//airjump w/o delay
						if( state== EDGEGRAB){
						     fHelper.Animate ("LedgeJump", false, 0);
							stats.edgegrab.edgeTmr.ResetTimer();
							stats.motion.vel.y = stats.jump.vel;
						}else{
							fHelper.Animate ("Jump", false, 0);
							stats.motion.vel.y = stats.jump.airJumpVel;
                        }
						stats.motion.vel.x = gCont.lStick.x*stats.walk.maxSpeed/2;	
					}
				}else if((!stats.tumble.tmr.IsReady())&&(!fHelper.airborne)&&(stats.walk.gndSpeed<LOWTHRESH)){//OTG
					
					GetUp();
				}
			}else{
				stats.edgegrab.flag = false; //edgeclimbed

			}
		}
		int toward=0;
		if(fHelper.IsFacingRight())
			toward=RIGHT;
		else
			toward=LEFT;
		if (gCont.TappedThisFrame (toward)) {//TOWARD
						if (EdgeRollCheck (stats.motion.pos)) { //check for edgeclimb
								stats.flags.invuln = true;
								
						}
				}
		else if(gCont.TappedThisFrame(GameController.A) ||gCont.TappedThisFrame(GameController.B)||gCont.TappedThisFrame(GameController.C)){
			if (EdgeAttack (stats.motion.pos)) { //check for edgeclimb
				StartAttack(LATK, "LedgeAttack");

				stats.flags.invuln = true;

			}
		}
		if(!gCont.Held (GameController.R_TRIGGER)){
			if(gCont.TappedThisFrame(GameController.A))
				if(gCont.Held(GameController.R_BUTTON))
					SpecA();
			else
				A();
			if(gCont.TappedThisFrame(GameController.B))
				if(gCont.Held(GameController.R_BUTTON))
					SpecB();
			else
				B();
			if(gCont.TappedThisFrame(GameController.C))
				if(gCont.Held(GameController.R_BUTTON))
					SpecC();
			else
				C();
		}
		if((Input.GetKey ("s")&&(plNum==1))|| (gCont.Held (GameController.R_TRIGGER))) {
						Guard ();
			if((gCont.TappedThisFrame(GameController.A))||(gCont.TappedThisFrame(GameController.B))||(gCont.TappedThisFrame(GameController.C))){
				Grab();
			    }
			else if(gCont.StickDblTapped()){
				if(gCont.lStick.x > LOWTHRESH)
					FaceRightIm(true);
				else if(gCont.lStick.x<-LOWTHRESH)
						FaceRightIm(false);
				
				Roll();
			}
		}
		else if(InState( GUARD)){//end of a guard
			stats.flags.mBusy=false;
			 state=IDLE;
			Idle();
		}		
		return true;
	}
	public bool EdgeRollCheck(SPoint pos){
		//pos is the player position
		//required by reference to change it
		if( state== EDGEGRAB){
			stats.edgegrab.edgeTmr.ResetTimer();
			 state =  EDGEROLL;
			float actTime = fHelper.Animate("LedgeRoll", false, 0);
			fHelper.actionTmr.SetTimer(actTime);
			fHelper.airborne=false;//no land
			stats.flags.aBusy=true;
			stats.flags.mBusy=true;
			fHelper.airborne=false;
			fHelper.animLoopStart=0;
			pos.y+=stats.size.y;
			float trDist = stats.size.x;
			if(fHelper.IsFacingRight()){
				pos.x+=trDist;
				FighterTranslate(new SPoint(trDist,stats.size.y));
				fHelper.ReTranslate(new SPoint(stats.size.x, 0));//so anim starts in correct location
			}else{		
				pos.x-=trDist;
				FighterTranslate(new SPoint(-trDist, stats.size.y));
				fHelper.ReTranslate(new SPoint(-stats.size.x, 0));
            }

        }else
            return false;
        return true;
        
    }
	public bool  EdgeClimbCheck(SPoint pos, bool isJump){
		//pos is the player position
		//required by reference to change it
		if( state== EDGEGRAB){
			if(isJump){
				if(stats.jump.tmr.IsReady()){
					stats.jump.tmr.SetTimer();
					ledgeJumpFlag=true;
					fHelper.Animate("LedgeJump", true, stats.jump.tmr.GetLen());

				fHelper.airborne=true;
				}
				//fHelper.UnTranslate();
				//fHelper.UnTranslate(new SPoint(0,stats.size.y));
				
				//stats.motion.vel.y=stats.jump.vel;
				
				return true;
			}
			 state =  EDGECLIMB;
			fHelper.actionTmr.SetTimer(fHelper.Animate("LedgeClimb", false, 0));
			stats.flags.mBusy=true;
			stats.flags.aBusy=true;
			fHelper.airborne=false;
			fHelper.animLoopStart=0;
			pos.y+=stats.size.y;
			if(fHelper.IsFacingRight()){	
				pos.x+=stats.size.x*0.01f;
				fHelper.UnTranslate();
				FighterTranslate(new SPoint(stats.size.x*0.01f, stats.size.y));
			}else{		
				pos.x-=stats.size.x*0.01f;
				fHelper.UnTranslate();
				FighterTranslate(new SPoint(-stats.size.x*0.01f,stats.size.y));
			}
		}else
			return false;
		return true;
	}
	public void  Crouch(){
		if((!fHelper.airborne)&&(( state==IDLE)||( state== WALK)||( state== DASH)))
		if((stats.tumble.tmr.IsReady())&&(!stats.flags.aBusy)&&(!stats.flags.mBusy)){
			 state= CROUCH;
			
			if(!fHelper.anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch"))
				fHelper.Animate("Crouch", true, 0);
			fHelper.animLoopStart=0.15f;
		}
	}
	public bool Dash(){
		if((( state==IDLE)||( state== WALK))){
			 state= DASH;
			fHelper.Animate("Run", true, 0);
			
			stats.walk.shuffle.SetTimer();
			return true;
		}
		return false;
	}
	public void FaceRightIm(bool fR){
		//will immediately pivot the character 
		//to face the desired direction

		//1: Cancel conditions
		if (!grbTmr.IsReady ())
						return;
		//2: Actions
		if (fHelper.IsFacingRight () != fR) {
						fHelper.Pivot (180);
						fHelper.FaceRight (fR);
      
                }
	
	}
	public void Roll(){
		if (state == GUARD)
						stats.flags.mBusy = false;
		if ((stats.flags.aBusy)||(stats.flags.mBusy)||(fHelper.airborne)||(!atkTmr.IsReady()))
						return;
		PivotEnd ();
		stats.flags.mBusy = true;
		stats.flags.aBusy = true;

		float rTime = fHelper.Animate ("Roll", false, 0);
		fHelper.actionTmr.SetTimer(rTime);
		stats.flags.invuln = true;
		fHelper.TurnAfterAnim();
		 state =  ROLLING;
		stats.walk.gndSpeed = fHelper.IntFacingRight()*rollBurstSpeed;
		if(((leftLip!=0)&&(rightLip!=0))&&((stats.motion.pos.x == leftLip) || (stats.motion.pos.x == rightLip)))
						stats.walk.gndSpeed = 0;
		rollInvOn.SetTimer (rTime * rollInvulnStart);
		rollInvOff = new STimer(rTime*(rollInvulnEnd-rollInvulnStart));

	}
	public bool  SpotDodge(){
		if (!stats.jump.tmr.IsReady ())
			return false;
		if(( state==IDLE)||( state== CROUCH)){
			 state= DODGE;
			float actTime = fHelper.Animate("SpotDodge", false, 0);
			stats.dodge.SetTimer(actTime);
			fHelper.actionTmr.SetTimer(actTime);
		}else
			return false;
		return true;
	}
	public void Guard(){
			//guarding is only done from an grounded idle or movement state
			//prevents guarding out of attacks
			if((!fHelper.airborne)&&((InState(IDLE))||(InState( WALK))||(InState( CROUCH))))
				if(gCont.lStick.y<-VERT_THRESH)
					SpotDodge();
		else if(!InState( DODGE)){
				if(!fHelper.InAnim("Guard"))
					fHelper.Animate("Guard", false, 0.5f);
			 state= GUARD;
			stats.flags.mBusy=true;
			}
		}
	public bool WalkCheck(float wlStart, float lStickX){
		//separate walk code for detailed behavior
		float gndSpeed=stats.walk.gndSpeed;
		if((stats.flags.mBusy)||(!stats.tumble.tmr.IsReady()))
			return false;	
		if(( state !=  GUARD)&&( state!= EDGEGRAB)&&( state!= EDGECLIMB)&&( state!= ATTK)&&(fHelper.actionTmr.IsReady())){
			if((!fHelper.airborne)&&(lStickX!=0)){
				if( state!= DASH)
					 state =  WALK;
				if(gndSpeed==0){
					if(((fHelper.IsFacingRight())&&(lStickX<-Fighter.WALK_THRESH))
					   ||((!fHelper.IsFacingRight())&&(lStickX>Fighter.WALK_THRESH)) )
						FighterPivot();
				}
				else if(((fHelper.IsFacingRight())&&(gndSpeed<0))||((!fHelper.IsFacingRight())&&(gndSpeed>0)))
					FighterPivot();
				else if(( state== WALK)&&(stats.jump.tmr.IsReady())&&(!fHelper.anim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))){
					fHelper.UnTranslate();

					fHelper.Animate("Walk", true, 0);
					fHelper.animLoopStart=wlStart;
				}else if(( state== DASH)&&(stats.jump.tmr.IsReady())&&(!fHelper.anim.GetCurrentAnimatorStateInfo(0).IsName("Run")&&(!fHelper.anim.GetCurrentAnimatorStateInfo(0).IsName("Halt")))){
					fHelper.UnTranslate();
					fHelper.Animate("Run", true, 0);
					fHelper.animLoopStart=wlStart;
				}
			}
			if(((lStickX<=-GameController.LOWTHRESH)&&(stats.walk.gndSpeed>0))||((lStickX>=GameController.LOWTHRESH)&&(stats.walk.gndSpeed<0))){
				if(!fHelper.anim.GetCurrentAnimatorStateInfo(0).IsName("Halt")){
					fHelper.UnTranslate();
					fHelper.Animate("Halt", false, 0);
                    fHelper.animLoopStart=0;}
            }
        }else
			return false;
		if((Mathf.Abs(stats.walk.gndSpeed)<LOWTHRESH)&&(stats.walk.shuffle.IsReady() )){//new walk
			if(lStickX*stats.walk.gndSpeed>0)//axis and player moving in the same direction	
				stats.walk.shuffle.SetTimer();
		}
		return true;
	}
	public bool InState(int s){
		if(  state==s)
			return true;
		return false;
	}
	public bool RevertState(){
		//returns true if state changes
		if(fHelper.lastState!= state){
			 state=fHelper.lastState;
			return true;}
		return false;
	}
	void Walk(float timeLapsed){
		if(Mathf.Abs(stats.walk.slopeAng.GetDir()) > Mathf.PI/3)
			stats.walk.slopeAng=new SPoint(1,0);
		
		//walk accelleration handled here
		bool canWalk=WalkCheck(stats.walk.loopStart, gCont.lStick.x); //will be false if the player is unable to walk

		float spdLmt=stats.walk.maxSpeed;
		if(InState( DASH)){
			if(Mathf.Abs(stats.walk.gndSpeed)<0.5f)
				 state= WALK;
			else
				spdLmt=spdLmt*3;
		}
		if(stats.walk.gndSpeed<0){
			spdLmt=-spdLmt;

		}
		if( (InAttack (DATK))&&(fHelper.frame<0.5f)) {
			stats.walk.gndSpeed = stats.walk.maxSpeed * fHelper.IntFacingRight ();
		} else if (InState ( EDGEROLL)) {
			if(stats.edgegrab.rollMax>0)
			stats.walk.gndSpeed = stats.edgegrab.rollSpd * fHelper.IntFacingRight ();
			else
				stats.walk.gndSpeed=0;
		}
		//get velocity updates from input(controller
		//	if(gCont.lStick.x!=0)
		float maxSpeed=stats.walk.maxSpeed;
		if(canWalk){
			float walkCoeff = 1.0f - stats.walk.gndSpeed/spdLmt;
			if(walkCoeff>1.0)//cap
				walkCoeff=1.0f;

                //else if(((gCont.lStick.x<0)&&(stats.walk.gndSpeed<0))||((gCont.lStick.x>0)&&(stats.walk.gndSpeed>0)))//moving forward
			//	walkCoeff=1.0;
			if(fHelper.airborne)//using lateral DI here
				walkCoeff=walkCoeff/3;
				else if( state== DASH){
				walkCoeff=3;
				maxSpeed= maxSpeed*3;
			}
			
			walkCoeff=walkCoeff*walkCoeff;//square result, mult with timeLapsed
			
			if(gCont.lStick.x<0){
				walkCoeff=-walkCoeff;
				if((rightLip!=0)&&(stats.motion.pos.x-stats.size.x/2<rightLip)){
					walkCoeff = walkCoeff/13;
				 
					if(GetVel().x<0)
						stats.motion.vel.x=0;
				}
			}else if ((gCont.lStick.x > 0))
            {
				if((leftLip!=0)&&(stats.motion.pos.x+stats.size.x/2>leftLip)){
					walkCoeff = walkCoeff/13;
					
					 
                    if (GetVel().x > 0)
                        stats.motion.vel.x = 0;
                }
              
            }

			if((Mathf.Abs(stats.walk.gndSpeed) < Mathf.Abs(gCont.lStick.x*stats.walk.maxSpeed))){
				if(( state!= GUARD)&&( state!= DODGE)){		
					if((gCont.lStick.x>WALK_THRESH)&&(stats.walk.gndSpeed<0)&&(stats.pivot.pTmr.IsReady())&&( (!stats.walk.shuffle.IsReady())||(Mathf.Abs(stats.walk.gndSpeed*10.1f)<WALK_THRESH)) ){
						stats.walk.gndSpeed=1;
						stats.walk.shuffle.SetTimer();
					}else if((gCont.lStick.x<-WALK_THRESH)&&(stats.walk.gndSpeed>0)&&(stats.pivot.pTmr.IsReady())&&( (!stats.walk.shuffle.IsReady())||(Mathf.Abs(stats.walk.gndSpeed*10.1f)<WALK_THRESH))){
						stats.walk.gndSpeed=-1;
						stats.walk.shuffle.SetTimer();
					}else
						stats.walk.gndSpeed+=stats.walk.vel*walkCoeff;
				}
			}
		}
		//friction
		
		
		if(!fHelper.airborne){
			int ifr=fHelper.IntFacingRight();
			float fricRat = 1.0f-(ifr*gCont.lStick.x);//less friction the higher the stick axis is
			if(!canWalk)
				fricRat=1;//force to proper value
			if(!stats.tumble.tmr.IsReady())
				fricRat=TUMBLE_FRICRAT;
			stats.GroundSpeedReduce(stats.fric*fricRat);
			if((stats.walk.gndSpeed>maxSpeed)||(stats.walk.gndSpeed<-stats.walk.maxSpeed)){
				stats.GroundSpeedReduce(stats.fric);//double friction above max speed
			}
			stats.motion.vel.x = stats.walk.gndSpeed*stats.walk.slopeAng.x;
			stats.motion.vel.y = (stats.walk.gndSpeed*stats.walk.slopeAng.y)/stats.walk.slopeAng.x;	
		}else{//airborne; This is 'DI'
			if(stats.walk.dirInf>0){

				/*float diCoeff=(Mathf.Abs(GetVel().x)/stats.walk.maxSpeed)/2+0.5f;
				if(diCoeff>1)
					diCoeff=1;
				diCoeff = diCoeff*diCoeff;
				diCoeff=diCoeff*stats.walk.dirInf;
				if(diCoeff>0.1)
					diCoeff=0.1f;*/
				float diCoeff=(Mathf.Abs(GetVel().x)/stats.walk.maxSpeed*scaleDI);
				if(diCoeff>1)
					diCoeff=1;
				diCoeff = stats.walk.dirInf*stats.walk.dirInf*scaleDI;

				if(diCoeff<0.9f)
					diCoeff=0.9f;
				if(gCont.lStick.x*fHelper.IntFacingRight()<0)
					diCoeff=diCoeff/1.6f;
				stats.motion.vel.x+=diCoeff*gCont.lStick.x*stats.walk.vel;
				//float downScl = (reduceDI-1)/reduceDI;//reduce the rate of DI depletion when stick partially-tilted
				//stats.walk.dirInf=stats.walk.dirInf*downScl;


			}
			
		}
	}
	public void AnimateJump(){
		if(fHelper.airborne)
			fHelper.Animate("Jump", true, stats.jump.tmr.GetLen());
		fHelper.airborne=true;
		 state= FALL;
		stats.flags.aBusy=false;//just to make sure
		
		//if(animState->getAnimationName().compare("Jump")!=0){
		//	Animate("Jump", true, stats.jump.tmr.GetLen());
		
	}
	public  bool  JumpCheck(float jumpDelay){
		//for buffering a jump
		//this version will cause an airjump

		if (stats.land.tmr > 0)
						return false;
		if (!stats.tumble.tmr.IsReady ())
						return false;
		if( (( state==IDLE)||( state== FALL)||( state== WALK)||( state== DASH)) &&(stats.jump.tmr.IsReady())&& ((fHelper.actionTmr.IsReady())&&( state<100)) ){
			//the plus one here v is because maxjumps is 2 for double jumps, and 1 for no double jumping
			if((!fHelper.airborne)){
				fHelper.Animate("Jump", false, 0);
				fHelper.animLoopStart=0;
				if( state== DASH)
					 state= WALK;
			}else if(stats.jump.airJumps<stats.jump.maxJumps){
				fHelper.Animate("Jump", false, 0);
				fHelper.animLoopStart=0;
				stats.jump.airJumps++;
                
                
            }else
                return false;
            return true;
        }else
            return false;
        return false;
	}
	public void LedgeJump(){
		//fHelper.UnTranslate();
		//specialized Ledgejump
		float jVel = stats.jump.vel;
		//if(!gCont.Pressed(GameController.D)&&!gCont.Pressed(GameController.UP))//see if jump was cancelled here
		//	jVel=jVel/2;
		ledgeJumpFlag = false;
		stats.motion.vel.y += jVel;
		if(!fHelper.airborne){
			if((stats.walk.gndSpeed>WALK_THRESH)&&(gCont.lStick.x<0))
				stats.motion.vel.x = gCont.lStick.x*stats.walk.maxSpeed/2;
			else if((stats.walk.gndSpeed<-WALK_THRESH)&&(gCont.lStick.x>0))
				stats.motion.vel.x = gCont.lStick.x*stats.walk.maxSpeed/2;
		}
		stats.walk.slopeAng=new SPoint(1,0);
		state= FALL;
		fHelper.UnTranslate();
		fHelper.nextAnim="";
		stats.motion.move.y=stats.size.y/5;

		stats.flags.aBusy=false;//just to make sure
	}
	public void Jump(){
		if (stats.land.tmr > 0)
						return;
		float jVel = stats.jump.vel;
		if(!gCont.Pressed(GameController.D)&&!gCont.Pressed(GameController.UP))//see if jump was cancelled here
			jVel=jVel/2;
		stats.motion.vel.y += jVel;
		if(!fHelper.airborne){
			if((stats.walk.gndSpeed>WALK_THRESH)&&(gCont.lStick.x<0))
				stats.motion.vel.x = gCont.lStick.x*stats.walk.maxSpeed/2;
			else if((stats.walk.gndSpeed<-WALK_THRESH)&&(gCont.lStick.x>0))
				stats.motion.vel.x = gCont.lStick.x*stats.walk.maxSpeed/2;
		}
		stats.walk.slopeAng=new SPoint(1,0);
		AnimateJump ();
	}
	public virtual void ThrowPlr(int tType){
		thwTmr.SetTimer (throwTime);
		stats.flags.aBusy = true;
		fHelper.Animate("Throw", false, 0);
	}
	public virtual void ThrowMotion(int tType, float dmg, float dir, float mag){
		//basic throw 
		if(grabbedPlr==null)
			return;//no throw
		float diff = 0;
		if (!fHelper.IsFacingRight()) {
			diff = ((Mathf.PI / 2) - dir) * 2.0f;
			dir += diff;
		}
		AttkData thr = new AttkData();
		thr.dmg=dmg;
		thr.dir=dir;
		thr.mag=mag;
		thr.isThrow = true;
		//felper.Animate("Throw", false, 0);

		grabbedPlr.GetHit(thr);
		grabbedPlr.stats.tumble.tmr.SetTimer (1.0f);
		grabbedPlr.transform.localEulerAngles = NORM_ORIENT;
		grabbedPlr.stats.flags.mBusy = false;
		grabbedPlr.stats.flags.aBusy = false;
		grabbedPlr. state =  FALL;
		SPoint rPos = new SPoint(GetPos ().x, GetPos().y);
		//if (Mathf.Sin (thr.dir) < 0)
		//	rPos.x -= stats.size.x;
		//else if (Mathf.Sin (thr.dir) > 0)
		//	rPos.x += stats.size.x;
		rPos.y += 4.0f;
		grabbedPlr.RePosition(rPos);
		grabbedPlr.stats.motion.lastPos = new SPoint(grabbedPlr.stats.motion.lastPos.x, grabbedPlr.stats.motion.lastPos.y+1.0f);
		grabbedPlr.fHelper.UnTranslate ();
		if(grabbedPlr.fHelper.IsFacingRight())
			grabbedPlr.fHelper.Orient(0);
		else
			grabbedPlr.fHelper.Orient(180);
        grabbedPlr = null;
		stats.flags.aBusy = false;

	}
	public void RePosition(SPoint rep){
		stats.motion.lastPos.x = rep.x;
		stats.motion.lastPos.y = rep.y;
		stats.motion.pos.x = rep.x;
		stats.motion.pos.y = rep.y;
		 
		SetPos(rep);

	}
	public void Grabbed(){
		if (!stats.dodge.IsReady ())
			return;
		 state= GRABBED;

		stats.flags.aBusy = true;
		stats.flags.mBusy = true;
		fHelper.Animate("Grabbed", false, 0);
	}
	public void TakeHit(AttkData hit){
		float dmgScale = 1.0f;
		if (stats.defence != 0)
			dmgScale = stats.damage / stats.defence;

		float tumTime = 0.1f +(0.5f+hit.dmg/250.0f)*dmgScale;
		dmgScale++;
		if (hit.dmg * dmgScale > stats.tumble.thresh) {
			stats.tumble.tmr.SetTimer (tumTime);


			stats.flags.aBusy=true;
		}


		float uVel, vVel, xVel, yVel;
		uVel = magScale*hit.mag*Mathf.Cos(hit.dir);//this determines the height to move up to
		vVel = magScale*hit.mag*Mathf.Sin(hit.dir);//this determines rough distance

		if(uVel>0)	
			xVel= Mathf.Sqrt(uVel*stats.grav);
		else 
			xVel= -Mathf.Sqrt(-uVel*stats.grav);
		if(vVel>0)
			yVel = Mathf.Sqrt(vVel*2*stats.grav);
		else
			yVel = -Mathf.Sqrt(-vVel*2*stats.grav);
		yVel*=dmgScale;
		xVel*=dmgScale;
		

		//TurnCheck(xVel);
		if(Mathf.Abs(xVel) < 1.2f)
			stats.walk.gndSpeed=xVel*(1+stats.fric);
		else{
			yVel=Mathf.Abs(xVel)/3.0f;
			stats.motion.vel.y=yVel;
			stats.motion.vel.x=xVel;
			Fall();
		}
		if ((!fHelper.airborne)) {//Grounded down momentum changed to up
			if(yVel<0){
				yVel = -0.8f*yVel;
				fHelper.airborne=true;
			}
        }
        if ((fHelper.airborne) || (yVel > Mathf.Abs (xVel) + 0.1f)) {//check to see if the hit was
				fHelper.airborne = true; //hard enough to launch the player into
				rightLip = 0;
				leftLip = 0;
				stats.motion.vel.y = yVel; //airborne state
				stats.motion.vel.x = xVel;
				if (Mathf.Abs (xVel) > 2)
						fHelper.Animate ("SentFlying", false, 0);
				else
						fHelper.Animate ("Tumble", false, 0);
		}
                 
        if (debugModeOn) {
			string log = "";
			log += CharName () + " hit!v(" + xVel + "," + yVel + ")m" + hit.mag + "d" + dmgScale;
			ConsoleLog (log);
		}

		dmgFlag = true;
		}
	public void GetHit(AttkData hit){
		//called when the player has taken a hit
		atkTmr.ResetTimer ();
		fHelper.nextAnim="";
		if (InState (EDGEGRAB)) {
						state = FALL;
						stats.edgegrab.delayTmr.SetTimer ();
				}
	//	int ifr = fHelper.IntFacingRight ();
		if (stats.flags.invuln)
						return;//dodged
		else if (hit.isThrow) {
						Grabbed ();
				} else if (InState (GUARD)) {
						for (double r = -Mathf.PI; r<Mathf.PI*2; r+=Mathf.PI*2)//check all transforms
								if ((hit.adir + r < stats.guard.dir + stats.guard.arc) && (hit.adir + r > stats.guard.dir - stats.guard.arc)) {
										OnGuard (hit);
										return;//attack blocked
								}
							stunTimer.SetTimer (hit.dmg*stunMult / stats.defence);

				} else {
			stunTimer.SetTimer (hit.dmg*stunMult / stats.defence);
        }
		hitHolder = hit;

		stats.damage+=hit.dmg;
		PlayEffect (EF_HIT);
		PlayGeneralSound ("hit");
		fHelper.Animate("Hit", false, 0);
        if (hit.isThrow)
						TakeHit (hit);
		if (hit.isVacuum) {
			stats.motion.move.x = hit.cenPt.x;//-stats.motion.pos.x;		
			stats.motion.move.y = hit.cenPt.y;//-stats.motion.pos.y;		
		}
        
        //TakeHit (hit);
	}
	public void RunTimers(float timeLapsed){
		if(stats.jump.tmr.RunTimer(timeLapsed)){
			if(!fHelper.airborne){
				Jump();
			}
		}
		if(stunTimer.RunTimer(timeLapsed)){
			if(hitHolder!=null)
				TakeHit (hitHolder);
			hitHolder=null;
		}
	    if(!debugFadeTmr.IsReady()){
			if(debugFadeTmr.RunTimer (timeLapsed))
				Destroy (attkHitbox);
		}
		if(rollInvOn.RunTimer(timeLapsed)){
			rollInvOff.SetTimer();
			stats.flags.invuln=true;

		}if (rollInvOff.RunTimer (timeLapsed)) {
			stats.flags.invuln=false;		
		}
		//attack timer
		atkTmr.RunTimer (timeLapsed);
		
		//edge timers
		if(stats.edgegrab.delayTmr.RunTimer(timeLapsed))
			fHelper.grabBox.isActive = true;
	
		if(InState( EDGEGRAB)){
			stats.motion.vel.y = 0;
			if((stats.edgegrab.edgeTmr.RunTimer(timeLapsed))||(gCont.lStick.y<-LOWTHRESH)){
				stats.flags.aBusy=false;
				stats.flags.mBusy=false;
				fHelper.UnTranslate();
				Fall();
				fHelper.grabBox.isActive = false;
				stats.motion.pos.y -= stats.edgegrab.hgt;
				stats.motion.vel.y = -4*stats.grav;
				stats.edgegrab.delayTmr.SetTimer();
				stats.edgegrab.edgeTmr.ResetTimer();
			}
		}else{
			if(stats.edgegrab.flag){//end of an edgegrab
				fHelper.grabBox.isActive = false;
				stats.edgegrab.flag = false;
				//fHelper.UnTranslate();
				//Translate(SPoint(0, stats.size.y));
				stats.edgegrab.delayTmr.SetTimer();
				stats.motion.pos.y-=stats.edgegrab.hgt;
				stats.motion.vel.y = -4*stats.grav;
			}
		}
		 
			//timer to track pivoting between left/right
		if(stats.pivot.pTmr.tmr>0){
			if(stats.pivot.pTmr.RunTimer(timeLapsed))
				PivotEnd();
			else if (stats.pivot.slowPivot)
				fHelper.Pivot((0.25f*timeLapsed/stats.pivot.pTmr.GetLen())*180.0f);
			else
				fHelper.Pivot((timeLapsed/stats.pivot.pTmr.GetLen())*180.0f);
			}
		else if(stats.pivot.pTmr.tmr<0){
			if(stats.pivot.pTmr.RunTimer(timeLapsed))
					PivotEnd();
			else if (stats.pivot.slowPivot)
				fHelper.Pivot(-0.25f*(timeLapsed/stats.pivot.pTmr.GetLen())*180.0f);
			else
				fHelper.Pivot(-(timeLapsed/stats.pivot.pTmr.GetLen())*180.0f);
			}
			if(stats.walk.shuffle.RunTimer(timeLapsed))
			stats.flags.running=true;
			
		if(Mathf.Abs(stats.walk.gndSpeed)<stats.walk.maxSpeed/2)
				stats.flags.running =false;
			//time between jump command and action (windup frames)
			
			
			
			//landing stuff added here
			if (stats.land.RunTimer (timeLapsed)) {
						stats.flags.aBusy = false;
			stats.flags.mBusy=false;
				}
			//actiontimer
			//Timer(timeLapsed);
			
			
			//tumble timer
			if (!stats.tumble.tmr.IsReady()) {
				if (stats.tumble.tmr.RunTimer (timeLapsed))//stop anim + set state idle
                if(!fHelper.airborne)
                    GetUp ();
            } else
			stats.tumble.tmr.ResetTimer ();
				
            //dodge timer
            if(InState( DODGE)){
				stats.flags.invuln=true;
				if(stats.dodge.RunTimer(timeLapsed)){
					stats.flags.invuln=false;
					stats.flags.aBusy=false;
					stats.flags.mBusy=false;
					 state=IDLE;
                    //Idle();
				 
			}
		}


		//fHelper.RunTimers(timeLapsed);
	}
	protected int AttStringToIndex (string s){
		//returns negative sentinel value if no conversion found
		if (s.CompareTo ("NA") == 0)
			return NA;
		if (s.CompareTo ("SA") == 0)
			return SA;
		if (s.CompareTo ("DA") == 0)
			return DA;
        if (s.CompareTo ("UA") == 0)
            return UA;
		if (s.CompareTo ("NB") == 0)
			return NB;
		if (s.CompareTo ("SB") == 0)
			return SB;
		if (s.CompareTo ("DB") == 0)
			return DB;
        if (s.CompareTo ("UB") == 0)
            return UB;
		if (s.CompareTo ("NC") == 0)
			return NC;
		if (s.CompareTo ("SC") == 0)
			return SC;
		if (s.CompareTo ("DC") == 0)
			return DC;
        if (s.CompareTo ("UC") == 0)
            return UC;
		if (s.CompareTo ("NAIR") == 0)
			return NAIR;
		if (s.CompareTo ("FAIR") == 0)
            return FAIR;
		if (s.CompareTo ("BAIR") == 0)
			return BAIR;
		if (s.CompareTo ("DAIR") == 0)
            return DAIR;
		if (s.CompareTo ("UAIR") == 0)
			return UAIR;
		if (s.CompareTo ("DATK") == 0)
            return DATK;
		if (s.CompareTo ("SPA") == 0)
			return SPA;
		if (s.CompareTo ("SPB") == 0)
			return SPB;
		if (s.CompareTo ("SPC") == 0)
			return SPC;
        
        else
            return -1;
    }
	virtual protected void SetUniqueMove(string uName, MoveInstruct uMove){
		//should not be called here
	}
	virtual protected bool LoadMoveScript(TextAsset file){
		bool hr = true;
		bool uniFlag = false;
		//string line = "";
		string lPc = "";
		int splitInd = 0;
		int atInd = -1;
		int cInd = 0;
		int[] instructNum = new int[MOVENUM]; 
		int[] uniqueNum = new int[UNIQUE_LMT];
		MoveInstruct uniMove = new MoveInstruct();
		int uInd = 0;
		moveCont = new MoveInstruct[MOVENUM];
		string[] lines = file.text.Split('\n');
		string line;	
		foreach (string rline in lines){
			line = rline.Trim();
					if(FirstNCharChk(line, "<ATTACK")){
						splitInd = line.IndexOf(":")+1;
						atInd = AttStringToIndex(line.Substring(splitInd, line.IndexOf(">")-8));
						if(atInd>=0)
						instructNum[atInd]=0;
	
					}
					if(FirstNCharChk(line, "<UNIQUE")){
						splitInd = line.IndexOf(":")+1;
						uniFlag=true;
						//atInd = AttStringToIndex(line.Substring(splitInd, line.IndexOf(">")-8));
						if(atInd>=0)
							atInd=-1;
						
					}
					if(FirstNCharChk(line, "time")){
						if(atInd>=0)
							instructNum[atInd]++;
						else if(uniFlag)
							uniqueNum[uInd]++;
					}
					if(FirstNCharChk(line, "</ATTACK>")){
						atInd=-1;
					}
					if(FirstNCharChk(line, "</UNIQUE>")){
						uInd++;
					}
				
				}
			
				//infile.DiscardBufferedData(); 
				//infile.BaseStream.Seek(0, SeekOrigin.Begin); 
				//infile.BaseStream.Position = 0;
				cInd=0;
				uInd=0;
				string uniName = "";
		foreach (string rline in lines){
			line = rline.Trim();		
			if(FirstNCharChk(line, "<ATTACK")){
						splitInd = line.IndexOf(":")+1;
						atInd = AttStringToIndex(line.Substring(splitInd, line.IndexOf(">")-8));
						moveCont[atInd]=new MoveInstruct(instructNum[atInd]);
						uniFlag=false;
						//instantiate moveset instructions
                        
					}if(FirstNCharChk(line, "<UNIQUE")){
						splitInd = line.IndexOf(":")+1;
						uniName =  line.Substring(splitInd, line.IndexOf(">")-8);
						uniMove=new MoveInstruct(uniqueNum[uInd]);
						uniFlag=true;
						atInd=-1;
						//moveCont[atInd]=new MoveInstruct(instructNum[atInd]);
						//instantiate moveset instructions
						
					}
					if(FirstNCharChk(line, "time")){
						//moveCont.

						lPc = line.Substring(5);
						float time = float.Parse (lPc.Substring(0, lPc.IndexOf(" ")));
						lPc = lPc.Substring(lPc.IndexOf(" ")+1);

						if(FirstNCharChk(lPc, "burst")){
							lPc = lPc.Substring(lPc.IndexOf(" ")+1);
							float val = float.Parse (lPc);
							if(atInd>=0)
								moveCont[atInd].SetInstruct(cInd, time, "BURST",  val);
							else if(uniFlag)
								uniMove.SetInstruct(cInd, time, "BURST",  val);
						}else if(FirstNCharChk(lPc, "airburst")){
							lPc = lPc.Substring(lPc.IndexOf(" ")+1);
							float xV = float.Parse (lPc.Substring(0, lPc.IndexOf (" ")));
							lPc = lPc.Substring(lPc.IndexOf(" ")+1);
                            float yV = float.Parse (lPc);
                            if(atInd>=0)
                                moveCont[atInd].SetInstruct(cInd, time, "AIRBURST",  new SPoint(xV, yV));
							else if(uniFlag)
								uniMove.SetInstruct(cInd, time, "AIRBURST",  new SPoint(xV, yV));
                        }
						else if(FirstNCharChk(lPc, "slide")){
							lPc = lPc.Substring(lPc.IndexOf(" ")+1);
							float val = float.Parse (lPc);
							if(atInd>=0)
								moveCont[atInd].SetInstruct(cInd, time, "SLIDE",  val);
							else if(uniFlag)
								uniMove.SetInstruct(cInd, time, "SLIDE",  val);
						}else if(FirstNCharChk(lPc, "brake")){
							lPc = lPc.Substring(lPc.IndexOf(" ")+1);
							float val = float.Parse (lPc);
							if(atInd>=0)
								moveCont[atInd].SetInstruct(cInd, time, "BRAKE",  val);
							else if(uniFlag)
								uniMove.SetInstruct(cInd, time, "BRAKE",  val);
						}else if(FirstNCharChk(lPc, "airbrake")){
							lPc = lPc.Substring(lPc.IndexOf(" ")+1);
							float xV = float.Parse (lPc.Substring(0, lPc.IndexOf (" ")));
							lPc = lPc.Substring(lPc.IndexOf(" ")+1);
                            float yV = float.Parse (lPc);
							if(atInd>=0)
								moveCont[atInd].SetInstruct(cInd, time, "AIRBRAKE",   new SPoint(xV, yV));
							else if(uniFlag)
								uniMove.SetInstruct(cInd, time, "AIRBRAKE",   new SPoint(xV, yV));
                        }
                        cInd++;
                    }
                    if(FirstNCharChk(line, "</ATTACK>")){
						atInd=-1;
						cInd=0;
					}
					if(FirstNCharChk(line, "</UNIQUE>")){
						SetUniqueMove (uniName, uniMove);
						atInd=-1;
						uInd++;
						cInd=0;
					}
				}


       
	
	return hr;
	
	}
	protected bool FirstNCharChk(string line, string comp){
		int n = comp.Length;
		if ((line.Length >= n) && (line.Substring (0, n).CompareTo (comp) == 0)) 
			return true;
		else
				return false;
	}
	protected bool LoadFighter(){
	//should be called from a child class
		return false;
	}
	public SPoint GetLastPos(){
		return stats.motion.lastPos;
	}
	
	public string CharName(){
		string rtnStr="";
		
		switch(stats.id.fighter){
		case SERENITY :
			rtnStr="Serenity";
			break;
		case SERENITYSWORD :
			rtnStr="PsySword";
			break;
		case YARA :
			rtnStr="Yara";
			break;
		case KIROCH :
			rtnStr="Kiroch";
			break;
		}
		return rtnStr;
	}
	public SPoint GetPos(){
		return stats.motion.pos;
	}
	public SPoint GetVel(){
		return stats.motion.vel;
	}

	public virtual void SetPos(SPoint p){
		stats.motion.pos = new SPoint (p.x, p.y);
		fHelper.TR.position=new Vector3(p.x, p.y, 0);
		if (debugHitbox != null) {
						
						debugHitbox.transform.position = new Vector3 (stats.motion.pos.x, stats.motion.pos.y, 0);
								}

	}
	public SPoint[] GetCurrentColBox(){
			//look at point 0 to determine index to ignore
			SPoint sP=stats.motion.pos;
			SPoint lP=stats.motion.lastPos;
			SPoint[] getP = GetHitBox();
			SPoint[] lasP = GetLastHitBox();
			SPoint[] retP = new SPoint[6];
			int iInd=0;
			int oInd=2;
			int j=0;
			if((sP.x<=lP.x)&&(sP.y>lP.y)){
				iInd=1;
				oInd=3;
			}
			else if((sP.x<=lP.x)&&(sP.y<=lP.y)){
			int iew=0;
			if(lP.x-sP.x<5)
                iew=0;
				iInd=2;
				oInd=0;
			}
			else if((sP.x>lP.x)&&(sP.y<=lP.y)){

				iInd=3;
				oInd=1;
			}else if((sP.x>lP.x)&&(sP.y>lP.y)){
				iInd=0;
				oInd=2;
			}
			
			int k=iInd+1;//start from after this break for convenience
			if(k==4)
				k=0;
			for(int i=0;i<3;i++){
				if(k==4)
					k=0;
				retP[j]=lasP[k];//print the 3 verts from lasp
				j++;
				k++;
				
			}
		k = oInd + 1;
			for(int i=0;i<3;i++){//current
				
					if(k==4)
						k=0;
					retP[j]=getP[k];//print the next three
					j++;
					k++;
				
			}
			
			return retP;
		}
	public SPoint[] GetHitBox(){
		//0=Upper Right
		//1=Upper Left
		//2=Lower Left
		//3=Lower Right
		SPoint[] retP = new SPoint[4];
		retP[0] = new SPoint(Right(), Top());
		retP [1] = new SPoint (Left(),Top ()); 
		retP [2] = new SPoint (Left (), Bottom ());
		retP[3]= new SPoint(Right(), Bottom());
		return retP;
	}
	public SPoint[] GetLastHitBox(){
		//0=Upper Right
		//1=Upper Left
		//2=Lower Left
		//3=Lower Right	
		SPoint[] retP = new SPoint[4];
		for (int i=0; i<4; i++)
			retP[i] = new SPoint ();
		retP[0].x = stats.motion.lastPos.x + stats.size.x/2;
		retP[0].y = stats.motion.lastPos.y + stats.size.y;
		retP[1].x = stats.motion.lastPos.x - stats.size.x/2;
		retP[1].y = stats.motion.lastPos.y + stats.size.y;
		retP[2].x = stats.motion.lastPos.x - stats.size.x/2;
		retP [2].y = stats.motion.lastPos.y;
		retP[3].x = stats.motion.lastPos.x + stats.size.x/2;
		retP [3].y = stats.motion.lastPos.y;
		return retP;
	}
	public SPoint[] GetGrabBox(){
		SPoint[] retP = new SPoint[4];
		for (int i=0; i<4; i++)
			retP[i] = new SPoint ();
		retP[0].x = GetPos().x + fHelper.grabBox.GetSPoint(0, 0).x;
		retP[0].y = GetPos().y + fHelper.grabBox.GetSPoint(0, 0).y;
		retP[1].x = GetPos().x + fHelper.grabBox.GetSPoint(0, 1).x;
		retP[1].y = GetPos().y + fHelper.grabBox.GetSPoint(0, 1).y;
		retP[2].x = GetPos().x + fHelper.grabBox.GetSPoint(0, 2).x;
		retP[2].y = GetPos().y + fHelper.grabBox.GetSPoint(0, 2).y;
		retP[3].x = GetPos().x + fHelper.grabBox.GetSPoint(0, 3).x;
		retP[3].y = GetPos().y +fHelper.grabBox.GetSPoint(0, 3).y;
		return retP;
	}

	public bool CheckAxis( SPoint origin, float dir, SPoint[] plBox, SPoint[] aBox, int pNum, int aNum){
		// helper function for the Separating Axis Theorem that takes an axis defined by origin and dir 
		//re-conditioned to handle the parameters of the attack box 
		bool isHit = true;
		float projMin, projMax, hitMin, hitMax, distSq, projVal, cVal;
		SPoint projVec, nVec, rVec, cVec;
		projVec = new SPoint();
		projMin = AXIS_LIMIT;
		projMax = -AXIS_LIMIT;
		hitMin = AXIS_LIMIT;
		hitMax = -AXIS_LIMIT;	
		for(int i = 0; i < aNum; i++){
			projVec.x = aBox[i].x - origin.x;
			projVec.y = aBox[i].y - origin.y;
			
			distSq = projVec.x*projVec.x + projVec.y*projVec.y;
			nVec=projVec.GetNormal();
			
			
			cVec = new SPoint(Mathf.Cos(dir+Mathf.PI/2),Mathf.Sin(dir+Mathf.PI/2));
			
			cVal = -cVec.Dot(nVec);
			//projVal = sin(rAng)*abs(sin(rAng))*distSq;
			projVal = cVal*Mathf.Abs(cVal)*distSq;
			
			if(projVal < projMin)
				projMin = projVal;
			if(projVal > projMax)
				projMax = projVal;
		}
		for(int i = 0; i < pNum; i++){
			projVec.x = plBox[i].x - origin.x;
			projVec.y = plBox[i].y - origin.y;
			distSq = projVec.x*projVec.x + projVec.y*projVec.y;
			cVec = new SPoint(Mathf.Cos(dir+Mathf.PI/2),Mathf.Sin(dir+Mathf.PI/2));
			nVec = projVec.GetNormal();
			cVal = -cVec.Dot(nVec);
			//projVal = sin(rAng)*abs(sin(rAng))*distSq;
			projVal = cVal*Mathf.Abs(cVal)*distSq;
			if(projVal < hitMin)
				hitMin = projVal;
			if(projVal > hitMax)
				hitMax = projVal;
		}
		if((hitMax <= projMin)||(projMax <= hitMin))
			isHit=false;
		if((Mathf.Abs(projMax - hitMin)< EPS)||(Mathf.Abs(hitMax - projMin)< EPS))
			isHit=false;
		return isHit;
	}
	public void OnGuard(AttkData hit){
		stats.guard.arc-=hit.dmg/stats.defence;//formula for now
	}
	public void FallT(){
		if(( state!= ATTK)&&( state!= GRABBED)){
			 state= FALL;
			fHelper.Animate("Idle", true, 0);
		}
		fHelper.airborne=true;
	}
	public void Fall(){	
		//fHelper.UnTranslate();
		if((!InState( FALL))&&(!InState( EDGECLIMB)))
		{
			 state= FALL;
			if(!stats.tumble.tmr.IsReady())
				fHelper.Animate("Fall", true, 0);
		}
		atkTmr.ResetTimer ();
		fHelper.actionTmr.ResetTimer ();
		rightLip = 0;
		leftLip = 0;
		stats.walk.slopeAng=new SPoint(1,0);
		stats.walk.gndSpeed = 0;
		fHelper.airborne = true;
	}
	
	public void Land(float angle){
		//Also called by c_Stage::CollisionDetect whenever the player
		//hits the ground from above
		//there is no more 'Tumble' state
		//the below code will check the 'tumble' timer
		//if (!stats.tumble.tmr.IsReady()) {
		//	GameObject cons = new GameObject();
		//	cons.name="TumbleLand";
	//	ledgeJumpFlag = false;	
		//}
		if(angle > Mathf.PI/2.0)
			angle -= Mathf.PI;
		if(angle < -Mathf.PI/2.0)
			angle += Mathf.PI;
		if(Mathf.Abs(angle) < Mathf.PI/3){ //landing on a flat-ish ground
			stats.walk.slopeAng.x = Mathf.Cos(angle);
			stats.walk.slopeAng.y = Mathf.Sin(angle);
			if(fHelper.airborne){
				stats.flags.landed=true;
				
				fHelper.airborne=false;
				fHelper.actionTmr.ResetTimer();

				stats.jump.airJumps=0;
				if(!stats.tumble.tmr.IsReady ()){
					float rollVel=Mathf.Sqrt(stats.motion.vel.SqDistFromOrigin());

					if(stats.motion.vel.x<0)
						rollVel = -rollVel;
					if((stats.motion.vel.x*fHelper.IntFacingRight() >0)&&(stats.land.IsReady()))
						FaceRightIm (!fHelper.IsFacingRight());
					stats.land.SetTimer(fHelper.Animate("Tbland", false, 0));
						

					stats.walk.gndSpeed=rollVel;
					string st = "rollVel: ";
					st += rollVel + "/" + stats.motion.vel.x;
					ConsoleLog (st);
				}
				else if(!atkTmr.IsReady()){
					if(attackBox.HasHits(fHelper.atkType)){
						stats.land.SetTimer(fHelper.Animate("Land", false, 0)/LAGCANCEL);
						fHelper.animSpeed=LAGCANCEL;
						stats.flags.aBusy=true;
						stats.flags.mBusy=true;
                        fHelper.actionTmr.SetTimer(stats.land.GetLen());
					}else{
					stats.land.SetTimer(fHelper.Animate("Land", false, 0));
					stats.flags.aBusy=true;
					stats.flags.mBusy=true;
					fHelper.actionTmr.SetTimer(stats.land.GetLen());
					}
				}
				else{
					fHelper.Animate("Land", false, 0);

				}
				//stats.tumble.tmr.ResetTimer();
				atkTmr.ResetTimer();
				 state=IDLE;
				stats.jump.airJumps=0;
				stats.walk.gndSpeed = stats.motion.vel.x; //cut in half for landing
				stats.walk.dirInf=1;
			}
			//fHelper.Land();

			if((fHelper.airborne)&&(gCont.Tapped(GameController.DN))){//quick landing
				float qckLndSpd=2;
				fHelper.SetAnimSpeed(qckLndSpd);
				stats.land.tmr=stats.land.tmr/qckLndSpd;
				
			}
			recUsed=0;
			if(!InState( FALL))
				stats.walk.slopeAng=new SPoint(1,0);
		}
	}
	public float GetVelF(){
		//for coverting SPoint vel into float
		float velF = stats.motion.vel.SqDistFromOrigin();
		return Mathf.Sqrt (velF);
	
	}
	public bool Rebound(float rAng){
        //returns true of a rebound occurs	
        //called automatically in Stage.CollisionDetect(...)
        //returns false when no rebound occurs
        if (!fHelper.airborne)
            return false;
		SPoint nVec = new SPoint(Mathf.Cos(rAng-Mathf.PI/2), Mathf.Sin(rAng-Mathf.PI/2));//should be normalized already
		float two_n_dot_d = 2*(nVec.x*GetVel().x + nVec.y*GetVel().y);
		SPoint rVec = new SPoint(GetVel().x - two_n_dot_d*nVec.x, GetVel().y - two_n_dot_d*nVec.y);
		float rDotN =  rVec.x*nVec.x + rVec.y+nVec.y ;//angle of reflection
		float b_r=0;
		float fVel = GetVelF ()-tu_vRoThresh;//speed in units
		if (fVel > 0) { //if player is moving faster than the 'roll' threshhold
			b_r = (GetVelF())/ (tu_vReThresh-tu_vRoThresh);
			//bounce angle between 1 and 0, to determine how shallow to bounce
			if(b_r>1.0f)//simple sanity checks
				b_r=1.0f;
			else if(b_r<0)
				b_r=0;
        }

		//now determine rVec. 
		//interpolate between rVec (max reflection)
		//and sVec(sliding)
	
		SPoint sVec = new SPoint ();
		sVec.x = Mathf.Cos (rAng)*fVel;
		sVec.y = Mathf.Sin (rAng)*fVel;
	//	if (GetVel ().x > 0)
	//		sVec.x = sVec.x * -1;//for facing in opposite direction


		SPoint cVel=new SPoint();//to combine 
		cVel.x = rVec.x*b_r+sVec.x*(1.0f-b_r);
		cVel.y = rVec.y*b_r+sVec.y*(1.0f-b_r);
        if (cVel.x < 0)
            cVel.x = cVel.x;
		cVel.x = cVel.x * tu_bncRat;//bounce ratio here
		cVel.y = cVel.y * tu_bncRat;//bounce ratio here

		//Check for a tech
		float stAng=0;
		if (gCont.LStickTapped()) {
			SPoint stInv = new SPoint(-gCont.lStick.x, -gCont.lStick.y);
			stAng = stInv.GetDir ();
			if(Mathf.Abs (stAng-nVec.GetDir())<(Mathf.PI/4) ){
				cVel.x = cVel.x*0.1f;
				cVel.y = cVel.y*0.1f;
			}
		}
		//if(rDotN> stats.tumble.thresh){
		if( (fVel>0)){
			stats.motion.vel = new SPoint(cVel.x, cVel.y);
			//TurnCheck(GetVel().x);
			if((Mathf.Abs(rAng) < Mathf.PI/3)||(Mathf.Abs(rAng-Mathf.PI) < Mathf.PI/3)||(Mathf.Abs(rAng+Mathf.PI) < Mathf.PI/3) ){ //landing on a flat-ish ground
				if(!stats.tumble.tmr.IsReady()){		
					fHelper.Animate("Slam", false, 0);
					Land (rAng);
					float spd = Mathf.Sqrt(stats.motion.vel.SqDistFromOrigin());
				}else
					return false;//just land
			}else
				if(!stats.tumble.tmr.IsReady())		
					fHelper.Animate("WallSlam", false, 0);
			else
                return false;//just land
            
		}else
			return false;
		stats.tumble.rbdCount++;
		return true;
	}

	
	void UpdateGrabBox(){
		float widDiv2 = stats.size.x/2;//precalculate for optimization
		float hgtSum = stats.size.y+stats.edgegrab.hgt;
		if(!fHelper.IsFacingRight()){
			fHelper.grabBox.SetVertex(-widDiv2 - stats.edgegrab.wid, hgtSum, 0, 0);
			fHelper.grabBox.SetVertex(-widDiv2 - stats.edgegrab.wid, stats.size.y, 0, 1);
			fHelper.grabBox.SetVertex(-widDiv2, stats.size.y, 0, 2);
			fHelper.grabBox.SetVertex(-widDiv2, hgtSum, 0, 3);
		}else{
			fHelper.grabBox.SetVertex(widDiv2, hgtSum, 0, 0);
			fHelper.grabBox.SetVertex(widDiv2, stats.size.y, 0, 1);
			fHelper.grabBox.SetVertex(widDiv2 + stats.edgegrab.wid, stats.size.y, 0, 2);
			fHelper.grabBox.SetVertex(widDiv2 + stats.edgegrab.wid, hgtSum, 0, 3);
		}
	}
	public bool InAttack(int a){
		//check if state is attack and proper attack
		//will return even if the attack has ended
		if((fHelper.atkType==a)&&( state== ATTK))
			return true;
		return false;
	}
	public bool StateChange(){
		//returns true if state changes
		if(fHelper.lastState!= state)
			return true;
		return false;
	}
	public bool EdgeAttack(SPoint pos){
		//pos is the player position
		//required by reference to change it
		if( state== EDGEGRAB){
			 state =  EDGEATTK;
			
			fHelper.airborne=false;//no land
			stats.flags.aBusy=true;
			stats.flags.mBusy=true;
			fHelper.airborne=false;
			fHelper.animLoopStart=0;
			pos.y+=stats.size.y;
			float trDist = stats.size.x;
			if(fHelper.IsFacingRight()){
				pos.x+=trDist;
				//transform.Translate(trDist,stats.size.y,0,Space.World);
				FighterTranslate (new SPoint(trDist, stats.size.y));
				fHelper.ReTranslate(new SPoint(-stats.size.x, 0));//so anim starts in correct location
			}else{		
				pos.x-=trDist;
				//transform.Translate(-trDist,stats.size.y,0,Space.World);
				FighterTranslate (new SPoint(-trDist, stats.size.y));
				fHelper.ReTranslate(new SPoint(	stats.size.x, 0));
            }
        }else
            return false;
        return true;
        
	}
	public void GetUp(){
		if (fHelper.airborne)
						return;
		fHelper.UnTranslate();
		float getupScl = 3.0f - stats.damage/stats.defence;
		if (getupScl < 0.2f)
			getupScl = 0.2f;

		fHelper.actionTmr.SetTimer(fHelper.Animate("GetUp", false, 0));
		fHelper.animSpeed = getupScl;
		stats.tumble.tmr.ResetTimer();
		stats.flags.aBusy = false;
		stats.flags.mBusy = false;
		fHelper.animLoopStart=0;
		 state = IDLE;
    }
	public void EdgeGrab(float rollDist){
		//no grab while tumble in effect

		if(!InState( EDGEGRAB)){
			rollDist-=stats.size.x;
			 state= EDGEGRAB;
			stats.jump.airJumps=0;
			stats.motion.vel.x = stats.size.x;
			float xDir = fHelper.IntFacingRight()*stats.size.x;
			stats.motion.vel.x = xDir;
			stats.motion.vel.y=-1.0f;
			fHelper.Animate("LedgeGrab", false, 0);
			fHelper.nextAnim="LedgeHang";
			
			stats.edgegrab.edgeTmr.SetTimer();
			fHelper.ReTranslate(new SPoint(fHelper.IntFacingRight()*stats.size.x, stats.size.y));
			stats.edgegrab.flag =true;
			stats.edgegrab.rollMax=rollDist;
		}
	}

	public float Left(){
		return stats.motion.pos.x-stats.size.x/2;
	}
	
	public float Right(){
		return stats.motion.pos.x+stats.size.x/2;
	}
	
	public float Top(){
		return stats.motion.pos.y+stats.size.y;
	}
	
	public float Bottom(){
		return stats.motion.pos.y;
	}

	public bool CreateColBox (){
		if (debugHitbox == null) {
			debugHitbox = new GameObject ();
			debugHitbox.name = "hb"+stats.id.num;
			debugHitbox.AddComponent<MeshFilter>();
			debugHitbox.AddComponent<MeshRenderer>();
			debugHitbox.GetComponent<MeshRenderer>().material = hitboxMat;
			HitBox cfHB = new HitBox();
			cfHB.AllocateFirstIndices(1);
			cfHB.AllocateSecondIndices(0,4);
			SPoint[]  cmbHB=GetHitBox();
			for(int i=0;i<4;i++){
				cfHB.SetVertex(cmbHB[i].x-GetPos().x*2, cmbHB[i].y-GetPos().y*2, 0, i);
			}
			cfHB.Render("HitBox", GetPos(), stats.id.num);
			MeshFilter mf = debugHitbox.GetComponent<MeshFilter>();
			mf.mesh = cfHB.mesh;
			DontDestroyOnLoad(debugHitbox);
			debugHitbox.transform.position = new Vector3(stats.motion.pos.x, stats.motion.pos.y, 0);
            return true;
        } else
            return false;
    }

	public virtual bool  RenderHitbox(){

		//Destroy (debugHitbox);
		/*if (debugHitbox == null) {
			debugHitbox = new GameObject ();
			debugHitbox.name = "hb"+stats.id.num;
			debugHitbox.AddComponent<MeshFilter>();
			debugHitbox.AddComponent<MeshRenderer>();
			debugHitbox.GetComponent<MeshRenderer>().material = hitboxMat;
		}
		MeshFilter mf = debugHitbox.GetComponent<MeshFilter>();*/
		//mf.mesh.Clear ();
 	
		//MeshRenderer mra =  attkHitbox.AddComponent<MeshRenderer>();
		/*if (edgeGrbBox == null) {
			edgeGrbBox = new GameObject ();
			edgeGrbBox.name = "eGrb"+stats.id.num;
			edgeGrbBox.AddComponent<MeshFilter>();
			edgeGrbBox.AddComponent<MeshRenderer>();
			edgeGrbBox.GetComponent<MeshRenderer>().material = hitboxMat;
		}
	
	
		MeshFilter mfe =	edgeGrbBox.GetComponent<MeshFilter>();
	

		 HitBox cfHB = new HitBox();
		cfHB.AllocateFirstIndices(1);
		cfHB.AllocateSecondIndices(0,6);
		SPoint[]  cmbHB=GetCurrentColBox();
		for(int i=0;i<6;i++){
			cfHB.SetVertex(cmbHB[i].x-GetPos().x, cmbHB[i].y-GetPos().y, 0, i);
		}
		dmgDebugFlag=false;
		//stringstream mn;
		fHelper.grabBox.Render("GrabBox", GetPos(), stats.id.num);
		if (fHelper.grabBox.isActive) {
			mfe.mesh=fHelper.grabBox.mesh;
		}*/

		if ((state == ATTK) || (state == AIRATK)) {
			
						if (attkHitbox == null) {
								attkHitbox = new GameObject ();
								attkHitbox.name = "atk" + stats.id.num;
								attkHitbox.AddComponent<MeshFilter> ();
								attkHitbox.AddComponent<MeshRenderer> ();

				attkHitbox.GetComponent<MeshRenderer> ().materials = new Material[] {attkBoxMat, attkBoxMat,attkBoxMat};
				attkHitbox.GetComponent<MeshRenderer> ().material.color=new Color(1.0f,1.0f,1.0f);
						}

			MeshFilter mfa = attkHitbox.GetComponent<MeshFilter> ();
				int fadeBox=-1;
			if (fHelper.seqInd > 0){
					attackBox.RenderSequence ("AttkBox", GetPos (), fHelper.atkType, fHelper.frame, stats.id.num, fHelper.IsFacingRight (), atkTmr.GetLen (), fHelper.seqInd);
			 
			}
			else
				fadeBox=attackBox.Render ("AttkBox", GetPos (), fHelper.atkType, fHelper.frame, stats.id.num, fHelper.IsFacingRight (), atkTmr.GetLen ());
			if (attackBox.mesh != null) {
				MeshRenderer mra = attkHitbox.GetComponent<MeshRenderer> ();

								mfa.mesh = attackBox.mesh;	
				if((fadeBox>=0)&&(fadeBox<3)){
					if((fadeBox==0)&&(mra.material.color.b!=0.4f)){
						mra.material.color=new Color(0.2f, 0.2f, 0.4f);
						mra.materials[fadeBox].color=new Color(0.2f, 0.2f, 0.4f);}
				}


						}

				} else if ((attkHitbox != null) && (debugFadeTmr.IsReady ()))
						debugFadeTmr.SetTimer ();
				
				
		if ( state ==  GUARD)
			RenderPlrGuard (stats.id.num, GetPos (), stats.guard.dir, stats.guard.arc);
		else if (guardDisplay != null) 
			Destroy (guardDisplay);	
		

		/*
		if(dmgDebugFlag)
			hitbox.Render( "IsHit", GetPos(), stats.id.num);
		else if(!stats.flags.invuln){//No draw when invulnerable
			cfHB.Render("HitBox", GetPos(), stats.id.num);
			//hitbox.Render(eng, (LPCSTR)"HitBox", GetPos(), stats.id.num);
		}


		bool flag = false;

				}

for (int i=0; i<PROJ_LMT; i++) {
		mn << CharName() << "-PROJ" << i;
		eng->DestroyManualObject(mn.str().c_str());
		if(projectile[i].active)
			eng->DrawTriangleFan(projectile[i].v, (LPCSTR)"AttkBox" , mn.str().c_str(), 0,projectile[i].GetVNum()-1, projectile[i].pos); 
			
	}
		mf.mesh = cfHB.mesh;
		*/
		int fbvTot = 8;
		Vector3[] fbVertices = new Vector3[8];
		string pName = "";
		for (int i=0; i<PROJ_LMT; i++) {
			if(projectile[i].active){
				pName = "PROJ"  + stats.id.num +"n"+i;
				for(int j=0;j<8;j++){
					fbVertices [j] = new Vector3 (projectile[i].v[j].x, projectile[i].v[j].y, 0);
				}

				int fbtTot = (fbvTot - 2) * 3;
				int[] fbTri = new int[fbtTot];
				for (int j = 0; j<(fbtTot/3);j++) {//triangle fan
					fbTri[j*3]=0;
                    fbTri[(j*3)+1]=j+1;
                    fbTri[(j*3)+2]=j+2;
				}
				if (projectile[i].dbgBox == null) {
					projectile[i].dbgBox = new GameObject ();
					projectile[i].dbgBox.name = pName;
					projectile[i].dbgBox.AddComponent<MeshFilter> ();
					projectile[i].dbgBox.AddComponent<MeshRenderer> ();
					projectile[i].dbgBox.GetComponent<MeshRenderer> ().material = attkBoxMat;

					Mesh mesh = new Mesh();
					mesh.vertices=fbVertices;
					mesh.triangles = fbTri;
                    
					projectile[i].dbgBox.GetComponent<MeshFilter> ().mesh = mesh;
				}else{
					projectile[i].dbgBox.transform.position=new Vector3(projectile[i].pos.x, projectile[i].pos.y, 0);
				}

			}else if (projectile[i].dbgBox != null){
				Destroy (projectile[i].dbgBox);

			}
        }
		return true;
	}
	public void RenderPlrGuard(int pNum, SPoint plPos, float dir, float arc){
				//renders a visual representing the player's guard
		

        //Destroy (guardDisplay);
		if (guardDisplay == null) {
			guardDisplay = new GameObject ();
			guardDisplay.name = "grd"+stats.id.num;
			guardDisplay.AddComponent<MeshFilter>();
			guardDisplay.AddComponent<MeshRenderer>();
			guardDisplay.GetComponent<MeshRenderer> ().material = attkBoxMat;
		}

		MeshFilter mf =	guardDisplay.GetComponent<MeshFilter> ();
  


		float accuracy = 35; //size of steps
		float radius = 15;//how wide to draw
		int vTot = (int)(2 * arc / (Mathf.PI/accuracy))+2;
		Vector3[] vertices = new Vector3[vTot];
		Color32[] col = new Color32[vTot];
		int SPoint_index=0;
		for (float theta = dir-arc; theta <= dir+arc; theta += Mathf.PI / accuracy) {
			vertices[SPoint_index]= new Vector3(plPos.x + radius * Mathf.Cos(theta), plPos.y + radius * Mathf.Sin(theta)+stats.size.y/2, 0);
			SPoint_index++;
		}
		vertices [SPoint_index] = new Vector3 (plPos.x , plPos.y+stats.size.y/2, 0);
		int tTot = (vTot - 2) * 3;
		int[] tri = new int[tTot];
		for (int i = 0; i<(tTot/3);i++) {//triangle fan
			tri[i*3]=0;
			tri[(i*3)+1]=i+1;
			tri[(i*3)+2]=i+2;
		}

		Mesh mesh = new Mesh();
		mesh.vertices=vertices;
		mesh.triangles = tri;
		mesh.colors32 = col;
		mf.mesh = mesh;

		/*
		std::stringstream hbName;
		hbName << "P#" << pNum << ":GUARD";
		if(mSceneMgr->hasManualObject(hbName.str()))
			mSceneMgr->destroyManualObject(hbName.str());
		Ogre::ManualObject* guard = mSceneMgr->createManualObject(hbName.str());
		guard->begin("GuardBox", Ogre::RenderOperation::OT_TRIANGLE_FAN);
		int SPoint_index=0;
		for (float theta = dir-arc; theta <= dir+arc; theta += M_PI / accuracy) {
			guard->position(plPos.x + radius * cos(theta), plPos.y
			                + radius * sin(theta), 0);
			guard->index(SPoint_index++);
		}
		guard->position(plPos.x, plPos.y, 0);
		guard->index(SPoint_index++);
		guard->index(0); // Rejoins the last SPoint to the first.
		guard->end();
		hitBounds->attachObject(guard);
		return hr;*/
		}
	public void Move(SPoint p){
		stats.motion.move.x+=p.x;
		stats.motion.move.y+=p.y;
	}
	
	public void SetStunMult(float sMult){
		stunMult=sMult;
	}
	public void SetPhysVars (float t_vRT, float t_vRoT, float t_bR, float m_s, float DI_s, float DI_r){

		tu_vReThresh = t_vRT;
		tu_vRoThresh = t_vRoT;
		tu_bncRat = t_bR;
		magScale = m_s;
		scaleDI = DI_s;
		reduceDI = DI_r;
	}
	public void FighterTranslate(SPoint t){
		transform.Translate (t.x, t.y, 0, Space.World);
		
		if (debugHitbox != null) {
			debugHitbox.transform.Translate (t.x, t.y, 0, Space.World);		
		}

	}

}

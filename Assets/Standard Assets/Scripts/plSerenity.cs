				using UnityEngine;
			using System.Collections;

public class plSerenity : Fighter {

	public ParticleSystem ptlPShot;

	const float PS_SCALE = 1.2f;
	//public  string SCRIPT_FILE = "Assets/Standard Assets/Fighters/Serenity/serenity.script";
	//const string MESH_FILE = "..\\..\\characters\\Serenity\\serenity.mesh";
	//const string PSYSWORD_MESH_FILE=  "..\\..\\characters\\Serenity\\psysword.mesh";
	//const string PSYSWORD_ANIMHELP = "..\\..\\characters\\Serenity\\psyanim.mesh";
	//public const string STATS_FILE = "Assets/Standard Assets/Fighters/Serenity/serenity.stats";
	//public const string MOVESCRIPT = "Assets/Standard Assets/Fighters/Serenity/serenitymove.txt";
	//public const string MOVE_FILE = "Assets/Standard Assets/Fighters/Serenity/serenity.movescript";

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
	const int S_NAIR = 24;
	const int S_FAIR = 25;
	const int S_BAIR = 26;
	const int S_DAIR = 27;
	const int S_UAIR = 28;
 
	
	const int SNAIR = 24;
	const int SFAIR = 25;
	const int SBAIR = 26;
	const int SDAIR = 27;
	const int SUAIR = 28;
	
	
	const int SER_NUMANIMS = 30;

	plSerenSword psysword;
	SPoint plPsyPos;
	STimer scTmr;
	
	//spec move timers
	STimer spaTmr;
	STimer spb1; 
	STimer spa1;
	STimer spc1;
	STimer spc2;
	STimer spc3;
	float scSpd;

	// Use this for initialization
	public void Awake(){
		PROJ_LMT = 0;
		stunTimer = new STimer ();
		thwTmr = new STimer (throwTime);
		grbTmr = new STimer (grabTime);
        scTmr = new STimer ();
		spa1 = new STimer ();
		spb1 = new STimer ();
		spc1 = new STimer ();
		spc2 = new STimer ();
		spc3 = new STimer ();
		spaTmr = new STimer (1.5f);
		hitbox = new HitBox();
		fHelper = new FighterHelper ();

		stats.id.fighter = SERENITY;
		attackBox=new AttkBox(SER_NUMANIMS);
		stats.tumble.loop=true;
		//psysword=plSerenSword(plNum);
		scSpd=0;

		
		//for(int i=0;i<PROJ_LMT;i++){
		//	projectile[i].SetAtkData(12.0f, 1.6f, 2.0f);
		//	projectile[i].Scale(5.0f);
		//}

		scSpd=0;

		GameObject goSword = Instantiate(Resources.Load("SerenSword")) as GameObject;
		DontDestroyOnLoad(goSword);

		psysword = goSword.GetComponent ("plSerenSword") as plSerenSword;
		psysword.fHelper.TR.Rotate (0, -90, 0);
		psysword.fHelper.FaceRight (true);
		LoadPlayer ();
		psysword.transform.position = new Vector3 (transform.position.x + 10, transform.position.y + 10, 0);
		psysword.stats.motion.pos = new SPoint (transform.position.x + 10, transform.position.y + 10);
		psysword.plNum=11;


    }
	
	// Update is called once per frame
	void Update () {
     
	 
		if (Time.deltaTime != 0) {
			if(debugModeOn) 
				psysword.debugModeOn = debugModeOn;

            gCont.FrameUpdate (plNum);
			float timeLapsed = Time.deltaTime;
			int ifr = fHelper.IntFacingRight ();
			spaTmr.RunTimer(timeLapsed);
			SPoint swordTarget = new SPoint ();
			swordTarget.y = GetPos ().y + stats.size.y / 2 + psysword.hovHgt;
			if (psysword.atkMode > 0)
					swordTarget = new SPoint (GetPos ().x, GetPos ().y+stats.size.y/2);
			else {
					if (fHelper.IsFacingRight ())
							swordTarget.x = GetPos ().x - psysword.hovDst;
					else
							swordTarget.x = GetPos ().x + psysword.hovDst;
			}
			psysword.swordTarget = swordTarget;
			if (gCont.Held (GameController.R_BUTTON)||gCont.Held (GameController.L_BUTTON)){
				psysword.spdLmt = 3;
				SPoint distVec = new SPoint(psysword.GetPos().x-GetPos().x, psysword.GetPos().y-GetPos().y);
				float distF=distVec.x*distVec.x + distVec.y*distVec.y;
				if(distF<520){
					psysword.SetPos(swordTarget);
				}

            }       
			else
					psysword.spdLmt = 0.5f;

			if (InAttack (SC)) {
					stats.walk.gndSpeed = scSpd * ifr;
					scTmr.RunTimer (timeLapsed);
			}
			//psysword.Update(timeLapsed,swordTarget);


			//spec move handling
			//B move
			if (spb1.RunTimer (timeLapsed)) {
					stats.motion.vel.y = 4;
					fHelper.airborne = true;//must leave the ground
					recUsed = 1;
			} 
			if (InAttack (SPA)){
			if (spa1.RunTimer (timeLapsed)) {
					//stats.motion.pos.x = psysword.stats.motion.pos.x;
					//stats.motion.pos.y = psysword.stats.motion.pos.y;
				    SetPos (psysword.stats.motion.pos);
				    fHelper.airborne = true;//must leave the ground
					fHelper.Animate ("SPAout", false, 0);
					atkTmr.SetTimer (fHelper.aniTmr.GetLen ());
					recUsed = 1;
			     }
				if(!spa1.IsReady()){
				stats.motion.vel.y = 0;
					stats.motion.vel.x = 0;}
			}
			if (!spc1.IsReady ()) {//active
					if (spc1.RunTimer (timeLapsed))
							psysword.transform.rotation = Quaternion.identity;
					else if (spc2.RunTimer (timeLapsed)) {
							psysword.fHelper.WeightedAnim (0.6f, "SPC");
							psysword.attackBox.ResetHits (SPC);
							psysword.Spin (1);
							spc3.SetTimer (0.1f);
					} else if (spc3.RunTimer (timeLapsed)) {
							spc2.SetTimer (0.1f);
					psysword.fHelper.WeightedAnim (0.1f, "SPC");
					}
			}
			FrameUpdate ();
			//PostUpdate ();
			//RenderHitbox ();
		}
	}
	//
    
	public override void ReloadStats(){
		stats.LoadStats (STATS_FILE);
    }
    void plSerenityeplSerenity(int plNum){
		/*stats.id.fighter = SERENITY;
		stats.grav = 0.032f;
		stats.fric = 0.02f;
		stats.jump.vel = 3.0;
		stats.jump.airJumpVel=2.2f;
		stats.walk.vel = 0.07f;
		stats.size.x = 7.1f;
		stats.size.y = 21.4f;
		stats.edgegrab.climbDist=20;
		stats.edgegrab.wid = 9;
		stats.edgegrab.hgt = 6; 
		stats.edgegrab.climbDist=8;
		stats.walk.maxSpeed = 1.6;
		stats.walk.loopStart=(49.0/60);
		stats.walk.shuffle=STimer(0.6);
		attackBox=c_AttkBox(SER_NUMANIMS);
		stats.tumble.loop=true;
		psysword=plSerenSword(plNum);
		scSpd=0;
		
		//spec move timers
		*/
	}

	/*void LoadMoveScript(string s){
		
		int endL, enPos;
		float dVal;
	std:string par, val, line;
		ifstream infile;//get the polygon count for the arrays
		infile.open(s);
		if (infile.is_open()){
			while ( infile.good() ){
				getline (infile,line);
				endL = line.length()-1;
				enPos = line.find_first_of("=");
				par = line.substr(0, enPos);
				val = line.substr(enPos+1, line.length());
				dVal = atof(val.c_str());
				if(par.compare("scSpeed") == 0)
					scSpd=dVal;
			}
		}
		
		
	}*/
	bool LoadPlayer(){
		FindPNum ();
 
		attackBox = new AttkBox ();
		hitbox = new HitBox();

		hitbox.AllocateFirstIndices(1);//for a 1 dimensional array of vertices
		hitbox.AllocateSecondIndices(0, 4);
		hitbox.SetVertex(-stats.size.x/2, 0, 0, 3);
		hitbox.SetVertex(stats.size.x/2, 0, 0, 2);
		hitbox.SetVertex(stats.size.x/2, stats.size.y, 0, 1);
		hitbox.SetVertex(-stats.size.x/2, stats.size.y, 0, 0);
		fHelper.grabBox = new HitBox();
		fHelper.grabBox.AllocateFirstIndices(1);
		fHelper.grabBox.AllocateSecondIndices(0, 4);
		fHelper.grabBox.SetVertex(-stats.size.x/2, stats.size.y + stats.edgegrab.hgt, 0, 0);
		fHelper.grabBox.SetVertex(-stats.size.x/2, stats.size.y, 0, 3);
		fHelper.grabBox.SetVertex(-stats.size.x/2 - stats.edgegrab.wid, stats.size.y, 0, 2);
		fHelper.grabBox.SetVertex(-stats.size.x/2 - stats.edgegrab.wid, stats.size.y + stats.edgegrab.hgt, 0, 16);
		fHelper.grabBox.isActive = false;
		fHelper.sScale=1;
		//fHelper.Load(eng, MESH_FILE, GetPos());
		stats.LoadStats(STATS_FILE);


		


		attackBox.LoadFromScript(SCRIPT_FILE, fHelper.sScale, stats.size.y);
		
		
		stats.id.num = plNum;//todo not hard code
		SetPlayer ();
		transform.Rotate (0, 90.0f, 0);
		fHelper.Animate ("Idle", true, 0);	
		LoadMoveScript (MOVE_FILE);
		 
		//		
		
		//psysword.Load( PSYSWORD_MESH_FILE, GetPos());//load sword model
		//psysword.LoadAnimHolder( PSYSWORD_ANIMHELP, GetPos());//load serenity's anims for reference
		psysword.attackBox.LoadFromScript(SCRIPT_FILE, fHelper.sScale, stats.size.y);
		SPoint bT = new SPoint(0, -psysword.stats.size.y);
		
		psysword.attackBox.TranslateAttack(bT, NB);
		psysword.attackBox.TranslateAttack(bT, SB);
		psysword.attackBox.TranslateAttack(bT, UB);
		psysword.attackBox.TranslateAttack(bT, DB);
		

		//LoadMoveScript(MOVESCRIPT);
		return true;
	}
	/*HRESULT plSerenity::RenderEffects(GrEngine* eng){
		HRESULT hr =S_OK;
		hr=psysword.RenderEffects(eng);
		hr=c_Player::RenderEffects(eng);
		
		return hr;
	}*/
	Fighter GetFamiliar(){
		return psysword;
		
	}public override bool RenderHitbox(){
				SPoint rePos = new SPoint (2.0f, -11.0f);
				SPoint sOrigin = new SPoint (0, 0);
				if ((!psysword.atkTmr.IsReady ()) && ((is_B (psysword.atkMode) || (psysword.atkMode == SPC)))) {
						sOrigin.x = psysword.GetPos ().x;
						sOrigin.y = psysword.GetPos ().y;

          //  psysword.attackBox.Render ("AttkBox", sOrigin, psysword.atkMode, psysword.fHelper.frame, stats.id.num * 11, fHelper.IsFacingRight (), atkTmr.GetLen());
//			MeshFilter mfa = attkHitbox.GetComponent<MeshFilter> ();
			} else
				rePos = new SPoint (0, -11.0f);
				
		//	psysword.stats.motion.pos.x += rePos.x;
		//psysword.stats.motion.pos.y += rePos.y;
		//psysword.RenderHitbox ();
		//psysword.stats.motion.pos.x -= rePos.x;
		//psysword.stats.motion.pos.y -= rePos.y;
		base.RenderHitbox ();
		return true;
		}
	void S_Aerial(){
		int atk = 0;
		string name = "";
		if(gCont.Held(UP)){
			atk=SUAIR;
			name="SUAIR";
		}else if(gCont.Held(DOWN)){
			atk=SDAIR;
			name="SDAIR";
		}else if((fHelper.IsFacingRight()&&gCont.Held(RIGHT))||(!fHelper.IsFacingRight()&&gCont.Held(LEFT))){
			atk=SFAIR;
			name="SFAIR";
		}else if((fHelper.IsFacingRight()&&gCont.Held(LEFT))||(!fHelper.IsFacingRight()&&gCont.Held(RIGHT))){
			atk=SBAIR;
			name="SBAIR";
		}else{
			atk=SNAIR;
			name="SNAIR";
		}
		StartAttack(atk, name);	
		attackBox.ResetHits(atk);
	}
	protected override void SpecA(){
		string name;
		if((!spaTmr.IsReady())||(InState( GUARD)))
			return;//no moves when guarding
		else{
			if(AttackCheck(SPA, "SPA")){//if attack possible
				atkTmr.SetTimer(fHelper.aniTmr.GetLen());
				spaTmr.SetTimer(1.5f);
				attackBox.ResetHits(SPA);
				spa1.SetTimer(fHelper.aniTmr.GetLen());
			}
		}
	}
	protected override void SpecB(){
		string name;
		if (recUsed > 0)
			return;
		if(moveCont[SPB]!=null)
			moveCont[SPB].StartAttack();
		if((InState( GUARD))&&(recUsed>0))//only use recovery 1 time before landing
			return;//no moves when guarding
		else{
			if(AttackCheck(SPB, "SPB")){//if attack possible
				atkTmr.SetTimer(fHelper.aniTmr.GetLen());
				psysword.StartAttack(SPB, "SPB");
				attackBox.ResetHits(SPB);
				spb1.SetTimer(0.3f);
			}
		}
	}
	protected override void SpecC(){
		string name;
		if(InState( GUARD))
			return;//no moves when guarding
		else{
			if(AttackCheck(SPC, "SPC")){//if attack possible
				//atkTmr.SetTimer(fHelper.aniTmr.Length());
				psysword.StartAttack(SPC, "SPC");
				psysword.attackBox.ResetHits(SPC);
				psysword.atkTmr.SetTimer(4);
				spc1.SetTimer(4);
				spc2.SetTimer(0.1f);
			}
		}
	}
	void B(){
		int atk;
		string name;
		if(InState( GUARD))
			return;//no moves when guarding
		else if (PerAttk ())
			return;
        else if((fHelper.airborne)&&(psysword.IsSwordAirReady()))
			S_Aerial();
		else{
			if(gCont.Held(UP)){
				atk=UB;
				name="UB";
			}else if(gCont.Held(DOWN)){
				atk=DB;
				name="DB";
			}else if((gCont.Held(LEFT))||(gCont.Held(RIGHT))){
				atk=SB;
				name="SB";
			}else{
				atk=NB;
				name="NB";
			}	
			StartAttack(atk, name);
			attackBox.ResetHits(atk);
		}
	}
	public override void StartAttack(int aI, string aS){
		//override to customize attacks
		bool ifr = fHelper.IsFacingRight();
		int iIfr = 1;
	
		if(!ifr)
			iIfr=-1;
		if((is_B(aI))&&(psysword.fHelper.atkType!=SPC)){//sword aerials
			if(!fHelper.airborne){
				if(psysword.atkMode==0)
					psysword.FaceRightIm (fHelper.IsFacingRight());
				if(aI==NB)
					psysword.StartAttack(S_NB, aS);
				else if(aI==SB)
					psysword.StartAttack(S_SB, 	aS);
				else if(aI==DB)
					psysword.StartAttack(S_DB, aS);
				else if(aI==UB)
					psysword.StartAttack(S_UB, aS);


			}
		}//particle effects for a attacks
		 

		if(aI==SPB)
			psysword.StartAttack(SPB, "SPB");
		else if(aI==NC)
			psysword.StartAttack(S_NC, aS);
		else if(aI==SC)
			psysword.StartAttack(S_SC, aS);
		else if(aI==DC)
			psysword.StartAttack(S_DC, aS);
		else if(aI==UC)
			psysword.StartAttack(S_UC, aS);
		else if(aI==S_NAIR)
			psysword.StartAttack(S_NAIR, aS);
		else if(aI==SFAIR)
			psysword.StartAttack(S_FAIR, aS);
		else if(aI==SBAIR)
			psysword.StartAttack(S_BAIR, aS);
		else if(aI==SDAIR)
			psysword.StartAttack(S_DAIR, aS);
		else if(aI==SUAIR)
			psysword.StartAttack(S_UAIR, aS);
		else if(aI==NA){
			ptlPShot.Emit (9);
		}
		if((!is_C(aI))&&(!is_B(aI)))//

			 base.StartAttack(aI, aS);
		else if(((psysword.atkMode>=S_NC)&&(psysword.atkMode<=S_UC))||(psysword.atkMode==SPB) ){
			if(!fHelper.IsFacingRight())
				psysword.fHelper.Pivot(180);
			if(psysword.curMove==aI){
				if(AttackCheck(aI, aS)){


					atkTmr.SetTimer(fHelper.aniTmr.GetLen());
					attackBox.ResetHits(aI);
					if((atkTmr.IsReady ())&&(aI==SC)){
						scTmr.SetTimer(atkTmr.GetLen());
						}
					}else
					psysword.Idle();
			}
		}

	}
	
	public override void  AttackDetect(Fighter plr){
		 AttkTrack[] aBox=null; 
		int pState;
		SPoint rePos = new SPoint(0,0);
		
		stats.motion.pos.AddTo(rePos, 1);
		base.AttackDetect(plr);
		stats.motion.pos.AddTo(rePos, -1);
		if(psysword.atkMode>0){
			pState=psysword.atkMode;
			if((pState==SPC)||(is_B(pState))){
				psysword.stats.motion.pos.y-=12.0f;
				psysword.AttackDetect(plr);
				psysword.stats.motion.pos.y+=12.0f;
			}
		}
	}
	


	public override void PostUpdate(){
		
		SPoint t = new SPoint ();
		t.x = psysword.GetPos().x - psysword.stats.motion.lastPos.x + psysword.psMove.x - psysword.lastMove.x;
		t.y = psysword.GetPos().y - psysword.stats.motion.lastPos.y + psysword.psMove.y - psysword.lastMove.y;
		//psysword.Translate(t);
		//psysword.fHelper.Translate(t);
		psysword.stats.motion.lastPos = psysword.GetPos();
		psysword.lastMove.x = psysword.psMove.x;
		psysword.lastMove.y = psysword.psMove.y;
		base.PostUpdate();
	}
	public void SetPlayer(){

		base.SetPlayer ();


    }

	

}

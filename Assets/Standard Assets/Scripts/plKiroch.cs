using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class plKiroch : Fighter {

 

	//STimer scTmr;
	float chargeT, scSpd, sbSpd;
	public float chLv, chMx;
	const float SPC_MOVER = 1.2f;
	//public const string SCRIPT_FILE = "Assets/Standard Assets/Fighters/Kiroch/kiroch.script";
	//public const string STATS_FILE="Assets/Standard Assets/Fighters/Kiroch/kiroch.stats";
	//public const string MOVE_FILE="Assets/Standard Assets/Fighters/Kiroch/kiroch.movescript";
	public const float PROJ_GRAV = 0.5f;

	protected const int SPB_L = 0;
	protected const int SPB_M = 1;
	protected const int SPB_H = 2;
	protected const int SPC_L = 3;
	protected const int SPC_M = 4;
	protected const int SPC_H = 5;
	protected int specInd=-1;
	//requires 3 elements!v
	public ParticleSystem[] fireBall;
	public float[] aRad;
	public float[] aSpd;
	public float[] aDam;
	public float[] aMag;
	//requires 3 elements!^
	const int KIR_NUMANIMS = 29;
	protected MoveInstruct[] specInst = new MoveInstruct[8];

	public void Awake(){
		PROJ_LMT = 3;	
		chLv = 2.0f;
		chMx = 4.0f;
		stunTimer = new STimer ();
		thwTmr = new STimer (throwTime);
		grbTmr = new STimer (grabTime);
		attackBox=new AttkBox(KIR_NUMANIMS);
		hitbox = new HitBox();

		LoadPlayer ();
		stats.id.fighter = KIROCH;

		chargeT=0;
		if (projectile == null)
			projectile = new plProjectile[PROJ_LMT];
		for (int i=0; i<PROJ_LMT; i++)
			projectile [i]=new plProjectile();
		for(int i=0;i<PROJ_LMT;i++){
			projectile[i].SetAtkData(12.0f, 1.6f, 2.0f);
			projectile[i].Scale(5.0f);
			projectile[i].ps = fireBall[i];
		}
		sbSpd=0;
		scSpd=0;
		
	}
	 
	public override void ReloadStats(){
		stats.LoadStats (STATS_FILE);
	}
	public override void LoadPlayer(){
			
			//pPtcl.LoadParticles(stats.id.num, "Kiroch");
		base.LoadPlayer ();
		stats.LoadStats (STATS_FILE);
		attackBox.LoadFromScript (SCRIPT_FILE, fHelper.sScale, stats.size.y);
		LoadMoveScript (MOVE_FILE);

		stats.id.num = plNum;//todo not hard code
		SetPlayer ();
		transform.Rotate (0, 90.0f, 0);
		fHelper.Animate ("Idle", true, 0);	

	}
	protected override void SetUniqueMove(string uName, MoveInstruct uMove){
		if(uName.CompareTo("SPB_L")==0)
			specInst[SPB_L]=uMove;
		if(uName.CompareTo("SPB_M")==0)
			specInst[SPB_M]=uMove;
		if(uName.CompareTo("SPB_H")==0)
            specInst[SPB_H]=uMove;
		if(uName.CompareTo("SPC_L")==0)
			specInst[SPC_L]=uMove;
		if(uName.CompareTo("SPC_M")==0)
			specInst[SPC_M]=uMove;
		if(uName.CompareTo("SPC_H")==0)
            specInst[SPC_H]=uMove;
    }
	public override float GetSpecMeter(){
		return chargeT;
	}
	protected override void SpecA(){
		string name;
		int cLvl;
		if(InState( GUARD))
			return;//no moves when guarding
		else if(!projectile[0].active){
			if(AttackCheck(SPA, "SPA")){//if attack possible
				cLvl=UseCurrentCharge()-1;
				if(cLvl==2)
					PlayAnimSound (8);
				else if(cLvl==1)
					PlayAnimSound (7);
				else
					PlayAnimSound (6);
				atkTmr.SetTimer(fHelper.aniTmr.GetLen());	
				int ifr=fHelper.IntFacingRight();

				SPoint fFrom = new SPoint(GetPos().x, GetPos().y+stats.size.y/2);
				projectile[cLvl].Scale(aRad[cLvl]);
				//projectile[0].ps=fireBall[cLvl];

				//projectile[cLvl].Fire(fFrom, new SPoint(ifr*aSpd[cLvl], 0));
				FireProjectile(cLvl+1);
				
			}
		}
	}

	public void FireProjectile(int lv){
		//code to spawn projectile here

		GameObject pl1 = Instantiate(Resources.Load("KirochFBall"+lv)) as GameObject;
		plKirochFBall fb = pl1.GetComponent ("plKirochFBall") as plKirochFBall;
		fb.Fire(this, stats.motion.pos, new SPoint(fHelper.IntFacingRight(), 0));
	}
	public int UseCurrentCharge(){
		int cLv;
		if(chargeT<chLv)
			cLv=1;
		else if(chargeT>=chMx)
			cLv=3;
		else
			cLv=2;
		chargeT = 0;//use charge
		return cLv;
	}

	protected override void SpecB(){
		string name;
		int cLv;
		if(InState( GUARD))
			return;//no moves when guarding
		else if(recUsed==0){
			if(chargeT<chLv)
				cLv=1;
			else if(chargeT>=chMx)
				cLv=3;
			else
				cLv=2;
			name="SPB";//+cLv;

			if(AttackCheck(SPB, name, cLv)){//if attack possible
				atkTmr.SetTimer(fHelper.aniTmr.GetLen());	
				attackBox.ResetHits(SPB);
				chargeT=0;

				recUsed=1;
				int sInd =0;
				if(cLv==1)
					sInd =SPB_L;
				else if(cLv==2)
					sInd =SPB_M;
				else if(cLv==3)
					sInd = SPB_H;
				specInd=sInd;
				specInst[sInd].StartAttack();

			}
		}
	}
	protected override void SpecC(){
		string name;
		int cLv;
		if(InState( GUARD))
			return;//no moves when guarding
		else{
			if(AttackCheck(SPC, "SPC")){//if attack possible
				atkTmr.SetTimer(fHelper.aniTmr.GetLen());	
				attackBox.ResetHits(SPC);
				int sInd =0;
				if(chargeT<chLv)
					sInd=SPC_L;
				else if(chargeT>=chMx)
					sInd=SPC_H;
                else
					sInd=SPC_M;
				
				specInd=sInd;
				specInst[sInd].StartAttack();
			}
		}
	}
	protected void  LoadOldMoveScript(string s){
		int endL, enPos;
		float dVal;
	string par, val, line;
		try{
		using (StreamReader infile = new StreamReader(s)) {
				while ((line = infile.ReadLine()) != null) {
					endL = line.Length-1;
					enPos = line.IndexOf("=");
					par = line.Substring(0, enPos);
					val = line.Substring(enPos+1);
					dVal = float.Parse(val);
					if(par.CompareTo("sbSpeed") == 0)
						sbSpd=dVal;
					if(par.CompareTo("scSpeed") == 0)
						scSpd=dVal;
					}
				}
		}catch(FileNotFoundException e){
			return;
		}


		
		
	}
	protected void  Update(){
		if(Time.deltaTime!=0){
			gCont.FrameUpdate (stats.id.num);
			float timeLapsed = Time.deltaTime;
			if (gCont.Held (GameController.R_BUTTON))
					chargeT += timeLapsed;
			else
					chargeT = 0;
			if(chargeT>chMx)
				chargeT=chMx;
			int ifr = fHelper.IntFacingRight ();


			for(int i=0;i<PROJ_LMT;i++)
			if(projectile[i].active){
		//	projectile[i].vel.y-=PROJ_GRAV;
				projectile[i].ps.transform.position=new Vector3(projectile[i].pos.x, projectile[i].pos.y, 0);
				if(!projectile[i].ps.isPlaying)
					projectile[i].ps.Play();
				//projectile[i].ps.Emit((int)(projectile[i].ps.emissionRate*timeLapsed));
				 
			}else if(projectile[i].ps.isPlaying){

				projectile[i].ps.enableEmission=false;
				projectile[i].ps.Stop();
                //projectile[i].ps.transform.position=new Vector3(stats.motion.pos.x,0,0);
			}else{
				projectile[i].ps.enableEmission=false;
				projectile[i].ps.Stop();
			}
			if (InAttack (SPC))
					stats.motion.vel.x = ifr * SPC_MOVER;
            if (InState( ATTK)) {
                if(specInd>=0){
                    specInst[specInd].RunTimers(timeLapsed, fHelper, stats);
                }
			}else
				specInd=-1;
			FrameUpdate ();
			//PostUpdate ();
			//RenderHitbox ();	
		}
	}
	

	
	public override void  StartAttack(int aI, string aS){//redefine to customize attacks
		SPoint moveDist = new SPoint (0, 0);

		if(moveCont[aI]!=null)
			moveCont[aI].StartAttack();
		if ((aI != SC) && (aI != SB)){
				//StartAttack(aI, aS);
				if(AttackCheck(aI, aS)){//if attack possible
					atkTmr.SetTimer(fHelper.aniTmr.GetLen());
					attackBox.ResetHits(aI);
				}
			}else{
			if(AttackCheck(aI, aS)){
				atkTmr.SetTimer(fHelper.aniTmr.GetLen());
				attackBox.ResetHits(aI);
				 
				if(!fHelper.IsFacingRight())
					moveDist.x=-moveDist.x;
				Move(moveDist);
				fHelper.ReTranslate(new SPoint(-moveDist.x, 0));
			}

		}
		if(attackBox.trackList[aI].trackType==AttkBox.FLASH){
			attackBox.flashAtkRdy=true;
			lastFlash=attackBox.trackList[aI].aVtx[0][0].frame;
		}else
            lastFlash=1;
    }
    
}


using UnityEngine;
using System.Collections;
using System;
using System.IO;
public class plYara : Fighter{
	float rage;
	STimer  spb1, spb2, specGrabTmr, specThrowTmr;
	public float specThwTime;



	public const int RAGEMAX = 10;
	public const float RAGEMULT = 0.4f;

	public float sTime;
	STimer thwLockTmr;
    protected STimer aSpecGrabTmr;
	protected STimer aSpecThrowTmr;
	public float airspecGrabTime;
	public float airspecThrowTime;
	public float specGrabTime;
	public float specThrowTime;
	public int aSpecThrowDmg;
	public float aSpecThrowMag;
	public float aSpecThrowDir;
	public int specThrowDmg;
	public float specThrowMag;
	public float specThrowDir;
    //Player grabbedPlr;
	public void Awake(){
		thwLockTmr = new STimer();
		specThrowTmr = new STimer (specThwTime);
		specGrabTmr = new STimer (specGrabTime);
        aSpecThrowTmr = new STimer(airspecThrowTime);
		thwTmr = new STimer (throwTime);
		grbTmr = new STimer (grabTime);
		stunTimer = new STimer ();
		spb1 = new STimer ();
		spb2 = new STimer ();
		aSpecGrabTmr = new STimer ( );

		LoadPlayer();
		stats.id.fighter = YARA;
		rage=0;
		}
	public override void ReloadStats(){
		stats.LoadStats (STATS_FILE);
    }

public override	void LoadPlayer(){
			//pPtcl.LoadParticles(eng, stats.id.num, "Yara");
		base.LoadPlayer ();
        
		//fHelper.Load(eng, MESH_FILE, GetPos());
		stats.LoadStats(STATS_FILE);

		attackBox.LoadFromScript(SCRIPT_FILE, fHelper.sScale, stats.size.y);
		LoadMoveScript (MOVE_FILE);
			
		stats.id.num = plNum;
		SetPlayer ();
		transform.Rotate (0, 90.0f, 0);
		fHelper.Animate ("Idle", true, 0);	
		}

protected override void SpecA(){
	if(InState(GUARD))
		return;//no moves when guarding
	else{
		if(fHelper.airborne){
				if(AttackCheck(GRAB, "aSPA")){//if attack possible
					atkTmr.SetTimer(fHelper.aniTmr.GetLen());
					for(int i=0;i<fighterList.Length;i++){
						AttackDetect (fighterList[i]);
					}
					if(grabbedPlr==null){
						fHelper.Animate ("aSPAf", false, 0);
						atkTmr.ResetTimer();
					}
					else{
					     atkTmr.SetTimer(fHelper.aniTmr.GetLen());	
						aSpecGrabTmr.SetTimer(fHelper.aniTmr.GetLen());	
						fHelper.atkType = ASPA;
					}

					attackBox.ResetHits(SPA);
				}
		}
		else {
		if(AttackCheck(SPA, "SPA")){//if attack possible
			fHelper.TurnAfterAnim();
			atkTmr.SetTimer(fHelper.aniTmr.GetLen());	
				if(moveCont[SPA]!=null)
					moveCont[SPA].StartAttack();
                attackBox.ResetHits(SPA);

			}
		}
	}
}
protected override void SpecB(){
	if(InState(GUARD))
		return;//no moves when guarding
	if (recUsed > 0)
			return;
	else{
		if(AttackCheck(SPB, "Shadow")){//if attack possible
			RevertState();
			atkTmr.SetTimer(fHelper.aniTmr.GetLen());	
			attackBox.ResetHits(SPB);
			spb1.SetTimer(fHelper.aniTmr.GetLen());
		}
	}
}
	protected override void SpecC(){
	if(InState(GUARD))
		return;//no moves when guarding
	else{
		if(AttackCheck(SPC, "SPC")){//if attack possible
				if(moveCont[SPC]!=null)
					moveCont[SPC].StartAttack();
			atkTmr.SetTimer(fHelper.aniTmr.GetLen());	
			attackBox.ResetHits(SPC);

		}
	}
}
	public override float GetSpecMeter(){
		return rage;
	}
	protected void  Update(){
		if (Time.deltaTime != 0) {
			tPortFlag=false;
			gCont.FrameUpdate (stats.id.num);
			float timelapsed = Time.deltaTime;
			int ifr = 1;

			if (!fHelper.IsFacingRight ())
					ifr = -1;
			if (rage > RAGEMAX)
					rage = RAGEMAX;
			if (rage > 0)
					rage -= timelapsed / 2;
			if (rage < 0)
					rage = 0;

			if (InAttack (SPC))
					stats.walk.gndSpeed = ifr * 1;

			if(aSpecGrabTmr.RunTimer(timelapsed)){

					aSpecThrowTmr.SetTimer (fHelper.Animate("aSPAs", false, 0));
				state=ATTK;
				atkTmr.SetTimer(aSpecThrowTmr.GetLen());
				fHelper.actionTmr.SetTimer(aSpecThrowTmr.GetLen());
			}

				if(specThrowTmr.RunTimer(timelapsed)){
					ThrowMotion(2, specThrowDmg, specThrowDir, aSpecThrowMag);
				}
				if(specGrabTmr.RunTimer(timelapsed)){
				specThrowTmr.SetTimer (specThrowTime);
				state=ATTK;
				atkTmr.SetTimer(fHelper.Animate("aSPAs", false, 0));
				fHelper.actionTmr.SetTimer(atkTmr.GetLen());
				thwLockTmr.SetTimer(atkTmr.GetLen());
                }
                
			thwLockTmr.RunTimer(timelapsed);
                //***
            FrameUpdate ();
			if((!aSpecThrowTmr.IsReady())&&(fHelper.airborne==false)){//in this case, landing interrupted a throw
			     //and we want to move to the toss animation immediately
				fHelper.Animate("aSPAs", false, aSpecThrowTmr.GetLen()-aSpecThrowTmr.tmr);
			}
			//***
			if (spb1.RunTimer (timelapsed)) {//From the Darkness teleport
				float dDir = gCont.lStick.GetDir ();
				float dMag = Mathf.Sqrt (gCont.lStick.SqDistFromOrigin ());
				dMag = dMag * 80 + 20;
				SPoint dMove = new SPoint (Mathf.Cos (dDir) * dMag, Mathf.Sin (dDir) * dMag);
				if (gCont.lStick.SqDistFromOrigin () == 0)
					dMove = new SPoint (0, 0);
				stats.motion.pos.x += dMove.x;
				stats.motion.pos.y += dMove.y;
				FaceRightIm (!fHelper.IsFacingRight ());
				fHelper.actionTmr.ResetTimer ();
				fHelper.airborne=true;//set for safety
				AttackCheck (SPB, "SPB");
				atkTmr.SetTimer (fHelper.aniTmr.GetLen ());
				fHelper.grabBox.isActive=false;
				tPortFlag=true;
			} else if (!spb1.IsReady ()) {
				stats.motion.vel.x = 0;
				stats.motion.vel.y = 0;
				
			}else if (InAttack (ASPA)) {
				stats.motion.vel.x = 0;
				stats.motion.vel.y = 0;
				if(!aSpecThrowTmr.IsReady()){
					stats.motion.vel.y = -4;
				}
                
            }
			if(aSpecThrowTmr.RunTimer (timelapsed)){
				ThrowMotion(2, aSpecThrowDmg, aSpecThrowDir, aSpecThrowMag);
			}
			//PostUpdate ();
			//RenderHitbox ();
		}
	}


	protected override void Attack(Fighter plr,int tr, AttkTrack aBox, AttkData data){
		if(fHelper.atkType == SPA){
          
            data.isThrow = true;
			data.throwType=1;

			SpecGrab(plr);
		}else if(fHelper.atkType == UB){
		
			data.isThrow = true;
			data.throwType=1;
			fHelper.Animate("UB2", false, 0);
			SpecGrab(plr);
		}
		base.Attack (plr, tr, aBox, data);
		}
	public void  SpecGrab(Fighter plr){
		if((thwLockTmr.IsReady())&&(specGrabTmr.IsReady())){
			float timeLeft = atkTmr.tmr;
			attackBox.trackList[SPA].hit[0][plr.stats.id.num]=true;
			if(1.2f-timeLeft>0)
				specThrowTmr.SetTimer(1.2f-timeLeft);
			grabbedPlr=plr;
			plr.Grabbed ();
		}
	}

}

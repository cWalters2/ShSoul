using UnityEngine.UI;
using UnityEngine;
using System.Collections;



public class Stage : MonoBehaviour {
	public const float SCALE_FAC=5.0f;
	protected int numObj;
	public int numPlr;
	protected SPoint lBound=new SPoint();
	protected SPoint uBound=new SPoint();
	public SPoint[] startPos = new SPoint[4];
	protected TerrainBox tBox;
	const float EPS = 0.00001f;
	const int STAGE_LIMIT = 1000;
	public Fighter[] player = new Fighter[4];
	public bool debugModeOn;
	public GameObject stageHitbox;
	public Material stageDebugMat;
	public bool matchModeOn=false;
	bool gameWon=false;
	// Use this for initialization
	protected void Start () {
		for (int i=0; i<numPlr; i++)
			if (player [i] == null)
				SetPlStart (player [i]);
	
	}
	
	// Update is called once per frame
	protected void Update () {


		string pName;
		//SPoint vel = player[0].GetVel();
		for (int i=0; i<numPlr; i++) {
			if(player[i]==null){
				GameObject go = new GameObject ();
				pName= "Player" + (i+1);
				go = GameObject.FindGameObjectWithTag (pName);
				player[i]=go.GetComponent<Fighter>();

			}
		}
		if(Time.deltaTime!=0){
		for(int i=0;i<numPlr;i++){
			ColDetect(player[i]);
			player[i].PostUpdate();
			
		}
			if(matchModeOn)
			WinCheck ();
			if(debugModeOn)
				RenderHitbox ();
		}
	}
	void WinCheck(){
		int winner = -1;//-1 for sentinel val
		for(int i=0;i<numPlr;i++)
			if(player[i].stats.stocks>0)
				if(winner==-1)
					winner=i;
		else//more than one player remaining
			winner=-2;//value for no winner yet
		
		if(winner>=0){//winner found and not a debug match
			WinScreen(player[winner]);
			//Render();
			return;}//stop here
	}	
	
	void WinScreen(Fighter wnr){
		if(matchModeOn){//not demo
			if(gameWon==false){

				GameObject wp = GameObject.FindGameObjectWithTag("MenuCursor").GetComponent<PauseMenuDemo>().winPanel;
				wp.SetActive(true);
				GameObject.FindGameObjectWithTag("WinName").GetComponent<Text>().text=wnr.CharName();
				 
				gameWon=true;}
			for(int i=0;i<numPlr;i++){
				player[i].stats.flags.mBusy=true;
				player[i].stats.flags.aBusy=true;
				if(player[i].gCont.Tapped(GameController.START)){
					foreach (GameObject o in Object.FindObjectsOfType<GameObject>()) {
						Destroy(o);}
					Application.LoadLevel ("menu");

				//	player[i].gCont.Pressed(GameController.START)=false;
				}
			}
		}
	}

	bool LoadStage(){
		//this function should be redefined in child classes
		return false;
	}
	public bool ProjectileDetect(Projectile p){
		SPoint pCtr, prV;
		bool pHitFlag=false;
		for (int i = 0; i < tBox.iLength; i++) {
			float[] prAngs = new float[tBox.GetJlength (i)];
			pCtr = new SPoint (p.pos.x, p.pos.y);
			int pJl = tBox.GetJlength (i) - 1;
			float lastAng = Mathf.Atan2 ((tBox.pBounds [i] [pJl].y - pCtr.y), (tBox.pBounds [i] [pJl].x - pCtr.x));
			pHitFlag = true;
			if(i==4)
				pHitFlag=true;
			for (int j = 0; j < tBox.GetJlength(i); j++) {
				prV = new SPoint (tBox.pBounds [i] [j].x - pCtr.x, tBox.pBounds [i] [j].y - pCtr.y);
				prAngs [j] = Mathf.Atan2 (prV.y, prV.x);
				if ((lastAng > prAngs [j]) && (lastAng -prAngs [j]  < Mathf.PI )){
					pHitFlag = false;
					 
				}
				if ((lastAng < prAngs [j]) && (lastAng -prAngs [j]  < - Mathf.PI )){
					pHitFlag = false;
                    
                }
                
				lastAng = prAngs [j];
			}
			if (pHitFlag){
				p.Detonate ();
			}
			
		}
		return pHitFlag;
	}
	protected bool ColDetect(Fighter plr){

		if (gameWon) {
	
		if(Input.anyKey){
				Application.LoadLevel ("menu");
				foreach (GameObject o in Object.FindObjectsOfType<GameObject>()) {
					Destroy(o);}
				Application.LoadLevel ("menu");
            }
		return false;
		}
		if (plr.InState (Fighter.GRABBED))
						return false;
		/*collision detection using the Separating Axis Theorem
    //more information on how this function works can be found at 
    http://projectgemini.wikidot.com/class-stage*/
		bool hitflag=false;
		//bool pAxisFlag = false;    
		int polyIndex = -1;//to keep record of which polygon caused the collision
		int hitIndex = -1;//to keep record of which side of the polygon caused collision
		SPoint[] plBox, plLast;
		// float plAng[4] = {0, M_PI/2, M_PI, -M_PI/2};//store angles related to player hitbox for efficiency
		bool retFlag=false;//holds the return value
		//this loads plBox[] and plLast[] with the player's hitboc this
		//and last frame respectively
		int hbLen=6;
		SPoint posDiff = new SPoint(plr.GetPos().x-plr.GetLastPos().x, plr.GetPos().y-plr.GetLastPos().y);
		if(posDiff.SqDistFromOrigin()!=0){
			plBox = plr.GetCurrentColBox();
			plLast=plr.GetCurrentColBox();
			for(int i=0;i<hbLen;i++){
				plLast[i].x-=posDiff.x;
				plLast[i].y-=posDiff.y;
			}
		}else{
			plBox = plr.GetHitBox();
			plLast=plr.GetHitBox();
			hbLen=4;
		}
		
		
		SPoint hld;
		float[] plAng = new float[hbLen];
		for(int i=0;i<hbLen-1;i++){
			plAng[i]= new SPoint(plBox[i].x-plBox[i+1].x,plBox[i].y-plBox[i+1].y).GetDir();
		}
		plAng[hbLen-1] = new SPoint(plBox[hbLen-1].x-plBox[0].x,plBox[hbLen-1].y-plBox[0].y).GetDir();
		// FillCollisionBox(plBox, plr->GetPos(), plr->stats.size.y, plr->stats.size.x);
		// FillCollisionBox(plLast, plr->stats.motion.lastPos, plr->stats.size.y, plr->stats.size.x);
		//set up angles here for easy acces along with the hitboxes above.
		
		for(int i = 0; i < tBox.iLength; i++){
			hitflag = true;//assume true, then prove false.
			for(int j = 0; j < tBox.GetJlength(i); j++)
			if(!plr.CheckAxis(tBox.GetSPoint(i, j), tBox.GetAng(i, j), plBox,tBox.pBounds[i], hbLen, tBox.jLength[i])){ //no axis intersection
				hitflag = false;
				j=tBox.GetJlength(i);//exit loop early
			}
			if(hitflag)//possible collision; check against other polygon axise
			for(int j = 0; j < hbLen; j++){
				if(!plr.CheckAxis(plBox[j], plAng[j], plBox, tBox.pBounds[i], hbLen, tBox.jLength[i])){ //no axis intersection
					hitflag = false;
					j=hbLen;//exit loop early
				}
			}
			if(hitflag){//all checks are done, collision is sure
				polyIndex = i;//keep record of the last polygon caused the flag
				i=tBox.iLength;//Collision detected, end loop early
			}

		}

		SPoint prV=new SPoint();
		SPoint pCtr;

				//extra added to detect and destroy any projectiles on-screen
		int cv;
		bool pHitFlag = true;
		/*for (int p=0; p<plr.PROJ_LMT; p++) {
			if (plr.projectile [p].active) {
				for (int i = 0; i < tBox.iLength; i++) {
						float[] prAngs = new float[tBox.GetJlength (i)];
						pCtr = new SPoint (plr.projectile [p].pos.x, plr.projectile [p].pos.y);
						int pJl = tBox.GetJlength (i) - 1;
						float lastAng = Mathf.Atan2 ((tBox.pBounds [i] [pJl].y - pCtr.y), (tBox.pBounds [i] [pJl].x - pCtr.x));
						pHitFlag = true;
						for (int j = 0; j < tBox.GetJlength(i); j++) {
							prV = new SPoint (tBox.pBounds [i] [j].x - pCtr.x, tBox.pBounds [i] [j].y - pCtr.y);
							prAngs [j] = Mathf.Atan2 (prV.y, prV.x);
						if ((lastAng > prAngs [j]) && (lastAng -prAngs [j]  < Mathf.PI )){
								pHitFlag = false;
								if(i==6)
									cv=1;//debug spot
							}
							lastAng = prAngs [j];
						}
						if (pHitFlag)
							plr.projectile [p].Detonate ();

						}
					}
				}*/
		float flAng = 1.55f;
        if(polyIndex!=-1){
			//hitflag = true;//force to true to return
			retFlag=true;
			//see which side has collided
			for(int i = 0; i < tBox.GetJlength(polyIndex); i++){
				if(!plr.CheckAxis(tBox.GetSPoint(polyIndex, i), tBox.GetAng(polyIndex, i), plLast, tBox.pBounds[polyIndex], hbLen, tBox.jLength[polyIndex])){
					hitIndex = i;
					i=tBox.GetJlength(polyIndex);//end loop prematurely
				}
			}
			bool pAxisFlag=false;
			if(hitIndex == -1){
				for(int i  = 0; i < hbLen; i++){//check player angs to find the axis
					if(!plr.CheckAxis(plLast[i], plAng[i], plLast, tBox.pBounds[polyIndex], hbLen, tBox.jLength[polyIndex]) ){
						hitIndex = i;
						pAxisFlag = true; 
					}
				}//do collision detection again for this particular index on the polygon
			}//to get the exit vector

			SPoint axP = new SPoint();        
			if(hitIndex>=0){
				if(!pAxisFlag){//using poly axis
					axP = ExitDist(polyIndex, tBox.GetSPoint(polyIndex, hitIndex), tBox.GetAng(polyIndex, hitIndex), plr, 0);
					flAng=tBox.GetAng(polyIndex, hitIndex);
                                       }
				else{//using player axis
					axP = ExitDist(polyIndex, plBox[hitIndex], plAng[hitIndex], plr, 0);

					flAng=plAng[hitIndex];
				}
			}else
				retFlag=false;
			
			plr.stats.motion.pos.x+=axP.x;
			plr.stats.motion.pos.y+=axP.y;
			//for walking    
			float axAng = Mathf.Atan2(axP.y, axP.x);
			float wallAng = Mathf.Atan2(plr.GetVel().y, plr.GetVel().x); 
			float wallDist = Mathf.Sqrt(Mathf.Pow(plr.GetVel().x, 2) + Mathf.Pow(plr.GetVel().y, 2) );
			wallAng -= axAng;
			
			//separate cases for wall collision
			//depend on whether player is tumbling
			if (!plr.stats.tumble.tmr.IsReady()){
				float rbdVec = tBox.GetAng(polyIndex, hitIndex);
				if(rbdVec<=-Mathf.PI)//keep within proper bounds
					rbdVec+=(2*Mathf.PI);
				if(!plr.Rebound(rbdVec)){
					plr.Land(flAng);
					plr.stats.walk.gndSpeed=wallDist;
					plr.stats.motion.vel.x -= Mathf.Cos(wallAng)*wallDist*Mathf.Cos(axAng);
					plr.stats.motion.vel.y -= Mathf.Cos(wallAng)*wallDist*Mathf.Sin(axAng);

				}
			}else{//non-reounding
				int testInt=1;
				if(plr.GetVel().SqDistFromOrigin() > 400)
					testInt=2;
				if((axAng>0)&&(axAng < Mathf.PI))
					plr.Land(flAng);        
				plr.stats.motion.vel.x -= Mathf.Cos(wallAng)*wallDist*Mathf.Cos(axAng);
				plr.stats.motion.vel.y -= Mathf.Cos(wallAng)*wallDist*Mathf.Sin(axAng);
			}


		}    
		
		if((!plr.fHelper.airborne)&&(!retFlag)&&(!plr.tPortFlag))            
			if(!GroundTrack(plr))//ground tracking done here
				plr.Fall();
		if((!retFlag)&&(plr.fHelper.grabBox.isActive))
			retFlag = GrabDetect(plr);//grab checking done here
		
		if((plr.GetPos().x > uBound.x)||(plr.GetPos().y > uBound.y)||(plr.GetPos().x < lBound.x)||(plr.GetPos().y < lBound.y)){
			if((plr.stats.stocks>0)||(!matchModeOn)){
			SetPlStart(plr);
			plr.stats.stocks--;
			plr.stats.motion.vel = new SPoint(0,0);
			plr.stats.damage=0;}
		}
		if (retFlag)
			ColDetect (plr);
		return retFlag;
	}
	
	public bool GrabDetect(Fighter plr){
		bool hitflag = false;
		//bool indflag = true;
		bool pAxisFlag = false;
		//float hitInd = -1;
		int polyIndex = -1;
		SPoint[] gLast = new SPoint[4];
		SPoint[] gBox = new SPoint[4];
		for (int i=0; i<4; i++)
			gBox[i] = new SPoint ();
		float[] gAng = new float[4];
		SPoint diff= new SPoint();
		float gHgt, gLen;
		SPoint moveVec = new SPoint();
		moveVec.x = 0; //must be initialized
		moveVec.y = 0;
		diff.x = plr.GetPos().x - plr.GetLastPos().x;
		diff.y = plr.GetPos().y - plr.GetLastPos().y;
		
		gHgt = plr.stats.edgegrab.hgt;
		gLen = plr.stats.edgegrab.wid;
		//Boxes filled manually for now
		gBox[0].x = plr.GetPos().x + plr.fHelper.grabBox.GetSPoint(0, 0).x;
		gBox[0].y = plr.GetPos().y + plr.fHelper.grabBox.GetSPoint(0, 0).y;
		gBox[1].x = plr.GetPos().x + plr.fHelper.grabBox.GetSPoint(0, 1).x;
		gBox[1].y = plr.GetPos().y + plr.fHelper.grabBox.GetSPoint(0, 1).y;
		gBox[2].x = plr.GetPos().x + plr.fHelper.grabBox.GetSPoint(0, 2).x;
		gBox[2].y = plr.GetPos().y + plr.fHelper.grabBox.GetSPoint(0, 2).y;
		gBox[3].x = plr.GetPos().x + plr.fHelper.grabBox.GetSPoint(0, 3).x;
		gBox[3].y = plr.GetPos().y + plr.fHelper.grabBox.GetSPoint(0, 3).y;
		for (int i=0; i<4; i++)
			gLast[i] = new SPoint ();
		gLast[0].x = gBox[0].x;
		gLast[0].y = gBox[0].y +gHgt-diff.y;
		gLast[1].x = gBox[1].x;
		gLast[1].y = gBox[1].y +gHgt-diff.y;
		gLast[2].x = gBox[2].x;
		gLast[2].y = gBox[2].y + gHgt-diff.y;
		gLast[3].x = gBox[3].x;
		gLast[3].y = gBox[3].y + gHgt-diff.y;
		int gbLen=4;
		gAng[0] = 0;
		gAng[1] = Mathf.PI/2;
		gAng[2] = Mathf.PI;
		gAng[3] = -Mathf.PI/2;
		
		for(int i = 0; i < tBox.iLength; i++){
			hitflag = true;
			if(!tBox.isPform[i]){
				for(int j = 0; j < tBox.GetJlength(i); j++)
					if(!plr.CheckAxis(tBox.GetSPoint(i, j), tBox.GetAng(i, j), gBox, tBox.pBounds[i], gbLen, tBox.jLength[i])) //no axis intersection
						hitflag = false;
				if(hitflag)
					for(int j = 0; j < 4; j++)
						if(!plr.CheckAxis(gBox[j], gAng[j], gBox, tBox.pBounds[i], gbLen, tBox.jLength[i])) //no axis intersection
							hitflag = false;
			}
			else
				hitflag = false;
			if(hitflag)
				polyIndex = i;
		}
		int hitIndex = -1;
		if(polyIndex >= 0){
			hitflag = true;
			float moveDist = 0;
			//see which side has collided
			for(int i = 0; i < tBox.GetJlength(polyIndex); i++){
				if(!plr.CheckAxis(tBox.GetSPoint(polyIndex, i), tBox.GetAng(polyIndex, i), gLast, tBox.pBounds[polyIndex], gbLen, tBox.jLength[polyIndex])) {
					hitIndex = i;
				}
			}
			if(hitIndex == -1){
				//check grab angs to find the axis
				for(int i  = 0; i < 4; i++){
					if(!plr.CheckAxis(gLast[i], gAng[i], gLast, tBox.pBounds[polyIndex], gbLen, tBox.jLength[polyIndex])){
						hitIndex = i;
						pAxisFlag = true;
					}
				}
			}
			moveVec.x = 0;
			moveVec.y = -plr.stats.edgegrab.hgt;
			if(hitIndex == -1)
				return false;
			SPoint pExit=new SPoint(0,0);
			if(!pAxisFlag)
				pExit=ExitDist(polyIndex, tBox.GetSPoint(polyIndex, hitIndex), tBox.GetAng(polyIndex, hitIndex), plr, 1);
			else
				pExit=ExitDist(polyIndex, gBox[hitIndex], gAng[hitIndex], plr, 1);
			float ledgeAng = Mathf.Atan2(pExit.y, pExit.x);
			if(Mathf.Abs(ledgeAng -Mathf.PI/2)< Mathf.PI/6){//movevec SPoints up within 30 deg
				plr.stats.motion.pos.y = plr.stats.motion.pos.y + pExit.y;
				float wallAng = Mathf.Atan2(plr.stats.motion.vel.y, plr.GetVel ().x); 
				float wallDist = Mathf.Sqrt(Mathf.Pow(plr.GetVel().x, 2) + Mathf.Pow(plr.GetVel().y, 2) );
				float rollDist = tBox.GetBoxWidth(polyIndex);
				wallAng -= ledgeAng;
				plr.stats.motion.vel.x=0;
				plr.stats.motion.vel.y -= Mathf.Cos(wallAng)*wallDist*Mathf.Sin(Mathf.Atan2(pExit.y, pExit.x));
				plr.EdgeGrab(rollDist);
			}
			else
				return false;
		}
		return hitflag;
	}

	public SPoint ExitDist(int iInd, SPoint origin, float dir, Fighter plr, int ifGrab){
		//plBox must contain 4 vertices!!!
		bool isHit = true;
		float rAng, distSq, projVal, moveDist;
		SPoint moveVec= new SPoint();
		moveVec.x = plr.GetPos().x - plr.GetLastPos().x;
		moveVec.y = plr.GetPos().y - plr.GetLastPos().y;
		SPoint[] plBox;
		int hbLen=4;
		if(ifGrab<1)
		if(ifGrab<0){//implies groundCheck
			plBox = plr.GetHitBox();
			hbLen=4;
		}else{
			plBox = plr.GetCurrentColBox();
			hbLen=6;
		}
		else{
			plBox = plr.GetGrabBox();
			moveVec.y-=plr.stats.edgegrab.hgt;
			moveVec.x=0;
			hbLen=4;
		}
		float dHitMinSq = 1000;
		float dHitMaxSq = 0;
		float dPrMinSq  = 1000;
		float dPrMaxSq  = 0;
		SPoint projVec= new SPoint();
		float projMin = STAGE_LIMIT*STAGE_LIMIT;
		float projMax = -STAGE_LIMIT*STAGE_LIMIT;
		float hitMin = STAGE_LIMIT*STAGE_LIMIT;
		float hitMax = -STAGE_LIMIT*STAGE_LIMIT;
		for(int i = 0; i < tBox.GetJlength(iInd); i++){
			projVec.x = tBox.GetSPoint(iInd, i).x - origin.x;
			projVec.y = tBox.GetSPoint(iInd, i).y - origin.y;
			distSq = projVec.x*projVec.x + projVec.y*projVec.y;
			if(distSq < EPS*EPS)
				rAng = dir; //Do NOT take atan2(0, 0)
			else
				rAng = dir - Mathf.Atan2(projVec.y, projVec.x);					
			projVal = Mathf.Sin(rAng)*distSq;
			if(projVal < projMin){
				projMin = projVal;
				dPrMinSq = distSq;}
			if(projVal > projMax){
				projMax = projVal;
				dPrMaxSq=distSq;}
		}
		for(int i = 0; i < hbLen; i++){
			projVec.x = plBox[i].x - origin.x;
			projVec.y = plBox[i].y - origin.y;
			distSq = projVec.x*projVec.x + projVec.y*projVec.y;
			if(distSq < EPS*EPS)
				rAng = dir; //Do NOT take atan2(0, 0)
			else
				rAng = dir - Mathf.Atan2(projVec.y, projVec.x);	
			projVal = Mathf.Sin(rAng)*distSq;
			if(projVal < hitMin){
				hitMin = projVal;
				dHitMinSq=distSq;}
			if(projVal > hitMax){
				hitMax = projVal;
				dHitMaxSq=distSq;}
		}
		if(dPrMinSq!=0)
			projMin=projMin/Mathf.Sqrt(dPrMinSq);
		if(dPrMaxSq!=0)
			projMax=projMax/Mathf.Sqrt(dPrMaxSq);
		if(dHitMinSq!=0)
			hitMin=hitMin/Mathf.Sqrt(dHitMinSq);
		if(dHitMaxSq!=0)
			hitMax=hitMax/Mathf.Sqrt(dHitMaxSq);
		if((hitMax <= projMin)||(projMax <= hitMin)	)
			isHit = false;
		if((Mathf.Abs(projMax - hitMin)< EPS)||(Mathf.Abs(hitMax - projMin)< EPS))
			isHit=false;
		//determine exit vector here
		float moveAng = Mathf.Atan2(moveVec.y, moveVec.x);
		float angDiff = Mathf.Abs(moveAng - (dir - Mathf.PI/2)) ;
		if(angDiff > Mathf.PI)
			angDiff = Mathf.Abs(angDiff - Mathf.PI*2);
		moveDist=0;
		if(isHit){
			if(angDiff < Mathf.PI/2) //moveVector in direction of exit vector
				moveDist = -Mathf.Abs(hitMax - projMin);
			else
				moveDist = Mathf.Abs(projMax - hitMin);
		}
		moveVec.x = moveDist*Mathf.Cos(dir - Mathf.PI/2);
		moveVec.y = moveDist*Mathf.Sin(dir - Mathf.PI/2);
		if(!isHit){
			moveVec.x = 0;
			moveVec.y = 0;
		}

		return moveVec;
	}

	public bool GroundTrack(Fighter plr){
		//specialized collision detection function
		//ensures the fighter keeps flush with the floor	
		bool hitflag = true;
		bool indflag = true;
		bool pAxisFlag = false;
		float hitInd = -1;
		int polyIndex = -1;
		SPoint[] plBox = new SPoint[4];
		SPoint[] plLast = new SPoint[4];
		for(int i=0;i<4;i++){
			plBox[i]=new SPoint();
			plLast[i]=new SPoint();
	}
		float[] plAng = {0, Mathf.PI/2, Mathf.PI, -Mathf.PI/2};
		int hbLen=4;

				//modified from colision detect to move the player down
	
		FillCollisionBox(plBox, new SPoint(plr.GetPos().x, plr.GetPos().y-plr.stats.size.y/2), plr.stats.size.y, plr.stats.size.x);
		FillCollisionBox(plLast, new SPoint(plr.GetPos().x, plr.GetPos().y+plr.stats.size.y/2), plr.stats.size.y, plr.stats.size.x);
		//holds default angles
		
		for(int i = 0; i < tBox.iLength; i++){
			hitflag = true;
			for(int j = 0; j < tBox.GetJlength(i); j++)
				if(!plr.CheckAxis(tBox.GetSPoint(i, j), tBox.GetAng(i, j), plBox,tBox.pBounds[i], hbLen, tBox.jLength[i]) ) //no axis intersection
					hitflag = false;
			if(hitflag)
				for(int j = 0; j < 4; j++)
					if(!plr.CheckAxis(plBox[j], plAng[j], plBox,tBox.pBounds[i], hbLen, tBox.jLength[i]) ) //no axis intersection
						hitflag = false;
			if(hitflag)
				polyIndex = i;
		}
		int hitIndex = -1;
		if(polyIndex >= 0){
			hitflag = true;
			float moveDist = 0;
			for(int i = 0; i < tBox.GetJlength(polyIndex); i++){
				if(!plr.CheckAxis(tBox.GetSPoint(polyIndex, i), tBox.GetAng(polyIndex, i), plLast,tBox.pBounds[polyIndex], hbLen, tBox.jLength[polyIndex])){
					hitIndex = i;
				}
			}
			if(hitIndex == -1){
				//check player angs to find the axis
				for(int i  = 0; i < 4; i++){
					if(!plr.CheckAxis(plLast[i], plAng[i], plLast, tBox.pBounds[polyIndex], hbLen, tBox.jLength[polyIndex]) ){
						hitIndex = i;
						pAxisFlag = true;
					}
				}
			}		
			
			bool floorHit = false;
			SPoint floorVec=new SPoint(0,0);
			plr.stats.motion.pos.y-=plr.stats.size.y/2;
			if(!pAxisFlag)
				floorVec = ExitDist(polyIndex, tBox.GetSPoint(polyIndex, hitIndex), tBox.GetAng(polyIndex, hitIndex), plr, -1);
			else
				floorVec = ExitDist(polyIndex, plBox[hitIndex], plAng[hitIndex], plr, -1);
			float wallAng = Mathf.Atan2(floorVec.y, floorVec.x)-Mathf.PI/2;
			float wallDist=Mathf.Sqrt(floorVec.SqDistFromOrigin());
			if((floorVec.x==0)&&(floorVec.y==0))
				plr.stats.motion.pos.y+=plr.stats.size.y/2;
			else if(Mathf.Cos(wallAng)!=0){
				plr.stats.motion.pos.y+=(wallDist)/Mathf.Cos(wallAng);
				floorHit=true;
			}
			else{
				plr.stats.motion.pos.y+=plr.stats.size.y/2;
				floorHit=true;
			}
			if((floorHit)&&(!plr.fHelper.airborne)){
				plr.Land(tBox.GetAng(polyIndex, hitIndex));
				if(tBox.rLipAr[polyIndex])
					plr.SetRightLip (tBox.pBounds[polyIndex][1].x);
				else
					plr.SetRightLip (0);
				if(tBox.lLipAr[polyIndex])
					plr.SetLeftLip (tBox.pBounds[polyIndex][0].x);
				else
					plr.SetLeftLip (0);
				return true;
			}
		}
		return false;
	}
	void SetPlStart(Fighter plr){
		plr.stats.motion.lastPos.x = startPos[plr.stats.id.num-1].x;
		plr.stats.motion.lastPos.y = startPos[plr.stats.id.num-1].y;
		plr.stats.motion.pos.x = startPos[plr.stats.id.num-1].x;
		plr.stats.motion.pos.y = startPos[plr.stats.id.num-1].y;
		//plr.stats.damage = 150.2f;
		plr.SetPos(startPos[plr.stats.id.num-1]);
		plr.Fall();
	}
	void FillCollisionBox(SPoint[] plBox, SPoint pos, float h, float w){
		plBox[0].x = pos.x - w/2;
		plBox[0].y = pos.y;
		plBox[1].x = pos.x + w/2;
		plBox[1].y = pos.y;
		plBox[2].x = pos.x + w/2;
		plBox[2].y = pos.y + h;
		plBox[3].x = pos.x - w/2;
		plBox[3].y = pos.y + h;
	}
	bool  RenderHitbox( ){
		if (stageHitbox == null) {
			stageHitbox = new GameObject ();
			stageHitbox.name = "stage";
			MeshFilter mf = stageHitbox.AddComponent<MeshFilter> ();
			MeshRenderer mr = stageHitbox.AddComponent<MeshRenderer> ();
			tBox.Render ("CollisionBox");
			Material stMat = Resources.Load ("Material", typeof(Material)) as Material;
			mf.mesh = tBox.mesh;
			mr.material = stageDebugMat;
			return true;
	} else
			return false;
	}
}

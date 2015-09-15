﻿using UnityEngine;
using System.Collections;

public class Projectile :MonoBehaviour{
	public const int V_NUM = 8;
	public GameObject dbgBox;
	public SPoint pos, vel;
	protected STimer ttl;
	public ParticleSystem[] ps;
	public bool active;
	public Fighter[] fighterList;
	public float pScale;
	public float dir, dam, mag;
	int vNum;
	public AttkData hitdata;
	public SPoint[] v;
	public float[] ang;
	public int plNum;
	public Projectile()
	{
		dbgBox = null;
		hitdata = new AttkData ();
		active=false;
		vNum = V_NUM;
		pScale = 1.0f;
		v = new SPoint[V_NUM];
		ang = new float[V_NUM];
		v[0]=new SPoint(0,  1);
		v[1]=new SPoint(-0.71f, .71f);
		v[2]=new SPoint(-1, 0);
		v[3]=new SPoint(-0.71f,-0.71f);
		v[4]=new SPoint(0, -1);
		v[5]=new SPoint(0.71f,-0.71f);
		v[6]=new SPoint(1, 0);
		v[7]=new SPoint(0.71f,0.71f);
		ttl = new STimer ();
		for(int j = 0; j < vNum-1; j++)
			ang[j] = Mathf.Atan2(v[j+1].y - v[j].y, v[j+1].x - v[j].x); //store angles for detection
		ang[V_NUM-1] = Mathf.Atan2(v[0].y - v[vNum-1].y, v[0].x - v[vNum-1].x);
		
		
	}
	public void Start(){
				GameObject go;
				int pCt = 0;
				for (int i=1; i<5; i++) {
						go = GameObject.FindGameObjectWithTag ("Player" + i);
						if (go != null)
								pCt++;
				}
	 
				fighterList = new Fighter[pCt - 1];
				int plC = 0;
				int otC = 0;
				go = new GameObject ();
				for (int i=1; i<pCt+1; i++) {
						go = GameObject.FindGameObjectWithTag ("Player" + i);
						if (go != null) {
				
								if (i != plNum) {
										fighterList [otC] = go.GetComponent<Fighter> ();
										otC++;
								}
								plC++;
						}
				}
		}
	public void Update(){
		FrameUpdate (Time.deltaTime);
	}
 
	public virtual void Detonate(){
		active = false;
		for(int i=0;i<ps.Length;i++){
		ps[i].Stop ();
		ps[i].enableEmission=false;
			}
	}
	public virtual void FrameUpdate(float timeLapsed){
		for (int i=0; i<fighterList.Length; i++) {
			AttackDetect (fighterList[i]);
			
		}
        
	}public bool CheckAxis( SPoint origin, float dir, SPoint[] plBox, SPoint[] aBox, int pNum, int aNum){
		// helper function for the Separating Axis Theorem that takes an axis defined by origin and dir 
		//re-conditioned to handle the parameters of the attack box 
		bool isHit = true;
		float projMin, projMax, hitMin, hitMax, distSq, projVal, cVal;
		SPoint projVec, nVec, rVec, cVec;
		projVec = new SPoint();
		projMin = 100;
		projMax = -100;
		hitMin = 100;
		hitMax = -100;	
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
		float eps = 0.00001f;
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
		if((Mathf.Abs(projMax - hitMin)< eps)||(Mathf.Abs(hitMax - projMin)< eps))
			isHit=false;
		return isHit;
	}
public void AttackDetect(Fighter plr){
				//projectile checks
		SPoint[] pHdr = new SPoint[8];
		SPoint[] plBox;
		plBox = plr.GetHitBox();
		int hbLen=4;
		float[] plAng ={0, Mathf.PI/2, Mathf.PI, -Mathf.PI/2};
		bool hitflag = true;
				for (int i=0; i<8; i++)
						pHdr [i] = new SPoint (v [i].x + pos.x, v [i].y +pos.y);
		float atkLen = GetVNum ();
				for (int j = 0; j < atkLen; j++)
						if (!CheckAxis (pHdr [0], ang [j], plBox, pHdr, hbLen, GetVNum ())) //no axis intersection
								hitflag = false;
	
				if (hitflag)//test on the axis of the player hit box to confirm
						for (int j = 0; j < 4; j++)
								if (!CheckAxis (plBox [j], plAng [j], plBox, pHdr, hbLen,GetVNum ())) //no axis intersection
										hitflag = false;
				if (hitflag) {
						active = false;
						plr.GetHit (hitdata);
						Detonate ();
				}
	
		
				
		}
	public void SetAtkData(float dm, float di, float ma){
		hitdata.dir=di;
        hitdata.mag=ma;
        hitdata.dmg=dm;
    }
    public void ResetV(){
        v[0]=new SPoint(0,  1);
        v[1]=new SPoint(-0.71f, .71f);
        v[2]=new SPoint(-1, 0);
        v[3]=new SPoint(-0.71f,-0.71f);
        v[4]=new SPoint(0, -1);
		v[5]=new SPoint(0.71f,-0.71f);
		v[6]=new SPoint(1, 0);
		v[7]=new SPoint(0.71f,0.71f);
	}
	SPoint getPosition(int ind){
		SPoint rS;
		rS = new SPoint(v[ind].x+pos.x, v[ind].y+pos.y);
		return rS;
	}

	public void Scale(float s){
		s=Mathf.Abs(s);
		pScale=s;
		ResetV();
		for(int i=0;i<vNum;i++){
			v[i].x=v[i].x*s;
			v[i].y=v[i].y*s;
		}
	}
	public virtual bool Fire(SPoint o, SPoint s){
		//dir is the vector to travel in per second
		if(active)
			return false;
		active=true;
		pos=o;
		vel=s;
		ttl.SetTimer(2);
		for(int i=0;i<ps.Length;i++){
		if (ps != null) {
			ps[i].transform.position=new Vector3(pos.x, pos.y,0);		
			ps[i].enableEmission=true;
			}

		}
		return true;
	}
	public int GetVNum(){
		return vNum;
	}
}

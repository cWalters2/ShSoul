using UnityEngine;
using System.Collections;

public class plProjectile {
	public const int V_NUM = 8;
	public GameObject dbgBox;
	public SPoint pos, vel;
	STimer ttl;
	public ParticleSystem ps;
	public bool active;
	public float pScale;
	int vNum;
	public AttkData hitdata;
	public SPoint[] v;
	public float[] ang;
	public plProjectile()
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
	public void Detonate(){
		active = false;
		ps.Stop ();
		ps.enableEmission=false;
	}
	public void FrameUpdate(float timeLapsed){

		if (active)
				if (!ttl.RunTimer (timeLapsed)) {
						pos.x += vel.x * timeLapsed;
						pos.y += vel.y * timeLapsed;
			//ps.transform.position.Set (pos.x, pos.y, 0);
        } else {
						active = false;
						if (ps.IsAlive ())
								ps.Stop ();
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
	public bool Fire(SPoint o, SPoint v){
		//dir is the vector to travel in per second
		if(active)
			return false;
		active=true;
		pos=o;
		vel=v;
		ttl.SetTimer(2);
		if (ps != null) {
			ps.transform.position=new Vector3(pos.x, pos.y,0);		
			ps.enableEmission=true;


		}
		return true;
	}
	public int GetVNum(){
		return vNum;
	}
}

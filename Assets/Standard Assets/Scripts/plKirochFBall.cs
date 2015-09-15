using UnityEngine;
using System.Collections;

public class plKirochFBall : Projectile {

	public float grav;
	public float velx, vely;
	// Update is called once per frame
	public void Start(){
       
		SetAtkData (dam, dir, mag);
		base.Start ();
	}
	public override void FrameUpdate(float timeLapsed){
	//	float timeLapsed = Time.deltaTime;
		if (timeLapsed == 0)
						return;
		vel.y += grav;
		pos.x += vel.x;
		pos.y += vel.y;
		transform.Translate (new Vector3(0,vel.y, vel.x));

		if (ttl.RunTimer (timeLapsed))
						Destroy (gameObject);
		if(GameObject.FindGameObjectWithTag ("Stage").GetComponent<Stage> ().ProjectileDetect(this))
			Destroy (gameObject);
		base.FrameUpdate (timeLapsed);
    }
	public override void Detonate(){
		active = false;
		for (int i=0; i<ps.Length; i++) {
						ps[i].Stop ();
						ps[i].enableEmission = false;
				}
		Destroy (gameObject);
    }
    public virtual bool Fire(Fighter plr, SPoint o, SPoint v){
		plNum = plr.plNum;
		vel = new SPoint (velx*v.x, vely);
		float diff;
		if (!plr.fHelper.IsFacingRight()) {
			diff = ((Mathf.PI / 2) - dir) * 2.0f;
			dir += diff;
		}
        //dir 2.0fis the vector to travel in per second
		//if(active)
		//	return false;
		active=true;
		pos=new SPoint(o.x,o.y+16);
		transform.position = new Vector3 (pos.x, pos.y, 0);

		ttl.SetTimer(2);
		for (int i=0; i<ps.Length; i++) {
			if (ps != null) {
				ps[i].transform.position = new Vector3 (pos.x, pos.y, 0);		
				ps[i].enableEmission = true;
				ps[i].Play ();


			}
	}
        return true;
    
	}
}

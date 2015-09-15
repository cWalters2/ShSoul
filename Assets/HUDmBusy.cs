using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDmBusy : MonoBehaviour {
	public int num;
	
	Fighter f;
	Color mOff;
	Color mOn;
	GameObject g;
	// Use this for initialization
	void Start () {
		GameObject player = GameObject.FindGameObjectWithTag ("Player" + num);
		mOff = new Color (0, 0, 0);
		mOn = new Color (1, 1, 1);
		g = this.gameObject;
		if (player != null) {
			f = player.GetComponent<Fighter> ();
			
		} else {
			g.SetActive (false);
		}
	}	
	
	// Update is called once per frame
	void Update () {
		if (f != null) {
			if(f.stats.flags.mBusy)
				g.GetComponent<RawImage>().color=  mOn;
			else
				g.GetComponent<RawImage>().color=  mOff;
		}
	}
}

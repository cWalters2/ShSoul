using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUD_tumble : MonoBehaviour {
	public int num;
	
	Fighter f;
	Color aOff;
	Color aOn;
	GameObject g;
	// Use this for initialization
	void Start () {
		GameObject player = GameObject.FindGameObjectWithTag ("Player" + num);
		aOff = new Color (0, 0, 0);
		aOn = new Color (1, 1, 1);
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
			if(!f.stats.tumble.tmr.IsReady())
				g.GetComponent<RawImage>().color=  aOn;
			else
				g.GetComponent<RawImage>().color=  aOff;
		}
	}
}

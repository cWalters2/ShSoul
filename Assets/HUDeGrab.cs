using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDeGrab : MonoBehaviour {
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
			if(f.fHelper.grabBox.isActive)
				g.GetComponent<RawImage>().color=  mOn;
			else
				g.GetComponent<RawImage>().color=  mOff;
		}
	}
}

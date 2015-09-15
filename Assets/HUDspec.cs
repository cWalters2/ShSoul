using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class HUDspec : MonoBehaviour {
	public int num;
	string plSpec = "";
	Fighter f;
	Text t;
	GameObject g;
	// Use this for initialization
	void Start () {
		g = this.gameObject;
		t = GetComponent<Text> ();
		GameObject player = GameObject.FindGameObjectWithTag ("Player" + num);
		if (player != null) {
						f = player.GetComponent<Fighter> ();
						
						plSpec = f.GetSpecMeter ().ToString () + "%";
						t.text = plSpec;
			} else {

			g.SetActive (false);
			}
		}
	
	// Update is called once per frame
	void Update () {
		if (f != null) {
			plSpec = f.GetSpecMeter ().ToString ();
			t.text = plSpec;
		}
	}
}

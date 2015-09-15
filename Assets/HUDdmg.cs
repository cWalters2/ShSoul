using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDdmg : MonoBehaviour {
	public int num;
	string plDmg = "";
	Fighter f;
	Text t;
	GameObject g;
	// Use this for initialization
	void Start () {
		GameObject player = GameObject.FindGameObjectWithTag ("Player" + num);
		g = this.gameObject;
		if (player != null) {
				f = player.GetComponent<Fighter> ();
				t = GetComponent<Text> ();
				plDmg = f.stats.damage.ToString () + "%";
				t.text = plDmg;
			} else {
			g.SetActive (false);
			}
		}	
	
	// Update is called once per frame
	void Update () {
		if (f != null) {
			plDmg = f.stats.damage.ToString ();
			t.text = plDmg;
		}
	}
}

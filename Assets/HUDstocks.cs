using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class HUDstocks : MonoBehaviour {

	public int num;
	string plStk = "";
	Fighter f;
	Text t;
	GameObject g;
	// Use this for initialization
	void Start () {
		GameObject player = GameObject.FindGameObjectWithTag ("Player" + num);
		g = this.gameObject;
		if (player != null) {	
			f = player.GetComponent<Fighter>();
			t = GetComponent<Text>();
			plStk =  f.stats.stocks.ToString() + "%";
			t.text = plStk;
		} else {
			g.SetActive (false);
		}
	}
		
		// Update is called once per frame
	void Update () {
		if (f != null) {
			plStk = f.stats.stocks.ToString ();
			t.text = plStk;
			}
		}
	}

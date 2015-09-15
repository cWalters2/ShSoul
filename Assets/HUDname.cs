using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDname : MonoBehaviour {
	public int num;
	string plName = "";
	GameObject g;
	// Use this for initialization
	void Start () {
		GameObject player = GameObject.FindGameObjectWithTag ("Player" + num);
		g = this.gameObject;
		if (player != null) {
			Fighter f = player.GetComponent<Fighter>();
			Text t = GetComponent<Text>();
			plName = f.CharName ();
			t.text = plName;
		}else {
			g.SetActive (false);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDpanel : MonoBehaviour {
	GameObject g;
	public int num;
	// Use this for initialization
	void Start () {
		g = this.gameObject;

		GameObject player = GameObject.FindGameObjectWithTag ("Player" + num);
		if (player == null)
			g.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

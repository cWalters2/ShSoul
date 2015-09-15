using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MatchConsole : MonoBehaviour {
	public Material attMat;
	public Material boxMat;

	Text t;

	HitDisplay hd;
	// Use this for initialization
	void Start () {
		t = GetComponent<Text>();
		hd = new HitDisplay ();
		hd.attMat = attMat;
		hd.boxMat = boxMat;
	}

	// Update is called once per frame
	void Update () {
	
	}
	public void PostToConsole(string s){

		t.text = t.text + s + "/r/n";


	}
	public void PostHit(SPoint[] a, int numArr, SPoint[] b, int numBArr, SPoint[] p, SPoint gO, float gI){
			hd.PostHit(a, numArr, b, numBArr, p, gO, gI);
	}
}

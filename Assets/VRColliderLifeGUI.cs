using UnityEngine;
using System.Collections;

public class VRColliderLifeGUI : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI(){
		float getLife=GameObject.Find("headCollider").GetComponent<VRCollider>().getLife();
		if(getLife!=0.0f){
			GUI.Box (new Rect (Screen.width * 0.3f, Screen.height -30f, Screen.width * 0.4f * (getLife/3.0f), 20f),
				""
			);
			GUI.Label (new Rect (Screen.width * 0.3f, Screen.height -30f, Screen.width * 0.4f, 20f),
				"HP: "+getLife
			);
		}

		if(getLife==0.0f){
			GUI.Box (
				new Rect (
					Screen.width * 0.3f, Screen.height * 0.4f,
					Screen.width * 0.4f, Screen.height * 0.2f
				),
				"GAME OVER\n\nBOOM"
			);
			if(GUI.Button (
				new Rect (
					Screen.width * 0.3f, Screen.height * 0.6f,
					Screen.width * 0.4f, Screen.height * 0.05f
				),
				"back"
			)){
				Application.LoadLevel ("StartMenu");
			}
		}
	}
}

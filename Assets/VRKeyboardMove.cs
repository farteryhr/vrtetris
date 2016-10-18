using UnityEngine;
using System.Collections;

public class VRKeyboardMove : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float getLife=GameObject.Find("headCollider").GetComponent<VRCollider>().getLife();
		if(getLife!=0.0f){
			if(Input.GetButton("MoveL") && transform.position.x>-5.5f){
				transform.Translate(Vector3.left*0.05f);
			}
			if(Input.GetButton("MoveR") && transform.position.x<5.5f){
				transform.Translate(Vector3.right*0.05f);
			}
		}
	}
}

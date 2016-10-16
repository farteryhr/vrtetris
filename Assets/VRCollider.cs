using UnityEngine;
using System.Collections;

public class VRCollider : MonoBehaviour {

	public float life=3.0f;

	public float getLife(){
		return life;
	}
	public void addLife(float add){
		if(life!=0.0f){
			life+=add;
			if(life>3.0f){
				life=3.0f;
			}
		}
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter(Collision collision) {
		//Debug.Log(collision.impulse);
		//Debug.Log(collision.impulse.magnitude);
		life-=collision.impulse.magnitude;
		if(life<=0.0f){
			life=0.0f;
		}
    }

	void OnCollisionStay(Collision collision) {
		OnCollisionEnter(collision);
	}
}

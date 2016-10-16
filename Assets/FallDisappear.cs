using UnityEngine;
using System.Collections;

public class FallDisappear : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(transform.position.y<-10.0f){
			GameObject.Destroy(this.gameObject);
		}
	}
}

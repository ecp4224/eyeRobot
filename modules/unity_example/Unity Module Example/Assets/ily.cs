using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ily : MonoBehaviour {

	public GameObject love;

	void Update(){
		if (love == null) Destroy (this.gameObject);
		//if (!love.activeSelf) Destroy (this.gameObject);
	}
}

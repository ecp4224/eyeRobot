using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destination : MonoBehaviour {

	public Light glow;
	public float glowCycle=2f;
	public float glowIntensity=10f;

	public float rotSpeed;
	Vector3 rotVector;


	bool toGlow;

	void Start(){
		
		//glow = this.GetComponent<Light>();
		float x = Random.Range (0f, 90f);
		float y = Random.Range (0f, 90f);
		float z = Random.Range (0f, 90f);

		rotVector = new Vector3 (x, y, z);
	}

	void Update(){

		this.transform.Rotate (rotVector * rotSpeed * Time.deltaTime);

		if (toGlow) {
			if (glow.intensity < glowIntensity-.5f) {
				glow.intensity = Mathf.Lerp (glow.intensity, glowIntensity, glowCycle * Time.deltaTime);
			} else {
				toGlow = !toGlow;
			}
		} else {
			if (glow.intensity > 0.5f) {
				glow.intensity = Mathf.Lerp (glow.intensity, 0f, glowCycle * Time.deltaTime);
			} else {
				toGlow = !toGlow;
			}
		}
	}
}

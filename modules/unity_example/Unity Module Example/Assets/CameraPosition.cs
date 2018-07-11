using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour {

	public Transform camPos;
	public float smoothSpeed = 0.75f;

	void Update(){
	
		if (camPos == null) return;

		if (this.transform.position != camPos.position)
			this.transform.position = Vector3.Lerp (this.transform.position, camPos.position, (1f/smoothSpeed) * Time.deltaTime);

		if (this.transform.rotation != camPos.rotation)
			this.transform.rotation = Quaternion.Lerp (this.transform.rotation, camPos.rotation, (1f / smoothSpeed) * Time.deltaTime);
		
	
	}
}

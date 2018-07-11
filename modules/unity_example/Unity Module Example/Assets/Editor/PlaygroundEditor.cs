using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Playground))]
public class PlaygroundEditor : Editor {

	public override void OnInspectorGUI(){
	
		base.OnInspectorGUI ();

		Playground pg = target as Playground;

		pg.UpdatePlayground ();
	
	
	}
}

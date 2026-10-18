using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByTime : MonoBehaviour {

	[SerializeField]
	private bool hideObject = false;
	[SerializeField]
	private float destroyTime = 3;
	private float elapsedTime = 0;


    // Use this for initialization
    void Start () {
		if (!hideObject) {
			Destroy (gameObject, destroyTime);
		}
	}


	private void FixedUpdate(){
		if (hideObject) {
			elapsedTime += Time.deltaTime;
			if (elapsedTime >= destroyTime) {
				gameObject.SetActive (false);
				elapsedTime = 0;
			}
		}
	}
	
	 
	 
}

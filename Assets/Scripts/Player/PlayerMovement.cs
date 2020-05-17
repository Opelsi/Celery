using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float movementSpeed = 200f;
	public GameObject planet;
	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		Vector3 forceOfGravity = new Vector3(0, 0, 0);
		if (planet != null)
		{
			forceOfGravity = (planet.transform.position-transform.position).normalized * 10f*Time.deltaTime;
		}
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");
		Vector3 movement = new Vector3(moveHorizontal,0, moveVertical);
		gameObject.transform.position+=(movement * movementSpeed*Time.deltaTime);
		gameObject.GetComponent<Rigidbody>().AddForce(forceOfGravity);
    }
}

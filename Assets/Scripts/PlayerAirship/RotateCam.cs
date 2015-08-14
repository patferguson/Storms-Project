﻿using UnityEngine;
using System.Collections;
//This script takes input from the input manager, and passes the movement into an empty game object with an attached camera.
//Most of this script was derived from teh Unity Example for transform.rotate
public class RotateCam : MonoBehaviour 
{
	private StateManager referenceStateManager;

	//The rotate cam is the Center GameObject - not the Camera itself.
	public GameObject rotateCam;
	
	public float horizontalTiltAngle = 360.0f;
	public float verticalTiltAngle = 90.0f;
	public float smooth = 2.0f;
	public float deadZoneFactor = 0.25f;
	
	private float tiltAroundY;
	private float tiltAroundX;
	
	//Move the target object
	
	public GameObject lookyHereTarget;
	public float targetHeightFactor = 5.0f;
	private float yPos = 0;
	
	//Move the camera directly
	
	public GameObject camHereTarget;
	private float xPos;
	public float camPositionFactor = 2.0f;
	private float zPos;
	public float camDistanceFactor = 15.0f;
	
	//Link to Cannons
	public GameObject[] cannons;
	
	private enum CannonStates {Front, Port, Starboard};
	
	
	void Start()
	{
		referenceStateManager = gameObject.GetComponent<StateManager>();
	}

	
	
	public void PlayerInputs(float camVertical, float camHorizontal, float dPadVertical, float dPadHorizontal, bool fireCannon)
	{
		tiltAroundY = -camHorizontal * horizontalTiltAngle * deadZoneFactor;
		tiltAroundX = -camVertical * verticalTiltAngle * deadZoneFactor;
		
		Quaternion target =  Quaternion.Euler(tiltAroundX, tiltAroundY, 0);
		
		if (referenceStateManager.currentPlayerState == EPlayerState.Control)
		{
			rotateCam.transform.localRotation = Quaternion.Slerp(rotateCam.transform.localRotation, target, Time.deltaTime * smooth);
		}

		
		//Move lookTarget around.
		float internalCamYRotation = rotateCam.transform.localEulerAngles.y;
		//Debug.Log(internalCamYRotation);
	
		
		if (internalCamYRotation <= 315 && internalCamYRotation > 225)
		{
			//print ("Left");
			//Move the target
			yPos = Mathf.Lerp(yPos, targetHeightFactor, Time.deltaTime * smooth/2);
			
			//Move the cam
			xPos = Mathf.Lerp(xPos, camPositionFactor, Time.deltaTime * smooth/2);
			zPos = Mathf.Lerp(zPos, camDistanceFactor, Time.deltaTime * smooth/2);
			
			//Allow CannonFire
			if (fireCannon)
			{
				Cannons(CannonStates.Port);
			}
			
		}
		else
		if (internalCamYRotation <= 135 && internalCamYRotation > 45)
		{
			//print ("Right");
			
			//Move the target
			yPos = Mathf.Lerp(yPos, targetHeightFactor, Time.deltaTime * smooth/2);
			
			
			//Move the cam
			xPos = Mathf.Lerp(xPos, -camPositionFactor, Time.deltaTime * smooth/2);
			zPos = Mathf.Lerp(zPos, camDistanceFactor, Time.deltaTime * smooth/2);
			
			//Allow CannonFire
			if (fireCannon)
			{
				Cannons(CannonStates.Starboard);
			}
		
		}
		else
		if ( internalCamYRotation <= 225 && internalCamYRotation > 135)
		{
		 	//print ("Back");
			//Move the target
		 	yPos = Mathf.Lerp(yPos, 0, Time.deltaTime * smooth/2);
		 	
		 	
			//Move the cam
			xPos = Mathf.Lerp(xPos, 0, Time.deltaTime * smooth/2);
			zPos = Mathf.Lerp(zPos, 20, Time.deltaTime * smooth/2);
		}
		else
		{
			//print ("Forward");
			//Move the target
			yPos = Mathf.Lerp(yPos, 0, Time.deltaTime * smooth/2);
			
			
			//Move the cam
			xPos = Mathf.Lerp(xPos, 0, Time.deltaTime * smooth/2);
			zPos = Mathf.Lerp(zPos, 20, Time.deltaTime * smooth/2);
			
			//Allow CannonFire
			if (fireCannon)
			{
				Cannons(CannonStates.Front);
			}
			
		}
		
		lookyHereTarget.transform.localPosition = new Vector3(lookyHereTarget.transform.localPosition.x, yPos, lookyHereTarget.transform.localPosition.z);
		camHereTarget.transform.localPosition = new Vector3(xPos, camHereTarget.transform.localPosition.y, -zPos);

	}
	
	void Cannons(CannonStates angle)
	{
		CannonFire script; 
	
		for (int i = 0; i < cannons.Length; i++)
		{
			script = cannons[i].GetComponent<CannonFire>();
			
		
			if (angle == CannonStates.Front)
			{
				print ("Fore");
				if(script.cannon == ECannonPos.Forward)
				{
					script.Fire();
				}
			}
			else
			if (angle == CannonStates.Port)
			{
				print ("Port");
				if(script.cannon == ECannonPos.Port)
				{
					script.Fire();
				}
			}
			else
			if (angle == CannonStates.Starboard)
			{
				print ("Starboard");
				if(script.cannon == ECannonPos.Starboard)
				{
					script.Fire();	
				}
			}
			
		}
	}

}
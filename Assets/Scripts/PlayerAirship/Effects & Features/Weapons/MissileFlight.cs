﻿/**
 * File: MissileFlight.cs
 * Author: Rowan Donaldson
 * Maintainer: Pat Ferguson
 * Created: 28/09/2015
 * Copyright: (c) 2015 Team Storms, All Rights Reserved.
 * Description: Controls the homing missile behaviour.
 **/

using UnityEngine;
using System.Collections;

namespace ProjectStorms
{
	[RequireComponent(typeof(Rigidbody))]
	public class MissileFlight : MonoBehaviour 
	{
		public GameObject target;

		private Rigidbody myRigid;

		private bool attacking = false;
		private bool startWait = false;

		public float movementVelocity = 1;
		public float turnSpeed = 5;

		private GameObject targetProxy;

		//This is the key value
		public float closeRangeThreshold = 20;

		public float secondsTillTimeout = 5;

		void Awake()
		{
			myRigid = gameObject.GetComponent<Rigidbody> ();

			targetProxy = new GameObject();
			targetProxy.name = "MissileTarget";
		}

		void Update () 
		{
			//Raycast
			Vector3 rayDirection = (targetProxy.transform.position - gameObject.transform.position).normalized;
			float rayDistance = Vector3.Distance (targetProxy.transform.position, gameObject.transform.position);

			if (attacking)
			{
				Debug.DrawRay (gameObject.transform.position, rayDirection * rayDistance, Color.red);
			}
			else
			if (!attacking)
			{
				Debug.DrawRay(gameObject.transform.position, rayDirection * rayDistance, Color.green);
			}

			if (!attacking && !startWait)
			{
				//When there is no more target, begin to time out the object
				Invoke ("GoToSleep", secondsTillTimeout);
			}
		}


		void FixedUpdate()
		{
			if (attacking) 
			{
				myRigid.velocity = transform.forward * movementVelocity;

				//Try it with targetProxy
				Quaternion targetDirection = Quaternion.LookRotation (targetProxy.transform.position - myRigid.transform.position);
					
				myRigid.MoveRotation (Quaternion.RotateTowards (myRigid.transform.rotation, targetDirection, turnSpeed));
			} 
			else
			if (!attacking) 
			{
				//Move forward in a straight line
				myRigid.velocity = transform.forward * movementVelocity;
			}

			float distanceToProxyPoint = Vector3.Distance(targetProxy.transform.position, myRigid.transform.position);


			if (distanceToProxyPoint > closeRangeThreshold)
			{
				//only update the target pos if target is more than 10 meters away from missile
				targetProxy.transform.position = target.transform.position;
			}
			else
			if (distanceToProxyPoint < closeRangeThreshold)
			{
				//Turn off Movement
				attacking = false;
			}
		}

		void FindTarget()
		{
			//Reset angular velocity?
			myRigid.angularVelocity = Vector3.zero;

			//Only airships have the AirshipControlBehaviour scipt so look for them
			target = GameObject.FindObjectOfType<AirshipControlBehaviour> ().gameObject;
			print (target.gameObject.transform.root.gameObject.name);

			//Give the missile a target
			targetProxy.transform.position = target.transform.position;
			
			if  (!attacking)
			{
				attacking = true;
			}

			startWait = false;
		}


		void OnEnable()
		{
			//Invoke ("GoToSleep", secondsTillTimeout);
			///FindTarget ();
			/// //Don't try and find target straight away, because it'll just find the player that shot the missile.
			Invoke ("FindTarget", 1);
			//Fire ();
			startWait = true;
		}

		void GoToSleep()
		{
			if (!attacking)
			{
				gameObject.SetActive (false);
			}
		}
		
		
		/*
		void Spiral()
		{

			Vector3 pos = gameObject.transform.position;

			Quaternion rot = Quaternion.AngleAxis (spiralSpeed * Time.time, Vector3.forward);
			Vector3 dir = pos - new Vector3 (gameObject.transform.position.x+1, gameObject.transform.position.y, gameObject.transform.position.z);
			dir = rot * dir;

			myRigid.MovePosition (pos + dir);
		}*/

		/*
		void Fire()
		{
			//Reset angular velocity?
			//myRigid.angularVelocity = Vector3.zero;

			if  (!attacking)
			{
				attacking = true;
			}

			//Give the missile a target
			//targetProxy.transform.position = target.transform.position;
		}*/
	}
}

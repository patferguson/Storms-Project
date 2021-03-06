﻿/**
 * File: ShipPartDestroy.cs
 * Author: Patrick Ferguson
 * Created: 20/08/2015
 * Copyright: (c) 2015 Team Storms, All Rights Reserved.
 * Description: Manages the destruction of player ship parts, and parent mass change.
 **/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProjectStorms
{
    /// <summary>
    /// Manages the destruction of player ship parts, and parent mass change.
    /// </summary>
    public class ShipPartDestroy : MonoBehaviour
    {
        /// <summary>
        /// What the part is, affects how the ship's handling will be hampered.
        /// </summary>
        public enum EShipPartType
        {
            INVALID,
            LEFT_BALLOON,
            RIGHT_BALLOON,
            MAST,
            RUDDER,
        }

        /// <summary>
        /// Threshold for a collision to consider a bump rumble.
        /// </summary>
        public float bumpVelThreshold = 90.0f;

        /// <summary>
        /// Strength of the bump rumble and screenshake.
        /// </summary>
        public float bumpRumbleStr = 1.0f;

        /// <summary>
        /// Duration of the bump rumble and screenshake.
        /// </summary>
        public float bumpRumbleDurr = 0.75f;

        /// <summary>
        /// Force to explode the balloons with.
        /// </summary>
        public float balDestForce = 500.0f;

        /// <summary>
        /// Strength to rumble and screenshake the balloon destruction on.
        /// </summary>
        public float balDestRumbleStr = 1.0f;

        /// <summary>
        /// How long to rumble and screenshake the balloon destruction for.
        /// </summary>
        public float balDestRumbleDurr = 0.2f;

        /// <summary>
        /// Cone angle for detecting directional collisions.
        /// </summary>
        public float colDirThreshold = 0.258819f; // Sin(15*);

        /// <summary>
        /// Trigger the component on balloon destruction
        /// </summary>
        public AudioSource balloonPopNoise;

        /// <summary>
        /// A collection object to contain part information.
        /// </summary>
        [System.Serializable]
        public class ShipPart
        {
            /// <summary>
            /// Handle to the parent of all of the part's colliders, this part will be disabled.
            /// </summary>
            public GameObject partObject = null;
            /// <summary>
            /// What the part is, affects how the ship's handling will be hampered.
            /// </summary>
            public EShipPartType partType = EShipPartType.INVALID;
            /// <summary>
            /// If a collision relative velocity is larger than this value, the part will break.
            /// </summary>
            public float breakVelocity = 100;
            /// <summary>
            /// Amount to subtract from the ship's mass when this part gets destroyed.
            /// </summary>
            public float partMass = 5;

            [HideInInspector]
            // Cached variables, these don't have to be updated by hand
            public float cached_breakVelocitySqr = 0;
        }
        /// <summary>
        /// The parts of this ship that have children colliders and can be destroyed.
        /// </summary>
        public ShipPart[] destructableParts;

        /// <summary>
        /// For testing the player death.
        /// </summary>
        private bool m_balLeftDest = false;
        /// <summary>
        /// For testing the player death.
        /// </summary>
        private bool m_balRightDest = false;

        // Cached variables
        private Transform m_trans = null;
        private Rigidbody m_rb = null;
        private PassengerTray m_shipTray = null;
        private StateManager m_shipStates = null;
        private float m_bumpVelSqr = 0;

        void Awake()
        {
            m_trans = transform;
            m_rb = GetComponent<Rigidbody>();
            m_shipTray = GetComponentInChildren<PassengerTray>();
            m_shipStates = GetComponent<StateManager>();
            m_bumpVelSqr = bumpVelThreshold * bumpVelThreshold;
        }

        /// <summary>
        /// Use this for initialisation.
        /// </summary>
        void Start()
        {

        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {

        }

        /// <summary>
        /// Called when a collision begins to objects.
        /// </summary>
        /// <param name="a_colInfo">Collision information.</param>
        void OnCollisionEnter(Collision a_colInfo)
        {
            // Ignore losing parts to prisoners
            if (!a_colInfo.gameObject.tag.Contains("Passengers"))
            {
                Rigidbody rbOther = a_colInfo.rigidbody;

                // Work out relative collision velocity
                Vector3 offsetVel = a_colInfo.relativeVelocity;
                float velDiffSqr = offsetVel.sqrMagnitude;

                if (rbOther != null)
                {
                    // Evaluate player collision if ramming other
                    if (a_colInfo.gameObject.tag.Contains("Player") &&
                        m_rb.velocity.sqrMagnitude >= rbOther.velocity.sqrMagnitude)
                    {
                        EvaluatePlayerCollision(a_colInfo.collider, offsetVel, velDiffSqr);
                    }
                }
                else
                {
                    // If slamming into a wall
                    if (a_colInfo.relativeVelocity.sqrMagnitude >= m_bumpVelSqr)
                    {
                        InputManager.SetControllerVibrate(gameObject.tag, bumpRumbleStr, bumpRumbleStr, bumpRumbleDurr, true);

                        // Turn off the passenger tray for a bit
                        m_shipTray.PowerDownTray();
                    }
                }

                // Check part destruction
                EvaluatePartCollision(a_colInfo.collider, velDiffSqr);
            }
        }

        /// <summary>
        /// Called when a trigger enter begins to objects.
        /// </summary>
        /// <param name="a_otherCol">Other collider.</param>
        void OnTriggerEnter(Collider a_otherCol)
        {
            // Ignore losing parts to prisoners
            if (!a_otherCol.tag.Contains("Passengers"))
            {
                Rigidbody rbOther = a_otherCol.GetComponentInParent<Rigidbody>();
                if (rbOther != null)
                {
                    // Work out relative collision velocity
                    Vector3 offsetVel = (m_rb.velocity - rbOther.velocity);
                    float velDiffSqr = offsetVel.sqrMagnitude;

                    // Run on the other ship because OnTriggerEnter only allows us to get the other component, not our own collider
                    ShipPartDestroy scriptOther = a_otherCol.GetComponentInParent<ShipPartDestroy>();
                    if (scriptOther != null)
                    {
                        scriptOther.EvaluatePartCollision(a_otherCol, velDiffSqr);
                    }
                }
            }
        }
        
        /// <summary>
        /// Evaluates the collision with the input collider.
        /// </summary>
        /// <param name="a_colInfo">Collision information.</param>
        /// <param name="a_velDiff">Relative collision velocity.</param>
        /// <param name="a_colVelSqr">Squared relative collision velocity magnitude.</param>
        private void EvaluatePlayerCollision(Collider a_otherCol, Vector3 a_velDiff, float a_colVelSqr)
        {
            // Get scripts
            AirshipControlBehaviour ship = a_otherCol.GetComponentInParent<AirshipControlBehaviour>();
            if (ship != null)
            {
                PassengerTray otherTray = ship.gameObject.GetComponentInChildren<PassengerTray>();

                if (otherTray != null)
                {
                    // Establish collision direction
                    Transform otherTrans = a_otherCol.transform;
                    float forwardDot = Vector3.Dot(m_trans.forward, otherTrans.forward);
                    float rightDot = Vector3.Dot(m_trans.right, otherTrans.right);

                    // Get positions
                    Vector3 myPos = m_trans.position;
                    Vector3 otherPos = otherTrans.position;
                    Vector3 offset = otherPos - myPos;

                    Rigidbody rbOther = a_otherCol.GetComponent<Rigidbody>();

                    if (rbOther == null)
                    {
                        rbOther = a_otherCol.GetComponentInParent<Rigidbody>();
                    }

                    // For evaluating whether the collision is from below/above
                    float upDot = Vector3.Dot(offset.normalized, otherTrans.up);

                    /*Debug.Log("Front dot: " + Mathf.Abs(forwardDot) +
                        ", right dot: " + Mathf.Abs(rightDot) +
                        ", up dot: " + Mathf.Abs(upDot) +
                        ", threshold: " + colDirThreshold);*/

                    bool powerDownOther = false;
                    bool powerDownMe = false;

                    if (upDot >= 1 - colDirThreshold &&
                        offset.y > 0)
                    {
                        // Above - My ship onto them from above

                        // Make them lose passengers
                        //Debug.Log("Collision - From Above!");
                        powerDownOther = true;
                    }
                    else if (upDot <= -1 + colDirThreshold &&
                        offset.y < 0)
                    {
                        // Below - My ship onto them from below

                        // Make them lose passengers
                        //Debug.Log("Collision - From Below!");
                        powerDownOther = true;
                    }
                    else if (Mathf.Abs(forwardDot) <= colDirThreshold)
                    {
                        // T-Bone - My front into their side

                        // Make them lose passengers
                        //Debug.Log("Collision - From T-Bone!");
                        powerDownOther = true;
                    }
                    else if (forwardDot <= -1 + colDirThreshold)
                    {
                        // Head on - My front into their front

                        // Make both lose passengers
                        //Debug.Log("Collision - From Head On!");
                        powerDownMe = true;
                        powerDownOther = true;
                    }
                    else if (forwardDot >= 1 - colDirThreshold)
                    {
                        // Behind - My front into their back

                        // Make them lose passengers
                        //Debug.Log("Collision - From Behind!");
                        powerDownOther = true;
                    }
                    else if (Mathf.Abs(rightDot) >= 1 - colDirThreshold)
                    {
                        // Horizontal/Side by side - My ship gently bumping them

                        // Make nobody lose passengers
                        //Debug.Log("Collision - Horiztonal bump!");
                    }
                    else if (a_colVelSqr >= m_bumpVelSqr)
                    {
                        // Colliding above rumble threshold from random angle, both lose passengers
                        InputManager.SetControllerVibrate(gameObject.tag, bumpRumbleStr, bumpRumbleStr, bumpRumbleDurr, true);

                        // Turn off the passenger tray for a bit
                        powerDownMe = true;
                        powerDownOther = true;
                    }

                    if (powerDownMe)
                    {
                        m_shipTray.PowerDownTray();

                        // Launch passengers with relative velocity
                        Rigidbody rbTemp = null;
                        List<GameObject> contents = m_shipTray.trayContents;
                        foreach (GameObject passenger in contents)
                        {
                            rbTemp = passenger.GetComponent<Rigidbody>();
                            if (rbTemp != null)
                            {
                                rbTemp.AddForce(rbOther.velocity, ForceMode.VelocityChange);
                            }
                        }
                    }
                    if (powerDownOther)
                    {
                        m_shipTray.PowerDownTray();

                        // Launch passengers with relative velocity
                        Rigidbody rbTemp = null;
                        List<GameObject> contents = otherTray.trayContents;
                        foreach (GameObject passenger in contents)
                        {
                            rbTemp = passenger.GetComponent<Rigidbody>();
                            if (rbTemp != null)
                            {
                                rbTemp.AddForce(m_rb.velocity, ForceMode.VelocityChange);
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Evaluates the collision with the input collider.
        /// </summary>
        /// <param name="a_colInfo">Collision information.</param>
        /// <param name="a_colVelSqr">Squared relative collision velocity magnitude.</param>
        public void EvaluatePartCollision(Collider a_otherCol, float a_colVelSqr)
        {
            // Find the part being collided with
            Collider[] childColliders;
            foreach (ShipPart part in destructableParts)
            {
                childColliders = part.partObject.GetComponentsInChildren<Collider>();
                foreach (Collider col in childColliders)
                {
                    // Check if the collision actually involves the part
                    if (col == a_otherCol)
                    {
                        //Debug.Log("Colliding with: " + col + ", part : " + part.partObject.name);
                        //Debug.Log(colVelSqr);

                        ApplyPartCollision(part, a_colVelSqr);

                        // Collision has evaluated, return back to OnCollisionEnter
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Actually applies the collision to the part, shares this method with the trigger system.
        /// </summary>
        /// <param name="a_part">Ship part collision.</param>
        /// <param name="a_colVelSqr">Squared collision velocity.</param>
        private void ApplyPartCollision(ShipPart a_part, float a_colVelSqr)
        {
            // Cache part squared velocity
            if (Mathf.Approximately(a_part.cached_breakVelocitySqr, 0))
            {
                a_part.cached_breakVelocitySqr = a_part.breakVelocity * a_part.breakVelocity;
            }

            // Break the part if the collision is fast enough
            if (!IsPartDestroyed(a_part) && a_colVelSqr >= a_part.cached_breakVelocitySqr)
            {
                //BreakPart(a_part);
            }
        }

        /// <summary>
        /// Returns whether a part of the input type is still active.
        /// </summary>
        /// <param name="a_type">Ship part type.</param>
        /// <returns>True if destroyed, false if a part is not.</returns>
        public bool IsPartTypeDestroyed(EShipPartType a_type)
        {
            bool partActive = false;
            foreach (ShipPart part in destructableParts)
            {
                // If any part is still around, return true
                if (part.partType == a_type && !IsPartDestroyed(part))
                {
                    partActive = true;
                }
            }
            return !partActive;
        }

        /// <summary>
        /// Returns whether the part has been destroyed.
        /// </summary>
        /// <param name="a_part">The part to test</param>
        /// <returns>True if destroyed, false if not.</returns>
        public bool IsPartDestroyed(ShipPart a_part)
        {
            return !a_part.partObject.activeSelf;
        }

        /// <summary>
        /// Repairs all of the destructible parts on the ship.
        /// </summary>
        public void RepairAllParts()
        {
            foreach (ShipPart part in destructableParts)
            {
                RepairPart(part);
            }
        }

        /// <summary>
        /// Re-enables the part, returns mass and thus restores control to the ship.
        /// </summary>
        /// <param name="a_part">The part to test</param>
        public void RepairPart(ShipPart a_part)
        {
            // Only repair the part if it is destroyed
            if (IsPartDestroyed(a_part))
            {
                // Make the mass change to the parent
                m_shipTray.shipPartMassAdd += a_part.partMass;

                // Re-enable the part
                a_part.partObject.SetActive(true);

                // Clear the destroyed flag
                ClearPartDestroyFlags(a_part);
            }
        }

        /// <summary>
        /// Breaks the part, takes mass and hampers control to the ship.
        /// </summary>
        /// <param name="a_part">The part to test</param>
        public void BreakPart(ShipPart a_part)
        {
            // Make the mass change to the parent
            m_shipTray.shipPartMassAdd -= a_part.partMass;

            // Disable the part
            a_part.partObject.SetActive(false);

            // For testing ship death
            TestShipDestroyed(a_part);
        }

        /// <summary>
        /// Clears the conditions necessary for ship destruction.
        /// </summary>
        /// <param name="a_part">Ship part being repaired.</param>
        private void ClearPartDestroyFlags(ShipPart a_part)
        {
            if (a_part.partType == EShipPartType.LEFT_BALLOON)
            {
                m_balLeftDest = false;
            }
            else if (a_part.partType == EShipPartType.RIGHT_BALLOON)
            {
                m_balRightDest = false;
            }
        }

        /// <summary>
        /// Tests the conditions necessary for ship destruction.
        /// </summary>
        /// <param name="a_part">Ship part being destroyed.</param>
        private void TestShipDestroyed(ShipPart a_part)
        {
            if (a_part.partType == EShipPartType.LEFT_BALLOON)
            {
                // Apply balloon explosion
                m_rb.AddForceAtPosition(m_trans.up * balDestForce, a_part.partObject.transform.position);
                InputManager.SetControllerVibrate(gameObject.tag, balDestRumbleStr, 0.0f, balDestRumbleDurr, true);

                
                m_balLeftDest = true;
            }
            else if (a_part.partType == EShipPartType.RIGHT_BALLOON)
            {
                // Apply balloon explosion
                m_rb.AddForceAtPosition(m_trans.up * balDestForce, a_part.partObject.transform.position);
                InputManager.SetControllerVibrate(gameObject.tag, 0.0f, balDestRumbleStr, balDestRumbleDurr, true);

                m_balRightDest = true;
            }
            if (m_balLeftDest && m_balRightDest)
            {
                // Kill the player
                m_shipStates.SetPlayerState(EPlayerState.Dying);
            }


            if (!balloonPopNoise.isPlaying)
            {
                //balloonPopNoise.pitch = Random.RandomRange(-0.75f, 1.25f);
                balloonPopNoise.Play();
            }
        }
    } 
}

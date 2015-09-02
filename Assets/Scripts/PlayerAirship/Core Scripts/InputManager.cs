﻿/**
 * File: InputManager.cs
 * Author: Rowan Donaldson
 * Maintainer: Patrick Ferguson
 * Created: 6/08/2015
 * Copyright: (c) 2015 Team Storms, All Rights Reserved.
 * Description: Manages player input across various devices, functions for the player linked to each gamepad.
 **/

using UnityEngine;
using System.Collections;
using XInputDotNetPure;

namespace ProjectStorms
{
    /// <summary>
    /// Just the raw controller inputs. These are mainly passed into the airship movement controls, but also effect suicide/fireship and roulette controls.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        private AirshipControlBehaviour m_standardControl;
        private AirshipSuicideBehaviour m_fireshipControl;
        private RouletteBehaviour m_rouletteControl;
        private RotateCam m_rotateCam;
        private ShuntingController m_shuntingControl;

        // TODO: We might need to add more script references here as we progress

        /// <summary>
        /// All of the player controller tags in order.
        /// </summary>
        private static string[] ms_playerTags = {"Player1_", "Player2_", "Player3_", "Player4_"};

        public void Awake()
        {
            m_standardControl = GetComponent<AirshipControlBehaviour>();
            m_fireshipControl = GetComponent<AirshipSuicideBehaviour>();
            m_rouletteControl = GetComponent<RouletteBehaviour>();
            m_rotateCam = GetComponent<RotateCam>();
            m_shuntingControl = GetComponent<ShuntingController>();
        }

        void Start()
        {

        }

        /// <summary>
        /// This input stuff was all figured out in an old script called 'TempDebugScript'.
        /// It's clever, because it determines which input to look for based off the player tag.
        /// 
        /// InputManager update is set to run before anything else.
        /// </summary>
        void Update()
        {
            // Clear rumble
            for (int i = 0; i < ms_playerTags.Length; ++i)
            {
                GamePad.SetVibration((PlayerIndex)i, 0, 0);
            }

            #region Axis Input
            // Left Stick Input	- One Stick to Determine Movement
            float upDown = Input.GetAxis(gameObject.tag + "Vertical");
            float leftRight = Input.GetAxis(gameObject.tag + "Horizontal");

            // Right Stick Input - Probably for Camera Control
            float camUpDown = Input.GetAxis(gameObject.tag + "CamVertical");
            float camLeftRight = Input.GetAxis(gameObject.tag + "CamHorizontal");

            // Trigger Input - For acceleration
            float triggers = -Input.GetAxis(gameObject.tag + "Triggers");

            // DPad Input - For menus and such
            //float dPadUpDown = -Input.GetAxis(gameObject.tag + "DPadVertical");
            //float dPadLeftRight = Input.GetAxis(gameObject.tag + "DPadHorizontal");
            #endregion

            #region Button Input
            // Bumpers
            bool bumperLeft = Input.GetButton(gameObject.tag + "BumperLeft");
            bool bumperRight = Input.GetButton(gameObject.tag + "BumperRight");

            // Face Buttons
            bool faceDown = Input.GetButton(gameObject.tag + "FaceDown");
            bool faceLeft = Input.GetButton(gameObject.tag + "FaceLeft");
            bool faceRight = Input.GetButton(gameObject.tag + "FaceRight");
            bool faceUp = Input.GetButton(gameObject.tag + "FaceUp");

            // Start and Select
            //bool select = Input.GetButton(gameObject.tag + "Select");
            //bool start = Input.GetButton(gameObject.tag + "Start");

            // Analogue Stick Clicks
            bool clickLeft = Input.GetButton(gameObject.tag + "ClickLeft");
            bool clickRight = Input.GetButton(gameObject.tag + "ClickRight");
            #endregion

            // Send variable data to individual scripts
            m_rouletteControl.PlayerInput(faceDown, faceUp);	// Use the face button inputs to Stop/Start the roulette wheel
            m_standardControl.PlayerInputs(upDown, leftRight, camUpDown, camLeftRight, triggers, bumperLeft, bumperRight, faceUp, faceDown, faceLeft, faceRight);
            m_fireshipControl.PlayerFireshipInputs(upDown, leftRight);
            m_rotateCam.PlayerInputs(camUpDown, camLeftRight, triggers, faceDown, bumperLeft, bumperRight, clickLeft, clickRight);
            m_shuntingControl.PlayerInputs(bumperLeft, bumperRight);
        }

        /// <summary>
        /// Checks whether any input has been pressed for the input player.
        /// </summary>
        /// <param name="a_playerTag">Input player's tag.</param>
        /// <returns>True if any button was pressed, false if not.</returns>
        public static bool GetAnyButtonDown(string a_playerTag)
        {
            // Check if any button is down
            if (Input.GetButton(a_playerTag + "Start") ||
                Input.GetButton(a_playerTag + "Select") ||
                Input.GetButton(a_playerTag + "FaceDown") ||
                Input.GetButton(a_playerTag + "FaceUp") ||
                Input.GetButton(a_playerTag + "FaceLeft") ||
                Input.GetButton(a_playerTag + "FaceRight") ||
                Input.GetButton(a_playerTag + "BumperLeft") ||
                Input.GetButton(a_playerTag + "BumperRight") ||
                Input.GetButton(a_playerTag + "ClickLeft") ||
                Input.GetButton(a_playerTag + "ClickRight") ||
                Mathf.Abs(Input.GetAxisRaw(a_playerTag + "Triggers")) >= 0.25f ||
                Mathf.Abs(Input.GetAxisRaw(a_playerTag + "Horizontal")) >= 0.25f ||
                Mathf.Abs(Input.GetAxisRaw(a_playerTag + "Vertical")) >= 0.25f ||
                Mathf.Abs(Input.GetAxisRaw(a_playerTag + "CamHorizontal")) >= 0.25f ||
                Mathf.Abs(Input.GetAxisRaw(a_playerTag + "CamVertical")) >= 0.25f)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Makes the input controller vibrate.
        /// </summary>
        /// <param name="a_playerIndex">Player tag. E.g. "Player1_"</param>
        /// <param name="a_motorLeft">Vibration value for the left controller motor.</param>
        /// <param name="a_motorRight">Vibration value for the right controller motor.</param>
        public static void SetControllerVibrate(string a_playerTag, float a_motorLeft, float a_motorRight)
        {
            // Find the player of the input tag
            for (int i = 0; i < ms_playerTags.Length; ++i)
            {
                if (ms_playerTags[i].CompareTo(a_playerTag) == 0)
                {
                    // Apply the vibration
                    GamePad.SetVibration((PlayerIndex) i, a_motorLeft, a_motorRight);
                    break;
                }
            }
        }
    } 
}

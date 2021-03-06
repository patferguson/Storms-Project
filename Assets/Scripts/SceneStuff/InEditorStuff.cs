﻿/**
 * File: InEditorStuff.cs
 * Author: Rowan Donaldson
 * Maintainer: Patrick Ferguson
 * Created: 12/08/2015
 * Copyright: (c) 2015 Team Storms, All Rights Reserved.
 * Description: Editor-only scripts. Everything here is only for ease of access, and should only effect stuff in the editor.
 *              We should delete the Entire 'InEditor' branch of the airshipGameobject before master build.
 **/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ProjectStorms
{
    /// <summary>
    /// Everything here is only for ease of access, and should only effect stuff in the editor.
    /// We should delete the Entire 'InEditor' branch of the airshipGameobject before master build.
    /// </summary>
    public class InEditorStuff : MonoBehaviour
    {

        public Renderer myRenderer;
        public Renderer myOtherRenderer;
        private Color m_playerColor = Color.red;

        public GameObject airshipTopOfHierachy;
        public GameObject canvasChild;
        private Text m_canvasText;

        // Cached variables
        StateManager m_stateManager;

		//Private check for game type
		private bool amIATeamGame;
		private Color alphaTeamColour = Color.magenta;
        private Color omegaTeamColour = Color.magenta;

        void Awake()
        {
            m_canvasText = canvasChild.GetComponentInChildren<Text>();
            m_stateManager = airshipTopOfHierachy.GetComponent<StateManager>();
        }


        void Update()
        {
            if (Application.isEditor)
            {
                // Explain game states
                m_canvasText.text = ("State: " + (m_stateManager.GetPlayerState()));
            }
            else
            {
                m_canvasText.text = "";
            }

			//Determine which colours to use
			amIATeamGame = ScoreManager.teamGame;

			if (!amIATeamGame)
			{
				SetColour();
			}
			else
			if (amIATeamGame)
			{
				SetTeamColour(alphaTeamColour, omegaTeamColour);
			}

        }

		void SetTeamColour(Color alpha, Color omega)
		{
			// Set team color
			if (myRenderer.enabled == true)
			{
				myRenderer.material.color = m_playerColor;
				
				string myTag = tag;
				if (myTag == "Player1_")
				{
					m_playerColor = Color.red;
				}
				
				if (myTag == "Player2_")
				{
					m_playerColor = Color.red;
				}
				
				if (myTag == "Player3_")
				{
					m_playerColor = Color.black;
				}
				
				if (myTag == "Player4_")
				{
					m_playerColor = Color.black;
				}
				
				//In case of a second render object
				if (myOtherRenderer != null)
				{
					if (myOtherRenderer.enabled == true)
					{
						myOtherRenderer.material.color = m_playerColor;
					}
				}
			}
		}


		void SetColour()
		{
			// Set color
			if (myRenderer.enabled == true)
			{
				myRenderer.material.color = m_playerColor;
				
				string myTag = tag;
				if (myTag == "Player1_")
				{
					m_playerColor = Color.red;
				}
				
				if (myTag == "Player2_")
				{
					m_playerColor = Color.blue;
				}
				
				if (myTag == "Player3_")
				{
					m_playerColor = Color.green;
				}
				
				if (myTag == "Player4_")
				{
					m_playerColor = Color.yellow;
				}
				
				//In case of a second render object
				if (myOtherRenderer != null)
				{
					if (myOtherRenderer.enabled == true)
					{
						myOtherRenderer.material.color = m_playerColor;
					}
				}
			}
		}
    } 
}

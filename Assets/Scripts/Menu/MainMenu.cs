﻿/**
 * File: MainMenu.cs
 * Author: Andrew Barbour
 * Maintainers: Andrew Barbour
 * Created: 25/09/2015
 * Copyright: (c) 2015 Team Storms, All Rights Reserved.
 * Description: 
 **/

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

namespace ProjectStorms
{
	public class MainMenu : MonoBehaviour 
	{
        public bool isTeamsGameMode
        {
            get
            {
                return m_gameMode == Gamemode.TEAMS;
            }
        }

#if UNITY_EDITOR
        [Header("Editor Only")]
        public bool overrideLevel = false;
        public string overrideLevelName;
#endif

        private Gamemode m_gameMode = Gamemode.NONE;
        private string m_mapName;

        public void ExitGame()
        {
            Application.Quit();

#if UNITY_EDITOR
            // Stop editor playing, to simulate game exiting in editor
            EditorApplication.ExecuteMenuItem("Edit/Play");
            return;
#endif
        }

        public void SetMap(string a_name)
        {
            m_mapName = a_name;
        }

        public void SetGamemodeTeams()
        {
            m_gameMode = Gamemode.TEAMS;
        }

        public void SetGamemodeFFA()
        {
            m_gameMode = Gamemode.FFA;
        }

        public void StartMatch()
        {
            if (m_mapName == "" ||
                m_gameMode == Gamemode.NONE)
            {
                Debug.LogError("Map and gamemode misconfiguration... unable to start game");
                return;
            }

#if UNITY_EDITOR
            if (overrideLevel && m_mapName != "")
            {
                m_mapName = overrideLevelName;
                Debug.LogWarning(string.Format("Loading overrided level ({0})...", m_mapName));
            }
#endif

            // Set gamemode within level settings
            LevelSettings.Instance.gamemode = m_gameMode;

            Application.LoadLevel(m_mapName);
        }

        public void LoadLevel(string a_levelName)
        {
            Application.LoadLevel(a_levelName);
        }
	}
}

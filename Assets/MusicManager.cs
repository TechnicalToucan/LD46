﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string MenuMusicEvent;

    [FMODUnity.EventRef]
    public string GameMusicEvent;

    private FMOD.Studio.EventInstance menuMusicInstance;
    private FMOD.Studio.EventInstance gameLoopInstance;

    private bool playingMenuMusic;
    private bool playingGameMusic;

    public void PlayMenuMusic()
    {
        if (playingMenuMusic)
        {
            return;
        }

        playingMenuMusic = true;

        menuMusicInstance = FMODUnity.RuntimeManager.CreateInstance(MenuMusicEvent);
        menuMusicInstance.start();
        menuMusicInstance.setTimelinePosition(22000);
    }

    public void StopMenuMusic()
    {
        menuMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        playingMenuMusic = false;
    }

    public void PlayGameLoopMusic()
    {
        if (playingGameMusic)
        {
            return;
        }

        playingGameMusic = true;

        gameLoopInstance = FMODUnity.RuntimeManager.CreateInstance(GameMusicEvent);
        gameLoopInstance.start();
        
    }

    public void StopGameLoopMusic()
    {
        gameLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        playingGameMusic = false;
    }

    void OnApplicationQuit()
    {
        if (menuMusicInstance.isValid())
        {
            menuMusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            menuMusicInstance.release();
            menuMusicInstance.clearHandle();
        }

        if (gameLoopInstance.isValid())
        {
            gameLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            gameLoopInstance.release();
            gameLoopInstance.clearHandle();
        }
    }

    void Awake()
    {
        Service.Music = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Service.Music = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
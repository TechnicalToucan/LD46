﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
using Time = UnityEngine.Time;

public class FlowManager : MonoBehaviour
{
#if DEBUG && UNITY_EDITOR
    public bool SkipTitleScreen;
#endif

    public CanvasGroup GameUICanvasGroup;

    [Serializable]
    public struct RandomStatInt
    {
        public int Base;
        public int Min;
        public int Max;

        public int Lowest;
        public int Highest;

        public int Generate(int mod = 1)
        {
            var ret = Base;

            for (int i = 0; i < mod; i++)
            {
                ret += Random.Range(Min, Max);
            }

            return Mathf.Clamp(ret, Lowest, Highest);
        }
    }
    [Serializable]
    public struct RandomStatFloat
    {
        public float Base;
        public float Min;
        public float Max;

        public float Lowest;
        public float Highest;

        public float Generate(int mod = 1)
        {
            var ret = Base;

            for (int i = 0; i < mod; i++)
            {
                ret += Random.Range(Min, Max);
            }
            
            return Mathf.Clamp(ret, Lowest, Highest);
        }
    }


    public int MaxRaceTime;
    public int MaxEnemies;
    public int MaxObstacles;

    public int BaseRaceTimer;
    public RandomStatInt TimeIncreasePerCheckpoint;

    public int FirstRoundEnemySpawnDelay;
    public int FirstRoundObstacleSpawnDelay;

    public RandomStatInt TimeBetweenEnemySpawns;
    public RandomStatInt EnemySpawnTimeChangePerCheckpoint;

    public RandomStatInt TimeBetweenObstacleSpawns;
    public RandomStatInt ObstacleSpawnTimeChangePerCheckpoint;

    public float InitialFadeInTime = 1;

    /// <summary>
    /// Canvas group for the initial fade in on game start
    /// </summary>
    private CanvasGroup gameStartFadeInGroup;

    private EndScreen endScreenRef = null;

    void Awake()
    {
        Service.Flow = this;
        gameStartFadeInGroup = GetComponentInChildren<CanvasGroup>();
        Assert.IsNotNull(gameStartFadeInGroup, "Flow manager did not find a canvas group component in a child. ERROR.");
    }

    // Start is called before the first frame update
    void Start()
    {
        Service.Flow = this;
        gameStartFadeInGroup.alpha = 1f;
        GameUICanvasGroup.alpha = 0f;

#if DEBUG && UNITY_EDITOR
        if (SkipTitleScreen)
        {
            GameUICanvasGroup.alpha = 1f;
            gameStartFadeInGroup.alpha = 0f;
            SetupGameStart();
            return;
        }
#endif
        var title = (GameObject)Instantiate(Service.Prefab.TitleScreen);
        var titleScreen = title.GetComponent<TitleScreen>();

        Assert.IsNotNull(titleScreen);

        // Set up the start of the game and alpha out the title screen fader
        titleScreen.OnTitleFadedOut += (sender, args) => SetupGameStart();
        StartCoroutine(FadeIntoGameAtStart());
    }

    // Update is called once per frame
    void Update()
    {
        if (endScreenRef && endScreenRef.ReadyToFadeBackIn)
        {
            //Destroy gameplay manager so SetupGameStart can make a new one
            
            Service.Game.Shutdown();
            Destroy(Service.Game.gameObject);

            Service.Game = null;

            endScreenRef.FadeBackIn();
            SetupGameStart();
        }
    }


    private void SetupGameStart()
    {
        endScreenRef = null;

        var gameplay = (GameObject)Instantiate(Service.Prefab.GameplayManager);
        var gameplayRef = gameplay.GetComponent<GameplayManager>();

        Assert.IsNotNull(gameplayRef);

        gameplayRef.OnGameFinished += (o, eventArgs) =>
        {
            var end = (GameObject)Instantiate(Service.Prefab.EndScreen);
            var endScreen = end.GetComponent<EndScreen>();

            Assert.IsNotNull(endScreen);

            endScreenRef = endScreen;
        };
    }

    private IEnumerator FadeIntoGameAtStart()
    {
        yield return new WaitForSeconds(1f);

        while (gameStartFadeInGroup.alpha > 0)
        {
            gameStartFadeInGroup.alpha -= InitialFadeInTime * Time.deltaTime;
            yield return null;
        }
    }
}

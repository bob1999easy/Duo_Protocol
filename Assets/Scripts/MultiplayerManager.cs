using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            isInitialized = true;

            Debug.Log("Unity Services initialized!");
            Debug.Log("Player ID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.LogError("Unity Services initialization failed!");
            Debug.LogException(e);
        }
    }

    public async void CreateHostSession()
    {
        if (!isInitialized)
        {
            Debug.LogError("Unity Services are not initialized yet!");
            return;
        }

        try
        {
            var options = new SessionOptions
            {
                MaxPlayers = 2
            }.WithRelayNetwork();

            var session = await MultiplayerService.Instance.CreateSessionAsync(options);

            Debug.Log("Session created!");
            Debug.Log("Session ID: " + session.Id);
            Debug.Log("Join Code: " + session.Code);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to create session!");
            Debug.LogException(e);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

using System.Threading.Tasks;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    private bool isInitialized = false;

    private NetworkStartUI networkStartUI;  // referamce script networkstartui


    // Sessions found by the server browser
    public List<ISessionInfo> availableSessions = new List<ISessionInfo>();

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
        networkStartUI = FindAnyObjectByType<NetworkStartUI>();  // find script 

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


    // HOST

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

            networkStartUI.HideCanvas(); // hide canvas in host mode after joining session 
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to create session!");
            Debug.LogException(e);
        }
    }


    // FIND SESSIONS

    public async Task FindSessions()
    {
        if (!isInitialized)
        {
            Debug.LogError("Unity Services are not initialized yet!");
            return;
        }

        try
        {
            QuerySessionsOptions options = new QuerySessionsOptions();

            QuerySessionsResults results =
                await MultiplayerService.Instance.QuerySessionsAsync(options);

            availableSessions.Clear();

            foreach (ISessionInfo session in results.Sessions)
            {
                availableSessions.Add(session);

                Debug.Log(
                    "Session found: " +
                    session.Id +
                    " | Max Players: " +
                    session.MaxPlayers
                );

                // old code with current players info
                // Debug.Log(
                //     "Session found: " +
                //     session.Id +
                //     " | Players: " +
                //     session.CurrentPlayers +
                //     "/" +
                //     session.MaxPlayers
                // );

            }

            Debug.Log("Found " + availableSessions.Count + " sessions.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to find sessions!");
            Debug.LogException(e);
        }
    }


    // JOIN SESSION

    public async void JoinSession(ISessionInfo session)
    {
        if (!isInitialized)
        {
            Debug.LogError("Unity Services are not initialized yet!");
            return;
        }

        try
        {
            Debug.Log("Joining session: " + session.Id);

            await MultiplayerService.Instance.JoinSessionByIdAsync(session.Id);

            Debug.Log("Successfully joined session!");

            networkStartUI.HideCanvas(); // hide canvas in cliend mode after joining session
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to join session!");
            Debug.LogException(e);
        }
    }
}
using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class MultiplayerManager : MonoBehaviour
{
    private async void Start()
    {
        try
        {
            // Initialize Unity Gaming Services
            await UnityServices.InitializeAsync();

            // Sign in anonymously
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("Unity Services initialized!");
            Debug.Log("Player ID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.LogError("Unity Services initialization failed!");
            Debug.LogException(e);
        }
    }
}
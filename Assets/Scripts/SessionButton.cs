using UnityEngine;
using Unity.Services.Multiplayer;
using TMPro;

public class SessionButton : MonoBehaviour
{
    public TMP_Text gameName;
    public TMP_Text playerCount;
    
    private ISessionInfo session;

    public void Setup(ISessionInfo sessionInfo, int gameNumber)
    {
        session = sessionInfo;

        gameName.text = "Game " + gameNumber;
        playerCount.text = "Max: " + session.MaxPlayers;
    }

    public void OnJoinButton()
    {
        MultiplayerManager.Instance.JoinSession(session);
    }
}
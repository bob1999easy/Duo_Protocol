using UnityEngine;
using Unity.Services.Multiplayer;

public class SessionButton : MonoBehaviour
{
    private ISessionInfo session;

    public void Setup(ISessionInfo sessionInfo)
    {
        session = sessionInfo;
    }

    public void OnJoinButton()
    {
        MultiplayerManager.Instance.JoinSession(session);
    }
}
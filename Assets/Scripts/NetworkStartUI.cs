using UnityEngine;
using Unity.Netcode;

public class NetworkStartUI : MonoBehaviour
{
    void OnGUI()
    {
        float w = 200f;
        float h = 40f;
        float x = 10f;
        float y = 10f;

        if (GUI.Button(new Rect(x, y, w, h), "Host"))
        {
            MultiplayerManager.Instance.CreateHostSession();
        }
    }
}
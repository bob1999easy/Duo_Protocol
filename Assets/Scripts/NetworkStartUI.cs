using UnityEngine;

public class NetworkStartUI : MonoBehaviour
{
    private bool showServerBrowser = false;

    void OnGUI()
    {
        float w = 250f;
        float h = 45f;
        float x = 20f;
        float y = 20f;

        // MAIN MENU

        if (!showServerBrowser)
        {
            if (GUI.Button(new Rect(x, y, w, h), "HOST"))
            {
                MultiplayerManager.Instance.CreateHostSession();
            }

            if (GUI.Button(new Rect(x, y + h + 10, w, h), "JOIN"))
            {
                showServerBrowser = true;

                MultiplayerManager.Instance.FindSessions();
            }
        }

        // SERVER BROWSER

        else
        {
            GUI.Label(new Rect(x, y, w, h), "AVAILABLE GAMES");

            float currentY = y + 50f;

            // Get visable sessions
            var sessions = MultiplayerManager.Instance.availableSessions;

            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];

                string sessionName = "Game " + (i + 1);

                string playerCount = "Max: " + session.MaxPlayers;

                // old code with current players info
                // string playerCount =
                //     session.CurrentPlayers +
                //     "/" +
                //     session.MaxPlayers;

                GUI.Label(new Rect(x, currentY, 120f, h), sessionName);

                GUI.Label(new Rect(x + 120f, currentY, 70f, h), playerCount);

                // bool isFull = session.CurrentPlayers >= session.MaxPlayers; //no info for numer of current players

                // if (!isFull)
                // {
                //     if (GUI.Button(
                //         new Rect(x + 190f, currentY, 100f, h),
                //         "JOIN"))
                //     {
                //         MultiplayerManager.Instance.JoinSession(session);
                //     }
                // }
                // else
                // {
                //     GUI.Label(
                //         new Rect(x + 190f, currentY, 100f, h),
                //         "FULL"
                //     );
                // }

                if (GUI.Button(new Rect(x + 150f, currentY, 100f, h), "JOIN"))
                {
                    MultiplayerManager.Instance.JoinSession(session);
                }

                currentY += h + 10f;
            }

            // REFRESH

            if (GUI.Button(new Rect(x, currentY + 10f, w, h), "REFRESH"))
            {
                MultiplayerManager.Instance.FindSessions();
            }

            // BACK

            if (GUI.Button(new Rect(x, currentY + h + 20f, w, h), "BACK"))
            {
                showServerBrowser = false;
            }
        }
    }
}
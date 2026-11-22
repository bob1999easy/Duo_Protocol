using System.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text objectiveText;   // "KILL THEM ALL!"
    [SerializeField] private TMP_Text timerText;       // timer
    [SerializeField] private TMP_Text enemiesLeftText; // Enemies left: xxx
    [SerializeField] private TMP_Text resultText;      // YOU WON! + time
    [SerializeField] private TMP_Text hintText;        // [R] Restart and [Q] Quit

    private NetworkVariable<int> enemiesLeft = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<double> startTime = new NetworkVariable<double>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<float> finalTime = new NetworkVariable<float>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<bool> gameOver = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool IsGameOver => gameOver.Value;

    private bool Started => startTime.Value > 0;

    private float Elapsed
    {
        get
        {
            if (gameOver.Value) return finalTime.Value;
            if (!Started) return 0f;
            return (float)(NetworkManager.ServerTime.Time - startTime.Value);
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    public override void OnDestroy()
    {
        if (Instance == this) Instance = null;
        base.OnDestroy();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            StartCoroutine(InitRoutine());
    }

    private IEnumerator InitRoutine()
    {
        // wait one frame so all enemies from the scene are spawned
        yield return null;

        enemiesLeft.Value = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length;
        startTime.Value = NetworkManager.ServerTime.Time;
    }

    // called by EnemyHealth on the server
    public void EnemyDied()
    {
        if (!IsServer || gameOver.Value)
            return;

        enemiesLeft.Value--;

        if (enemiesLeft.Value <= 0)
        {
            finalTime.Value = Elapsed;
            gameOver.Value = true;
        }
    }

    private void Update()
    {
        UpdateUI();

        if (!gameOver.Value || Keyboard.current == null)
            return;

        // only the host can restart (server loads the scene for everyone)
        if (IsServer && Keyboard.current.rKey.wasPressedThisFrame)
        {
            NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // for in engine use
#else
            Application.Quit();
#endif
        }
    }

    private void UpdateUI()
    {
        if (timerText != null)
            timerText.text = FormatTime(Elapsed);

        if (enemiesLeftText != null)
            enemiesLeftText.text = "Enemies left: " + enemiesLeft.Value;

        bool over = gameOver.Value;

        if (objectiveText != null) objectiveText.gameObject.SetActive(!over);
        if (resultText != null) resultText.gameObject.SetActive(over);
        if (hintText != null) hintText.gameObject.SetActive(over);

        if (over)
        {
            if (resultText != null)
                resultText.text = "YOU WON!\nTime: " + FormatTime(finalTime.Value);

            if (hintText != null)
                hintText.text = IsServer
                    ? "[R] Restart    [Q] Quit"
                    : "Waiting for host to restart    [Q] Quit";
        }
    }

    private string FormatTime(float t)
    {
        int minutes = (int)(t / 60f);
        float seconds = t % 60f;
        return minutes.ToString("00") + ":" + seconds.ToString("00.0");
    }
}
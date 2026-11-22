using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private TMP_Text timerText;       // 00:12.3
    [SerializeField] private TMP_Text enemiesLeftText; // Enemies left: 5
    [SerializeField] private TMP_Text resultText;      // YOU WON! / YOU LOST!
    [SerializeField] private TMP_Text hintText;        // [R] Restart  [Q] Quit

    [Header("Spawn")]
    [SerializeField] private Transform[] spawnPoints;  // optional: one per player

    private NetworkVariable<int> enemiesLeft = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<double> startTime = new NetworkVariable<double>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<float> finalTime = new NetworkVariable<float>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<bool> gameOver = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<bool> won = new NetworkVariable<bool>(
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
        {
            StartCoroutine(InitRoutine());

            // when all players finished loading the scene, reset them
            NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoadCompleted;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager != null && NetworkManager.SceneManager != null)
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoadCompleted;
    }

    private IEnumerator InitRoutine()
    {
        // wait one frame so all enemies from the scene are spawned
        yield return null;

        enemiesLeft.Value = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length;
        startTime.Value = NetworkManager.ServerTime.Time;
    }

    private void OnSceneLoadCompleted(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        ResetPlayers();
    }

    // player objects survive scene changes, so health (and position) must be reset by hand
    private void ResetPlayers()
    {
        if (!IsServer)
            return;

        int index = 0;

        foreach (var client in NetworkManager.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            PlayerHealth hp = client.PlayerObject.GetComponent<PlayerHealth>();

            if (hp == null)
                continue;

            bool hasSpawn = spawnPoints != null && spawnPoints.Length > 0;
            Transform spawn = hasSpawn ? spawnPoints[index % spawnPoints.Length] : null;

            hp.ResetPlayer(
                hasSpawn ? spawn.position : Vector3.zero,
                hasSpawn ? spawn.rotation : Quaternion.identity,
                hasSpawn
            );

            index++;
        }
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
            won.Value = true;
            gameOver.Value = true;
        }
    }

    // called by PlayerHealth on the server
    public void CheckAllPlayersDead()
    {
        if (!IsServer || gameOver.Value)
            return;

        foreach (var client in NetworkManager.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            PlayerHealth hp = client.PlayerObject.GetComponent<PlayerHealth>();

            if (hp != null && !hp.IsDead)
                return; // someone is still alive
        }

        finalTime.Value = Elapsed;
        won.Value = false;
        gameOver.Value = true;
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
            UnityEditor.EditorApplication.isPlaying = false;
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
            {
                resultText.text = won.Value
                    ? "YOU WON!\nTime: " + FormatTime(finalTime.Value)
                    : "YOU LOST!\nEnemies left: " + enemiesLeft.Value;
            }

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
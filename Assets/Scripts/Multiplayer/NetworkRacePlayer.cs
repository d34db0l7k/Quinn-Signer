using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;

namespace Multiplayer
{
    public class NetworkRacePlayer : NetworkBehaviour
    {
        [Header("Movement")]
        public float boostAmount = 4f;
        public float visualMoveLerp = 10f;

        [Header("References")]
        public Transform shipVisual;
        public Transform cameraFollowTarget;
        public SwipeInputReader swipeInputReader;
        public TMP_Text nameText;

        public Transform CameraFollowTarget => cameraFollowTarget;

        public NetworkVariable<Vector3> SpawnPosition = new(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<float> Progress = new(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<int> CurrentPrompt = new(
            0,
            NetworkVariableReadPermission.Owner,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<FixedString64Bytes> PlayerName = new(
            "Player",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private RaceHUD _localHud;

        public override void OnNetworkSpawn()
        {
            CurrentPrompt.OnValueChanged += OnPromptChanged;
            PlayerName.OnValueChanged += OnPlayerNameChanged;

            if (IsOwner && swipeInputReader != null)
                swipeInputReader.OnSwipeDetected += HandleLocalSwipe;

            UpdateNameUI(PlayerName.Value.ToString());

            if (IsOwner)
            {
                _localHud = FindObjectOfType<RaceHUD>();
                if (_localHud != null)
                    _localHud.SetPrompt(CurrentPrompt.Value);
            }

            if (IsServer)
            {
                if (string.IsNullOrWhiteSpace(PlayerName.Value.ToString()) || PlayerName.Value.ToString() == "Player")
                    PlayerName.Value = $"Player {OwnerClientId + 1}";
            }
        }

        public override void OnNetworkDespawn()
        {
            CurrentPrompt.OnValueChanged -= OnPromptChanged;
            PlayerName.OnValueChanged -= OnPlayerNameChanged;

            if (IsOwner && swipeInputReader != null)
                swipeInputReader.OnSwipeDetected -= HandleLocalSwipe;
        }

        private void Update()
        {
            Vector3 targetPos = new Vector3(
                SpawnPosition.Value.x,
                SpawnPosition.Value.y,
                SpawnPosition.Value.z + Progress.Value
            );

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * visualMoveLerp);
        }

        private void HandleLocalSwipe(SwipeDirection dir)
        {
            if (!IsOwner) return;
            if (RaceManager.Instance == null) return;
            if (!RaceManager.Instance.RaceStarted.Value) return;
            if (RaceManager.Instance.RaceFinished.Value) return;

            SubmitSwipeServerRpc((int)dir);
        }

        [ServerRpc]
        private void SubmitSwipeServerRpc(int swipeDir)
        {
            if (RaceManager.Instance == null) return;
            if (!RaceManager.Instance.RaceStarted.Value) return;
            if (RaceManager.Instance.RaceFinished.Value) return;

            if (swipeDir == CurrentPrompt.Value)
            {
                Progress.Value += boostAmount;
                RaceManager.Instance.CheckForWinner(this);

                if (!RaceManager.Instance.RaceFinished.Value)
                    ServerGenerateNextPrompt();
            }
        }

        public void ServerGenerateNextPrompt()
        {
            if (!IsServer) return;
            CurrentPrompt.Value = Random.Range(0, 4);
        }

        public void SetSpawnFromServer(Vector3 pos, Quaternion rot)
        {
            if (!IsServer) return;

            SpawnPosition.Value = pos;
            transform.SetPositionAndRotation(pos, rot);
        }

        private void OnPromptChanged(int oldValue, int newValue)
        {
            if (!IsOwner) return;
            if (_localHud != null)
                _localHud.SetPrompt(newValue);
        }

        private void OnPlayerNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
        {
            UpdateNameUI(newValue.ToString());
        }

        private void UpdateNameUI(string newName)
        {
            if (nameText != null)
                nameText.text = newName;
        }

        [ServerRpc]
        public void SubmitNameServerRpc(string chosenName)
        {
            if (string.IsNullOrWhiteSpace(chosenName))
                chosenName = $"Player {OwnerClientId + 1}";

            if (chosenName.Length > 20)
                chosenName = chosenName.Substring(0, 20);

            PlayerName.Value = chosenName;
        }
    }
}
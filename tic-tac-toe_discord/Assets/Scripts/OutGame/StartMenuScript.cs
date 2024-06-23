using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class StartMenuScript : MonoBehaviourPunCallbacks
{
    public TMP_InputField nameInputField;
    public Button startButton;
    private bool isMatching = false;

    void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    public void OnStartButtonClicked()
    {
        if (!string.IsNullOrEmpty(nameInputField.text))
        {
            PhotonNetwork.NickName = nameInputField.text;
            PhotonNetwork.ConnectUsingSettings();
            startButton.interactable = false;
            startButton.GetComponentInChildren<TextMeshProUGUI>().text = "マッチング待機中";
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.LoadLevel("InGameScene");
        }
        else
        {
            isMatching = true;
            startButton.GetComponentInChildren<TextMeshProUGUI>().text = "マッチング待機中";
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && isMatching)
        {
            PhotonNetwork.LoadLevel("InGameScene");
        }
    }
}
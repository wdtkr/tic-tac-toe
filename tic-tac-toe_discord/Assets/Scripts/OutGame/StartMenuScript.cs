using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using DG.Tweening;

public class StartMenuScript : MonoBehaviourPunCallbacks
{
    public TMP_InputField nameInputField;
    public Button startButton;
    public TextMeshProUGUI startButtonText;
    private bool isAnimating = false;
    private string[] waitingTexts = { "マッチング待機中", "マッチング待機中・", "マッチング待機中・・", "マッチング待機中・・・", "マッチング待機中" };

    private void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        ResetUI();
    }

    private void ResetUI()
    {
        startButtonText.text = "スタート！";
        startButton.interactable = true;
        isAnimating = false;
        DOTween.Kill(startButtonText); // アニメーションを停止して元の状態に戻す

        // Ensure we are not connected to any server
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public void OnStartButtonClicked()
    {
        if (!string.IsNullOrEmpty(nameInputField.text))
        {
            PhotonNetwork.NickName = nameInputField.text;
            PhotonNetwork.ConnectUsingSettings();
            startButton.interactable = false;
            AnimateButtonText(); // アニメーションを開始するメソッドを呼び出す
        }
    }

    private void AnimateButtonText()
    {
        if (isAnimating) return;

        isAnimating = true;

        // カウント用の変数
        int textIndex = 0;

        // カスタムアニメーションの設定
        DOTween.To(() => textIndex, x => {
            textIndex = x;
            // テキストを更新する
            startButtonText.text = waitingTexts[textIndex % waitingTexts.Length];
        }, waitingTexts.Length - 1, 2.5f).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        ResetUI();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
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
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.LoadLevel("InGameScene");
        }
    }
}
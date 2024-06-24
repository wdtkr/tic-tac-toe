using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
public class GameScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public List<Button> buttonList;
    public TextMeshProUGUI turnDisplay;
    public TextMeshProUGUI turnCountDisplay;
    private int[,] squareStatus = new int[3, 3];
    private Button[,] buttons = new Button[3, 3];
    private int currentPlayer = 1; // 1 for user1, 2 for user2
    private int turnCount = 0;
    private List<GameObject> user1Squares = new List<GameObject>();
    private List<GameObject> user2Squares = new List<GameObject>();
    private bool isMyTurn = false;
    private string user1Name = "";
    private string user2Name = "";
    private const int NoSlot = -1;
    void Start()
    {
        // Initial setup for the players
        SetUpPlayers();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int index = i * 3 + j;
                Button button = buttonList[index];
                int x = i;
                int y = j;
                button.onClick.AddListener(() => OnSquareClicked(x, y));
                buttons[i, j] = button;
                squareStatus[i, j] = 0;
            }
        }
        UpdateTurnCountDisplay();
    }
    private void SetUpPlayers()
    {
        if (PhotonNetwork.PlayerList.Length == 2)
        {
            var player1 = PhotonNetwork.PlayerList[0];
            var player2 = PhotonNetwork.PlayerList[1];
            user1Name = player1.NickName;
            user2Name = player2.NickName;
            if (PhotonNetwork.IsMasterClient)
            {
                currentPlayer = Random.Range(1, 3);
                photonView.RPC("SyncInitialPlayer", RpcTarget.AllBuffered, currentPlayer);
            }
            else
            {
                // Request to sync initial player information
                photonView.RPC("RequestSyncInitialPlayer", RpcTarget.MasterClient);
            }
        }
    }
    [PunRPC]
    private void RequestSyncInitialPlayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncInitialPlayer", RpcTarget.AllBuffered, currentPlayer);
        }
    }
    [PunRPC]
    private void SyncInitialPlayer(int player)
    {
        currentPlayer = player;
        UpdateTurnStatus();
    }
    private void OnSquareClicked(int x, int y)
    {
        if (!isMyTurn || squareStatus[x, y] != 0) return;
        photonView.RPC("OnSquareConfirmed", RpcTarget.All, x, y, currentPlayer);
    }
    [PunRPC]
    private void OnSquareConfirmed(int x, int y, int player)
    {
        if (squareStatus[x, y] != 0) return;
        squareStatus[x, y] = player;
        buttons[x, y].image.color = (player == 1) ? Color.blue : Color.green;
        AddPlayerSquare(player, x, y);
        var winningButtons = CheckWinCondition(player);
        if (winningButtons.Count > 0)
        {
            turnDisplay.text = $"{(player == 1 ? user1Name : user2Name)}の勝利";
            StopAllBlinking();
            HighlightWinningButtons(winningButtons);
            DisableAllButtons();
            StartCoroutine(ReturnToStartMenu());
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                currentPlayer = (currentPlayer == 1) ? 2 : 1;
                turnCount++;
                photonView.RPC("SyncChangeTurn", RpcTarget.All, currentPlayer, turnCount);
            }
        }
    }
    [PunRPC]
    private void SyncChangeTurn(int player, int turn)
    {
        currentPlayer = player;
        turnCount = turn;
        UpdateTurnStatus();
    }
    private void AddPlayerSquare(int player, int x, int y)
    {
        if (player == 1)
        {
            user1Squares.Add(buttons[x, y].gameObject);
            if (user1Squares.Count > 3)
            {
                RemoveOldestSquare(user1Squares, Color.blue);
            }
        }
        else
        {
            user2Squares.Add(buttons[x, y].gameObject);
            if (user2Squares.Count > 3)
            {
                RemoveOldestSquare(user2Squares, Color.green);
            }
        }
        if (player == currentPlayer) // Only the current player should blink their buttons
        {
            UpdateBlinkingButtons();
        }
    }
    private void RemoveOldestSquare(List<GameObject> squares, Color playerColor)
    {
        StopBlinking(squares[0].GetComponent<Button>(), playerColor);
        GameObject first = squares[0];
        string[] coordinates = first.name.Split('-')[1].ToCharArray().Select(c => c.ToString()).ToArray();
        squareStatus[int.Parse(coordinates[0]), int.Parse(coordinates[1])] = 0;
        first.GetComponent<Button>().image.color = Color.white;
        squares.RemoveAt(0);
    }
    private IEnumerator ReturnToStartMenu()
    {
        yield return new WaitForSeconds(5);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("StartMenuScene");
    }
    private void UpdateTurnDisplay()
    {
        string turnText = (currentPlayer == 1) ? $"{user1Name}のターン" : $"{user2Name}のターン";
        turnDisplay.text = turnText;
    }
    private void UpdateTurnCountDisplay()
    {
        turnCountDisplay.text = $"{turnCount}ターン目";
    }
    private void UpdateTurnStatus()
    {
        bool isMasterClientLocalPlayer = PhotonNetwork.LocalPlayer.IsMasterClient;
        isMyTurn = (isMasterClientLocalPlayer && currentPlayer == 1) || (!isMasterClientLocalPlayer && currentPlayer == 2);
        UpdateTurnDisplay();
        UpdateTurnCountDisplay();

        UpdateBlinkingButtons();
    }
    private void UpdateBlinkingButtons()
    {
        List<GameObject> opponentSquares = currentPlayer == 1 ? user2Squares : user1Squares;
        List<GameObject> mySquares = currentPlayer == 1 ? user1Squares : user2Squares;
        if (opponentSquares.Count == 3)
        {
            StopBlinking(opponentSquares[0].GetComponent<Button>(), currentPlayer == 1 ? Color.green : Color.blue);
        }
        if (mySquares.Count == 3)
        {
            BlinkButton(mySquares[0].GetComponent<Button>(), currentPlayer == 1 ? Color.blue : Color.green);
        }
    }
    private List<Button> CheckWinCondition(int player)
    {
        List<Button> winningButtons = new List<Button>();
        for (int i = 0; i < 3; i++)
        {
            if (squareStatus[i, 0] == player && squareStatus[i, 1] == player && squareStatus[i, 2] == player)
            {
                winningButtons.Add(buttons[i, 0]);
                winningButtons.Add(buttons[i, 1]);
                winningButtons.Add(buttons[i, 2]);
                return winningButtons;
            }
            if (squareStatus[0, i] == player && squareStatus[1, i] == player && squareStatus[2, i] == player)
            {
                winningButtons.Add(buttons[0, i]);
                winningButtons.Add(buttons[1, i]);
                winningButtons.Add(buttons[2, i]);
                return winningButtons;
            }
        }
        if (squareStatus[0, 0] == player && squareStatus[1, 1] == player && squareStatus[2, 2] == player)
        {
            winningButtons.Add(buttons[0, 0]);
            winningButtons.Add(buttons[1, 1]);
            winningButtons.Add(buttons[2, 2]);
            return winningButtons;
        }
        if (squareStatus[0, 2] == player && squareStatus[1, 1] == player && squareStatus[2, 0] == player)
        {
            winningButtons.Add(buttons[0, 2]);
            winningButtons.Add(buttons[1, 1]);
            winningButtons.Add(buttons[2, 0]);
            return winningButtons;
        }
        return winningButtons;
    }
    private void DisableAllButtons()
    {
        foreach (Button button in buttonList)
        {
            button.interactable = false;
        }
    }
    private void BlinkButton(Button button, Color playerColor)
    {
        button.image.DOColor(Color.white, 0.5f).SetLoops(-1, LoopType.Yoyo).OnStepComplete(() =>
        {
            button.image.color = playerColor;
        });
    }
    private void StopBlinking(Button button, Color playerColor)
    {
        button.image.DOKill();
        button.image.color = playerColor;
    }
    private void StopAllBlinking()
    {
        foreach (GameObject square in user1Squares)
        {
            Button btn = square.GetComponent<Button>();
            StopBlinking(btn, Color.blue);
        }
        foreach (GameObject square in user2Squares)
        {
            Button btn = square.GetComponent<Button>();
            StopBlinking(btn, Color.green);
        }
    }
    private void HighlightWinningButtons(IEnumerable<Button> winningButtons)
    {
        foreach (Button btn in winningButtons)
        {
            btn.image.DOColor(Color.yellow, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(FlattenArray(squareStatus));  // Send the flattened array
            stream.SendNext(currentPlayer);
            stream.SendNext(turnCount);
        }
        else
        {
            int[] receivedArray = (int[])stream.ReceiveNext();
            squareStatus = UnflattenArray(receivedArray, 3, 3);  // Convert back to multi-dimensional array
            currentPlayer = (int)stream.ReceiveNext();
            turnCount = (int)stream.ReceiveNext();
            UpdateTurnStatus();
        }
    }
    private int[] FlattenArray(int[,] array)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);
        int[] flat = new int[rows * cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                flat[i * cols + j] = array[i, j];
            }
        }
        return flat;
    }
    private int[,] UnflattenArray(int[] array, int rows, int cols)
    {
        int[,] multi = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                multi[i, j] = array[i * cols + j];
            }
        }
        return multi;
    }
}
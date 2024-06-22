using TMPro;

using UnityEngine;

public class GameScript : MonoBehaviour
{
    public GameObject[] squares;  // 9つのSquareオブジェクトを設定する
    public TextMeshProUGUI turnDisplay;  // ターン表示のテキストオブジェクト
    private int[,] _board = new int[3, 3];  // ゲーム盤の状態を保持する配列
    private int _currentTurn;  // 現在のターンのプレイヤー
    private int _turnCount;  // ターンの合計数
    private int[] _player1Squares = new int[3];  // ユーザー1が保持するSquareインデックス
    private int[] _player2Squares = new int[3];  // ユーザー2が保持するSquareインデックス
    void Start()
    {
        InitializeGame();
    }
    void InitializeGame()
    {
        // ゲーム盤を初期化
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                _board[i, j] = 0;
            }
        }

        // ターンを初期化
        _turnCount = 0;
        _currentTurn = Random.Range(1, 3);
        UpdateTurnDisplay();
        // Squareインデックスを初期化
        for (int i = 0; i < 3; i++)
        {
            _player1Squares[i] = -1;
            _player2Squares[i] = -1;
        }
    }
    void UpdateTurnDisplay()
    {
        if (_currentTurn == 1)
        {
            turnDisplay.text = "ユーザー1のターン";
        }
        else
        {
            turnDisplay.text = "ユーザー2のターン";
        }
    }

    public void OnSquareClicked(int index)
    {
        int row = index / 3;
        int col = index % 3;

        // 状態が0であるSquareのみクリック可能
        if (_board[row, col] != 0) return;

        // Squareの色を赤に変更
        squares[index].GetComponent<SpriteRenderer>().color = Color.red;

        // Squareの状態を自分のものに変更する
        _board[row, col] = _currentTurn;

        // プレイヤーのSquareの管理
        if (_currentTurn == 1)
        {
            ManagePlayerSquares(_player1Squares, index);
        }
        else
        {
            ManagePlayerSquares(_player2Squares, index);
        }

        // 勝利条件のチェック
        if (CheckVictory())
        {
            turnDisplay.text = (_currentTurn == 1) ? "ユーザー1の勝利" : "ユーザー2の勝利";
            return;
        }

        // ターンの変更
        _currentTurn = (_currentTurn == 1) ? 2 : 1;
        _turnCount++;
        UpdateTurnDisplay();
    }
    void ManagePlayerSquares(int[] playerSquares, int newSquareIndex)
    {
        for (int i = 0; i < 3; i++)
        {
            if (playerSquares[i] == -1)
            {
                playerSquares[i] = newSquareIndex;
                return;
            }
        }
        // 最初に設定したSquareの状態を0に戻す
        int oldestSquareIndex = playerSquares[0];
        _board[oldestSquareIndex / 3, oldestSquareIndex % 3] = 0;
        // 配列をシフトして上書き
        for (int i = 0; i < 2; i++)
        {
            playerSquares[i] = playerSquares[i + 1];
        }
        playerSquares[2] = newSquareIndex;
    }
    bool CheckVictory()
    {
        // 縦、横、斜めの勝利条件をチェック
        for (int i = 0; i < 3; i++)
        {
            if ((_board[i, 0] == _currentTurn && _board[i, 1] == _currentTurn && _board[i, 2] == _currentTurn) ||
                (_board[0, i] == _currentTurn && _board[1, i] == _currentTurn && _board[2, i] == _currentTurn))
            {
                return true;
            }
        }
        if ((_board[0, 0] == _currentTurn && _board[1, 1] == _currentTurn && _board[2, 2] == _currentTurn) ||
            (_board[0, 2] == _currentTurn && _board[1, 1] == _currentTurn && _board[2, 0] == _currentTurn))
        {
            return true;
        }
        return false;
    }
}
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class GameScript : MonoBehaviour
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
    private int previousButtonX = -1;
    private int previousButtonY = -1;
    void Start()
    {
        // Initialize the buttons and their listeners from the list
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int index = i * 3 + j;
                Button button = buttonList[index];
                int x = i, y = j;
                button.onClick.AddListener(() => OnSquareClicked(x, y));
                buttons[i, j] = button;
                squareStatus[i, j] = 0;
            }
        }
        // Randomly decide the starting player
        currentPlayer = Random.Range(1, 3);
        UpdateTurnDisplay();
        UpdateTurnCountDisplay();
    }
    private void OnSquareClicked(int x, int y)
    {
        if (previousButtonX == -1 && previousButtonY == -1)
        {
            previousButtonX = x;
            previousButtonY = y;
        }
        else if (previousButtonX == x && previousButtonY == y)
        {
            OnSquareConfirmed(x, y);
        }
        else
        {
            previousButtonX = x;
            previousButtonY = y;
        }
    }
    private void OnSquareConfirmed(int x, int y)
    {
        if (squareStatus[x, y] == 0)
        {
            squareStatus[x, y] = currentPlayer;
            buttons[x, y].image.color = (currentPlayer == 1) ? Color.blue : Color.green;
            if (currentPlayer == 1)
            {
                user1Squares.Add(buttons[x, y].gameObject);
                if (user1Squares.Count > 3)
                {
                    StopBlinking(user1Squares[0].GetComponent<Button>(), Color.blue); // 既存の点滅を停止
                    GameObject first = user1Squares[0];
                    string[] coordinates = first.name.Split('-')[1].ToCharArray().Select(c => c.ToString()).ToArray();
                    squareStatus[int.Parse(coordinates[0]), int.Parse(coordinates[1])] = 0;
                    first.GetComponent<Button>().image.color = Color.white;
                    user1Squares.RemoveAt(0);
                }
                // If there are 3 squares and it is user1's turn, blink the button
                if (user1Squares.Count == 3)
                {
                    BlinkButton(user1Squares[0].GetComponent<Button>(), Color.blue);
                }
            }
            else
            {
                user2Squares.Add(buttons[x, y].gameObject);
                if (user2Squares.Count > 3)
                {
                    StopBlinking(user2Squares[0].GetComponent<Button>(), Color.green); // 既存の点滅を停止
                    GameObject first = user2Squares[0];
                    string[] coordinates = first.name.Split('-')[1].ToCharArray().Select(c => c.ToString()).ToArray();
                    squareStatus[int.Parse(coordinates[0]), int.Parse(coordinates[1])] = 0;
                    first.GetComponent<Button>().image.color = Color.white;
                    user2Squares.RemoveAt(0);
                }
                // If there are 3 squares and it is user2's turn, blink the button
                if (user2Squares.Count == 3)
                {
                    BlinkButton(user2Squares[0].GetComponent<Button>(), Color.green);
                }
            }
            var winningButtons = CheckWinCondition();
            if (winningButtons.Count > 0)
            {
                turnDisplay.text = $"ユーザー{currentPlayer}の勝利";
                StopAllBlinking();
                HighlightWinningButtons(winningButtons);
                DisableAllButtons();
            }
            else
            {
                ChangeTurn();
            }
            previousButtonX = -1;
            previousButtonY = -1;
        }
    }
    private void UpdateTurnDisplay()
    {
        string turnText = (currentPlayer == 1) ? "ユーザー1のターン" : "ユーザー2のターン";
        turnDisplay.text = turnText;
    }
    private void UpdateTurnCountDisplay()
    {
        turnCountDisplay.text = $"{turnCount}ターン目";
    }
    private void ChangeTurn()
    {
        turnCount++;
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        UpdateTurnDisplay();
        UpdateTurnCountDisplay();
        // Stop previous user's blinking
        if (currentPlayer == 1 && user2Squares.Count == 3)
        {
            StopBlinking(user2Squares[0].GetComponent<Button>(), Color.green);
        }
        else if (currentPlayer == 2 && user1Squares.Count == 3)
        {
            StopBlinking(user1Squares[0].GetComponent<Button>(), Color.blue);
        }
        // Start current user's blinking
        if (currentPlayer == 1 && user1Squares.Count == 3)
        {
            BlinkButton(user1Squares[0].GetComponent<Button>(), Color.blue);
        }
        else if (currentPlayer == 2 && user2Squares.Count == 3)
        {
            BlinkButton(user2Squares[0].GetComponent<Button>(), Color.green);
        }
    }
    private List<Button> CheckWinCondition()
    {
        List<Button> winningButtons = new List<Button>();
        for (int i = 0; i < 3; i++)
        {
            if (squareStatus[i, 0] == currentPlayer && squareStatus[i, 1] == currentPlayer && squareStatus[i, 2] == currentPlayer)
            {
                winningButtons.Add(buttons[i, 0]);
                winningButtons.Add(buttons[i, 1]);
                winningButtons.Add(buttons[i, 2]);
                return winningButtons;
            }
            if (squareStatus[0, i] == currentPlayer && squareStatus[1, i] == currentPlayer && squareStatus[2, i] == currentPlayer)
            {
                winningButtons.Add(buttons[0, i]);
                winningButtons.Add(buttons[1, i]);
                winningButtons.Add(buttons[2, i]);
                return winningButtons;
            }
        }
        if (squareStatus[0, 0] == currentPlayer && squareStatus[1, 1] == currentPlayer && squareStatus[2, 2] == currentPlayer)
        {
            winningButtons.Add(buttons[0, 0]);
            winningButtons.Add(buttons[1, 1]);
            winningButtons.Add(buttons[2, 2]);
            return winningButtons;
        }
        if (squareStatus[0, 2] == currentPlayer && squareStatus[1, 1] == currentPlayer && squareStatus[2, 0] == currentPlayer)
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
        foreach (Button button in buttons)
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
    private void HighlightWinningButtons(List<Button> winningButtons)
    {
        foreach (Button btn in winningButtons)
        {
            btn.image.DOColor(Color.yellow, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
    }
}
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        // Масив кнопок гри 3x3
        private Button[,] cells = new Button[3, 3];

        // Символи гравця та комп'ютера
        private string userSymbol, computerSymbol;

        // Чи активна гра
        private bool gameActive = false;

        // Таймер гри
        private Timer gameTimer = new Timer();
        private int secondsElapsed = 0;

        // Кнопки керування та таймер
        private Button buttonNewGame;
        private Button buttonStopGame;
        private Label labelTimer;

        // Рахунок та кількість раундів
        private int userScore = 0;
        private int computerScore = 0;
        private int currentRound = 0;
        private const int MaxRounds = 3;

        public Form1()
        {
            InitializeComponent();
            InitializeGameControls();
            this.Load += Form1_Load;
        }
        // Завантаження форми — вибір символу гравця
        private void Form1_Load(object sender, EventArgs e)
        {
            // Повідомлення для вибору символу (X або O)
            var choice = MessageBox.Show(
                "Choose your symbol:\nYes = X\nNo = O",
                "Symbol Selection",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            userSymbol = (choice == DialogResult.Yes) ? "X" : "O";
            computerSymbol = (userSymbol == "X") ? "O" : "X";

            // Initialize first round
            buttonNewGame.Enabled = true;
            buttonStopGame.Enabled = false;
            UpdateScoreboard();
            labelTimer.Text = "Time: 0s";
        }
        // Ініціалізація кнопок, мітки часу, сітки
        private void InitializeGameControls()
        {
            // New Game button
            buttonNewGame = new Button
            {
                Text = "New Game",
                Location = new Point(10, 10),
                Enabled = false
            };
            buttonNewGame.Click += (s, e) => StartNewGame();
            Controls.Add(buttonNewGame);

            // Stop Game button
            buttonStopGame = new Button
            {
                Text = "Stop Game",
                Location = new Point(100, 10),
                Enabled = false
            };
            buttonStopGame.Click += (s, e) => StopGame();
            Controls.Add(buttonStopGame);

            // Timer label
            labelTimer = new Label { Location = new Point(200, 14), AutoSize = true };
            Controls.Add(labelTimer);

            // Timer setup
            gameTimer.Interval = 1000;
            gameTimer.Tick += (s, e) =>
            {
                secondsElapsed++;
                labelTimer.Text = $"Time: {secondsElapsed}s";
            };

            // Create 3x3 grid
            int size = 60;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    var btn = new Button
                    {
                        Location = new Point(10 + j * size, 50 + i * size),
                        Size = new Size(size, size),
                        Font = new Font("Arial", 24, FontStyle.Bold),
                        Enabled = false
                    };
                    btn.Click += Cell_Click;
                    cells[i, j] = btn;
                    Controls.Add(btn);
                }
        }

        private void StartNewGame()
        {
            // Очистити поле, активувати кнопки, запустити таймер
            foreach (var btn in cells)
            {
                btn.Text = "";
                btn.BackColor = SystemColors.Control;
                btn.Enabled = true;
            }

            secondsElapsed = 0;
            labelTimer.Text = "Time: 0s";
            gameTimer.Start();
            gameActive = true;

            buttonNewGame.Enabled = false;
            buttonStopGame.Enabled = true;
        }
        // Деактивувати гру та зупинити таймер
        private void StopGame()
        {
            gameActive = false;
            gameTimer.Stop();
            foreach (var btn in cells) btn.Enabled = false;

            buttonNewGame.Enabled = true;
            buttonStopGame.Enabled = false;
        }
        // Обробка кліку по клітинці
        private void Cell_Click(object sender, EventArgs e)
        {
            // Якщо хід дозволено: гравець -> перевірка виграшу -> хід комп'ютера -> перевірка
            if (!gameActive) return;
            var btn = (Button)sender;
            if (btn.Text != "") return;

            // User move
            btn.Text = userSymbol;
            if (CheckWin(userSymbol))
            {
                HighlightWin(userSymbol);
                EndGame("User wins!");
                return;
            }

            // Computer move
            ComputerMove();
            if (CheckWin(computerSymbol))
            {
                HighlightWin(computerSymbol);
                EndGame("Computer wins!");
                return;
            }

            // Draw? (нічія)
            if (cells.Cast<Button>().All(b => b.Text != ""))
                EndGame("Draw!");
        }

        private void ComputerMove()
        {
            // 1) Спроба виграти
            if (TryCompleteLine(computerSymbol)) return;
            // 2) Спроба заблокувати гравця
            if (TryCompleteLine(userSymbol)) return;
            // 3) Випадковий хід
            var rnd = new Random();
            var empties = cells.Cast<Button>().Where(b => b.Text == "").ToList();
            if (empties.Count > 0)
                empties[rnd.Next(empties.Count)].Text = computerSymbol;
        }
        // Спроба завершити ряд/колонку/діагональ
        private bool TryCompleteLine(string sym)
        {
            // rows/columns
            for (int i = 0; i < 3; i++)
            {
                // row i
                if (CompleteIfPossible(cells[i, 0], cells[i, 1], cells[i, 2], sym)) return true;
                // col i
                if (CompleteIfPossible(cells[0, i], cells[1, i], cells[2, i], sym)) return true;
            }
            // diagonals
            if (CompleteIfPossible(cells[0, 0], cells[1, 1], cells[2, 2], sym)) return true;
            if (CompleteIfPossible(cells[0, 2], cells[1, 1], cells[2, 0], sym)) return true;
            return false;
        }
        // Допоміжна перевірка трійки кнопок
        private bool CompleteIfPossible(Button a, Button b, Button c, string sym)
        {
            var trio = new[] { a, b, c };
            if (trio.Count(x => x.Text == sym) == 2 && trio.Count(x => x.Text == "") == 1)
            {
                trio.First(x => x.Text == "").Text = computerSymbol;
                return true;
            }
            return false;
        }
        // Перевірка виграшу
        private bool CheckWin(string sym)
        {
            // rows & columns
            for (int i = 0; i < 3; i++)
                if (Enumerable.Range(0, 3).All(j => cells[i, j].Text == sym) ||
                    Enumerable.Range(0, 3).All(j => cells[j, i].Text == sym))
                    return true;
            // diagonals
            if (Enumerable.Range(0, 3).All(i => cells[i, i].Text == sym) ||
                Enumerable.Range(0, 3).All(i => cells[i, 2 - i].Text == sym))
                return true;
            return false;
        }
        // Виділення виграшної лінії
        private void HighlightWin(string sym)
        {
            var winColor = (sym == userSymbol) ? Color.LightGreen : Color.LightCoral;
            // rows
            for (int i = 0; i < 3; i++)
                if (Enumerable.Range(0, 3).All(j => cells[i, j].Text == sym))
                    foreach (var j in Enumerable.Range(0, 3)) cells[i, j].BackColor = winColor;
            // cols
            for (int j = 0; j < 3; j++)
                if (Enumerable.Range(0, 3).All(i => cells[i, j].Text == sym))
                    foreach (var i in Enumerable.Range(0, 3)) cells[i, j].BackColor = winColor;
            // diags
            if (Enumerable.Range(0, 3).All(i => cells[i, i].Text == sym))
                foreach (var i in Enumerable.Range(0, 3)) cells[i, i].BackColor = winColor;
            if (Enumerable.Range(0, 3).All(i => cells[i, 2 - i].Text == sym))
                foreach (var i in Enumerable.Range(0, 3)) cells[i, 2 - i].BackColor = winColor;
        }
        // Завершення гри, підрахунок рахунку, вивід повідомлень
        private void EndGame(string message)
        {
            gameActive = false;
            gameTimer.Stop();
            MessageBox.Show(message, "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // update scores
            if (message.Contains("User wins")) userScore++;
            else if (message.Contains("Computer wins")) computerScore++;

            currentRound++;
            UpdateScoreboard();
            
            if (currentRound < MaxRounds)
                buttonNewGame.Enabled = true;
            else
            {
                // final summary
                string finale =
                    $"After {MaxRounds} rounds:\nYou {userScore} : {computerScore} Computer\n\n" +
                    (userScore > computerScore ? "You win the series!" :
                     userScore < computerScore ? "Computer wins the series!" :
                     "Series is a draw!");
                MessageBox.Show(finale, "Series Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
                buttonNewGame.Enabled = false;
                            buttonStopGame.Enabled = false;
            MessageBox.Show( 
                "Thank you for playing! The application will now close.",
                "Goodbye", MessageBoxButtons.OK,MessageBoxIcon.Information);
            Application.Exit();
            }
            buttonStopGame.Enabled = false;
        }
        // Оновлення заголовка вікна з рахунком
        private void UpdateScoreboard()
        {
            this.Text = $"Tic Tac Toe — Score: You {userScore} : {computerScore} — Round {currentRound}/{MaxRounds}";
        }
    }
}

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace kab3._4
{
    public partial class Game : Form
    {
        private Thread squareThread;
        private Thread ballThread;
        private Thread barThread;
        private int squareSize = 200;
        private int ballSize = 20;
        private int ballX;
        private int ballY;
        private int ballVelocityX;
        private int ballVelocityY;
        private int circleSpeed;
        private int barPosition;
        private Random rnd = new Random(); // Один объект Random для всей программы
        private Button startButton;
        private bool ballStarted = false; // Флаг для отслеживания того, что шар начал движение
        private TrackBar trackBarCircle;
        private TrackBar trackBarBar;
        private bool isBarMoving = false; // Флаг для отслеживания того, что полоска перемещается мышью


        public Game()
        {
            InitializeStartButton(); // Инициализация кнопки "Старт"
            circleSpeed = 10;
            InitializeComponent();
            InitializeTrackBar();
            InitializeThreads();
            barPosition = (ClientSize.Width - squareSize / 3) / 2;
        }

        private void InitializeTrackBar()
        {
            trackBarCircle = new TrackBar();
            trackBarCircle.Location = new Point(10, 10); // Установите желаемое расположение
            trackBarCircle.Width = 200; // Установите желаемую ширину
            trackBarCircle.Minimum = 1; // Минимальное значение скорости
            trackBarCircle.Maximum = 10; // Максимальное значение скорости
            trackBarCircle.Value = 1; // Начальное значение скорости
            trackBarCircle.ValueChanged += TrackBar_ValueChanged; // Обработчик события изменения значения
            Controls.Add(trackBarCircle); // Добавление TrackBar на форму

            trackBarBar = new TrackBar();
            trackBarBar.Location = new Point(10, ClientSize.Height - 50); // Установите желаемое расположение
            trackBarBar.Width = 200; // Установите желаемую ширину
            trackBarBar.Minimum = 0; // Минимальное значение положения полоски
            trackBarBar.Maximum = ClientSize.Width - 50; // Максимальное значение положения полоски
            trackBarBar.Value = (trackBarBar.Maximum - trackBarBar.Minimum) / 2; // Начальное значение положения полоски
            trackBarBar.ValueChanged += TrackBarBar_ValueChanged; // Обработчик события изменения значения
            Controls.Add(trackBarBar); // Добавление TrackBar на форму
        }

        private void InitializeStartButton()
        {
            startButton = new Button();
            startButton.Text = "Старт";
            startButton.Location = new Point(300, 10);
            startButton.Click += StartButton_Click; // Обработчик события нажатия кнопки "Старт"
            Controls.Add(startButton);
        }

        private void InitializeThreads()
        {
            // Устанавливаем начальные координаты шара в центр окна
            ballX = (ClientSize.Width - ballSize) / 2;
            ballY = (ClientSize.Height - ballSize) / 2;

            ballVelocityX = 6; // Генерируем случайное значение от -5 до 5
            ballVelocityY = 6;

            // Запускаем потоки
            squareThread = new Thread(new ThreadStart(AnimateSquare));
            ballThread = new Thread(new ThreadStart(MoveBall));
            barThread = new Thread(new ThreadStart(MoveBar)); // Добавленный поток для перемещения полоски

            squareThread.IsBackground = true;
            ballThread.IsBackground = true;
            barThread.IsBackground = true;

            squareThread.Start();
            ballThread.Start();
            barThread.Start(); // Запускаем поток для перемещения полоски
        }

        private void AnimateSquare()
        {
            while (true)
            {
                if (ballStarted)
                {
                    if (squareSize > 180) // Уменьшаем на 1/5 текущего размера
                    {
                        squareSize -= 1;
                        trackBarBar.Maximum = ClientSize.Width - 50; // Обновляем максимальное значение ползунка
                    }
                    else
                    {
                        Thread.Sleep(31);
                        while (squareSize < 200)
                        {
                            squareSize += 1;
                            Thread.Sleep(31);
                        }
                    }
                }
                Invalidate();
                Thread.Sleep(31);
            }
        }

        private void MoveBall()
        {
            while (true)
            {
                if (ballStarted)
                {
                    // Проверка столкновения шара с полоской
                    if (ballY + ballSize >= (ClientSize.Height + squareSize) / 2 - 10)
                    {
                        if (ballX + ballSize >= barPosition && ballX <= barPosition + squareSize / 3)
                        {
                            // Если шар касается полоски, меняем его направление движения вверх и увеличиваем скорость
                            ballVelocityY = -ballVelocityY;
                            ballVelocityX = rnd.Next(-6, 7); // Генерируем случайное значение скорости по оси X
                        }
                        else
                        {
                            // Если шар падает ниже полоски, выводим сообщение об окончании игры и завершаем игру
                            MessageBox.Show("Игра закончена!");
                            ResetGame(); // Сбрасываем игру
                        }
                    }
                    else if (ballY <= (ClientSize.Height - squareSize) / 2 || ballY + ballSize >= (ClientSize.Height + squareSize) / 2)
                    {
                        // Если шар достигает верхней или нижней границы квадрата, меняем его траекторию
                        ballVelocityY = -ballVelocityY;
                    }
                    else if (ballX <= (ClientSize.Width - squareSize) / 2 || ballX + ballSize >= (ClientSize.Width + squareSize) / 2)
                    {
                        // Если шар достигает левой или правой границы квадрата, меняем его траекторию
                        ballVelocityX = -ballVelocityX;
                    }

                    // Вычисляем коэффициент скорости
                    double speedCoefficient = circleSpeed / 100.0; // Приводим значение трекбара к диапазону от 0 до 1

                    // Вычисляем расстояние, которое шарик пройдет за определенный интервал времени
                    double distanceX = ballVelocityX * speedCoefficient;
                    double distanceY = ballVelocityY * speedCoefficient;

                    // Обновляем координаты шарика
                    ballX += (int)distanceX;
                    ballY += (int)distanceY;

                    Invalidate();
                }
                Thread.Sleep(30); // Интервал времени между обновлениями координат шарика
            }
        }


        private void MoveBar()
        {
            while (true)
            {
                if (ballStarted)
                {
                    // Обновляем положение полоски в соответствии с новым значением ползунка
                    if (trackBarBar.InvokeRequired)
                    {
                        trackBarBar.Invoke(new MethodInvoker(delegate { barPosition = trackBarBar.Value; }));
                    }
                    else
                    {
                        barPosition = trackBarBar.Value;
                    }

                    // Перерисовываем форму
                    if (InvokeRequired)
                    {
                        Invoke(new MethodInvoker(delegate { Invalidate(); }));
                    }
                    else
                    {
                        Invalidate();
                    }
                }

                // Добавляем небольшую задержку, чтобы не нагружать процессор
                Thread.Sleep(10);
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            ballStarted = true;
            // Начинаем новую игру
            trackBarCircle.Value = trackBarCircle.Maximum / 2;
            circleSpeed = 10 * trackBarCircle.Value;
        }

        private void ResetGame()
        {
            // Сбрасываем координаты шара на начальные значения
            ballX = (ClientSize.Width - ballSize) / 2;
            ballY = (ClientSize.Height - ballSize) / 2;
            // Останавливаем шар и сбрасываем флаг
            ballStarted = false;

            // Используем Invoke для обновления трекбара в главном потоке
            if (trackBarCircle.InvokeRequired)
            {
                trackBarCircle.Invoke(new MethodInvoker(delegate { trackBarCircle.Value = trackBarCircle.Minimum; }));
            }
            else
            {
                trackBarCircle.Value = trackBarCircle.Minimum;
            }

            // Перемещаем ползунок на середину
            if (trackBarBar.InvokeRequired)
            {
                trackBarBar.Invoke(new MethodInvoker(delegate { trackBarBar.Value = (trackBarBar.Maximum - trackBarBar.Minimum) / 2; }));
            }
            else
            {
                trackBarBar.Value = (trackBarBar.Maximum - trackBarBar.Minimum) / 2;
            }

            // Перерисовываем форму
            Invalidate();
        }


        private void TrackBarBar_ValueChanged(object sender, EventArgs e)
        {
            // Обновляем положение полоски в соответствии с новым значением ползунка
            barPosition = trackBarBar.Value;

            // Перерисовываем форму
            Invalidate();
        }

        private void TrackBar_ValueChanged(object sender, EventArgs e)
        {
            circleSpeed = 10 * trackBarCircle.Value;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw Square
            Rectangle squareRect = new Rectangle((ClientSize.Width - squareSize) / 2, (ClientSize.Height - squareSize) / 2, squareSize, squareSize);
            e.Graphics.FillRectangle(Brushes.White, squareRect); // Fill the square with white color
            e.Graphics.DrawRectangle(Pens.Black, squareRect); // Draw the border of the square

            // Draw Ball
            e.Graphics.FillEllipse(Brushes.Red, ballX, ballY, ballSize, ballSize);

            // Calculate bar position and dimensions
            int barWidth = squareSize / 3; // Set the bar width to one third of the square size
            int barHeight = 10; // Bar height
            int barY = (ClientSize.Height + squareSize) / 2 - barHeight; // Bar position on Y-axis

            int maxBarPosition = ClientSize.Width - barWidth; // Maximum position for the bar

            // Calculate bar position within the available range
            int clampedBarPosition = Math.Max(0, Math.Min(maxBarPosition, barPosition));

            // Draw the bar
            Rectangle barRect = new Rectangle(clampedBarPosition, barY, barWidth, barHeight);
            e.Graphics.FillRectangle(Brushes.Blue, barRect);
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

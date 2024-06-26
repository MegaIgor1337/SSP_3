﻿using System;
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
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            InitializeStartButton(); // Инициализация кнопки "Старт"
            circleSpeed = 5;
            InitializeComponent();
            this.MaximizeBox = false; // Запрещаем полноэкранный режим
            this.ResumeLayout(false);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle; // Задаем стиль окна
            this.ResumeLayout(false);
                this.DoubleBuffered = false;

            InitializeTrackBar();
            InitializeThreads();
            barPosition = (ClientSize.Width - squareSize / 3) / 2;
            this.FormClosing += Game_FormClosing;
        }

        private bool IsBallNearSquare(int x, int y)
        {
            int squareX = (ClientSize.Width - squareSize) / 2;
            int squareY = (ClientSize.Height - squareSize) / 2;
            int squareCenterX = squareX + squareSize / 2;
            int squareCenterY = squareY + squareSize / 2;

            double distance = Math.Sqrt(Math.Pow(x - squareCenterX, 2) + Math.Pow(y - squareCenterY, 2));

            return distance < 10; // Здесь 10 - это минимальное допустимое расстояние между квадратом и шариком
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
            trackBarBar.KeyDown += trackBarBar_KeyDown;

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
                if (ballStarted && !IsBallNearSquare(ballX + ballSize / 2, ballY + ballSize / 2))
                {
                    // Проверяем, находится ли шар близко к границе квадрата
                    bool ballNearBorder = ballX <= (ClientSize.Width - squareSize) / 2 + 20 || ballX + ballSize >= (ClientSize.Width + squareSize) / 2 - 20 ||
                                          ballY <= (ClientSize.Height - squareSize) / 2 + 20 || ballY + ballSize >= (ClientSize.Height + squareSize) / 2 - 20;

                    if (squareSize > 170 && !ballNearBorder)
                    {
                        squareSize -= 1;
                        trackBarBar.Maximum = ClientSize.Width - 50;
                    }
                    else if (!ballNearBorder)
                    {
                        Thread.Sleep(500);
                        while (squareSize < 200)
                        {
                            squareSize += 1;
                            Thread.Sleep(500);
                        }
                    }
                }

                Invalidate();
                Thread.Sleep(500);
            }
        }

        private void trackBarBar_KeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, нажата ли клавиша влево или вправо
            if (e.KeyCode == Keys.Left)
            {
                // Изменяем положение ползунка влево
                if (trackBarBar.Value > trackBarBar.Minimum && !IsBarTouchingLeftBoundary())
                {
                    trackBarBar.Value -= 5;
                }
            }
            else if (e.KeyCode == Keys.Right)
            {
                // Изменяем положение ползунка вправо
                if (trackBarBar.Value < trackBarBar.Maximum && !IsBarTouchingRightBoundary())
                {
                    trackBarBar.Value += 5;
                }
            }
        }

        private bool IsBarTouchingLeftBoundary()
        {
            return barPosition <= (ClientSize.Width - squareSize) / 2;
        }

        private bool IsBarTouchingRightBoundary()
        {
            return barPosition + squareSize / 3 >= (ClientSize.Width + squareSize) / 2;
        }




        private void MoveBall()
        {
            while (true)
            {
                if (ballStarted)
                {
                    // Проверяем столкновение шара с нижней полоской
                    if (ballY + ballSize >= (ClientSize.Height + squareSize) / 2 - 10)
                    {
                        // Вычисляем координаты полоски
                        int barTop = (ClientSize.Height + squareSize) / 2 - 10; // Верхняя граница полоски
                        int barBottom = barTop + 10; // Нижняя граница полоски
                        int barLeft = barPosition; // Левая граница полоски
                        int barRight = barPosition + squareSize / 3; // Правая граница полоски

                        // Проверяем столкновение шара с полоской
                        if (ballX + ballSize >= barLeft && ballX <= barRight && ballY + ballSize >= barTop)
                        {
                            // Определяем центр полоски
                            int barCenterX = barPosition + squareSize / 6;

                            // Проверяем, попадает ли шарик в верхнюю часть полоски (верхняя треть)
                            if (ballX + ballSize / 2 >= barLeft && ballX + ballSize / 2 <= barRight && ballY + ballSize / 2 <= barTop + (barBottom - barTop) / 3)
                            {
                                // Изменяем вертикальную скорость шарика на противоположную (отрицательную)
                                ballVelocityY = -Math.Abs(ballVelocityY);
                            }
                            else
                            {
                                // Если шарик касается полоски, но не в верхней части, отражаем его по горизонтали, как раньше
                                // Определяем угол нормали к поверхности полоски
                                double normalAngle = Math.Atan2(barCenterX - ballX, (ClientSize.Height + squareSize) / 2 - ballY);

                                // Определяем угол падения шара на поверхность полоски, но оставляем горизонтальное направление неизменным
                                double incidenceAngle = Math.Atan2(ballVelocityY, ballVelocityX);

                                // Определяем угол отражения от поверхности полоски, сохраняя горизонтальное направление неизменным
                                double reflectionAngle = 2 * normalAngle - incidenceAngle;

                                // Сохраняем модуль вертикальной скорости (по оси Y)
                                double speedY = Math.Abs(ballVelocityY);

                                // Вычисляем новое значение ballVelocityY на основе угла отражения с сохранением модуля скорости
                                ballVelocityY = (int)Math.Round(speedY * Math.Sin(reflectionAngle));

                                // Если шарик касается полоски, поднимаем его на расстояние, равное его диаметру, чтобы он не проваливался в полоску
                                ballY = (ClientSize.Height + squareSize) / 2 - ballSize - 11;
                            }
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
                }
                Invalidate();
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

                    // Ограничиваем положение полоски в пределах квадрата
                    int maxBarPosition = ClientSize.Width - squareSize / 3;
                    barPosition = Math.Max(0, Math.Min(maxBarPosition, barPosition));

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
            trackBarCircle.Value = trackBarCircle.Maximum / 3;
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


        // Глобальная переменная для хранения предыдущего значения ползунка
        private int previousSliderValue = 0;

        private void TrackBarBar_ValueChanged(object sender, EventArgs e)
        {
            // Проверяем, касается ли полоска правого угла квадрата или задевает его
            bool touchingRightCorner = barPosition + squareSize / 3 >= (ClientSize.Width + squareSize) / 2;

            // Проверяем, касается ли полоска левого угла квадрата или задевает его
            bool touchingLeftCorner = barPosition <= (ClientSize.Width - squareSize) / 2;

            // Если полоска касается правого угла квадрата, разрешаем ей перемещаться влево
            if (touchingRightCorner && trackBarBar.Value > previousSliderValue)
            {
                trackBarBar.Value = previousSliderValue;
                return;
            }

            // Если полоска касается левого угла квадрата, разрешаем ей перемещаться вправо
            if (touchingLeftCorner && trackBarBar.Value < previousSliderValue)
            {
                trackBarBar.Value = previousSliderValue;
                return;
            }

            // Если ни с одного из углов квадрата полоска не касается, изменяем максимальное значение ползунка
            int maxSliderValue = squareSize - trackBarBar.Width;
            if (maxSliderValue >= 0)
            {
                if (trackBarBar.InvokeRequired)
                {
                    trackBarBar.Invoke(new MethodInvoker(delegate { trackBarBar.Maximum = maxSliderValue; }));
                }
            }
            else
            {
                if (trackBarBar.InvokeRequired)
                {
                    trackBarBar.Invoke(new MethodInvoker(delegate { trackBarBar.Maximum = 0; }));
                }
            }

            // Если значение ползунка изменилось пользователем и полоска не касается краев квадрата,
            // сохраняем текущее значение ползунка
            if (trackBarBar.Value != previousSliderValue && !touchingRightCorner && !touchingLeftCorner)
            {
                previousSliderValue = trackBarBar.Value;
            }
        }





        private void TrackBar_ValueChanged(object sender, EventArgs e)
        {
            circleSpeed = 10 * trackBarCircle.Value;
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
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

            // Calculate the visible portion of the bar
            int visibleBarWidth = Math.Min(barWidth, squareSize - Math.Abs(barPosition - (ClientSize.Width - squareSize) / 2));
            int visibleBarX = barPosition < (ClientSize.Width - squareSize) / 2 ? (ClientSize.Width - squareSize) / 2 : barPosition;

            // Ensure that the bar does not extend beyond the square boundaries
            if (visibleBarX < (ClientSize.Width - squareSize) / 2)
            {
                visibleBarWidth -= ((ClientSize.Width - squareSize) / 2 - visibleBarX);
                visibleBarX = (ClientSize.Width - squareSize) / 2;
            }
            else if (visibleBarX + visibleBarWidth > (ClientSize.Width + squareSize) / 2)
            {
                visibleBarWidth -= (visibleBarX + visibleBarWidth - (ClientSize.Width + squareSize) / 2);
            }

            // Draw the visible portion of the bar
            if (visibleBarWidth > 0)
            {
                Rectangle barRect = new Rectangle(visibleBarX, barY, visibleBarWidth, barHeight);
                e.Graphics.FillRectangle(Brushes.Blue, barRect);
            }
        }


        private void Game_FormClosing(object sender, FormClosingEventArgs e)
        {
            squareThread.Abort();
            ballThread.Abort();
            barThread.Abort();
            // Дожидаемся завершения всех потоков
            squareThread.Join();
            ballThread.Join();
            barThread.Join();

          
        }




        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

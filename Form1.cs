using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace SpaceDefender
{
    public partial class Form1 : Form
    {
        // Звуки
        private SoundPlayer backgroundSound;
        private SoundPlayer bonusSound;
        private SoundPlayer hitSound;

        // Изображения
        private Image playerImage;
        private Image bulletImage;
        private Image enemyImage;
        private Image explosionImage;

        // Игровые объекты
        private PlayerShip player;
        private List<Bullet> playerBullets = new List<Bullet>();
        private List<Enemy> enemies = new List<Enemy>();

        // Состояния
        private bool soundEnabled = true;
        private int score = 0;
        private int level = 1;
        private bool leftPressed, rightPressed, upPressed, downPressed;
        private Random rand = new Random();

        public Form1()
        {
            InitializeComponent();
            LoadResources();
            InitializeGame();

            pictureBox1.Paint += PictureBox1_Paint;
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }

        private void LoadResources()
        {
            playerImage = Image.FromFile("Resources/playerShip.png");
            bulletImage = Image.FromFile("Resources/bullet.png");
            enemyImage = Image.FromFile("Resources/meteor.png");
            explosionImage = Image.FromFile("Resources/explosion.png");

            backgroundSound = new SoundPlayer("Resources/backgroundMusic.wav");
            bonusSound = new SoundPlayer("Resources/bonus.wav");
            hitSound = new SoundPlayer("Resources/hit.wav");
        }

        private void InitializeGame()
        {
            if (soundEnabled)
                backgroundSound.PlayLooping();

            player = new PlayerShip(pictureBox1.Width / 2, pictureBox1.Height - 60, playerImage);

            playerBullets.Clear();
            enemies.Clear();
            score = 0;
            level = 1;
        }

        private void Timer1_Tick(object sender, EventArgs eventArgs)
        {
            // Движение игрока
            int speed = 5;
            if (leftPressed && player.X > 0) player.X -= speed;
            if (rightPressed && player.X < pictureBox1.Width) player.X += speed;
            if (upPressed && player.Y > 0) player.Y -= speed;
            if (downPressed && player.Y < pictureBox1.Height) player.Y += speed;

            // Выстрелы
            foreach (var bullet in playerBullets)
                bullet.Y -= 10;
            playerBullets.RemoveAll(bullet => bullet.Y < -10);

            // Враги
            if (rand.Next(0, 20) == 0)
            {
                int enemyX = rand.Next(0, pictureBox1.Width - 30);
                enemies.Add(new Enemy(enemyX, -30, enemyImage));
            }

            foreach (var enemy in enemies)
                enemy.Y += 3;
            enemies.RemoveAll(enemy => enemy.Y > pictureBox1.Height);

            // Коллизии
            CheckCollisions();

            pictureBox1.Invalidate();
        }

        private void CheckCollisions()
        {
            foreach (var bullet in playerBullets.ToList())
            {
                foreach (var enemy in enemies.ToList())
                {
                    if (Math.Abs(bullet.X - enemy.X) < 20 && Math.Abs(bullet.Y - enemy.Y) < 20)
                    {
                        if (soundEnabled) hitSound.Play();

                        playerBullets.Remove(bullet);
                        enemies.Remove(enemy);
                        score += 10;

                        if (score % 100 == 0)
                        {
                            if (soundEnabled) bonusSound.Play();
                            level++;
                        }
                        break;
                    }
                }
            }
        }

        private void FireBullet()
        {
            if (player != null)
            {
                playerBullets.Add(new Bullet(player.X, player.Y - 20, bulletImage));
            }
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs paintArgs)
        {
            Graphics g = paintArgs.Graphics;

            if (player != null)
                player.Draw(g);

            foreach (var bullet in playerBullets)
                bullet.Draw(g);

            foreach (var enemy in enemies)
                enemy.Draw(g);

            g.DrawString($"Счёт: {score}   Уровень: {level}", new Font("Arial", 14, FontStyle.Bold), Brushes.White, new PointF(10, 10));
        }

        private void Form1_KeyDown(object sender, KeyEventArgs keyArgs)
        {
            if (keyArgs.KeyCode == Keys.Left || keyArgs.KeyCode == Keys.A) leftPressed = true;
            if (keyArgs.KeyCode == Keys.Right || keyArgs.KeyCode == Keys.D) rightPressed = true;
            if (keyArgs.KeyCode == Keys.Up || keyArgs.KeyCode == Keys.W) upPressed = true;
            if (keyArgs.KeyCode == Keys.Down || keyArgs.KeyCode == Keys.S) downPressed = true;
            if (keyArgs.KeyCode == Keys.Space) FireBullet();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs keyArgs)
        {
            if (keyArgs.KeyCode == Keys.Left || keyArgs.KeyCode == Keys.A) leftPressed = false;
            if (keyArgs.KeyCode == Keys.Right || keyArgs.KeyCode == Keys.D) rightPressed = false;
            if (keyArgs.KeyCode == Keys.Up || keyArgs.KeyCode == Keys.W) upPressed = false;
            if (keyArgs.KeyCode == Keys.Down || keyArgs.KeyCode == Keys.S) downPressed = false;
        }

        private void ToggleSound(object sender, EventArgs eventArgs)
        {
            soundEnabled = !soundEnabled;
            toggleSoundMenuItem.Text = soundEnabled ? "Звук: Вкл" : "Звук: Выкл";
            if (soundEnabled)
                backgroundSound.PlayLooping();
            else
                backgroundSound.Stop();
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs eventArgs)
        {
            backgroundSound.Stop();
            InitializeGame();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs eventArgs)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs eventArgs)
        {
            timer1.Interval = 33;
            timer1.Tick += Timer1_Tick;
            timer1.Start();
        }

        // === Игровые объекты ===

        public class PlayerShip
        {
            public int X { get; set; }
            public int Y { get; set; }
            public Image ShipImage { get; set; }

            public PlayerShip(int x, int y, Image image)
            {
                X = x;
                Y = y;
                ShipImage = image;
            }

            public void Draw(Graphics g)
            {
                g.DrawImage(ShipImage, X - 15, Y - 15, 30, 30);
            }
        }

        public class Bullet
        {
            public int X { get; set; }
            public int Y { get; set; }
            public Image BulletImage { get; set; }

            public Bullet(int x, int y, Image image)
            {
                X = x;
                Y = y;
                BulletImage = image;
            }

            public void Draw(Graphics g)
            {
                g.DrawImage(BulletImage, X - 2, Y - 5, 4, 10);
            }
        }

        public class Enemy
        {
            public int X { get; set; }
            public int Y { get; set; }
            public Image EnemyImage { get; set; }

            public Enemy(int x, int y, Image image)
            {
                X = x;
                Y = y;
                EnemyImage = image;
            }

            public void Draw(Graphics g)
            {
                g.DrawImage(EnemyImage, X - 15, Y - 15, 30, 30);
            }
        }
    }
}

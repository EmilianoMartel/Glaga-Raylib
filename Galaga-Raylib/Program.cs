using Raylib_cs;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;

public class PruebasRayLib
{
    static Random random = new Random();

    struct Character
    {
        public Vector2 position;
        public Texture2D image;
        public Texture2D explotionImage;
        public Bullet[] bulletPool;
        public int shootDirection;
        public int life;
        public bool isActive;
    }

    struct Bullet
    {
        public Vector2 position;
        public Texture2D image;
        public Rectangle bound;
        public bool isActive;
    }

    enum GameMoment
    {
        Menu,
        GamePlay,
        FinalScreen
    }

    //Screen
    const int SCREEN_WIDTH = 1080;
    const int SCREEN_HEIGTH = 720;

    //Const
    const string TITLE = "GALAGA";
    static readonly Color COLOR = Color.SKYBLUE;
    static readonly Color TEXT_COLOR = Color.WHITE;
    static readonly Vector2 PLAYER_ORIGINAL_POSITION = new Vector2(SCREEN_WIDTH / 2, 600);
    static readonly Vector2 ENEMIES_ORIGINAL_POSITION = new Vector2(0, -70);
    const float PLAYER_SPEED = 500f;
    const float PLAYER_SHOOT_COLDDOWN = 1f;
    const float BULLET_SPEED = 400f;
    const float ENEMIES_SPEED = 100f;
    const float ENEMIES_Y_ADVANCE = 20f;
    const float MIN_SHOOT_ENEMIES_COLDDOWN = 0.5f;
    const float MAX_SHOOT_ENEMIES_COLDDOWN = 2f;
    const int ORIGINAL_LIFE = 3;

    //Images
    static Image imagePlayer = Raylib.LoadImage("assets/player.png");
    static Image alien1Image = Raylib.LoadImage("assets/alien1.png");
    static Image alien2Image = Raylib.LoadImage("assets/alien2.png");
    static Image alien3Image = Raylib.LoadImage("assets/alien3.png");
    static Image bulletImage = Raylib.LoadImage("assets/bullet.png");
    static Image explotionImage = Raylib.LoadImage("assets/explosion.png");

    //Textures
    static Texture2D texturePlayer;
    static Texture2D textureAlien1;
    static Texture2D textureAlien2;
    static Texture2D textureAlien3;
    static Texture2D textureBullet;
    static Texture2D explotionTexture;

    //Player
    static Character player;
    static Bullet[] playerBulletsArray = new Bullet[5];
    static Rectangle playerBound;

    //Enemies
    static Character[,] enemiesArray = new Character[3, 5];
    static Vector2[,] originalPositionArray = new Vector2[3, 5];
    static int rows = enemiesArray.GetLength(0);
    static int column = enemiesArray.GetLength(1);
    static int direction = 1;
    static bool directionChange = false;
    static Bullet[] enemyBulletsArray = new Bullet[10];
    static Rectangle[,] enemyBounds = new Rectangle[3, 5];

    //Delta and Timers
    static float delta;
    static float playerTime = 10;
    static float shootEnemiesTimer = 0;
    static float shootEnemiesCouldDown = 0;

    //GamePlay
    static GameMoment gameMoment = GameMoment.Menu;
    static int highScore = 0;
    static int score = 0;

    //Path
    private static string path = Path.Combine(Environment.CurrentDirectory, @"data.txt");

    public static void Main()
    {
        Raylib.InitWindow(SCREEN_WIDTH, SCREEN_HEIGTH, "Galaga-Raylib");

        if (File.Exists(path))
        {
            int num;
            if (int.TryParse(File.ReadAllText(path), out num))
            {
                highScore = num;
            }
        }

        //Textures
        texturePlayer = Raylib.LoadTextureFromImage(imagePlayer);
        Raylib.UnloadImage(imagePlayer);

        textureAlien1 = Raylib.LoadTextureFromImage(alien1Image);
        Raylib.UnloadImage(alien1Image);

        textureAlien2 = Raylib.LoadTextureFromImage(alien2Image);
        Raylib.UnloadImage(alien2Image);

        textureAlien3 = Raylib.LoadTextureFromImage(alien3Image);
        Raylib.UnloadImage(alien3Image);

        textureBullet = Raylib.LoadTextureFromImage(bulletImage);
        Raylib.UnloadImage(bulletImage);

        explotionTexture = Raylib.LoadTextureFromImage(explotionImage);
        Raylib.UnloadImage(explotionImage);

        //Instantiate functions
        InstantiateBullets();
        InstantiatePlayer();
        shootEnemiesCouldDown = (float)(random.NextDouble() * (MAX_SHOOT_ENEMIES_COLDDOWN - MIN_SHOOT_ENEMIES_COLDDOWN) + MIN_SHOOT_ENEMIES_COLDDOWN);
        InstantiateEnemies();

        while (!Raylib.WindowShouldClose())
        {
            delta = Raylib.GetFrameTime();

            Raylib.BeginDrawing();

            Raylib.ClearBackground(COLOR);

            switch (gameMoment)
            {
                case GameMoment.Menu:
                    DrawMenu();
                    break;
                case GameMoment.GamePlay:
                    UpdatePlayer();
                    if (!directionChange)
                    {
                        UpdateEnemies();
                    }
                    DrawUi();
                    break;
                case GameMoment.FinalScreen:
                    DrawFinalScreen();
                    break;
                default:
                    break;
            }

            Raylib.EndDrawing();
        }

        ReWriteScore();
        Raylib.CloseWindow();
    }

    static private void InstantiateBullets()
    {
        Bullet bullet = new Bullet();
        bullet.image = textureBullet;
        bullet.isActive = false;
        for (int i = 0; i < playerBulletsArray.Length; i++)
        {
            playerBulletsArray[i] = bullet;
        }
        for (int i = 0; i < enemyBulletsArray.Length; i++)
        {
            enemyBulletsArray[i] = bullet;
        }
    }

    static private void InstantiatePlayer()
    {
        player.position = PLAYER_ORIGINAL_POSITION;
        player.image = texturePlayer;
        player.isActive = true;
        player.shootDirection = -1;
        player.life = ORIGINAL_LIFE;
        player.bulletPool = playerBulletsArray;
    }

    static private void InstantiateEnemies()
    {
        rows = enemiesArray.GetLength(0);
        column = enemiesArray.GetLength(1);
        float diffWidht = textureAlien1.Width;
        float diffHeight = textureAlien1.Height;

        Character enemy = new Character();
        enemy.position = ENEMIES_ORIGINAL_POSITION;
        enemy.bulletPool = enemyBulletsArray;
        enemy.isActive = true;
        enemy.shootDirection = 1;
        enemy.life = 1;

        for (int i = 0; i < rows; i++)
        {
            enemy.position.Y += diffHeight;
            if (i == 0)
            {
                enemy.image = textureAlien1;
            }
            else if (i == 1)
            {
                enemy.image = textureAlien2;
            }
            else if (i == 2)
            {
                enemy.image = textureAlien3;
            }
            enemy.position.X = ENEMIES_ORIGINAL_POSITION.X;
            for (int j = 0; j < column; j++)
            {
                enemy.position.X += diffWidht;
                originalPositionArray[i, j] = enemy.position;
                enemiesArray[i, j] = enemy;
            }
        }
    }

    static private void UpdatePlayer()
    {
        if (player.isActive)
        {
            ReadInput();
            ViewTexture(player);
            UpdateBullets(player);
            playerBound = new Rectangle(player.position.X - player.image.Width / 2, player.position.Y, player.image.Width, player.image.Height);
            CheckCollissions(playerBound, ref enemyBulletsArray, ref player);
        }
        else
        {
            EndGame();
        }
    }

    static private void UpdateEnemies()
    {
        int count = 0;
        shootEnemiesTimer += Raylib.GetFrameTime();
        UpdateBullets(enemiesArray[0, 0], ref enemyBulletsArray);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < column; j++)
            {
                if (enemiesArray[i, j].isActive)
                {
                    ViewTexture(enemiesArray[i, j]);
                    enemiesArray[i, j].position.X += ENEMIES_SPEED * delta * direction;
                    Rectangle bound = new Rectangle(enemiesArray[i, j].position.X - enemiesArray[i, j].image.Width / 2, enemiesArray[i, j].position.Y, enemiesArray[i, j].image.Width, enemiesArray[i, j].image.Height);
                    CheckCollissions(bound, ref playerBulletsArray, ref enemiesArray[i, j], true);
                    count++;
                    if (enemiesArray[i, j].position.X < 0 || enemiesArray[i, j].position.X > SCREEN_WIDTH)
                    {
                        directionChange = true;
                        ChangeDirection();
                        break;
                    }
                }
            }
        }
        if (count == 0)
        {
            EndGame();
        }
        if (shootEnemiesCouldDown < shootEnemiesTimer)
        {
            shootEnemiesTimer = 0;
            shootEnemiesCouldDown = (float)(random.NextDouble() * (MAX_SHOOT_ENEMIES_COLDDOWN - MIN_SHOOT_ENEMIES_COLDDOWN) + MIN_SHOOT_ENEMIES_COLDDOWN);
            SelectionEnemyShoot();
        }
    }

    static private void SelectionEnemyShoot()
    {
        int randomRow = random.Next(0, rows);
        int randomColumn = random.Next(0, column);
        if (enemiesArray[randomRow, randomColumn].isActive)
        {
            SelectionBullet(enemiesArray[randomRow, randomColumn].position);
            return;
        }
        SelectionEnemyShoot();
    }

    private static void CheckCollissions(Rectangle characterBound, ref Bullet[] bullets, ref Character character, bool isEnemy = false)
    {
        for (int i = 0; i < bullets.Length; i++)
        {
            if (Raylib.CheckCollisionRecs(characterBound, bullets[i].bound) && bullets[i].isActive)
            {
                bullets[i].isActive = false;
                character.life--;
                if (character.life <= 0)
                {
                    character.isActive = false;
                }
                if (isEnemy)
                {
                    score++;
                }
            }
        }
    }

    static private void ChangeDirection()
    {
        direction = direction * (-1);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < column; j++)
            {
                if (enemiesArray[i, j].isActive)
                {
                    enemiesArray[i, j].position.Y += 1;
                    if (direction < 0)
                    {
                        enemiesArray[i, j].position.X -= textureAlien1.Width;
                    }
                    else
                    {
                        enemiesArray[i, j].position.X += textureAlien1.Width;
                    }
                }
            }
        }
        directionChange = false;
    }

    static private void ReadInput()
    {
        playerTime += Raylib.GetFrameTime();
        if (Raylib.IsKeyDown(KeyboardKey.KEY_D) || Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT))
        {
            player.position.X += PLAYER_SPEED * delta;
        }
        else if (Raylib.IsKeyDown(KeyboardKey.KEY_A) || Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
        {
            player.position.X -= PLAYER_SPEED * delta;
        }
        if (Raylib.IsKeyDown(KeyboardKey.KEY_SPACE) && playerTime > PLAYER_SHOOT_COLDDOWN)
        {
            playerTime = 0;
            CharacterShoot(player);
        }
    }

    static private void ViewTexture(Character character)
    {
        Raylib.DrawTexture(character.image, (int)character.position.X - character.image.Width / 2, (int)character.position.Y, Color.WHITE);
    }

    static private void ViewTexture(Bullet bullet)
    {
        Raylib.DrawTexture(bullet.image, (int)bullet.position.X, (int)bullet.position.Y, Color.WHITE);
    }

    static private void CharacterShoot(Character character)
    {
        for (int i = 0; i < character.bulletPool.Length; i++)
        {
            if (!character.bulletPool[i].isActive)
            {
                character.bulletPool[i].position = character.position;
                character.bulletPool[i].isActive = true;
                return;
            }
        }
    }

    static private void UpdateBullets(Character character)
    {
        Rectangle boundBullet = new Rectangle();
        for (int i = 0; i < character.bulletPool.Length; i++)
        {
            if (character.bulletPool[i].isActive)
            {
                boundBullet = new Rectangle(character.bulletPool[i].position.X, character.bulletPool[i].position.Y, character.bulletPool[i].image.Width, character.bulletPool[i].image.Height);
                character.bulletPool[i].position.Y += character.shootDirection * BULLET_SPEED * delta;
                character.bulletPool[i].bound = boundBullet;
                if (character.bulletPool[i].position.Y > SCREEN_HEIGTH || character.bulletPool[i].position.Y < 0)
                {
                    character.bulletPool[i].isActive = false;
                }
                ViewTexture(character.bulletPool[i]);
            }
        }
    }

    static private void UpdateBullets(Character character, ref Bullet[] bullet)
    {
        Rectangle boundBullet = new Rectangle();
        for (int i = 0; i < bullet.Length; i++)
        {
            if (bullet[i].isActive)
            {
                boundBullet = new Rectangle(bullet[i].position.X, bullet[i].position.Y, bullet[i].image.Width, bullet[i].image.Height);
                bullet[i].bound = boundBullet;
                bullet[i].position.Y += character.shootDirection * BULLET_SPEED * delta;
                if (bullet[i].position.Y > SCREEN_HEIGTH || bullet[i].position.Y < 0)
                {
                    bullet[i].isActive = false;
                }
                ViewTexture(bullet[i]);
            }
        }
    }

    static private void SelectionBullet(Vector2 position)
    {
        for (int i = 0; i < enemyBulletsArray.Length; i++)
        {
            if (!enemyBulletsArray[i].isActive)
            {
                enemyBulletsArray[i].position = position;
                enemyBulletsArray[i].isActive = true;
                return;
            }
        }
    }

    static private void DrawMenu()
    {
        Raylib.DrawText(TITLE, SCREEN_WIDTH / 2 - 275, 50, 150, TEXT_COLOR);
        Raylib.DrawText("Click to start", SCREEN_WIDTH / 2 - 125, 350, 50, TEXT_COLOR);
        Raylib.DrawText($"Highscore: {highScore} points", SCREEN_WIDTH / 2 - 250, 600, 50, TEXT_COLOR);
        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) || Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
        {
            gameMoment = GameMoment.GamePlay;
        }
    }

    static private void DrawUi()
    {
        Raylib.DrawText($"Life: {player.life}", 10, 10, 50, TEXT_COLOR);
        Raylib.DrawText($"Score: {score}", 10, 50, 50, TEXT_COLOR);
    }

    static private void EndGame()
    {
        if (highScore < score)
        {
            highScore = score;
        }
        gameMoment = GameMoment.FinalScreen;
    }

    static private void DrawFinalScreen()
    {
        Raylib.DrawText(TITLE, SCREEN_WIDTH / 2 - 275, 50, 150, TEXT_COLOR);
        Raylib.DrawText($"GAME OVER", SCREEN_WIDTH / 2 - 100, 300, 20, TEXT_COLOR);
        Raylib.DrawText($"Score: {highScore} points", SCREEN_WIDTH / 2 - 100, 400, 30, TEXT_COLOR);
        Raylib.DrawText($"Highscore: {highScore} points", SCREEN_WIDTH / 2 - 100, 500, 30, TEXT_COLOR);
        Raylib.DrawText("Click to start", SCREEN_WIDTH / 2 - 125, 600, 30, TEXT_COLOR);

        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) || Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
        {
            ResetGame();
            gameMoment = GameMoment.GamePlay;
        }
    }

    static private void ResetGame()
    {
        player.position = PLAYER_ORIGINAL_POSITION;
        player.life = ORIGINAL_LIFE;
        player.isActive = true;

        score = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < column; j++)
            {
                enemiesArray[i, j].isActive = true;
                enemiesArray[i, j].position = originalPositionArray[i, j];
            }
        }
        for (int i = 0; i < enemyBulletsArray.Length; i++)
        {
            enemyBulletsArray[i].isActive = false;
        }
    }

    private static void ReWriteScore()
    {
        try
        {
            File.WriteAllText(path, highScore.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error to try write data: " + ex.Message);
        }
    }
}
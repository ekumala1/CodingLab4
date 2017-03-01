using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Coding_Lab_4
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // gameplay mechanics
        Vector2 window = new Vector2(800, 600);
        float initialBallSpeed;
        float aiPaddleSpeed;
        int numBricks;
        int initialPaddleSpeed, slimedPaddleSpeed, greasedPaddleSpeed;        

        // temporary or constant variables
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        Vector2 leftPaddle, ball, rightPaddle;
        Vector2 ballVelocity;
        Vector2 goalArea;
        Vector2 powerupPosition;
        KeyboardState lastKeyboardState;
        SoundEffectInstance music;
        int powerupType = 0;
        double powerupTimer = 0;
        float ballSpeed;
        int[] leftHealth;
        int[] rightHealth;
        string goalText;
        bool menuState = true, difficultyState, goalState, pauseState;
        double leftScore, rightScore;
        int brickWidth = 50;
        int brickHeight;
        bool frozen = false, grease = false, slimy = false, flexibility = false;
        int lastPaddle; // 1 for left, 2 for right
        int menuSelected = 1;
        int gamemode;
        int timer = 0;

        SpriteFont titleFont;

        public void drawRectangle(int x, int y, int width, int height, Color fill, Color outline)
        {
            // credit to Stack Overflow post
            // http://stackoverflow.com/questions/5751732/draw-rectangle-in-xna-using-spritebatch

            Texture2D outlineTexture = new Texture2D(graphics.GraphicsDevice, width+2, height+2);
            Texture2D fillTexture = new Texture2D(graphics.GraphicsDevice, width, height);

            Color[] outlineData = new Color[(width+2) * (height+2)];
            for (int i = 0; i < outlineData.Length; ++i) outlineData[i] = outline;
            outlineTexture.SetData(outlineData);

            Vector2 outlineCoor = new Vector2(x-1, y-1);

            Color[] fillData = new Color[width * height];
            for (int i = 0; i < fillData.Length; ++i) fillData[i] = fill;
            fillTexture.SetData(fillData);

            Vector2 fillCoor = new Vector2(x, y);

            spriteBatch.Draw(outlineTexture, outlineCoor, outline);
            spriteBatch.Draw(fillTexture, fillCoor, fill);
        }

        public void drawLine(int x, int y, int width, int height, Color color)
        {
            if (width >= height)
                for (int i = x; i < x + width; i++)
                    spriteBatch.Draw(Content.Load<Texture2D>("dot"), new Vector2(i, y + (i - x) * height / width), Color.White);
            else
                for (int i = y; i < y + height; i++)
                    spriteBatch.Draw(Content.Load<Texture2D>("dot"), new Vector2(x + (i - y) * width / height, i), Color.White);
        }

        public bool collide(Vector2 coordinates1, Vector2 coordinates2, int radius)
        {
            Vector2 center1, center2;

            center1 = coordinates1 + new Vector2(radius, radius);
            center2 = coordinates2 + new Vector2(radius, radius);

            if (Math.Sqrt(Math.Pow(center2.X - center1.X, 2) + Math.Pow(center2.Y - center1.Y, 2)) <= 64)
                return true;

            return false;
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = (int)window.X;
            graphics.PreferredBackBufferHeight = (int)window.Y;
            goalArea.Y = 100;

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            powerupPosition = window;
            music = Content.Load<SoundEffect>("music").CreateInstance();
            music.IsLooped = true;
            music.Play();

            base.Initialize();
        }

        /// <summary>
        ///// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("Courier New");
            titleFont = Content.Load<SpriteFont>("Magneto");

            leftPaddle = new Vector2(brickWidth + 10, 50f);
            ball = new Vector2(window.X / 2, window.Y / 2);
            rightPaddle = new Vector2(window.X - 24 - (brickWidth + 10), 536f);
            goalText = "";

            ballSpeed = 5;
            ballVelocity = new Vector2(ballSpeed, 0);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (!goalState)
            {
                #region collisions with paddle
                if (ball.X <= leftPaddle.X + 24 && ball.Y + 32 >= leftPaddle.Y && ball.Y <= leftPaddle.Y + 64)
                {
                    ballVelocity = new Vector2(ballSpeed, (ball.Y - (leftPaddle.Y - 32) - 48) / 48 * ballSpeed);
                    if (!menuState && !difficultyState) Content.Load<SoundEffect>("hit").Play();
                    lastPaddle = 1;
                }
                else if (ball.X + 32 >= rightPaddle.X && ball.Y + 32 >= rightPaddle.Y && ball.Y <= rightPaddle.Y + 64)
                {
                    ballVelocity = new Vector2(-ballSpeed, (ball.Y - (rightPaddle.Y - 32) - 48) / 48 * ballSpeed);
                    if (!menuState && !difficultyState) Content.Load<SoundEffect>("hit").Play();
                    lastPaddle = 2;
                }
                #endregion

                // collisions with top and bottom walls
                if (ball.Y <= 0 || ball.Y >= window.Y - 32) ballVelocity.Y *= -1;

                #region goals
                if (ball.X <= -32)
                {
                    rightScore += 1;
                    ball = new Vector2(window.X / 2, window.Y / 2);
                    ballVelocity = new Vector2(ballSpeed, new Random().Next((int)-ballSpeed, (int)ballSpeed));
                    Content.Load<SoundEffect>("buzzer").Play();
                    goalState = true;

                    if (!menuState && !difficultyState)
                    {
                        goalText = "GOAL!  You have gained one point!";
                        if (rightScore >= 10 && !menuState && !difficultyState) goalText += "\nYou have also won!";
                        goalText += "\nClick to continue!";
                    }

                    goalArea.X = 200;
                }
                else if (ball.X >= window.X)
                {
                    leftScore += 1;
                    ball = new Vector2(300f, 300f);
                    ballVelocity = new Vector2(-ballSpeed, new Random().Next((int)-ballSpeed, (int)ballSpeed));
                    Content.Load<SoundEffect>("buzzer").Play();
                    goalState = true;

                    if (!menuState && !difficultyState)
                    {
                        goalText = "GOAL!  Your enemy has gained one point!";
                        if (leftScore >= 10 && !menuState && !difficultyState) goalText += "\nYou have lost.";
                        goalText += "\nClick to continue!";
                    }

                    goalArea.X = 175;
                }
                #endregion

                #region collisions with bricks
                if (!menuState && !difficultyState)
                {
                    if (ball.X <= brickWidth && leftHealth[(int)(ball.Y / brickHeight)] != 0)
                    {
                        if (leftHealth[(int)(ball.Y / brickHeight)] == 1) rightScore += 1.0 / numBricks;

                        leftHealth[(int)(ball.Y / brickHeight)]--;
                        ballVelocity.X *= -1;
                        Content.Load<SoundEffect>("brick").Play();
                    }
                    else if (ball.X + 32 >= window.X - brickWidth && rightHealth[(int)(ball.Y / brickHeight)] != 0)
                    {
                        if (rightHealth[(int)(ball.Y / brickHeight)] == 1) leftScore += 1.0 / numBricks;

                        rightHealth[(int)(ball.Y / brickHeight)]--;
                        ballVelocity.X *= -1;
                        Content.Load<SoundEffect>("brick").Play();
                    }
                }
                #endregion

                #region collision with powerups
                if (collide(ball, powerupPosition, 32))
                {
                    powerupPosition = window; // move it off-screen

                    if (powerupType == 1) frozen = true;
                    else if (powerupType == 2) ballSpeed += 5;
                    else if (powerupType == 3) grease = true;
                    else if (powerupType == 4) ballSpeed -= 2;
                    else if (powerupType == 5) slimy = true;
                    else if (powerupType == 6) flexibility = true;

                    powerupTimer = 3;
                }
                #endregion

                #region ai paddle stuff
                if (!menuState && !difficultyState)
                {
                    if (gamemode == 1)
                    {
                        if (!(frozen && lastPaddle == 2) && !(slimy && lastPaddle == 2) && (!grease && lastPaddle == 2))
                        {
                            KeyboardState ks = Keyboard.GetState();
                            if (ks.IsKeyDown(Keys.S))
                                leftPaddle.Y += initialPaddleSpeed;
                            else if (ks.IsKeyDown(Keys.W))
                                leftPaddle.Y -= initialPaddleSpeed;

                            if (flexibility)
                            {
                                if (ks.IsKeyDown(Keys.Right))
                                    leftPaddle.X += initialPaddleSpeed;
                                else if (ks.IsKeyDown(Keys.Left))
                                    leftPaddle.X -= initialPaddleSpeed;
                            }
                        }
                        else if (slimy && lastPaddle == 2)
                        {
                            KeyboardState ks = Keyboard.GetState();
                            if (ks.IsKeyDown(Keys.S))
                                leftPaddle.Y += slimedPaddleSpeed;
                            else if (ks.IsKeyDown(Keys.W))
                                leftPaddle.Y -= slimedPaddleSpeed;

                            if (flexibility)
                            {
                                if (ks.IsKeyDown(Keys.Right))
                                    leftPaddle.X += slimedPaddleSpeed;
                                else if (ks.IsKeyDown(Keys.Left))
                                    leftPaddle.X -= slimedPaddleSpeed;
                            }
                        }
                        else if (grease && lastPaddle == 2)
                        {
                            KeyboardState ks = Keyboard.GetState();
                            if (ks.IsKeyDown(Keys.S))
                                leftPaddle.Y += greasedPaddleSpeed;
                            else if (ks.IsKeyDown(Keys.W))
                                leftPaddle.Y -= greasedPaddleSpeed;

                            if (flexibility)
                            {
                                if (ks.IsKeyDown(Keys.Right))
                                    leftPaddle.X += greasedPaddleSpeed;
                                else if (ks.IsKeyDown(Keys.Left))
                                    leftPaddle.X -= greasedPaddleSpeed;
                            }
                        }
                    }
                    else if (gamemode == 2)
                    {
                        if (ball.X <= 100 && !(frozen && lastPaddle == 2))
                        {
                            if (ball.Y > leftPaddle.Y) leftPaddle.Y += aiPaddleSpeed;
                            else if (ball.Y < leftPaddle.Y) leftPaddle.Y -= aiPaddleSpeed;
                        }
                    }
                }
                #endregion

                #region player paddle stuff
                if (!(frozen && lastPaddle == 1) && !(slimy && lastPaddle == 1) && !(grease && lastPaddle == 1))
                {
                    KeyboardState ks = Keyboard.GetState();
                    if (ks.IsKeyDown(Keys.Down) && rightPaddle.Y + 64 <= window.Y)
                        rightPaddle.Y += initialPaddleSpeed;
                    else if (ks.IsKeyDown(Keys.Up) && rightPaddle.Y >= 0)
                        rightPaddle.Y -= initialPaddleSpeed;

                    if (flexibility)
                    {
                        if (ks.IsKeyDown(Keys.Right) && rightPaddle.X <= window.X - 24 - (brickWidth + 10))
                            rightPaddle.X += initialPaddleSpeed;
                        else if (ks.IsKeyDown(Keys.Left) && rightPaddle.X >= window.X / 2 + 20)
                            rightPaddle.X -= initialPaddleSpeed;
                    }
                }
                else if (slimy && lastPaddle == 1)
                {
                    KeyboardState ks = Keyboard.GetState();
                    if (ks.IsKeyDown(Keys.Down) && rightPaddle.Y + 64 <= window.Y)
                        rightPaddle.Y += slimedPaddleSpeed;
                    else if (ks.IsKeyDown(Keys.Up) && rightPaddle.Y >= 0)
                        rightPaddle.Y -= slimedPaddleSpeed;

                    if (flexibility)
                    {
                        if (ks.IsKeyDown(Keys.Right) && rightPaddle.X <= window.X - 24 - (brickWidth + 10))
                            rightPaddle.X += slimedPaddleSpeed;
                        else if (ks.IsKeyDown(Keys.Left) && rightPaddle.X >= window.X / 2 + 20)
                            rightPaddle.X -= slimedPaddleSpeed;
                    }
                }
                else if (grease && lastPaddle == 1)
                {
                    KeyboardState ks = Keyboard.GetState();
                    if (ks.IsKeyDown(Keys.Down) && rightPaddle.Y + 64 <= window.Y)
                        rightPaddle.Y += greasedPaddleSpeed;
                    else if (ks.IsKeyDown(Keys.Up) && rightPaddle.Y >= 0)
                        rightPaddle.Y -= greasedPaddleSpeed;

                    if (flexibility)
                    {
                        if (ks.IsKeyDown(Keys.Right) && rightPaddle.X <= window.X - 24 - (brickWidth + 10))
                            rightPaddle.X += greasedPaddleSpeed;
                        else if (ks.IsKeyDown(Keys.Left) && rightPaddle.X >= window.X / 2 + 20)
                            rightPaddle.X -= greasedPaddleSpeed;
                    }
                }
                #endregion

                #region powerup stuff
                if (timer % 300 == 0 && !menuState && !difficultyState)
                {
                    powerupType = new Random().Next(1, 7);

                    powerupPosition.X = new Random().Next(brickWidth + 100, (int)window.X - brickWidth - 100);
                    powerupPosition.Y = new Random().Next(0, (int)window.Y - 32);
                }

                if (powerupTimer > 0) powerupTimer -= 0.01;
                else
                {
                    frozen = false;
                    ballSpeed = initialBallSpeed;
                    grease = false;
                    slimy = false;
                    flexibility = false;
                    rightPaddle.X = window.X - 24 - (brickWidth + 10);

                }
                #endregion

                timer++;
                ball += ballVelocity;
            }

            // TODO: Add your update logic here
            if (menuState || difficultyState)
            {
                if (leftPaddle.Y + 64 < ball.Y) leftPaddle.Y = ball.Y - 64;
                else if (leftPaddle.Y > ball.Y + 32) leftPaddle.Y = ball.Y + 32;

                if (rightPaddle.Y + 64 < ball.Y) rightPaddle.Y = ball.Y - 64;
                else if (rightPaddle.Y > ball.Y + 32) rightPaddle.Y = ball.Y + 32;
            }
            else if (goalState)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    if (rightScore >= 10 || leftScore >= 10) menuState = true;
                    else
                    {
                        goalState = false;
                        goalText = "";

                        for (int i = 0; i < numBricks; i++)
                        {
                            leftHealth[i] = 2;
                            rightHealth[i] = 2;
                        }
                    }
                }
            }

            base.Update(gameTime);
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            string powerupString;
            switch (powerupType)
            {
                case 1: powerupString = "Freeze: "; break;
                case 2: powerupString = "Speed: "; break;
                case 3: powerupString = "Grease: "; break;
                case 4: powerupString = "Heavy: "; break;
                case 5: powerupString = "Slime: "; break;
                case 6: powerupString = "Flexibility: "; break;
                default: powerupString = ""; break;
            }
            powerupString += Math.Round(powerupTimer, 0);

            spriteBatch.Draw(Content.Load<Texture2D>("background"), Vector2.Zero, Color.White);

            spriteBatch.Draw(Content.Load<Texture2D>("left_paddle"), leftPaddle, Color.White);
            spriteBatch.Draw(Content.Load<Texture2D>("small_ball"), ball, Color.White);
            spriteBatch.Draw(Content.Load<Texture2D>("right_paddle"), rightPaddle, Color.White);
            spriteBatch.DrawString(spriteFont, goalText, goalArea, Color.Yellow);

            if (powerupTimer > 0)
                spriteBatch.DrawString(spriteFont, powerupString, new Vector2(window.X / 2 - 50, window.Y - 100), Color.Yellow);

            for (int i = 0; i < numBricks; i++)
            {
                int x = (int)window.X - brickWidth;
                int y = i * brickHeight;

                if (leftHealth[i] > 0) drawRectangle(0, y, brickWidth, brickHeight, Color.White, Color.Black);
                if (leftHealth[i] == 1)
                {
                    drawLine(0, y + 50, 20, 5, Color.Black);
                    drawLine(20, y + 55, 5, -10, Color.Black);
                    drawLine(25, y + 45, 10, -5, Color.Black);
                    drawLine(35, y + 40, 15, 10, Color.Black);
                }

                if (rightHealth[i] > 0) drawRectangle(x, y, brickWidth, brickHeight, Color.White, Color.Black);
                if (rightHealth[i] == 1)
                {
                    drawLine(x, y + 50, 20, 5, Color.Black);
                    drawLine(x + 20, y + 55, 5, -10, Color.Black);
                    drawLine(x + 25, y + 45, 10, -5, Color.Black);
                    drawLine(x + 35, y + 40, 15, 10, Color.Black);
                }
            }

            spriteBatch.DrawString(spriteFont, "" + Math.Round(leftScore, 1), new Vector2(window.X / 2 - 50, 0), Color.Yellow);
            spriteBatch.DrawString(spriteFont, "" + Math.Round(rightScore, 1), new Vector2(window.X / 2 + 25, 0), Color.Yellow);

            switch (powerupType)
            {
                case 1:
                    spriteBatch.Draw(Content.Load<Texture2D>("freeze"), powerupPosition, Color.White);
                    break;
                case 2:
                    spriteBatch.Draw(Content.Load<Texture2D>("speed"), powerupPosition, Color.White);
                    break;
                case 3:
                    spriteBatch.Draw(Content.Load<Texture2D>("grease"), powerupPosition, Color.White);
                    break;
                case 4:
                    spriteBatch.Draw(Content.Load<Texture2D>("heavy"), powerupPosition, Color.White);
                    break;
                case 5:
                    spriteBatch.Draw(Content.Load<Texture2D>("slime"), powerupPosition, Color.White);
                    break;
                case 6:
                    spriteBatch.Draw(Content.Load<Texture2D>("flexibility"), powerupPosition, Color.White);
                    break;
            }

            if (menuState)
            {
                // credit to Stack Overflow post
                // http://stackoverflow.com/questions/6632723/how-to-make-a-texture2d-50-transparent-xna
                spriteBatch.Draw(Content.Load<Texture2D>("transparency"), new Vector2(0, 0), Color.White);

                spriteBatch.DrawString(titleFont, "PONG", new Vector2(200, 50), Color.White);
                spriteBatch.DrawString(titleFont, "Breaker", new Vector2(250, 100), Color.White);

                if (menuSelected == 1)
                {
                    spriteBatch.DrawString(titleFont, "Play in 1 vs. 1 mode", new Vector2(100, 350), Color.Yellow);
                    spriteBatch.DrawString(titleFont, "Play in vs. AI mode", new Vector2(100, 400), Color.White);

                    if (lastKeyboardState.IsKeyDown(Keys.Enter) && Keyboard.GetState().IsKeyUp(Keys.Enter))
                    {
                        menuState = false;
                        menuSelected = 1;
                        difficultyState = true;
                        gamemode = menuSelected;
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.Down))
                    {
                        menuSelected = 2;
                        Content.Load<SoundEffect>("menuChange").Play(0.3f, 0, 0);
                    }
                }
                else if (menuSelected == 2)
                {
                    spriteBatch.DrawString(titleFont, "Play in 1 vs. 1 mode", new Vector2(100, 350), Color.White);
                    spriteBatch.DrawString(titleFont, "Play in vs. AI mode", new Vector2(100, 400), Color.Yellow);

                    if (lastKeyboardState.IsKeyDown(Keys.Enter) && Keyboard.GetState().IsKeyUp(Keys.Enter))
                    {
                        menuState = false;
                        menuSelected = 2;
                        difficultyState = true;
                        gamemode = menuSelected;
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.Up))
                    {
                        menuSelected = 1;
                        Content.Load<SoundEffect>("menuChange").Play(0.3f, 0, 0);
                    }
                }

            }
            else if (difficultyState)
            {
                spriteBatch.Draw(Content.Load<Texture2D>("transparency"), new Vector2(0, 0), Color.White);

                switch (menuSelected)
                {
                    case 1:
                        spriteBatch.DrawString(titleFont, "Easy", new Vector2(310, 150), Color.Yellow);
                        spriteBatch.DrawString(titleFont, "Normal", new Vector2(310, 225), Color.White);
                        spriteBatch.DrawString(titleFont, "Hard", new Vector2(310, 300), Color.White);

                        if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                        {
                            initialBallSpeed = 5;
                            ballSpeed = initialBallSpeed;
                            aiPaddleSpeed = 7;
                            initialPaddleSpeed = 6;
                            slimedPaddleSpeed = 3;
                            greasedPaddleSpeed = 8;
                            #region initialize bricks
                            numBricks = 3;
                            brickHeight = (int)window.Y / numBricks;
                            leftHealth = new int[numBricks];
                            rightHealth = new int[numBricks];

                            for (int i = 0; i < numBricks; i++)
                            {
                                leftHealth[i] = 2;
                                rightHealth[i] = 2;
                            }
                            #endregion

                            ballVelocity = new Vector2(ballSpeed, ballSpeed);
                            difficultyState = false;
                        }
                        break;
                    case 2:
                        spriteBatch.DrawString(titleFont, "Easy", new Vector2(310, 150), Color.White);
                        spriteBatch.DrawString(titleFont, "Normal", new Vector2(310, 225), Color.Yellow);
                        spriteBatch.DrawString(titleFont, "Hard", new Vector2(310, 300), Color.White);

                        if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                        {
                            initialBallSpeed = 8;
                            ballSpeed = initialBallSpeed;
                            aiPaddleSpeed = 15;
                            initialPaddleSpeed = 8;
                            slimedPaddleSpeed = 5;
                            greasedPaddleSpeed = 10;
                            #region initialize bricks
                            numBricks = 5;
                            brickHeight = (int)window.Y / numBricks;
                            leftHealth = new int[numBricks];
                            rightHealth = new int[numBricks];

                            for (int i = 0; i < numBricks; i++)
                            {
                                leftHealth[i] = 2;
                                rightHealth[i] = 2;
                            }
                            #endregion

                            ballVelocity = new Vector2(ballSpeed, ballSpeed);
                            difficultyState = false;
                        }
                        break;
                    case 3:
                        spriteBatch.DrawString(titleFont, "Easy", new Vector2(310, 150), Color.White);
                        spriteBatch.DrawString(titleFont, "Normal", new Vector2(310, 225), Color.White);
                        spriteBatch.DrawString(titleFont, "Hard", new Vector2(310, 300), Color.Yellow);

                        if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                        {
                            initialBallSpeed = 9;
                            ballSpeed = initialBallSpeed;
                            aiPaddleSpeed = 11;
                            initialPaddleSpeed = 9;
                            slimedPaddleSpeed = 4;
                            greasedPaddleSpeed = 10;
                            #region initialize bricks
                            numBricks = 7;
                            brickHeight = (int)window.Y / numBricks;
                            leftHealth = new int[numBricks];
                            rightHealth = new int[numBricks];

                            for (int i = 0; i < numBricks; i++)
                            {
                                leftHealth[i] = 2;
                                rightHealth[i] = 2;
                            }
                            #endregion

                            ballVelocity = new Vector2(ballSpeed, ballSpeed);
                            difficultyState = false;
                        }
                        break;
                }

                if (lastKeyboardState.IsKeyDown(Keys.Up) && Keyboard.GetState().IsKeyUp(Keys.Up)
                    && menuSelected > 1)
                {
                    menuSelected--;
                    Content.Load<SoundEffect>("menuChange").Play();
                }
                else if (lastKeyboardState.IsKeyDown(Keys.Down) && Keyboard.GetState().IsKeyUp(Keys.Down)
                    && menuSelected < 3)
                {
                    menuSelected++;
                    Content.Load<SoundEffect>("menuChange").Play();
                }
            }
            else if (pauseState)
            {
                spriteBatch.Draw(Content.Load<Texture2D>("transparency"), new Vector2(0, 0), Color.White);


            }

            lastKeyboardState = Keyboard.GetState();
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

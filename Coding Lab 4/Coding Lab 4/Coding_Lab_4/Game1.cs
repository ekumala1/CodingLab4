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
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        Vector2 window;
        Vector2 leftPaddle, ball, rightPaddle;
        Vector2 ballVelocity;
        Vector2 goalArea;
        bool[] brokenLeft;
        bool[] brokenRight;
        string goalText;
        bool goalState;
        int leftScore, rightScore;
        int brickWidth = 50;

        // gameplay mechanics
        float speed = 3;
        int numBricks = 5;

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

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 600;
            graphics.PreferredBackBufferHeight = 600;
            window = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
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

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            brokenLeft = new bool[numBricks];
            brokenRight = new bool[numBricks];

            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("Courier New");
            leftPaddle = new Vector2(brickWidth + 10, 50f);
            ball = new Vector2(300f, 300f);
            rightPaddle = new Vector2(window.X - 24 - (brickWidth + 10), 536f);
            ballVelocity = new Vector2(speed, speed);
            goalText = "";

            for (int i=0; i<numBricks; i++)
            {
                brokenLeft[i] = false;
                brokenRight[i] = false;
            }

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

            // TODO: Add your update logic here
            if (goalState)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    goalState = false;
                    goalText = "";
                }
            }
            else
            {
                #region ball stuff
                // collisions with paddle
                if (ball.X + 32 >= rightPaddle.X && ball.Y + 32 >= rightPaddle.Y && ball.Y <= rightPaddle.Y + 64)
                    ballVelocity = new Vector2(-speed, (ball.Y - (rightPaddle.Y - 32) - 48) / 48 * speed);
                else if (ball.X <= leftPaddle.X + 24 && ball.Y + 32 >= leftPaddle.Y && ball.Y <= leftPaddle.Y + 64)
                    ballVelocity = new Vector2(speed, (ball.Y - (leftPaddle.Y - 32) - 48) / 48 * speed);

                // collisions with top and bottom walls
                if (ball.Y <= 0 || ball.Y >= window.Y - 32) ballVelocity.Y *= -1;

                // goals
                if (ball.X <= -32)
                {
                    ball = new Vector2(300f, 300f);
                    ballVelocity = new Vector2(speed, new Random().Next((int)-speed, (int)speed));
                    goalState = true;
                    goalText = "GOAL!  You have gained one point!\nClick to continue!";
                    goalArea.X = 200;
                    rightScore += 1;
                }
                else if (ball.X >= window.X)
                {
                    ball = new Vector2(300f, 300f);
                    ballVelocity = new Vector2(-speed, new Random().Next((int)-speed, (int)speed));
                    goalState = true;
                    goalText = "GOAL!  Your enemy has gained one point!\nClick to continue!";
                    goalArea.X = 175;
                    leftScore += 1;
                }

                // collisions with bricks


                ball += ballVelocity;
                #endregion

                #region ai paddle stuff
                if (ball.Y > leftPaddle.Y) leftPaddle.Y += speed - 1;
                else if (ball.Y < leftPaddle.Y) leftPaddle.Y -= speed - 1;
                #endregion

                #region player paddle stuff
                rightPaddle.Y = Mouse.GetState().Y;
                #endregion
            }

            Console.WriteLine(ball);

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
            spriteBatch.Draw(Content.Load<Texture2D>("left_paddle"), leftPaddle, Color.White);
            spriteBatch.Draw(Content.Load<Texture2D>("small_ball"), ball, Color.White);
            spriteBatch.Draw(Content.Load<Texture2D>("right_paddle"), rightPaddle, Color.White);
            spriteBatch.DrawString(spriteFont, goalText, goalArea, Color.Yellow);

            for (int i=0; i<numBricks; i++)
            {
                int height = (int)(window.Y / numBricks);
                int y = i * height;

                if (brokenLeft[i]) drawRectangle(0, y, brickWidth, height, Color.Red, Color.Black);
                if (brokenRight[i]) drawRectangle((int)window.X - brickWidth, y, brickWidth, height, Color.Red, Color.Black);
            }

            spriteBatch.DrawString(spriteFont, "Computer: " + leftScore, Vector2.Zero, Color.Yellow);
            spriteBatch.DrawString(spriteFont, "You: " + rightScore, new Vector2(window.X - 70, 0), Color.Yellow);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

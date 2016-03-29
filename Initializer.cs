using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace RainFrogs
{


    class Initializer : GameState
    {
        RainFrog ingame;
        KeyboardState keyboard;
        KeyboardState oldkeyboardState;
        SpriteFont output18;

        string tag = "initializer";
        string entry = "";
        string outputText = "";

        public Initializer(RainFrog ingame)
        {
            this.ingame = ingame;
            this.oldkeyboardState = Keyboard.GetState();
            output18 = ingame.Content.Load<SpriteFont>("Output18pt");

        }

        public string getTag() { return this.tag; }

        public void update(GameTime gametime)
        {
            keyboard = Keyboard.GetState();
            char key;
            TryConvertKeyboardInput(keyboard, oldkeyboardState, out key);
            oldkeyboardState = keyboard;
            //Debug.Print(Convert.ToString(key));
            if (!key.Equals('\0'))
            {
                entry += Convert.ToString(key);
            }
            if (keyboard.IsKeyDown(Keys.Escape)) { entry = ""; }
            string[] outputSplit = outputText.Split('\n');
            if (outputSplit.Length > ingame.GraphicsDevice.Viewport.Height / 32)
            {
                outputText = "\n" + string.Join("\n", outputSplit.Skip(outputSplit.Length - ingame.GraphicsDevice.Viewport.Height / 32));
                //outputText = outputSplit.Skip(outputSplit.Length - 25).; 
            }

            //Debug.Print(Convert.ToString(entry.Length)+" "+entry);

        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(output18,
                "> " + entry + "\n" + outputText,
                new Vector2(40, 20),
                Playing.getColor("White"));
            spriteBatch.End();
        }
        public void parseCommand(string command)
        {
            string[] key = command.ToUpper().Split(' ');
            switch (key[0])
            {
                case "HELP": { outputText += "\n<" + command + ">" + "\nCOMMANDS:\nNew           -> Start a new game\nSaved         -> List of saved games\nLoad [name]   -> Load a saved game\nDelete [name] -> Delete a saved game\nClear         -> Clear the console\nAbout         -> About the game\nQuit          -> Exit the game"; break; }
                case "CLEAR": { outputText = ""; break; }
                case "NEW": { if (key.Length == 1) { ingame.changeGameState("playing"); } else if (key[1].Equals("SKIP")) { ingame.changeGameState("playing"); } else { goto default; } break; }
                case "LOAD": { outputText += "\n<" + command + ">" + "\nOutput text if is broken"; break; }
                case "SAVED": { outputText += "\n<" + command + ">" + "\nList saved games here"; break; }
                case "ABOUT": { outputText += "\n<" + command + ">" + "\nJust like make game~\nby Rain Frogs\n\nAmbiance by reillocb \nhttps://soundcloud.com/reilloc"; break; }
                case "DELETE": { outputText += "\n<" + command + ">" + "\nFile successfully deleted (not currently implemented)"; break; }
                case "QUIT": { ingame.Exit(); break; }
                case "EXIT": { ingame.Exit(); break; }
                default: { outputText += "\n<" + command + ">" + "\nI do not recognize your command: '" + command.Split(' ')[0] + "' \n-- Please type 'help' for a list of accepted commands."; break; }
            }
        }
        public void entering() { }
        public void leaving() { this.entry = ""; this.outputText = ""; }


        public bool TryConvertKeyboardInput(KeyboardState keyboard, KeyboardState oldKeyboard, out char key)
        {
            Keys[] keys = keyboard.GetPressedKeys();

            bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            for (int i = 0; i < keys.Length; i++)
            {
                if (!oldKeyboard.IsKeyDown(keys[i]))
                {
                    switch (keys[i])
                    {
                        //Alphabet keys
                        case Keys.A: if (shift) { key = 'A'; } else { key = 'a'; } return true;
                        case Keys.B: if (shift) { key = 'B'; } else { key = 'b'; } return true;
                        case Keys.C: if (shift) { key = 'C'; } else { key = 'c'; } return true;
                        case Keys.D: if (shift) { key = 'D'; } else { key = 'd'; } return true;
                        case Keys.E: if (shift) { key = 'E'; } else { key = 'e'; } return true;
                        case Keys.F: if (shift) { key = 'F'; } else { key = 'f'; } return true;
                        case Keys.G: if (shift) { key = 'G'; } else { key = 'g'; } return true;
                        case Keys.H: if (shift) { key = 'H'; } else { key = 'h'; } return true;
                        case Keys.I: if (shift) { key = 'I'; } else { key = 'i'; } return true;
                        case Keys.J: if (shift) { key = 'J'; } else { key = 'j'; } return true;
                        case Keys.K: if (shift) { key = 'K'; } else { key = 'k'; } return true;
                        case Keys.L: if (shift) { key = 'L'; } else { key = 'l'; } return true;
                        case Keys.M: if (shift) { key = 'M'; } else { key = 'm'; } return true;
                        case Keys.N: if (shift) { key = 'N'; } else { key = 'n'; } return true;
                        case Keys.O: if (shift) { key = 'O'; } else { key = 'o'; } return true;
                        case Keys.P: if (shift) { key = 'P'; } else { key = 'p'; } return true;
                        case Keys.Q: if (shift) { key = 'Q'; } else { key = 'q'; } return true;
                        case Keys.R: if (shift) { key = 'R'; } else { key = 'r'; } return true;
                        case Keys.S: if (shift) { key = 'S'; } else { key = 's'; } return true;
                        case Keys.T: if (shift) { key = 'T'; } else { key = 't'; } return true;
                        case Keys.U: if (shift) { key = 'U'; } else { key = 'u'; } return true;
                        case Keys.V: if (shift) { key = 'V'; } else { key = 'v'; } return true;
                        case Keys.W: if (shift) { key = 'W'; } else { key = 'w'; } return true;
                        case Keys.X: if (shift) { key = 'X'; } else { key = 'x'; } return true;
                        case Keys.Y: if (shift) { key = 'Y'; } else { key = 'y'; } return true;
                        case Keys.Z: if (shift) { key = 'Z'; } else { key = 'z'; } return true;

                        //Decimal keys
                        case Keys.D0: if (shift) { key = ')'; } else { key = '0'; } return true;
                        case Keys.D1: if (shift) { key = '!'; } else { key = '1'; } return true;
                        case Keys.D2: if (shift) { key = '@'; } else { key = '2'; } return true;
                        case Keys.D3: if (shift) { key = '#'; } else { key = '3'; } return true;
                        case Keys.D4: if (shift) { key = '$'; } else { key = '4'; } return true;
                        case Keys.D5: if (shift) { key = '%'; } else { key = '5'; } return true;
                        case Keys.D6: if (shift) { key = '^'; } else { key = '6'; } return true;
                        case Keys.D7: if (shift) { key = '&'; } else { key = '7'; } return true;
                        case Keys.D8: if (shift) { key = '*'; } else { key = '8'; } return true;
                        case Keys.D9: if (shift) { key = '('; } else { key = '9'; } return true;

                        //Decimal numpad keys
                        case Keys.NumPad0: key = '0'; return true;
                        case Keys.NumPad1: key = '1'; return true;
                        case Keys.NumPad2: key = '2'; return true;
                        case Keys.NumPad3: key = '3'; return true;
                        case Keys.NumPad4: key = '4'; return true;
                        case Keys.NumPad5: key = '5'; return true;
                        case Keys.NumPad6: key = '6'; return true;
                        case Keys.NumPad7: key = '7'; return true;
                        case Keys.NumPad8: key = '8'; return true;
                        case Keys.NumPad9: key = '9'; return true;

                        //Special keys
                        case Keys.OemTilde: if (shift) { key = '~'; } else { key = '`'; } return true;
                        case Keys.OemSemicolon: if (shift) { key = ':'; } else { key = ';'; } return true;
                        case Keys.OemQuotes: if (shift) { key = '"'; } else { key = '\''; } return true;
                        case Keys.OemQuestion: if (shift) { key = '?'; } else { key = '/'; } return true;
                        case Keys.OemPlus: if (shift) { key = '+'; } else { key = '='; } return true;
                        case Keys.OemPipe: if (shift) { key = '|'; } else { key = '\\'; } return true;
                        case Keys.OemPeriod: if (shift) { key = '>'; } else { key = '.'; } return true;
                        case Keys.OemOpenBrackets: if (shift) { key = '{'; } else { key = '['; } return true;
                        case Keys.OemCloseBrackets: if (shift) { key = '}'; } else { key = ']'; } return true;
                        case Keys.OemMinus: if (shift) { key = '_'; } else { key = '-'; } return true;
                        case Keys.OemComma: if (shift) { key = '<'; } else { key = ','; } return true;
                        case Keys.Space: key = ' '; return true;
                        case Keys.Back: key = '\0'; if (entry.Length > 0) { entry = entry.Substring(0, entry.Length - 1); }; return true;
                        //change enter command to put result up and clear entry
                        case Keys.Enter: key = '\0'; this.parseCommand(entry); entry = ""; return true;
                    }
                }
            }

            key = (char)0;
            return false;
        }

        public int getTileAlpha()
        {
            throw new NotImplementedException();
        }


        public int getAlpha(Drawable prop)
        {
            throw new NotImplementedException();
        }


        public string getState()
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Game.Board;
using static Utils.Utils;

namespace Game {
    internal class Program {
        private static void Main(string[] args) {
            bool debug = false, playerchoice = true, isnumber, isletter, game = true, Enemyturn;
            int seed = default, count, remaining;
            string inputLetterstr, inputNumberstr, digit, pos; //change name of pos
            ConsoleKey inputKey;
            char[] checkifdigit;
            (Letter, int) enemyInput = default;
            (Letter, int)[] indexes;
            string[] enemySelected = new string[2];
            string[] selected = new string[2];

#if DEBUG
            debug = true;
            seed = 10;
#endif
            Board Player = new Board(auto: debug, seed: seed);
            Console.Clear();
            Player.Print();
            _ = InputToKey("Press enter to start");
            Board Enemy = new Board(auto: true, seed: seed);
            while (game) {
                Enemyturn = true;
                playerchoice = true;
                Console.Clear();
                Enemy.Print(hide_pieces: !debug);
                Console.WriteLine("");
                Player.Print(enemySelected, hide_pieces: false);
                enemySelected[0] = default;
                enemySelected[1] = default;
                inputLetterstr = default;
                inputNumberstr = default;
                do {
                    do {
                        Console.WriteLine("Where?");
                        do {
                            inputKey = Console.ReadKey(intercept: true).Key;
                            checkifdigit = inputKey.ToString().ToCharArray();
                            isnumber = Char.IsDigit(checkifdigit[checkifdigit.Length - 1]);
                            isletter = Enum.IsDefined(typeof(Letter), inputKey.ToString());
                        } while (!isnumber && !isletter);
                        if (isletter) {
                            inputLetterstr = inputKey.ToString();
                            selected[0] = inputLetterstr;
                            if (inputNumberstr == null) Enemy.Print(selected.ToArray(), hide_pieces: true);
                        } else if (isnumber) {
                            digit = checkifdigit[checkifdigit.Length - 1].ToString();
                            inputNumberstr = digit == "0" ? "10" : digit;
                            selected[1] = inputNumberstr;
                            if (inputLetterstr == null) Enemy.Print(selected, hide_pieces: true);
                        }
                        Console.Clear();
                        Enemy.Print(selected, hide_pieces: true);
                    } while (inputLetterstr == null || inputNumberstr == null);
                    pos = Enemy[inputLetterstr, inputNumberstr];
                    if (pos != "X" && pos != "O") {
                        if (pos == null) {
                            Enemy[inputLetterstr, inputNumberstr] = "O";
                            Console.Clear();
                            Enemy.Print(selected, hide_pieces: true);
                            _ = InputToKey("Miss");
                            playerchoice = false;
                        } else {
                            Enemy[inputLetterstr, inputNumberstr] = "X";
                            Console.Clear();
                            Enemy.Print(selected, hide_pieces: true);
                            _ = InputToKey("Hit");
                            remaining = Enemy.Count(Piece.possibleIds);
                            if (remaining == 0) {
                                //TODO testar isto
                                Console.Clear();
                                Enemy.Print();
                                Player.Print();
                                InputToKey("Player wins");
                                game = false;
                                playerchoice = false;
                                Enemyturn = false;
                            }
                        }
                    } else {
                        Console.WriteLine("Already chosen");
                    }
                    inputNumberstr = default;
                    inputLetterstr = default;
                    enemySelected[0] = default;
                    enemySelected[1] = default;
                } while (playerchoice);

                while (Enemyturn) {
                    enemyInput = default;
                    count = Player.Count("X");
                    if (count == 0) {
                        do {
                            enemyInput = Player.Random();
                            pos = Player[enemyInput];
                        } while (pos == "X" || pos == "O");

                    } else {
                        indexes = Player.GetAllIndexes("X").ToArray();
                        //check if its a line
                        foreach ((Letter, int) i in indexes) {
                            if (i.Item1 + 1 <= Letter.J) {
                                pos = Player[i.Item1 + 1, i.Item2];
                                if (i.Item1 - 1 >= Letter.A && pos == "X") {
                                    pos = Player[i.Item1 - 1, i.Item2];
                                    if (pos != "X" && pos != "O") {
                                        enemyInput = (i.Item1 - 1, i.Item2);
                                        break;
                                    }
                                }

                            }
                            if (i.Item1 - 1 >= Letter.A) {
                                pos = Player[i.Item1 - 1, i.Item2];
                                if (i.Item1 + 1 <= Letter.J) {
                                    pos = Player[i.Item1 + 1, i.Item2];
                                    if (pos != "X" && pos != "O") {
                                        enemyInput = (i.Item1 + 1, i.Item2);
                                        break;
                                    }
                                }
                            }
                            if (i.Item2 - 1 >= 1) {
                                pos = Player[i.Item1, i.Item2 - 1];
                                if (i.Item2 + 1 <= 10) {
                                    pos = Player[i.Item1, i.Item2 + 1];
                                    if (pos != "X" && pos != "O") {
                                        enemyInput = (i.Item1, i.Item2 + 1);
                                        break;
                                    }
                                }
                            }
                            if (i.Item2 + 1 <= 10) {
                                pos = Player[i.Item1, i.Item2 + 1];
                                if (i.Item2 - 1 >= 1) {
                                    pos = Player[i.Item1, i.Item2 - 1];
                                    if (pos != "X" && pos != "O") {
                                        enemyInput = (i.Item1, i.Item2 - 1);
                                        break;
                                    }
                                }
                            }
                        }
                        //if its not a line
                        if (enemyInput == default) {
                            foreach ((Letter, int) i in indexes) {
                                if (i.Item1 + 1 <= Letter.J) {
                                    pos = Player[i.Item1 + 1, i.Item2];
                                    if (pos != "X" && pos != "O") {
                                        enemyInput = (i.Item1 + 1, i.Item2);
                                        break;
                                    }
                                }
                                if (i.Item1 - 1 >= Letter.A) {
                                    pos = Player[i.Item1 - 1, i.Item2];
                                    if (pos != "X" && pos != "O") {
                                        enemyInput = (i.Item1 - 1, i.Item2);
                                        break;
                                    }
                                }
                                if (i.Item2 + 1 <= 10) {
                                    pos = Player[i.Item1, i.Item2 + 1];
                                    if (pos != "X" && pos != "O") {
                                        enemyInput = (i.Item1, i.Item2 + 1);
                                        break;
                                    }
                                }
                                if (i.Item2 - 1 >= 1) {
                                    pos = Player[i.Item1, i.Item2 - 1];
                                    if (pos != "X" && pos != "O") {
                                        enemyInput = (i.Item1, i.Item2 - 1);
                                        break;
                                    }
                                }

                            }
                        }
                        //if all checked around
                        if (enemyInput == default) {
                            do {
                                enemyInput = Player.Random();
                                pos = Player[enemyInput];
                            } while (pos == "X" || pos == "O");
                        }
                    }
                    enemySelected[0] = enemyInput.Item1.ToString();
                    enemySelected[1] = enemyInput.Item2.ToString();
                    Console.Clear();
                    if (pos == null) {
                        Player[enemyInput] = "O";
                        Player.Print(enemySelected);
                        _ = InputToKey("Enemy miss");
                        Enemyturn = false;
                    } else {
                        Player[enemyInput] = "X";
                        Player.Print(enemySelected);
                        _ = InputToKey("Enemy hit");
                    }
                    //WIN?
                    remaining = Player.Count(Piece.possibleIds);
                    if (remaining == 0) {
                        Enemy.Print();
                        Player.Print();
                        InputToKey("Enemy wins");
                        game = false;
                        Enemyturn = false;
                    }
                }
            }

        }
    }
}

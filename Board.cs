using System;
using System.Collections.Generic;
using System.Linq;
using static Utils.Utils;

namespace Game {
    internal class Board {
#pragma warning disable IDE1006 // Naming Styles
        private string[,] booard { get; set; }
        private List<Piece> pieces { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        public static string[] defaultPieces = new string[] {
            "Carrier",
            "Battleship", "Battleship",
            "Destroyer", "Destroyer",
            "Submarine", "Submarine",
            "Boat patrol","Boat patrol","Boat patrol","Boat patrol",
        };

        public static Random random = default;
        public string this[string letter, string number] {
            get {
                return this.booard[this.LetterToNumber(letter), int.Parse(number) - 1];
            }
            set {
                this.booard[this.LetterToNumber(letter), int.Parse(number) - 1] = value;
            }
        }
        public string this[string letter, int number] {
            get {
                return this.booard[this.LetterToNumber(letter), number - 1];
            }
            set {
                this.booard[this.LetterToNumber(letter), number - 1] = value;
            }
        }
        public string this[int letter, int number] {
            get {
                return this.booard[letter, number - 1];
            }
            set {
                this.booard[letter, number - 1] = value;
            }
        }
        public string this[Letter letter, int number] {
            get {
                return this[this.LetterToNumber(letter), number];
            }
            set {
                this[this.LetterToNumber(letter), number] = value;
            }
        }
        public string this[(Letter, int) pos] {
            get {
                return this[pos.Item1, pos.Item2];
            }
            set {
                this[pos.Item1, pos.Item2] = value;
            }
        }
        public string this[Piece p] {
            get { return this[p.HeadLocation]; }
            set { this[p.HeadLocation] = value; }
        }

        public Board(string[] piecesStr = null, bool auto = false, int seed = default) {
            //make default
            if (piecesStr == null) piecesStr = Board.defaultPieces;
            this.pieces = new List<Piece>();
            Piece[] piecesleft = new Piece[piecesStr.Length];
            //make pieces left
            for (int i = 0; i < (piecesStr).Length; i++) {
                piecesleft[i] = new Piece(name: (piecesStr)[i]);
            }
            //make empty board
            this.booard = new string[10, 10];
            if (auto) {
                Letter headLetter;
                int headNumber;
                Piece.Orientations orientation;
                if (seed != default) random = new Random(seed);
                else random = new Random();
                foreach (Piece i in piecesleft) {
                    do {
                        headLetter = (Letter)random.Next(0, 9);
                        headNumber = random.Next(0, 9);
                        orientation = (Piece.Orientations)random.Next(0, 3);
                        i.HeadLocation = (headLetter, headNumber);
                        i.Orientation = orientation;
                    } while (!this.ValidPlace(i));
                    this.AddPiece(i);
                }
            } else this.BuildBoard(piecesleft);
        }

        private void BuildBoard(Piece[] piecesLeft) {
            //initalizers
            bool isnumber, isletter, validLocation;
            ConsoleKey inputKey;
            char[] checkifdigit;
            Letter inputLetter;
            int inputNumber;
            string digit;
            ConsoleKey[] directionOptions = default;
            string[] directionOptionsStr, inputStr = new string[2];

            //Make the inputs possible
            foreach (Piece iPiece in piecesLeft) {
                validLocation = false;
                do {
                    Console.Clear();
                    this.Print(showPieceOutline:true);
                    Array.Clear(inputStr, 0, inputStr.Length);
                    //get the input of the place
                    do {
                        Console.WriteLine($"Where to put {iPiece.Name} (size {iPiece.Size})");
                        do {
                            inputKey = Console.ReadKey(intercept: true).Key;
                            checkifdigit = inputKey.ToString().ToCharArray();
                            isnumber = char.IsDigit(checkifdigit[checkifdigit.Length - 1]);
                            isletter = Enum.IsDefined(typeof(Letter), inputKey.ToString());
                        } while (!isnumber && !isletter);//enquanto nao for valido
                        if (isletter) { //Letra (ja verificada se existe)
                            inputStr[0] = inputKey.ToString();
                            inputStr[0] = inputStr[0];
                        } else if (isnumber) { //numero
                            digit = checkifdigit[checkifdigit.Length - 1].ToString();
                            inputStr[1] = digit == "0" ? "10" : digit;
                        }
                        Console.Clear();
                        this.Print(inputStr, showPieceOutline:true);
                    } while ((inputStr[0] == null || inputStr[1] == null));
                    //show where is putting and let user choose side orientation
                    if (this[inputStr[0], inputStr[1]] == null) this[inputStr[0], inputStr[1]] = iPiece.Id;
                    else {
                        Console.WriteLine("Already chosen");
                        continue;
                    }
                    Enum.TryParse(inputStr[0], out inputLetter);
                    int.TryParse(inputStr[1], out inputNumber);
                    iPiece.HeadLocation = (inputLetter, inputNumber);
                    directionOptions = this.PossibleDirections(iPiece);
                    if (directionOptions.Length == 0) {
                        Console.Clear();
                        this.Print(inputStr);
                        _ = InputToKey("Invalid Location");
                        this[inputStr[0], inputStr[1]] = null;
                        iPiece.HeadLocation = default;
                    } else {
                        validLocation = true;
                    }
                } while (!validLocation);
                directionOptionsStr = new string[directionOptions.Length];
                for (int i = 0; i < directionOptions.Length; i++) {
                    directionOptionsStr[i] = directionOptions[i] switch
                    {
                        ConsoleKey.UpArrow => "Orientation up",
                        ConsoleKey.DownArrow => "Orientation down",
                        ConsoleKey.LeftArrow => "Orientation left",
                        ConsoleKey.RightArrow => "Orientation right",
                        _ => throw new NotImplementedException(),
                    };
                }
                Console.Clear();
                this.Print(inputStr, showPieceOutline:true);
                inputKey = InputsToKey("What orientation?",
                    directionOptions,
                    directionOptionsStr,
                    clear: false
                    );
                iPiece.Orientation = inputKey switch
                {
                    ConsoleKey.UpArrow => Piece.Orientations.Up,
                    ConsoleKey.DownArrow => Piece.Orientations.Down,
                    ConsoleKey.LeftArrow => Piece.Orientations.Left,
                    ConsoleKey.RightArrow => Piece.Orientations.Right,
                    _ => throw new NotImplementedException(),
                };

                this.AddPiece(iPiece);
            }//Repeats until all pieces are in play
        }
        private ConsoleKey[] PossibleDirections(Piece p) {
            List<ConsoleKey> ret = new List<ConsoleKey>();
            bool up = true, down = true, left = true, right = true;
            Letter headLetter = p.HeadLocation.Item1;
            int headNumber = p.HeadLocation.Item2;
            for (int i = 1; i < p.Size; i++) {

                //esta assim para salvar ser mais rapido(so entra na condicao se nao estiver ja falso)
                if (up) if (headLetter - i < Letter.A || this[headLetter - i, headNumber] != null) {
                        up = false;
                    }
                if (down) if (headLetter + i > Letter.J || this[headLetter + i, headNumber] != null) {
                        down = false;
                    }
                //Ver se est abem posto (se tiver mal posto as indiczacoes estao trocadas
                if (right) if (headNumber + i > 10 || this[headLetter, headNumber + i] != null) {
                        right = false;
                    }
                if (left) if (headNumber - i < 1 || this[headLetter, headNumber - i] != null) {
                        left = false;
                    }
            }
            if (up) ret.Add(ConsoleKey.UpArrow);
            if (down) ret.Add(ConsoleKey.DownArrow);
            if (left) ret.Add(ConsoleKey.LeftArrow);
            if (right) ret.Add(ConsoleKey.RightArrow);
            return ret.ToArray();
        }
        [Obsolete("Not tested and not to use, use AddPiece(Piece p) instead")]
        private void AddPiece(Piece p, Letter letter, int number) {
            //IndexOutOfRangeException() - fora do index
            //InvalidCastException() - ja esta ocupado (apenas se building esta true)
            number = number == 0 ? 10 : number - 1;
            bool plustrueminusfalse = (p.Orientation == Piece.Orientations.Up) || (p.Orientation == Piece.Orientations.Left) ? true : false;
            bool adderOnLetterTrueAdderOnNumberFalse = p.Orientation == Piece.Orientations.Up || p.Orientation == Piece.Orientations.Down ? true : false;
            for (int i = 1; i < p.Size; i += plustrueminusfalse ? 1 : -1) {//é i-- pq para cima o numero desce
                this.ValidPlace(
                    adderOnLetterTrueAdderOnNumberFalse ? (int)letter + i : (int)letter,
                    adderOnLetterTrueAdderOnNumberFalse ? number : number + i,
                    building: true
                    );
            } //Se passar nao houve exceptions
            for (int i = 1; i < p.Size; i += plustrueminusfalse ? 1 : -1) {
                this.booard[
                    adderOnLetterTrueAdderOnNumberFalse ? (int)letter + i : (int)letter,
                    adderOnLetterTrueAdderOnNumberFalse ? number : number + i
                    ] = p.Id;

            }

        }
        private void AddPiece(Piece p) {
            int addonLetter = p.Orientation switch
            {
                Piece.Orientations.Up => -1,
                Piece.Orientations.Down => 1,
                Piece.Orientations.Left => 0,
                Piece.Orientations.Right => 0,
                _ => throw new NotImplementedException(),
            };
            int addonNumber = p.Orientation switch
            {
                Piece.Orientations.Left => -1,
                Piece.Orientations.Right => 1,
                Piece.Orientations.Up => 0,
                Piece.Orientations.Down => 0,
                _ => throw new NotImplementedException(),
            };
            for (int i = 0; i < p.Size; i++) {
                this[p.HeadLocation.Item1 + (i * addonLetter), p.HeadLocation.Item2 + (i * addonNumber)] = p.Id;
            }
            this.pieces.Add(p);
        }
        [Obsolete("Not used")]
        private int OnBoard(string str) {
            //returns the number in the array, -1 if it's not on board
            int ret;
            if (int.TryParse(str, out ret)) { //se true e numero
                if (1 <= ret && ret <= 10) //entre 1 e 10
                    return ret - 1; //-1 para devolver do array
                else {
                    Console.WriteLine("Invalid or out of range");
                    return -1;
                }
            } else if (Enum.IsDefined(typeof(Letter), str)) {//é letra dentro do enum
                return (int)Enum.Parse(typeof(Letter), str);


            } else {
                Console.WriteLine("Invalid or out of range");
                return -1;
            }
        }
        [Obsolete("Not ready to use", error: true)]
        private void RemovePiece(Piece p, ref Piece[] piecesleft) {
            for (int i = 0; i < 10; i++) {
                for (int i2 = 0; i < 10; i++) {
                    if (this.booard[i, i2] == p.Id) this.booard[i, i2] = null;
                }
            }
            int temp = Array.IndexOf(piecesleft, null);
            piecesleft[temp] = p;
        }
        [Obsolete("No current use, not to use")]
        private void ValidPlace(int number, int number2, bool building = false) {
            //IndexOutOfRangeException() - fora do index
            //InvalidCastException() - ja esta ocupado (apenas se building esta true)
            if (
                number < 0 || number > 9
                || number2 < 0 || number2 > 9
                ) {
                throw new IndexOutOfRangeException();
            } else if (building) {
                if (this.booard[number, number2] != null) {
                    throw new InvalidCastException();
                }
            }
        }
        private bool ValidPlace(Piece p) {
            int addonLetter = p.Orientation switch
            {
                Piece.Orientations.Up => -1,
                Piece.Orientations.Down => 1,
                Piece.Orientations.Left => 0,
                Piece.Orientations.Right => 0,
                _ => throw new NotImplementedException(),
            };
            int addonNumber = p.Orientation switch
            {
                Piece.Orientations.Left => -1,
                Piece.Orientations.Right => 1,
                Piece.Orientations.Up => 0,
                Piece.Orientations.Down => 0,
                _ => throw new NotImplementedException(),
            };
            for (int i = 0; i < p.Size; i++) {
                try {
                    if (this[
                        p.HeadLocation.Item1 + (i * addonLetter),
                        p.HeadLocation.Item2 + (i * addonNumber)
                        ] != null)
                        return false;
                } catch (IndexOutOfRangeException) {
                    return false;
                }
            }
            //if reaches here then all the spaces are unnocupied
            return true;
        }

        public void Print(string selected_line, bool hide_pieces = false, bool showPieceOutline = false) => this.Print(new string[] { selected_line }, hide_pieces: hide_pieces, showPieceOutline:showPieceOutline); //Just a redirect

        //TODO fazer isto em ves do de cima, para nao so o utilizador saber onde o inimigo atirou mas tambem para saber onde esta aquela peca que o utilizador quer dizer a orientaçao
        public void Print(string[] selected_lines = null, bool hide_pieces = false, bool showPieceOutline = false) {
            if (hide_pieces == true) showPieceOutline = false;
            if (selected_lines is null) selected_lines = new string[] { };
            string selected, hided, highlited = " ";//change name for highlited later?
            int selectedint;
            Piece p;
            (Letter, int)[] PieceLocations = default;
            if (showPieceOutline) PieceLocations = this.getAllPieceLocations();
            List<Letter> letters_selected = new List<Letter>();
            List<int> numbers_selected = new List<int>();
            foreach (string i in selected_lines) {
                if (i == null || i == "") continue;
                else if (Enum.IsDefined(typeof(Letter), i)) {
                    letters_selected.Add(this.StringToLetter(i));
                } else if (int.TryParse(i, out selectedint) && selectedint <= 10 && 1 <= selectedint) {
                    numbers_selected.Add(selectedint);
                }
            }

            Console.Write("  ");
            for (int i = 1; i <= 10; i++) {
                selected = numbers_selected.Contains(i) ? "*" : " ";
                Console.Write($"{selected}{i}{selected}");
            }
            Console.WriteLine("");
            for (Letter i = Letter.A; i <= Letter.J; i++) {
                Console.Write($"{(letters_selected.Contains(i) ? "*" : " ")}{i}");
                for (int i2 = 1; i2 <= 10; i2++) {
                    hided = this[i, i2];
                    if ((
                        hide_pieces && hided != "X" && hided != "O"
                        ) || (
                        hided == null
                        )) hided = " ";
                    if (showPieceOutline && !(hided == " ")) {
                        p = this.getPieceFromLocation((i, i2));
                        if (p == default) highlited = " ";
                        else {
                            highlited = this.getPieceFromLocation((i, i2)).Orientation switch {
                                Piece.Orientations.Down => "|",
                                Piece.Orientations.Up => "|",
                                Piece.Orientations.Left => "-",
                                Piece.Orientations.Right => "-",
                                _ => throw new NotImplementedException()
                            };
                        }
                    }
                    else highlited = " ";
                    Console.Write($"{highlited}{hided}{highlited}");
                }
                Console.WriteLine("");
            }
        }
        public enum Letter {
            A,
            B,
            C,
            D,
            E,
            F,
            G,
            H,
            I,
            J,
        }
        public int Count(string a) {
            int ret = 0;
            foreach (string i in this.booard) {
                if (a == i) ret += 1;
            }
            return ret;
        }
        public (Letter, int)[] getAllPieceLocations(List<Piece> p = default) {
            if (p == default) p = this.pieces;
            List<(Letter, int)> ret = new List<(Letter, int)>();
            foreach(Piece iPiece in p) {
                foreach((Letter, int) i in iPiece.Locations) {
                    ret.Add(i);
                }
            }
            return ret.ToArray();
        }
        public Piece getPieceFromLocation((Letter, int) pos, List<Piece> p = default) {
            if (p == default) p = this.pieces;
            foreach (Piece iPiece in p) {
                if (iPiece.Locations.Contains(pos)) return iPiece;
            }
            return default;
        }


        public int Count(string[] a) {
            int ret = 0;
            foreach (string i in this.booard) {
                if (a.Contains(i)) ret += 1;
            }
            return ret;
        }

        public (Letter, int) GetIndex(string a) {
            for (Letter i = Letter.A; i <= Letter.J; i++) {
                for (int i2 = 1; i2 <= 10; i2++) {
                    if (this[i, i2] == a) return (i, i2);
                }
            }
            throw new KeyNotFoundException();
        }
        public List<(Letter, int)> GetAllIndexes(string a) {
            List<(Letter, int)> ret = new List<(Letter, int)>();
            for (Letter i = Letter.A; i <= Letter.J; i++) {
                for (int i2 = 1; i2 <= 10; i2++) {
                    if (this[i, i2] == a) ret.Add((i, i2));
                }
            }
            return ret;
        }

        public (Letter, int) Random(string x = null) {
            if (x is null) {
                (Letter, int) retTuple;
                retTuple.Item1 = (Letter)random.Next((int)Letter.A, (int)Letter.J);
                retTuple.Item2 = random.Next(1, 10);
                return retTuple;
            } else {
                (Letter, int)[] indexes;
                indexes = this.GetAllIndexes(x).ToArray();
                return indexes[random.Next(indexes.Length)];
            }
        }
        private bool isnull(Piece p) {
            if (p == null) return true;
            else return false;
        }

        private string NumberToLetter(int a) => Enum.GetName(typeof(Letter), a);
        private Letter StringToLetter(string a) {
            Enum.TryParse(a, out Letter b);
            return b;
        }
        private int LetterToNumber(string a) => (int)Enum.Parse(typeof(Letter), a);
        private int LetterToNumber(Letter a) => (int)a;
    }
}


using System;
using System.Collections.Generic;
using System.Text;

namespace Game {
    internal class Piece {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Size { get; }
        private (Board.Letter, int) headLocation;
        public (Board.Letter, int) HeadLocation {
            set {
                this.headLocation = value;
                if (value == default) Array.Clear(this.Locations, 0, this.Locations.Length);
                else {
                    bool HasOri = true;
                    for (int i = 0; i < this.Locations.Length && HasOri; i++) {
                        switch (this.Orientation) {
                            case Orientations.Up:
                                this[i] = (value.Item1 - i, value.Item2);
                                break;
                            case Orientations.Down:
                                this[i] = (value.Item1 + i, value.Item2);
                                break;
                            case Orientations.Left:
                                this[i] = (value.Item1, value.Item2 - i);
                                break;
                            case Orientations.Right:
                                this[i] = (value.Item1, value.Item2 + i);
                                break;
                            default:
                                HasOri = false;
                                break;
                        }
                    }
                }
            }
            get => this.headLocation;
        }
        public (Board.Letter, int) this[int a] {
            get => this.Locations[a];
            private set {
                this.Locations[a] = value;
            }
        }
        private Orientations orientation;

        public Orientations Orientation {
            get => this.orientation;
            set {
                this.orientation = value;
                this.HeadLocation = this.HeadLocation; //para fazer Locations
            }
        }
        public (Board.Letter, int)[] Locations { get; set; }
        public static string[] possibleIds = new string[] { "C", "B", "D", "S", "P" };
        public enum Orientations {
            Up,
            Down,
            Left,
            Right
        }
        public Piece(string name) {
            this.Name = name;
            switch (name) {
                case "Carrier":
                    this.Size = 5;
                    this.Id = "C";
                    break;
                case "Battleship":
                    this.Size = 4;
                    this.Id = "B";
                    break;
                case "Destroyer":
                    this.Size = 3;
                    this.Id = "D";
                    break;
                case "Submarine":
                    this.Size = 3;
                    this.Id = "S";
                    break;
                case "Boat patrol":
                    this.Size = 2;
                    this.Id = "P";
                    break;
            }
            this.Locations = new (Board.Letter, int)[this.Size];
        }
        public Piece(string name, Orientations orientation) : this(name) {
            this.Orientation = orientation;
        }
        public Piece(string name, Board.Letter letter, int number) : this(name) {
            this.HeadLocation = (letter, number);
        }
        public Piece(string name, Board.Letter letter, int number, Orientations orientation) : this(name, letter, number) {
            this.Orientation = orientation;
        }
        private void AddToBoard(Board b) { //not ready to use
            b[nameof(this.HeadLocation.Item1), this.HeadLocation.Item2.ToString()] = this.Id;
        }
        [Obsolete("Not ready to use")]
        private void AddToBoard(Board b, Board.Letter letter, int number) {
            this.HeadLocation = (letter, number);
            this.AddToBoard(b);
        }
    }
}
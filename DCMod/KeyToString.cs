﻿using System.Collections.Generic;
using UnityEngine.InputSystem;

// ChatGPT is great
namespace DCMod {
    public class KeyHelpers {
        public static readonly Dictionary<Key, string> keyToStringMapping = new Dictionary<Key, string> {
            { Key.Space, "Space" },
            { Key.Enter, "Enter" },
            { Key.Tab, "Tab" },
            { Key.Backquote, "Backquote" },
            { Key.Quote, "Quote" },
            { Key.Semicolon, ":" },
            { Key.Comma, "," },
            { Key.Period, "." },
            { Key.Slash, "/" },
            { Key.Backslash, "\\" },
            { Key.LeftBracket, "[" },
            { Key.RightBracket, "]" },
            { Key.Minus, "-" },
            { Key.Equals, "=" },
            { Key.A, "A" },
            { Key.B, "B" },
            { Key.C, "C" },
            { Key.D, "D" },
            { Key.E, "E" },
            { Key.F, "F" },
            { Key.G, "G" },
            { Key.H, "H" },
            { Key.I, "I" },
            { Key.J, "J" },
            { Key.K, "K" },
            { Key.L, "L" },
            { Key.M, "M" },
            { Key.N, "N" },
            { Key.O, "O" },
            { Key.P, "P" },
            { Key.Q, "Q" },
            { Key.R, "R" },
            { Key.S, "S" },
            { Key.T, "T" },
            { Key.U, "U" },
            { Key.V, "V" },
            { Key.W, "W" },
            { Key.X, "X" },
            { Key.Y, "Y" },
            { Key.Z, "Z" },
            { Key.Digit1, "1" },
            { Key.Digit2, "2" },
            { Key.Digit3, "3" },
            { Key.Digit4, "4" },
            { Key.Digit5, "5" },
            { Key.Digit6, "6" },
            { Key.Digit7, "7" },
            { Key.Digit8, "8" },
            { Key.Digit9, "9" },
            { Key.Digit0, "0" },
            { Key.LeftShift, "LShift" },
            { Key.RightShift, "RShift" },
            { Key.LeftAlt, "LAlt" },
            { Key.RightAlt, "RAlt" },
            { Key.LeftCtrl, "LCtrl" },
            { Key.RightCtrl, "RCtrl" },
            { Key.LeftWindows, "LWindows" },
            { Key.RightWindows, "RWindows" },
            { Key.ContextMenu, "Context menu" },
            { Key.Escape, "ESC" },
            { Key.LeftArrow, "Left arrow" },
            { Key.RightArrow, "Right arrow" },
            { Key.UpArrow, "Up arrow" },
            { Key.DownArrow, "Down arrow" },
            { Key.Backspace, "Backspace" },
            { Key.PageDown, "Page down" },
            { Key.PageUp, "Page up" },
            { Key.Home, "Home" },
            { Key.End, "End" },
            { Key.Insert, "Insert" },
            { Key.Delete, "Delete" },
            { Key.CapsLock, "Caps lock" },
            { Key.NumLock, "Num lock" },
            { Key.PrintScreen, "Print screen" },
            { Key.ScrollLock, "Scroll lock" },
            { Key.Pause, "Pause" },
            { Key.NumpadEnter, "Numpad enter" },
            { Key.NumpadDivide, "Numpad /" },
            { Key.NumpadMultiply, "Numpad *" },
            { Key.NumpadPlus, "Numpad +" },
            { Key.NumpadMinus, "Numpad -" },
            { Key.NumpadPeriod, "Numpad ." },
            { Key.NumpadEquals, "Numpad =" },
            { Key.Numpad0, "Numpad 0" },
            { Key.Numpad1, "Numpad 1" },
            { Key.Numpad2, "Numpad 2" },
            { Key.Numpad3, "Numpad 3" },
            { Key.Numpad4, "Numpad 4" },
            { Key.Numpad5, "Numpad 5" },
            { Key.Numpad6, "Numpad 6" },
            { Key.Numpad7, "Numpad 7" },
            { Key.Numpad8, "Numpad 8" },
            { Key.Numpad9, "Numpad 9" },
            { Key.F1, "F1" },
            { Key.F2, "F2" },
            { Key.F3, "F3" },
            { Key.F4, "F4" },
            { Key.F5, "F5" },
            { Key.F6, "F6" },
            { Key.F7, "F7" },
            { Key.F8, "F8" },
            { Key.F9, "F9" },
            { Key.F10, "F10" },
            { Key.F11, "F11" },
            { Key.F12, "F12" },
            { Key.OEM1, "OEM1"},
            { Key.OEM2, "OEM2"},
            { Key.OEM3, "OEM3"},
            { Key.OEM4, "OEM4"},
            { Key.OEM5, "OEM5"},
            { Key.IMESelected, "IMESelected" }
        };

        /// <summary>
        /// Returns the friendly name of a key.
        /// </summary>
        /// <param name="key">Key code.</param>
        /// <returns>Friendly, short name of the key.</returns>
        public static string GetStringForKey(Key key) {
            return keyToStringMapping.TryGetValue(key, out string result) ? result : key.ToString();
        }
    }
}

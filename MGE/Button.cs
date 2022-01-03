namespace MGE;

public enum Button
{
	/// <summary> No Key pressed </summary>
	None = 0,

	// Offset: 0 ~ 32, Any: 250

	#region Keyboard

	/// <summary> BACKSPACE key. </summary>
	KB_Backspace = 8,
	/// <summary> TAB key. </summary>
	KB_Tab = 9,
	/// <summary> ENTER key. </summary>
	KB_Enter = 13,
	/// <summary> CAPS LOCK key. </summary>
	KB_CapsLock = 20,
	/// <summary> ESC key. </summary>
	KB_Escape = 27,
	/// <summary> SPACEBAR key. </summary>
	KB_Space = 32,
	/// <summary> PAGE UP key. </summary>
	KB_PageUp = 33,
	/// <summary> PAGE DOWN key. </summary>
	KB_PageDown = 34,
	/// <summary> END key. </summary>
	KB_End = 35,
	/// <summary> HOME key. </summary>
	KB_Home = 36,
	/// <summary> LEFT ARROW key. </summary>
	KB_Left = 37,
	/// <summary> UP ARROW key. </summary>
	KB_Up = 38,
	/// <summary> RIGHT ARROW key. </summary>
	KB_Right = 39,
	/// <summary> DOWN ARROW key. </summary>
	KB_Down = 40,
	/// <summary> SELECT key. </summary>
	KB_Select = 41,
	/// <summary> PRINT key. </summary>
	KB_Print = 42,
	/// <summary> EXECUTE key. </summary>
	KB_Execute = 43,
	/// <summary> PRINT SCREEN key. </summary>
	KB_PrintScreen = 44,
	/// <summary> INS key. </summary>
	KB_Insert = 45,
	/// <summary> DEL key. </summary>
	KB_Delete = 46,
	/// <summary> HELP key. </summary>
	KB_Help = 47,
	/// <summary> Used for miscellaneous characters; it can vary by keyboard but is usually used for the number keys. </summary>
	KB_D0 = 48,
	/// <summary> Used for miscellaneous characters; it can vary by keyboard but is usually used for the number keys. </summary>
	KB_D1 = 49,
	/// <summary> Used for miscellaneous characters; it can vary by keyboard but is usually used for the number keys. </summary>
	KB_D2 = 50,
	/// <summary> Used for miscellaneous characters; it can vary by keyboard but is usually used for the number keys. </summary>
	KB_D3 = 51,
	/// <summary> Used for miscellaneous characters; it can vary by keyboard but is usually used for the number keys. </summary>
	KB_D4 = 52,
	/// <summary> Used for miscellaneous characters; it can vary by keyboard but is usually used for the number keys. </summary>
	KB_D5 = 53,
	/// <summary> Used for miscellaneous characters; it can vary by keyboard but is usually used for the number keys. </summary>
	KB_D6 = 54,
	/// <summary> Used for miscellaneous characters; it can vary by keyboard but is usually used for the number keys. </summary>
	KB_D7 = 55,
	/// <summary> Used for miscellaneous characters; it can vary by keyboard but is usually used for the number keys. </summary>
	KB_D8 = 56,
	/// <summary> Used for miscellaneous characters; it can vary by keyboard but is usually used for the number keys. </summary>
	KB_D9 = 57,
	/// <summary> A key. </summary>
	KB_A = 65,
	/// <summary> B key. </summary>
	KB_B = 66,
	/// <summary> C key. </summary>
	KB_C = 67,
	/// <summary> D key. </summary>
	KB_D = 68,
	/// <summary> E key. </summary>
	KB_E = 69,
	/// <summary> F key. </summary>
	KB_F = 70,
	/// <summary> G key. </summary>
	KB_G = 71,
	/// <summary> H key. </summary>
	KB_H = 72,
	/// <summary> I key. </summary>
	KB_I = 73,
	/// <summary> J key. </summary>
	KB_J = 74,
	/// <summary> K key. </summary>
	KB_K = 75,
	/// <summary> L key. </summary>
	KB_L = 76,
	/// <summary> M key. </summary>
	KB_M = 77,
	/// <summary> N key. </summary>
	KB_N = 78,
	/// <summary> O key. </summary>
	KB_O = 79,
	/// <summary> P key. </summary>
	KB_P = 80,
	/// <summary> Q key. </summary>
	KB_Q = 81,
	/// <summary> R key. </summary>
	KB_R = 82,
	/// <summary> S key. </summary>
	KB_S = 83,
	/// <summary> T key. </summary>
	KB_T = 84,
	/// <summary> U key. </summary>
	KB_U = 85,
	/// <summary> V key. </summary>
	KB_V = 86,
	/// <summary> W key. </summary>
	KB_W = 87,
	/// <summary> X key. </summary>
	KB_X = 88,
	/// <summary> Y key. </summary>
	KB_Y = 89,
	/// <summary> Z key. </summary>
	KB_Z = 90,
	/// <summary> Left Windows key. </summary>
	KB_LeftWindows = 91,
	/// <summary> Right Windows key. </summary>
	KB_RightWindows = 92,
	/// <summary> Applications key. </summary>
	KB_Apps = 93,
	/// <summary> Computer Sleep key. </summary>
	KB_Sleep = 95,
	/// <summary> Numeric keypad 0 key. </summary>
	KB_NumPad0 = 96,
	/// <summary> Numeric keypad 1 key. </summary>
	KB_NumPad1 = 97,
	/// <summary> Numeric keypad 2 key. </summary>
	KB_NumPad2 = 98,
	/// <summary> Numeric keypad 3 key. </summary>
	KB_NumPad3 = 99,
	/// <summary> Numeric keypad 4 key. </summary>
	KB_NumPad4 = 100,
	/// <summary> Numeric keypad 5 key. </summary>
	KB_NumPad5 = 101,
	/// <summary> Numeric keypad 6 key. </summary>
	KB_NumPad6 = 102,
	/// <summary> Numeric keypad 7 key. </summary>
	KB_NumPad7 = 103,
	/// <summary> Numeric keypad 8 key. </summary>
	KB_NumPad8 = 104,
	/// <summary> Numeric keypad 9 key. </summary>
	KB_NumPad9 = 105,
	/// <summary> Multiply key. </summary>
	KB_Multiply = 106,
	/// <summary> Add key. </summary>
	KB_Add = 107,
	/// <summary> Separator key. </summary>
	KB_Separator = 108,
	/// <summary> Subtract key. </summary>
	KB_Subtract = 109,
	/// <summary> Decimal key. </summary>
	KB_Decimal = 110,
	/// <summary> Divide key. </summary>
	KB_Divide = 111,
	/// <summary> F1 key. </summary>
	KB_F1 = 112,
	/// <summary> F2 key. </summary>
	KB_F2 = 113,
	/// <summary> F3 key. </summary>
	KB_F3 = 114,
	/// <summary> F4 key. </summary>
	KB_F4 = 115,
	/// <summary> F5 key. </summary>
	KB_F5 = 116,
	/// <summary> F6 key. </summary>
	KB_F6 = 117,
	/// <summary> F7 key. </summary>
	KB_F7 = 118,
	/// <summary> F8 key. </summary>
	KB_F8 = 119,
	/// <summary> F9 key. </summary>
	KB_F9 = 120,
	/// <summary> F10 key. </summary>
	KB_F10 = 121,
	/// <summary> F11 key. </summary>
	KB_F11 = 122,
	/// <summary> F12 key. </summary>
	KB_F12 = 123,
	/// <summary> F13 key. </summary>
	KB_F13 = 124,
	/// <summary> F14 key. </summary>
	KB_F14 = 125,
	/// <summary> F15 key. </summary>
	KB_F15 = 126,
	/// <summary> F16 key. </summary>
	KB_F16 = 127,
	/// <summary> F17 key. </summary>
	KB_F17 = 128,
	/// <summary> F18 key. </summary>
	KB_F18 = 129,
	/// <summary> F19 key. </summary>
	KB_F19 = 130,
	/// <summary> F20 key. </summary>
	KB_F20 = 131,
	/// <summary> F21 key. </summary>
	KB_F21 = 132,
	/// <summary> F22 key. </summary>
	KB_F22 = 133,
	/// <summary> F23 key. </summary>
	KB_F23 = 134,
	/// <summary> F24 key. </summary>
	KB_F24 = 135,
	/// <summary> NUM LOCK key. </summary>
	KB_NumLock = 144,
	/// <summary> SCROLL LOCK key. </summary>
	KB_ScrollLock = 145,
	/// <summary> Left SHIFT key. </summary>
	KB_LeftShift = 160,
	/// <summary> Right SHIFT key. </summary>
	KB_RightShift = 161,
	/// <summary> Left CONTROL key. </summary>
	KB_LeftControl = 162,
	/// <summary> Right CONTROL key. </summary>
	KB_RightControl = 163,
	/// <summary> Left ALT key. </summary>
	KB_LeftAlt = 164,
	/// <summary> Right ALT key. </summary>
	KB_RightAlt = 165,
	/// <summary> Browser Back key. </summary>
	KB_BrowserBack = 166,
	/// <summary> Browser Forward key. </summary>
	KB_BrowserForward = 167,
	/// <summary> Browser Refresh key. </summary>
	KB_BrowserRefresh = 168,
	/// <summary> Browser Stop key. </summary>
	KB_BrowserStop = 169,
	/// <summary> Browser Search key. </summary>
	KB_BrowserSearch = 170,
	/// <summary> Browser Favorites key. </summary>
	KB_BrowserFavorites = 171,
	/// <summary> Browser Start and Home key. </summary>
	KB_BrowserHome = 172,
	/// <summary> Volume Mute key. </summary>
	KB_VolumeMute = 173,
	/// <summary> Volume Down key. </summary>
	KB_VolumeDown = 174,
	/// <summary> Volume Up key. </summary>
	KB_VolumeUp = 175,
	/// <summary> Next Track key. </summary>
	KB_MediaNextTrack = 176,
	/// <summary> Previous Track key. </summary>
	KB_MediaPreviousTrack = 177,
	/// <summary> Stop Media key. </summary>
	KB_MediaStop = 178,
	/// <summary> Play/Pause Media key. </summary>
	KB_MediaPlayPause = 179,
	/// <summary> Start Mail key. </summary>
	KB_LaunchMail = 180,
	/// <summary> Select Media key. </summary>
	KB_SelectMedia = 181,
	/// <summary> Start Application 1 key. </summary>
	KB_LaunchApplication1 = 182,
	/// <summary> Start Application 2 key. </summary>
	KB_LaunchApplication2 = 183,
	/// <summary> The OEM Semicolon key on a US standard keyboard. </summary>
	KB_Semicolon = 186,
	/// <summary> For any country/region, the '+' key. </summary>
	KB_Plus = 187,
	/// <summary> For any country/region, the ',' key. </summary>
	KB_Comma = 188,
	/// <summary> For any country/region, the '-' key. </summary>
	KB_Minus = 189,
	/// <summary> For any country/region, the '.' key. </summary>
	KB_Period = 190,
	/// <summary> The OEM question mark key on a US standard keyboard. </summary>
	KB_Question = 191,
	/// <summary> The OEM tilde key on a US standard keyboard. </summary>
	KB_Tilde = 192,
	/// <summary> The OEM open bracket key on a US standard keyboard. </summary>
	KB_OpenBrackets = 219,
	/// <summary> The OEM pipe key on a US standard keyboard. </summary>
	KB_Pipe = 220,
	/// <summary> The OEM close bracket key on a US standard keyboard. </summary>
	KB_CloseBrackets = 221,
	/// <summary> The OEM singled/double quote key on a US standard keyboard. </summary>
	KB_Quotes = 222,
	/// <summary> Used for miscellaneous characters; it can vary by keyboard. </summary>
	KB_OemMisc = 223,
	/// <summary> The OEM angle bracket or backslash key on the RT 102 key keyboard. </summary>
	KB_Backslash = 226,
	/// <summary> IME PROCESS key. </summary>
	KB_ProcessKey = 229,
	/// <summary> Attn key. </summary>
	KB_Attn = 246,
	/// <summary> CrSel key. </summary>
	KB_Crsel = 247,
	/// <summary> ExSel key. </summary>
	KB_Exsel = 248,
	/// <summary> Erase EOF key. </summary>
	KB_EraseEof = 249,
	/// <summary> Play key. </summary>
	KB_Play = 250,
	/// <summary> Zoom key. </summary>
	KB_Zoom = 251,
	/// <summary> PA1 key. </summary>
	KB_Pa1 = 253,
	/// <summary> CLEAR key. </summary>
	KB_OemClear = 254,
	/// <summary> Green ChatPad key. </summary>
	KB_ChatPadGreen = 0xCA,
	/// <summary> Orange ChatPad key. </summary>
	KB_ChatPadOrange = 0xCB,
	/// <summary> PAUSE key. </summary>
	KB_Pause = 0x13,
	/// <summary> IME Convert key. </summary>
	KB_ImeConvert = 0x1c,
	/// <summary> IME NoConvert key. </summary>
	KB_ImeNoConvert = 0x1d,
	/// <summary> Kana key on Japanese keyboards. </summary>
	KB_Kana = 0x15,
	/// <summary> Kanji key on Japanese keyboards. </summary>
	KB_Kanji = 0x19,
	/// <summary> OEM Auto key. </summary>
	KB_OemAuto = 0xf3,
	/// <summary> OEM Copy key. </summary>
	KB_OemCopy = 0xf2,
	/// <summary> OEM Enlarge Window key. </summary>
	KB_OemEnlW = 0xf4,

	KB_Any = 250,

	#endregion Keyboard

	// Offset: 251, Any: 260
	#region Mouse

	Mouse_Left = 251,
	Mouse_Right = 252,
	Mouse_Middle = 253,
	Mouse_Button4 = 254,
	Mouse_Button5 = 255,
	Mouse_Button6 = 256,
	Mouse_Button7 = 257,
	Mouse_Button8 = 258,

	Mouse_Any = 260,

	#endregion Mouse

	// Offset: 261, Any: 300
	#region Gamepad

	GP_Home = 261,
	GP_Start = 262,
	GP_Select = 263,

	GP_North = 264,
	GP_South = 265,
	GP_East = 266,
	GP_West = 267,

	GP_DPad_Up = 268,
	GP_DPad_Down = 269,
	GP_DPad_Left = 270,
	GP_DPad_Right = 271,

	GP_LB = 272,
	GP_RB = 273,
	GP_LT = 274,
	GP_RT = 275,

	GP_Any = 300,

	#endregion Gamepad
}

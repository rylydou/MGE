namespace MGE;

public enum Button
{
	/// <summary> No Key pressed </summary>
	None = 0,

	// Offset: 0 ~ 32, Any: 250

	#region Keyboard

	#region ASCII Keys

	KB_Space = 32,
	KB_Apostrophe = 39,
	KB_Comma = 44,
	KB_Minus = 45,
	KB_Period = 46,
	/// <summary>"/" key (the one on the questionmark key)</summary>
	KB_Slash = 47,
	KB_0 = 48,
	KB_1 = 49,
	KB_2 = 50,
	KB_3 = 51,
	KB_4 = 52,
	KB_5 = 53,
	KB_6 = 54,
	KB_7 = 55,
	KB_8 = 56,
	KB_9 = 57,
	KB_Semicolon = 59,
	KB_Equal = 61,
	KB_A = 65,
	KB_B = 66,
	KB_C = 67,
	KB_D = 68,
	KB_E = 69,
	KB_F = 70,
	KB_G = 71,
	KB_H = 72,
	KB_I = 73,
	KB_J = 74,
	KB_K = 75,
	KB_L = 76,
	KB_M = 77,
	KB_N = 78,
	KB_O = 79,
	KB_P = 80,
	KB_Q = 81,
	KB_R = 82,
	KB_S = 83,
	KB_T = 84,
	KB_U = 85,
	KB_V = 86,
	KB_W = 87,
	KB_X = 88,
	KB_Y = 89,
	KB_Z = 90,
	KB_LeftBracket = 91,
	/// <summary>"\" key (the one to the right of the "]" key)</summary>
	KB_Backslash = 92,
	KB_RightBracket = 93,
	/// <summary>"`" key (the one on the "~" key)</summary>
	KB_GraveAccent = 96,

	#endregion ASCII Keys

	KB_Escape = 256,
	KB_Enter = 257,
	KB_Tab = 258,
	KB_Backspace = 259,
	KB_Insert = 260,
	KB_Delete = 261,
	KB_Right = 262,
	KB_Left = 263,
	KB_Down = 264,
	KB_Up = 265,
	KB_PageUp = 266,
	KB_PageDown = 267,
	KB_Home = 268,
	KB_End = 269,
	KB_CapsLock = 280,
	KB_ScrollLock = 281,
	KB_NumLock = 282,
	KB_PrintScreen = 283,
	KB_Pause = 284,
	KB_F1 = 290,
	KB_F2 = 291,
	KB_F3 = 292,
	KB_F4 = 293,
	KB_F5 = 294,
	KB_F6 = 295,
	KB_F7 = 296,
	KB_F8 = 297,
	KB_F9 = 298,
	KB_F10 = 299,
	KB_F11 = 300,
	KB_F12 = 301,
	KB_F13 = 302,
	KB_F14 = 303,
	KB_F15 = 304,
	KB_F16 = 305,
	KB_F17 = 306,
	KB_F18 = 307,
	KB_F19 = 308,
	KB_F20 = 309,
	KB_F21 = 310,
	KB_F22 = 311,
	KB_F23 = 312,
	KB_F24 = 313,
	KB_F25 = 314,
	KB_KeyPad0 = 320,
	KB_KeyPad1 = 321,
	KB_KeyPad2 = 322,
	KB_KeyPad3 = 323,
	KB_KeyPad4 = 324,
	KB_KeyPad5 = 325,
	KB_KeyPad6 = 326,
	KB_KeyPad7 = 327,
	KB_KeyPad8 = 328,
	KB_KeyPad9 = 329,
	KB_KeyPadDecimal = 330,
	KB_KeyPadDivide = 331,
	KB_KeyPadMultiply = 332,
	KB_KeyPadSubtract = 333,
	KB_KeyPadAdd = 334,
	KB_KeyPadEnter = 335,
	KB_KeyPadEqual = 336,
	KB_LeftShift = 340,
	KB_LeftControl = 341,
	KB_LeftAlt = 342,
	/// <summary>The left super key, aka the Windows key</summary>
	KB_LeftSuper = 343,
	KB_RightShift = 344,
	KB_RightControl = 345,
	KB_RightAlt = 346,
	/// <summary>The right super key, aka the Windows key</summary>
	KB_RightSuper = 347,
	KB_Menu = 348,

	KB_Any = 350,

	#endregion Keyboard

	// Offset: 351, Any: 360
	#region Mouse

	Mouse_Left = 351,
	Mouse_Right = 352,
	Mouse_Middle = 353,
	Mouse_Button4 = 354,
	Mouse_Button5 = 355,
	Mouse_Button6 = 356,
	Mouse_Button7 = 357,
	Mouse_Button8 = 358,

	Mouse_Any = 360,

	#endregion Mouse

	// Offset: 361, Any: 400
	#region Gamepad

	GP_Home = 361,
	GP_Start = 362,
	GP_Select = 363,

	GP_North = 364,
	GP_South = 365,
	GP_East = 366,
	GP_West = 367,

	GP_DPad_Up = 368,
	GP_DPad_Down = 369,
	GP_DPad_Left = 370,
	GP_DPad_Right = 371,

	GP_LB = 372,
	GP_RB = 373,
	GP_LT = 374,
	GP_RT = 375,

	GP_LSB = 376,
	GP_RSB = 377,

	GP_Any = 400,

	#endregion Gamepad
}

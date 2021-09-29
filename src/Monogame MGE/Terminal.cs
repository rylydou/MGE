using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MGE
{
	public static class Terminal
	{
		public static bool visible;

		internal static Dictionary<(string, Type[]), MethodInfo> commandMethods = new Dictionary<(string, Type[]), MethodInfo>();

		static List<(string, Color)> _history = new List<(string, Color)>();

		static string _lastCommand = string.Empty;
		static StringBuilder _currentCommand = new StringBuilder();

		static int _scroll;

		public static void Write(string text, Color? color = null)
		{
			if (!color.HasValue)
				color = Color.white;
			_history.Insert(0, (text, color.Value));
		}

		public static void Clear()
		{
			_history.Clear();
		}

		internal static void Update()
		{
			if (Input.IsButtonPressed(Buttons.KB_Tilde))
			{
				Terminal.visible = !Terminal.visible;
				return;
			}
			else if (Input.IsButtonPressed(Buttons.KB_Arrow_Up))
			{
				var tmp = _currentCommand.ToString();
				_currentCommand.Clear();
				_currentCommand.Append(_lastCommand);
				_lastCommand = tmp;
			}

			if (!visible) return;

			_scroll -= Input.scroll * 3;

			foreach (var letter in Input.keyboardString)
			{
				if (letter < 32)
				{
					switch (letter)
					{
						case '\b':
							if (_currentCommand.Length > 0)
								_currentCommand.Remove(_currentCommand.Length - 1, 1);
							break;
						case '\t':
						case '\n':
						case (char)13:
							if (_currentCommand.Length < 1) return;
							var command = _currentCommand.Append(' ').ToString();
							Write("- " + command, new Color("#FB4"));
							try
							{
								RunCommand(command);
							}
							catch (System.Exception e)
							{
								Write("Unknown Error - " + e.Message, Color.red);
							}
							_lastCommand = _currentCommand.Remove(_currentCommand.Length - 1, 1).ToString();
							_currentCommand.Clear();
							_scroll = 0;
							break;
						case (char)27: _currentCommand.Clear(); break;
						default: Logger.LogVar("Unknown Char", (int)letter); break;
					}
				}
				else
				{
					_currentCommand.Append(letter);
				}
			}
		}

		internal static void Draw()
		{
			if (!visible) return;

			var line = 1;
			foreach (var item in _history)
				Assets.font.Draw(item.Item1, LinePos(ref line), item.Item2);

			GFX.DrawBox(new Rect(0, Window.size.y - 4 * 2 - Assets.font.fullCharSize.y, Window.size.x, Assets.font.fullCharSize.y + 4 * 2), new Color(0, 0.9f));
			Assets.font.Draw("- " + _currentCommand.ToString() + (char)219, new Vector2(8, Window.size.y - 2 - Assets.font.fullCharSize.y), new Color("#FB4"));
		}

		public static void RegisterCommand<T>()
		{
			foreach (var method in typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Static))
				commandMethods.Add((method.Name, method.GetParameters().Select(p => p.ParameterType).ToArray()), method);
		}

		public static void RunCommand(string command)
		{
			var commandName = string.Empty;
			var commandParamValues = new List<object>();
			var currentCommandSection = new StringBuilder();

			var index = 0;
			var inString = false;
			var paramType = typeof(int);
			foreach (var letter in command)
			{
				try
				{
					switch (letter)
					{
						case ' ':
							if (inString) goto default;
							else
							{
								if (string.IsNullOrEmpty(commandName))
								{
									commandName = currentCommandSection.ToString();
								}
								else
								{
									if (paramType == typeof(int))
									{
										if (int.TryParse(currentCommandSection.ToString(), out var i))
											commandParamValues.Add(i);
										else
											commandParamValues.Add(currentCommandSection.ToString());
									}
									else if (paramType == typeof(float))
									{
										commandParamValues.Add(float.Parse(currentCommandSection.ToString()));
									}
									else if (paramType == typeof(string))
									{
										commandParamValues.Add(currentCommandSection.ToString());
									}
									paramType = typeof(int);
								}
								currentCommandSection.Clear();
							}
							break;
						case '"':
							if (inString.Toggle())
								paramType = typeof(string);
							break;
						case '.':
							if (!inString)
								paramType = typeof(float);
							goto default;
						default:
							currentCommandSection.Append(letter);
							break;
					}
				}
				catch (System.Exception e)
				{
					Write($"Syntax Error @{index} - {e.Message}", Color.red);
					return;
				}
				index++;
			}

			if (string.IsNullOrEmpty(commandName))
				commandName = currentCommandSection.ToString();

			var paramTypes = commandParamValues.Select(o => o.GetType()).ToArray();
			var foundCommand = false;
			foreach (var item in commandMethods)
			{
				if (item.Key.Item1 == commandName)
				{
					if (item.Key.Item2.Length == paramTypes.Length)
					{
						var matchedCount = 0;
						var paramEnum = paramTypes.GetEnumerator();
						foreach (var item2 in item.Key.Item2)
						{
							paramEnum.MoveNext();
							if (item2.Equals(typeof(object)) || (item2 == (Type)paramEnum.Current) || (paramEnum.Current.Equals(typeof(float)) && item2.Equals(typeof(int))))
								matchedCount++;
						}
						if (matchedCount == item.Key.Item2.Length)
						{
							foundCommand = true;
							// TODO Make the try catch actually work
							try
							{
								item.Value.Invoke(null, commandParamValues.ToArray());
							}
							catch (System.Exception e)
							{
								Write($"Error running command - {e.Message}", Color.red);
								return;
							}
						}
					}
				}
			}

			if (!foundCommand)
			{
				var sb = new StringBuilder("Command \"");
				sb.Append(commandName);
				sb.Append(" [");
				var i = 0;
				foreach (var value in commandParamValues)
				{
					sb.Append(value.GetType().Name.Replace("Int32", "int").Replace("Single", "num").Replace("String", "str"));
					if (i < commandParamValues.Count - 1) sb.Append(' ');
					i++;
				}
				sb.Append("]\" doesn't exsist");

				Write(sb.ToString(), Color.red);
			}
		}

		static Vector2 LinePos(ref int line)
		{
			line++;
			return new Vector2(8, Window.size.y - 8 - line * Assets.font.fullCharSize.y + _scroll * Assets.font.fullCharSize.y);
		}
	}
}

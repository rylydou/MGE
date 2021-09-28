using System.Globalization;
using System.IO;
using System.Text;

namespace MGE.Editor.Util.Expression
{
	class Tokenizer
	{
		public Tokenizer(TextReader reader)
		{
			this.reader = reader;
			this.identifier = string.Empty;
			NextChar();
			NextToken();
		}

		public TextReader reader;
		public char currentChar;
		public EToken token;
		public double number;
		public string identifier;

		// Read the next character from the input strem
		// and store it in _currentChar, or load '\0' if EOF
		void NextChar()
		{
			var ch = reader.Read();
			currentChar = ch < 0 ? '\0' : (char)ch;
		}

		// Read the next token from the input stream
		public void NextToken()
		{
			// Skip whitespace
			while (char.IsWhiteSpace(currentChar)) NextChar();

			// Special characters
			switch (currentChar)
			{
				case '\0':
					token = EToken.EOF;
					return;

				case '+':
					NextChar();
					token = EToken.Add;
					return;

				case '-':
					NextChar();
					token = EToken.Subtract;
					return;

				case '*':
					NextChar();
					token = EToken.Multiply;
					return;

				case '/':
					NextChar();
					token = EToken.Divide;
					return;

				case '(':
					NextChar();
					token = EToken.OpenParens;
					return;

				case ')':
					NextChar();
					token = EToken.CloseParens;
					return;

				case ',':
					NextChar();
					token = EToken.Comma;
					return;
			}

			// Number?
			if (char.IsDigit(currentChar) || currentChar == '.')
			{
				// Capture digits/decimal point
				var sb = new StringBuilder();
				var haveDecimalPoint = false;
				while (char.IsDigit(currentChar) || (!haveDecimalPoint && currentChar == '.'))
				{
					sb.Append(currentChar);
					haveDecimalPoint = currentChar == '.';
					NextChar();
				}

				// Parse it
				number = double.Parse(sb.ToString(), CultureInfo.InvariantCulture);
				token = EToken.Number;
				return;
			}

			// Identifier - starts with letter or underscore
			if (char.IsLetter(currentChar) || currentChar == '_')
			{
				var sb = new StringBuilder();

				// Accept letter, digit or underscore
				while (char.IsLetterOrDigit(currentChar) || currentChar == '_')
				{
					sb.Append(currentChar);
					NextChar();
				}

				// Setup token
				identifier = sb.ToString();
				token = EToken.Identifier;
				return;
			}
		}
	}
}

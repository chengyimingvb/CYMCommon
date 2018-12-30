#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Text;

namespace CodeStage.Maintainer
{
	[Serializable]
	public abstract class RecordBase
	{
		public RecordLocation location;

		public bool compactMode = true;
		public bool selected = true;

		public string headerExtra;
		public string bodyExtra;
		public int headerFormatArgument;

		protected StringBuilder cachedHeader;
		protected StringBuilder cachedCompactLine;
		protected StringBuilder cachedBody;

		public string GetCompactLine()
		{
			if (cachedCompactLine != null) return cachedCompactLine.ToString();

			cachedCompactLine = new StringBuilder();
			ConstructCompactLine(cachedCompactLine);
			return cachedCompactLine.ToString();
		}

		public string GetHeader()
		{
			if (cachedHeader != null) return cachedHeader.ToString();

			cachedHeader = new StringBuilder();
			cachedHeader.Append("<b><size=14>");

			ConstructHeader(cachedHeader);

			if (!string.IsNullOrEmpty(headerExtra))
			{
				cachedHeader.Append(' ').Append(headerExtra);
			}

			cachedHeader.Append("</size></b>");

			return cachedHeader.ToString();
		}

		public string GetBody()
		{
			if (cachedBody != null) return cachedBody.ToString();

			cachedBody = new StringBuilder();

			ConstructBody(cachedBody);

			if (!string.IsNullOrEmpty(bodyExtra))
			{
				cachedBody.Append("\n").Append(bodyExtra);
			}

			return cachedBody.ToString();
		}

		public override string ToString()
		{
			return GetHeader() + "\n" + GetBody();
		}

		public string ToString(bool clearHtml)
		{
			return StripTagsCharArray(ToString());
		}

		protected abstract void ConstructCompactLine(StringBuilder text);
		protected abstract void ConstructHeader(StringBuilder text);
        protected abstract void ConstructBody(StringBuilder text);

		// source: http://www.dotnetperls.com/remove-html-tags
		private static string StripTagsCharArray(string input)
		{
			var arrayIndex = 0;
			var inside = false;
			var len = input.Length;

			var array = new char[len];

			for (var i = 0; i < len; i++)
			{
				var let = input[i];

				if (let == '<')
				{
					inside = true;
					continue;
				}
				if (let == '>')
				{
					inside = false;
					continue;
				}

				if (inside) continue;

				array[arrayIndex] = @let;
				arrayIndex++;
			}
			return new string(array, 0, arrayIndex);
		}
	}
}
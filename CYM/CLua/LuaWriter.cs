using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CYM.Lua
{

	public class LuaWriter : StringWriter
	{
		private Type _requiredAttributeType;

		#region Tabs Management

		private int _tabs;
		private bool _waitingForTabs = true;

		/// <summary>
		/// Adds a tab at the beginning of all following lines.
		/// </summary>
		public void AddTab()
		{
			_tabs++;
		}

		/// <summary>
		/// Removes a tab from the beginning of all following lines.
		/// </summary>
		public void RemoveTab()
		{
			_tabs--;
			if (_tabs < 0) _tabs = 0;
		}

		/// <summary>
		/// Writes to the current Lua string stream, taking tabs into account.
		/// </summary>
		/// <param name="str"></param>
		public override void Write(string str)
		{
			if (_waitingForTabs)
			{
				WriteTabs();
				_waitingForTabs = false;
			}
			base.Write(str);
		}

		/// <summary>
		/// Writes to the current Lua string stream, taking tabs into account, and adding a new line.
		/// </summary>
		public override void WriteLine()
		{
			if (_waitingForTabs)
			{
				WriteTabs();
				_waitingForTabs = false;
			}
			base.WriteLine();
			_waitingForTabs = true;
		}

		private void WriteTabs()
		{
			for (int i = 0; i < _tabs; i++) base.Write("\t");
		}

		#endregion

		/// <summary>
		///		Sets a required attribute to restrict which properties are processed when calling WriteObject.
		/// </summary>
		/// <param name="attributeType">Type of the attribute, or null to clear the requirement.</param>
		public void SetRequiredAttribute(Type attributeType)
		{
			_requiredAttributeType = attributeType;
		}

		/// <summary>
		///     Writes a Lua representation of a CLR public property.
		/// </summary>
		/// <typeparam name="T">Type of object</typeparam>
		/// <param name="obj">Object instance containing the property.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="multiline">Optional parameter to specifiy if the resulting Lua data should spawn across multiple lines.</param>
		/// <param name="trailingComma">
		///     Optional parameter to specify if the resulting Lua data should have a comma at the end (if
		///     it's inside a table, or a list of arguments).
		/// </param>
		public void WriteProperty<T>(T obj, string propertyName, bool multiline = true, bool trailingComma = false)
		{
			PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
			if (propertyInfo == null) Debug.Log("ERROR " + propertyName);
			WriteProperty(obj, propertyInfo, multiline, trailingComma);
		}

		/// <summary>
		///     Writes a Lua representation of a CLR public property.
		/// </summary>
		/// <typeparam name="T">Type of object</typeparam>
		/// <param name="obj">Object instance containing the property.</param>
		/// <param name="propertyInfo">PropertyInfo object.</param>
		/// <param name="multiline">Optional parameter to specifiy if the resulting Lua data should spawn across multiple lines.</param>
		/// <param name="trailingComma">
		///     Optional parameter to specify if the resulting Lua data should have a comma at the end (for example if
		///     it's inside a table, or a list of arguments).
		/// </param>
		private void WriteProperty<T>(T obj, PropertyInfo propertyInfo, bool multiline = true, bool trailingComma = false)
		{
			//Debug.Log("WriteProperty: " + propertyInfo.Name);
			object propertyValue = propertyInfo.GetValue(obj, null);
			Type propertyType = propertyInfo.PropertyType;
			if (propertyValue != null)
			{
				Write(propertyInfo.Name + " = ");
				WriteObject(propertyValue, propertyType, multiline, trailingComma);
			}
		}

		//public void WriteObject(object obj, bool multiLine = true, bool trailingComma = false)
		//{
			//WriteObject(obj, obj.GetType(), multiLine, trailingComma);
		//}

		/// <summary>
		/// Writes a Lua representation of a CLR type.
		/// </summary>
		/// <typeparam name="T">Type of the object.</typeparam>
		/// <param name="obj">CLR object instance.</param>
		/// <param name="multiline">Optional parameter to specifiy if the resulting Lua data should spawn across multiple lines.</param>
		/// <param name="trailingComma"></param>
		public void WriteObject<T>(T obj, bool multiline = true, bool trailingComma = false)
		{
			WriteObject(obj, typeof(T), multiline, trailingComma);
		}

		private void WriteObject(object obj, Type type, bool multiline = true, bool trailingComma = false)
		{
			// Write built-in types

			string[] array = {" sdf", "sdf"};

			if (type == typeof (bool)) Write(((bool) obj).ToString().ToLower());
			else if (type == typeof(int)) Write(((int)obj).ToString(CultureInfo.InvariantCulture).ToLower());
			else if (type == typeof(float)) Write(((float)obj).ToString(CultureInfo.InvariantCulture).ToLower());
			else if (type == typeof(double)) Write(((double)obj).ToString(CultureInfo.InvariantCulture).ToLower());
			else if (type == typeof(string)) Write(((string)obj).ToLiteral());
			else if (type == typeof(byte)) Write(((byte)obj).ToString(CultureInfo.InvariantCulture).ToLower());
			else if (type == typeof(decimal)) Write(((decimal)obj).ToString(CultureInfo.InvariantCulture).ToLower());

			// Write enums

			else if (type.IsEnum) Write(obj.ToString().ToLiteral());

			// Write Unity types

			else if (type == typeof(Color)) WriteColor((Color) obj);
			else if (type == typeof(Color32)) WriteColor32((Color32)obj);
			else if (type == typeof(Rect)) WriteRect((Rect)obj);
			else if (type == typeof(Vector2)) WriteVector2((Vector2)obj);
			else if (type == typeof(Vector3)) WriteVector3((Vector3)obj);
			else if (type == typeof(Vector4)) WriteVector4((Vector4)obj);
			
			// Write arrays

			else if (type.IsArray) WriteArray((Array) obj, multiline);

			// Write generic lists
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) WriteList((IList)obj, multiline);
			
			// Write generic dictionaries
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) WriteDictionary((IDictionary)obj, multiline);
			
			// Write classes
			else if (type.IsClass) WriteClass(obj, multiline);

			if (trailingComma)
			{
				Write(", ");
				if (multiline) WriteLine();
			}
		}

		private void WriteColor(Color color)
		{
			WriteArray(new[] { color.r, color.g, color.b, color.a }, false);
		}

		private void WriteColor32(Color32 color32)
		{
			WriteArray(new[] { color32.r, color32.g, color32.b, color32.a }, false);
		}

		private void WriteRect(Rect rect)
		{
			WriteArray(new[] { rect.x, rect.y, rect.width, rect.height }, false);
		}

		private void WriteVector2(Vector2 vector2)
		{
			WriteArray(new[] { vector2.x, vector2.y }, false);
		}

		private void WriteVector3(Vector3 vector3)
		{
			WriteArray(new[] { vector3.x, vector3.y, vector3.z }, false);
		}

		private void WriteVector4(Vector4 vector4)
		{
			WriteArray(new[] { vector4.x, vector4.y, vector4.z, vector4.w }, false);
		}

		private void WriteDictionary(IDictionary dictionary, bool multiline = true)
		{
			Write("{");
			if (multiline) WriteLine();
			AddTab();
			foreach (object key in dictionary.Keys)
			{
				var s = key as string;
				if (s != null)
				{
					if (IsValidTableString(s)) Write(s + " = ");
					else Write("[\"" + s + "\"] = ");
				}
				else if (key is int) Write("[" + (int) key + "] = ");
				WriteObject(dictionary[key], dictionary.GetType().GetGenericArguments()[1], multiline, true);
			}
			RemoveTab();
			Write("}");
		}

		private bool IsValidTableString(string key)
		{
			return Regex.IsMatch(key, @"^\D\w*$");
		}

		private void WriteArray(Array array, bool multiline = true)
		{
			Write("{");
			if (multiline) WriteLine();
			AddTab();
			if (array.Rank == 1)
			{
				var arrayLength = array.GetLength(0);
				for (int i = 0; i < arrayLength; i++)
				{
					WriteObject(array.GetValue(i), array.GetType().GetElementType(), multiline, i < arrayLength - 1);
				}
			}
			else if (array.Rank == 2)
			{
				var arrayLength0 = array.GetLength(0);
				var arrayLength1 = array.GetLength(1);
				for (int i = 0; i < arrayLength0; i++)
				{
					WriteLine("{");
					AddTab();
					for (int j = 0; j < arrayLength1; j++) WriteObject(array.GetValue(i, j), array.GetType().GetElementType(), multiline, j < arrayLength1 - 1);
					RemoveTab();
					Write("}");
					if (i < arrayLength0 - 1) WriteLine(","); else WriteLine();
				}
			}
			RemoveTab();
			Write("}");
		}

		private void WriteList(IList list, bool multiline = true)
		{
			Write("{");
			if (multiline) WriteLine();
			AddTab();
			foreach (object element in list) WriteObject(element, list.GetType().GetGenericArguments()[0], multiline, true);
			RemoveTab();
			Write("}");
		}

		private void WriteClass<T>(T obj, bool multiline = true, bool trailingComma = false)
		{
			Write("{");
			if (multiline) WriteLine();
			AddTab();
			var customSerializer = obj as ICustomLuaSerializer;
			if (customSerializer != null)
			{
				customSerializer.Serialize(this);
			}
			else
			{
				foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties())
				{
					if (_requiredAttributeType != null)
					{
						object[] attributes = propertyInfo.GetCustomAttributes(_requiredAttributeType, false);
						if (attributes.Length <= 0) continue;
					}
					WriteProperty(obj, propertyInfo, multiline, true);
				}
			}
			RemoveTab();
			Write("}");
			if (trailingComma)
			{
				Write(", ");
				if (multiline) WriteLine();
			}
		}


	}

	public interface ICustomLuaSerializer
	{
		void Serialize(LuaWriter luaWriter);
	}

	public class LuaSerializableAttribute : System.Attribute
	{
		
	}

	public static class LuaWriterExtensionMethods
	{
		/// <summary>
		/// Converts a string to a literal representation, including opening and closing quotes.
		/// </summary>
		/// <param name="input">The string to convert.</param>
		/// <returns>Converted string.</returns>
		public static string ToLiteral(this string input)
		{
			var literal = new StringBuilder(input.Length + 2);
			literal.Append("\"");
			foreach (char c in input)
			{
				switch (c)
				{
					case '\'':
						literal.Append(@"\'");
						break;
					case '\"':
						literal.Append("\\\"");
						break;
					case '\\':
						literal.Append(@"\\");
						break;
					case '\0':
						literal.Append(@"\0");
						break;
					case '\a':
						literal.Append(@"\a");
						break;
					case '\b':
						literal.Append(@"\b");
						break;
					case '\f':
						literal.Append(@"\f");
						break;
					case '\n':
						literal.Append(@"\n");
						break;
					case '\r':
						literal.Append(@"\r");
						break;
					case '\t':
						literal.Append(@"\t");
						break;
					case '\v':
						literal.Append(@"\v");
						break;
					default:
						if (Char.GetUnicodeCategory(c) != UnicodeCategory.Control) literal.Append(c);
						else literal.Append(((ushort) c).ToString("x4"));
						break;
				}
			}
			literal.Append("\"");
			return literal.ToString();
		}
	}

}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using UnityEngine;

namespace CYM.Lua
{
	public static class LuaReader
	{
		/// <summary>
		///		Reads a Lua Table and maps it to a CLR type.
		/// </summary>
		/// <typeparam name="T">Type of the CLR object.</typeparam>
		/// <param name="luaTable">Lua Table containing the data for the object.</param>
		/// <returns>CLR object of type T.</returns>
		public static T Read<T>(Table luaTable)
		{
			return (T)Convert(DynValue.NewTable(luaTable), typeof(T));
		}

		/// <summary>
		///		Reads a Lua variable and maps its contents to a CLR type.
		/// </summary>
		/// <typeparam name="T">Type of the CLR object.</typeparam>
		/// <param name="luaValue">Lua value containing the data for the object.</param>
		/// <returns>CLR object of type T.</returns>
		public static T Read<T>(DynValue luaValue)
		{
			return (T) Convert(luaValue, typeof (T));
		}

        public static object Read(DynValue luaValue,Type type)
        {
            return Convert(luaValue, type);
        }

        private static void ReadProperty<T>(T obj, string propertyName, DynValue propertyValue)
		{
			try
			{
				//Debug.Log(propertyName);
				PropertyInfo property = obj.GetType().GetProperty(propertyName);

				if (property != null && property.CanWrite && propertyValue != null)
				{
					Type propertyType = property.PropertyType;
					property.SetValue(obj, Convert(propertyValue, propertyType), null);
				}
			}
			catch
			{
				Debug.Log("LuaReader: could not define property \"" + propertyName + "\".");
				throw new Exception();
			}
		}

		private static object Convert(DynValue luaValue, Type type)
		{
            // Custom Converters
            if (customReaders.ContainsKey(type)) return customReaders[type](luaValue);

            // Read basic types

            if (type == typeof (bool)) return luaValue.Boolean;
			if (type == typeof (int)) return (int) luaValue.Number;
			if (type == typeof(float)) return (float)luaValue.Number;
			if (type == typeof(double)) return luaValue.Number;
			if (type == typeof(string) && luaValue.String != null) return luaValue.String;
			if (type == typeof(byte)) return (byte)luaValue.Number;
			if (type == typeof(decimal)) return (decimal)luaValue.Number;

            // Read Lua closure
            if (luaValue.Type == DataType.Function) return luaValue.Function;

            // Read enums

            if (type.IsEnum && luaValue.String != null) return System.Convert.ChangeType(Enum.Parse(type, luaValue.String), type);

			if (luaValue.Table == null) return null;

			// Read Unity types

			if (type == typeof (Color)) return ReadColor(luaValue.Table);
			if (type == typeof (Color32)) return ReadColor32(luaValue.Table);
			if (type == typeof (Rect)) return ReadRect(luaValue.Table);
			if (type == typeof (Vector2)) return ReadVector2(luaValue.Table);
			if (type == typeof (Vector3)) return ReadVector3(luaValue.Table);
			if (type == typeof (Vector4)) return ReadVector4(luaValue.Table);

			// Read arrays

			if (type.IsArray) return ReadArray(luaValue.Table, type);

			// Read generic lists

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (List<>)) return ReadList(luaValue.Table, type);

            // read hash set
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashList<>)) return ReadHashSet(luaValue.Table, type);

            // Read generic dictionaries

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Dictionary<,>)) return ReadDictionary(luaValue.Table, type);

			// Read classes

			if (type.IsClass) return ReadClass(luaValue.Table, type);

			return null;
		}

		private static Color ReadColor(Table luaTable)
		{
			float r = luaTable[1] == null ? 0f : (float) (double) luaTable[1];
			float g = luaTable[2] == null ? 0f : (float) (double) luaTable[2];
			float b = luaTable[3] == null ? 0f : (float) (double) luaTable[3];
			float a = luaTable[4] == null ? 1f : (float) (double) luaTable[4];
			return new Color(r, g, b, a);
		}

		private static Color32 ReadColor32(Table luaTable)
		{
			byte r = luaTable[1] == null ? (byte) 0 : (byte) (double) luaTable[1];
			byte g = luaTable[2] == null ? (byte) 0 : (byte) (double) luaTable[2];
			byte b = luaTable[3] == null ? (byte) 0 : (byte) (double) luaTable[3];
			byte a = luaTable[4] == null ? (byte) 255 : (byte) (double) luaTable[4];
			return new Color32(r, g, b, a);
		}

		private static Rect ReadRect(Table luaTable)
		{
			float x = luaTable[1] == null ? 0 : (float) (double) luaTable[1];
			float y = luaTable[2] == null ? 0 : (float) (double) luaTable[2];
			float width = luaTable[3] == null ? 0 : (float) (double) luaTable[3];
			float height = luaTable[4] == null ? 0 : (float) (double) luaTable[4];
			return new Rect(x, y, width, height);
		}

		private static Vector2 ReadVector2(Table luaTable)
		{
			float x = luaTable[1] == null ? 0 : (float) (double) luaTable[1];
			float y = luaTable[2] == null ? 0 : (float) (double) luaTable[2];
			return new Vector2(x, y);
		}

		private static Vector3 ReadVector3(Table luaTable)
		{
			float x = luaTable[1] == null ? 0 : (float) (double) luaTable[1];
			float y = luaTable[2] == null ? 0 : (float) (double) luaTable[2];
			float z = luaTable[3] == null ? 0 : (float) (double) luaTable[3];
			return new Vector3(x, y, z);
		}

		private static Vector4 ReadVector4(Table luaTable)
		{
			float x = luaTable[1] == null ? 0 : (float) (double) luaTable[1];
			float y = luaTable[2] == null ? 0 : (float) (double) luaTable[2];
			float z = luaTable[3] == null ? 0 : (float) (double) luaTable[3];
			float w = luaTable[4] == null ? 0 : (float) (double) luaTable[4];
			return new Vector4(x, y, z, w);
		}

		private static Array ReadArray(Table luaTable, Type type)
		{
			Type elementType = type.GetElementType();
			if (elementType == null) return null;
			if (type.GetArrayRank() == 1)
			{
				Array array = Array.CreateInstance(elementType, luaTable.Values.Count());
				int i = 0;
				foreach (var dynValue in luaTable.Values)
				{
					array.SetValue(System.Convert.ChangeType(Convert(dynValue, elementType), elementType), i);
					i++;
				}
				return array;
			}
			if (type.GetArrayRank() == 2)
			{
				var maxLength = (from dynValue in luaTable.Values where dynValue.Table != null select dynValue.Table.Values.Count()).Concat(new[] {0}).Max();
				Array array = Array.CreateInstance(elementType, luaTable.Values.Count(), maxLength);
				int i = 0;
				foreach (var dynValue in luaTable.Values)
				{
					int j = 0;
					foreach (var subElement in dynValue.Table.Values)
					{
						array.SetValue(System.Convert.ChangeType(Convert(subElement, elementType), elementType), i, j);
						j++;
					}
					i++;
				}
				return array;
			}
			return null;
		}

		private static IList ReadList(Table luaTable, Type type)
		{
			Type elementType = type.GetGenericArguments()[0];
			var list = (IList) Activator.CreateInstance(typeof (List<>).MakeGenericType(elementType));
			foreach (DynValue value in luaTable.Values) list.Add(System.Convert.ChangeType(Convert(value, elementType), elementType));
			return list;
		}

        private static IList ReadHashSet(Table luaTable, Type type)
        {
            Type elementType = type.GetGenericArguments()[0];
            var list = (IList)Activator.CreateInstance(typeof(HashList<>).MakeGenericType(elementType));
            foreach (DynValue value in luaTable.Values) list.Add(System.Convert.ChangeType(Convert(value, elementType), elementType));
            return list;
        }

        private static IDictionary ReadDictionary(Table luaTable, Type type)
		{
			Type keyType = type.GetGenericArguments()[0];
			Type valueType = type.GetGenericArguments()[1];
			var dictionary = (IDictionary) Activator.CreateInstance(typeof (Dictionary<,>).MakeGenericType(keyType, valueType));
			foreach (TablePair pair in luaTable.Pairs)
			{
				object keyValue = System.Convert.ChangeType(Convert(pair.Key, keyType), keyType);
				if (keyValue == null) continue;
				dictionary[keyValue] = System.Convert.ChangeType(Convert(pair.Value, valueType), valueType);
			}
			return dictionary;
		}

		private static object ReadClass(Table luaTable, Type type)
		{
			object cObject = Activator.CreateInstance(type);
			ReadClassData(cObject, luaTable);
			return cObject;
		}

		/// <summary>
		///     Reads a Lua table and maps its properties to an existing CLR object of Type T.
		/// </summary>
		/// <typeparam name="T">Type of the CLR object.</typeparam>
		/// <param name="clrObject">CLR object to map the Lua data to.</param>
		/// <param name="luaValue">DynValue containing the Lua table containing the data.</param>
		public static void ReadClassData<T>(T clrObject, DynValue luaValue)
		{
			ReadClassData<T>(clrObject, luaValue.Table);
		}

		/// <summary>
		///     Reads a Lua table and maps its properties to an existing CLR object of Type T.
		/// </summary>
		/// <typeparam name="T">Type of the CLR object.</typeparam>
		/// <param name="clrObject">CLR object to map the Lua data to.</param>
		/// <param name="luaTable">Lua table containing the data.</param>
		public static void ReadClassData<T>(T clrObject, Table luaTable)
		{
			if (!typeof(T).IsValueType && clrObject == null)
			{
				Debug.LogWarning("LuaReader: method ReadObjectData called with null object.");
				return;
			}
			if (luaTable == null)
			{
				Debug.LogWarning("LuaReader: method ReadObjectData called with null Lua table.");
				return;
			}
			foreach (TablePair propertyPair in luaTable.Pairs) ReadProperty(clrObject, propertyPair.Key.String, propertyPair.Value);
		}

		/// <summary>
		///     Reads a Lua table and maps a single property to an existing CLR object of Type T.
		/// </summary>
		/// <typeparam name="T">Type of the CLR object.</typeparam>
		/// <param name="clrObject">CLR object to map the Lua data to.</param>
		/// <param name="propertyName">Name of the property to map.</param>
		/// <param name="luaTable">Lua table containing the data.</param>
		public static void ReadSingleProperty<T>(T clrObject, string propertyName, Table luaTable)
		{
			if (clrObject == null)
			{
				Debug.LogWarning("LuaReader: method ReadSingleProperty called with null object.");
				return;
			}
			if (luaTable == null)
			{
				Debug.LogWarning("LuaReader: method ReadSingleProperty called with null Lua table.");
				return;
			}
			ReadProperty(clrObject, propertyName, luaTable.Get(propertyName));
		}

        private static readonly Dictionary<Type, Func<DynValue, object>> customReaders = new Dictionary<Type, Func<DynValue, object>>();
        
        /// <summary>
        /// Adds a custom Lua reader or overrides the default reader for a specific CLR type.
        /// </summary>
        /// <param name="type">Type to set the reader for.</param>
        /// <param name="reader">function that performs the conversion from DynValue to the desired type.</param>
        public static void AddCustomReader(Type type, Func<DynValue, object> reader)
        {
            if (type == null || reader == null) return;
            customReaders[type] = reader;
        }

        /// <summary>
        /// Removes a custom Lua reader set for a specific CLR type.
        /// </summary>
        /// <param name="type">Type to remove the reader for.</param>
        public static void RemoveCustomReader(Type type)
        {
            if (type == null) return;
            customReaders.Remove(type);
        }




    }

}
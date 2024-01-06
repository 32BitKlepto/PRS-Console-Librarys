using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using XDevkit;

namespace JRPC2_Client
{
	public static class JRPC
	{
		static JRPC()
		{
			JRPC.ValueTypeSizeMap = new Dictionary<Type, int>
			{
				{
					typeof(bool),
					4
				},
				{
					typeof(byte),
					1
				},
				{
					typeof(short),
					2
				},
				{
					typeof(int),
					4
				},
				{
					typeof(long),
					8
				},
				{
					typeof(ushort),
					2
				},
				{
					typeof(uint),
					4
				},
				{
					typeof(ulong),
					8
				},
				{
					typeof(float),
					4
				},
				{
					typeof(double),
					8
				}
			};
			JRPC.StructPrimitiveSizeMap = new Dictionary<Type, int>();
			HashSet<Type> hashSet = new HashSet<Type>();
			hashSet.Add(typeof(void));
			hashSet.Add(typeof(bool));
			hashSet.Add(typeof(byte));
			hashSet.Add(typeof(short));
			hashSet.Add(typeof(int));
			hashSet.Add(typeof(long));
			hashSet.Add(typeof(ushort));
			hashSet.Add(typeof(uint));
			hashSet.Add(typeof(ulong));
			hashSet.Add(typeof(float));
			hashSet.Add(typeof(double));
			hashSet.Add(typeof(string));
			hashSet.Add(typeof(bool[]));
			hashSet.Add(typeof(byte[]));
			hashSet.Add(typeof(short[]));
			hashSet.Add(typeof(int[]));
			hashSet.Add(typeof(long[]));
			hashSet.Add(typeof(ushort[]));
			hashSet.Add(typeof(uint[]));
			hashSet.Add(typeof(ulong[]));
			hashSet.Add(typeof(float[]));
			hashSet.Add(typeof(double[]));
			hashSet.Add(typeof(string[]));
			JRPC.ValidReturnTypes = hashSet;
		}

		private static T[] ArrayReturn<T>(this IXboxConsole console, uint Address, uint Size)
		{
			T[] result;
			if (Size == 0U)
			{
				result = new T[1];
			}
			else
			{
				Type typeFromHandle = typeof(T);
				object obj = new object();
				if (typeFromHandle == typeof(short))
				{
					obj = console.ReadInt16(Address, Size);
				}
				if (typeFromHandle == typeof(ushort))
				{
					obj = console.ReadUInt16(Address, Size);
				}
				if (typeFromHandle == typeof(int))
				{
					obj = console.ReadInt32(Address, Size);
				}
				if (typeFromHandle == typeof(uint))
				{
					obj = console.ReadUInt32(Address, Size);
				}
				if (typeFromHandle == typeof(long))
				{
					obj = console.ReadInt64(Address, Size);
				}
				if (typeFromHandle == typeof(ulong))
				{
					obj = console.ReadUInt64(Address, Size);
				}
				if (typeFromHandle == typeof(float))
				{
					obj = console.ReadFloat(Address, Size);
				}
				if (typeFromHandle == typeof(byte))
				{
					obj = console.GetMemory(Address, Size);
				}
				result = (T[])obj;
			}
			return result;
		}

		public static T Call<T>(this IXboxConsole console, uint Address, params object[] Arguments) where T : struct
		{
			return (T)((object)JRPC.CallArgs(console, true, JRPC.TypeToType<T>(false), typeof(T), null, 0, Address, 0U, Arguments));
		}

		public static T Call<T>(this IXboxConsole console, JRPC.ThreadType Type, uint Address, params object[] Arguments) where T : struct
		{
			return (T)((object)JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.TypeToType<T>(false), typeof(T), null, 0, Address, 0U, Arguments));
		}

		public static T Call<T>(this IXboxConsole console, string module, int ordinal, params object[] Arguments) where T : struct
		{
			return (T)((object)JRPC.CallArgs(console, true, JRPC.TypeToType<T>(false), typeof(T), module, ordinal, 0U, 0U, Arguments));
		}

		public static T Call<T>(this IXboxConsole console, JRPC.ThreadType Type, string module, int ordinal, params object[] Arguments) where T : struct
		{
			return (T)((object)JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.TypeToType<T>(false), typeof(T), module, ordinal, 0U, 0U, Arguments));
		}

		private static object CallArgs(IXboxConsole console, bool SystemThread, uint Type, Type t, string module, int ordinal, uint Address, uint ArraySize, params object[] Arguments)
		{
			if (!JRPC.IsValidReturnType(t))
			{
				throw new Exception(string.Concat(new object[]
				{
					"Invalid type ",
					t.Name,
					Environment.NewLine,
					"JRPC only supports: bool, byte, short, int, long, ushort, uint, ulong, float, double"
				}));
			}
			console.ConnectTimeout = (console.ConversationTimeout = 400U);
			string text = string.Concat(new object[]
			{
				"consolefeatures ver=",
				JRPC.JRPCVersion,
				" type=",
				Type,
				SystemThread ? " system" : "",
				(module != null) ? string.Concat(new object[]
				{
					" module=\"",
					module,
					"\" ord=",
					ordinal
				}) : "",
				" as=",
				ArraySize,
				" params=\"A\\",
				Address.ToString("X"),
				"\\A\\",
				Arguments.Length,
				"\\"
			});
			if (Arguments.Length > 37)
			{
				throw new Exception("Can not use more than 37 paramaters in a call");
			}
			foreach (object obj in Arguments)
			{
				bool flag = false;
				if (obj is uint)
				{
					object obj2 = text;
					text = string.Concat(new object[]
					{
						obj2,
						JRPC.Int,
						"\\",
						JRPC.UIntToInt((uint)obj),
						"\\"
					});
					flag = true;
				}
				if (obj is int || obj is bool || obj is byte)
				{
					if (obj is bool)
					{
						object obj3 = text;
						text = string.Concat(new object[]
						{
							obj3,
							JRPC.Int,
							"/",
							Convert.ToInt32((bool)obj),
							"\\"
						});
					}
					else
					{
						object obj4 = text;
						text = string.Concat(new object[]
						{
							obj4,
							JRPC.Int,
							"\\",
							(obj is byte) ? Convert.ToByte(obj).ToString() : Convert.ToInt32(obj).ToString(),
							"\\"
						});
					}
					flag = true;
				}
				else if (obj is int[] || obj is uint[])
				{
					byte[] array = JRPC.IntArrayToByte((int[])obj);
					object obj5 = text;
					text = string.Concat(new object[]
					{
						obj5,
						JRPC.ByteArray.ToString(),
						"/",
						array.Length,
						"\\"
					});
					for (int j = 0; j < array.Length; j++)
					{
						text += array[j].ToString("X2");
					}
					text += "\\";
					flag = true;
				}
				else if (obj is string)
				{
					string text2 = (string)obj;
					object obj6 = text;
					text = string.Concat(new object[]
					{
						obj6,
						JRPC.ByteArray.ToString(),
						"/",
						text2.Length,
						"\\",
						((string)obj).ToHexString(),
						"\\"
					});
					flag = true;
				}
				else if (obj is double)
				{
					double num = (double)obj;
					string text3 = text;
					text = string.Concat(new string[]
					{
						text3,
						JRPC.Float.ToString(),
						"\\",
						num.ToString(),
						"\\"
					});
					flag = true;
				}
				else if (obj is float)
				{
					float num2 = (float)obj;
					string text3 = text;
					text = string.Concat(new string[]
					{
						text3,
						JRPC.Float.ToString(),
						"\\",
						num2.ToString(),
						"\\"
					});
					flag = true;
				}
				else if (obj is float[])
				{
					float[] array2 = (float[])obj;
					string text3 = text;
					string[] values = new string[]
					{
						text3,
						JRPC.ByteArray.ToString(),
						"/",
						(array2.Length * 4).ToString(),
						"\\"
					};
					text = string.Concat(values);
					for (int k = 0; k < array2.Length; k++)
					{
						byte[] bytes = BitConverter.GetBytes(array2[k]);
						Array.Reverse(bytes);
						for (int l = 0; l < 4; l++)
						{
							text += bytes[l].ToString("X2");
						}
					}
					text += "\\";
					flag = true;
				}
				else if (obj is byte[])
				{
					byte[] array3 = (byte[])obj;
					object obj2 = text;
					text = string.Concat(new object[]
					{
						obj2,
						JRPC.ByteArray.ToString(),
						"/",
						array3.Length,
						"\\"
					});
					for (int m = 0; m < array3.Length; m++)
					{
						text += array3[m].ToString("X2");
					}
					text += "\\";
					flag = true;
				}
				if (!flag)
				{
					string text3 = text;
					text = string.Concat(new string[]
					{
						text3,
						JRPC.Uint64.ToString(),
						"\\",
						JRPC.ConvertToUInt64(obj).ToString(),
						"\\"
					});
				}
			}
			text += "\"";
			string text4 = JRPC.SendCommand(console, text);
			string text5 = "buf_addr=";
			while (text4.Contains(text5))
			{
				Thread.Sleep(250);
				text4 = JRPC.SendCommand(console, "consolefeatures " + text5 + "0x" + uint.Parse(text4.Substring(text4.find(text5) + text5.Length), NumberStyles.HexNumber).ToString("X"));
			}
			console.ConversationTimeout = 2000U;
			console.ConnectTimeout = 500U;
			switch (Type)
			{
				case 1U:
					{
						uint num3 = uint.Parse(text4.Substring(text4.find(" ") + 1), NumberStyles.HexNumber);
						if (t == typeof(uint))
						{
							return num3;
						}
						if (t == typeof(int))
						{
							return JRPC.UIntToInt(num3);
						}
						if (t == typeof(short))
						{
							return short.Parse(text4.Substring(text4.find(" ") + 1), NumberStyles.HexNumber);
						}
						if (t == typeof(ushort))
						{
							return ushort.Parse(text4.Substring(text4.find(" ") + 1), NumberStyles.HexNumber);
						}
						break;
					}
				case 2U:
					{
						string text6 = text4.Substring(text4.find(" ") + 1);
						if (t == typeof(string))
						{
							return text6;
						}
						if (t == typeof(char[]))
						{
							return text6.ToCharArray();
						}
						break;
					}
				case 3U:
					if (t == typeof(double))
					{
						return double.Parse(text4.Substring(text4.find(" ") + 1));
					}
					if (t == typeof(float))
					{
						return float.Parse(text4.Substring(text4.find(" ") + 1));
					}
					break;
				case 4U:
					{
						byte b = byte.Parse(text4.Substring(text4.find(" ") + 1), NumberStyles.HexNumber);
						if (t == typeof(byte))
						{
							return b;
						}
						if (t == typeof(char))
						{
							return (char)b;
						}
						break;
					}
				case 5U:
					{
						string text7 = text4.Substring(text4.find(" ") + 1);
						int num4 = 0;
						string text8 = "";
						uint[] array4 = new uint[8];
						foreach (char c in text7)
						{
							if (c != ',' && c != ';')
							{
								text8 += c.ToString();
							}
							else
							{
								array4[num4] = uint.Parse(text8, NumberStyles.HexNumber);
								num4++;
								text8 = "";
							}
							if (c == ';')
							{
								return array4;
							}
						}
						return array4;
					}
				case 6U:
					{
						string text10 = text4.Substring(text4.find(" ") + 1);
						int num5 = 0;
						string text11 = "";
						float[] array5 = new float[ArraySize];
						foreach (char c2 in text10)
						{
							if (c2 != ',' && c2 != ';')
							{
								text11 += c2.ToString();
							}
							else
							{
								array5[num5] = float.Parse(text11);
								num5++;
								text11 = "";
							}
							if (c2 == ';')
							{
								return array5;
							}
						}
						return array5;
					}
				case 7U:
					{
						string text12 = text4.Substring(text4.find(" ") + 1);
						int num6 = 0;
						string text13 = "";
						byte[] array6 = new byte[ArraySize];
						foreach (char c3 in text12)
						{
							if (c3 != ',' && c3 != ';')
							{
								text13 += c3.ToString();
							}
							else
							{
								array6[num6] = byte.Parse(text13);
								num6++;
								text13 = "";
							}
							if (c3 == ';')
							{
								return array6;
							}
						}
						return array6;
					}
				case 8U:
					if (t == typeof(long))
					{
						return long.Parse(text4.Substring(text4.find(" ") + 1), NumberStyles.HexNumber);
					}
					if (t == typeof(ulong))
					{
						return ulong.Parse(text4.Substring(text4.find(" ") + 1), NumberStyles.HexNumber);
					}
					break;
			}
			if (Type == JRPC.Uint64Array)
			{
				string text14 = text4.Substring(text4.find(" ") + 1);
				int num7 = 0;
				string text15 = "";
				ulong[] array7 = new ulong[ArraySize];
				foreach (char c4 in text14)
				{
					if (c4 != ',' && c4 != ';')
					{
						text15 += c4.ToString();
					}
					else
					{
						array7[num7] = ulong.Parse(text15);
						num7++;
						text15 = "";
					}
					if (c4 == ';')
					{
						break;
					}
				}
				if (t == typeof(ulong))
				{
					return array7;
				}
				if (t == typeof(long))
				{
					long[] array8 = new long[ArraySize];
					int num8 = 0;
					while ((long)num8 < (long)((ulong)ArraySize))
					{
						array8[num8] = BitConverter.ToInt64(BitConverter.GetBytes(array7[num8]), 0);
						num8++;
					}
					return array8;
				}
			}
			object result;
			if (Type == JRPC.Void)
			{
				result = 0;
			}
			else
			{
				result = ulong.Parse(text4.Substring(text4.find(" ") + 1), NumberStyles.HexNumber);
			}
			return result;
		}

		public static T[] CallArray<T>(this IXboxConsole console, uint Address, uint ArraySize, params object[] Arguments) where T : struct
		{
			T[] result;
			if (ArraySize == 0U)
			{
				result = new T[1];
			}
			else
			{
				result = (T[])JRPC.CallArgs(console, true, JRPC.TypeToType<T>(true), typeof(T), null, 0, Address, ArraySize, Arguments);
			}
			return result;
		}

		public static T[] CallArray<T>(this IXboxConsole console, JRPC.ThreadType Type, uint Address, uint ArraySize, params object[] Arguments) where T : struct
		{
			T[] result;
			if (ArraySize == 0U)
			{
				result = new T[1];
			}
			else
			{
				result = (T[])JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.TypeToType<T>(true), typeof(T), null, 0, Address, ArraySize, Arguments);
			}
			return result;
		}

		public static T[] CallArray<T>(this IXboxConsole console, string module, int ordinal, uint ArraySize, params object[] Arguments) where T : struct
		{
			T[] result;
			if (ArraySize == 0U)
			{
				result = new T[1];
			}
			else
			{
				result = (T[])JRPC.CallArgs(console, true, JRPC.TypeToType<T>(true), typeof(T), module, ordinal, 0U, ArraySize, Arguments);
			}
			return result;
		}

		public static T[] CallArray<T>(this IXboxConsole console, JRPC.ThreadType Type, string module, int ordinal, uint ArraySize, params object[] Arguments) where T : struct
		{
			T[] result;
			if (ArraySize == 0U)
			{
				result = new T[1];
			}
			else
			{
				result = (T[])JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.TypeToType<T>(true), typeof(T), module, ordinal, 0U, ArraySize, Arguments);
			}
			return result;
		}

		public static string CallString(this IXboxConsole console, uint Address, params object[] Arguments)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[JRPC][",
				DateTime.Now.ToShortTimeString(),
				"] > CallString(",
				Address,
				"new object[] { "
			}));
			foreach (object obj in Arguments)
			{
				File.AppendAllText(JRPC.logfile, obj.ToString() + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			return (string)JRPC.CallArgs(console, true, JRPC.String, typeof(string), null, 0, Address, 0U, Arguments);
		}

		public static string CallString(this IXboxConsole console, JRPC.ThreadType Type, uint Address, params object[] Arguments)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[JRPC][",
				DateTime.Now.ToShortTimeString(),
				"] > CallString(",
				Type,
				", ",
				Address,
				", new object[] { "
			}));
			foreach (object obj in Arguments)
			{
				File.AppendAllText(JRPC.logfile, obj.ToString() + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			return (string)JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.String, typeof(string), null, 0, Address, 0U, Arguments);
		}
		
		public static string CallString(this IXboxConsole console, string module, int ordinal, params object[] Arguments)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[JRPC][",
				DateTime.Now.ToShortTimeString(),
				"] > CallString(",
				module,
				", ",
				ordinal,
				", new object[] { "
			}));
			foreach (object obj in Arguments)
			{
				File.AppendAllText(JRPC.logfile, obj.ToString() + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			return (string)JRPC.CallArgs(console, true, JRPC.String, typeof(string), module, ordinal, 0U, 0U, Arguments);
		}

		public static string CallString(this IXboxConsole console, JRPC.ThreadType Type, string module, int ordinal, params object[] Arguments)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[JRPC][",
				DateTime.Now.ToShortTimeString(),
				"] > CallString(",
				Type,
				", ",
				module,
				", ",
				ordinal,
				", new object[] { "
			}));
			foreach (object obj in Arguments)
			{
				File.AppendAllText(JRPC.logfile, obj.ToString() + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			return (string)JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.String, typeof(string), module, ordinal, 0U, 0U, Arguments);
		}

		public static void CallVoid(this IXboxConsole console, uint Address, params object[] Arguments)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[JRPC][",
				DateTime.Now.ToShortTimeString(),
				"] > CallString(",
				Address,
				", new object[] { "
			}));
			foreach (object obj in Arguments)
			{
				File.AppendAllText(JRPC.logfile, obj.ToString() + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			JRPC.CallArgs(console, true, JRPC.Void, typeof(void), null, 0, Address, 0U, Arguments);
		}

		public static void CallVoid(this IXboxConsole console, JRPC.ThreadType Type, uint Address, params object[] Arguments)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[JPRC][",
				DateTime.Now.ToShortTimeString(),
				"] > CallString(",
				Address,
				", new object[] { "
			}));
			foreach (object obj in Arguments)
			{
				File.AppendAllText(JRPC.logfile, obj.ToString() + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.Void, typeof(void), null, 0, Address, 0U, Arguments);
		}

		public static void CallVoid(this IXboxConsole console, string module, int ordinal, params object[] Arguments)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[JRPC][",
				DateTime.Now.ToShortTimeString(),
				"] > CallString(",
				module,
				", ",
				ordinal,
				", new object[] { "
			}));
			foreach (object obj in Arguments)
			{
				File.AppendAllText(JRPC.logfile, obj.ToString() + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			JRPC.CallArgs(console, true, JRPC.Void, typeof(void), module, ordinal, 0U, 0U, Arguments);
		}

		public static void CallVoid(this IXboxConsole console, JRPC.ThreadType Type, string module, int ordinal, params object[] Arguments)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[JRPC][",
				DateTime.Now.ToShortTimeString(),
				"] > CallString(",
				Type,
				", ",
				module,
				", ",
				ordinal,
				", new object[] { "
			}));
			foreach (object obj in Arguments)
			{
				File.AppendAllText(JRPC.logfile, obj.ToString() + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.Void, typeof(void), module, ordinal, 0U, 0U, Arguments);
		}

		public static bool Connect(this IXboxConsole console, out IXboxConsole Console, string XboxNameOrIP = "default")
		{
			File.AppendAllText(JRPC.logfile, "[JRPC][" + DateTime.Now.ToShortTimeString() + "] > Connect();" + Environment.NewLine);
			if (XboxNameOrIP == "default")
			{
				XboxNameOrIP = new XboxManager().DefaultConsole;
			}
			IXboxConsole xboxConsole = ((IXboxManager)new XboxManager()).OpenConsole(XboxNameOrIP);
			int num = 0;
			bool flag = false;
			while (!flag)
			{
				try
				{
					JRPC.connectionId = xboxConsole.OpenConnection(null);
					flag = true;
				}
				catch (COMException ex)
				{
					if (ex.ErrorCode != JRPC.UIntToInt(2195325184U))
					{
						Console = xboxConsole;
						return false;
					}
					if (num >= 3)
					{
						Console = xboxConsole;
						return false;
					}
					num++;
					Thread.Sleep(100);
				}
			}
			Console = xboxConsole;
			return true;
		}

		public static string ConsoleType(this IXboxConsole console)
		{
			File.AppendAllText(JRPC.logfile, "[JRPC][" + DateTime.Now.ToShortTimeString() + "] > ConsoleType();" + Environment.NewLine);
			string command = "consolefeatures ver=" + JRPC.JRPCVersion + " type=17 params=\"A\\0\\A\\0\\\"";
			string text = JRPC.SendCommand(console, command);
			return text.Substring(text.find(" ") + 1);
		}

		public static void constantMemorySet(this IXboxConsole console, uint Address, uint Value)
		{
			JRPC.constantMemorySetting(console, Address, Value, false, 0U, false, 0U);
		}

		public static void constantMemorySet(this IXboxConsole console, uint Address, uint Value, uint TitleID)
		{
			JRPC.constantMemorySetting(console, Address, Value, false, 0U, true, TitleID);
		}

		public static void constantMemorySet(this IXboxConsole console, uint Address, uint Value, uint IfValue, uint TitleID)
		{
			JRPC.constantMemorySetting(console, Address, Value, true, IfValue, true, TitleID);
		}

		public static void constantMemorySetting(IXboxConsole console, uint Address, uint Value, bool useIfValue, uint IfValue, bool usetitleID, uint TitleID)
		{
			string command = string.Concat(new object[]
			{
				"consolefeatures ver=",
				JRPC.JRPCVersion,
				" type=18 params=\"A\\",
				Address.ToString("X"),
				"\\A\\5\\",
				JRPC.Int,
				"\\",
				JRPC.UIntToInt(Value),
				"\\",
				JRPC.Int,
				"\\",
				useIfValue ? 1 : 0,
				"\\",
				JRPC.Int,
				"\\",
				IfValue,
				"\\",
				JRPC.Int,
				"\\",
				usetitleID ? 1 : 0,
				"\\",
				JRPC.Int,
				"\\",
				JRPC.UIntToInt(TitleID),
				"\\\""
			});
			JRPC.SendCommand(console, command);
		}

		internal static ulong ConvertToUInt64(object o)
		{
			ulong result;
			if (o is bool)
			{
				if ((bool)o)
				{
					result = 1UL;
				}
				else
				{
					result = 0UL;
				}
			}
			else if (o is byte)
			{
				result = (ulong)((byte)o);
			}
			else if (o is short)
			{
				result = (ulong)((long)((short)o));
			}
			else if (o is int)
			{
				result = (ulong)((long)((int)o));
			}
			else if (o is long)
			{
				result = (ulong)((long)o);
			}
			else if (o is ushort)
			{
				result = (ulong)((ushort)o);
			}
			else if (o is uint)
			{
				result = (ulong)((uint)o);
			}
			else if (o is ulong)
			{
				result = (ulong)o;
			}
			else if (o is float)
			{
				result = (ulong)BitConverter.DoubleToInt64Bits((double)((float)o));
			}
			else if (o is double)
			{
				result = (ulong)BitConverter.DoubleToInt64Bits((double)o);
			}
			else
			{
				result = 0UL;
			}
			return result;
		}

		public static int find(this string String, string _Ptr)
		{
			if (_Ptr.Length != 0 && String.Length != 0)
			{
				for (int i = 0; i < String.Length; i++)
				{
					if (String[i] == _Ptr[0])
					{
						bool flag = true;
						for (int j = 0; j < _Ptr.Length; j++)
						{
							if (String[i + j] != _Ptr[j])
							{
								flag = false;
							}
						}
						if (flag)
						{
							return i;
						}
					}
				}
			}
			return -1;
		}

		public static string GetCPUKey(this IXboxConsole console)
		{
			File.AppendAllText(JRPC.logfile, "[JRPC][" + DateTime.Now.ToShortTimeString() + "] > GetCPUKey();" + Environment.NewLine);
			string command = "consolefeatures ver=" + JRPC.JRPCVersion + " type=10 params=\"A\\0\\A\\0\\\"";
			string text = JRPC.SendCommand(console, command);
			return "9C9ADE829214FA93DEB5C17A88B151A2";
		}

		public static uint GetKernalVersion(this IXboxConsole console)
		{
			File.AppendAllText(JRPC.logfile, "[JRPC][" + DateTime.Now.ToShortTimeString() + "] > GetKernalVersion();" + Environment.NewLine);
			string command = "consolefeatures ver=" + JRPC.JRPCVersion + " type=13 params=\"A\\0\\A\\0\\\"";
			string text = JRPC.SendCommand(console, command);
			return uint.Parse(text.Substring(text.find(" ") + 1));
		}

		public static byte[] GetMemory(this IXboxConsole console, uint Address, uint Length)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[JRPC][",
				DateTime.Now.ToShortTimeString(),
				"] > GetMemory(0x",
				Address.ToString("X8"),
				", ",
				Length.ToString("X4"),
				");",
				Environment.NewLine
			}));
			uint num = 0U;
			byte[] array = new byte[Length];
			console.DebugTarget.GetMemory(Address, Length, array, out num);
			console.DebugTarget.InvalidateMemoryCache(true, Address, Length);
			return array;
		}

		public static uint GetTemperature(this IXboxConsole console, JRPC.TemperatureType TemperatureType)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[JRPC][",
				DateTime.Now.ToShortTimeString(),
				"] > GetTemperature(",
				TemperatureType.ToString(),
				");",
				Environment.NewLine
			}));
			string command = string.Concat(new object[]
			{
				"consolefeatures ver=",
				JRPC.JRPCVersion,
				" type=15 params=\"A\\0\\A\\1\\",
				JRPC.Int,
				"\\",
				(int)TemperatureType,
				"\\\""
			});
			string text = JRPC.SendCommand(console, command);
			return uint.Parse(text.Substring(text.find(" ") + 1), NumberStyles.HexNumber);
		}

		private static byte[] IntArrayToByte(int[] iArray)
		{
			byte[] array = new byte[iArray.Length * 4];
			int i = 0;
			int num = 0;
			while (i < iArray.Length)
			{
				for (int j = 0; j < 4; j++)
				{
					array[num + j] = BitConverter.GetBytes(iArray[i])[j];
				}
				i++;
				num += 4;
			}
			return array;
		}

		internal static bool IsValidReturnType(Type t)
		{
			return JRPC.ValidReturnTypes.Contains(t);
		}

		internal static bool IsValidStructType(Type t)
		{
			return !t.IsPrimitive && t.IsValueType;
		}

		public static void Push(this byte[] InArray, out byte[] OutArray, byte Value)
		{
			OutArray = new byte[InArray.Length + 1];
			InArray.CopyTo(OutArray, 0);
			OutArray[InArray.Length] = Value;
		}

		public static bool ReadBool(this IXboxConsole console, uint Address)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadBool(0x",
				Address.ToString("X8"),
				");",
				Environment.NewLine
			}));
			return console.GetMemory(Address, 1U)[0] != 0;
		}

		public static byte ReadByte(this IXboxConsole console, uint Address)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadByte(0x",
				Address.ToString("X8"),
				");",
				Environment.NewLine
			}));
			return console.GetMemory(Address, 1U)[0];
		}

		public static float ReadFloat(this IXboxConsole console, uint Address)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadFloat(0x",
				Address.ToString("X8"),
				");",
				Environment.NewLine
			}));
			byte[] memory = console.GetMemory(Address, 4U);
			JRPC.ReverseBytes(memory, 4);
			return BitConverter.ToSingle(memory, 0);
		}

		public static float[] ReadFloat(this IXboxConsole console, uint Address, uint ArraySize)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadFloat(0x",
				Address.ToString("X8"),
				", ",
				ArraySize,
				");",
				Environment.NewLine
			}));
			float[] array = new float[ArraySize];
			byte[] memory = console.GetMemory(Address, ArraySize * 4U);
			JRPC.ReverseBytes(memory, 4);
			int num = 0;
			while ((long)num < (long)((ulong)ArraySize))
			{
				array[num] = BitConverter.ToSingle(memory, num * 4);
				num++;
			}
			return array;
		}

		public static short ReadInt16(this IXboxConsole console, uint Address)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadInt16(0x",
				Address.ToString("X8"),
				");",
				Environment.NewLine
			}));
			byte[] memory = console.GetMemory(Address, 2U);
			JRPC.ReverseBytes(memory, 2);
			return BitConverter.ToInt16(memory, 0);
		}

		public static short[] ReadInt16(this IXboxConsole console, uint Address, uint ArraySize)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteInt16(0x",
				Address.ToString("X8"),
				", ",
				ArraySize,
				");",
				Environment.NewLine
			}));
			short[] array = new short[ArraySize];
			byte[] memory = console.GetMemory(Address, ArraySize * 2U);
			JRPC.ReverseBytes(memory, 2);
			int num = 0;
			while ((long)num < (long)((ulong)ArraySize))
			{
				array[num] = BitConverter.ToInt16(memory, num * 2);
				num++;
			}
			return array;
		}

		public static int ReadInt32(this IXboxConsole console, uint Address)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadInt32(0x",
				Address.ToString("X8"),
				");",
				Environment.NewLine
			}));
			byte[] memory = console.GetMemory(Address, 4U);
			JRPC.ReverseBytes(memory, 4);
			return BitConverter.ToInt32(memory, 0);
		}

		public static int[] ReadInt32(this IXboxConsole console, uint Address, uint ArraySize)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadInt32(0x",
				Address.ToString("X8"),
				", ",
				ArraySize,
				");",
				Environment.NewLine
			}));
			int[] array = new int[ArraySize];
			byte[] memory = console.GetMemory(Address, ArraySize * 4U);
			JRPC.ReverseBytes(memory, 4);
			int num = 0;
			while ((long)num < (long)((ulong)ArraySize))
			{
				array[num] = BitConverter.ToInt32(memory, num * 4);
				num++;
			}
			return array;
		}

		public static long ReadInt64(this IXboxConsole console, uint Address)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadInt64(0x",
				Address.ToString("X8"),
				");",
				Environment.NewLine
			}));
			byte[] memory = console.GetMemory(Address, 8U);
			JRPC.ReverseBytes(memory, 8);
			return BitConverter.ToInt64(memory, 0);
		}

		public static long[] ReadInt64(this IXboxConsole console, uint Address, uint ArraySize)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadInt64(0x",
				Address.ToString("X8"),
				", ",
				ArraySize,
				");",
				Environment.NewLine
			}));
			long[] array = new long[ArraySize];
			byte[] memory = console.GetMemory(Address, ArraySize * 8U);
			JRPC.ReverseBytes(memory, 8);
			int num = 0;
			while ((long)num < (long)((ulong)ArraySize))
			{
				array[num] = (long)((ulong)BitConverter.ToUInt32(memory, num * 8));
				num++;
			}
			return array;
		}

		public static sbyte ReadSByte(this IXboxConsole console, uint Address)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadSByte(0x",
				Address.ToString("X8"),
				");",
				Environment.NewLine
			}));
			return (sbyte)console.GetMemory(Address, 1U)[0];
		}

		public static string ReadString(this IXboxConsole console, uint Address, uint size)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadString(0x",
				Address.ToString("X8"),
				", ",
				size,
				");",
				Environment.NewLine
			}));
			return Encoding.UTF8.GetString(console.GetMemory(Address, size));
		}

		public static ushort ReadUInt16(this IXboxConsole console, uint Address)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadUInt16(0x",
				Address.ToString("X8"),
				");",
				Environment.NewLine
			}));
			byte[] memory = console.GetMemory(Address, 2U);
			JRPC.ReverseBytes(memory, 2);
			return BitConverter.ToUInt16(memory, 0);
		}

		public static ushort[] ReadUInt16(this IXboxConsole console, uint Address, uint ArraySize)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadUInt16(0x",
				Address.ToString("X8"),
				", ",
				ArraySize,
				");",
				Environment.NewLine
			}));
			ushort[] array = new ushort[ArraySize];
			byte[] memory = console.GetMemory(Address, ArraySize * 2U);
			JRPC.ReverseBytes(memory, 2);
			int num = 0;
			while ((long)num < (long)((ulong)ArraySize))
			{
				array[num] = BitConverter.ToUInt16(memory, num * 2);
				num++;
			}
			return array;
		}

		public static uint ReadUInt32(this IXboxConsole console, uint Address)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadUInt32(0x",
				Address.ToString("X8"),
				");",
				Environment.NewLine
			}));
			byte[] memory = console.GetMemory(Address, 4U);
			JRPC.ReverseBytes(memory, 4);
			return BitConverter.ToUInt32(memory, 0);
		}

		public static uint[] ReadUInt32(this IXboxConsole console, uint Address, uint ArraySize)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadUInt32(0x",
				Address.ToString("X8"),
				", ",
				ArraySize,
				");",
				Environment.NewLine
			}));
			uint[] array = new uint[ArraySize];
			byte[] memory = console.GetMemory(Address, ArraySize * 4U);
			JRPC.ReverseBytes(memory, 4);
			int num = 0;
			while ((long)num < (long)((ulong)ArraySize))
			{
				array[num] = BitConverter.ToUInt32(memory, num * 4);
				num++;
			}
			return array;
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00004D34 File Offset: 0x00002F34
		public static ulong ReadUInt64(this IXboxConsole console, uint Address)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadUInt64(0x",
				Address.ToString("X8"),
				");",
				Environment.NewLine
			}));
			byte[] memory = console.GetMemory(Address, 8U);
			JRPC.ReverseBytes(memory, 8);
			return BitConverter.ToUInt64(memory, 0);
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00004DB8 File Offset: 0x00002FB8
		public static ulong[] ReadUInt64(this IXboxConsole console, uint Address, uint ArraySize)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] ReadUInt64(0x",
				Address.ToString("X8"),
				", ",
				ArraySize,
				");",
				Environment.NewLine
			}));
			ulong[] array = new ulong[ArraySize];
			byte[] memory = console.GetMemory(Address, ArraySize * 8U);
			JRPC.ReverseBytes(memory, 8);
			int num = 0;
			while ((long)num < (long)((ulong)ArraySize))
			{
				array[num] = (ulong)BitConverter.ToUInt32(memory, num * 8);
				num++;
			}
			return array;
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00004E7C File Offset: 0x0000307C
		public static uint ResolveFunction(this IXboxConsole console, string ModuleName, uint Ordinal)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[JRPC][",
				DateTime.Now.ToShortTimeString(),
				"] > ResolveFunction(",
				ModuleName,
				", ",
				Ordinal,
				");",
				Environment.NewLine
			}));
			string command = string.Concat(new object[]
			{
				"consolefeatures ver=",
				JRPC.JRPCVersion,
				" type=9 params=\"A\\0\\A\\2\\",
				JRPC.String,
				"/",
				ModuleName.Length,
				"\\",
				ModuleName.ToHexString(),
				"\\",
				JRPC.Int,
				"\\",
				Ordinal,
				"\\\""
			});
			string text = JRPC.SendCommand(console, command);
			return uint.Parse(text.Substring(text.find(" ") + 1), NumberStyles.HexNumber);
		}

		private static void ReverseBytes(byte[] buffer, int groupSize)
		{
			if (buffer.Length % groupSize != 0)
			{
				throw new ArgumentException("Group size must be a multiple of the buffer length", "groupSize");
			}
			for (int i = 0; i < buffer.Length; i += groupSize)
			{
				int j = i;
				int num = i + groupSize - 1;
				while (j < num)
				{
					byte b = buffer[j];
					buffer[j] = buffer[num];
					buffer[num] = b;
					j++;
					num--;
				}
			}
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00005014 File Offset: 0x00003214
		private static string SendCommand(IXboxConsole console, string Command)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[JRPC][",
				DateTime.Now.ToShortTimeString(),
				"] >  SendCommand();",
				Environment.NewLine
			}));
			uint num = JRPC.connectionId;
			string text;
			try
			{
				console.SendTextCommand(JRPC.connectionId, Command, out text);
				if (text.Contains("error="))
				{
					throw new Exception(text.Substring(11));
				}
				if (text.Contains("DEBUG"))
				{
					throw new Exception("JRPC is not installed on the current console");
				}
			}
			catch (COMException ex)
			{
				if (ex.ErrorCode == JRPC.UIntToInt(2195324935U))
				{
					throw new Exception("JRPC is not installed on the current console");
				}
				throw ex;
			}
			return text;
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00005114 File Offset: 0x00003314
		public static void SetLeds(this IXboxConsole console, JRPC.LEDState Top_Left, JRPC.LEDState Top_Right, JRPC.LEDState Bottom_Left, JRPC.LEDState Bottom_Right)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[JRPC][",
				DateTime.Now.ToShortTimeString(),
				"] > SetLeds(",
				Top_Left,
				", ",
				Top_Right,
				", ",
				Bottom_Left,
				", ",
				Bottom_Right,
				");",
				Environment.NewLine
			}));
			string command = string.Concat(new object[]
			{
				"consolefeatures ver=",
				JRPC.JRPCVersion,
				" type=14 params=\"A\\0\\A\\4\\",
				JRPC.Int,
				"\\",
				(uint)Top_Left,
				"\\",
				JRPC.Int,
				"\\",
				(uint)Top_Right,
				"\\",
				JRPC.Int,
				"\\",
				(uint)Bottom_Left,
				"\\",
				JRPC.Int,
				"\\",
				(uint)Bottom_Right,
				"\\\""
			});
			JRPC.SendCommand(console, command);
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00005284 File Offset: 0x00003484
		public static void SetMemory(this IXboxConsole console, uint Address, byte[] Data)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] SetMemory(0x",
				Address.ToString("X8"),
				", new byte[] { "
			}));
			foreach (byte b in Data)
			{
				File.AppendAllText("./logs/jrpc.log", "0x" + b.ToString("X2") + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00005344 File Offset: 0x00003544
		public static void ShutDownConsole(this IXboxConsole console)
		{
			try
			{
				File.AppendAllText(JRPC.logfile, "[JRPC][" + DateTime.Now.ToShortTimeString() + "] > ShutDownConsole();" + Environment.NewLine);
				string command = "consolefeatures ver=" + JRPC.JRPCVersion + " type=11 params=\"A\\0\\A\\0\\\"";
				JRPC.SendCommand(console, command);
			}
			catch
			{
			}
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000053BC File Offset: 0x000035BC
		public static byte[] ToByteArray(this string String)
		{
			byte[] array = new byte[String.Length + 1];
			for (int i = 0; i < String.Length; i++)
			{
				array[i] = (byte)String[i];
			}
			return array;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00005400 File Offset: 0x00003600
		public static string ToHexString(this string String)
		{
			string text = "";
			for (int i = 0; i < String.Length; i++)
			{
				text += ((byte)String[i]).ToString("X2");
			}
			return text;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00005454 File Offset: 0x00003654
		public static byte[] ToWCHAR(this string String)
		{
			return JRPC.WCHAR(String);
		}

		// Token: 0x0600003F RID: 63 RVA: 0x0000546C File Offset: 0x0000366C
		private static uint TypeToType<T>(bool Array) where T : struct
		{
			Type typeFromHandle = typeof(T);
			uint result;
			if (typeFromHandle == typeof(int) || typeFromHandle == typeof(uint) || typeFromHandle == typeof(short) || typeFromHandle == typeof(ushort))
			{
				if (Array)
				{
					result = JRPC.IntArray;
				}
				else
				{
					result = JRPC.Int;
				}
			}
			else if (typeFromHandle == typeof(string) || typeFromHandle == typeof(char[]))
			{
				result = JRPC.String;
			}
			else if (typeFromHandle == typeof(float) || typeFromHandle == typeof(double))
			{
				if (Array)
				{
					result = JRPC.FloatArray;
				}
				else
				{
					result = JRPC.Float;
				}
			}
			else if (typeFromHandle == typeof(byte) || typeFromHandle == typeof(char))
			{
				if (Array)
				{
					result = JRPC.ByteArray;
				}
				else
				{
					result = JRPC.Byte;
				}
			}
			else if ((typeFromHandle == typeof(ulong) || typeFromHandle == typeof(long)) && Array)
			{
				result = JRPC.Uint64Array;
			}
			else
			{
				result = JRPC.Uint64;
			}
			return result;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x000055CC File Offset: 0x000037CC
		private static int UIntToInt(uint Value)
		{
			return BitConverter.ToInt32(BitConverter.GetBytes(Value), 0);
		}

		// Token: 0x06000041 RID: 65 RVA: 0x000055EC File Offset: 0x000037EC
		public static byte[] WCHAR(string String)
		{
			byte[] array = new byte[String.Length * 2 + 2];
			int num = 1;
			for (int i = 0; i < String.Length; i++)
			{
				array[num] = (byte)String[i];
				num += 2;
			}
			return array;
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00005640 File Offset: 0x00003840
		public static void WriteBool(this IXboxConsole console, uint Address, bool Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteBool(0x",
				Address.ToString("X8"),
				", ",
				Value,
				");",
				Environment.NewLine
			}));
			console.SetMemory(Address, new byte[]
			{

			});
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000056D0 File Offset: 0x000038D0
		public static void WriteBool(this IXboxConsole console, uint Address, bool[] Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteBool(0x",
				Address.ToString("X8"),
				",  new bool[] { "
			}));
			foreach (bool flag in Value)
			{
				File.AppendAllText(JRPC.logfile, flag + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			byte[] array = new byte[0];
			for (int j = 0; j < Value.Length; j++)
			{

			}
			console.SetMemory(Address, array);
		}

		// Token: 0x06000044 RID: 68 RVA: 0x000057C0 File Offset: 0x000039C0
		public static void WriteByte(this IXboxConsole console, uint Address, byte Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteByte(0x",
				Address.ToString("X8"),
				", ",
				Value,
				");",
				Environment.NewLine
			}));
			console.SetMemory(Address, new byte[]
			{
				Value
			});
		}

		// Token: 0x06000045 RID: 69 RVA: 0x0000584C File Offset: 0x00003A4C
		public static void WriteByte(this IXboxConsole console, uint Address, byte[] Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteByte(0x",
				Address.ToString("X8"),
				",  new byte[] { "
			}));
			foreach (byte b in Value)
			{
				File.AppendAllText(JRPC.logfile, b + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			console.SetMemory(Address, Value);
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00005908 File Offset: 0x00003B08
		public static void WriteFloat(this IXboxConsole console, uint Address, float[] Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteFloat(0x",
				Address.ToString("X8"),
				",  new float[] { "
			}));
			foreach (float num in Value)
			{
				File.AppendAllText(JRPC.logfile, num + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			byte[] array = new byte[Value.Length * 4];
			for (int j = 0; j < Value.Length; j++)
			{
				BitConverter.GetBytes(Value[j]).CopyTo(array, j * 4);
			}
			JRPC.ReverseBytes(array, 4);
			console.SetMemory(Address, array);
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00005A04 File Offset: 0x00003C04
		public static void WriteFloat(this IXboxConsole console, uint Address, float Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteFloat(0x",
				Address.ToString("X8"),
				", ",
				Value,
				");",
				Environment.NewLine
			}));
			byte[] bytes = BitConverter.GetBytes(Value);
			Array.Reverse(bytes);
			console.SetMemory(Address, bytes);
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00005A94 File Offset: 0x00003C94
		public static void WriteInt16(this IXboxConsole console, uint Address, short[] Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteInt16(0x",
				Address.ToString("X8"),
				",  new short[] { "
			}));
			foreach (short num in Value)
			{
				File.AppendAllText(JRPC.logfile, num + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			byte[] array = new byte[Value.Length * 2];
			for (int j = 0; j < Value.Length; j++)
			{
				BitConverter.GetBytes(Value[j]).CopyTo(array, j * 2);
			}
			JRPC.ReverseBytes(array, 2);
			console.SetMemory(Address, array);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00005B90 File Offset: 0x00003D90
		public static void WriteInt16(this IXboxConsole console, uint Address, short Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteInt16(0x",
				Address.ToString("X8"),
				", ",
				Value,
				");",
				Environment.NewLine
			}));
			byte[] bytes = BitConverter.GetBytes(Value);
			JRPC.ReverseBytes(bytes, 2);
			console.SetMemory(Address, bytes);
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00005C20 File Offset: 0x00003E20
		public static void WriteInt32(this IXboxConsole console, uint Address, int[] Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteInt16(0x",
				Address.ToString("X8"),
				", new int[] { "
			}));
			foreach (int num in Value)
			{
				File.AppendAllText(JRPC.logfile, num + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			byte[] array = new byte[Value.Length * 4];
			for (int j = 0; j < Value.Length; j++)
			{
				BitConverter.GetBytes(Value[j]).CopyTo(array, j * 4);
			}
			JRPC.ReverseBytes(array, 4);
			console.SetMemory(Address, array);
		}

		public static void WriteInt32(this IXboxConsole console, uint Address, int Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteInt32(0x",
				Address.ToString("X8"),
				", ",
				Value,
				");",
				Environment.NewLine
			}));
			byte[] bytes = BitConverter.GetBytes(Value);
			JRPC.ReverseBytes(bytes, 4);
			console.SetMemory(Address, bytes);
		}

		public static void WriteInt64(this IXboxConsole console, uint Address, long[] Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteInt64(0x",
				Address.ToString("X8"),
				", new long[] { "
			}));
			foreach (long num in Value)
			{
				File.AppendAllText(JRPC.logfile, num + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			byte[] array = new byte[Value.Length * 8];
			for (int j = 0; j < Value.Length; j++)
			{
				BitConverter.GetBytes(Value[j]).CopyTo(array, j * 8);
			}
			JRPC.ReverseBytes(array, 8);
			console.SetMemory(Address, array);
		}

		public static void WriteInt64(this IXboxConsole console, uint Address, long Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteInt64(0x",
				Address.ToString("X8"),
				", ",
				Value,
				");",
				Environment.NewLine
			}));
			byte[] bytes = BitConverter.GetBytes(Value);
			JRPC.ReverseBytes(bytes, 8);
			console.SetMemory(Address, bytes);
		}

		public static void WriteSByte(this IXboxConsole console, uint Address, sbyte[] Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteInt64(0x",
				Address.ToString("X8"),
				", new sbyte[] { "
			}));
			foreach (sbyte b in Value)
			{
				File.AppendAllText(JRPC.logfile, b + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			byte[] array = new byte[0];
			foreach (byte value in Value)
			{
				array.Push(out array, value);
			}
			console.SetMemory(Address, array);
		}

		public static void WriteSByte(this IXboxConsole console, uint Address, sbyte Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteSByte(0x",
				Address.ToString("X8"),
				", ",
				Value,
				");",
				Environment.NewLine
			}));
			console.SetMemory(Address, new byte[]
			{
				BitConverter.GetBytes((short)Value)[0]
			});
		}

		public static void WriteString(this IXboxConsole console, uint Address, string String)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteString(0x",
				Address.ToString("X8"),
				", ",
				String,
				");",
				Environment.NewLine
			}));
			byte[] array = new byte[0];
			foreach (byte value in String)
			{
				array.Push(out array, value);
			}
			array.Push(out array, 0);
			console.SetMemory(Address, array);
		}

		public static void WriteUInt16(this IXboxConsole console, uint Address, ushort[] Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteUInt16(0x",
				Address.ToString("X8"),
				", new ushort[] { "
			}));
			foreach (ushort num in Value)
			{
				File.AppendAllText(JRPC.logfile, num + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			byte[] array = new byte[Value.Length * 2];
			for (int j = 0; j < Value.Length; j++)
			{
				BitConverter.GetBytes(Value[j]).CopyTo(array, j * 2);
			}
			JRPC.ReverseBytes(array, 2);
			console.SetMemory(Address, array);
		}

		public static void WriteUInt16(this IXboxConsole console, uint Address, ushort Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteUInt16(0x",
				Address.ToString("X8"),
				", ",
				Value,
				");",
				Environment.NewLine
			}));
			byte[] bytes = BitConverter.GetBytes(Value);
			JRPC.ReverseBytes(bytes, 2);
			console.SetMemory(Address, bytes);
		}

		public static void WriteUInt32(this IXboxConsole console, uint Address, uint[] Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteUInt32(0x",
				Address.ToString("X8"),
				", new uint[] { "
			}));
			foreach (uint num in Value)
			{
				File.AppendAllText(JRPC.logfile, num + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			byte[] array = new byte[Value.Length * 4];
			for (int j = 0; j < Value.Length; j++)
			{
				BitConverter.GetBytes(Value[j]).CopyTo(array, j * 4);
			}
			JRPC.ReverseBytes(array, 4);
			console.SetMemory(Address, array);
		}

		public static void WriteUInt32(this IXboxConsole console, uint Address, uint Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteUInt32(0x",
				Address.ToString("X8"),
				", ",
				Value,
				");",
				Environment.NewLine
			}));
			byte[] bytes = BitConverter.GetBytes(Value);
			JRPC.ReverseBytes(bytes, 4);
			console.SetMemory(Address, bytes);
		}

		public static void WriteUInt64(this IXboxConsole console, uint Address, ulong Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteUInt64(0x",
				Address.ToString("X8"),
				", ",
				Value,
				");",
				Environment.NewLine
			}));
			byte[] bytes = BitConverter.GetBytes(Value);
			JRPC.ReverseBytes(bytes, 8);
			console.SetMemory(Address, bytes);
		}

		public static void WriteUInt64(this IXboxConsole console, uint Address, ulong[] Value)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[",
				DateTime.Now.ToShortTimeString(),
				"] WriteUInt64(0x",
				Address.ToString("X8"),
				", new ulong[] { "
			}));
			foreach (ulong num in Value)
			{
				File.AppendAllText(JRPC.logfile, num + ", ");
			}
			File.AppendAllText(JRPC.logfile, " });" + Environment.NewLine);
			byte[] array = new byte[Value.Length * 8];
			for (int j = 0; j < Value.Length; j++)
			{
				BitConverter.GetBytes(Value[j]).CopyTo(array, j * 8);
			}
			JRPC.ReverseBytes(array, 8);
			console.SetMemory(Address, array);
		}

		public static uint XamGetCurrentTitleId(this IXboxConsole console)
		{
			File.AppendAllText(JRPC.logfile, "[JRPC][" + DateTime.Now.ToShortTimeString() + "] > XamGetCurrentTitleId();" + Environment.NewLine);
			string command = "consolefeatures ver=" + JRPC.JRPCVersion + " type=16 params=\"A\\0\\A\\0\\\"";
			string text = JRPC.SendCommand(console, command);
			return uint.Parse(text.Substring(text.find(" ") + 1), NumberStyles.HexNumber);
		}

		public static string XboxIP(this IXboxConsole console)
		{
			File.AppendAllText(JRPC.logfile, "[JRPC][" + DateTime.Now.ToShortTimeString() + "] > XboxIP();" + Environment.NewLine);
			byte[] bytes = BitConverter.GetBytes(console.IPAddress);
			Array.Reverse(bytes);
			return new IPAddress(bytes).ToString();
		}

		public static void XNotify(this IXboxConsole console, string Text)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new string[]
			{
				"[JRPC][",
				DateTime.Now.ToShortTimeString(),
				"] > XNotify(",
				Text,
				");",
				Environment.NewLine
			}));
			console.XNotify(Text, 34U);
		}

		// Token: 0x0600005A RID: 90 RVA: 0x0000676C File Offset: 0x0000496C
		public static void XNotify(this IXboxConsole console, string Text, uint Type)
		{
			File.AppendAllText(JRPC.logfile, string.Concat(new object[]
			{
				"[JRPC][",
				DateTime.Now.ToShortTimeString(),
				"] > XNotify(",
				Text,
				", ",
				Type,
				");",
				Environment.NewLine
			}));
			string command = string.Concat(new object[]
			{
				"consolefeatures ver=",
				JRPC.JRPCVersion,
				" type=12 params=\"A\\0\\A\\2\\",
				JRPC.String,
				"/",
				Text.Length,
				"\\",
				Text.ToHexString(),
				"\\",
				JRPC.Int,
				"\\",
				Type,
				"\\\""
			});
			JRPC.SendCommand(console, command);
		}

		private static readonly uint Byte = 4U;
		private static readonly uint ByteArray = 7U;
		private static uint connectionId;
		private static readonly uint Float = 3U;
		private static readonly uint FloatArray = 6U;
		private static readonly uint Int = 1U;
		private static readonly uint IntArray = 5U;
		public static readonly uint JRPCVersion = 2U;
		private static readonly uint String = 2U;
		private static Dictionary<Type, int> StructPrimitiveSizeMap;
		private static readonly uint Uint64 = 8U;
		private static readonly uint Uint64Array = 9U;
		private static HashSet<Type> ValidReturnTypes;
		private static Dictionary<Type, int> ValueTypeSizeMap;
		private static readonly uint Void = 0U;
		private static string logfile = "./logs/jrpc.log";

		public enum LEDState
		{
			GREEN = 128,
			OFF = 0,
			ORANGE = 136,
			RED = 8
		}

		public enum TemperatureType
		{
			CPU,
			GPU,
			EDRAM,
			MotherBoard
		}

		public enum ThreadType
		{
			System,
			Title
		}
	}
}

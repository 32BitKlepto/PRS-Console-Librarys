using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using XDevkit;

namespace Xbox_USB_Utility
{
    public class XRPC
	{

		public string xrpclogfile = "./logs/xrpc.log";
		public uint Call(uint address, params object[] arg)
		{

			long[] array = new long[9];
			bool flag = !activeConnection;
			File.AppendAllText(xrpclogfile, "[XRPC]" + "[" + DateTime.Now.ToString("hh:mm:ss tt") + "] > " + "CHECKING THE ACTIVE CONNECTION" + Environment.NewLine);
			if (flag)
			{
				Connect();
				File.AppendAllText(xrpclogfile, "[XRPC]" + "[" + DateTime.Now.ToString("hh:mm:ss tt") + "] > " + "CONNECTION IS GOOD!" + Environment.NewLine);
			}
			bool flag2 = XRPC.firstRan == 0;
			if (flag2)
			{
				byte[] array2 = new byte[4];
				xbCon.DebugTarget.GetMemory(2445314222U, 4U, array2, out XRPC.meh);
				xbCon.DebugTarget.InvalidateMemoryCache(true, 2445314222U, 4U);
				Array.Reverse(array2);
				bufferAddress = BitConverter.ToUInt32(array2, 0);
				XRPC.firstRan = 1;
				stringPointer = bufferAddress + 1500U;
				floatPointer = bufferAddress + 2700U;
				bytePointer = bufferAddress + 3200U;
				xbCon.DebugTarget.SetMemory(bufferAddress, 100U, nulled, out XRPC.meh);
				xbCon.DebugTarget.SetMemory(stringPointer, 100U, nulled, out XRPC.meh);
			}
			bool flag3 = bufferAddress == 0U;
			if (flag3)
			{
				byte[] array3 = new byte[4];
				xbCon.DebugTarget.GetMemory(2445314222U, 4U, array3, out XRPC.meh);
				xbCon.DebugTarget.InvalidateMemoryCache(true, 2445314222U, 4U);
				Array.Reverse(array3);
				bufferAddress = BitConverter.ToUInt32(array3, 0);
			}
			stringPointer = bufferAddress + 1500U;
			floatPointer = bufferAddress + 2700U;
			bytePointer = bufferAddress + 3200U;
			int num = 0;
			int num2 = 0;
			foreach (object obj in arg)
			{
				bool flag4 = obj is byte;
				if (flag4)
				{
					byte[] value = (byte[])obj;
					array[num2] = (long)((ulong)BitConverter.ToUInt32(value, 0));
				}
				else
				{
					bool flag5 = obj is byte[];
					if (flag5)
					{
						byte[] array4 = (byte[])obj;
						xbCon.DebugTarget.SetMemory(bytePointer, (uint)array4.Length, array4, out XRPC.meh);
						array[num2] = (long)((ulong)bytePointer);
						bytePointer += (uint)(array4.Length + 2);
					}
					else
					{
						bool flag6 = obj is float;
						if (flag6)
						{
							byte[] bytes = BitConverter.GetBytes(float.Parse(Convert.ToString(obj)));
							xbCon.DebugTarget.SetMemory(floatPointer, (uint)bytes.Length, bytes, out XRPC.meh);
							array[num2] = (long)((ulong)floatPointer);
							floatPointer += (uint)(bytes.Length + 2);
						}
						else
						{
							bool flag7 = obj is float[];
							if (flag7)
							{
								byte[] array5 = new byte[12];
								for (int j = 0; j <= 2; j++)
								{
									byte[] array6 = new byte[4];
									Buffer.BlockCopy((Array)obj, j * 4, array6, 0, 4);
									Array.Reverse(array6);
									Buffer.BlockCopy(array6, 0, array5, 4 * j, 4);
								}
								xbCon.DebugTarget.SetMemory(floatPointer, (uint)array5.Length, array5, out XRPC.meh);
								array[num2] = (long)((ulong)floatPointer);
								floatPointer += 2U;
							}
							else
							{
								bool flag8 = obj is string;
								if (flag8)
								{
									byte[] bytes2 = Encoding.ASCII.GetBytes(Convert.ToString(obj));
									xbCon.DebugTarget.SetMemory(stringPointer, (uint)bytes2.Length, bytes2, out XRPC.meh);
									array[num2] = (long)((ulong)stringPointer);
									string text = Convert.ToString(obj);
									stringPointer += (uint)(text.Length + 1);
								}
								else
								{
									array[num2] = Convert.ToInt64(obj);
								}
							}
						}
					}
				}
				num++;
				num2++;
			}
			byte[] data = XRPC.getData(array);
			xbCon.DebugTarget.SetMemory(bufferAddress + 8U, (uint)data.Length, data, out XRPC.meh);
			byte[] bytes3 = BitConverter.GetBytes(num);
			Array.Reverse(bytes3);
			xbCon.DebugTarget.SetMemory(bufferAddress + 4U, 4U, bytes3, out XRPC.meh);
			Thread.Sleep(0);
			byte[] bytes4 = BitConverter.GetBytes(address);
			Array.Reverse(bytes4);
			xbCon.DebugTarget.SetMemory(bufferAddress, 4U, bytes4, out XRPC.meh);
			Thread.Sleep(50);
			byte[] array7 = new byte[4];
			xbCon.DebugTarget.GetMemory(bufferAddress + 4092U, 4U, array7, out XRPC.meh);
			xbCon.DebugTarget.InvalidateMemoryCache(true, bufferAddress + 4092U, 4U);
			Array.Reverse(array7);
			return BitConverter.ToUInt32(array7, 0);
			File.AppendAllText(xrpclogfile, "[XRPC]" + "[" + DateTime.Now.ToString("hh:mm:ss tt") + "] > " + "ADDRESS MANIPULATED" + Environment.NewLine);
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002550 File Offset: 0x00000750
		public uint CallSysFunction(uint address, params object[] arg)
		{
			long[] array = new long[9];
			bool flag = !activeConnection;
			if (flag)
			{
				Connect();
			}
			bool flag2 = XRPC.firstRan == 0;
			if (flag2)
			{
				byte[] array2 = new byte[4];
				xbCon.DebugTarget.GetMemory(2445314222U, 4U, array2, out XRPC.meh);
				xbCon.DebugTarget.InvalidateMemoryCache(true, 2445314222U, 4U);
				Array.Reverse(array2);
				bufferAddress = BitConverter.ToUInt32(array2, 0);
				XRPC.firstRan = 1;
				stringPointer = bufferAddress + 1500U;
				floatPointer = bufferAddress + 2700U;
				bytePointer = bufferAddress + 3200U;
				xbCon.DebugTarget.SetMemory(bufferAddress, 100U, nulled, out XRPC.meh);
				xbCon.DebugTarget.SetMemory(stringPointer, 100U, nulled, out XRPC.meh);
			}
			bool flag3 = bufferAddress == 0U;
			if (flag3)
			{
				byte[] array3 = new byte[4];
				xbCon.DebugTarget.GetMemory(2445314222U, 4U, array3, out XRPC.meh);
				xbCon.DebugTarget.InvalidateMemoryCache(true, 2445314222U, 4U);
				Array.Reverse(array3);
				bufferAddress = BitConverter.ToUInt32(array3, 0);
			}
			stringPointer = bufferAddress + 1500U;
			floatPointer = bufferAddress + 2700U;
			bytePointer = bufferAddress + 3200U;
			int num = 0;
			int num2 = 0;
			array[num2] = (long)((ulong)address);
			num2++;
			foreach (object obj in arg)
			{
				bool flag4 = obj is byte;
				if (flag4)
				{
					byte[] value = (byte[])obj;
					array[num2] = (long)((ulong)BitConverter.ToUInt32(value, 0));
				}
				else
				{
					bool flag5 = obj is byte[];
					if (flag5)
					{
						byte[] array4 = (byte[])obj;
						xbCon.DebugTarget.SetMemory(bytePointer, (uint)array4.Length, array4, out XRPC.meh);
						array[num2] = (long)((ulong)bytePointer);
						bytePointer += (uint)(array4.Length + 2);
					}
					else
					{
						bool flag6 = obj is float;
						if (flag6)
						{
							byte[] bytes = BitConverter.GetBytes(float.Parse(Convert.ToString(obj)));
							xbCon.DebugTarget.SetMemory(floatPointer, (uint)bytes.Length, bytes, out XRPC.meh);
							array[num2] = (long)((ulong)floatPointer);
							floatPointer += (uint)(bytes.Length + 2);
						}
						else
						{
							bool flag7 = obj is float[];
							if (flag7)
							{
								byte[] array5 = new byte[12];
								for (int j = 0; j <= 2; j++)
								{
									byte[] array6 = new byte[4];
									Buffer.BlockCopy((Array)obj, j * 4, array6, 0, 4);
									Array.Reverse(array6);
									Buffer.BlockCopy(array6, 0, array5, 4 * j, 4);
								}
								xbCon.DebugTarget.SetMemory(floatPointer, (uint)array5.Length, array5, out XRPC.meh);
								array[num2] = (long)((ulong)floatPointer);
								floatPointer += 2U;
							}
							else
							{
								bool flag8 = obj is string;
								if (flag8)
								{
									byte[] bytes2 = Encoding.ASCII.GetBytes(Convert.ToString(obj));
									xbCon.DebugTarget.SetMemory(stringPointer, (uint)bytes2.Length, bytes2, out XRPC.meh);
									array[num2] = (long)((ulong)stringPointer);
									string text = Convert.ToString(obj);
									stringPointer += (uint)(text.Length + 1);
								}
								else
								{
									array[num2] = Convert.ToInt64(obj);
								}
							}
						}
					}
				}
				num++;
				num2++;
			}
			byte[] data = XRPC.getData(array);
			xbCon.DebugTarget.SetMemory(bufferAddress + 8U, (uint)data.Length, data, out XRPC.meh);
			byte[] bytes3 = BitConverter.GetBytes(num);
			Array.Reverse(bytes3);
			xbCon.DebugTarget.SetMemory(bufferAddress + 4U, 4U, bytes3, out XRPC.meh);
			Thread.Sleep(0);
			byte[] bytes4 = BitConverter.GetBytes(2181038080U);
			Array.Reverse(bytes4);
			xbCon.DebugTarget.SetMemory(bufferAddress, 4U, bytes4, out XRPC.meh);
			Thread.Sleep(50);
			byte[] array7 = new byte[4];
			xbCon.DebugTarget.GetMemory(bufferAddress + 4092U, 4U, array7, out XRPC.meh);
			xbCon.DebugTarget.InvalidateMemoryCache(true, bufferAddress + 4092U, 4U);
			Array.Reverse(array7);
			return BitConverter.ToUInt32(array7, 0);
		}

		public void Connect()
		{
			File.AppendAllText(xrpclogfile, "[XRPC]" + "[" + DateTime.Now.ToString("hh:mm:ss tt") + "] > " + "ATTEMPTING TO CONNECT TO XRPC CLIENT" + Environment.NewLine);
			initialize();
			bool flag = activeConnection && sa == 0;
			if (flag)
            {
				//Successful Connection
				Form1.xrpcconnected = true;
                sa = 1;
			}
		}

		private static byte[] getData(long[] argument)
		{
			byte[] array = new byte[argument.Length * 8];
			int num = 0;
			foreach (long value in argument)
			{
				byte[] bytes = BitConverter.GetBytes(value);
				Array.Reverse(bytes);
				bytes.CopyTo(array, num);
				num += 8;
			}
			return array;
		}

		public byte[] GetMemory(uint address, uint length)
		{
			byte[] array = new byte[length];
			File.AppendAllText(xrpclogfile, "[XRPC]" + "[" + DateTime.Now.ToString("hh:mm:ss tt") + "] > " + "GETTING MEMORY" + Environment.NewLine);
			xbCon.DebugTarget.GetMemory(address, length, array, out XRPC.g);
			xbCon.DebugTarget.InvalidateMemoryCache(true, address, length);
			File.AppendAllText(xrpclogfile, "[XRPC]" + "[" + DateTime.Now.ToString("hh:mm:ss tt") + "] > " + "RETURN STATUS 1" + Environment.NewLine);
			return array;
		}

		public void initialize()
		{
			bool flag = !activeConnection;
			File.AppendAllText(xrpclogfile, "[XRPC]" + "[" + DateTime.Now.ToString("hh:mm:ss tt") + "] > " + "CHECKING FOR XRPC CONNECTION" + Environment.NewLine);
			if (flag)
			{
				File.AppendAllText(xrpclogfile, "[XRPC]" + "[" + DateTime.Now.ToString("hh:mm:ss tt") + "] > " + "CLIENT FOUND, ESTABLISHING STABLE CONNECTION" + Environment.NewLine);
				xboxMgr = (XboxManager)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("A5EB45D8-F3B6-49B9-984A-0D313AB60342")));
				xbCon = xboxMgr.OpenConsole(xboxMgr.DefaultConsole);
				try
				{
					File.AppendAllText(xrpclogfile, "[XRPC]" + "[" + DateTime.Now.ToString("hh:mm:ss tt") + "] > " + "CONNECTED TO XRPC CLIENT ON DEFAULT CONSOLE" + Environment.NewLine);
					xbConnection = xbCon.OpenConnection(null);
				}
				catch (Exception)
				{
					return;
				}
				string text;
				string text2;
				bool flag2 = xbCon.DebugTarget.IsDebuggerConnected(out text, out text2);
				if (flag2)
				{
					activeConnection = true;
				}
				else
				{
					xbCon.DebugTarget.ConnectAsDebugger("XRPC", XboxDebugConnectFlags.Force);
					bool flag3 = xbCon.DebugTarget.IsDebuggerConnected(out text, out text2);
					if (flag3)
					{
						activeConnection = true;
					}
				}
			}
			else
			{
				File.AppendAllText(xrpclogfile, "[XRPC]" + "[" + DateTime.Now.ToString("hh:mm:ss tt") + "] > " + "NO ACTIVE XRPC CONNECTION FOUND, IS THE DEFAULT CONSOLE SET?" + Environment.NewLine);
				string text;
				string text2;
				bool flag4 = !xbCon.DebugTarget.IsDebuggerConnected(out text, out text2);
				if (flag4)
				{
					activeConnection = false;
					Connect();
				}
			}
		}

		public uint ResolveFunction(string titleID, uint ord)
		{
			bool flag = XRPC.firstRan == 0;
			if (flag)
			{
				byte[] array = new byte[4];
				xbCon.DebugTarget.GetMemory(2445314222U, 4U, array, out XRPC.meh);
				xbCon.DebugTarget.InvalidateMemoryCache(true, 2445314222U, 4U);
				Array.Reverse(array);
				bufferAddress = BitConverter.ToUInt32(array, 0);
				XRPC.firstRan = 1;
				stringPointer = bufferAddress + 1500U;
				floatPointer = bufferAddress + 2700U;
				bytePointer = bufferAddress + 3200U;
				xbCon.DebugTarget.SetMemory(bufferAddress, 100U, nulled, out XRPC.meh);
				xbCon.DebugTarget.SetMemory(stringPointer, 100U, nulled, out XRPC.meh);
			}
			byte[] bytes = Encoding.ASCII.GetBytes(titleID);
			xbCon.DebugTarget.SetMemory(stringPointer, (uint)bytes.Length, bytes, out XRPC.meh);
			long[] array2 = new long[2];
			array2[0] = (long)((ulong)stringPointer);
			string text = Convert.ToString(titleID);
			stringPointer += (uint)(text.Length + 1);
			array2[1] = (long)((ulong)ord);
			byte[] data = XRPC.getData(array2);
			xbCon.DebugTarget.SetMemory(bufferAddress + 8U, (uint)data.Length, data, out XRPC.meh);
			byte[] bytes2 = BitConverter.GetBytes(2181038081U);
			Array.Reverse(bytes2);
			xbCon.DebugTarget.SetMemory(bufferAddress, 4U, bytes2, out XRPC.meh);
			Thread.Sleep(50);
			byte[] array3 = new byte[4];
			xbCon.DebugTarget.GetMemory(bufferAddress + 4092U, 4U, array3, out XRPC.meh);
			xbCon.DebugTarget.InvalidateMemoryCache(true, bufferAddress + 4092U, 4U);
			Array.Reverse(array3);
			return BitConverter.ToUInt32(array3, 0);
		}

		public void SetMemory(uint address, byte[] data)
		{
			xbCon.DebugTarget.SetMemory(address, (uint)data.Length, data, out XRPC.g);
			File.AppendAllText(xrpclogfile, "[XRPC]" + "[" + DateTime.Now.ToString("hh:mm:ss tt") + "] > " + "SET MEMORY, VALUES: (" + address + data.Length.ToString() + ")" + Environment.NewLine);
		}

		public uint SystemCall(params object[] arg)
		{
			long[] array = new long[9];
			bool flag = !activeConnection;
			if (flag)
			{
				Connect();
			}
			bool flag2 = XRPC.firstRan == 0;
			if (flag2)
			{
				byte[] array2 = new byte[4];
				xbCon.DebugTarget.GetMemory(2445314222U, 4U, array2, out XRPC.meh);
				xbCon.DebugTarget.InvalidateMemoryCache(true, 2445314222U, 4U);
				Array.Reverse(array2);
				bufferAddress = BitConverter.ToUInt32(array2, 0);
				XRPC.firstRan = 1;
				stringPointer = bufferAddress + 1500U;
				floatPointer = bufferAddress + 2700U;
				bytePointer = bufferAddress + 3200U;
				xbCon.DebugTarget.SetMemory(bufferAddress, 100U, nulled, out XRPC.meh);
				xbCon.DebugTarget.SetMemory(stringPointer, 100U, nulled, out XRPC.meh);
			}
			bool flag3 = bufferAddress == 0U;
			if (flag3)
			{
				byte[] array3 = new byte[4];
				xbCon.DebugTarget.GetMemory(2445314222U, 4U, array3, out XRPC.meh);
				xbCon.DebugTarget.InvalidateMemoryCache(true, 2445314222U, 4U);
				Array.Reverse(array3);
				bufferAddress = BitConverter.ToUInt32(array3, 0);
			}
			stringPointer = bufferAddress + 1500U;
			floatPointer = bufferAddress + 2700U;
			bytePointer = bufferAddress + 3200U;
			int num = 0;
			int num2 = 0;
			foreach (object obj in arg)
			{
				bool flag4 = obj is byte;
				if (flag4)
				{
					byte[] value = (byte[])obj;
					array[num2] = (long)((ulong)BitConverter.ToUInt32(value, 0));
				}
				else
				{
					bool flag5 = obj is byte[];
					if (flag5)
					{
						byte[] array4 = (byte[])obj;
						xbCon.DebugTarget.SetMemory(bytePointer, (uint)array4.Length, array4, out XRPC.meh);
						array[num2] = (long)((ulong)bytePointer);
						bytePointer += (uint)(array4.Length + 2);
					}
					else
					{
						bool flag6 = obj is float;
						if (flag6)
						{
							byte[] bytes = BitConverter.GetBytes(float.Parse(Convert.ToString(obj)));
							xbCon.DebugTarget.SetMemory(floatPointer, (uint)bytes.Length, bytes, out XRPC.meh);
							array[num2] = (long)((ulong)floatPointer);
							floatPointer += (uint)(bytes.Length + 2);
						}
						else
						{
							bool flag7 = obj is float[];
							if (flag7)
							{
								byte[] array5 = new byte[12];
								for (int j = 0; j <= 2; j++)
								{
									byte[] array6 = new byte[4];
									Buffer.BlockCopy((Array)obj, j * 4, array6, 0, 4);
									Array.Reverse(array6);
									Buffer.BlockCopy(array6, 0, array5, 4 * j, 4);
								}
								xbCon.DebugTarget.SetMemory(floatPointer, (uint)array5.Length, array5, out XRPC.meh);
								array[num2] = (long)((ulong)floatPointer);
								floatPointer += 2U;
							}
							else
							{
								bool flag8 = obj is string;
								if (flag8)
								{
									byte[] bytes2 = Encoding.ASCII.GetBytes(Convert.ToString(obj));
									xbCon.DebugTarget.SetMemory(stringPointer, (uint)bytes2.Length, bytes2, out XRPC.meh);
									array[num2] = (long)((ulong)stringPointer);
									string text = Convert.ToString(obj);
									stringPointer += (uint)(text.Length + 1);
								}
								else
								{
									array[num2] = Convert.ToInt64(obj);
								}
							}
						}
					}
				}
				num++;
				num2++;
			}
			byte[] data = XRPC.getData(array);
			xbCon.DebugTarget.SetMemory(bufferAddress + 8U, (uint)data.Length, data, out XRPC.meh);
			byte[] bytes3 = BitConverter.GetBytes(num);
			Array.Reverse(bytes3);
			xbCon.DebugTarget.SetMemory(bufferAddress + 4U, 4U, bytes3, out XRPC.meh);
			Thread.Sleep(0);
			byte[] bytes4 = BitConverter.GetBytes(2181038080U);
			Array.Reverse(bytes4);
			xbCon.DebugTarget.SetMemory(bufferAddress, 4U, bytes4, out XRPC.meh);
			Thread.Sleep(50);
			byte[] array7 = new byte[4];
			xbCon.DebugTarget.GetMemory(bufferAddress + 4092U, 4U, array7, out XRPC.meh);
			xbCon.DebugTarget.InvalidateMemoryCache(true, bufferAddress + 4092U, 4U);
			Array.Reverse(array7);
			return BitConverter.ToUInt32(array7, 0);
		}

		private float[] toFloatArray(double[] arr)
		{
			bool flag = arr == null;
			float[] result;
			if (flag)
			{
				result = null;
			}
			else
			{
				int num = arr.Length;
				float[] array = new float[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = (float)arr[i];
				}
				result = array;
			}
			return result;
		}

		public byte[] WideChar(string text)
		{
			byte[] array = new byte[text.Length * 2 + 2];
			int num = 1;
			array[0] = 0;
			foreach (char value in text)
			{
				array[num] = Convert.ToByte(value);
				num += 2;
			}
			return array;
		}

		public bool activeConnection;
		public bool activeTransfer;
		private uint[] buffcheck = new uint[15];
		private uint bufferAddress;
		private uint bufferAddressRead = 2445323722U;
		private uint bytePointer;
		private string currentVersion = "1.7";
		private static int firstRan;
		private uint floatPointer;
		public static uint g;
		private static uint meh;
		private int multiple;
		private byte[] nulled = new byte[100];
		private int sa;
		private uint stringPointer;
		public IXboxConsole xbCon;
		private uint xbConnection;

		public IXboxManager xboxMgr;

		public struct wChar
		{
			public string Text;
		}

		public enum XNotiyLogo
		{
			ACHIEVEMENT_UNLOCKED = 27,
			ACHIEVEMENTS_UNLOCKED = 39,
			AVATAR_AWARD_UNLOCKED = 60,
			BLANK = 42,
			CANT_CONNECT_XBL_PARTY = 56,
			CANT_DOWNLOAD_X = 32,
			CANT_SIGN_IN_MESSENGER = 43,
			DEVICE_FULL = 36,
			DISCONNECTED_FROM_XBOX_LIVE = 11,
			DISCONNECTED_XBOX_LIVE_11_MINUTES_REMAINING = 46,
			DISCONNECTED_XBOX_LIVE_PARTY = 54,
			DOWNLOAD = 12,
			DOWNLOAD_STOPPED_FOR_X = 33,
			DOWNLOADED = 55,
			FAMILY_TIMER_X_TIME_REMAINING = 45,
			FLASH_LOGO = 23,
			FLASHING_CHAT_ICON = 38,
			FLASHING_CHAT_SYMBOL = 65,
			FLASHING_DOUBLE_SIDED_HAMMER = 16,
			FLASHING_FROWNING_FACE = 15,
			FLASHING_HAPPY_FACE = 14,
			FLASHING_MUSIC_SYMBOL = 13,
			FLASHING_XBOX_CONSOLE = 34,
			FLASHING_XBOX_LOGO = 4,
			FOUR_2 = 25,
			FOUR_3,
			FOUR_5 = 48,
			FOUR_7 = 37,
			FOUR_9 = 28,
			FRIEND_REQUEST_LOGO = 2,
			GAME_INVITE_SENT = 22,
			GAME_INVITE_SENT_TO_XBOX_LIVE_PARTY = 51,
			GAMER_PICTURE_UNLOCKED = 59,
			// Token: 0x0400006C RID: 108
			GAMERTAG_HAS_JOINED_CHAT = 20,
			// Token: 0x0400006D RID: 109
			GAMERTAG_HAS_JOINED_XBL_PARTY = 57,
			// Token: 0x0400006E RID: 110
			GAMERTAG_HAS_LEFT_CHAT = 21,
			// Token: 0x0400006F RID: 111
			GAMERTAG_HAS_LEFT_XBL_PARTY = 58,
			// Token: 0x04000070 RID: 112
			GAMERTAG_SENT_YOU_A_MESSAGE = 5,
			// Token: 0x04000071 RID: 113
			GAMERTAG_SIGNED_IN_OFFLINE = 9,
			// Token: 0x04000072 RID: 114
			GAMERTAG_SIGNED_INTO_XBOX_LIVE = 8,
			// Token: 0x04000073 RID: 115
			GAMERTAG_SIGNEDIN = 7,
			// Token: 0x04000074 RID: 116
			GAMERTAG_SINGED_OUT = 6,
			// Token: 0x04000075 RID: 117
			GAMERTAG_WANTS_TO_CHAT = 10,
			// Token: 0x04000076 RID: 118
			GAMERTAG_WANTS_TO_CHAT_2 = 17,
			// Token: 0x04000077 RID: 119
			GAMERTAG_WANTS_TO_TALK_IN_VIDEO_KINECT = 29,
			// Token: 0x04000078 RID: 120
			GAMERTAG_WANTS_YOU_TO_JOIN_AN_XBOX_LIVE_PARTY = 49,
			// Token: 0x04000079 RID: 121
			JOINED_XBL_PARTY = 61,
			// Token: 0x0400007A RID: 122
			KICKED_FROM_XBOX_LIVE_PARTY = 52,
			// Token: 0x0400007B RID: 123
			KINECT_HEALTH_EFFECTS = 47,
			// Token: 0x0400007C RID: 124
			MESSENGER_DISCONNECTED = 41,
			// Token: 0x0400007D RID: 125
			MISSED_MESSENGER_CONVERSATION = 44,
			// Token: 0x0400007E RID: 126
			NEW_MESSAGE = 3,
			// Token: 0x0400007F RID: 127
			NEW_MESSAGE_LOGO = 1,
			// Token: 0x04000080 RID: 128
			NULLED = 53,
			// Token: 0x04000081 RID: 129
			PAGE_SENT_TO = 24,
			// Token: 0x04000082 RID: 130
			PARTY_INVITE_SENT = 50,
			// Token: 0x04000083 RID: 131
			PLAYER_MUTED = 63,
			// Token: 0x04000084 RID: 132
			PLAYER_UNMUTED,
			// Token: 0x04000085 RID: 133
			PLEASE_RECONNECT_CONTROLLERM = 19,
			// Token: 0x04000086 RID: 134
			PLEASE_REINSERT_MEMORY_UNIT = 18,
			// Token: 0x04000087 RID: 135
			PLEASE_REINSERT_USB_STORAGE_DEVICE = 62,
			// Token: 0x04000088 RID: 136
			READY_TO_PLAY = 31,
			// Token: 0x04000089 RID: 137
			UPDATING = 76,
			// Token: 0x0400008A RID: 138
			VIDEO_CHAT_INVITE_SENT = 30,
			// Token: 0x0400008B RID: 139
			X_HAS_SENT_YOU_A_NUDGE = 40,
			// Token: 0x0400008C RID: 140
			X_SENT_YOU_A_GAME_MESSAGE = 35,
			// Token: 0x0400008D RID: 141
			XBOX_LOGO = 0
		}
	}
}

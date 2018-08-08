using System;
using System.Collections;
using System.Collections.Generic;

public class NetBuff
{
	private byte[] buffer = new byte[1024];
	private int dataLength;


	///读和写的指针
	private int pointer;

	public byte[] Buffer
	{
		get
		{
			return buffer;
		}
	}

	public int DataLength
	{
		get
		{
			return dataLength;
		}
	}

	public byte[] DataBuffer
	{
		get
		{
			byte[] b = new byte[dataLength + 4];
			Array.Copy(buffer, 0, b, 4, dataLength);
			//var ddd = BitConverter.GetBytes(dataLength);
			Array.Copy(BitConverter.GetBytes(dataLength), 0, b, 0, 4);
			//Console.WriteLine("dataLength {0}, bufferLenght {1}, {2}", dataLength, b.Length, ddd.Length);
			return b;
		}
	}

	public void Set(byte[] b, int count)
	{
		pointer = 0;
		dataLength = count;
		b.CopyTo(buffer, 0);
	}

	public void Set(byte[] b, int startIndex, int count)
	{
		pointer = 0;
		dataLength = count;
		Array.Copy(b, startIndex, buffer, 0, count);
	}

	public void Reset()
	{
		pointer = 0;
		dataLength = 0;
	}

	public int ReadInt()
	{
		int n = BitConverter.ToInt32(buffer, pointer);
		pointer += 4;
		return n;
	}

	public void WriteInt(int n)
	{
		var bytes = BitConverter.GetBytes(n);
		for(int i = 0; i < bytes.Length; i++)
		{
			buffer[pointer + i] = bytes[i];
		}

		pointer += bytes.Length;
		dataLength = pointer;
	}


}

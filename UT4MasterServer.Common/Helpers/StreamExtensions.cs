using System.Buffers;
using System.Text;
using System;

namespace UT4MasterServer.Common.Helpers;

public static class StreamExtensions
{
	public static async Task<string> ReadAsStringAsync(this Stream stream, int maxByteLength)
	{
		return Encoding.UTF8.GetString(await stream.ReadAsBytesAsync(maxByteLength));
	}

	public static async Task<byte[]> ReadAsBytesAsync(this Stream stream, int maxByteLength)
	{
		var bytes = ArrayPool<byte>.Shared.Rent(maxByteLength);
		int fillCount = 0;

		while (true)
		{
			int readCount = await stream.ReadAsync(bytes.AsMemory(fillCount, bytes.Length - fillCount));

			if (readCount <= 0)
			{
				// EOF reached
				break;
			}

			fillCount += readCount;
		}

		byte[] ret = new byte[fillCount];
		Array.Copy(bytes, ret, fillCount);
		ArrayPool<byte>.Shared.Return(bytes);

		return ret;
	}
}

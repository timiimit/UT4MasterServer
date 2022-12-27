

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace UT4MasterServer;

public static class PipeReaderExtensions
{
	public static async Task<string> ReadAsStringAsync(this PipeReader pipe, int maxByteLength)
	{
		return Encoding.UTF8.GetString(await ReadAsBytesAsync(pipe, maxByteLength));
	}

	public static async Task<byte[]> ReadAsBytesAsync(this PipeReader pipe, int maxByteLength)
	{
		var rawArray = ArrayPool<byte>.Shared.Rent(maxByteLength);
		var rawMemory = new Memory<byte>(rawArray);
		int fillCount = 0;

		while (true)
		{
			ReadResult readResult = await pipe.ReadAsync();
			var buffer = readResult.Buffer;
			bool isEOF = false;

			if (readResult.IsCompleted || buffer.Length >= maxByteLength)
			{
				fillCount = (int)buffer.Length;
				buffer.CopyTo(rawMemory.Span.Slice(0, fillCount));
				isEOF = true;
			}

			pipe.AdvanceTo(buffer.Start, buffer.End);

			if (isEOF)
			{
				break;
			}
		}

		byte[] ret = new byte[fillCount];
		Array.Copy(rawArray, ret, fillCount);
		ArrayPool<byte>.Shared.Return(rawArray);


		return ret;
	}
}
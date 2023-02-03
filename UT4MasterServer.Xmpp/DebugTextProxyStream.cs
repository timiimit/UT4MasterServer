using System.Text;

namespace UT4MasterServer.Xmpp;

internal class DebugTextProxyStream : Stream
{
	public override bool CanRead => InnerStream.CanRead;
	public override bool CanSeek => InnerStream.CanSeek;
	public override bool CanWrite => InnerStream.CanWrite;
	public override long Length => InnerStream.Length;
	public override long Position { get => InnerStream.Position; set => InnerStream.Position = value; }

	public Stream InnerStream { get; private set; }

	private bool leaveOpen;

	public DebugTextProxyStream(Stream innerStream, bool leaveOpen = false)
	{
		InnerStream = innerStream;
		this.leaveOpen = leaveOpen;
	}

	public override void Flush()
	{
		InnerStream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int ret = InnerStream.Read(buffer, offset, count);
		var s = Encoding.UTF8.GetString(buffer, offset, ret);
		lock (InnerStream)
		{
			Console.WriteLine($" [ <- ] {s}");
		}
		return ret;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return InnerStream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		InnerStream.SetLength(value);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		var s = Encoding.UTF8.GetString(buffer, offset, count);
		lock (InnerStream)
		{
			Console.WriteLine($" [ -> ] {s}");
		}
		InnerStream.Write(buffer, offset, count);
	}

	protected override void Dispose(bool disposing)
	{
		if (!leaveOpen)
		{
			InnerStream.Dispose();
		}
		base.Dispose(disposing);
	}
}

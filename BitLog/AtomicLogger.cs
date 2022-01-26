using System.Security.AccessControl;

namespace BitLog;

public class AtomicLogger : IDisposable
{
	private readonly FileStream _stream;

	public AtomicLogger(string filename, int bufferSize = 4096)
	{
		_stream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize, FileOptions.WriteThrough | FileOptions.Asynchronous);
	}

	public async Task WriteDataAsync(byte[] data, CancellationToken token)
	{
		await _stream.WriteAsync(data, token);
		await _stream.FlushAsync(token);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing) 
			_stream.Dispose();
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
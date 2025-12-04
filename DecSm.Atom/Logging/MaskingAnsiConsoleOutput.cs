namespace DecSm.Atom.Logging;

/// <summary>
///     An IAnsiConsoleOutput wrapper that masks secret values before writing to the underlying output.
/// </summary>
internal sealed class MaskingAnsiConsoleOutput : IAnsiConsoleOutput
{
    private readonly IAnsiConsoleOutput _inner;

    public MaskingAnsiConsoleOutput(IAnsiConsoleOutput inner)
    {
        _inner = inner;
        Writer = new MaskingTextWriter(inner.Writer);
    }

    public int Width => _inner.Width;

    public int Height => _inner.Height;

    public TextWriter Writer { get; }

    public bool IsTerminal => _inner.IsTerminal;

    public void SetEncoding(Encoding encoding) =>
        _inner.SetEncoding(encoding);

    private sealed class MaskingTextWriter : TextWriter
    {
        private readonly TextWriter _innerWriter;

        public MaskingTextWriter(TextWriter innerWriter)
        {
            _innerWriter = innerWriter;
        }

        public override Encoding Encoding => _innerWriter.Encoding;

        public override void Write(char value) =>
            Write(new string(value, 1));

        public override void Write(string? value)
        {
            if (value is { Length: > 0 })
            {
                var masker = ServiceStaticAccessor<IParamService>.Service;

                if (masker is not null)
                    value = masker.MaskMatchingSecrets(value);
            }

            _innerWriter.Write(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            var value = new string(buffer, index, count);
            Write(value);
        }

        public override Task WriteAsync(char value) =>
            WriteAsync(new string(value, 1));

        public override Task WriteAsync(string? value)
        {
            if (value is not { Length: > 0 })
                return _innerWriter.WriteAsync(value);

            var masker = ServiceStaticAccessor<IParamService>.Service;

            if (masker is not null)
                value = masker.MaskMatchingSecrets(value);

            return _innerWriter.WriteAsync(value);
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            var value = new string(buffer, index, count);

            return WriteAsync(value);
        }

        public override void Flush() =>
            _innerWriter.Flush();

        public override Task FlushAsync() =>
            _innerWriter.FlushAsync();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _innerWriter.Dispose();

            base.Dispose(disposing);
        }
    }
}

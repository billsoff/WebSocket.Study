using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketService
{
    public sealed class WebSocketSession : IWebSocketSession
    {
        private const int DEFAULT_BUFFER_SIZE = 1024; // 1KB

        private readonly WebSocket _socket;

        private readonly ConcurrentQueue<string> _messages;
        private readonly ArraySegment<byte> _dataBuffer;
        private readonly char[] _charBuffer;

        public WebSocketSession(WebSocket socket, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            _socket = socket;

            _messages = new ConcurrentQueue<string>();
            _dataBuffer = new ArraySegment<byte>(new byte[bufferSize]);
            _charBuffer = new char[Encoding.UTF8.GetMaxByteCount(bufferSize)];

            _ = StartReadAsync();
        }

        public bool IsSessionActive => _socket.State == WebSocketState.Open;

        public bool HasMessages() => !_messages.IsEmpty;

        public async Task<string> ReadMessageAsync(TimeSpan timeout = default(TimeSpan))
        {
            CancellationToken token = timeout > TimeSpan.Zero ? new CancellationTokenSource(timeout).Token : CancellationToken.None;

            do
            {
                if (_messages.TryDequeue(out string message))
                {
                    return message;
                }

                if (!IsSessionActive || token.IsCancellationRequested)
                {
                    return null;
                }

                await Task.Delay(100);
            } while (true);
        }

        public async Task<bool> SendMessageAsync(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);

                await _socket.SendAsync(
                        new ArraySegment<byte>(data),
                        WebSocketMessageType.Text,
                        endOfMessage: true,
                        cancellationToken: CancellationToken.None
                    );

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return false;
            }
        }

        public void Dispose()
        {
            if (IsSessionActive)
            {
                _ = _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
        }

        #region Helper Methods

        private async Task StartReadAsync()
        {
            while (IsSessionActive)
            {
                string message = await ReceiveAsync();

                if (_socket.State == WebSocketState.CloseReceived)
                {
                    await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);

                    break;
                }

                _messages.Enqueue(message);
            }
        }

        private async Task<string> ReceiveAsync()
        {
            Decoder decoder = Encoding.UTF8.GetDecoder();
            StringBuilder buffer = new StringBuilder();
            WebSocketReceiveResult result;

            do
            {
                result = await _socket.ReceiveAsync(_dataBuffer, CancellationToken.None);

                DecodeMessage(
                        decoder,
                        _dataBuffer.Array,
                        result.Count,
                        result.EndOfMessage,
                        buffer
                    );
            } while (!result.EndOfMessage);

            return buffer.ToString();
        }

        private void DecodeMessage(Decoder decoder, byte[] data, int byteCount, bool hasNoMoreData, StringBuilder buffer)
        {
            int index = 0;
            int bytesToRead = byteCount;

            while (bytesToRead != 0)
            {
                decoder.Convert(
                    data,
                    index,
                    bytesToRead,
                    _charBuffer,
                    0,
                    _charBuffer.Length,
                    hasNoMoreData,
                    out int bytesUsed,
                    out int charsUsed,
                    out _
                );

                buffer.Append(_charBuffer, 0, charsUsed);

                index += bytesUsed;
                bytesToRead -= bytesUsed;
            }
        }

        #endregion
    }
}

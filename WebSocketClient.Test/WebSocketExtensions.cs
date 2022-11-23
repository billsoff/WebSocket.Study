using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketClient.Test
{
    public static class WebSocketExtensions
    {
        public static async Task<string> ReadMessageAsync(this WebSocket socket)
        {
            const int BUFFER_SIZE = 1024; // 1KB

            ArraySegment<byte> dataBuffer = new ArraySegment<byte>(new byte[BUFFER_SIZE]);
            char[] charBuffer = new char[BUFFER_SIZE * 2];

            Decoder decoder = Encoding.UTF8.GetDecoder();
            StringBuilder buffer = new StringBuilder();
            WebSocketReceiveResult result;

            do
            {
                result = await socket.ReceiveAsync(dataBuffer, CancellationToken.None);

                DecodeMessage(
                        decoder,
                        dataBuffer.Array,
                        charBuffer,
                        result.Count,
                        result.EndOfMessage,
                        buffer
                    );
            } while (!result.EndOfMessage);

            return buffer.ToString();
        }

        public static async Task SendMessageAsync(this WebSocket socket, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            await socket.SendAsync(
                    new ArraySegment<byte>(data),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: CancellationToken.None
                );
        }

        private static void DecodeMessage(Decoder decoder, byte[] data, char[] charBuffer, int byteCount, bool hasNoMoreData, StringBuilder buffer)
        {
            int index = 0;
            int bytesToRead = byteCount;

            while (bytesToRead != 0)
            {
                decoder.Convert(
                    data,
                    index,
                    bytesToRead,
                    charBuffer,
                    0,
                    charBuffer.Length,
                    hasNoMoreData,
                    out int bytesUsed,
                    out int charsUsed,
                    out _
                );

                buffer.Append(charBuffer, 0, charsUsed);

                index += bytesUsed;
                bytesToRead -= bytesUsed;
            }
        }
    }
}

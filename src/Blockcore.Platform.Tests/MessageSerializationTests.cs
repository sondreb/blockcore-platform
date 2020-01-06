using Blockcore.Platform.Networking;
using Blockcore.Platform.Networking.Handlers;
using Blockcore.Platform.Networking.Messages;
using Blockcore.Platform.Tests.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Blockcore.Platform.Tests
{
    public class MessageSerializationTests
    {
        private readonly ITestOutputHelper output;

        public MessageSerializationTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void ShouldSerializeMessages()
        {
            var msg = new TestMessage()
            {
                CustomInt = 123,
                CustomString = "Hello",
                Endpoint = "0.0.0.0:6000"
            };

            var maps = new MessageMaps();
            maps.AddCommand(MessageTypes.TEST, new Map() { Command = MessageTypes.TEST, MessageType = typeof(TestMessage) });
            maps.AddHandler(MessageTypes.TEST, new TestMessageHandler());

            var serializer = new MessageSerializer(maps);

            var serialized = serializer.Serialize(msg);
            var deserialized = serializer.Deserialize<TestMessage>(serialized);

            Assert.True(msg.Equals(deserialized));

            var stream = new MemoryStream();
            serializer.Serialize(msg, stream);
            stream.Seek(0, SeekOrigin.Begin);
            deserialized = serializer.Deserialize(stream);

            Assert.True(msg.Equals(deserialized));
        }

        [Fact]
        public void CheckParsingMillionSmallMessages()
        {
            var maps = new MessageMaps();
            maps.AddCommand(MessageTypes.TEST, new Map() { Command = MessageTypes.TEST, MessageType = typeof(TestMessage) });
            maps.AddHandler(MessageTypes.TEST, new TestMessageHandler());
            var serializer = new MessageSerializer(maps);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            int count = 1000000;

            byte[][] messagesAsBytes = new byte[count][];
            BaseMessage[] messagesAsObjects = new BaseMessage[count];

            for (int i = 0; i < count; i++)
            {
                var msg = new TestMessage()
                {
                    CustomInt = 123 + i,
                    CustomString = "Hello" + i,
                    Endpoint = "0.0.0.0:" + i
                };

                var serialized = serializer.Serialize(msg);

                messagesAsBytes[i] = serialized;
            }

            watch.Stop();

            output.WriteLine($"Took {watch.ElapsedMilliseconds} ms to serialize.");
            Assert.True(watch.ElapsedMilliseconds < 1500);

            watch.Restart();

            for (int i = 0; i < count; i++)
            {
                var array = messagesAsBytes[i];
                var deserialized = serializer.Deserialize(array);
                messagesAsObjects[i] = deserialized;
            }

            watch.Stop();

            output.WriteLine($"Took {watch.ElapsedMilliseconds} ms to deserialize.");
            Assert.True(watch.ElapsedMilliseconds < 1500);
        }

        [Fact]
        public void CheckParsingMillionLargeMessages()
        {
            var maps = new MessageMaps();
            maps.AddCommand(MessageTypes.CHAT, new Map() { Command = MessageTypes.CHAT, MessageType = typeof(ChatMessage) });
            maps.AddHandler(MessageTypes.CHAT, new MessageMessageTestHandler());
            var serializer = new MessageSerializer(maps);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            int count = 1000000;

            byte[][] messagesAsBytes = new byte[count][];
            BaseMessage[] messagesAsObjects = new BaseMessage[count];

            for (int i = 0; i < count; i++)
            {
                var msg = new ChatMessage()
                {
                    From = "USER1",
                    To = "USER2",
                    Id = "id" + i,
                    RecipientId = i.ToString(),
                    Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed ut dolor varius, aliquam lectus nec, rutrum dolor. Vestibulum faucibus eleifend ante, quis aliquet lacus rhoncus in. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Sed nec sapien condimentum mi tempor porta hendrerit at metus. Nam eu blandit odio. Sed consectetur et justo non condimentum. Quisque in ullamcorper sapien. Fusce eget augue ut ligula gravida porttitor. In iaculis cursus nulla quis ornare."
                };

                var serialized = serializer.Serialize(msg);

                messagesAsBytes[i] = serialized;
            }

            watch.Stop();

            output.WriteLine($"Took {watch.ElapsedMilliseconds} ms to serialize.");
            //Assert.True(watch.ElapsedMilliseconds < 1500);

            watch.Restart();

            for (int i = 0; i < count; i++)
            {
                var array = messagesAsBytes[i];
                var deserialized = serializer.Deserialize(array);
                messagesAsObjects[i] = deserialized;
            }

            watch.Stop();

            output.WriteLine($"Took {watch.ElapsedMilliseconds} ms to deserialize.");
            //Assert.True(watch.ElapsedMilliseconds < 1500);
        }
    }
}

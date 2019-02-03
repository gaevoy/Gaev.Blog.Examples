using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NSubstitute;
using NUnit.Framework;
using PlantUml.Net;
using Serilog;

namespace Gaev.Blog.Examples
{
    public class AcceptanceTests
    {
        [Test]
        public void It_should_send_document_via_ftp_and_succeed()
        {
            // Given
            var plantUmlCode = new StringBuilder();
            IMessageBus bus = new InMemoryMessageBus();
            bus = new PlantUmlDiagramBuilder(bus, NewLogger(plantUmlCode));
            var documentHandler = new DocumentHandler(bus);
            var ftpHandler = new FtpHandler(bus);
            bus
                .Subscribe<CreateDocument>(documentHandler)
                .Subscribe<DocumentDelivered>(documentHandler)
                .Subscribe<DocumentFailed>(documentHandler)
                .Subscribe<SendDocumentToFtp>(ftpHandler);

            // When
            bus.Publish(new CreateDocument {Content = "Send to ftp://test.com please!"});

            // Then
            Console.Write(RenderAsciiDiagram(plantUmlCode));
        }

        [Test]
        public void It_should_send_document_via_ftp_and_fail()
        {
            // Given
            var plantUmlCode = new StringBuilder();
            IMessageBus bus = new InMemoryMessageBus();
            bus = new PlantUmlDiagramBuilder(bus, NewLogger(plantUmlCode));
            var documentHandler = new DocumentHandler(bus);
            var ftpHandler = new FtpHandler(bus);
            bus
                .Subscribe<CreateDocument>(documentHandler)
                .Subscribe<DocumentDelivered>(documentHandler)
                .Subscribe<DocumentFailed>(documentHandler)
                .Subscribe<SendDocumentToFtp>(ftpHandler);

            // When
            bus.Publish(new CreateDocument {Content = "Send to ftp://fail.com"});

            // Then
            Console.Write(RenderAsciiDiagram(plantUmlCode));
        }

        [Test]
        public void It_should_not_send_document()
        {
            // Given
            var plantUmlCode = new StringBuilder();
            IMessageBus bus = new InMemoryMessageBus();
            bus = new PlantUmlDiagramBuilder(bus, NewLogger(plantUmlCode));
            var documentHandler = new DocumentHandler(bus);
            var ftpHandler = new FtpHandler(bus);
            bus
                .Subscribe<CreateDocument>(documentHandler)
                .Subscribe<DocumentDelivered>(documentHandler)
                .Subscribe<DocumentFailed>(documentHandler)
                .Subscribe<SendDocumentToFtp>(ftpHandler);

            // When
            bus.Publish(new CreateDocument {Content = "Send to ..."});

            // Then
            Console.Write(RenderAsciiDiagram(plantUmlCode));
        }

        private ILogger NewLogger(StringBuilder output)
        {
            var logger = Substitute.For<ILogger>();
            logger
                .When(e => e.Debug(Arg.Any<string>()))
                .Do(e => output.AppendLine(e.Arg<string>()));
            return logger;
        }

        private string RenderAsciiDiagram(StringBuilder plantUmlCode)
        {
            var diagram = new RendererFactory()
                .CreateRenderer()
                .Render("participant Queue\n" + plantUmlCode, OutputFormat.Ascii);
            var asciiDiagram = Encoding.UTF8.GetString(diagram);
            return asciiDiagram;
        }

        public class PlantUmlDiagramBuilder : IMessageBus
        {
            private readonly IMessageBus _bus;
            private readonly ILogger _logger;

            public PlantUmlDiagramBuilder(IMessageBus bus, ILogger logger)
            {
                _bus = bus;
                _logger = logger;
            }

            public void Publish(object message)
            {
                var callingType = new StackTrace().GetFrame(1).GetMethod().DeclaringType;
                _logger.Debug($"{callingType.Name} -> Queue\nnote left: {message.GetType().Name}");
                _bus.Publish(message);
            }

            public IMessageBus Subscribe<T>(IHandle<T> handler)
            {
                _bus.Subscribe(new HandlerWrapper<T>(handler, _logger));
                return this;
            }

            private class HandlerWrapper<T> : IHandle<T>
            {
                private readonly IHandle<T> _handler;
                private readonly ILogger _logger;

                public HandlerWrapper(IHandle<T> handler, ILogger logger)
                {
                    _handler = handler;
                    _logger = logger;
                }

                public void Handle(T message)
                {
                    _logger.Debug($"Queue -> {_handler.GetType().Name}\nnote right: {message.GetType().Name}");
                    _handler.Handle(message);
                }
            }
        }

        #region Infrastructure

        public interface IMessageBus
        {
            void Publish(object message);
            IMessageBus Subscribe<T>(IHandle<T> handler);
        }

        public interface IHandle<T>
        {
            void Handle(T message);
        }

        public class InMemoryMessageBus : IMessageBus
        {
            private readonly Dictionary<Type, Action<object>> _handlers = new Dictionary<Type, Action<object>>();

            public void Publish(object message)
            {
                if (_handlers.TryGetValue(message.GetType(), out var handler))
                    handler(message);
            }

            public IMessageBus Subscribe<T>(IHandle<T> handler)
            {
                _handlers[typeof(T)] = message => handler.Handle((T) message);
                return this;
            }
        }

        #endregion

        #region Domain

        public class CreateDocument
        {
            public string Content { get; set; }
        }

        public class DocumentDelivered
        {
        }

        public class DocumentFailed
        {
        }

        public class SendDocumentToFtp
        {
            public string Content { get; set; }
        }

        public class DocumentHandler :
            IHandle<CreateDocument>,
            IHandle<DocumentDelivered>,
            IHandle<DocumentFailed>
        {
            private readonly IMessageBus _bus;

            public DocumentHandler(IMessageBus bus)
            {
                _bus = bus;
            }

            public void Handle(CreateDocument message)
            {
                if (message.Content.Contains("ftp://"))
                    _bus.Publish(new SendDocumentToFtp {Content = message.Content});
            }

            public void Handle(DocumentDelivered message)
            {
            }

            public void Handle(DocumentFailed message)
            {
            }
        }

        public class FtpHandler : IHandle<SendDocumentToFtp>
        {
            private readonly IMessageBus _bus;

            public FtpHandler(IMessageBus bus)
            {
                _bus = bus;
            }

            public void Handle(SendDocumentToFtp message)
            {
                if (message.Content.Contains("ftp://test.com"))
                    _bus.Publish(new DocumentDelivered());
                else
                    _bus.Publish(new DocumentFailed());
            }
        }

        #endregion
    }
}
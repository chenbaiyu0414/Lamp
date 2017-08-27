﻿using System.Net;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Lamp.Network.KcpLib;

namespace Lamp.Network.Server
{
    public abstract class BedRockUdpServer
    {
        private IEventLoopGroup mGroup;
        private IChannel mBootstrapChannel;

        private volatile bool mRunning;
        private readonly SessionManager mSessionManager;

        protected abstract void SessionConnectAccept(Session session);
        protected abstract void SessionConnectRefuse(Session session);
        protected abstract void SessionDisconnected(Session session);
        protected abstract void PacketReceived(IByteBuffer buffer, Session session);

        protected BedRockUdpServer()
        {
            mSessionManager = SessionManager.Create(100);
        }

        public async Task Run()
        {
            if (mRunning)
                return;

            //InternalLoggerFactory.DefaultFactory.AddProvider(new ConsoleLoggerProvider((s, level) => true, false));

            mGroup = new MultithreadEventLoopGroup();

            var bootstrap = new Bootstrap();
            bootstrap
                .Group(mGroup)
                .Channel<SocketDatagramChannel>()
                .Option(ChannelOption.SoBroadcast, true)
                .Handler(new ActionChannelInitializer<SocketDatagramChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;

                    pipeline.AddLast(new BedRockUdpServerHandler(mSessionManager,
                        SessionConnectAccept,
                        SessionDisconnected,
                        PacketReceived,
                        SessionConnectRefuse));
                }));

            mBootstrapChannel = await bootstrap.BindAsync(8686);

            mRunning = true;
        }

        public async Task Stop()
        {
            if (!mRunning)
                return;

            if (mGroup == null || mBootstrapChannel == null)
                return;

            await mBootstrapChannel.CloseAsync();
            await mGroup.ShutdownGracefullyAsync();

            mRunning = false;
        }

    }
}

﻿using Lamp.Network.Server;
using System.Collections.Concurrent;
using System.Net;

namespace Lamp.Network
{
    class SessionManager
    {
        public static SessionManager Instance { get; } = new SessionManager();

        private readonly ConcurrentQueue<int> mSessionIdQueue = new ConcurrentQueue<int>();
        private readonly ConcurrentDictionary<EndPoint, Session> mSessions = new ConcurrentDictionary<EndPoint, Session>();

        private SessionManager()
        {

        }

        /// <summary>
        /// 创建一个<see cref="SessionManager"/>
        /// </summary>
        /// <param name="peerCapcity">连接点容量</param>
        /// <returns></returns>
        public static SessionManager Create(uint peerCapcity)
        {
            var sm = new SessionManager();
            
            for (var sessionId = 0; sessionId <= peerCapcity; sessionId++)
            {
                sm.mSessionIdQueue.Enqueue(sessionId);
            }

            return sm;
        }

        /// <summary>
        /// 获得一个新的会话标识，如果获取失败则返回<see cref="NetworkOperationCode.MAX_CONN_EXCEED"/>
        /// </summary>
        /// <returns></returns>
        public int GetNewSessionId()
        {
            if (mSessionIdQueue.IsEmpty)
            {
                return NetworkOperationCode.MAX_CONN_EXCEED;
            }

            return mSessionIdQueue.TryDequeue(out var newPeerId) ? newPeerId : NetworkOperationCode.MAX_CONN_EXCEED;
        }

        /// <summary>
        /// 回收一个会话标识以备复用
        /// </summary>
        /// <param name="sessionId">会话标识</param>
        public void RecycleSessionId(int sessionId)
        {
            mSessionIdQueue.Enqueue(sessionId);
        }

        public Session FindSession(EndPoint endpoint)
        {
            if(mSessions.TryGetValue(endpoint,out var value))
            {
                return value;
            }

            return null;
        }

        public bool AddSession(EndPoint endpoint, Session session)
        {
            return mSessions.TryAdd(endpoint, session);
        }
    }
}

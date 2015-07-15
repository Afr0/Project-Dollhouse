﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.*/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Linq;
using GonzoNet;
using GonzoNet.Encryption;
using ProtocolAbstractionLibraryD;

namespace TSO_LoginServer.Network
{
    public class CityServerListener : Listener
    {
        public BlockingCollection<CityInfo> CityServers;
		public BlockingCollection<NetworkClient> PotentialLogins;
        private Socket m_ListenerSock;
        private IPEndPoint m_LocalEP;

        public CityServerListener(EncryptionMode EncMode) : base(EncMode)
        {
            m_ListenerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			CityServers = new BlockingCollection<CityInfo>();
			PotentialLogins = new BlockingCollection<NetworkClient>();
        }

        public override void Initialize(IPEndPoint LocalEP)
        {
            m_LocalEP = LocalEP;

            try
            {
                m_ListenerSock.Bind(LocalEP);
                m_ListenerSock.Listen(10000);

                Logger.LogDebug("Started listening on: " + LocalEP.Address.ToString()
                    + ":" + LocalEP.Port + "\r\n");
            }
            catch (SocketException E)
            {
                Logger.LogWarning("Winsock error caused by call to Socket.Bind(): \n" + E.ToString() + "\r\n");
            }

            m_ListenerSock.BeginAccept(new AsyncCallback(OnAccept), m_ListenerSock);
        }

		/// <summary>
		/// Gets a city server.
		/// </summary>
		/// <param name="UUID">UUID of city server.</param>
		/// <returns>A CityInfo instance if found, null otherwise.</returns>
		public CityInfo GetCityServer(string UUID)
		{
			lock (NetworkFacade.CServerListener.CityServers)
			{
				foreach (CityInfo CServer in NetworkFacade.CServerListener.CityServers)
				{
					if (CServer.UUID.Equals(UUID, StringComparison.CurrentCultureIgnoreCase))
					{
						return CServer;
					}
				}
			}

			return null;
		}

        public override void OnAccept(IAsyncResult AR)
        {
            Socket AcceptedSocket = m_ListenerSock.EndAccept(AR);

            if (AcceptedSocket != null)
            {
                Logger.LogDebug("\nNew cityserver connected!\r\n");

                //Let sockets linger for 5 seconds after they're closed, in an attempt to make sure all
                //pending data is sent!
                AcceptedSocket.LingerState = new LingerOption(true, 5);

                NetworkClient Client = new NetworkClient(AcceptedSocket, this, EncryptionMode.NoEncryption);

                PotentialLogins.Add(Client);
            }

            m_ListenerSock.BeginAccept(new AsyncCallback(OnAccept), m_ListenerSock);
        }

		public override void RemoveClient(NetworkClient Client)
		{
			CityInfo Info = CityServers.FirstOrDefault(x => x.Client == Client);

			if (CityServers.TryTake(out Info))
			{
				lock (NetworkFacade.ClientListener.Clients)
				{
					PacketStream ClientPacket = new PacketStream((byte)PacketType.CITY_SERVER_OFFLINE, 0);
					ClientPacket.WriteString(Info.Name);
					ClientPacket.WriteString(Info.Description);
					ClientPacket.WriteString(Info.IP);
					ClientPacket.WriteInt32(Info.Port);
					ClientPacket.WriteByte((byte)Info.Status);
					ClientPacket.WriteUInt64(Info.Thumbnail);
					ClientPacket.WriteString(Info.UUID);
					ClientPacket.WriteUInt64(Info.Map);

					foreach (NetworkClient Receiver in NetworkFacade.ClientListener.Clients)
						Receiver.SendEncrypted((byte)PacketType.CITY_SERVER_OFFLINE, ClientPacket.ToArray());
				}

				Debug.WriteLine("Removed CityServer!");
			}
		}
    }
}

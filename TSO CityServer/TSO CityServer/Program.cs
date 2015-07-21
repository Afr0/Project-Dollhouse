﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using CityDataModel;
using TSO_CityServer.VM;
using TSO_CityServer.Network;
using GonzoNet;
using GonzoNet.Encryption;
using Nancy.Hosting.Self;

namespace TSO_CityServer
{
	class Program
	{
		private static NetworkClient m_LoginClient;
		private static NancyHost m_NancyHost;
		private static VM.VM m_VM;

		static void Main(string[] args)
		{
			bool FoundConfig = ConfigurationManager.LoadCityConfig();

			Logger.Initialize("Log.txt");
			Logger.WarnEnabled = true;
			Logger.DebugEnabled = true;

			GonzoNet.Logger.OnMessageLogged += new GonzoNet.MessageLoggedDelegate(Logger_OnMessageLogged);
			CityDataModel.Logger.OnMessageLogged += new CityDataModel.MessageLoggedDelegate(Logger_OnMessageLogged);
			ProtocolAbstractionLibraryD.Logger.OnMessageLogged += new ProtocolAbstractionLibraryD.MessageLoggedDelegate(Logger_OnMessageLogged);

			if (!FoundConfig)
			{
				Console.WriteLine("Couldn't find a ServerConfig.ini file!");
				Console.ReadLine();
				Environment.Exit(0);
			}

			//This has to happen for the static constructor to be called...
			NetworkFacade m_NetworkFacade = new NetworkFacade();

			var dbConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MAIN_DB"];
			DataAccess.ConnectionString = dbConnectionString.ConnectionString;

			NetworkFacade.NetworkListener = new Listener(EncryptionMode.AESCrypto);
			//Remove a player from the current session when it disconnects.
			NetworkFacade.NetworkListener.OnDisconnected += new OnDisconnectedDelegate(NetworkFacade.CurrentSession.RemovePlayer);

			m_LoginClient = new NetworkClient("127.0.0.1", 2108, EncryptionMode.AESCrypto, true);
			m_LoginClient.OnNetworkError += new NetworkErrorDelegate(m_LoginClient_OnNetworkError);
			m_LoginClient.OnConnected += new OnConnectedDelegate(m_LoginClient_OnConnected);
			m_LoginClient.Connect(null);

			//Adds all houses from DB to the current session.
			using(DataAccess db = DataAccess.Get())
			{
				IQueryable<Character> Chars = db.Characters.GetAllCharsWithHouses();

				var CharsWithHouses = Chars.Where(x => x.HouseHouse != null);

				foreach(Character Char in CharsWithHouses)
					NetworkFacade.CurrentSession.AddHouse(Char, Char.HouseHouse);
			}

			NetworkFacade.NetworkListener.Initialize(Settings.BINDING);
			m_NancyHost = new NancyHost(new Uri("http://173.248.136.133:8888/city/"));
			m_NancyHost.Start();

			try
			{
				NetworkFacade.CurrentTerrain.Initialize("East Jerome");
				NetworkFacade.CurrentTerrain.LoadContent();
				NetworkFacade.CurrentTerrain.GenerateCityMesh();
			}
			catch(Exception e)
			{
				Console.WriteLine("Couldn't load elevation data!");
				Console.WriteLine(e.ToString());
				Console.ReadLine();
				Environment.Exit(0);
			}
			

			m_VM = new VM.VM();
			m_VM.Init();

			while (true)
			{
				m_VM.Update();
				Thread.Sleep(1000);
			}
		}

		/// <summary>
		/// Server successfully connected to login server!
		/// </summary>
		/// <param name="LoginArgs">Arguments used to login.</param>
		private static void m_LoginClient_OnConnected(LoginArgsContainer LoginArgs)
		{
			LoginPacketSenders.SendServerInfo(m_LoginClient);
		}

		/// <summary>
		/// Event triggered if a network error occurs while communicating with
		/// the LoginServer.
		/// </summary>
		/// <param name="Exception"></param>
		private static void m_LoginClient_OnNetworkError(SocketException Exc)
		{
			Console.WriteLine(Exc.ToString());
			Console.ReadLine();
			Environment.Exit(0);
		}

		#region Log Sink

		private static void Logger_OnMessageLogged(ProtocolAbstractionLibraryD.LogMessage Msg)
		{
			switch (Msg.Level)
			{
				case ProtocolAbstractionLibraryD.LogLevel.info:
					Logger.LogInfo(Msg.Message);
					break;
				case ProtocolAbstractionLibraryD.LogLevel.error:
					Logger.LogDebug(Msg.Message);
					break;
				case ProtocolAbstractionLibraryD.LogLevel.warn:
					Logger.LogWarning(Msg.Message);
					break;
			}
		}

		private static void Logger_OnMessageLogged(CityDataModel.LogMessage Msg)
		{
			switch (Msg.Level)
			{
				case CityDataModel.LogLevel.info:
					Logger.LogInfo(Msg.Message);
					break;
				case CityDataModel.LogLevel.error:
					Logger.LogDebug(Msg.Message);
					break;
				case CityDataModel.LogLevel.warn:
					Logger.LogWarning(Msg.Message);
					break;
			}
		}

		private static void Logger_OnMessageLogged(GonzoNet.LogMessage Msg)
		{
			switch (Msg.Level)
			{
				case GonzoNet.LogLevel.info:
					Logger.LogInfo(Msg.Message);
					break;
				case GonzoNet.LogLevel.error:
					Logger.LogDebug(Msg.Message);
					break;
				case GonzoNet.LogLevel.warn:
					Logger.LogWarning(Msg.Message);
					break;
			}
		}

		#endregion
	}
}

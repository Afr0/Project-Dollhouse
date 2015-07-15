﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;
using TSO_LoginServer.Network;
using GonzoNet;
using GonzoNet.Encryption;
using System.Configuration;
using LoginDataModel;
using ProtocolAbstractionLibraryD;

namespace TSO_LoginServer
{
	class Program
	{
		private static void Main(string[] args)
		{
			/**
			 * BOOTSTRAP - THIS IS WHERE THE SERVER STARTS UP
			 * STEPS:
			 *  > Start logging system
			 *  > Load configuration
			 *  > Register packet handlers
			 *  > Start the login server service
			 */
			Logger.Initialize("log.txt");
			Logger.InfoEnabled = true; //Disable for release.
			Logger.DebugEnabled = true;
			Logger.WarnEnabled = true;

			GonzoNet.Logger.OnMessageLogged += new GonzoNet.MessageLoggedDelegate(Logger_OnMessageLogged);
			LoginDataModel.Logger.OnMessageLogged += new LoginDataModel.MessageLoggedDelegate(Logger_OnMessageLogged);
			ProtocolAbstractionLibraryD.Logger.OnMessageLogged += new ProtocolAbstractionLibraryD.MessageLoggedDelegate(Logger_OnMessageLogged);

			PacketHandlers.Register((byte)PacketType.LOGIN_REQUEST, false, 0, new OnPacketReceive(LoginPacketHandlers.HandleLoginRequest));
			PacketHandlers.Register((byte)PacketType.CHALLENGE_RESPONSE, true, 0, new OnPacketReceive(LoginPacketHandlers.HandleChallengeResponse));
			PacketHandlers.Register((byte)PacketType.CHARACTER_LIST, true, 0, new OnPacketReceive(LoginPacketHandlers.HandleCharacterInfoRequest));
			PacketHandlers.Register((byte)PacketType.CITY_LIST, true, 0, new OnPacketReceive(LoginPacketHandlers.HandleCityInfoRequest));
			PacketHandlers.Register((byte)PacketType.CHARACTER_CREATE, true, 0, new OnPacketReceive(LoginPacketHandlers.HandleCharacterCreate));
			PacketHandlers.Register((byte)PacketType.REQUEST_CITY_TOKEN, true, 0, new OnPacketReceive(LoginPacketHandlers.HandleCityTokenRequest));
			PacketHandlers.Register((byte)PacketType.RETIRE_CHARACTER, true, 0, new OnPacketReceive(LoginPacketHandlers.HandleCharacterRetirement));

			var Listener = new Listener(EncryptionMode.AESCrypto);
			Listener.Initialize(Settings.BINDING);
			NetworkFacade.ClientListener = Listener;

			NetworkFacade.CServerListener = new CityServerListener(EncryptionMode.AESCrypto);
			NetworkFacade.CServerListener.Initialize(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2108));

			var dbConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MAIN_DB"];
			DataAccess.ConnectionString = dbConnectionString.ConnectionString;

			//Test the DB connection.
			try
			{
				using (var db = DataAccess.Get())
				{
					var testAccount = db.Accounts.GetByUsername("root");
					if (testAccount == null)
					{
						db.Accounts.Create(new Account
						{
							AccountName = "root",
							Password = Account.GetPasswordHash("root", "root")
						});
					}
				}
			}
			catch (Exception)
			{
				Console.WriteLine("Couldn't connect to database!");
				Console.ReadKey();
				Environment.Exit(0);
			}

			//64 is 100 in decimal.
			PacketHandlers.Register(0x64, false, 0, new OnPacketReceive(CityServerPacketHandlers.HandleCityServerLogin));
			PacketHandlers.Register(0x67, false, 0, new OnPacketReceive(CityServerPacketHandlers.HandlePlayerOnlineResponse));

			while(true)
			{
				Thread.Sleep(1000);
			}
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

		private static void Logger_OnMessageLogged(LoginDataModel.LogMessage Msg)
		{
			switch (Msg.Level)
			{
				case LoginDataModel.LogLevel.info:
					Logger.LogInfo(Msg.Message);
					break;
				case LoginDataModel.LogLevel.error:
					Logger.LogDebug(Msg.Message);
					break;
				case LoginDataModel.LogLevel.warn:
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

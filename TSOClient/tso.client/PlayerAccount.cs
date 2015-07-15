﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.*/

using System;
using System.Collections.Generic;
using System.Text;
using TSOClient.Code.UI.Controls;
using TSOClient.Lot;
using GonzoNet;

namespace TSOClient
{
    /// <summary>
    /// A class representing the current player's account.
    /// Holds things such as the account's sims and client
    /// (used to communicate with the server).
    /// </summary>
    class PlayerAccount
    {
        public static UISim CurrentlyActiveSim;
        
        public static string Username = "";
        //The hash of the username and password. See UIPacketSenders.SendLoginRequest()
        public static byte[] Hash = new byte[1];

        //Token received from LoginServer when transitioning to a CityServer.
        public static string CityToken = "";
    }
}

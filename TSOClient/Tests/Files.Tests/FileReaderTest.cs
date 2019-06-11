﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the Files.Test.

The Initial Developer of the Original Code is
Mats 'Afr0' Vederhus. All Rights Reserved.

Contributor(s):
*/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Files.Manager;
using Files.FAR1;
using Files.FAR3;
using Files.DBPF;
using Files.IFF;
using Files.AudioLogic;
using Files.AudioFiles;
using Sound;
using Microsoft.Win32;

namespace Files.Tests
{
    [TestClass]
    public class FileReaderTest
    {
        private static bool is64BitProcess = (IntPtr.Size == 8);
        private static bool is64BitOperatingSystem = is64BitProcess || InternalCheckIsWow64();

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process);

        /// <summary>
        /// Determines if this process is run on a 64bit OS.
        /// </summary>
        /// <returns>True if it is, false otherwise.</returns>
        private static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!IsWow64Process(p.Handle, out retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            else
            {
                return false;
            }
        }

        private string GetInstallDir()
        {
            string Software = "";

            if ((is64BitOperatingSystem == false) && (is64BitProcess == false))
                Software = "SOFTWARE";
            else
                Software = "SOFTWARE\\Wow6432Node";

            //Find the path to TSO on the user's system.
            RegistryKey softwareKey = Registry.LocalMachine.OpenSubKey(Software);

            if (Array.Exists(softwareKey.GetSubKeyNames(), delegate (string s) { return s.Equals("Maxis", StringComparison.InvariantCultureIgnoreCase); }))
            {
                RegistryKey maxisKey = softwareKey.OpenSubKey("Maxis");
                if (Array.Exists(maxisKey.GetSubKeyNames(), delegate (string s) { return s.Equals("The Sims Online", StringComparison.InvariantCultureIgnoreCase); }))
                {
                    RegistryKey tsoKey = maxisKey.OpenSubKey("The Sims Online");
                    return (string)tsoKey.GetValue("InstallDir") + "\\TSOClient\\";
                }
            }

            return "";
        }

        /// <summary>
        /// Gets a value indicating if platform is Linux.
        /// </summary>
        /// <value><c>true</c> if is Linux; otherwise, <c>false</c>.</value>
        private bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        /// <summary>
        /// Tests FAR1 parsing by attempting to open a FAR1 as a FAR3 archive.
        /// Currently hardcoded for Windows because test methods can't take parameters.
        /// </summary>
        [TestMethod]
        public void TestIncorrectFAR1Parsing()
        {
            string GameDir = GetInstallDir();
            string Delimiter = (IsLinux) ? "//" : "\\";

            FAR1Archive Arch = new FAR1Archive(GameDir + "uigraphics" + Delimiter + "credits" + Delimiter +
                "credits.dat");

            Assert.IsFalse(Arch.ReadArchive(false));
        }

        /// <summary>
        /// Tests FAR3 parsing by attempting to open a FAR3 as a DBPF archive.
        /// Currently hardcoded for Windows because test methods can't take parameters.
        /// </summary>
        [TestMethod]
        public void TestIncorrectFAR3Parsing()
        {
            string GameDir = GetInstallDir();

            FAR3Archive Arch = new FAR3Archive(GameDir + "EP2.dat");

            Assert.IsFalse(Arch.ReadArchive(false));
        }

        /// <summary>
        /// Tests DBPF parsing by attempting to open a DBPF as a FAR3 archive.
        /// Currently hardcoded for Windows because test methods can't take parameters.
        /// </summary>
        [TestMethod]
        public void TestIncorrectDBPFParsing()
        {
            string GameDir = GetInstallDir();
            string Delimiter = (IsLinux) ? "//" : "\\";

            DBPFArchive Arch = new DBPFArchive(GameDir + "uigraphics" + Delimiter + "credits" + Delimiter + 
                "credits.dat");

            Assert.IsFalse(Arch.ReadArchive(false));
        }

        /// <summary>
        /// Tests FAR1 parsing by attempting to open a FAR1 archive.
        /// Currently hardcoded for Windows because test methods can't take parameters.
        /// </summary>
        [TestMethod]
        public void TestCorrectFAR1Parsing()
        {
            string GameDir = GetInstallDir();
            string Delimiter = (IsLinux) ? "//" : "\\";

            FAR1Archive Arch = new FAR1Archive(GameDir + "objectdata" + Delimiter + "objects" + Delimiter +
                "objiff.far");

            Assert.IsTrue(Arch.ReadArchive(false));
        }

        /// <summary>
        /// Tests FAR3 parsing by attempting to open a FAR3 archive.
        /// Currently hardcoded for Windows because test methods can't take parameters.
        /// </summary>
        [TestMethod]
        public void TestCorrectFAR3Parsing()
        {
            string GameDir = GetInstallDir();
            string Delimiter = (IsLinux) ? "//" : "\\";

            FAR3Archive Arch = new FAR3Archive(GameDir + "uigraphics" + Delimiter + "credits" + Delimiter +
                "credits.dat");

            Assert.IsTrue(Arch.ReadArchive(false));
        }

        /// <summary>
        /// Tests DBPF parsing by attempting to open a DBPF archive.
        /// Currently hardcoded for Windows because test methods can't take parameters.
        /// </summary>
        [TestMethod]
        public void TestCorrectDBPFParsing()
        {
            string GameDir = GetInstallDir();
            
            DBPFArchive Arch = new DBPFArchive(GameDir + "EP2.dat");

            Assert.IsTrue(Arch.ReadArchive(false));
        }

        /// <summary>
        /// Tests IFF parsing by attempting to open a IFF.
        /// Currently hardcoded for Windows because test methods can't take parameters.
        /// </summary>
        [TestMethod]
        public void TestCorrectIFFParsing()
        {
            string GameDir = GetInstallDir();
            string Delimiter = (IsLinux) ? "//" : "\\";

            FAR1Archive Arch = new FAR1Archive(GameDir + "objectdata" + Delimiter + "objects" + Delimiter +
                "objiff.far");
            Arch.ReadArchive(false);

            Iff Obj = new Iff();
            Assert.IsTrue(Obj.Init(Arch.GrabEntry(FileUtilities.GenerateHash("anniversary.iff")), false));
        }

        /// <summary>
        /// Tests HIT parsing by attempting to open a HIT.
        /// Currently hardcoded for Windows because test methods can't take parameters.
        /// </summary>
        [TestMethod]
        public void TestCorrectHITParsing()
        {
            string GameDir = GetInstallDir();
            string Delimiter = (IsLinux) ? "//" : "\\";

            try
            {
                Hit Snd = new Hit(File.Open(GameDir + "sounddata" + Delimiter + "newmain.hit",
                    FileMode.Open, FileAccess.ReadWrite));
            }
            catch(HitException)
            {
                Assert.Fail();
            }
        }

        /// <summary>
        /// Tests XA parsing by attempting to open a XA.
        /// Currently hardcoded for Windows because test methods can't take parameters.
        /// </summary>
        [TestMethod]
        public void TestCorrectXAParsing()
        {
            string GameDir = GetInstallDir();
            string Delimiter = (IsLinux) ? "//" : "\\";

            try
            {
                XAFile Snd = new XAFile(File.Open(GameDir + "sounddata" + Delimiter + "tvstations" + Delimiter + 
                    "tv_romance" + Delimiter +  "tv_r1.xa", FileMode.Open, FileAccess.ReadWrite));
            }
            catch (Exception E)
            {
                Debug.WriteLine(E.ToString());
                Assert.Fail();
            }
        }

        /// <summary>
        /// Tests MP3 parsing by attempting to open a MP3.
        /// Currently hardcoded for Windows because test methods can't take parameters.
        /// </summary>
        [TestMethod]
        public void TestCorrectMP3Parsing()
        {
            string GameDir = GetInstallDir();
            string Delimiter = (IsLinux) ? "//" : "\\";

            if (FileManager.IsLinux)
            {
                try
                {
                    MP3File Snd = new MP3File(File.Open(GameDir + "music" + Delimiter + "modes" + Delimiter +
                        "map" + Delimiter + "tsomap4_v1.mp3", FileMode.Open, FileAccess.ReadWrite));
                    Snd.DecompressedWav();
                }
                catch (Exception E)
                {
                    Debug.WriteLine(E.ToString());
                    Assert.Fail();
                }
            }
            else
            {
                try
                {
                    SoundPlayer Player = new SoundPlayer(GameDir + "music" + Delimiter + "modes" + Delimiter +
                            "map" + Delimiter + "tsobuild3.mp3");
                    Player.PlaySound(false, true);
                }
                catch (Exception E)
                {
                    Debug.WriteLine(E.ToString());
                    Assert.Fail();
                }
            }
        }

        /// <summary>
        /// Tests Ini parsing by attempting to open a Ini.
        /// Currently hardcoded for Windows because test methods can't take parameters.
        /// </summary>
        [TestMethod]
        public void TestCorrectIniParsing()
        {
            string GameDir = GetInstallDir();
            string Delimiter = (IsLinux) ? "//" : "\\";

            try
            {
                Ini IniFile = new Ini(File.Open(GameDir + "sys" + Delimiter + "radio.ini",
                    FileMode.Open, FileAccess.ReadWrite));
            }
            catch (FileNotFoundException)
            {
                Assert.Fail();
            }
        }
    }
}

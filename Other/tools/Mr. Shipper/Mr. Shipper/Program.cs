﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is Mr. Shipper.

The Initial Developer of the Original Code is
Mats 'Afr0' Vederhus. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using Files.FAR3;
using Files.FAR1;

namespace Mr.Shipper
{
    class Program
    {
        static void Main(string[] args)
        {
            //Find the path to TSO on the user's system.
            RegistryKey softwareKey = Registry.LocalMachine.OpenSubKey("SOFTWARE");
            if (Array.Exists(softwareKey.GetSubKeyNames(), delegate(string s) { return s.CompareTo("Maxis") == 0; }))
            {
                RegistryKey maxisKey = softwareKey.OpenSubKey("Maxis");
                if (Array.Exists(maxisKey.GetSubKeyNames(), delegate(string s) { return s.CompareTo("The Sims Online") == 0; }))
                {
                    RegistryKey tsoKey = maxisKey.OpenSubKey("The Sims Online");
                    string installDir = (string)tsoKey.GetValue("InstallDir");
                    installDir += "\\TSOClient\\";
                    GlobalSettings.Default.StartupPath = installDir;
                }
                else
                {
                    Console.WriteLine("Error TSO was not found on your system.");
                    Console.ReadLine();
                    return;
                }
            }
            else
            {
                Console.WriteLine("Error: No Maxis products were found on your system.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Building database of existing entries...");
            Database.BuildEntryDatabase();
            Console.WriteLine("Done!");

            Console.WriteLine("Generating uigraphics database...");
            GenerateUIGraphicsDatabase();
            Console.WriteLine("Done!");

            Console.WriteLine("Generating collections database...");
            GenerateCollectionsDatabase();
            Console.WriteLine("Done!");

            Console.WriteLine("Generating purchasables database...");
            GeneratePurchasablesDatabase();
            Console.WriteLine("Done!");

            Console.WriteLine("Generating outfits database...");
            GenerateOutfitsDatabase();
            Console.WriteLine("Done!");

            Console.WriteLine("Generating appearances database...");
            GenerateAppearancesDatabase();
            Console.WriteLine("Done!");

            Console.WriteLine("Generating thumbnails database...");
            GenerateThumbnailsDatabase();
            Console.WriteLine("Done!");

            Console.WriteLine("Generating mesh database...");
            GenerateMeshDatabase();
            Console.WriteLine("Done!");

            Console.WriteLine("Generating texture database...");
            GenerateTexturesDatabase();
            Console.WriteLine("Done!");

            Console.WriteLine("Generating binding database...");
            GenerateBindingsDatabase();
            Console.WriteLine("Done!");

            Console.WriteLine("Generating handgroups database...");
            GenerateHandGroupsDatabase();
            Console.WriteLine("Done!");

            Console.WriteLine("Generating cities database...");
            GenerateCitiesDatabase();
            Console.WriteLine("Done!");

            Console.WriteLine("Generating animations database...");
            GenerateAnimationsDatabase();
            Console.WriteLine("Done!");

            Console.ReadLine();
        }

        /// <summary>
        /// Generates a database of the files in the uigraphics folder,
        /// as well as a *.cs file with an enumeration of the same files
        /// and their corresponding FileIDs.
        /// </summary>
        private static void GenerateUIGraphicsDatabase()
        {
            Dictionary<FAR3Entry, string> UIEntries = new Dictionary<FAR3Entry, string>();

            AddFilesFromDir(GlobalSettings.Default.StartupPath + "uigraphics\\", "", ref UIEntries);

            Directory.CreateDirectory("packingslips");
            WriteCSEnum("packingslips\\UIFileIDs.cs", "UIFileIDs", UIEntries);

            StreamWriter Writer = new StreamWriter(File.Create(GlobalSettings.Default.StartupPath + 
                "packingslips\\uigraphics.xml"));
            Writer.WriteLine("<?xml version=\"1.0\"?>");
            Writer.WriteLine("<AssetList>");

            //For some really weird reason, "key" and "assetID" are written in reverse order...
            //UI entries need to be padded out with extra 0s because there are duplicate entries.
            foreach (KeyValuePair<FAR3Entry, string> KVP in UIEntries)
            {
                if (KVP.Value.Contains(".dat"))
                {
                    /*Writer.WriteLine("  " + "<DefineAssetString key=\"" + KVP.Value +
                        "\" assetID=\"" + "0x" + 
                        HelperFuncs.ApplyPadding(string.Format("{0:X}", KVP.Key.TypeID)) +
                        HelperFuncs.ApplyPadding(string.Format("{0:X}", KVP.Key.FileID))
                        .Replace("0x", "") + "\"/>");*/
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + KVP.Value +
                        "\" assetID=\"" + "0x" +
                        string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
                else
                {
                    /*DirectoryInfo DirInfo = new DirectoryInfo(KVP.Value);
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + DirInfo.Parent + "\\" +
                        Path.GetFileName(KVP.Value) + "\" assetID=\"" +
                        HelperFuncs.ApplyPadding(string.Format("{0:X}", KVP.Key.TypeID)) + 
                        HelperFuncs.ApplyPadding(string.Format("{0:X}", 
                        KVP.Key.FileID)).Replace("0x", "") + "\"/>");*/
                    DirectoryInfo DirInfo = new DirectoryInfo(KVP.Value);
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + DirInfo.Parent + "\\" +
                        Path.GetFileName(KVP.Value) + "\" assetID=\"" +
                        string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
            }

            Writer.WriteLine("</AssetList>");
            Writer.Close();
        }

        private static void GenerateCollectionsDatabase()
        {
            Dictionary<FAR3Entry, string> CollectionEntries = new Dictionary<FAR3Entry, string>();

            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\bodies\\", "collections", ref CollectionEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\heads\\", "collections", ref CollectionEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\bodies\\", "collections", ref CollectionEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\heads\\", "collections", ref CollectionEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\bodies\\", "collections", ref CollectionEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\heads\\", "collections", ref CollectionEntries);

            StreamWriter Writer = new StreamWriter(File.Create("packingslips\\CollectionsFileIDs.cs"));

            Writer.WriteLine("using System;");
            Writer.WriteLine("");
            Writer.WriteLine("namespace TSOClient");
            Writer.WriteLine("{");
            Writer.WriteLine("  //Generated by Mr. Shipper - filenames have been sanitized, and does not match");
            Writer.WriteLine("  //actual filenames character for character!");
            Writer.WriteLine("  partial class FileIDs");
            Writer.WriteLine("  {");
            Writer.WriteLine("      public enum CollectionsFileIDs : long");
            Writer.WriteLine("      {");

            int StopCounter = 0;
            foreach (KeyValuePair<FAR3Entry, string> KVP in CollectionEntries)
            {
                StopCounter++;

                if (StopCounter < CollectionEntries.Count)
                {
                    Writer.WriteLine("          " + HelperFuncs.SanitizeFilename(Path.GetFileName(KVP.Key.Filename)) + " = " +
                        "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + ",");
                }
                else
                {
                    Writer.WriteLine("          " + HelperFuncs.SanitizeFilename(Path.GetFileName(KVP.Key.Filename)) + " = " +
                        "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)));
                }
            }

            Writer.WriteLine("      };");
            Writer.WriteLine("  }");
            Writer.WriteLine("}");
            Writer.Close();

            Writer = new StreamWriter(File.Create(GlobalSettings.Default.StartupPath + 
                "packingslips\\collections.xml"));
            Writer.WriteLine("<?xml version=\"1.0\"?>");
            Writer.WriteLine("<AssetList>");

            //For some really weird reason, "key" and "assetID" are written in reverse order...
            foreach (KeyValuePair<FAR3Entry, string> KVP in CollectionEntries)
            {
                if (KVP.Value.Contains(".dat"))
                {
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + KVP.Value +
                        "\" assetID=\"" + "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
                else
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(KVP.Value);
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + DirInfo.Parent + "\\" + 
                        Path.GetFileName(KVP.Value) + "\" assetID=\"" + "0x" + 
                        string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
            }

            Writer.WriteLine("</AssetList>");
            Writer.Close();
        }

        private static void GeneratePurchasablesDatabase()
        {
            Dictionary<FAR3Entry, string> PurchasablesEntries = new Dictionary<FAR3Entry, string>();

            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\bodies\\", "purchasables", ref PurchasablesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\heads\\", "purchasables", ref PurchasablesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\bodies\\", "purchasables", ref PurchasablesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\heads\\", "purchasables", ref PurchasablesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\bodies\\", "purchasables", ref PurchasablesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\heads\\", "purchasables", ref PurchasablesEntries);

            WriteCSEnum("packingslips\\PurchasablesFileIDs.cs", "PurchasablesFileIDs", PurchasablesEntries);

            StreamWriter Writer = new StreamWriter(File.Create(GlobalSettings.Default.StartupPath + 
                "packingslips\\purchasables.xml"));
            Writer.WriteLine("<?xml version=\"1.0\"?>");
            Writer.WriteLine("<AssetList>");

            //For some really weird reason, "key" and "assetID" are written in reverse order...
            foreach (KeyValuePair<FAR3Entry, string> KVP in PurchasablesEntries)
            {
                if (KVP.Value.Contains(".dat"))
                {
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + KVP.Value +
                        "\" assetID=\"" + "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
                else
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(KVP.Value);
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + DirInfo.Parent + "\\" +
                        Path.GetFileName(KVP.Value) + "\" assetID=\"" + "0x" +
                        string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
            }

            Writer.WriteLine("</AssetList>");
            Writer.Close();
        }

        private static void GenerateOutfitsDatabase()
        {
            Dictionary<FAR3Entry, string> OutfitsEntries = new Dictionary<FAR3Entry, string>();

            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\bodies\\", "outfits", ref OutfitsEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\heads\\", "outfits", ref OutfitsEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\bodies\\", "outfits", ref OutfitsEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\heads\\", "outfits", ref OutfitsEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\bodies\\", "outfits", ref OutfitsEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\heads\\", "outfits", ref OutfitsEntries);

            WriteCSEnum("packingslips\\OutfitsFileIDs.cs", "OutfitsFileIDs", OutfitsEntries);

            StreamWriter Writer = new StreamWriter(File.Create(GlobalSettings.Default.StartupPath + 
                "packingslips\\alloutfits.xml"));
            Writer.WriteLine("<?xml version=\"1.0\"?>");
            Writer.WriteLine("<AssetList>");

            //For some really weird reason, "key" and "assetID" are written in reverse order...
            foreach (KeyValuePair<FAR3Entry, string> KVP in OutfitsEntries)
            {
                if (KVP.Value.Contains(".dat"))
                {
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + KVP.Value +
                        "\" assetID=\"" + "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
                else
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(KVP.Value);
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + DirInfo.Parent + "\\" +
                        Path.GetFileName(KVP.Value) + "\" assetID=\"" + "0x" +
                        string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
            }

            Writer.WriteLine("</AssetList>");
            Writer.Close();
        }

        private static void GenerateAppearancesDatabase()
        {
            Dictionary<FAR3Entry, string> AppearancesEntries = new Dictionary<FAR3Entry, string>();

            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\bodies\\", "appearances", ref AppearancesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\heads\\", "appearances", ref AppearancesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\hands\\", "appearances", ref AppearancesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\accessories\\", "appearances", ref AppearancesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\bodies\\", "appearances", ref AppearancesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\heads\\", "appearances", ref AppearancesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\hands\\", "appearances", ref AppearancesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\accessories\\", "appearances", ref AppearancesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\bodies\\", "appearances", ref AppearancesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\heads\\", "appearances", ref AppearancesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\hands\\", "appearances", ref AppearancesEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\accessories\\", "appearances", ref AppearancesEntries);

            WriteCSEnum("packingslips\\AppearancesFileIDs.cs", "AppearancesFileIDs", AppearancesEntries);

            StreamWriter Writer = new StreamWriter(File.Create(GlobalSettings.Default.StartupPath + 
                "packingslips\\appearances.xml"));
            Writer.WriteLine("<?xml version=\"1.0\"?>");
            Writer.WriteLine("<AssetList>");

            //For some really weird reason, "key" and "assetID" are written in reverse order...
            foreach (KeyValuePair<FAR3Entry, string> KVP in AppearancesEntries)
            {
                if (KVP.Value.Contains(".dat"))
                {
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + KVP.Value +
                        "\" assetID=\"" + "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
                else
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(KVP.Value);
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + DirInfo.Parent + "\\" +
                        Path.GetFileName(KVP.Value) + "\" assetID=\"" + "0x" +
                        string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
            }

            Writer.WriteLine("</AssetList>");
            Writer.Close();
        }

        private static void GenerateThumbnailsDatabase()
        {
            Dictionary<FAR3Entry, string> ThumbnailsEntries = new Dictionary<FAR3Entry, string>();

            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\bodies\\", "thumbnails", ref ThumbnailsEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\heads\\", "thumbnails", ref ThumbnailsEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\bodies\\", "thumbnails", ref ThumbnailsEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\heads\\", "thumbnails", ref ThumbnailsEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\bodies\\", "thumbnails", ref ThumbnailsEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\heads\\", "thumbnails", ref ThumbnailsEntries);

            WriteCSEnum("packingslips\\ThumbnailsFileIDs.cs", "ThumbnailsFileIDs", ThumbnailsEntries);

            StreamWriter Writer = new StreamWriter(File.Create(GlobalSettings.Default.StartupPath + 
                "packingslips\\thumbnails.xml"));
            Writer.WriteLine("<?xml version=\"1.0\"?>");
            Writer.WriteLine("<AssetList>");

            //For some really weird reason, "key" and "assetID" are written in reverse order...
            foreach (KeyValuePair<FAR3Entry, string> KVP in ThumbnailsEntries)
            {
                if (KVP.Value.Contains(".dat"))
                {
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + KVP.Value +
                        "\" assetID=\"" + "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
                else
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(KVP.Value);
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + DirInfo.Parent + "\\" +
                        Path.GetFileName(KVP.Value) + "\" assetID=\"" + "0x" +
                        string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
            }

            Writer.WriteLine("</AssetList>");
            Writer.Close();
        }

        private static void GenerateMeshDatabase()
        {
            Dictionary<FAR3Entry, string> MeshEntries = new Dictionary<FAR3Entry, string>();

            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\bodies\\", "meshes", ref MeshEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\heads\\", "meshes", ref MeshEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\hands\\", "meshes", ref MeshEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\accessories\\", "meshes", ref MeshEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\bodies\\", "meshes", ref MeshEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\heads\\", "meshes", ref MeshEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\hands\\", "meshes", ref MeshEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\accessories\\", "meshes", ref MeshEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\bodies\\", "meshes", ref MeshEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\heads\\", "meshes", ref MeshEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\hands\\", "meshes", ref MeshEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\accessories\\", "meshes", ref MeshEntries);

            WriteCSEnum("packingslips\\MeshFileIDs.cs", "MeshFileIDs", MeshEntries);

            StreamWriter Writer = new StreamWriter(File.Create(GlobalSettings.Default.StartupPath + 
                "packingslips\\meshes.xml"));
            Writer.WriteLine("<?xml version=\"1.0\"?>");
            Writer.WriteLine("<AssetList>");

            //For some really weird reason, "key" and "assetID" are written in reverse order...
            foreach (KeyValuePair<FAR3Entry, string> KVP in MeshEntries)
            {
                if (KVP.Value.Contains(".dat"))
                {
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + KVP.Value +
                        "\" assetID=\"" + "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
                else
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(KVP.Value);
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + DirInfo.Parent + "\\" +
                        Path.GetFileName(KVP.Value) + "\" assetID=\"" + "0x" +
                        string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
            }

            Writer.WriteLine("</AssetList>");
            Writer.Close();
        }

        private static void GenerateTexturesDatabase()
        {
            Dictionary<FAR3Entry, string> TextureEntries = new Dictionary<FAR3Entry, string>();

            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\bodies\\", "textures", ref TextureEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\heads\\", "textures", ref TextureEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\hands\\", "textures", ref TextureEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\accessories\\", "textures", ref TextureEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\bodies\\", "textures", ref TextureEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\heads\\", "textures", ref TextureEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\hands\\", "textures", ref TextureEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\accessories\\", "textures", ref TextureEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\bodies\\", "textures", ref TextureEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\heads\\", "textures", ref TextureEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\hands\\", "textures", ref TextureEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\accessories\\", "textures", ref TextureEntries);

            WriteCSEnum("packingslips\\TextureFileIDs.cs", "TextureFileIDs", TextureEntries);

            StreamWriter Writer = new StreamWriter(File.Create(GlobalSettings.Default.StartupPath + 
                "packingslips\\textures.xml"));
            Writer.WriteLine("<?xml version=\"1.0\"?>");
            Writer.WriteLine("<AssetList>");

            //For some really weird reason, "key" and "assetID" are written in reverse order...
            foreach (KeyValuePair<FAR3Entry, string> KVP in TextureEntries)
            {
                if (KVP.Value.Contains(".dat"))
                {
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + KVP.Value +
                        "\" assetID=\"" + "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
                else
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(KVP.Value);
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + DirInfo.Parent + "\\" +
                        Path.GetFileName(KVP.Value) + "\" assetID=\"" + "0x" +
                        string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
            }

            Writer.WriteLine("</AssetList>");
            Writer.Close();
        }

        private static void GenerateBindingsDatabase()
        {
            Dictionary<FAR3Entry, string> BindingEntries = new Dictionary<FAR3Entry, string>();

            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\bodies\\", "bindings", ref BindingEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\heads\\", "bindings", ref BindingEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\hands\\", "bindings", ref BindingEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\accessories\\", "bindings", ref BindingEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\bodies\\", "bindings", ref BindingEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\heads\\", "bindings", ref BindingEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\hands\\", "bindings", ref BindingEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\accessories\\", "bindings", ref BindingEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\bodies\\", "bindings", ref BindingEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\heads\\", "bindings", ref BindingEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\hands\\", "bindings", ref BindingEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\accessories\\", "bindings", ref BindingEntries);

            WriteCSEnum("packingslips\\BindingFileIDs.cs", "BindingsFileIDs", BindingEntries);

            StreamWriter Writer = new StreamWriter(File.Create(GlobalSettings.Default.StartupPath + 
                "packingslips\\bindings.xml"));
            Writer.WriteLine("<?xml version=\"1.0\"?>");
            Writer.WriteLine("<AssetList>");

            //For some really weird reason, "key" and "assetID" are written in reverse order...
            foreach (KeyValuePair<FAR3Entry, string> KVP in BindingEntries)
            {
                if (KVP.Value.Contains(".dat"))
                {
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + KVP.Value +
                        "\" assetID=\"" + "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
                else
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(KVP.Value);
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + DirInfo.Parent + "\\" +
                        Path.GetFileName(KVP.Value) + "\" assetID=\"" + "0x" +
                        string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
            }

            Writer.WriteLine("</AssetList>");
            Writer.Close();
        }

        private static void GenerateHandGroupsDatabase()
        {
            Dictionary<FAR3Entry, string> HandgroupEntries = new Dictionary<FAR3Entry, string>();

            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\hands\\", "groups", ref HandgroupEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\hands\\", "groups", ref HandgroupEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\hands\\", "groups", ref HandgroupEntries);

            WriteCSEnum("packingslips\\HandgroupsFileIDs.cs", "HandgroupsFileIDs", HandgroupEntries);

            StreamWriter Writer = new StreamWriter(File.Create(GlobalSettings.Default.StartupPath +
                "packingslips\\handgroups.xml"));
            Writer.WriteLine("<?xml version=\"1.0\"?>");
            Writer.WriteLine("<AssetList>");

            //For some really weird reason, "key" and "assetID" are written in reverse order...
            foreach (KeyValuePair<FAR3Entry, string> KVP in HandgroupEntries)
            {
                if (KVP.Value.Contains(".dat"))
                {
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + KVP.Value +
                        "\" assetID=\"" + "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
                else
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(KVP.Value);
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + DirInfo.Parent + "\\" +
                        Path.GetFileName(KVP.Value) + "\" assetID=\"" + "0x" +
                        string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
            }

            Writer.WriteLine("</AssetList>");
            Writer.Close();
        }

        private static void GenerateCitiesDatabase()
        {
            Dictionary<FAR3Entry, string> CityEntries = new Dictionary<FAR3Entry, string>();

            AddFilesFromDir(GlobalSettings.Default.StartupPath + "cities\\", "", ref CityEntries);
            WriteCSEnum("packingslips\\CitiesFileIDs.cs", "CitiesFileIDs", CityEntries);

            StreamWriter Writer = new StreamWriter(File.Create(GlobalSettings.Default.StartupPath +
                "packingslips\\cities.xml"));
            Writer.WriteLine("<?xml version=\"1.0\"?>");
            Writer.WriteLine("<AssetList>");

            //For some really weird reason, "key" and "assetID" are written in reverse order...
            foreach (KeyValuePair<FAR3Entry, string> KVP in CityEntries)
            {
                if (KVP.Value.Contains(".dat"))
                {
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + KVP.Value +
                        "\" assetID=\"" + "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
                else
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(KVP.Value);
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + DirInfo.Parent + "\\" +
                        Path.GetFileName(KVP.Value) + "\" assetID=\"" + "0x" +
                        string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
            }

            Writer.WriteLine("</AssetList>");
            Writer.Close();
        }

        private static void GenerateAnimationsDatabase()
        {
            Dictionary<FAR3Entry, string> AnimEntries = new Dictionary<FAR3Entry, string>();

            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata\\animations\\", "", ref AnimEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata2\\animations\\", "", ref AnimEntries);
            AddFilesFromDir(GlobalSettings.Default.StartupPath + "avatardata3\\animations\\", "", ref AnimEntries);

            WriteCSEnum("packingslips\\AnimationsFileIDs.cs", "AnimationsFileIDs", AnimEntries);

            StreamWriter Writer = new StreamWriter(File.Create(GlobalSettings.Default.StartupPath +
                "packingslips\\animations.xml"));
            Writer.WriteLine("<?xml version=\"1.0\"?>");
            Writer.WriteLine("<AssetList>");

            //For some really weird reason, "key" and "assetID" are written in reverse order...
            foreach (KeyValuePair<FAR3Entry, string> KVP in AnimEntries)
            {
                if (KVP.Value.Contains(".dat"))
                {
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + KVP.Value +
                        "\" assetID=\"" + "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
                else
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(KVP.Value);
                    Writer.WriteLine("  " + "<DefineAssetString key=\"" + DirInfo.Parent + "\\" +
                        Path.GetFileName(KVP.Value) + "\" assetID=\"" + "0x" +
                        string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + "\"/>");
                }
            }

            Writer.WriteLine("</AssetList>");
            Writer.Close();
        }

        private static void WriteCSEnum(string Path, string EnumName, Dictionary<FAR3Entry, string> Entries)
        {
            StreamWriter Writer = new StreamWriter(File.Create(Path));

            Writer.WriteLine("using System;");
            Writer.WriteLine("");
            Writer.WriteLine("namespace TSOClient");
            Writer.WriteLine("{");
            Writer.WriteLine("  //Generated by Mr. Shipper - filenames have been sanitized, and does not match");
            Writer.WriteLine("  //actual filenames character for character!");
            Writer.WriteLine("  partial class FileIDs");
            Writer.WriteLine("  {");
            Writer.WriteLine("      public enum " + EnumName + " : long");
            Writer.WriteLine("      {");

            int StopCounter = 0;
            foreach (KeyValuePair<FAR3Entry, string> KVP in Entries)
            {
                StopCounter++;

                if (StopCounter < Entries.Count)
                {
                    Writer.WriteLine("          " + HelperFuncs.SanitizeFilename(KVP.Key.Filename.Replace("\\", "_")) + " = " +
                        "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)) + ",");
                }
                else
                {
                    Writer.WriteLine("          " + HelperFuncs.SanitizeFilename(KVP.Key.Filename.Replace("\\", "_")) + " = " +
                        "0x" + string.Format("{0:X}", HelperFuncs.ToID(KVP.Key.TypeID, KVP.Key.FileID)));
                }
            }

            Writer.WriteLine("      };");
            Writer.WriteLine("  }");
            Writer.WriteLine("}");
            Writer.Close();
        }

        /// <summary>
        /// Adds files from a specified directory to a dictionary of entries.
        /// </summary>
        /// <param name="EntryDir">The directory to scan for entries.</param>
        /// <param name="Filetype">A fully qualified lowercase filetype to scan for (can be empty).</param>
        /// <param name="Entries">The Dictionary to add entries to.</param>
        private static void AddFilesFromDir(string EntryDir, string Filetype, ref Dictionary<FAR3Entry, string> Entries)
        {
            string[] Dirs = Directory.GetDirectories(EntryDir);

            foreach(string Dir in Dirs)
            {
                if (Filetype != "")
                {
                    if (Dir.Contains(Filetype))
                    {
                        string[] Files = Directory.GetFiles(Dir);
                        string[] SubDirs = Directory.GetDirectories(Dir);
                        foreach (string Fle in Files)
                        {
                            if (Fle.Contains(".dat"))
                            {
                                FAR3Archive Archive = new FAR3Archive(Fle);
                                Archive.ReadArchive(true);

                                foreach (FAR3Entry Entry in Archive.GrabAllEntries())
                                    Entries.Add(Entry, Fle.Replace(GlobalSettings.Default.StartupPath, ""));
                            }
                            else
                            {
                                FAR3Entry Entry = new FAR3Entry();
                                Entry.Filename = Fle.Replace(GlobalSettings.Default.StartupPath, "");
                                Entry.FileID = HelperFuncs.GetFileID(Entry);
                                Entry.TypeID = HelperFuncs.GetTypeID(Path.GetExtension(Fle));

                                HelperFuncs.CheckCollision(Entry.FileID, Entries);

                                //Ignore fonts to minimize the risk of ID collisions.
                                if (!Entry.Filename.Contains(".ttf"))
                                {
                                    if (!Entry.Filename.Contains(".ffn"))
                                        Entries.Add(Entry, Entry.Filename);
                                }
                            }
                        }

                        foreach (string SubDir in SubDirs)
                        {
                            Files = Directory.GetFiles(SubDir);
                            foreach (string SubFle in Files)
                            {
                                if (SubFle.Contains(".dat"))
                                {
                                    FAR3Archive Archive = new FAR3Archive(SubFle);
                                    Archive.ReadArchive(false);

                                    foreach (FAR3Entry Entry in Archive.GrabAllEntries())
                                        Entries.Add(Entry, SubFle.Replace(GlobalSettings.Default.StartupPath, ""));
                                }
                                else
                                {
                                    FAR3Entry Entry = new FAR3Entry();
                                    Entry.Filename = SubFle.Replace(GlobalSettings.Default.StartupPath, "");
                                    Entry.FileID = HelperFuncs.GetFileID(Entry);
                                    Entry.TypeID = HelperFuncs.GetTypeID(Path.GetExtension(SubFle));

                                    HelperFuncs.CheckCollision(Entry.FileID, Entries);

                                    //Ignore fonts to minimize the risk of ID collisions.
                                    if (!Entry.Filename.Contains(".ttf"))
                                    {
                                        if (!Entry.Filename.Contains(".ffn"))
                                            Entries.Add(Entry, Entry.Filename);
                                    }
                                }
                            }
                        }
                    }
                }
                else //Filetype was empty, so just add all filetypes found...
                {
                    string[] Files = Directory.GetFiles(Dir);
                    string[] SubDirs = Directory.GetDirectories(Dir);
                    foreach (string Fle in Files)
                    {
                        if (Fle.Contains(".dat"))
                        {
                            FAR3Archive Archive = new FAR3Archive(Fle);
                            Archive.ReadArchive(true);

                            foreach (FAR3Entry Entry in Archive.GrabAllEntries())
                                Entries.Add(Entry, Fle.Replace(GlobalSettings.Default.StartupPath, ""));
                        }
                        else
                        {
                            FAR3Entry Entry = new FAR3Entry();
                            Entry.Filename = Fle.Replace(GlobalSettings.Default.StartupPath, "");
                            Entry.FileID = HelperFuncs.GetFileID(Entry);
                            Entry.TypeID = HelperFuncs.GetTypeID(Path.GetExtension(Fle));

                            HelperFuncs.CheckCollision((ulong)(((ulong)Entry.FileID) << 32 | ((ulong)(Entry.TypeID >> 32))), Entries);

                            //Ignore fonts to minimize the risk of ID collisions.
                            if (!Entry.Filename.Contains(".ttf"))
                            {
                                if(!Entry.Filename.Contains(".ffn"))
                                    Entries.Add(Entry, Entry.Filename);
                            }
                        }
                    }

                    foreach (string SubDir in SubDirs)
                    {
                        Files = Directory.GetFiles(SubDir);
                        foreach (string SubFle in Files)
                        {
                            if (SubFle.Contains(".dat"))
                            {
                                FAR3Archive Archive = new FAR3Archive(SubFle);
                                Archive.ReadArchive(true);

                                foreach (FAR3Entry Entry in Archive.GrabAllEntries())
                                    Entries.Add(Entry, SubFle.Replace(GlobalSettings.Default.StartupPath, ""));
                            }
                            else
                            {
                                FAR3Entry Entry = new FAR3Entry();
                                Entry.Filename = SubFle.Replace(GlobalSettings.Default.StartupPath, "");
                                Entry.FileID = HelperFuncs.GetFileID(Entry);
                                Entry.TypeID = HelperFuncs.GetTypeID(Path.GetExtension(SubFle));

                                HelperFuncs.CheckCollision((ulong)(((ulong)Entry.FileID) << 32 | ((ulong)(Entry.TypeID >> 32))), Entries);

                                //Ignore fonts to minimize the risk of ID collisions.
                                if (!Entry.Filename.Contains(".ttf"))
                                {
                                    if (!Entry.Filename.Contains(".ffn"))
                                        Entries.Add(Entry, Entry.Filename);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

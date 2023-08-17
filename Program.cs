﻿using Bungie;
using Bungie.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

class StartLoc
{
    public string position_xyz { get; set; }
    public string facing_angle { get; set; }
    public string team_enum { get; set; }
    public string type_0 { get; set; }
    public string type_1 { get; set; }
    public string type_2 { get; set; }
    public string type_3 { get; set; }
    public string spawn_type_0 { get; set; }
    public string spawn_type_1 { get; set; }
    public string spawn_type_2 { get; set; }
    public string spawn_type_3 { get; set; }
}

class WeapLoc
{
    public string weap_xyz { get; set; }
    public string weap_orient { get; set; }
    public string spawn_time { get; set; }
    public string weap_type { get; set; }
}

class Scenery
{
    public string scen_type { get; set; }
    public string scen_xyz { get; set; }
    public string scen_orient { get; set; }
    public string scen_vrnt { get; set; }
}

class TrigVol
{
    public string vol_name { get; set; }
    public string vol_xyz { get; set; }
    public string vol_ext { get; set; }
    public string vol_fwd { get; set; }
    public string vol_up { get; set; }
}


class MB_Zones
{
    static void Main()
    {
        string scen_path;
        string xml_path;

        // Temporarily disable
        /*
        Console.WriteLine("H2 to H3 Scenario Converter by PepperMan\n\n");
        while (true)
        {
            Console.WriteLine("Please enter the path to the H3 scenario file you wish to patch:");
            scen_path = Console.ReadLine().Trim('"');
            if (scen_path.EndsWith(".scenario"))
            {
                if (scen_path.Contains("H3EK"))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("\nLooks like a scenario tag, but doesn't seem to be in the H3EK directory. Please try again.");
                }
            }
            else
            {
                Console.WriteLine("\nFile does not look like a .scenario tag. Please try again.");
            }
        }

        while (true)
        {
            Console.WriteLine("\nPlease enter the path to an exported H2 scenario XML file.\nThis must be the full path with file extension - I *will* crash if this path is invalid:");
            xml_path = Console.ReadLine().Trim('"');
            if (xml_path.EndsWith(".xml") || xml_path.EndsWith(".txt")) // H2 exports as .txt by default, but we'll let .xml slide too
            {
                break;
            }
            else
            {
                Console.WriteLine("\nFile doesn't look like a .txt or .xml file. Please try again.");
            }
        }
        */

        // TODO: Remove temporary hardcoding
        scen_path = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\H3EK\\tags\\halo_2\\levels\\elongation\\elongation.scenario";
        xml_path = @"G:\Steam\steamapps\common\H2EK\elongation_output.xml";

        string h3ek_path = scen_path.Substring(0, scen_path.IndexOf("H3EK") + "H3EK".Length);

        // Create scenario backup
        string backup_folderpath = Path.GetDirectoryName(scen_path) + @"\scenario_backup";
        Directory.CreateDirectory(backup_folderpath);
        string backup_filepath = Path.Combine(backup_folderpath, scen_path.Split('\\').Last());
        
        if(!File.Exists(backup_filepath))
        {
            File.Copy(scen_path, backup_filepath);
            Console.WriteLine("Backup created successfully.");
        }
        else
        {
            Console.WriteLine("Backup already exists.");
        }

        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);
        Convert_XML(xml_path, h3ek_path, scen_path);
    }

    static void Convert_XML(string xml_path, string h3ek_path, string scen_path)
    {
        Console.WriteLine("\nBeginning XML Conversion:\n");

        string newFilePath = Path.Combine(Directory.GetCurrentDirectory(), "modified_input.xml");

        try
        {
            string[] lines = File.ReadAllLines(xml_path);
            bool removeLines = false;

            using (StreamWriter writer = new StreamWriter(newFilePath))
            {
                foreach (string line in lines)
                {
                    if (line.Contains("<block name=\"source files\">"))
                    {
                        removeLines = true;
                        writer.WriteLine(line);
                    }
                    else if (line.Contains("<block name=\"scripting data\">"))
                    {
                        removeLines = false;
                    }
                    else if (!removeLines)
                    {
                        writer.WriteLine(line);
                    }
                }
            }

            Console.WriteLine("Modified file saved successfully.\n\nPreparing to patch tag data:\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }

        xml_path = newFilePath;

        XmlDocument scenfile = new XmlDocument();
        scenfile.Load(xml_path);

        XmlNode root = scenfile.DocumentElement;

        XmlNodeList player_start_loc_block = root.SelectNodes(".//block[@name='player starting locations']");
        XmlNodeList weapon_placements_block = root.SelectNodes(".//block[@name='netgame equipment']");
        XmlNodeList scen_palette_block = root.SelectNodes(".//block[@name='scenery palette']");
        XmlNodeList scen_entries_block = root.SelectNodes(".//block[@name='scenery']");
        XmlNodeList trig_vol_block = root.SelectNodes(".//block[@name='trigger volumes']");

        List<StartLoc> all_starting_locs = new List<StartLoc>();
        List<WeapLoc> all_weapon_locs = new List<WeapLoc>();
        List<TagPath> all_scen_types = new List<TagPath>();
        List<Scenery> all_scen_entries = new List<Scenery>();
        List<TrigVol> all_trig_vols = new List<TrigVol>();

        foreach (XmlNode location in player_start_loc_block)
        {
            bool locs_end = false;
            int i = 0;
            while (!locs_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = location.SelectSingleNode(search_string);
                if (element != null)
                {
                    string xyz = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();
                    string facing = element.SelectSingleNode("./field[@name='facing']").InnerText.Trim();
                    string team = element.SelectSingleNode("./field[@name='team designator']").InnerText.Trim();
                    string type0 = element.SelectSingleNode("./field[@name='type 0']").InnerText.Trim();
                    string type1 = element.SelectSingleNode("./field[@name='type 1']").InnerText.Trim();
                    string type2 = element.SelectSingleNode("./field[@name='type 2']").InnerText.Trim();
                    string type3 = element.SelectSingleNode("./field[@name='type 3']").InnerText.Trim();
                    string spawn_type0 = element.SelectSingleNode("./field[@name='spawn type 0']").InnerText.Trim();
                    string spawn_type1 = element.SelectSingleNode("./field[@name='spawn type 1']").InnerText.Trim();
                    string spawn_type2 = element.SelectSingleNode("./field[@name='spawn type 2']").InnerText.Trim();
                    string spawn_type3 = element.SelectSingleNode("./field[@name='spawn type 3']").InnerText.Trim();

                    all_starting_locs.Add(new StartLoc
                    {
                        position_xyz = xyz,
                        facing_angle = facing,
                        team_enum = team,
                        type_0 = type0,
                        type_1 = type1,
                        type_2 = type2,
                        type_3 = type3,
                        spawn_type_0 = spawn_type0,
                        spawn_type_1 = spawn_type1,
                        spawn_type_2 = spawn_type2,
                        spawn_type_3 = spawn_type3,
                    });

                    Console.WriteLine("Processed starting position " + i);
                    i++;
                }
                else
                {
                    locs_end = true;
                    Console.WriteLine("\nFinished processing starting positions data.");
                }
            }
        }

        foreach (XmlNode weapon in weapon_placements_block)
        {
            bool weaps_end = false;
            int i = 0;
            while (!weaps_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = weapon.SelectSingleNode(search_string);
                if (element != null)
                {
                    string xyz = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();
                    string orient = element.SelectSingleNode("./field[@name='orientation']").InnerText.Trim();
                    string time = element.SelectSingleNode("./field[@name='spawn time (in seconds, 0 = default)']").InnerText.Trim();
                    string type = element.SelectSingleNode("./tag_reference[@name='item/vehicle collection']").InnerText.Trim();

                    all_weapon_locs.Add(new WeapLoc
                    {
                        weap_xyz = xyz,
                        weap_orient = orient,
                        spawn_time = time,
                        weap_type = type
                    });

                    Console.WriteLine("Process netgame equipment " + i);
                    i++;
                }
                else
                {
                    weaps_end = true;
                    Console.WriteLine("\nFinished processing netgame equipment (weapon) data.");
                }
            }
        }

        foreach (XmlNode location in scen_palette_block)
        {
            bool scen_end = false;
            int i = 0;
            while (!scen_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = location.SelectSingleNode(search_string);
                if (element != null)
                {
                    string scen_type = element.SelectSingleNode("./tag_reference[@name='name']").InnerText.Trim();
                    all_scen_types.Add(TagPath.FromPathAndType(scen_type, "scen*"));
                    i++;
                }
                else
                {
                    scen_end = true;
                    Console.WriteLine("Finished processing scenery palette data.");
                }
            }
        }

        foreach (XmlNode location in scen_entries_block)
        {
            bool scen_end = false;
            int i = 0;
            while (!scen_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = location.SelectSingleNode(search_string);
                if (element != null)
                {
                    string type = element.SelectSingleNode("./block_index[@name='short block index']").Attributes["index"].Value.ToString();
                    string xyz = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();
                    string orient = element.SelectSingleNode("./field[@name='rotation']").InnerText.Trim();
                    string variant = element.SelectSingleNode("./field[@name='variant name']").InnerText.Trim();

                    all_scen_entries.Add(new Scenery
                    {
                        scen_type = type,
                        scen_xyz = xyz,
                        scen_orient = orient,
                        scen_vrnt = variant
                    });

                    i++;
                }
                else
                {
                    scen_end = true;
                    Console.WriteLine("Finished processing scenery placement data.");
                }
            }
        }

        foreach (XmlNode location in trig_vol_block)
        {
            bool vols_end = false;
            int i = 0;
            while (!vols_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = location.SelectSingleNode(search_string);
                if (element != null)
                {
                    string name = element.SelectSingleNode("./field[@name='name']").InnerText.Trim();
                    string xyz = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();
                    string ext = element.SelectSingleNode("./field[@name='extents']").InnerText.Trim();
                    string fwd = element.SelectSingleNode("./field[@name='forward']").InnerText.Trim();
                    string up = element.SelectSingleNode("./field[@name='up']").InnerText.Trim();

                    all_trig_vols.Add(new TrigVol
                    {
                        vol_name = name,
                        vol_xyz = xyz,
                        vol_ext = ext,
                        vol_fwd = fwd,
                        vol_up = up
                    });

                    i++;
                }
                else
                {
                    vols_end = true;
                    Console.WriteLine("Finished processing trigger volume data.");
                }
            }
        }


        ManagedBlamHandler(all_starting_locs, all_weapon_locs, all_scen_types, all_scen_entries, all_trig_vols, h3ek_path, scen_path);
    }

    static void ManagedBlamHandler(List<StartLoc> spawn_data, List<WeapLoc> weap_data, List<TagPath> all_scen_types, List<Scenery> all_scen_entries, List<TrigVol> all_trig_vols, string h3ek_path, string scen_path)
    {
        // Weapons dictionary
        Dictionary<string, TagPath> weapMapping = new Dictionary<string, TagPath>
        {
            {"frag_grenades", TagPath.FromPathAndType(@"objects\weapons\grenade\frag_grenade\frag_grenade", "eqip*")},
            {"plasma_grenades", TagPath.FromPathAndType(@"objects\weapons\grenade\plasma_grenade\plasma_grenade", "eqip*")},
            {"energy_sword", TagPath.FromPathAndType(@"objects\weapons\melee\energy_sword\energy_sword", "weap*")},
            {"magnum", TagPath.FromPathAndType(@"objects\weapons\pistol\magnum\magnum", "weap*")},
            {"needler", TagPath.FromPathAndType(@"objects\weapons\pistol\needler\needler", "weap*")},
            {"plasma_pistol", TagPath.FromPathAndType(@"objects\weapons\pistol\plasma_pistol\plasma_pistol", "weap*")},
            {"battle_rifle", TagPath.FromPathAndType(@"objects\weapons\rifle\battle_rifle\battle_rifle", "weap*")},
            {"beam_rifle", TagPath.FromPathAndType(@"objects\weapons\rifle\beam_rifle\beam_rifle", "weap*")},
            {"carbine", TagPath.FromPathAndType(@"objects\weapons\rifle\covenant_carbine\covenant_carbine", "weap*")},
            {"plasma_rifle", TagPath.FromPathAndType(@"objects\weapons\rifle\plasma_rifle\plasma_rifle", "weap*")},
            {"brute_plasma_rifle", TagPath.FromPathAndType(@"objects\weapons\rifle\plasma_rifle_red\plasma_rifle_red", "weap*")},
            {"shotgun", TagPath.FromPathAndType(@"objects\weapons\rifle\shotgun\shotgun", "weap*")},
            {"smg", TagPath.FromPathAndType(@"objects\weapons\rifle\smg\smg", "weap*")},
            {"smg_silenced", TagPath.FromPathAndType(@"objects\weapons\rifle\smg_silenced\smg_silenced", "weap*")},
            {"sniper_rifle", TagPath.FromPathAndType(@"objects\weapons\rifle\sniper_rifle\sniper_rifle", "weap*")},
            {"rocket_launcher", TagPath.FromPathAndType(@"objects\weapons\support_high\rocket_launcher\rocket_launcher", "weap*")},
            {"fuel_rod_gun", TagPath.FromPathAndType(@"objects\weapons\support_high\flak_cannon\flak_cannon", "weap*")},
            {"sentinel_beam", TagPath.FromPathAndType(@"objects\weapons\support_low\sentinel_gun\sentinel_gun", "weap*")},
            {"brute_shot", TagPath.FromPathAndType(@"objects\weapons\support_low\brute_shot\brute_shot", "weap*")},
        };



        // Variables
        var tag_path = TagPath.FromPathAndType(Path.ChangeExtension(scen_path.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");
        var respawn_scen_path = TagPath.FromPathAndType(@"objects\multi\spawning\respawn_point", "scen*");
        int respawn_scen_index = 0;

        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);

        using (var tagFile = new TagFile(tag_path))
        {
            // Spawns Section
            int i = 0;
            int temp_index = 0;
            bool respawn_found = false;
            int totalScenCount = 0;

            // Add respawn point scenery, if it doesn't already exist
            if (((TagFieldBlock)tagFile.Fields[21]).Elements.Count() != 0)
            {
                foreach (var scen_type in ((TagFieldBlock)tagFile.Fields[21]).Elements)
                {
                    if (((TagFieldReference)scen_type.Fields[0]).Path == respawn_scen_path)
                    {
                        // Respawn point scenery already in palette, set index
                        respawn_scen_index = temp_index;
                        respawn_found = true;
                        break;
                    }
                    temp_index++;
                }
                if (respawn_found == false)
                {
                    // Respawn point is not in the palette, add it and set the index
                    respawn_scen_index = ((TagFieldBlock)tagFile.Fields[21]).Elements.Count();
                    ((TagFieldBlock)tagFile.Fields[21]).AddElement();
                    var scen_tag = (TagFieldReference)((TagFieldBlock)tagFile.Fields[21]).Elements[respawn_scen_index].Fields[0];
                    scen_tag.Path = respawn_scen_path;
                    totalScenCount++;
                }
            }
            else
            {
                Console.WriteLine("No existing sceneries, adding respawn point");
                ((TagFieldBlock)tagFile.Fields[21]).AddElement();
                var scen_tag = (TagFieldReference)((TagFieldBlock)tagFile.Fields[21]).Elements[0].Fields[0];
                scen_tag.Path = respawn_scen_path;
                totalScenCount++;
            }
            


            foreach (var spawn in spawn_data)
            {
                // Add new
                ((TagFieldBlock)tagFile.Fields[20]).AddElement();

                // Type
                var scen_type = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[1];
                scen_type.Value = respawn_scen_index;

                // Dropdown type and source (won't be valid without these)
                var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                dropdown_type.Value = 6; // 6 for scenery
                dropdown_source.Value = 1; // 1 for editor

                // Position
                var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[0].FieldName;
                var xyz_pos = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[2];
                xyz_pos.Data = spawn.position_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Rotation
                var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[3];
                string angle_xyz = spawn.facing_angle + ",0,0";
                rotation.Data = angle_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Team
                var z = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[7]).Elements[0].Fields[0].FieldName;
                var team = (TagFieldEnum)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[7]).Elements[0].Fields[3];
                team.Value = int.Parse(new string(spawn.team_enum.TakeWhile(c => c != ',').ToArray()));


                i++;
            }

            Dictionary<string, int> weapPaletteMapping = new Dictionary<string, int>();

            // Weapons Section
            foreach (var weapon in weap_data)
            {
                string weap_type = weapon.weap_type.Split('\\')[weapon.weap_type.Split('\\').Length - 1];

                if (weap_type == "frag_grenades" || weap_type == "plasma_grenades")
                {
                    // Grenade stuff, need to treat as equipment not weapon
                    Console.WriteLine("Adding " + weap_type + " equipment");

                    // Equipment, check if palette entry exists first
                    Console.WriteLine("Adding " + weap_type + " equipment");
                    bool equip_entry_exists = false;
                    foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[27]).Elements)
                    {
                        var temp_type = weapMapping[weap_type];
                        if (((TagFieldReference)palette_entry.Fields[0]).Path == temp_type)
                        {
                            equip_entry_exists = true;
                        }
                    }

                    // Add palette entry if needed
                    if (!equip_entry_exists)
                    {
                        int current_count = ((TagFieldBlock)tagFile.Fields[27]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[27]).AddElement();
                        var equip_tag_ref = (TagFieldReference)((TagFieldBlock)tagFile.Fields[27]).Elements[current_count].Fields[0];
                        equip_tag_ref.Path = weapMapping[weap_type];
                        weapPaletteMapping.Add(weap_type, current_count);
                    }

                    // Now add the equipment itself
                    int equip_count = ((TagFieldBlock)tagFile.Fields[26]).Elements.Count();
                    ((TagFieldBlock)tagFile.Fields[26]).AddElement(); // Add new weapon entry

                    // XYZ
                    var equip_xyz = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[2];
                    equip_xyz.Data = weapon.weap_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Rotation
                    var equip_orient = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[3];
                    equip_orient.Data = weapon.weap_orient.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Type
                    var equip_tag = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[1];
                    equip_tag.Value = weapPaletteMapping[weap_type];

                    // Spawn timer
                    var equip_stime = (TagFieldElementInteger)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[6]).Elements[0].Fields[8];
                    equip_stime.Data = uint.Parse(weapon.spawn_time);

                    // Dropdown type and source (won't be valid without these)
                    var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                    var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                    dropdown_type.Value = 3; // 2 for equipment
                    dropdown_source.Value = 1; // 1 for editor

                    continue;
                }
                if (weap_type == "")
                {
                    // All games entries, ignore
                    Console.WriteLine("Ignoring blank weapon collection");
                    continue;
                }
                else
                {
                    // Weapon, check if palette entry exists first
                    Console.WriteLine("Adding " + weap_type + " weapon");
                    bool weap_entry_exists = false;
                    foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[29]).Elements)
                    {
                        var temp_type = weapMapping[weap_type];
                        if (((TagFieldReference)palette_entry.Fields[0]).Path == temp_type)
                        {
                            weap_entry_exists = true;
                        }
                    }

                    // Add palette entry if needed
                    if (!weap_entry_exists)
                    {
                        int current_count = ((TagFieldBlock)tagFile.Fields[29]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[29]).AddElement();
                        var weap_tag_ref = (TagFieldReference)((TagFieldBlock)tagFile.Fields[29]).Elements[current_count].Fields[0];
                        weap_tag_ref.Path = weapMapping[weap_type];
                        weapPaletteMapping.Add(weap_type, current_count);
                    }

                    // Now add the weapon itself
                    int weapon_count = ((TagFieldBlock)tagFile.Fields[28]).Elements.Count();
                    ((TagFieldBlock)tagFile.Fields[28]).AddElement(); // Add new weapon entry

                    // XYZ
                    var weap_xyz = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[4]).Elements[0].Fields[2];
                    weap_xyz.Data = weapon.weap_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Rotation
                    var weap_orient = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[4]).Elements[0].Fields[3];
                    weap_orient.Data = weapon.weap_orient.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Type
                    var weap_tag = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[1];
                    weap_tag.Value = weapPaletteMapping[weap_type];

                    // Spawn timer
                    var weap_stime = (TagFieldElementInteger)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[7]).Elements[0].Fields[8];
                    weap_stime.Data = uint.Parse(weapon.spawn_time);

                    // Dropdown type and source (won't be valid without these)
                    var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                    var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                    dropdown_type.Value = 2; // 2 for weapon
                    dropdown_source.Value = 1; // 1 for editor
                }
            }

            // Scenery Section - the idea is to place blank scenery with bad references so they can be easily changed to ported versions by the user

            foreach (TagPath scen_type in all_scen_types)
            {
                // Check if current type exists in palette
                bool type_exists_already = false;
                foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[21]).Elements)
                {
                    var x = ((TagFieldReference)palette_entry.Fields[0]).Path;
                    if (x == scen_type)
                    {
                        type_exists_already = true;
                        break;
                    }
                }

                // Add palette entry if needed
                if (!type_exists_already)
                {
                    int current_count = ((TagFieldBlock)tagFile.Fields[21]).Elements.Count();
                    ((TagFieldBlock)tagFile.Fields[21]).AddElement();
                    var scen_type_ref = (TagFieldReference)((TagFieldBlock)tagFile.Fields[21]).Elements[current_count].Fields[0];
                    scen_type_ref.Path = scen_type;
                }
            }

            // Now add all of the scenery placements
            foreach (Scenery scenery in all_scen_entries)
            {
                int current_count = ((TagFieldBlock)tagFile.Fields[20]).Elements.Count();
                ((TagFieldBlock)tagFile.Fields[20]).AddElement();
                var type_ref = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[1];
                int index = int.Parse(scenery.scen_type.ToString()) + totalScenCount;
                type_ref.Value = int.Parse(scenery.scen_type) + totalScenCount;

                // Dropdown type and source (won't be valid without these)
                var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                dropdown_type.Value = 6; // 6 for scenery
                dropdown_source.Value = 1; // 1 for editor

                // Position
                var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[0].FieldName;
                var xyz_pos = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[2];
                xyz_pos.Data = scenery.scen_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Rotation
                var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[3];
                rotation.Data = scenery.scen_orient.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Variant
                var z = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[5]).Elements[0].Fields[0].FieldName;
                var variant = (TagFieldElementStringID)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[5]).Elements[0].Fields[0];
                variant.Data = scenery.scen_vrnt;
            }

            // Trigger volumes section
            foreach (TrigVol vol in all_trig_vols)
            {
                int current_count = ((TagFieldBlock)tagFile.Fields[55]).Elements.Count();
                ((TagFieldBlock)tagFile.Fields[55]).AddElement();

                // Name
                var name = (TagFieldElementStringID)((TagFieldBlock)tagFile.Fields[55]).Elements[current_count].Fields[0];
                name.Data = vol.vol_name;

                // Forward
                var fwd = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[55]).Elements[current_count].Fields[4];
                fwd.Data = vol.vol_fwd.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Up
                var up = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[55]).Elements[current_count].Fields[5];
                up.Data = vol.vol_up.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Position
                var xyz = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[55]).Elements[current_count].Fields[6];
                xyz.Data = vol.vol_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Extents
                var ext = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[55]).Elements[current_count].Fields[7];
                ext.Data = vol.vol_ext.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();
            }

            tagFile.Save();

            Console.WriteLine("Finished");
        }
    }
}
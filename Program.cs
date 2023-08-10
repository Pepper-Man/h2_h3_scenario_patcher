using Bungie;
using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

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
        scen_path = @"C:\Program Files (x86)\Steam\steamapps\common\H3EK\tags\halo_2\levels\elongation\elongation.scenario";
        xml_path = @"G:\Steam\steamapps\common\H2EK\elongation_output.xml";

        string h3ek_path = scen_path.Substring(0, scen_path.IndexOf("H3EK") + "H3EK".Length);

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

            Console.WriteLine("Modified file saved successfully.\n\nPreparing to patch tag data.\n\nLoaded zones:\n");
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

        List<StartLoc> all_starting_locs = new List<StartLoc>();

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

        List<string> all_data = new List<string>();
        ManagedBlamHandler(all_data, h3ek_path, scen_path);
    }

    static void ManagedBlamHandler(List<string> all_data, string h3ek_path, string scen_path)
    {
        // Variables
        var tag_path = Bungie.Tags.TagPath.FromPathAndType(Path.ChangeExtension(scen_path.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");

        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);
        Console.WriteLine("\nSaving tag...\n");

        using (var tagFile = new Bungie.Tags.TagFile(tag_path))
        {
            tagFile.Save();
        }
    }
}
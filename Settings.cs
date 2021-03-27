using Godot;
using System;
using Godot.Collections;

public class Settings : Node
{

    float render_distance = 7;
    bool fog_enabled = true;

    public static int world_type = 0;

    String _save_path = "user://settings.json";
    static bool _loaded = false;


    public override void _EnterTree()
    {

        if (Settings._loaded)
        {
            GD.PrintErr("Error: Settings is an AutoLoad singleton and it shouldn't be instanced elsewhere.");

            GD.PrintErr("Please delete the instance at: " + GetPath());
        }
        else
            Settings._loaded = true;



        var file = new Godot.File();


        if (file.FileExists(_save_path))
        {
            file.Open(_save_path, File.ModeFlags.Read);
            string text = file.GetAsText();
            var jsonFile =  JSON.Parse(text).Result;
            Dictionary ParsedData = jsonFile as Dictionary;
            file.Close();

            try
            {

                render_distance = (float)ParsedData["render_distance"];

                fog_enabled = (bool) ParsedData["fog_enabled"];
            }
            catch (Exception ex) {
                GD.PrintErr(ex);
            }
        }
        else
            save_settings();
    }


    public void save_settings()
    {
        GD.Print("save_settings is not implemented yet.");

        var file = new Godot.File();

        file.Open(_save_path, File.ModeFlags.Write);

        var data = new Dictionary();
        
        data["render_distance"] = render_distance;
		data["fog_enabled"] = fog_enabled;
        String text = JSON.Print(data);
        file.StoreString(text);
        file.Close();
    }

}

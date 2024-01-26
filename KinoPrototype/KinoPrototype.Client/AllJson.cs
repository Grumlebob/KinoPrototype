using Newtonsoft.Json;
using System.Collections.Generic;

public class Root
{
    [JsonProperty("content")]
    public ContentLevel1 Content { get; set; }
}

public class ContentLevel1
{
    [JsonProperty("content")]
    public ContentLevel2 Content { get; set; }
}

public class ContentLevel2
{
    [JsonProperty("content")]
    public ContentLevel3 Content { get; set; }
}

public class ContentLevel3
{
    [JsonProperty("content")]
    public List<ContentLevel4> Content { get; set; }
}

public class ContentLevel4
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("movies")]
    public List<Movie> Movies { get; set; }
}

public class Movie
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("versions")]
    public List<Version> Versions { get; set; }

    [JsonProperty("content")]
    public MovieContent Content { get; set; }
}

public class Version
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("label")]
    public string Label { get; set; }

    [JsonProperty("dates")]
    public List<ShowtimeDate> Dates { get; set; }
}

public class ShowtimeDate
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("date")]
    public string Date { get; set; }

    [JsonProperty("showtimes")]
    public List<ShowtimeItem> Showtimes { get; set; }
}

public class ShowtimeItem
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("available_seats")]
    public int AvailableSeats { get; set; }

    [JsonProperty("time")]
    public string Time { get; set; }

    [JsonProperty("room")]
    public Room Room { get; set; }
}

public class Room
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("label")]
    public string Label { get; set; }
}

public class MovieContent
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("label")]
    public string Label { get; set; }

    [JsonProperty("field_censorship_icon")]
    public string FieldCensorshipIcon { get; set; }

    // Add other properties as per the JSON structure

    [JsonProperty("field_poster")]
    public FieldPoster FieldPoster { get; set; }

    // Define other properties as needed
}

public class FieldPoster
{
    [JsonProperty("type")]
    public string Type { get; set; }

    // Include other properties as per the JSON structure
    // For example, "entity_type", "bundle", "id", "label", etc.

    [JsonProperty("field_media_image")]
    public FieldMediaImage FieldMediaImage { get; set; }

    // Define other properties as needed
}

public class FieldMediaImage
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("responsive_image_style_id")]
    public string ResponsiveImageStyleId { get; set; }

    [JsonProperty("width")]
    public int Width { get; set; }

    [JsonProperty("height")]
    public int Height { get; set; }

    // Include other properties like "attributes", "sources", etc. TODO NÅEDE HERTIL
}

public class Advertisement
{
}

public class Breadcrumbs
{
}


public class Facets
{
}

public class Footer
{
}

public class Header
{
}

public class Initial
{
}

public class Pager
{
}


public class Universe
{
}


public static class JsonParser
{
    public static List<string> GetMoviesFromJson()
    {
        string
            relativePath =
                @"..\KinoPrototype.Client\mini5.json"; // Navigates up one level from AllJson to KinoPrototype.Client, then to tester.json
        string json = File.ReadAllText(relativePath);

        var result = new List<string>();
        Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(json);

        if (myDeserializedClass.Content != null)
        {
            Console.WriteLine("Level 1 Content is not null");
            if (myDeserializedClass.Content.Content != null)
            {
                Console.WriteLine("Level 2 Content is not null");
                if (myDeserializedClass.Content.Content.Content != null)
                {
                    Console.WriteLine("Level 3 Content is not null");
                    if (myDeserializedClass.Content.Content.Content.Content != null)
                    {
                        Console.WriteLine("Level 4 Content is not null");
                        Console.WriteLine("Parsed count: " + myDeserializedClass.Content.Content.Content.Content);
                        foreach (var item in myDeserializedClass.Content.Content.Content.Content)
                        {
                            foreach (var movie in item.Movies)
                            {
                                Console.WriteLine("movie label: " +movie.Content.Label);
                                Console.WriteLine("movie har poster image med højde: " + movie.Content.FieldPoster.FieldMediaImage.Height);
                                
                                foreach (var versions in movie.Versions)
                                {
                                    Console.WriteLine("version label: " + versions.Label);
                                    foreach (var showtimeDate in versions.Dates)
                                    {
                                        Console.WriteLine("Specific showtime " + showtimeDate.Date);
                                        foreach (var showtimeItem in showtimeDate.Showtimes)
                                        {
                                            Console.Write(" at time: " + showtimeItem.Time);
                                            Console.Write(" Seats left: " + showtimeItem.AvailableSeats);
                                            Console.Write(" at sal " + showtimeItem.Room.Label);
                                            Console.WriteLine(" ");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Level 4 Content is null");
                    }
                }
                else
                {
                    Console.WriteLine("Level 3 Content is null");
                }
            }
            else
            {
                Console.WriteLine("Level 2 Content is null");
            }
        }
        else
        {
            Console.WriteLine("Level 1 Content is null");
        }

        return result;
    }
}
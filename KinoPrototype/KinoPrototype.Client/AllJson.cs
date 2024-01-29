using Newtonsoft.Json;
using System.Collections.Generic;

public class Root
{
    [JsonProperty("content")] public ContentLevel1 Content { get; set; }
}

public class ContentLevel1
{
    [JsonProperty("content")] public ContentLevel2 Content { get; set; }
}

public class ContentLevel2
{
    [JsonProperty("content")] public ContentLevel3 Content { get; set; }

    [JsonProperty("facets")] public Facets Facets { get; set; }
}


public class ContentLevel3
{
    [JsonProperty("content")] public List<ContentLevel4> Content { get; set; }
}

public class ContentLevel4
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("id")] public int Id { get; set; }

    [JsonProperty("movies")] public List<Movie> Movies { get; set; }
}

public class Movie
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("id")] public int Id { get; set; }

    [JsonProperty("versions")] public List<Version> Versions { get; set; }

    [JsonProperty("content")] public MovieContent Content { get; set; }
}

public class Version
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("label")] public string Label { get; set; }

    [JsonProperty("dates")] public List<ShowtimeDate> Dates { get; set; }
}

public class ShowtimeDate
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("date")] public string Date { get; set; }

    [JsonProperty("showtimes")] public List<ShowtimeItem> Showtimes { get; set; }
}

public class ShowtimeItem
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("id")] public int Id { get; set; }

    [JsonProperty("available_seats")] public int AvailableSeats { get; set; }

    [JsonProperty("time")] public string Time { get; set; }

    [JsonProperty("room")] public Room Room { get; set; }
}

public class Room
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("id")] public int Id { get; set; }

    [JsonProperty("label")] public string Label { get; set; }
}

public class MovieContent
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("label")] public string Label { get; set; }

    [JsonProperty("field_censorship_icon")]
    public string FieldCensorshipIcon { get; set; }

    [JsonProperty("field_playing_time")] public string FieldPlayingTime { get; set; }

    [JsonProperty("field_poster")] public FieldPoster FieldPoster { get; set; }
    
    [JsonProperty("field_premiere")] public string FieldPremiere { get; set; }

}

public class FieldPoster
{
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("field_media_image")] public FieldMediaImage FieldMediaImage { get; set; }
}

public class FieldMediaImage
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("responsive_image_style_id")]
    public string ResponsiveImageStyleId { get; set; }

    [JsonProperty("width")] public int Width { get; set; }

    [JsonProperty("height")] public int Height { get; set; }

    [JsonProperty("sources")] public List<SourceItem> Sources { get; set; } // Added this line
}

public class SourceItem
{
    [JsonProperty("srcset")] public string Srcset { get; set; }

    [JsonProperty("media")] public string Media { get; set; }

    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("width")] public int? Width { get; set; } // Nullable because it might not always be present

    [JsonProperty("height")] public int? Height { get; set; }
}

public class Cinemas
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("title")] public string Title { get; set; }

    //ignore this json
    [JsonIgnore]
    [JsonProperty("default_value")] public string DefaultValue { get; set; }
    [JsonProperty("options")] public List<CinemaOption> Options { get; set; }
    
}

public class CinemaOption
{
    [JsonProperty("key")] public int Key { get; set; }

    [JsonProperty("value")] public string Value { get; set; }
}

public class Advertisement
{
}

public class Breadcrumbs
{
}


public class Facets
{
    [JsonProperty("sort")] public object Sort { get; set; } // Replace object with specific type if necessary

    [JsonProperty("city")] public object City { get; set; } // Replace object with specific type if necessary

    [JsonProperty("cinemas")] public Cinemas Cinemas { get; set; }

    [JsonProperty("movies")] public object Movies { get; set; } // Replace object with specific type if necessary

    [JsonProperty("versions")] public object Versions { get; set; } // Replace object with specific type if necessary

    [JsonProperty("genres")] public object Genres { get; set; } // Replace object with specific type if necessary

    [JsonProperty("date")] public object Date { get; set; } // Replace object with specific type if necessary
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
                @"..\KinoPrototype.Client\mini11.json"; // Navigates up one level from AllJson to KinoPrototype.Client, then to tester.json
        string json = File.ReadAllText(relativePath);

        var CinemaIdAndName = new Dictionary<int, string>();
        var CinemaFilters = new HashSet<int>();
        var result = new List<string>();
        Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(json);

        if (myDeserializedClass.Content != null)
        {
            if (myDeserializedClass.Content.Content != null)
            {
                if (myDeserializedClass.Content.Content.Content != null)
                {
                    foreach (var cinemaOption in myDeserializedClass.Content.Content.Facets.Cinemas.Options)
                    {
                        CinemaIdAndName.Add(cinemaOption.Key, cinemaOption.Value);
                    }

                    if (myDeserializedClass.Content.Content.Content.Content != null)
                    {
                        foreach (var cinema in myDeserializedClass.Content.Content.Content.Content)
                        {
                            //if (!CinemaFilters.Contains(cinema.Id)) continue; //their cinema filter doesn't work, instead get all cinemas and filter them out here
                            Console.WriteLine("Cinema name: " + CinemaIdAndName[cinema.Id]);
                            foreach (var movie in cinema.Movies)
                            {
                                Console.WriteLine("movie label: " + movie.Content.Label);
                                Console.WriteLine("Premiere: " + movie.Content.FieldPremiere);
                                Console.WriteLine("Poster url: " +
                                                  movie.Content.FieldPoster.FieldMediaImage.Sources[0].Srcset);
                                Console.WriteLine("Playing time: " + movie.Content.FieldPlayingTime);

                                foreach (var versions in movie.Versions)
                                {
                                    Console.WriteLine("-- Version label: " + versions.Label);
                                    foreach (var showtimeDate in versions.Dates)
                                    {
                                        Console.Write("Date " + showtimeDate.Date);
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
//https://kino.dk/ticketflow/showtimes?cinemas=53&movies=35883&sort=most_purchased
//https://api.kino.dk/ticketflow/showtimes?cinemas=53&movies=35883&sort=most_purchased?region=content&format=json

public static class JsonParser
{
    public static string getJsonString()
    {
        string
            relativePath =
                @"..\KinoPrototype.Client\mini11.json"; // Navigates up one level from AllJson to KinoPrototype.Client, then to tester.json
        string json = File.ReadAllText(relativePath);

        return json;
    }
    
}
//https://kino.dk/ticketflow/showtimes?cinemas=53&movies=35883&sort=most_purchased
//https://api.kino.dk/ticketflow/showtimes?cinemas=53&movies=35883&sort=most_purchased?region=content&format=json
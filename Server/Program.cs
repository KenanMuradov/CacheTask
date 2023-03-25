using ModelsDLL;
using Server.Contexts;
using System.Net;
using System.Text.Json;

using var listener = new HttpListener();

listener.Prefixes.Add(@"http://localhost:27001/");
listener.Prefixes.Add(@"http://localhost:27002/");


listener.Start();

while (true)
{
    var context = await listener.GetContextAsync();

    var request = context.Request;

    if (request == null)
        continue;

    switch (request.HttpMethod)
    {
        case "GET":
            {
                var response = context.Response;


                var key = request.QueryString["key"].ToCharArray()[0];

                var dbContext = new CacheDbContext();

                var x = dbContext.Find<KeyValue>(key);

                if (x is not null)
                {
                    response.ContentType = "application/json";

                    response.StatusCode = (int)HttpStatusCode.OK;

                    var keyValue = x;
                    var jsonStr = JsonSerializer.Serialize(keyValue);

                    var writer = new StreamWriter(response.OutputStream);
                    await writer.WriteAsync(jsonStr);
                }
                else
                    response.StatusCode = (int)HttpStatusCode.NotFound;


                response.Close();

                break;
            }
        case "POST":
            {
                var stream = request.InputStream;
                var reader = new StreamReader(stream);

                var jsonStr = reader.ReadToEnd();

                Console.WriteLine(jsonStr);

                var keyValue = JsonSerializer.Deserialize<KeyValue>(jsonStr);

                var response = context.Response;

                try
                {
                    var dbContext = new CacheDbContext();

                    if (dbContext.Find<KeyValue>(keyValue.Key) == null)
                    {
                        dbContext.Add(keyValue);
                        dbContext.SaveChanges();
                        response.StatusCode = (int)HttpStatusCode.OK;

                    }
                    else
                        response.StatusCode = (int)HttpStatusCode.Found;

                    response.Close();

                }
                catch (Exception)
                {

                    throw;
                }


                break;
            }

        case "PUT":
            {
                var stream = request.InputStream;
                var reader = new StreamReader(stream);

                var jsonStr = reader.ReadToEnd();

                Console.WriteLine(jsonStr);

                var keyValue = JsonSerializer.Deserialize<KeyValue>(jsonStr);

                var response = context.Response;

                try
                {
                    var dbContext = new CacheDbContext();
                    var x = dbContext.Find<KeyValue>(keyValue.Key);
                    if (x != null)
                    {
                        x.Value = keyValue.Value;
                        dbContext.SaveChanges();
                        response.StatusCode = (int)HttpStatusCode.OK;

                    }
                    else
                        response.StatusCode = (int)HttpStatusCode.NotFound;

                    response.Close();

                }
                catch (Exception)
                {

                    throw;
                }
                break;
            }

    }

}
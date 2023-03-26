using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ModelsDLL;
using Server.Contexts;
using System.Net;
using System.Text.Json;

Dictionary<string, int> requestCounts = new();

using var client = new HttpClient();
using var listener = new HttpListener();

listener.Prefixes.Add("http://localhost:27001/");

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

                var key = request.QueryString["key"];

                ArgumentNullException.ThrowIfNull(key, nameof(key));

                if (!requestCounts.ContainsKey(key))
                    requestCounts[key] = 0;

                requestCounts[key]++;

                if (requestCounts[key] >= 3)
                {
                    var clientResponse = await client.GetAsync($"http://localhost:27002/?key={key}");

                    if (clientResponse.StatusCode == HttpStatusCode.OK)
                    {
                        response.ContentType = "application/json";

                        response.StatusCode = (int)HttpStatusCode.OK;



                        var jsonStr = await clientResponse.Content.ReadAsStringAsync();

                        var writer = new StreamWriter(response.OutputStream);
                        await writer.WriteAsync(jsonStr);
                        writer.Flush();


                    }
                    else
                    {
                        var dbContext = new CacheDbContext();

                        var x = dbContext.Find<KeyValue>(key);

                        if (x is not null)
                        {
                            response.ContentType = "application/json";

                            response.StatusCode = (int)HttpStatusCode.OK;

                            var keyValue = x;
                            var jsonStr = JsonSerializer.Serialize(x);

                            var writer = new StreamWriter(response.OutputStream);
                            await writer.WriteAsync(jsonStr);
                            writer.Flush();

                            var content = new StringContent(jsonStr);
                            var responseMessage = await client.PostAsync("http://localhost:27002/", content);
                        }
                        else
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                else
                {
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
                        writer.Flush();

                    }
                    else
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                }

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

                var dbContext = new CacheDbContext();
                var key = keyValue.Key;

                if (dbContext.Find<KeyValue>(key) == null)
                {
                    if (!requestCounts.ContainsKey(key))
                        requestCounts[key] = 0;

                    requestCounts[key]++;

                    dbContext.Add(keyValue);
                    dbContext.SaveChanges();
                    response.StatusCode = (int)HttpStatusCode.OK;

                }
                else
                    response.StatusCode = (int)HttpStatusCode.Found;

                response.Close();

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
                        if (requestCounts[x.Key] >= 3)
                        {
                            var content = new StringContent(jsonStr);
                            var responseMessage = await client.PutAsync("http://localhost:27002/", content);
                        }

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
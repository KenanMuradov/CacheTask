using ModelsDLL;
using System.Net;
using System.Text.Json;

List<KeyValue> keyValues = new();

using var listener = new HttpListener();


listener.Prefixes.Add("http://localhost:27002/");

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

                var x = keyValues.FirstOrDefault(kv => kv.Key == key);

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
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                response.Close();
                break;
            }
        case "POST":
            {
                var response = context.Response;


                var stream = request.InputStream;
                var reader = new StreamReader(stream);

                var jsonStr = reader.ReadToEnd();

                var keyValue = JsonSerializer.Deserialize<KeyValue>(jsonStr);

                if (keyValue is not null)
                {

                    keyValues.Add(keyValue);


                    response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                    response.StatusCode = (int)HttpStatusCode.BadRequest;

                response.Close();
                break;

            }

        case "PUT":
            {

                break;
            }

    }
}
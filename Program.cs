using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.Intrinsics.X86;
using WireMock.Logging;
using WireMock.Matchers;
using WireMock.Net;
using WireMock.Net.StandAlone;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

internal class Program
{
    private static readonly ILogger XLogger = LoggerFactory.Create(o =>
    {
        o.SetMinimumLevel(LogLevel.Debug);
        o.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = false;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss ";
        });
    }).CreateLogger("WireMock.Net");

    private static readonly IWireMockLogger Logger = new WireMockLogger(XLogger);

    private static void Main(string[] args)
    {
        // see source code for all the possible properties
        var settings = new WireMockServerSettings
        {
            AllowPartialMapping = true,
            StartAdminInterface = true,
            Logger = Logger,
            Port = 8080,
            ReadStaticMappings = true
        };

        var server = StandAloneApp.Start(settings);

        //Response templating with Handlebars
        server.Given(Request.Create().WithPath("/templating").UsingGet())
        .RespondWith(Response.Create()
        .WithStatusCode(200)
        .WithHeader("Content-Type", MediaTypeNames.Application.Json)
        .WithBody(@"{ ""name"": ""{{request.path.[1]}}"" }"));

        server
        .Given(Request.Create()
            .WithPath("/xpath").UsingPost()
            .WithBody(new XPathMatcher("/todo-list[count(todo-item) = 3]"))
        )
        .RespondWith(Response.Create().WithBody("XPathMatcher!"));

        server
        .Given(Request
        .Create()
        .WithPath("/jsonpartialmatcher1")
        .WithBody(new JsonPartialMatcher("{ \"test\": \"abc\" }"))
        .UsingPost())
        .WithGuid("debaf408-3b23-4c04-9d18-ef1c020e79f2")
        .RespondWith(Response.Create().WithBody(@"{ ""result"": ""jsonpartialbodytest1"" }"));

        //Create basic stubbing
        server.Given(Request.Create().WithPath("/test").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("Hello world!"));


        //Create stubbing with response body
        server.Given(Request.Create().WithPath("/test3").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", MediaTypeNames.Application.Json)
                .WithBodyAsJson(new
                {
                    name = "test",
                    id = 1
                }));

        //Create stubbing with response body
        server.Given(Request.Create().WithPath("/test4").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", MediaTypeNames.Application.Json)
                .WithBodyAsJson(new[]
                {
                    new
                    {
                        name = "test",
                        id = 1
                    },
                    new
                    {
                        name = "test2",
                        id = 2
                    }
                }));

        //Create stubbing with response body
        server.Given(Request.Create().WithPath("/test5").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", MediaTypeNames.Application.Json)
                .WithBodyAsJson(new
                {
                    name = "test",
                    id = 1,
                    sub = new
                    {
                        name = "test",
                        id = 1
                    }
                }));


        Console.WriteLine("Press any key to stop the server");
        Console.ReadKey();
    }
}
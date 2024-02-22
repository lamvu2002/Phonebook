using Phonebook.WebApi;

Console.WriteLine("This web server is running...");
Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder
        .UseKestrel()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .UseWebRoot(Directory.GetCurrentDirectory())
        .UseStartup<Startup>();
    })
    .Build()
    .Run();


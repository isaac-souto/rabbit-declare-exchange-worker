using Polly;
using RabbitDeclareExchangeWorker;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton((sp) => new ConnectionFactory()
{
    Uri = new Uri("amqp://mc:mc2@rabbitmq:5672/main"),
    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
    AutomaticRecoveryEnabled = true,
    DispatchConsumersAsync = true
});

builder.Services.AddSingleton(sp => Policy
        .Handle<BrokerUnreachableException>()
        .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
        .Execute(() => sp.GetRequiredService<ConnectionFactory>().CreateConnection()));

builder.Services.AddTransient((sp) => sp.GetRequiredService<IConnection>().CreateModel());

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

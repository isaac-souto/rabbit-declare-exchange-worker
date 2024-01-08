using RabbitMQ.Client;

namespace RabbitDeclareExchangeWorker
{
    public class Worker(IModel model) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DeclareOrderCreateExchange();

            DeclareExchangeSystem1();

            DeclareExchangeSystem2();

            using PeriodicTimer Timer = new(TimeSpan.FromSeconds(1));

            while (!stoppingToken.IsCancellationRequested) await Timer.WaitForNextTickAsync(stoppingToken);
        }

        private void DeclareOrderCreateExchange()
        {
            model.ExchangeDelete("order.create.unrouted");
            model.ExchangeDelete("order.create");

            model.QueueDelete("topic.order.create.unrouted");

            model.ExchangeDeclare("order.create.unrouted", "fanout", true, false);
            model.QueueDeclare("order.create.unrouted", true, false, false, new Dictionary<string, object> { { "x-queue-type", "quorum" } });
            model.QueueBind("order.create.unrouted", "order.create.unrouted", string.Empty);

            model.ExchangeDeclare("order.create", "headers", true, false, new Dictionary<string, object>
            {
                { "alternate-exchange", "order.create.unrouted" }
            });
        }
        
        private void DeclareExchangeSystem1()
        {
            model.ExchangeDelete("system1.order.create.deadletter");
            model.ExchangeDelete("system1.order.create");

            model.QueueDelete("system1.order.create.deadletter");
            model.QueueDelete("system1.order.create");
            
            model.ExchangeDeclare("system1.order.create.deadletter", "fanout", true, false);
            model.QueueDeclare("system1.order.create.deadletter", true, false, false, new Dictionary<string, object> { { "x-queue-type", "quorum" } });
            model.QueueBind("system1.order.create.deadletter", "system1.order.create.deadletter", string.Empty);

            model.ExchangeDeclare("system1.order.create", "fanout", true, false);
            model.QueueDeclare("system1.order.create", true, false, false, new Dictionary<string, object>
            {
                { "x-queue-type", "quorum" },
                { "x-dead-letter-exchange", "system1.order.create.deadletter" },
                { "x-delivery-limit", 3 }
            });
            model.QueueBind("system1.order.create", "system1.order.create", string.Empty);

            model.ExchangeBind("system1.order.create", "order.create", string.Empty);
        }

        private void DeclareExchangeSystem2()
        {
            model.ExchangeDelete("system2.order.create.unrouted");
            model.ExchangeDelete("system2.order.create.deadletter");
            model.ExchangeDelete("system2.order.create");

            model.QueueDelete("system2.order.create.unrouted");
            model.QueueDelete("system2.order.create.deadletter");
            model.QueueDelete("system2.order.create");

            model.ExchangeDeclare("system2.order.create.deadletter", "fanout", true, false);
            model.QueueDeclare("system2.order.create.deadletter", true, false, false, new Dictionary<string, object> { { "x-queue-type", "quorum" } });
            model.QueueBind("system2.order.create.deadletter", "system2.order.create.deadletter", string.Empty);

            model.ExchangeDeclare("system2.order.create", "headers", true, false);
            model.QueueDeclare("system2.order.create", true, false, false, new Dictionary<string, object>
            {
                { "x-queue-type", "quorum" },
                { "x-dead-letter-exchange", "system2.order.create.deadletter" },
                { "x-delivery-limit", 3 }
            });
            model.QueueBind("system2.order.create", "system2.order.create", string.Empty);

            model.ExchangeBind("system2.order.create", "order.create", string.Empty, new Dictionary<string, object>
            {
                { "integration-fm-api", true },
                { "x-match", "all" }
            });
        }
    }
}

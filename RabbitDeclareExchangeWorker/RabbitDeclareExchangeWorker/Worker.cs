using RabbitMQ.Client;

namespace RabbitDeclareExchangeWorker
{
    public class Worker(IModel model) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DeclareOrderCreateExchange();

            DeclareTopicExchangeIntegration();

            DeclareHeadersExchangeIntegration();

            using PeriodicTimer Timer = new(TimeSpan.FromSeconds(1));

            while (!stoppingToken.IsCancellationRequested) await Timer.WaitForNextTickAsync(stoppingToken);
        }

        private void DeclareOrderCreateExchange()
        {
            model.ExchangeDelete("order.create");

            model.ExchangeDeclare("order.create", "fanout", true, false);
        }

        private void DeclareTopicExchangeIntegration()
        {
            model.ExchangeDelete("topic.order.create.unrouted");
            model.ExchangeDelete("topic.order.create.deadletter");
            model.ExchangeDelete("topic.order.create");

            model.QueueDelete("topic.order.create.unrouted");
            model.QueueDelete("topic.order.create.deadletter");
            model.QueueDelete("topic.order.create");

            //Unrouted
            model.ExchangeDeclare("topic.order.create.unrouted", "fanout", true, false);
            model.QueueDeclare("topic.order.create.unrouted", true, false, false, new Dictionary<string, object> { { "x-queue-type", "quorum" } });
            model.QueueBind("topic.order.create.unrouted", "topic.order.create.unrouted", string.Empty);

            //Deadletter
            model.ExchangeDeclare("topic.order.create.deadletter", "fanout", true, false);
            model.QueueDeclare("topic.order.create.deadletter", true, false, false, new Dictionary<string, object> { { "x-queue-type", "quorum" } });
            model.QueueBind("topic.order.create.deadletter", "topic.order.create.deadletter", string.Empty);

            //Exchange
            model.ConfirmSelect();
            model.BasicQos(0, 10, false);
            model.ExchangeDeclare("topic.order.create", "topic", true, false, new Dictionary<string, object>
            {
                { "alternate-exchange", "topic.order.create.unrouted" }
            });
            model.QueueDeclare("topic.order.create", true, false, false, new Dictionary<string, object>
            {
                { "x-queue-type", "quorum" },
                { "x-dead-letter-exchange", "topic.order.create.deadletter" },
                { "x-delivery-limit", 3 }
            });
            model.QueueBind("topic.order.create", "topic.order.create", string.Empty);

            model.ExchangeBind("topic.order.create", "order.create", string.Empty);
        }

        private void DeclareHeadersExchangeIntegration()
        {
            model.ExchangeDelete("headers.order.create.unrouted");
            model.ExchangeDelete("headers.order.create.deadletter");
            model.ExchangeDelete("headers.order.create");

            model.QueueDelete("headers.order.create.unrouted");
            model.QueueDelete("headers.order.create.deadletter");
            model.QueueDelete("headers.order.create");

            //Unrouted
            model.ExchangeDeclare("headers.order.create.unrouted", "fanout", true, false);
            model.QueueDeclare("headers.order.create.unrouted", true, false, false, new Dictionary<string, object> { { "x-queue-type", "quorum" } });
            model.QueueBind("headers.order.create.unrouted", "headers.order.create.unrouted", string.Empty);

            //Deadletter
            model.ExchangeDeclare("headers.order.create.deadletter", "fanout", true, false);
            model.QueueDeclare("headers.order.create.deadletter", true, false, false, new Dictionary<string, object> { { "x-queue-type", "quorum" } });
            model.QueueBind("headers.order.create.deadletter", "headers.order.create.deadletter", string.Empty);

            //Exchange
            model.ConfirmSelect();
            model.BasicQos(0, 10, false);
            model.ExchangeDeclare("headers.order.create", "headers", true, false, new Dictionary<string, object>
            {
                { "alternate-exchange", "headers.order.create.unrouted" }
            });
            model.QueueDeclare("headers.order.create", true, false, false, new Dictionary<string, object>
            {
                { "x-queue-type", "quorum" },
                { "x-dead-letter-exchange", "headers.order.create.deadletter" },
                { "x-delivery-limit", 3 }
            });
            model.QueueBind("headers.order.create", "headers.order.create", string.Empty);

            model.ExchangeBind("headers.order.create", "order.create", string.Empty, new Dictionary<string, object>
            {
                { "integration", true },
                { "x-match", "all" }
            });
        }
    }
}

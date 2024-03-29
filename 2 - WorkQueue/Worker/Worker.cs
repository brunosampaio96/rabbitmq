﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace Worker
{
    public class Worker
    {
        public static void Main()
        {
            ConnectionFactory factory = new() { HostName = "localhost" };

            using IConnection connection = factory.CreateConnection();
            using IModel channel = connection.CreateModel();

            _ = channel.QueueDeclare(queue: "task_queue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            Console.WriteLine(" [*] Waiting for messages.");

            EventingBasicConsumer consumer = new(channel);

            void p(object sender, BasicDeliverEventArgs ea)
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);

                int dots = message.Split('.').Length - 1;
                Thread.Sleep(dots * 1000);

                Console.WriteLine(" [x] Done");
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }

            consumer.Received += p;

            _ = channel.BasicConsume(queue: "task_queue",
                                 autoAck: false,
                                 consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
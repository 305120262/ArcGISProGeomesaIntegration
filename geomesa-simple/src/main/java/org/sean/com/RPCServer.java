package org.sean.com;


import com.alibaba.fastjson.JSON;
import com.rabbitmq.client.*;
import org.geotools.filter.text.cql2.CQLException;
import org.opengis.feature.simple.SimpleFeature;

import java.io.IOException;
import java.util.ArrayList;
import java.util.LinkedList;
import java.util.List;

public class RPCServer {
    private static final String RPC_QUEUE_NAME = "rpc_queue";
    private static GeomesaSimple demo;

    private static String doCommand(String command, ArrayList<String> parameters) throws IOException, CQLException {
        if(command.equals("open")) {
            if(demo==null) {
                demo = new GeomesaSimple();
                demo.connectDataStore("ubuntu.sean.com:2181");
            }
            return "opened";
        }
        else if(command.equals("getFields")) {
            List<String> fields = demo.getFields(parameters.get(0));
            return JSON.toJSONString(fields);
        }
        else if(command.equals("queryRows"))
        {
            List<String> rows = demo.queryRows(parameters.get(0));
            return JSON.toJSONString(rows);
        }
        else if(command.equals("squeryRows"))
        {
            List<String> rows = demo.squeryRows(parameters.get(0),Double.parseDouble(parameters.get(1)),Double.parseDouble(parameters.get(2)),Double.parseDouble(parameters.get(3)),Double.parseDouble(parameters.get(4)));
            return JSON.toJSONString(rows);
        }
        else if (command.equals("findRow"))
        {
            return demo.findRow(parameters.get(0),parameters.get(1));

        }
        else if (command.equals("close"))
        {
            demo.Close();
            return "closed";
        }
        else{
            return "invalid command";
        }
    }




    public static void main(String[] argv) throws Exception {
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost("localhost");

        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {
            channel.queueDeclare(RPC_QUEUE_NAME, false, false, false, null);
            channel.queuePurge(RPC_QUEUE_NAME);

            //channel.basicQos(1);

            System.out.println(" [x] Awaiting RPC requests");

            Object monitor = new Object();
            DeliverCallback deliverCallback = (consumerTag, delivery) -> {
                AMQP.BasicProperties replyProps = new AMQP.BasicProperties
                        .Builder()
                        .correlationId(delivery.getProperties().getCorrelationId())
                        .build();

                String response = "";

                try {
                    String message = new String(delivery.getBody(), "UTF-8");
                    System.out.println(message);
                    String[] parts = message.split("\\|");
                    ArrayList<String> parameters = new ArrayList<String>();
                    if(parts.length>1){
                        for(String para:parts[1].split(",")) {
                            parameters.add(para);
                        }
                    }
                    response +=doCommand(parts[0],parameters);
                } catch (RuntimeException | CQLException e) {
                    System.out.println(" [.] " + e.toString());
                } finally {
                    channel.basicPublish("", delivery.getProperties().getReplyTo(), replyProps, response.getBytes("UTF-8"));
                    channel.basicAck(delivery.getEnvelope().getDeliveryTag(), false);
                    // RabbitMq consumer worker thread notifies the RPC server owner thread
                    synchronized (monitor) {
                        monitor.notify();
                    }
                }
            };

            channel.basicConsume(RPC_QUEUE_NAME, false, deliverCallback, (consumerTag -> { }));
            // Wait and be prepared to consume the message from RPC client.
            while (true) {
                synchronized (monitor) {
                    try {
                        monitor.wait();
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }
                }
            }
        }
    }
}

# API Reference for MessageBroadcast

## Overview

The MessageBroadcast system allows a single publisher to send messages to multiple subscribers efficiently. This document provides a reference for the API endpoints and methods available in both the server and client libraries.

## Server API

### MessageHub

The `MessageHub` is the central hub for real-time communication between the server and clients. It uses SignalR to facilitate message broadcasting.

#### Methods

- **SendMessage**
  - **Description**: Sends a message from the publisher to all connected subscribers.
  - **Parameters**:
    - `Message message`: The message object containing the content, sender ID, and timestamp.
  - **Returns**: `Task`

### Connection Management

The server manages connections through the `ConnectionManager` service, which tracks active subscribers.

#### Methods

- **AddConnection**
  - **Description**: Adds a new subscriber connection.
  - **Parameters**:
    - `Client client`: The client object representing the subscriber.
  - **Returns**: `void`

- **RemoveConnection**
  - **Description**: Removes a subscriber connection.
  - **Parameters**:
    - `string connectionId`: The connection ID of the subscriber to be removed.
  - **Returns**: `void`

## Client API

### MessageBroadcastClient

The `MessageBroadcastClient` library provides methods for publishers and subscribers to interact with the MessageBroadcast server.

#### Publisher Methods

- **PublishMessage**
  - **Description**: Publishes a message to the server.
  - **Parameters**:
    - `BroadcastMessage message`: The message object to be sent.
  - **Returns**: `Task`

#### Subscriber Methods

- **SubscribeToMessages**
  - **Description**: Subscribes to receive messages from the server.
  - **Parameters**:
    - `Action<BroadcastMessage> onMessageReceived`: Callback to handle received messages.
  - **Returns**: `Task`

- **Unsubscribe**
  - **Description**: Unsubscribes from receiving messages.
  - **Returns**: `Task`

## Message Structure

### Message

The `Message` model represents the structure of a message in the system.

- **Properties**:
  - `string Content`: The content of the message.
  - `string SenderId`: The ID of the sender.
  - `DateTime Timestamp`: The time the message was sent.

### BroadcastMessage

The `BroadcastMessage` model is used by the client and mirrors the server's `Message` model.

## Protocol

The `MessageProtocol` defines the format for messages exchanged between the publisher and subscribers, ensuring efficient communication.

## Conclusion

This API reference provides a comprehensive overview of the methods and structures used in the MessageBroadcast system. For further details on implementation and usage, please refer to the respective sections in the documentation.
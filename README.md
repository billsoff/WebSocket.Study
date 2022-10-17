# WebSocket.Study

## Task
  - [x] Gracefully close web socket
  - [x] Initialize Job
  - [x] Is there event with close prompt window?　NG
  - [ ] Read address from configuration file
  - [ ] Receive complete message (multiple reading)
  - [ ] If client close the socket, what happen?
  - [x] `JobRepository` class need `sealed` keyword
  - [x] `JobRepositoty#WorkingJobs` should return `IList`
  - [x] `WebSocketServer` `Start` should append `Async`
  - [x] `WebSocketServer` new express remove `()` (L80)
  - [ ] `WebSocketServer` need locker?
  - [x] client.html add `;`

## Printing Command Schema

```js
  {
    command: string,
    data: string,
  }
```

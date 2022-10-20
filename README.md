# WebSocket.Study

## Task
  - [x] Gracefully close web socket
  - [x] Initialize Job
  - [x] Is there event with close prompt window?　NG
  - [ ] Read address from configuration file
  - [ ] Receive complete message (multiple reading)
  - [ ] If client close the socket, what happen?
  - [ ] When `WebSocket#ReceiveAsync()`, if closed, what happen?
  - [x] `JobRepository` class need `sealed` keyword
  - [x] `JobRepositoty#WorkingJobs` should return `IList`
  - [x] `WebSocketServer` `Start` should append `Async`
  - [x] `WebSocketServer` new express remove `()` (L80)
  - [ ] `WebSocketServer` need locker?
  - [x] client.html add `;`
  - [x] `WebSocketServer#AcceptJob()` loop exit using `return` (L96)
  - [ ] `Job#Terminate()` if not open, should do nothing
  - [ ] If port occupied, need check
  - [ ] `WebSocketCliennt` close event?

## Printing Command Schema

```js
  {
    command: string,
    data: string,
  }
```

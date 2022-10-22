function ClientWebSocket(url) {
    this.url = url;

    this.messageBuffer = [];

    this.isConnectedResolvings = [];
    this.recieveResolvings = [];

    this.utilCloseResolvings = [];

    this.socket = (function (client) {
        const socket = new WebSocket(client.url);

        socket.addEventListener('open', onOpen);
        socket.addEventListener('message', onMessage);
        socket.addEventListener('error', onError);
        socket.addEventListener('close', onClose)

        return socket;

        function onOpen() {
            client
                .isConnectedResolvings
                .splice(0, client.isConnectedResolvings.length)
                .forEach(([resole]) => resole(true));
        }

        function onMessage(e) {
            client.messageBuffer.push(e.data);

            const resolving = client.recieveResolvings.shift();

            if (resolving) {
                const resolve = resolving[0];

                return client.recievedArrival();
            }
        }

        function onError(e) {
            client.errorEvent = e;
        }

        function onClose(e) {
            client.closeEvent = e;

            client
                .isConnectedResolvings
                .splice(0, client.isConnectedResolvings.length)
                .forEach(([resole]) => resole(false));
            client
                .utilCloseResolvings
                .splice(0, client.utilCloseResolvings.length)
                .forEach(resolve => resolve());
            client
                .recieveResolvings
                .splice(0, client.recieveResolvings.length)
                .forEach(([_, reject]) => reject(new Error('Receive message failed. Connection cannot be established or was already closed.')));
        }
    })(this);
}

ClientWebSocket.prototype = {
    url: null,
    socket: null,

    errorEvent: null,
    closeEvent: null,

    messageBuffer: null,

    isConnectedResolvings: null,
    recieveResolvings: null,

    utilCloseResolvings: null,

    get readyState() {
        return this.socket.readyState;
    },

    get code() {
        return this.closeEvent && this.closeEvent.code;
    },

    get reason() {
        return this.closeEvent && this.closeEvent.reason;
    },

    get wasClean() {
        return this.closeEvent && this.closeEvent.wasClean;
    },

    get isAborted() {
        return this.errorEvent != null;
    },

    async isConnected() {
        switch (this.socket.readyState) {
            case WebSocket.OPEN:
                return true;

            case WebSocket.CLOSED:
                return false;

            default:
                return new Promise(resolve => this.isConnectedResolvings.push(resolve));
        }
    },

    async send(message) {
        switch (this.socket.readyState) {
            case WebSocket.OPEN:
                this.socket.send(message);

                return;

            case WebSocket.CLOSED:
                throw new Error('Send message failed. Connection cannot be established or was already closed.');

            default:
                await this.isConnected();
                await this.send(message);

                break;
        }
    },

    async recieve() {
        const message = this.messageBuffer.shift();

        if (message !== undefined) {
            return message;
        }

        return new Promise((resolve, reject) => this.recieveResolvings.push([resolve, reject]));
    },

    close() {
        this.socket.close();
    },

    recieveArrival() {
        return this.messageBuffer.shift() || null;
    },

    recieveAllArrivals() {
        return this.messageBuffer.splice(0, this.messageBuffer.length);
    },

    async untilClose() {
        if (this.socket.readyState == WebSocket.CLOSED) {
            return;
        }

        return new Promise(resolve => this.utilCloseResolvings.push(resolve));
    },
}

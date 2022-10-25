function ClientWebSocket(url) {
    this.messageBuffer = [];

    this.isConnectedResolvings = [];
    this.receiveResolvings = [];
    this.utilCloseResolvings = [];

    this.url = url;
    this.socket = new WebSocket(url);

    this.socket.addEventListener('open', () => {
        this.isConnectedResolvings
            .splice(0, this.isConnectedResolvings.length)
            .forEach(resolve => resolve(true));
    });

    this.socket.addEventListener('message', e => {
        this.messageBuffer.push(e.data);

        const [resolve] = this.receiveResolvings.shift() || {};

        if (resolve) {
            resolve(this.messageBuffer.shift());
        }
    });

    this.socket.addEventListener('error', e => {
        this.errorEvent = e;
    });

    this.socket.addEventListener('close', e => {
        this.closeEvent = e;

        this.isConnectedResolvings
            .splice(0, this.isConnectedResolvings.length)
            .forEach(resolve => resolve(false));
        this.utilCloseResolvings
            .splice(0, this.utilCloseResolvings.length)
            .forEach(resolve => resolve());
        this.receiveResolvings
            .splice(0, this.receiveResolvings.length)
            .forEach(([_, reject]) => reject(new Error('Receive message failed. Connection cannot be established or was already closed.')));
    });
}

ClientWebSocket.prototype = {
    url: null,
    socket: null,

    errorEvent: null,
    closeEvent: null,

    messageBuffer: null,

    isConnectedResolvings: null,
    receiveResolvings: null,
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

    async receive() {
        const message = this.messageBuffer.shift();

        if (message !== undefined) {
            return message;
        }

        return new Promise((resolve, reject) => this.receiveResolvings.push([resolve, reject]));
    },

    close() {
        this.socket.close();
    },

    receiveArrival() {
        return this.messageBuffer.shift() || null;
    },

    receiveAllArrivals() {
        return this.messageBuffer.splice(0, this.messageBuffer.length);
    },

    async untilClose() {
        if (this.socket.readyState == WebSocket.CLOSED) {
            return;
        }

        return new Promise(resolve => this.utilCloseResolvings.push(resolve));
    },
}

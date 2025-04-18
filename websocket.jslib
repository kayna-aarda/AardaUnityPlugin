mergeInto(LibraryManager.library, {
    RegisterSocketCallbacks: function(onOpen, onAudioMessage, onTextMessage, onError, onClose) {
        Module.webSocketOpenCallback = onOpen;
        Module.webSocketAudioMessageCallback = onAudioMessage;
        Module.webSocketTextMessageCallback = onTextMessage;
        Module.webSocketErrorCallback = onError;
        Module.webSocketCloseCallback = onClose;
    },

    WebSocketConnect: function (urlPtr) {
        if (!document.sockets) {
            document.sockets = {};
        }

        var url = UTF8ToString(urlPtr);
        var socket = new WebSocket(url);
        socket.binaryType = "arraybuffer";

        var socketId = Object.keys(document.sockets).length + 1;
        document.sockets[socketId] = socket;

        socket.onopen = function () {
            console.log("JS: onopen -> socketId=" + socketId);
            if (Module.webSocketOpenCallback) {
                // 'vi' means void callback(int)
                Module['dynCall_vi'](Module.webSocketOpenCallback, socketId);
            }
        };

        const decoder = new TextDecoder('utf-8');
        socket.onmessage = function (event) {
            // Let's add debug logs so we can see what's happening
            console.log("JS: onmessage fired for socketId=" + socketId);

            // Make sure both callbacks exist before proceeding
            if (Module.webSocketAudioMessageCallback && Module.webSocketTextMessageCallback) {
                
                // CASE 1: Binary data (ArrayBuffer)
                if (event.data instanceof ArrayBuffer) {
                    // Convert the ArrayBuffer to a Uint8Array
                    const audioData = new Uint8Array(event.data);
                    console.log("JS: onmessage => received ArrayBuffer, size=", audioData.length);

                    // Allocate space in WASM memory for audioData
                    // _malloc() and HEAPU8 come from Emscripten's runtime
                    const audioPtr = _malloc(audioData.length);
                    HEAPU8.set(audioData, audioPtr);

                    // Call the audio callback in C#
                    // Signature: v(ii) => void callback(int socketId, int pointer)
                    Module['dynCall_viii'](
                        Module.webSocketAudioMessageCallback,
                        socketId,
                        audioPtr,
                        audioData.length
                    );

                    // Always free the pointer after usage
                    _free(audioPtr);
                }
                
                // CASE 2: Text data (string)
                else if (typeof event.data === 'string') {
                    console.log("JS: onmessage => received text of length =", event.data.length);

                    // event.data is already a string, so just pass it along
                    const msgPtr = allocate(intArrayFromString(event.data), ALLOC_NORMAL);

                    // Call the text callback in C#
                    // Signature: v(ii) => void callback(int socketId, int pointer)
                    Module['dynCall_vii'](Module.webSocketTextMessageCallback, socketId, msgPtr);

                    // Free the allocated string
                    _free(msgPtr);
                }

                // Optional: You could handle Blobs or other data types here
                else {
                    console.warn("JS: onmessage => Received data in an unexpected format.", event.data);
                }
            } else {
                console.warn("JS: webSocketAudioMessageCallback or webSocketTextMessageCallback is not set.");
            }
        };


        socket.onerror = function (err) {
            console.log("JS: onerror -> socketId=" + socketId, err);
            if (Module.webSocketErrorCallback) {
                var errStr = err.message ? err.message : "Unknown WebSocket error";
                var errPtr = allocate(intArrayFromString(errStr), ALLOC_NORMAL);
                Module['dynCall_vii'](Module.webSocketErrorCallback, socketId, errPtr);
                _free(errPtr);
            }
        };

        socket.onclose = function () {
            console.log("JS: onclose -> socketId=" + socketId);
            if (Module.webSocketCloseCallback) {
                Module['dynCall_vi'](Module.webSocketCloseCallback, socketId);
            }
            delete document.sockets[socketId];
        };

        return socketId;
    },

    WebSocketClose: function (socketId) {
        if (document.sockets && document.sockets[socketId]) {
            document.sockets[socketId].close();
            delete document.sockets[socketId];
        }
    },

    WebSocketSend: function (socketId, msgPtr) {
        if (document.sockets && document.sockets[socketId]) {
            var socket = document.sockets[socketId];
            var message = UTF8ToString(msgPtr);
            socket.send(message);
        }
    }
});

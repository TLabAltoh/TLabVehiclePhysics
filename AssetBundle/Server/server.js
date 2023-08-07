const express = require("express");
const http = require("http");
const path = require('path');

const port = 5000;
const bar = "----------------";

console.log("\ncreate server " + bar);

var app = express();
const server = http.createServer(app);

app.use((req, res, next) => {
    res.header('Access-Control-Allow-Origin', '*');
    res.header('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    res.header('Access-Control-Allow-Headers', 'Content-Type, Authorization, access_token');

    if ('OPTIONS' === req.method) {
        res.send(200);
    } else {
        next();
    }
});
app.use(express.static(path.join(__dirname)));

server.listen(port);

console.log("\nstart server on port: " + port + " " + bar);

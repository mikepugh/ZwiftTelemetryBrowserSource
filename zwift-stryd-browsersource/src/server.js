const express = require('express');
const bodyParser = require('body-parser')
const path = require('path');
const app = express();
const Ant = require('ant-plus');

let lastPowerReading = 0.0;
let lastHrReading = 0.0;

app.use(express.static(path.join(__dirname, 'build')));

app.get('/ping', function (req, res) {
    return res.json({ response: 'Pong', power: lastPowerReading, hr: lastHrReading});
});

app.listen(process.env.PORT || 8080);


const stick = new Ant.GarminStick3();

const hrScanner = new Ant.HeartRateScanner(stick);
const speedScanner = new Ant.StrideSpeedDistanceScanner(stick);
const powerScanner = new Ant.BicyclePowerScanner(stick);

stick.on('startup', function () {
    console.log('ant+ startup');
    powerScanner.scan();
    hrScanner.scan();
    // speedScanner.scan();
});

// speedScanner.on('ssddata', function(data) {
//     console.log('***Received Speed***', data);
// })

powerScanner.on('powerData', function(data) {
    lastPowerReading = data.Power;
})


hrScanner.on('hbData', function(data) {
    lastHrReading = data.ComputedHeartRate;
})
if(!stick.open()) {
    console.log('Stick not found!');
}
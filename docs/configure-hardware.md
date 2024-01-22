## Hue HW Notes

### Hue Hardware Requirements

| Item  |
| ------------ |
| [Philips Hue Bridge](https://www2.meethue.com/en-us/p/hue-bridge/046677458478)
| [Philips Hue Light Bulb](https://www2.meethue.com/en-us/p/hue-white-and-color-ambiance-1-pack-e26/046677548483) |

You will need the above Philips Hue items to broadcast your presence to, but you can still "use" PresenceLight without them. One of the requirements of the Bridge is that it needs to be hard-wired to an internet connection via ethernet, so it will need to be placed close to a router or network switch. There are steps to setup the bridge and bulb in the [Hardware and Connectivity Section](https://www2.meethue.com/en-us/support/hardware-and-connectivity) of the Philips Support Site, but you should be able to just plug the bridge, wait for the lights to light up, get the IP address for the bridge, enter it into the app, and register the device. The app will register your device, create an account to interact with the bulbs, and finally add any bulbs it finds.

Philips also provides a Remote implementation of their connectivity (requires connecting your account to Philips Cloud). PresenceLight is configured to let you choose between the two.

## LIFX HW Notes

### LIFX Hardware Requirements
 [Any LIFX Light (tested with LIFX Beam & LIFX Color)](https://www.lifx.com/pages/all-products)

LIFX Bulbs can be connected to over [LAN Protocol](https://lan.developer.lifx.com/), or [Cloud Api](https://lifx.readme.io/docs). PresenceLight uses the Cloud, which requires getting a token from the [developer portal](https://cloud.lifx.com/settings). Putting that token in PresenceLight will enable all connected lights.

## Philips Wiz

| Item  |
| ------------ |
| [Wiz Smart Bulb](https://www.wizconnected.com/en-us/products/bulbs)
|
PresenceLight uses LAN discovery to get all Philips Wiz smart bulbs on the network.

## Yeelight
| Item  |
| ------------ |
| [Yeelight Smart Bulb](https://store.yeelight.com/collections/smart-bulb)
|
PresenceLight uses LAN discovery to get all Yeelight smart bulbs on the network.
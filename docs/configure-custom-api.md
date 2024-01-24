# Custom API

The Custom API page lets you use any generic service which has a web API which accepts GET or POST requests.

For example, IFTTT webhooks can be used to run an action on any IFTTT-integrated service.

In this way IFTTT can act as a bridge to other light services (such as Magic Home / MagicHue) or any other service which you may want to control with your Teams presence, e.g. 'When I'm in do not disturb pause Roomba'.

To connect PresenceLight to a custom API:

Configure the web service (e.g. created the applets in IFTTT)
Enter the corresponding API method and URI against each presence state.

   ![Configured](../static/CustomAPI.png)   
  
The Custom API REST API calls also support providing a json formatted body to the endpoints (Uri) of Custom API.

You can use the following variables in your JSON body:

- {{availability}}
- {{activity}}

If you use above variables in the JSON body they will be replaced with the availability and/or activity values of your Microsoft Teams status.

## Home Assistant integration

To use PresenceLight with Home Assistant you can use the Custom API functionality as follows:

In Home Assistant you can use [Webhooks triggers](https://www.home-assistant.io/docs/automation/trigger/#webhook-trigger) to trigger an Automation Action, like turning on a light bulb.

Example Automation for turning on a light bulb based on the Teams status send using the Custom API functionality of PresenceLight.

```yaml
alias: Teams presence - IKEA Light Bulb Living Room
description: >-
  Show the Microsoft Teams status via a color of the Light Bulb in the Living
  Room
trigger:
  - platform: webhook
    allowed_methods:
      - POST
    local_only: true
    webhook_id: "<enter secret webhook id here>"
condition: []
action:
  - choose:
      - conditions:
          - condition: template
            value_template: "{{ trigger.json.presence_status == 'Busy' }}"
        sequence:
          - service: light.turn_on
            metadata: {}
            data:
              color_name: red
            target:
              entity_id: light.ikea_bulb
      - conditions:
          - condition: template
            value_template: "{{ trigger.json.presence_status == 'Available' }}"
        sequence:
          - service: light.turn_on
            metadata: {}
            data:
              color_name: green
            target:
              entity_id: light.ikea_bulb
      - conditions:
          - condition: template
            value_template: "{{ trigger.json.presence_status == 'Away' }}"
        sequence:
          - service: light.turn_on
            metadata: {}
            data:
              color_name: yellow
            target:
              entity_id: light.ikea_bulb
      - conditions: null
        sequence:
          - service: light.turn_off
            metadata: {}
            target:
              entity_id: light.ikea_bulb
            data: {}
mode: single
```

In PresenceLight Custom API setting you need to enter the following information:

| Method | Uri            | Body |
|--------|----------------|------|
| POST   | http://homeassistant.local:8123/api/webhook/webhook_id |    {   "presence_status":"Away" }  |

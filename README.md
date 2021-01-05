# DurableCards

Exploration of using Adaptive Cards (https://adaptivecards.io/) with Durable Entities (https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-entities?tabs=csharp) for low-code stateful micro-UI. Possible use cases could be simple business scenarios such as Approvals, Questionnaires, Polls, Feedback Forms and Onboarding workflows. Deployment using Azure App Service "Easy Auth" could enable Azure AD authentication for internal applications. By using Adaptive Cards the same interactions can be reused in Bots (Microsoft Teams) and Actionable Messages (Office 365 email). Additionally rendering Adaptive Cards outside of Bots, as a more traditional web app, could open up other integrations such as human interactions within Azure Logic Apps workflows. 

## API

Post an Adaptive Card Template to CreateCard. Optionally include an action using Action.Submit and a JSON Schema to validate the postback. The text response is a guid for use with RenderCard, an HTML rendering of the card template using Bootstrap CSS. On postback, all input is saved to the `attachments` array for data binding in the template, allowing simple data driven interactions, with all state stored in a Durable Entity.

*CreateCard: [POST] http://{host}/card*

Sample request

```
{
    "definition": {
        "template": {
            "type": "AdaptiveCard",
            "version": "1.3",
            "body": [
                {
                    "type": "TextBlock",
                    "$data": "${attachments}",
                    "text": "You sent: ${text}"
                },
                {
                    "type": "Input.Text",
                    "id": "text",
                    "label": "Text"
                }
            ],
            "actions": [
                {
                    "type": "Action.Submit",
                    "title": "Send"
                }
            ]
        },
        "schema": {
            "type": "object",
            "required": [
                "text"
            ],
            "properties": {
                "text": {
                    "type": "string",
                    "minLength": 1,
                    "pattern": "^(?!\\s*$).+"
                }
            }
        }
    }
}
```

*RenderCard: [GET] http://{host}/card/{id:guid}*

*PostCard: [POST] http://{host}/card/{id:guid}*
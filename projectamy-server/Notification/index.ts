import { AzureFunction, Context, HttpRequest, HttpRequestHeaders } from "@azure/functions"
import { ChangeNotificationsService } from "../services/ChangeNotificationsService";
import { validateNewSubscription } from "../services/SubscriptionValidationService";
const httpTrigger: AzureFunction = async function (context: Context, req: HttpRequest): Promise<void> {
   

    const validationToken = req.query.validationToken;

    if (validationToken) {
        context.res = {
            status: 200,
            body: validateNewSubscription(validationToken),
            headers: {
                'Content-Type': 'text/plain'
            }
        };
    } else {
        context.log.info(JSON.stringify(req.body));

        const changeNotificationsService = new ChangeNotificationsService();
        try {
            const reactions = await changeNotificationsService.handleNotificationReceivedAsync(req.body);
            context.log.info(JSON.stringify(reactions));
            context.bindings.reactionsOutQueue = reactions;
        } catch (error) {
            context.log.error(error.constructor.name);
            context.log.error(error.message);
            context.log.error(error.name);
            context.log.error(JSON.stringify(error.stack));

            context.res = {
                status: 500,
                body: error.message
            };
            return;
        }

        context.res = {
            status: 200,
        };

    }

};

export default httpTrigger;
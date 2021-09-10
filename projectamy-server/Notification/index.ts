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
        const changeNotificationsService = new ChangeNotificationsService();
        try {
            await changeNotificationsService.handleNotificationReceivedAsync(req.body);
        } catch (error) {
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
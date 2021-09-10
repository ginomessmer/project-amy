export const validateNewSubscription = (validationToken: string): string => {
    const decodedToken = decodeURIComponent(validationToken);
    return decodedToken;

}
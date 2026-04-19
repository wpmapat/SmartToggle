import axios from "axios";
import type { IPublicClientApplication } from "@azure/msal-browser";
import { loginRequest, apiBaseUrl } from "./authConfig";

export const createApiClient = async (msalInstance: IPublicClientApplication) => {
    const accounts = msalInstance.getAllAccounts();
    const account = accounts[0];

    const tokenResponse = await msalInstance.acquireTokenSilent({
        ...loginRequest,
        account,
    });

    return axios.create({
        baseURL: apiBaseUrl,
        headers: {
            Authorization: `Bearer ${tokenResponse.accessToken}`,
        },
    });
};

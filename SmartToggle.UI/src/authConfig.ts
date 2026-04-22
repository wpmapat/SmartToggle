import type { Configuration, PopupRequest } from "@azure/msal-browser";

export const msalConfig: Configuration = {
    auth: {
        clientId: "19e962db-511a-4e6b-b8af-a5752b3d54e5",
        authority: "https://login.microsoftonline.com/organizations",
        redirectUri: window.location.origin,
    },
    cache: {
        cacheLocation: "sessionStorage",
    },
};

export const loginRequest: PopupRequest = {
    scopes: ["api://19e962db-511a-4e6b-b8af-a5752b3d54e5/access_as_user"],
};

export const graphRequest = {
    scopes: ["https://graph.microsoft.com/Organization.Read.All"],
};

export const apiBaseUrl = "https://smarttoggle-api.azurewebsites.net";

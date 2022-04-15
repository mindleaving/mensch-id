import * as Enums from './enums.d';

export namespace Models {
    interface Account extends Models.IId {
        accountType: Enums.AccountType;
        personId: string;
        preferedLanguage: Enums.Language;
    }

    interface LocalAccount extends Models.Account {
        username: string;
        salt: string;
        passwordHash: string;
    }

    interface ExternalAccount extends Models.Account {
        loginProvider: Enums.LoginProvider;
        externalId: string;
    }

    interface AuthenticationResult {
        isAuthenticated: boolean;
        accessToken?: string;
        error: Enums.AuthenticationErrorType;
    }

    interface IExternalLogin {
        loginProvider: Enums.LoginProvider;
        externalId: string;
    }

    interface IId {
        id: string;
    }

    interface LoginInformation {
        username: string;
        password: string;
    }

    interface MenschId extends Models.IId {
        type: Enums.IdType;
        isClaimed: boolean;
    }

    interface Person extends Models.IId {
        anonymousId: string;
    }
}

import * as Enums from './enums.d';

export namespace Models {
    interface Account extends Models.IId {
        accountType: Enums.AccountType;
        personId: string;
        preferedLanguage: Enums.Language;
    }

    interface LocalAnonymousAccount extends Models.Account {
        salt: string;
        passwordHash: string;
        passwordResetToken: string;
    }

    interface LocalAccount extends Models.LocalAnonymousAccount {
        email: string;
        emailVerificationToken: string;
        isEmailVerified: boolean;
    }

    interface ExternalAccount extends Models.Account {
        loginProvider: Enums.LoginProvider;
        externalId: string;
    }

    interface AssignerAccount extends Models.LocalAccount {
        name: string;
    }

    interface AssignerControlledProfile extends Models.IId {
        assignerAccountId: string;
        creationDate: Date;
        ownershipSecret: string;
    }

    interface AuthenticationResult {
        isAuthenticated: boolean;
        accessToken?: string;
        accountType?: Enums.AccountType | null;
        error: Enums.AuthenticationErrorType;
    }

    interface IExternalLogin {
        loginProvider: Enums.LoginProvider;
        externalId: string;
    }

    interface IId {
        id: string;
    }

    interface IsLoggedInResponse {
        accountType: Enums.AccountType;
    }

    interface LoginInformation {
        emailOrMenschId: string;
        password: string;
    }

    interface MenschId extends Models.IId {
        type: Enums.IdType;
        isClaimed: boolean;
    }

    interface MenschIdChallenge extends Models.IId {
        menschId: string;
        challengeShortId: string;
        challengeSecret: string;
        createdTimestamp: Date;
    }

    interface Person extends Models.IId {
        creationDate: Date;
    }

    interface RegistrationInformation {
        accountType: Enums.AccountType;
        email?: string;
        password: string;
        preferedLanguage?: Enums.Language | null;
    }

    interface ResendVerificationBody {
        email: string;
    }

    interface ResetPasswordBody {
        accountId: string;
        resetToken: string;
        password: string;
    }

    interface ResetPasswordRequest {
        email: string;
    }

    interface TakeControlBody {
        id: string;
        ownershipSecret: string;
    }

    interface Verification extends Models.IId {
        personId: string;
        verifierId: string;
        timestamp: Date;
    }
}

import * as Enums from './enums';

export namespace Models {
    interface Address {
        street: string;
        postalCode: string;
        city: string;
        country: string;
    }
    interface AssignerAccountRequest extends Models.IId<string> {
        contactPersonName: string;
        email: string;
        expectedAssignmentsPerYear: number;
        note: string;
    }
    interface AssignerControlledProfile extends Models.IId<string> {
        assignerAccountId: string;
        creationDate: string;
        ownershipSecret: string;
    }
    interface ChangePasswordRequest {
        accountId: string;
        currentPassword: string;
        newPassword: string;
    }
    interface Contact {
        name: string;
        address: Models.Address;
        phoneNumber: string;
        email: string;
    }
    interface IId<TKey> {
        id: TKey;
    }
    interface IsLoggedInResponse {
        accountType: Enums.AccountType;
    }
    interface LoginInformation {
        emailMenschIdOrUsername: string;
        password: string;
    }
    interface MenschIdChallenge extends Models.IId<string> {
        menschId: string;
        challengeShortId: string;
        challengeSecret: string;
        createdTimestamp: string;
    }
    interface MenschIdReservation extends Models.IId<string> {
        type: Enums.IdType;
        isClaimed: boolean;
    }
    interface Person extends Models.IId<string> {
        creationDate: string;
        creationAccountId: string;
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
        id?: string;
        ownershipSecret?: string;
    }
    interface Verification extends Models.IId<string> {
        personId: string;
        verifierId: string;
        timestamp: string;
    }
    export namespace Shop {
        interface Money {
            currency: Enums.Currency;
            value: number;
        }
        interface Order extends Models.IId<string> {
            orderedByAccountId: string;
            paymentMethod: Enums.PaymentMethod;
            isPaymentReceived: boolean;
            invoiceAddress: Models.Contact;
            sendInvoiceSeparately: boolean;
            shippingMethod: Enums.ShippingMethod;
            shippingAddress: Models.Contact;
            items: Models.Shop.OrderItem[];
            statusChanges: Models.Shop.OrderStatusChange[];
            status: Enums.OrderStatus;
        }
        interface OrderStatusChange {
            newStatus: Enums.OrderStatus;
            timestamp: string;
        }
        interface OrderItem {
            product: Models.Shop.Product;
            amount: number;
        }
        interface Product extends Models.IId<string> {
            name: string;
            price: Models.Shop.Money;
            imageUrl?: string;
            category: string;
            downloadLink?: string;
        }
    }
    export namespace RequestParameters {
        interface AssignedProfilesRequestParameters extends Models.RequestParameters.GenericItemsRequestParameters {
            creationTimeRangeStart?: string | null;
            creationTimeRangeEnd?: string | null;
            birthDateTimeRangeStart?: string | null;
            birthDateTimeRangeEnd?: string | null;
        }
        interface GenericItemsRequestParameters {
            count?: number | null;
            skip: number;
            searchText?: string;
            orderBy?: string;
            orderDirection: Enums.OrderDirection;
        }
        interface OrderRequestParameters extends Models.RequestParameters.GenericItemsRequestParameters {
            status: Enums.OrderStatus[];
        }
        interface ProductRequestParameters extends Models.RequestParameters.GenericItemsRequestParameters {
            category: string;
        }
    }
    export namespace ExternalServices {
        interface ExternalService extends Models.IId<string> {
            clientId: string;
            displayName: string;
            contactInformation: string;
            acceptRedirectUri: string;
            rejectRedirectUri: string;
            status: Enums.ExternalServiceStatus;
            createdTimestamp: string;
            activatedTimestamp?: string | null;
            activatedBy: string;
        }
        interface ExternalServiceCredentials {
            client_id: string;
            client_secret: string;
        }
        interface ExternalServiceRegistration {
            displayName: string;
            contactInformation: string;
            acceptRedirectUri: string;
            rejectRedirectUri: string;
        }
        interface ServiceChallenge extends Models.IId<string> {
            serviceId: string;
            menschId: string;
            description: string;
            serviceState: string;
            state: Enums.ServiceChallengeState;
            createdTimestamp: string;
            expiresAt: string;
            answeredTimestamp?: string | null;
            resultToken: string;
        }
        interface ServiceChallengeAnswerResponse {
            redirectUri: string;
        }
        interface ServiceChallengeCreationRequest extends Models.ExternalServices.ExternalServiceCredentials {
            mensch_id: string;
            description: string;
            state: string;
        }
        interface ServiceChallengeCreationResponse {
            challenge_id: string;
            expires_at: string;
            app_link: string;
        }
        interface ServiceChallengeResult {
            challenge_id: string;
            status: Enums.ServiceChallengeResultStatus;
            result_token: string;
        }
        interface ServiceChallengeResultTokenClaims {
            
        }
    }
    export namespace AccessControl {
        interface Account extends Models.IId<string> {
            accountType: Enums.AccountType;
            preferedLanguage: Enums.Language;
        }
        interface PersonAccount extends Models.AccessControl.Account {
            personId: string;
        }
        interface AdminAccount extends Models.AccessControl.ProfessionalAccount {
            
        }
        interface AssignerAccount extends Models.AccessControl.ProfessionalAccount {
            name: string;
            logoId: string;
            logoImageType: string;
            contact: Models.Contact;
        }
        interface ExternalAccount extends Models.AccessControl.PersonAccount {
            loginProvider: string;
            externalId: string;
        }
        interface IAccountWithPassword {
            passwordHash: string;
            failedLoginAttempts: number;
            lockoutEnd?: string | null;
        }
        interface LocalAccount extends Models.AccessControl.LocalAnonymousAccount {
            email: string;
            isEmailVerified: boolean;
        }
        interface LocalAnonymousAccount extends Models.AccessControl.PersonAccount, Models.AccessControl.IAccountWithPassword {
            
        }
        interface ProfessionalAccount extends Models.AccessControl.Account, Models.AccessControl.IAccountWithPassword {
            username: string;
        }
    }
}

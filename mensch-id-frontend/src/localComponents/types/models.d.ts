import * as Enums from './enums.d';

export namespace Models {
    interface Address {
        street: string;
        postalCode: string;
        city: string;
        country: string;
    }

    interface AssignerAccountRequest extends Models.IId {
        contactPersonName: string;
        email: string;
        expectedAssignmentsPerYear: number;
        note: string;
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
        error?: Enums.AuthenticationErrorType | null;
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
        id: string;
        ownershipSecret: string;
    }

    interface Verification extends Models.IId {
        personId: string;
        verifierId: string;
        timestamp: Date;
    }

    export namespace Shop {
        interface Money {
            currency: Enums.Currency;
            value: number;
        }
    
        interface Order extends Models.IId {
            orderedByAccountId: string;
            paymentMethod: Enums.PaymentMethod;
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
            timestamp: Date;
        }
    
        interface OrderItem {
            product: Models.Shop.Product;
            amount: number;
        }
    
        interface Product extends Models.IId {
            name: string;
            price: Models.Shop.Money;
            imageUrl?: string;
            category: string;
            downloadLink?: string;
        }
    }

    export namespace RequestParameters {
        interface AssignedProfilesRequestParameters extends Models.RequestParameters.GenericItemsRequestParameters {
            timeRangeStart?: Date | null;
            timeRangeEnd?: Date | null;
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

    export namespace AccessControl {
        interface Account extends Models.IId {
            accountType: Enums.AccountType;
            personId: string;
            preferedLanguage: Enums.Language;
        }
    
        interface AdminAccount extends Models.AccessControl.LocalAccount {
            
        }
    
        interface AssignerAccount extends Models.AccessControl.LocalAccount {
            name: string;
            logoUrl: string;
            contact: Models.Contact;
        }
    
        interface ExternalAccount extends Models.AccessControl.Account {
            loginProvider: Enums.LoginProvider;
            externalId: string;
        }
    
        interface IExternalLogin {
            loginProvider: Enums.LoginProvider;
            externalId: string;
        }
    
        interface LocalAccount extends Models.AccessControl.LocalAnonymousAccount {
            email: string;
            emailVerificationAndPasswordResetSalt: string;
            emailVerificationToken: string;
            isEmailVerified: boolean;
        }
    
        interface LocalAnonymousAccount extends Models.AccessControl.Account {
            passwordHash: string;
            passwordResetToken: string;
        }
    }
}

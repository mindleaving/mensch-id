import * as Enums from './enums';

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
        emailMenschIdOrUsername: string;
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
            creationTimeRangeStart?: Date | null;
            creationTimeRangeEnd?: Date | null;
            birthDateTimeRangeStart?: Date | null;
            birthDateTimeRangeEnd?: Date | null;
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
            loginProvider: Enums.LoginProvider;
            externalId: string;
        }
    
        interface IAccountWithPassword {
            passwordHash: string;
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
    
        interface LocalAnonymousAccount extends Models.AccessControl.PersonAccount, Models.AccessControl.IAccountWithPassword {
            passwordResetToken: string;
        }
    
        interface ProfessionalAccount extends Models.AccessControl.Account, Models.AccessControl.IAccountWithPassword {
            username: string;
        }
    }
}

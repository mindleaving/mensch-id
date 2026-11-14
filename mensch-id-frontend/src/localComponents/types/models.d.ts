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
            email: string;
            isEmailVerified: boolean;
            contactInformation: string;
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
            email: string;
            contactInformation: string;
        }
        interface ExternalServiceRegistrationResponse extends Models.ExternalServices.ExternalServiceCredentials {
            id: string;
        }
        interface ExternalServiceSecretRotationResponse {
            client_secret: string;
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
            isActivated: boolean;
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
        interface WebSession extends Models.IId<string> {
            accountId: string;
            serializedTicket: System.Byte[];
            userAgent: string;
            createdTimestamp: string;
            lastActivityTimestamp: string;
            expiresAt: string;
        }
    }
}
export namespace System {
    interface Byte extends System.ValueType, System.IConvertible, System.Numerics.IUnsignedNumber<System.Byte>, System.IUtfChar<System.Byte>, System.IBinaryIntegerParseAndFormatInfo<System.Byte> {
        
    }
    interface ValueType {
        
    }
    interface IConvertible {
        
    }
    interface IUtfChar<TSelf> extends System.Numerics.IBinaryInteger<TSelf> {
        
    }
    interface IBinaryIntegerParseAndFormatInfo<TSelf> extends System.Numerics.IBinaryInteger<TSelf>, System.Numerics.IMinMaxValue<TSelf> {
        isSigned: boolean;
        maxDigitCount: number;
        maxHexDigitCount: number;
        maxValueDiv10: TSelf;
        overflowMessage: string;
    }
    interface IEquatable<T> {
        
    }
    interface ISpanFormattable extends System.IFormattable {
        
    }
    interface ISpanParsable<TSelf> extends System.IParsable<TSelf> {
        
    }
    interface IUtf8SpanFormattable {
        
    }
    interface IUtf8SpanParsable<TSelf> {
        
    }
    interface IFormattable {
        
    }
    interface IParsable<TSelf> {
        
    }
    interface IComparable {
        
    }
    interface IComparable<T> {
        
    }
    export namespace Numerics {
        interface IUnsignedNumber<TSelf> extends System.Numerics.INumberBase<TSelf> {
            
        }
        interface INumberBase<TSelf> extends System.Numerics.IAdditionOperators<TSelf,TSelf,TSelf>, System.Numerics.IAdditiveIdentity<TSelf,TSelf>, System.Numerics.IDecrementOperators<TSelf>, System.Numerics.IDivisionOperators<TSelf,TSelf,TSelf>, System.IEquatable<TSelf>, System.Numerics.IEqualityOperators<TSelf,TSelf,boolean>, System.Numerics.IIncrementOperators<TSelf>, System.Numerics.IMultiplicativeIdentity<TSelf,TSelf>, System.Numerics.IMultiplyOperators<TSelf,TSelf,TSelf>, System.ISpanFormattable, System.ISpanParsable<TSelf>, System.Numerics.ISubtractionOperators<TSelf,TSelf,TSelf>, System.Numerics.IUnaryPlusOperators<TSelf,TSelf>, System.Numerics.IUnaryNegationOperators<TSelf,TSelf>, System.IUtf8SpanFormattable, System.IUtf8SpanParsable<TSelf> {
            one: TSelf;
            radix: number;
            zero: TSelf;
        }
        interface IBinaryInteger<TSelf> extends System.Numerics.IBinaryNumber<TSelf>, System.Numerics.IShiftOperators<TSelf,number,TSelf> {
            
        }
        interface IMinMaxValue<TSelf> {
            minValue: TSelf;
            maxValue: TSelf;
        }
        interface IAdditionOperators<TSelf,TOther,TResult> {
            
        }
        interface IAdditiveIdentity<TSelf,TResult> {
            additiveIdentity: TResult;
        }
        interface IDecrementOperators<TSelf> {
            
        }
        interface IDivisionOperators<TSelf,TOther,TResult> {
            
        }
        interface IEqualityOperators<TSelf,TOther,TResult> {
            
        }
        interface IIncrementOperators<TSelf> {
            
        }
        interface IMultiplicativeIdentity<TSelf,TResult> {
            multiplicativeIdentity: TResult;
        }
        interface IMultiplyOperators<TSelf,TOther,TResult> {
            
        }
        interface ISubtractionOperators<TSelf,TOther,TResult> {
            
        }
        interface IUnaryPlusOperators<TSelf,TResult> {
            
        }
        interface IUnaryNegationOperators<TSelf,TResult> {
            
        }
        interface IBinaryNumber<TSelf> extends System.Numerics.IBitwiseOperators<TSelf,TSelf,TSelf>, System.Numerics.INumber<TSelf> {
            allBitsSet: TSelf;
        }
        interface IShiftOperators<TSelf,TOther,TResult> {
            
        }
        interface IBitwiseOperators<TSelf,TOther,TResult> {
            
        }
        interface INumber<TSelf> extends System.IComparable, System.IComparable<TSelf>, System.Numerics.IComparisonOperators<TSelf,TSelf,boolean>, System.Numerics.IModulusOperators<TSelf,TSelf,TSelf>, System.Numerics.INumberBase<TSelf> {
            
        }
        interface IComparisonOperators<TSelf,TOther,TResult> extends System.Numerics.IEqualityOperators<TSelf,TOther,TResult> {
            
        }
        interface IModulusOperators<TSelf,TOther,TResult> {
            
        }
    }
}
